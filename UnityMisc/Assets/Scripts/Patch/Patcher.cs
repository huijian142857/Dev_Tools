using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using UnityEngine.Networking;

using System.Threading;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Specialized;
using System.Reflection;
using System.Security.Cryptography;
using System.Linq;


[Serializable]
public class FileVersion : IComparable<FileVersion>
{
    public string file;
    public string md5;
    //  public string crc;//for fast check
    public int size;
    //排序是为了让 检查线程工作更均匀 小文件在前面 让 进度条看起来更快
    public int CompareTo(FileVersion other)
    {
        return -(this.size - (other == null ? 0 : other.size));
    }
}


//这个是用来 文件检查时用的数据结构 会包含额外
class FileVersionChecker
{
    //   public bool force_check_md5 = false;// is fsfirm ?
    public FileStatus status = FileStatus.NotCheck;
}

public enum FileStatus
{
    NotCheck,
    FileOK,
    FileError,
};

[Serializable]
public class FileVersionList
{
    public List<FileVersion> files = new System.Collections.Generic.List<FileVersion>();
}

public static class MD5Code
{
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            using (FileStream file = new FileStream(fileName, System.IO.FileMode.Open))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR
            Debug.LogError("GetMD5HashFromFile() fail,error:" + ex.Message);
#endif
        }
        return "";
    }
}
public enum CheckResult
{
    Unknown = 0,
    NeedForceUpdate = 1,
    NeedHotUpdate = 2,
    DoNot = 3,
}

namespace Patches
{

    class WiseDownload
    {
        private string url;
        private string savePath;
        public long downloadByte;
        public long totalByte;
        public bool isDone = false;
        public bool isStart = false;
        public bool isError = false;
        public HttpStatusCode retCode = 0;
        public string errorMsg = "";
        public byte[] bytes = null;
        private string md5 = "";
        private int retryTimes = 0;
        public WiseDownload(string url, string savePath, string md5)
        {
            //if (DevConfig._ServerPlatform == ServerPlatform.ServerQA || DevConfig._ServerPlatform == ServerPlatform.ServerDev)
            //{
            //    Debug.Log("Download url=" + url + "  save=" + savePath);
            //}
            this.url = url;
            this.savePath = savePath;
            this.md5 = md5;
            var thread = new Thread(new ThreadStart(Down));
            thread.Start();
        }


        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
        private const int MAX_RETRY_TIMES = 10;
        public void Down()
        {
            try
            {
                downloadByte = 0;
                long lStartPos = 0;
                Stream fs = null;
                try
                {
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                }
                catch (Exception e) { }
                Thread.Sleep(50);
                //if ((DevConfig._ServerPlatform == ServerPlatform.ServerQA || DevConfig._ServerPlatform == ServerPlatform.ServerDev) && retryTimes > 0)
                //{
                //    Debug.LogWarning("retry times is " + retryTimes);
                //}
                if (retryTimes > 0)
                {
                    //证明是重试阶段
                    Thread.Sleep(2000);
                }
                //如果目录不存在，需要创建目录
                int found = savePath.LastIndexOf("/");
                if (found != savePath.Length)
                {
                    string directoryPath = savePath.Substring(0, found);
                    Directory.CreateDirectory(directoryPath);
                }

                fs = new FileStream(savePath, FileMode.Create);
                lStartPos = 0;


                HttpWebRequest request = null;
                HttpWebResponse response = null;
                try
                {
                    //如果是发送HTTPS请求  
                    if (this.url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                        request = (HttpWebRequest)WebRequest.Create(url);

                    }
                    else
                    {
                        request = (HttpWebRequest)WebRequest.Create(this.url);
                    }
                    //ms
                    request.Timeout = 10000;
                    request.ReadWriteTimeout = 10000;
                    request.Method = "GET";
                    //Debug.Log("request.KeepAlive == " + request.KeepAlive);
                    //Debug.Log("System.Net.ServicePointManager.DefaultConnectionLimit == " + System.Net.ServicePointManager.DefaultConnectionLimit);
                    //http1.1默认为true,不适用持久链接,DefaultConnectionLimit=2,避免由于过多http连接导致请求失败，解决多次请求http失败的问题
                    //web一般使用短连接
                    request.KeepAlive = false;
                    request.ProtocolVersion = HttpVersion.Version11;

                    //允许重定向默认就是true
                    //request.AllowAutoRedirect = true;
                    //it must be net framework 4.0 +
                    //HttpWebRequestExtension.SetRawHeader(request, "Host", "dir.lastone.qq.com");

                    response = (HttpWebResponse)request.GetResponse();
                    retCode = response.StatusCode;
                    //标识链接成功建立,此时才应该有进度信息的回调
                    isStart = true;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Redirect:
                            {
                                //发生重定向就重新获取 
                                this.url = response.Headers["Location"];
                                this.url = this.url.Trim();
                                response.Close();
                                response = null;
                                request.Abort();
                                request = null;
                                Down();
                                return;
                            }
                        case HttpStatusCode.RequestTimeout:
                        case HttpStatusCode.GatewayTimeout:
                            {
                                response.Close();
                                response = null;
                                request.Abort();
                                request = null;
                                Debug.Log("请求超时了，请检查网络状况");
                                if (retryTimes < MAX_RETRY_TIMES)
                                {
                                    // 如果当前有文件 重新下载吧
                                    if (File.Exists(savePath))
                                    {
                                        File.Delete(savePath);
                                    }
                                    isError = false;
                                    retryTimes++;
                                    Down();
                                    return;
                                }
                                isDone = true;
                                isError = true;
                                return;
                            }
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Created:
                        case HttpStatusCode.Accepted:
                        case HttpStatusCode.NonAuthoritativeInformation:
                        case HttpStatusCode.NoContent:
                        case HttpStatusCode.ResetContent:
                        case HttpStatusCode.PartialContent:
                            {
                                break;
                            }
                        default:
                            {
                                response.Close();
                                response = null;
                                request.Abort();
                                request = null;
                                Debug.Log("httq请求错误，retcode== " + retCode.ToString());
                                if (retryTimes < MAX_RETRY_TIMES)
                                {
                                    // 如果当前有文件 重新下载吧
                                    if (File.Exists(savePath))
                                    {
                                        File.Delete(savePath);
                                    }
                                    isError = false;
                                    retryTimes++;
                                    Down();
                                    return;
                                }
                                isError = true;
                                isDone = true;
                                return;
                            }
                    }

                    /***
                    StreamReader weatherStreamReader = new StreamReader(request.GetResponse().GetResponseStream(), System.Text.Encoding.UTF8);
                    //读取数据(直接文本内容了)
                    string retStream = weatherStreamReader.ReadToEnd();
                    weatherStreamReader.Close();
                     **/

                    //response.ContentLength 只返回断点之后的部分
                    var length = response.ContentLength + downloadByte;
                    this.totalByte = length;

                    if (downloadByte < length)
                    {
                        if (lStartPos > 0)
                        {
                            ///不支持断点续传添加容错处理///
                            /*   if (request.GetResponse().Headers["Content-Range"] == null)
                               {
                                   Debug.LogWarning("it can not support  resume from break point");

                                   downloadByte = 0;
                                   if (File.Exists(savePath))
                                   {
                                       File.Delete(savePath);
                                   }
                               }*/
                        }

                        Stream ns = response.GetResponseStream();
                        byte[] nbytes = new byte[1024];
                        int nReadSize = 0;
                        nReadSize = ns.Read(nbytes, 0, 1024);
                        while (nReadSize > 0)
                        {
                            fs.Write(nbytes, 0, nReadSize);
                            downloadByte += nReadSize;
                            nReadSize = ns.Read(nbytes, 0, 1024);
                            fs.Flush();
                        }
                        fs.Close();
                        ns.Close();
                        response.Close();
                        response = null;
                        request.Abort();
                        request = null;
#if UNITY_EDITOR
                        Debug.Log("DownloadByte:{0}" + downloadByte);
                        Debug.Log("length: {0}" + length);
#endif
                        if (downloadByte > length)
                        {
                            if (File.Exists(savePath))
                            {
                                File.Delete(savePath);
                            }
                            Down();
                            return;
                        }
                        else if (downloadByte < length)
                        {
                            isError = true;
                            errorMsg = "download data the length==" + length;
                            Debug.Log("下载数据错误，数据长度length={0}" + length);
                            if (retryTimes < MAX_RETRY_TIMES)
                            {
                                // 如果当前有文件 重新下载吧
                                if (File.Exists(savePath))
                                {
                                    File.Delete(savePath);
                                }
                                isError = false;
                                retryTimes++;
                                Down();
                                return;
                            }
                            Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                        }
                        else
                        {

                        }
                    }
                    else if (downloadByte > length)
                    {
                        fs.Close();
                        response.Close();
                        response = null;
                        request.Abort();
                        request = null;

                        if (File.Exists(savePath))
                        {
                            File.Delete(savePath);
                        }

                        if (length > 0)
                        {
                            Down();
                            return;
                        }
                        else
                        {
                            isError = true;
                            errorMsg = "request data the length==" + length;
                            Debug.Log("请求数据错误，数据长度length={0}" + length);

                            if (retryTimes < MAX_RETRY_TIMES)
                            {
                                // 如果当前有文件 重新下载吧
                                if (File.Exists(savePath))
                                {
                                    File.Delete(savePath);
                                }
                                isError = false;
                                retryTimes++;
                                Down();
                                return;
                            }
                            Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                        }

                    }
                    else
                    {
                        fs.Close();
                        response.Close();
                        response = null;
                        request.Abort();
                        request = null;

                        if (length == 0)
                        {
                            isError = true;
                            errorMsg = "request data the length==" + length;
                            Debug.Log("请求数据错误，数据长度length={0}" + length);
                            if (retryTimes < MAX_RETRY_TIMES)
                            {
                                // 如果当前有文件 重新下载吧
                                if (File.Exists(savePath))
                                {
                                    File.Delete(savePath);
                                }
                                isError = false;
                                retryTimes++;
                                Down();
                                return;
                            }
                            Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                        }

                    }
                }
                catch (Exception e)
                {
                    //下载zip过程中网络断开，会直接抛出异常
                    isError = true;
                    if (fs != null)
                    {
                        fs.Close();
                        fs = null;
                    }
                    if (response != null)
                    {
                        response.Close();
                        response = null;
                    }
                    if (request != null)
                    {
                        request.Abort();
                        request = null;
                    }

                    errorMsg = e.ToString();

                    if (retryTimes < MAX_RETRY_TIMES)
                    {
                        // 如果当前有文件 重新下载吧
                        if (File.Exists(savePath))
                        {
                            File.Delete(savePath);
                        }
                        isError = false;
                        retryTimes++;
                        Down();
                        return;
                    }
                    try
                    {
                        string error_str = errorMsg;
                        if (error_str.Contains("404") || error_str.Contains("Connect"))
                        {
                            Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                        }
                        else if (error_str.Contains("timed"))
                        {
                            Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                        }
                        else
                        {
                            Patcher.MakeLastError(PatcherErrorCode.DL_UNKNOW_ERROR, error_str);
                        }
                    }
                    catch (Exception eee)
                    {
                        Patcher.MakeLastError(PatcherErrorCode.DL_UNKNOW_ERROR, eee.Message);
                    }
                }
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    isError = true;
#if UNITY_EDITOR
                    Debug.LogError("errorMsg:{0} {1}" + errorMsg + url);
#endif
                    if (retryTimes < MAX_RETRY_TIMES)
                    {
                        // 如果当前有文件 重新下载吧
                        if (File.Exists(savePath))
                        {
                            File.Delete(savePath);
                        }
                        isError = false;
                        retryTimes++;
                        errorMsg = "";
                        Down();
                        return;
                    }
                }
                isDone = true;
#if UNITY_EDITOR
                Debug.Log("IsDone:{0}" + isDone);
                Debug.Log("IsError:{0}" + isError);
#endif
                string error_str1 = errorMsg;
                if (error_str1.Contains("404") || error_str1.Contains("Connect"))
                {
                    Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                }
                else if (error_str1.Contains("timed"))
                {
                    Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                }
                else
                {
                    Patcher.MakeLastError(PatcherErrorCode.DL_UNKNOW_ERROR, error_str1);
                }
            }
            catch (Exception e)
            {
                this.isDone = true;
                this.isError = true;
                //if (DevConfig._ServerPlatform != ServerPlatform.ServerRelease)
                //{
                //    Debug.LogError("PatcherDownload:" + e.Message);
                //}
                string error_str = e.Message;
                if (error_str.Contains("404") || error_str.Contains("Connect"))
                {
                    Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                }
                else if (error_str.Contains("timed"))
                {
                    Patcher.MakeLastError(PatcherErrorCode.DL_NET_ERROR, "");
                }
                else
                {
                    Patcher.MakeLastError(PatcherErrorCode.DL_UNKNOW_ERROR, error_str);
                }
            }
        }
        //需要引用 using System.Collections.Specialized; 
        /***
        public static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        } 
        ***/
    }

    public class Version : IComparable<Version>
    {
        //1.7.1
        public int MainVersion = 1;//主版本
        public int SubVersion = 7;//子版本
        public int PatchVersion = 0;//补丁包版本
        public string MD5 = "";//zip
        public int FileSize = 0;//文件大小
        public string version = "";//

        public string GetString()
        {
            return this.MainVersion + "." + this.SubVersion + "." + this.PatchVersion;
        }
        //1.7.0
        public void Parse(string str)
        {
            var vs = str.Split('.');
            this.MainVersion = int.Parse(vs[0]);
            this.SubVersion = int.Parse(vs[1]);
            this.PatchVersion = int.Parse(vs[2]);
            this.version = str;
        }
        public bool Match(Version other)
        {
            if (MainVersion != other.MainVersion) return false;
            if (SubVersion != other.SubVersion) return false;
            if (PatchVersion != other.PatchVersion) return false;

            return true;
        }
        public bool WillForceUpdate(Version other)
        {
            if (MainVersion == other.MainVersion && SubVersion == other.SubVersion) return false;

            return true;
        }

        public int CompareTo(Version other)
        {
            if (Match(other))
            {
                return 0;
            }
            if (this.GetVersionLong() > other.GetVersionLong())
            {
                //自己大
                return 1;
            }
            return -1;
            if (this.MainVersion > other.MainVersion) return 1;//自己大
            if (this.MainVersion < other.MainVersion) return -1;//自己小

            if (this.SubVersion > other.SubVersion) return 1;//自己大
            if (this.SubVersion < other.SubVersion) return -1;//自己小

            if (this.PatchVersion > other.PatchVersion) return 1;//自己大
            if (this.PatchVersion < other.PatchVersion) return -1;//自己小

            return 0;
        }
        public long GetVersionLong()
        {
            long version_long = 0;
            version_long += ((long)MainVersion) * 1000000000000L;
            version_long += (long)(SubVersion) * 1000000L;
            version_long += (long)(PatchVersion);
            return version_long;
        }
    }

    public enum PatcherErrorCode
    {
        //下载相关的错误
        DL_UNKNOW_ERROR = 1,
        DL_NET_ERROR,
        DL_FILE_ERROR,
    }

    public class FileChecker
    {
        public static bool IsValid(string file, string md5)
        {
            if (md5 != null && md5.Length > 0 && md5 == MD5Code.GetMD5HashFromFile(file))
            {
                return true;
            }
            return false;
        }
    }

    public class Patcher : MonoBehaviour
    {
        private bool mUseLb = true;
        public bool useLebian { get { return mUseLb; } set { mUseLb = value; } }
        public static Patcher ins = null;
        public static string LastErrorStr = "";
        public static void MakeLastError(PatcherErrorCode code, string str)
        {
            if (code == PatcherErrorCode.DL_NET_ERROR)
            {
                LastErrorStr = "";
            }
            else
            {
                LastErrorStr = str;
            }
        }
        void Awake()
        {
            ins = this;
            time = Time.time;

            this.CheckVersionFileValidate();
            //if (Patches.Patcher.ins.useLebian)
            //    HPermissionUtils.CallLebianCheckVersion();

        }
        void OnDestroy()
        {
            if (ins = this)
            {
                ins = null;
                co_check_current_max_version = null;
                co_checkdownload_task = null;
                downTask = null;
            }
            if (_install != null)
            {
                _install.Terminal();
                _install = null;
            }
        }

        void Update()
        {
            if (_LuaHasResponseOK == false)
            {
                if (Time.time - time > 20)
                {
                    Debug.LogError("LuaResponse Timeout Force End Game Process and Auto RepairClient");
                    this.RepairClient(true);
                    Thread.Sleep(1000);//尽可能保证 错误 抛到了bugly
                    Application.Quit();
                    return;
                }
            }

            if (useLebian) return;
            //内置的安装包下载器 下载
            if (_install != null)
            {
                //正在 下载 安装包
                if (InstallerDownloader._Status == InstallerDownloader.Status.None)
                {
                    //出于 等待状态
                    //Base.Events.ins.FireLua("global", "InstallNone");
                }
                else if (InstallerDownloader._Status == InstallerDownloader.Status.Error)
                {
                    //下载错误
                    //Base.Events.ins.FireLua("global", "InstallError");
                    _install.Terminal();
                    _install = null;
                }
                else if (InstallerDownloader._Status == InstallerDownloader.Status.Checking)
                {
                    //检查中
                    //Base.Events.ins.FireLua("global", "InstallChecking");
                }
                else if (InstallerDownloader._Status == InstallerDownloader.Status.Downloading)
                {
                    //下载中
                    //Base.Events.ins.FireLua("global", "InstallDownloading", ToFileSizeString(InstallerDownloader.CurrentSize), ToFileSizeString(InstallerDownloader.TotalSize),
                         //Mathf.Clamp((int)(((float)InstallerDownloader.CurrentSize / (float)InstallerDownloader.TotalSize * 100f)), 0, 100));
                }
                else if (InstallerDownloader._Status == InstallerDownloader.Status.Verifying)
                {
                    //校验MD5 中
                    //Base.Events.ins.FireLua("global", "InstallVerifying");
                }
                else if (InstallerDownloader._Status == InstallerDownloader.Status.OK)
                {
                    //下载成功
                    //Base.Events.ins.FireLua("global", "InstallOK");
                    _install.Terminal();
                    _install = null;
                    InstallerDownloader._Status = InstallerDownloader.Status.OK;
                    bool ok = InstallerDownloader.StartInstallApk(InstallerDownloader.InstallRootDir + "/" + InstallerDownloader.ApkName);
                    if (!ok)
                    {
                        Debug.LogError("InstallerDownloader.INATALL.ERROR" + Application.persistentDataPath);

                        //安装apk的时候  安装失败 
                        //if (LuaInterface.LuaMgr.ins != null)
                        //{
                        //    LuaInterface.LuaMgr.ins.CallGlobalFunction("PATCH2_INSTALL_HAS_BEEN_ABORT");
                        //}
                        //else
                        //{
                        //    Application.Quit();
                        //}
                    }
                }
                else
                {
                    Debug.LogError("unknow status " + InstallerDownloader._Status);
                    _install.Terminal();
                    _install = null;
                    InstallerDownloader._Status = InstallerDownloader.Status.OK;

                }
            }
        }
        public bool _LuaHasResponseOK = false;
        float time = 0;
        public void LuaResponseOK()
        {
            _LuaHasResponseOK = true;
        }
        //void StopAllCoroutines()
        //{
        //    throw new NotSupportedException();
        //}
        Version base_version = null;
        Version GetBaseVersion()
        {
            if (this.base_version != null)
            {
                return this.base_version;
            }
            try
            {
                //处理 固件版本
                var base_version = new Version();
                base_version.Parse(__BASE_VERSION__);
                this.base_version = base_version;
                return this.base_version;
            }
            catch (Exception e) { }
            return null;
        }
        static string EnginePath
        {
            get
            {
#if UNITY_EDITOR
                return "Patch3/AssetBundles/engine";
#else
                return Application.persistentDataPath + "/Patch3/AssetBundles/engine";
#endif
            }
        }
        Version GetCurrentVersion()
        {
            try
            {
                //处理 固件版本
                var ret = new Version();
                ret.Parse(GetVersion());
                return ret;
            }
            catch (Exception e) { }
            return null;
        }
        private string max_version = null;//最大版本 包括了 base_version ,local version,server version
        bool IsNewerFirm = false;
        //检查 版本目录 是否需要清理掉
        void CheckVersionFileValidate()
        {
            //
            var current_v = GetVersion();
            var current_version = new Version();
            var base_version = GetBaseVersion();
            var ok = false;
            try
            {
                //处理 固件版本 和热更版本
                current_version.Parse(current_v);
                if (base_version != null)
                {
                    ok = true;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Patcher2.cs this error must be not been seen " + e.Message + " " + e.StackTrace);
                //如果出现了 证明出现了致命性错误  要么 配置错误(__BASE_VERSION__) 要么需要重置客户端
                ok = false;
            }
            if (ok)
            {
                //版本转换成功 开始检查
                //不取等号是因为 可能覆盖了安装 可以清理掉废掉的 补丁信息
                //有可能补丁更新到1.7.8 但是覆盖APP版本也是1.7.8  其实也是可以重置的 
                //固定版本 和 当前补丁版本 的大版本一致 表示只有补丁的存在
                // 如果是覆盖安装的老版本固件 只是补丁的话 是不会清理的因为覆盖安装只会覆盖streamingassets patch目录并不会被清理 因此可以保留        
                //为了兼容 这个版本以下的 Patch目录暴力删除
                if (current_version.MainVersion <= 1 && current_version.SubVersion <= 28)
                {
                    try
                    {
                        Directory.Delete(PatchRootDir, true);
                    }
                    catch (Exception e) { }
                }
                if (base_version.GetVersionLong() >= current_version.GetVersionLong())
                {
                    IsNewerFirm = true;
                }
            }
            else
            {
                //DevConfig.WillRebotClient = true;
                //转换失败 表示客户端严重错误 可以直接强制重置版本
                RepairClient();
                Debug.LogError("Fatal error in Patcher2.cs reset client to factory with parse");
            }
        }

        //修复客户端
        public void RepairClient(bool slice = false)
        {
            //step1 清理版本信息 如果不一样的话
            if (__BASE_VERSION__ != GetVersion())
            {
                PlayerPrefs.SetString("__client_version__", __BASE_VERSION__);
                PlayerPrefs.Save();
            }

            //step2 清理缓存目录
            try
            {
                //删除本地缓存文件
                //LocalStorageMgr.DeleteCacheRootDirectory();
                //删除本地 玩家数据缓存
                //LocalCacheMgr.DeleteRootDirectory();
            }
            catch (Exception e) { }
            if (slice == false)
            {
                //修复完毕 通知Lua
                try
                {
                    //LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "RepairClientDone");
                }
                catch (Exception e)
                {
                    //step 优先通知lua显示ui 因为后面会删除补丁目录 防止找不到资源
                    try
                    {
                        Directory.Delete(PatchRootDir, true);
                    }
                    catch (Exception ee) { }
                    Application.Quit();
                }
            }
            //step2 清理Patch目录
            try
            {
                Directory.Delete(PatchRootDir, true);
            }
            catch (Exception e) { }
            //清理 读写目录
            try
            {
                foreach (var p in Directory.GetDirectories(Application.persistentDataPath))
                {
                    try
                    {
                        //  Directory.Delete(p, true);
                    }
                    catch (Exception e) { }
                }
            }
            catch (Exception e)
            {

            }
        }
        //获取版本前缀web 静态连接 所有配置服务器列表什么的 都走不同的版本号文件夹
        public static string GetVersionWebPre()
        {
            var current_version = new Version();
            var ok = false;
            try
            {
                //处理 固件版本 和热更版本
                current_version.Parse(GetVersion());
                ok = true;
            }
            catch (Exception e)
            {
                Debug.Log("Patcher2.cs this error must be not been seen " + e.Message + " " + e.StackTrace);
                //如果出现了 证明出现了致命性错误  要么 配置错误(__BASE_VERSION__) 要么需要重置客户端
                ok = false;
            }
            if (ok)
            {
                string str = current_version.MainVersion + "." + current_version.SubVersion;
                return str;
            }
            return "x.x";
        }
        //更新成功后 需要写入版本信息
        public void CheckSaveVersionInfo()
        {
            //if (StaticData.ClientVersionHotUpdateStatus == CheckResult.NeedForceUpdate)
            //{
            //    //检测出来需要强更 忽略版本设定 因为会继续更新资源 下载apk后 新apk会处理
            //}
            //else
            //{
            //    if (max_version != null)
            //    {
            //        PlayerPrefs.SetString("__client_version__", max_version);
            //        PlayerPrefs.Save();
            //    }
            //    else
            //    {
            //        //fatal error
            //        PlayerPrefs.SetString("__client_version__", __BASE_VERSION__);
            //        PlayerPrefs.Save();
            //    }
            //}
        }
        public static string VersionListCacheFileName
        {
            get
            {
#if UNITY_EDITOR
                return "VersionListCache.json";
#else
                return Application.persistentDataPath + "/" + "VersionListCache.json";
#endif
            }
        }

        #region 检查客户端版本信息

        public static string __BASE_VERSION__
        {
            get
            {
                //return DevConfig.__VERSION__;
                return "";
            }
        }

        public static string GetVersion()
        {
            if (Patcher.ins.useLebian)
            {
                PlayerPrefs.SetString("__client_version__", __BASE_VERSION__);
                return __BASE_VERSION__;
            }
            var current_v = PlayerPrefs.GetString("__client_version__");
            if (string.IsNullOrEmpty(current_v))
            {
                current_v = __BASE_VERSION__;
                PlayerPrefs.SetString("__client_version__", current_v);
                PlayerPrefs.Save();
            }
            return current_v;
        }
        public static string ToFileSizeString(int size)
        {
            if (size <= 0)
            {
                return "0MB";
            }
            double s = (double)size;
            s = s / 1024.0 / 1024.0;
            return s.ToString("0.00") + "MB";
        }
        Coroutine co_check_current_max_version = null;
        public void CheckVersion(bool back_ground)
        {
            //Debug.Log("<color=red>checkversion!!!!!!!!!!!!!!!!!!!!!!!</color>");
            if (co_check_current_max_version != null)
            {
                return;
                //do not suport multi call
            }
            co_check_current_max_version = StartCoroutine(CheckCurrentMaxVersion(back_ground));
        }
        string _current_server_version_ = null;
        MAX_VERSION _MAX_VERSION = null;
        IEnumerator CheckCurrentMaxVersion(bool back_ground)
        {
            if (useLebian)
            {
#if UNITY_EDITOR
                //   HPermissionUtils.CallLebianCheckVersion();
                yield return new WaitForSecondsRealtime(0.5f);
                //StaticData.ClientVersionHotUpdateStatus = CheckResult.DoNot;
                //if (GatewayServer.ins != null)
                //{
                //    GatewayServer.ins.OnCheckVersionResult(CheckResult.DoNot);
                //}
                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_CHECK_VERSION_RESULT", (int)CheckResult.DoNot, _MAX_VERSION);
                //}
#else

                HPermissionUtils.CallLebianCheckVersion();
                //wait lebian result
                int frames = 0;
                //10秒乐变响应超时
                while (HPermissionUtils.LebianResult == 0 && frames <= 10*60)
                {
                    ++frames;
                    yield return new WaitForEndOfFrame();
                }
                int lbresult = HPermissionUtils.LebianResult;
                if(frames>=10*60)
                {
                    lbresult = 0;
                }
                //-2: sdk未准备好
                //-1：请求失败
                //1：未知错误
                //2：没有更新
                //3：有非强更版本
                //4：有强更版本
                if (lbresult == 4 || lbresult == 3)
                {
                    //有更新 需要等待乐变更新
                    //IOS 下 强更需要跳转 appstore
                    if (DevConfig._GamePlatform == GamePlatform.Android)
                    {
                        StaticData.ClientVersionHotUpdateStatus = CheckResult.NeedHotUpdate;
                        if (GatewayServer.ins != null)
                        {
                            GatewayServer.ins.OnCheckVersionResult(CheckResult.NeedHotUpdate);
                        }
                        if (LuaInterface.LuaMgr.ins != null)
                        {
                            LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_CHECK_VERSION_RESULT", (int)CheckResult.DoNot, _MAX_VERSION, lbresult);
                        }
                    }
                    else // ios
                    {
                        if (lbresult == 3)
                        {
                            StaticData.ClientVersionHotUpdateStatus = CheckResult.NeedHotUpdate;
                            if (GatewayServer.ins != null)
                            {
                                GatewayServer.ins.OnCheckVersionResult(CheckResult.NeedHotUpdate);
                            }
                            if (LuaInterface.LuaMgr.ins != null)
                            {
                                LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_CHECK_VERSION_RESULT", (int)CheckResult.DoNot, _MAX_VERSION, lbresult);
                            }
                        }
                        else
                        {
                            //强更 需要跳转 appstore
                            StaticData.ClientVersionHotUpdateStatus = CheckResult.NeedForceUpdate;
                            if (GatewayServer.ins != null)
                            {
                                GatewayServer.ins.OnCheckVersionResult(CheckResult.NeedForceUpdate);
                            }
                            if (LuaInterface.LuaMgr.ins != null)
                            {
                                LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_CHECK_VERSION_RESULT", (int)CheckResult.DoNot, _MAX_VERSION, lbresult);
                            }
                        }
                    }
                }
                else if (lbresult == 2)
                {
                    //没有更新
                    StaticData.ClientVersionHotUpdateStatus = CheckResult.DoNot;
                    if (GatewayServer.ins != null)
                    {
                        GatewayServer.ins.OnCheckVersionResult(CheckResult.DoNot);
                    }
                    if (LuaInterface.LuaMgr.ins != null)
                    {
                        LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_CHECK_VERSION_RESULT", (int)CheckResult.DoNot, _MAX_VERSION);
                    }
                }
                else
                {
                    //网络错误  或者内部错误
                    StaticData.ClientVersionHotUpdateStatus = CheckResult.Unknown;
                    if (GatewayServer.ins != null)
                    {
                        GatewayServer.ins.OnCheckVersionResult(CheckResult.Unknown);
                    }
                    if (LuaInterface.LuaMgr.ins != null)
                    {
                        LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_CHECK_VERSION_RESULT", (int)CheckResult.DoNot, _MAX_VERSION, lbresult);
                    }
                }
#endif

                co_check_current_max_version = null;
                yield break;
            }
            else
            {
                Debug.Assert(false);//屏蔽 旧的更新方式
                string url = "";//NetConfig.GateWayWWW_IP;
                var result = CheckResult.Unknown;
                //StaticData.ClientVersionHotUpdateStatus = CheckResult.Unknown;
                this._current_server_version_ = null;
                Version current = null;
                _MAX_VERSION = null;
                max_version = null;
                Version current_server_version = null;//当前服务器版本
                                                      //WWW www = null;
                UnityWebRequest www = null;

                //if (DevConfig._GamePlatform == GamePlatform.Android)
                //{
                //    //android 平台内嵌了下载器 走另外一套逻辑
                //    www = UnityWebRequest.Get(url + "/version/" + (IsNewerFirm ? __BASE_VERSION__ : GetVersion()) + "/MAX_VERSION_" + DevConfig._DownloadSourcesLua + ".json");
                //}
                //else
                {
                    //   www = new UnityEngine.WWW(url + "/version/MAX_VERSION_IOS.json");
                    www = UnityWebRequest.Get(url + "/version/" + (IsNewerFirm ? __BASE_VERSION__ : GetVersion()) + "/MAX_VERSION_IOS.json");
                }
                using (www)
                {
                    www.timeout = 8;

                    Debug.Log(www.url);
                    //var time_now = Utils.GetTimestampSeconds();
                    yield return www.Send();
                    if (www.isDone && www.isNetworkError == false)
                    {
                        Version v = new Version();
                        bool ok = false;
                        try
                        {
                            var vvvv = JsonUtility.FromJson<MAX_VERSION>(www.downloadHandler.text);
                            if (vvvv.IsLegal())
                            {
                                try
                                {
                                    v.Parse(vvvv.Version);
                                    current_server_version = v;
                                    _current_server_version_ = vvvv.Version;
                                    ok = true;
                                    _MAX_VERSION = vvvv;
                                    //检查版本列表文件缓存信息            
                                    try
                                    {
                                        if (_MAX_VERSION.FileVersionListMD5 != null && _MAX_VERSION.FileVersionListMD5.Length > 3 && File.Exists(VersionListCacheFileName) && _MAX_VERSION.FileVersionListMD5 == MD5Code.GetMD5HashFromFile(VersionListCacheFileName))
                                        {

                                        }
                                        else
                                        {
                                            //缓存已经失效 需要重新下载
                                            try
                                            {
                                                File.Delete(VersionListCacheFileName);
                                            }
                                            catch (Exception e) { }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        //缓存已经失效 需要重新下载
                                        try
                                        {
                                            File.Delete(VersionListCacheFileName);
                                        }
                                        catch (Exception ee) { }
                                    }
                                    vvvv = null;
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning(e.Message + "  " + e.StackTrace);
                                    ok = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e.Message);
                        }
                        int delay = 0; //= Mathf.Clamp((int)(Utils.GetTimestampSeconds() - time_now), 0, 8);
                        if (ok == false && delay > 0)
                        {
                            yield return new WaitForSeconds(delay);
                        }
                        if (ok)
                        {
                            string current_v;
                            if (IsNewerFirm)
                            {
                                //固件是新版本 因此需要 从固件版本来对比
                                current_v = __BASE_VERSION__;
                            }
                            else
                            {
                                current_v = GetVersion();
                            }
                            current = new Version();
                            ok = false;
                            try
                            {
                                //如果这里失败了 那么表示 本地记录被写坏了 需要重置客户端
                                current.Parse(current_v);
                                ok = true;
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning(e.Message + "  " + e.StackTrace);
                                ok = false;
                            }
                            if (ok)
                            {
                                //需要拿到最高版本的 版本号前缀
                                if (current.GetVersionLong() >= v.GetVersionLong())
                                {
                                    max_version = current.version;
                                }
                                else
                                {
                                    max_version = v.version;
                                }
                                if (v.Match(current))//match 没有处理服务器版本小于 当前客户端版本的情况 ,只有配置错误才会出现应该抛出一个error日志
                                {//版本匹配 无需更新
                                    result = CheckResult.DoNot;
                                    //清理 安装包下载器 目录
                                    try
                                    {
                                        Directory.Delete(InstallerDownloader.InstallRootDir, true);
                                    }
                                    catch (System.IO.DirectoryNotFoundException e)
                                    {

                                    }
                                    catch (Exception e)
                                    {
#if UNITY_EDITOR
                                        Debug.LogWarning(e.Message);
#endif
                                    }
                                }
                                else if (current.GetVersionLong() > v.GetVersionLong())
                                {//客户端版本 大于服务器  IOS审核 或者 包提前发出去了  都当作不需要更新处理
                                    result = CheckResult.DoNot;
                                    //清理 安装包下载器 目录
                                    try
                                    {
                                        Directory.Delete(InstallerDownloader.InstallRootDir, true);
                                    }
                                    catch (System.IO.DirectoryNotFoundException e)
                                    {

                                    }
                                    catch (Exception e)
                                    {
#if UNITY_EDITOR
                                        Debug.LogWarning(e.Message);
#endif
                                    }
                                }
                                else
                                {
                                    //if (DevConfig._GamePlatform == GamePlatform.IOS)
                                    {
                                        //ios 下
                                        if (v.MainVersion * 100000 + v.SubVersion > current.MainVersion * 100000 + current.SubVersion)
                                        {
                                            //需要强更
                                            result = CheckResult.NeedForceUpdate;
                                        }
                                        else
                                        {
                                            //需要热更
                                            result = CheckResult.NeedHotUpdate;
                                            //清理 安装包下载器 目录
                                            try
                                            {
                                                Directory.Delete(InstallerDownloader.InstallRootDir, true);
                                            }
                                            catch (System.IO.DirectoryNotFoundException e)
                                            {

                                            }
                                            catch (Exception e)
                                            {
#if UNITY_EDITOR
                                                Debug.LogWarning(e.Message);
#endif
                                            }
                                        }
                                    }
                                    //else if (DevConfig._GamePlatform == GamePlatform.Android)
                                    {
                                        //android 下
                                        if (v.MainVersion > current.MainVersion)
                                        {
                                            //需要强更
                                            result = CheckResult.NeedForceUpdate;
                                        }
                                        else
                                        {
                                            //需要热更
                                            result = CheckResult.NeedHotUpdate;
                                            //清理 安装包下载器 目录
                                            try
                                            {
                                                Directory.Delete(InstallerDownloader.InstallRootDir, true);
                                            }
                                            catch (System.IO.DirectoryNotFoundException e)
                                            {

                                            }
                                            catch (Exception e)
                                            {
#if UNITY_EDITOR
                                                Debug.LogWarning(e.Message);
#endif
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                RepairClient();
                                result = CheckResult.Unknown;
                                yield break;
                            }
                        }
                    }
                    else
                    {
                        //
                    }
                }
                //StaticData.ClientVersionHotUpdateStatus = result;
                yield return new WaitForEndOfFrame();

                co_check_current_max_version = null;
                //if (GatewayServer.ins != null)
                //{
                //    GatewayServer.ins.OnCheckVersionResult(result);
                //}

                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    if (result == CheckResult.NeedForceUpdate)
                //    {
                //        LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_CHECK_VERSION_RESULT", (int)result, _MAX_VERSION,
                //           ToFileSizeString(InstallerDownloader.GetLeftSize(InstallerDownloader.InstallRootDir, "hcr.apk", (_MAX_VERSION.FileSize))));
                //    }
                //    else
                //    {
                //        LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_CHECK_VERSION_RESULT", (int)result, _MAX_VERSION);
                //    }
                //}
            }
        }
        #endregion


        #region 检查文件校对信息
        public void CheckFileVersionList(bool check_md5)
        {
            if (check_version_info != null)
            {
                //do not suport multi call
                return;
            }
            check_version_info = StartCoroutine(CheckVersionInfo(check_md5));
        }
        Coroutine check_version_info = null;
        Dictionary<FileVersion, FileVersionChecker> _thread_tags = null;
        Dictionary<string, FileVersion> _fsfirm_files = null; //带资源包发布的 列表

        //有一些核心文件是必须 检查MD5的
        public static bool ForceCheckMD5(string file)
        {
            if (string.IsNullOrEmpty(file)) return false;
            try
            {
                //lua 文件目录
                if (file.Contains("/lua/lua"))
                {
                    return true;
                }
                //通用强制检查md5资源的命名规范
                //fs = file system
                //firm = firm
                if (file.Contains("fsfirm"))
                {
                    return true;
                }
                if (file.Contains("AssetBundles/AssetBundles"))
                {
                    return true;
                }
                //更新包含了C# 
                if (file.Contains("/engine/"))
                {
                    return true;
                }
            }
            catch (Exception e) { }
            return false;
        }
        //文件检查行为
        /*   enum CheckFileMode
           {
               None,//无需检查
               Fast,//快速检查 即存在和文件大小
               MD5, // 检查MD5  属于严格模式
           }*/
        bool WillRebotClient = false;
        void FileCheckThreadTask(object param)
        {
            var task = param as List<FileVersion>;
            while (task != null && task.Count > 0 && _fsfirm_files != null)
            {
                var p = task[task.Count - 1];
                bool file_is_ok = false;
                try
                {
                    do
                    {
                        if (Patches.Patcher.ins.useLebian)
                        {
                            file_is_ok = true;
                            break;
                        }
#if UNITY_EDITOR && DISABLE_FILE_VALID
                        file_is_ok = true;
                        break;
#endif
                        FileVersion firm_info = null;
                        if (_fsfirm_files.ContainsKey(p.file))
                        {
                            firm_info = _fsfirm_files[p.file];
                        }

                        var status = CheckResult.Unknown;// StaticData.ClientVersionHotUpdateStatus;
                        FileInfo info = null;
                        string full_path = PatchRootDir + "/" + p.file;
                        try
                        {
                            info = new System.IO.FileInfo(full_path);
                            var l = info.Length;
                        }
                        catch (Exception e)
                        {
                            info = null;
                        }

                        if (info == null)
                        {
                            //文件不存在
                            if (firm_info != null && p.md5 == firm_info.md5)
                            {
                                //该文件是固化文件  并且md5是一样的  那么证明 不需要更新
                                file_is_ok = true;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            //文件存在 
                            if (firm_info != null)
                            {
                                //固化文件  需要检查 MD5
                            }
                            else
                            {
                                //非固化文件 那么需要检查版本状态了
                                if ((status == CheckResult.NeedForceUpdate || status == CheckResult.NeedHotUpdate))
                                {
                                    //要检查MD5
                                }
                                else if (info.Length == p.size)
                                {
                                    //ok
                                    file_is_ok = true;
                                    break;
                                }
                            }
                        }
                        //开始检查md5
                        try
                        {
                            //   Debug.LogError(full_path);
                            if (p.md5 == MD5Code.GetMD5HashFromFile(full_path))
                            {
                                //MD5 一样的话 那么证明是OK的
                                file_is_ok = true;
                                break;
                            }
                            else
                            {
                                //md5 不一样
                                if (firm_info != null && info != null)
                                {
                                    //证明是固化文件 和自带更新的不一样 更新完成后 需要重启客户端
                                    WillRebotClient = true;
                                }
                            }
                        }
                        catch (Exception e) { }

                    } while (false);

                }
                catch (Exception e)
                {

                }
                task.RemoveAt(task.Count - 1);
                lock (_thread_tags)
                {
                    if (_thread_tags.ContainsKey(p))
                    {
                        var tags = _thread_tags[p];
                        tags.status = file_is_ok ? FileStatus.FileOK : FileStatus.FileError;
                    }
                }
            }
        }
        bool CheckMD5 = false;
        IEnumerator CheckVersionInfo(bool check_md5 = false)
        {
            FileVersionList list = null;
            bool any_error = false;
            //if (LuaInterface.LuaMgr.ins != null)
            //{
            //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_START");
            //}
            try
            {
                list = UnityEngine.JsonUtility.FromJson<FileVersionList>(File.ReadAllText(VersionListCacheFileName));
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.Log(e.Message);
#endif
                try
                {
                    File.Delete(VersionListCacheFileName);
                }
                catch (Exception ee) { }
            }
            if (list == null)
            {
                UnityWebRequest www = null;

                //需要拿到最高版本的 版本号前缀
                Version current = new Version();
                try
                {
                    if (IsNewerFirm)
                    {
                        current.Parse(__BASE_VERSION__);
                    }
                    else
                    {
                        current.Parse(GetVersion());
                    }
                }
                catch (Exception e) { }

                Version server_v = new Version();
                try
                {
                    server_v.Parse(_current_server_version_);
                }
                catch (Exception e) { }

                string pre = null;
                if (server_v.GetVersionLong() >= current.GetVersionLong())
                {
                    pre = _current_server_version_;
                }
                else
                {
                    if (IsNewerFirm)
                    {
                        pre = __BASE_VERSION__;
                    }
                    else
                    {
                        pre = GetVersion();
                    }
                }
                //if (DevConfig._GamePlatform == GamePlatform.Android)
                //{
                //    www = UnityWebRequest.Get(NetConfig.GateWayWWW_IP + "/version/" + pre + "/FileVersionList.json");
                //}
                //else
                //{
                //    www = UnityWebRequest.Get(NetConfig.GateWayWWW_IP + "/version/" + pre + "/FileVersionList_ios.json");
                //}
                www = UnityWebRequest.Get(/*NetConfig.GateWayWWW_IP + */"/version/" + pre + "/FileVersionList_ios.json");
#if UNITY_EDITOR
                Debug.Log("Missing VersionList Cache try download from web " + www.url);
#endif
                www.timeout = 8;
                yield return www.Send();
                try
                {
                    if (www.isDone && www.isNetworkError == false)
                    {
                        string txt = www.downloadHandler.text;
                        try
                        {
                            list = UnityEngine.JsonUtility.FromJson<FileVersionList>(txt);
                        }
                        catch (Exception e) { }
                        try
                        {
                            File.WriteAllText(VersionListCacheFileName, txt);
                        }
                        catch (Exception e) { }
                    }
                }
                catch (Exception e) { }
            }
            else
            {

            }
            do
            {
                if (list == null)
                {
                    any_error = true;
                    break;
                }

                try
                {
                    //var ls = UnityEngine.JsonUtility.FromJson<FileVersionList>(DevConfig.FSFirmVersionList);
                    //this._fsfirm_files = new System.Collections.Generic.Dictionary<string, FileVersion>();
                    //foreach (var p in ls.files)
                    //{
                    //    if (_fsfirm_files.ContainsKey(p.file))
                    //    {

                    //    }
                    //    else
                    //    {
                    //        _fsfirm_files[p.file] = p;
                    //    }
                    //}
                    //ls = null;
                }
                catch (Exception e)
                {
                    any_error = true;
                    break;
                }

                //校验进度
                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_VERIFYING", 0, list.files.Count);
                //}
                this.CheckMD5 = check_md5;
                List<FileVersion> _invalid_file_list = new List<FileVersion>();//非法的文件 列表 这些文件需要重新下载
                list.files.Sort();
                int thread_num = Mathf.Max(6, Environment.ProcessorCount * 2);//尽可能把磁盘IO撑满
                _thread_tags = new System.Collections.Generic.Dictionary<FileVersion, FileVersionChecker>();
                foreach (var p in list.files)
                {
                    if (!_thread_tags.ContainsKey(p))
                    {
                        _thread_tags.Add(p, new FileVersionChecker());
                    }
                }

                List<List<FileVersion>> _thread_task = new System.Collections.Generic.List<System.Collections.Generic.List<FileVersion>>();
                for (int i = 0; i < thread_num; i++)
                {
                    _thread_task.Add(new System.Collections.Generic.List<FileVersion>());
                }

                int index = 0;
                foreach (var p in list.files)
                {
                    _thread_task[index].Add(p);
                    index = ++index % thread_num;
                }

                //ok order thread task done
                for (int i = 0; i < thread_num; i++)
                {
                    var thread = new Thread(new ParameterizedThreadStart(FileCheckThreadTask));
                    thread.Start(_thread_task[i]);
                }
                yield return new WaitForEndOfFrame();
                int max_round = 0;
                while (true)
                {
                    yield return new WaitForSeconds(0.1f);
                    ++max_round;
                    int total = _thread_tags.Count;
                    int current = 0;
                    lock (_thread_tags)
                    {
                        foreach (var p in _thread_tags)
                        {
                            if (p.Value.status == FileStatus.FileError)
                            {
                                ++current;
                            }
                            else if (p.Value.status == FileStatus.FileOK)
                            {
                                ++current;
                            }
                        }
                    }
                    //任务完成 或者超过了 10 分钟等待时间
                    if (current >= total || max_round > 6000)
                    {
                        break;
                    }
                    else
                    {
                        //校验进度
                        //if (LuaInterface.LuaMgr.ins != null)
                        //{
                        //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_VERIFYING", current + 1, total);
                        //}
                    }
                }
                yield return new WaitForEndOfFrame();
                lock (_thread_tags)
                {
                    foreach (var p in _thread_tags)
                    {
                        if (p.Value.status == FileStatus.FileError)
                        {
                            _invalid_file_list.Add(p.Key);
                        }
                    }
                }

                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_VERIFYING", list.files.Count, list.files.Count);
                //}
                yield return new WaitForEndOfFrame();
                if (_invalid_file_list.Count <= 0)
                {
                    //更新完毕 检查版本存储信息
                    //if (DevConfig.HasEverRunGame)
                    //{
                    //    //只要玩过游戏 那么都要重启客户端 因为网关 那一块的Disable处理简单粗暴 没有区分
                    //    //服务器踢下线 还是版本检查导致的下线 这块细活 后面再说把
                    //    DevConfig.WillRebotClient = true;
                    //}
                    CheckSaveVersionInfo();
                    //ok nothing will be download
                    //if (LuaInterface.LuaMgr.ins != null)
                    //{
                    //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_DONE", 0, 0);
                    //}
                }
                else
                {
                    //there has something will download
                    this.invalid_file_list = _invalid_file_list;
                    this.file_version_list = list;
                    int total = 0;
                    foreach (var p in this.invalid_file_list)
                    {
                        total += p.size;
                    }
                    //if (LuaInterface.LuaMgr.ins != null)
                    //{
                    //    /*  if (DevConfig._GamePlatform == GamePlatform.Android)
                    //      {
                    //          //android 如果是强更的话  要加上APK大小 因为更新完毕后 会进行 自动下载APK
                    //          if (StaticData.ClientVersionHotUpdateStatus == CheckResult.NeedForceUpdate)
                    //          {
                    //              LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_DONE", 1, ToFileSizeString(total + _MAX_VERSION.FileSize));
                    //          }
                    //          else
                    //          {
                    //              LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_DONE", 1, ToFileSizeString(total));
                    //          }
                    //      }
                    //      else*/
                    //    {
                    //        LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_DONE", 1, ToFileSizeString(total));
                    //    }
                    //}
                }
            }
            while (false);
            check_version_info = null;
            _thread_tags = null;
            _fsfirm_files = null;
            if (any_error)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 4f));
                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_FS_DONE", 2);
                //}
            }
            GC.Collect();
        }
        Coroutine co_checkdownload_task = null;
        public bool StartUpdate(string url_pre)
        {
            if (invalid_file_list != null && invalid_file_list.Count > 0 && downTask == null && file_version_list != null
                && co_checkdownload_task == null && string.IsNullOrEmpty(url_pre) == false && _MAX_VERSION != null)
            {
                co_checkdownload_task = StartCoroutine(CheckDownloadTask(url_pre));
                return true;
            }
            return false;
        }
        public static string PatchRootDir
        {
            get
            {
#if UNITY_EDITOR
                return "Patch3";
#else
                return Application.persistentDataPath + "/Patch3";
#endif
            }
        }
        public static string WriteablePath
        {
            get
            {
#if UNITY_EDITOR
                return "";
#else
                return Application.persistentDataPath + "/" ;
#endif
            }
        }
        private IEnumerator CheckDownloadTask(string url_pre)
        {
            //用于进度显示
            int total_byte = 0;
            int total_count = invalid_file_list.Count;
            int download_byte = 0;
            foreach (var p in invalid_file_list)
            {
                total_byte += p.size;
            }
            //copy just for record full set of update list
            List<FileVersion> invalid_file_list_tmp = new System.Collections.Generic.List<FileVersion>(invalid_file_list);

            //需要拿到最高版本的 版本号前缀
            Version current = new Version();
            try
            {
                if (IsNewerFirm)
                {
                    current.Parse(__BASE_VERSION__);
                }
                else
                {
                    current.Parse(GetVersion());
                }
            }
            catch (Exception e) { }

            Version server_v = new Version();
            try
            {
                server_v.Parse(_current_server_version_);
            }
            catch (Exception e) { }

            string pre = null;
            if (server_v.GetVersionLong() >= current.GetVersionLong())
            {
                pre = _current_server_version_;
            }
            else
            {
                if (IsNewerFirm)
                {
                    pre = __BASE_VERSION__;
                }
                else
                {
                    pre = GetVersion();
                }
            }
            //if (DevConfig._GamePlatform == GamePlatform.Android)
            //{
            //    url_pre = url_pre + "/patch_android/" + pre + "/";
            //}
            //else
            //{
            //    url_pre = url_pre + "/patch_ios/" + pre + "/";
            //}

            yield return new WaitForEndOfFrame();
            //if (LuaInterface.LuaMgr.ins != null)
            //{
            //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_DL_START");
            //}
            yield return new WaitForEndOfFrame();
            //删除下载临时文件
            try
            {
                File.Delete(WriteablePath + "PatchStorage/tmp.bytes");
            }
            catch (Exception e) { }
            while (invalid_file_list.Count > 0)
            {
                yield return new WaitForEndOfFrame();
                var task = invalid_file_list[0];
                if (downTask == null)
                {
                    downTask = new WiseDownload(url_pre + task.file, WriteablePath + "PatchStorage/tmp.bytes", "");
                }
                else
                {
                    if (downTask.isDone)
                    {
                        if (downTask.isError == false && task.md5 != null &&
                          task.md5 == MD5Code.GetMD5HashFromFile(WriteablePath + "PatchStorage/tmp.bytes"))
                        {
                            //ok download ok
                            //copy file to des
                            try
                            {
                                string f = PatchRootDir + "/" + task.file;
                                int found = f.LastIndexOf("/");
                                if (found != f.Length)
                                {
                                    string directoryPath = f.Substring(0, found);
                                    Directory.CreateDirectory(directoryPath);
                                }
                                if (File.Exists(f))
                                {
                                    File.Delete(f);
                                }
                                File.Move(WriteablePath + "PatchStorage/tmp.bytes", f);
                                invalid_file_list.RemoveAt(0);
                                download_byte += task.size;
                                //if (LuaInterface.LuaMgr.ins != null)
                                //{
                                //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_DL_RUNNING", total_count - invalid_file_list.Count, total_count, ToFileSizeString(download_byte), ToFileSizeString(total_byte), download_byte, total_byte);
                                //}
                                downTask = null;
                                continue;
                            }
                            catch (Exception e)
                            {
#if UNITY_EDITOR
                                Debug.LogError(e.Message);
#endif
                                //error
                                break;
                            }
                        }
                        else
                        {
#if UNITY_EDITOR
                            Debug.LogError(" download md5 error" + task.md5 + "  " + task.file);
#endif
                            //error download error
                            break;
                        }
                    }
                    else
                    {
                        //downloading
                        //如果有变化 也要显示出啦
                        int download = download_byte + (int)downTask.downloadByte;
                        if (download != download_byte)
                        {
                            //if (LuaInterface.LuaMgr.ins != null)
                            //{
                            //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_DL_RUNNING", total_count - invalid_file_list.Count, total_count, ToFileSizeString(download), ToFileSizeString(total_byte), download, total_byte);
                            //}
                        }
                    }
                }
            }
            GC.Collect();
            yield return new WaitForEndOfFrame();
            if (invalid_file_list.Count > 0)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
                //download error
                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_DL_DONE", 0, LastErrorStr);
                //}
            }
            else
            {
                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_DL_RUNNING", total_count, total_count, ToFileSizeString(total_byte), ToFileSizeString(total_byte));
                //}
                //更新完毕 检查版本存储信息
                CheckSaveVersionInfo();
                yield return new WaitForEndOfFrame();

                //只要有过更新 并且玩过游戏 那么就必须重启客户端
                //if (DevConfig.HasEverRunGame && invalid_file_list_tmp.Count > 0)
                //{
                //    DevConfig.WillRebotClient = true;
                //}
                //else
                //{
                //    foreach (var p in invalid_file_list_tmp)
                //    {
                //        //强制检查MD5的的文件的话 那么证明是核心文件(包括lua)更新了 需要重启客户单
                //        if (ForceCheckMD5(p.file))
                //        {
                //            DevConfig.WillRebotClient = true;
                //            break;
                //        }
                //    }
                //}
                //if (this.WillRebotClient)
                //{
                //    DevConfig.WillRebotClient = true;
                //}
                ////ok all is ok
                //if (LuaInterface.LuaMgr.ins != null)
                //{
                //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "PATCHER_DL_DONE", 2);
                //}
                invalid_file_list = null;
                file_version_list = null;
            }
            downTask = null;
            co_checkdownload_task = null;
        }

        WiseDownload downTask = null;
        List<FileVersion> invalid_file_list = null;
        FileVersionList file_version_list = null;

        #endregion


        #region 内置下载器下载APK
        //调用该接口 开始 用内置的下载器 下载
        public void StartInstallerDownload()
        {
            if (_MAX_VERSION == null)
            {
                return;
            }
            if (_install == null)
            {
                _install = new InstallerDownloader();
            }
            else
            {
                _install.Terminal();
                _install = null;
                _install = new InstallerDownloader();
            }
            _install.StartDownload(_MAX_VERSION.Url, InstallerDownloader.InstallRootDir, InstallerDownloader.ApkName, _MAX_VERSION.MD5, true);
        }
        InstallerDownloader _install = null;

        void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                if (InstallerDownloader._Status == InstallerDownloader.Status.OK)
                {
                    //安装apk的时候 回到了前台 证明 安装失败 直接退出游戏
                    //if (LuaInterface.LuaMgr.ins != null)
                    //{
                    //    //     LuaInterface.LuaMgr.ins.CallGlobalFunction("PATCH2_INSTALL_HAS_BEEN_ABORT");
                    //    LuaInterface.LuaMgr.ins.CallGlobalEvent("Patcher", "InstallerHasBeenAbort");
                    //}
                    //else
                    {
                        Application.Quit();
                    }
                }
            }
        }
        #endregion
    }
}


