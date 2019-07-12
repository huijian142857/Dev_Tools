using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using UnityEngine;
using System.IO;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Specialized;
using System.Reflection;

//just android suport  IOS直接跳转app-store 即可
//用于游戏内置的下载器 下载
public class MAX_VERSION
{
    public string Version = "";
    public string MD5 = "";
    public int FileSize = 5000;//这个file size 只是给玩家看的 实际下载 不用到该变量
    public string Url = "";
    public string RollBackUrl = "";//下载回滚页面 比如下载fatal error 会直接opel url 
    public string FileVersionListMD5 = "";//版本文件MD5 用于缓存和校验使用 如果过期 那么MD5将会不一样 用于处理缓存

    //this data is legal or illegal 
    public bool IsLegal()
    {
        try
        {
            if (Version != null && MD5 != null && FileSize > 0 && Url != null)
            {
                if (Version.Length >= 5 && MD5.Length > 0 && Url.Length > 5)
                {
                    return true;
                }
            }
        }
        catch (Exception e) { }
        return false;
    }
}


//安装包下载器 支持断点续传 支持游戏内置 下载  下载完成后 调用native api进行安装
//其实只需要android 平台即可 IOS直接跳转 app-store
public class InstallerDownloader
{
    //下载的 apk 存放名字
    public const string ApkName = "hcr.apk";
    //调用该函数 开始安装  只能在 unity主线程调用
    public static bool StartInstallApk(string full_file_path)
    {
#if UNITY_ANDROID
        return NativeApi.InstallApk(full_file_path);
#endif
        return true;
    }
    public enum Status
    {
        None = 0,//未知
        Error, // 下载错误  或者超时 
        Downloading, // 下载中  外部可以读取静态变量 来显示下载进度
        Checking,// 处理 下载前逻辑中
        Verifying, // 校验文件中 可能是下载前 可能是下载后
        OK,//下载完成 可以开始执行安装操作了
    }
    public class DownloadParam
    {
        public string url;
        public string path_to_save;
        public string file_name;
        public bool enable_break_point;//是否开启断点续传功能
        public string md5;//md5 用于校验安装包的完整性
        public bool IsRedirect = false;//是否是重定向 是重定向的话 会忽略 失败次数统计
    }
    Thread t_download = null;
    public void Terminal()
    {
        if (t_download != null)
        {
            try
            {
                t_download.Abort();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            t_download = null;
        }
        CurrentSize = 0;
        TotalSize = 0;
        _Status = Status.None;
    }
    // 需要下载的大小  内部减去了 断点续传的 部分
    public static int GetLeftSize(string path_to_save, string file_name, int total_size)
    {
        string full_path = path_to_save + "/" + file_name;
        if (File.Exists(full_path))
        {
            var info = new FileInfo(full_path);
            if (info != null)
            {
                //json 传入的大小不一定可靠 因此 容差处理
                return Mathf.Clamp(total_size - (int)info.Length, 0, int.MaxValue);
            }
        }
        return total_size;
    }
    //改变量只能在 unity主线程调用
    public static string InstallRootDir
    {
        get
        {
#if !UNITY_EDITOR
                return Application.persistentDataPath + "/Installer";
#else
            return "Installer";
#endif
        }
    }
    public void StartDownload(string url, string path_to_save, string file_name, string md5, bool enable_break_point = true)
    {
        if (t_download != null)
        {
            try
            {
                t_download.Abort();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
        t_download = new Thread(new ParameterizedThreadStart(ThreadFunc));
        IsThreadRunning = true;
        DownloadParam _param = new DownloadParam
        {
            url = url,
            path_to_save = path_to_save,
            file_name = file_name,
            enable_break_point = enable_break_point,
            md5 = md5
        };
        try_times = 0;
        _Status = Status.Checking;
        t_download.Start(_param);
    }
    bool IsThreadRunning = false;

    public static Status _Status = Status.None;
    public int HttpRetCode = 0;

    public static int TotalSize = 0; // 总大小
    public static int CurrentSize = 0; // 当前大小 可用于进度显示

    int try_times = 0;


    private void DownloadOK()
    {
        _Status = Status.OK;

        //开始安装流程  调用native api 开始安装
        Debug.LogWarning("download ok");

    }

    private void ThreadFunc(object _param_call)
    {
        CurrentSize = 0;
        TotalSize = 0;
        DownloadParam _param = _param_call as DownloadParam;
        if (_param == null)
        {
            IsThreadRunning = false;
            _Status = Status.Error;
            return;
        }
        Debug.LogWarning("start to download " + _param.url);
        if (_param.IsRedirect)
        {
            //重定向的话 不处理 失败 重试次数
            _param.IsRedirect = false;
        }
        else
        {
            //重新下载 或者 分批下载 都会重试计次
            ++try_times;
        }
        if (try_times > 10)
        {
            //fatal error or net error
            _Status = Status.Error;
            return;
        }
        try
        {
            _Status = Status.Verifying;
            if (File.Exists(_param.path_to_save + "/" + _param.file_name))
            {
                //文件存在的话 检查一下 是否成功  不成功的话 才开始下载
                _Status = Status.Verifying;
                if (string.IsNullOrEmpty(_param.md5) == false && _param.md5 == MD5Code.GetMD5HashFromFile(_param.path_to_save + "/" + _param.file_name))
                {
                    //ok
                    _Status = Status.OK;
                    this.DownloadOK();
                    return;
                }
                else
                {
                    //md5 verify error 需要处理下载 (or断点下载) 下载完成后 才 再次校验
                }
            }
        }
        catch (Exception e)
        {

        }
        _Status = Status.Checking;

        if (_param.enable_break_point == false)
        {
            //无需断点下载 尝试暴力删除文件
            try
            {
                Directory.Delete(_param.path_to_save, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
        if (Directory.Exists(_param.path_to_save) == false)
        {
            try
            {
                Directory.CreateDirectory(_param.path_to_save);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
        //先打开文件
        Stream file = null;
        using (file = (File.Exists(_param.path_to_save + "/" + _param.file_name)) ? File.OpenWrite(_param.path_to_save + "/" + _param.file_name) : file = File.Create(_param.path_to_save + "/" + _param.file_name))
        {
            /*  try
              {
                  if (File.Exists(_param.path_to_save + "/" + _param.file_name))
                  {
                      file = File.OpenWrite(_param.path_to_save + "/" + _param.file_name);
                  }
                  else
                  {
                      file = File.Create(_param.path_to_save + "/" + _param.file_name);
                  }
              }
              catch (Exception e)
              {
                  Debug.LogWarning(e.Message);
              }*/
            try
            {
                long current_size = file.Length;
                if (current_size > 0)
                {
                    file.Seek(current_size, SeekOrigin.Begin);
                }
                HttpWebRequest request = null;

                //如果是发送HTTPS请求  
                if (_param.url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    request = (HttpWebRequest)WebRequest.Create(_param.url);
                }
                else
                {
                    request = (HttpWebRequest)WebRequest.Create(_param.url);
                }
                request.ProtocolVersion = new System.Version(1, 1);
                if (current_size > 0)
                {
                    request.AddRange((int)current_size);
                    CurrentSize = (int)current_size;
                }
                HttpWebResponse response = null;
                request.Timeout = 10000;
                request.ReadWriteTimeout = 10000;
                request.Method = "GET";
                request.KeepAlive = false;
                response = (HttpWebResponse)request.GetResponse();

                var HttpRetCode = response.StatusCode;
                Debug.Log("InstallDownloader http " + HttpRetCode);


                if (HttpRetCode == HttpStatusCode.Redirect)
                {
                    //重定向
                    _param.url = response.Headers["Location"].Trim();
                    response.Close();
                    response = null;
                    request.Abort();
                    request = null;
                    try
                    {
                        file.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message);
                    }
                    Debug.Log("Redirect " + _param.url);
                    _param.IsRedirect = true;
                    ThreadFunc(_param);
                    return;

                }
                else if (HttpRetCode == HttpStatusCode.GatewayTimeout || HttpRetCode == HttpStatusCode.RequestTimeout)
                {
                    //net error
                    response.Close();
                    response = null;
                    request.Abort();
                    request = null;
                    try
                    {
                        file.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message);
                    }
                    Debug.Log("timeout");

                    return;

                }
                else if (HttpRetCode == HttpStatusCode.OK || HttpRetCode == HttpStatusCode.Created || HttpRetCode == HttpStatusCode.Accepted || HttpRetCode == HttpStatusCode.NonAuthoritativeInformation || HttpRetCode == HttpStatusCode.NoContent || HttpRetCode == HttpStatusCode.ResetContent || HttpRetCode == HttpStatusCode.PartialContent)
                {
                    if (HttpRetCode != HttpStatusCode.PartialContent)
                    {
                        //如果不是断点下载 或者服务器不支持 那么需要 重新下载完整文件
                        try
                        {
                            file.Close();
                            file = null;
                        }
                        catch (Exception e)
                        {
                        }
                        try
                        {
                            Directory.Delete(_param.path_to_save, true);
                        }
                        catch (Exception e)
                        {
                        }
                        try
                        {
                            Directory.CreateDirectory(_param.path_to_save);
                        }
                        catch (Exception e)
                        {
                        }
                        file = File.Create(_param.path_to_save + "/" + _param.file_name);
                    }
                }
                else
                {
                    //req error
                    response.Close();
                    response = null;
                    request.Abort();
                    request = null;
                    try
                    {
                        file.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message);
                    }
                    try
                    {
                        File.Delete(_param.path_to_save + "/" + _param.file_name);
                    }
                    catch (Exception e) { }
                    Debug.LogWarning("error");
                    return;
                }

                //web 请求处理完成了 开始处理 接受数据了
                long total_len = response.ContentLength;

                TotalSize = (int)total_len + (int)current_size;
                if (current_size < TotalSize)
                {
                    if (current_size > 0)
                    {
                        //   request.AddRange((int)current_size);
                        CurrentSize = (int)current_size;
                    }
                    Stream web = request.GetResponse().GetResponseStream();
                    byte[] _cache = new byte[10240]; // 10kb
                    int down_size = 0;
                    int read_size = web.Read(_cache, 0, 10240);
                    int total_read_size = 0;
                    _Status = Status.Downloading;
                    while (read_size > 0)
                    {
                        _Status = Status.Downloading;
                        file.Write(_cache, 0, read_size);
                        total_read_size += read_size;

                        down_size += read_size;
                        CurrentSize += read_size;
                        //    Debug.LogError("download ing " + CurrentSize + "         " + TotalSize);
                        file.Flush();
                        read_size = web.Read(_cache, 0, 10240);
                    }
                    file.Close();
                    file = null;
                    web.Close();
                    web = null;
                    response.Close();
                    response = null;
                    request.Abort();
                    request = null;


                    if (current_size + down_size < TotalSize)
                    {
                        //下载文件 长度不够 需要重新下载
                        Debug.LogWarning("file is smaller will re-try");
                        ThreadFunc(_param);
                        return;
                    }
                    else if (current_size + down_size > TotalSize)
                    {
                        //下载的长度 超过了 实际长度 文件已经损坏 重新下载把
                        try
                        {
                            Directory.Delete(_param.path_to_save, true);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e.Message);
                        }
                        Debug.LogWarning("file is bigger will delete and re-download");

                        ThreadFunc(_param);
                        return;
                    }
                    else
                    {
                        //下载文件成功 开始校验MD5
                        _Status = Status.Verifying;
                        string download_md5 = MD5Code.GetMD5HashFromFile(_param.path_to_save + "/" + _param.file_name);
                        if (string.IsNullOrEmpty(_param.md5) == false && _param.md5 == download_md5)
                        {
                            //ok
                        }
                        else
                        {
                            if (_param.md5 != null)
                            {
                                Debug.LogWarning("excepted md5=" + _param.md5 + "  file=" + download_md5);
                            }
                            //md5 verify error 尝试重新下载
                            try
                            {
                                file.Close();
                                file = null;
                                response.Close();
                                response = null;
                                request.Abort();
                                request = null;
                            }
                            catch (Exception e)
                            {
                            }
                            try
                            {
                                Directory.Delete(_param.path_to_save, true);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning(e.Message);
                            }
                            ThreadFunc(_param);
                            return;
                        }
                        _Status = Status.OK;
                    }
                }
                else if (current_size == total_len)
                {//当前文件和 服务器文件大小一样 默认为 下载完成 需要校验MD5

                    try
                    {
                        file.Close();
                        file = null;
                        response.Close();
                        response = null;
                        request.Abort();
                        request = null;
                    }
                    catch (Exception e)
                    {
                    }
                    Debug.LogWarning("file is  req just done");
                    _Status = Status.Verifying;

                    var download_md5 = MD5Code.GetMD5HashFromFile(_param.path_to_save + "/" + _param.file_name);
                    if (string.IsNullOrEmpty(_param.md5) == false && _param.md5 == download_md5)
                    {
                        //ok
                    }
                    else
                    {
                        if (_param.md5 != null)
                        {
                            Debug.LogWarning("1excepted md5=" + _param.md5 + "  file=" + download_md5);
                        }
                        //md5 verify error 尝试重新下载
                        try
                        {
                            file.Close();
                            file = null;
                            response.Close();
                            response = null;
                            request.Abort();
                            request = null;
                        }
                        catch (Exception e)
                        {
                        }
                        try
                        {
                            Directory.Delete(_param.path_to_save, true);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e.Message);
                        }
                        ThreadFunc(_param);
                        return;
                    }
                    _Status = Status.OK;
                }
                else
                {
                    //当前文件超过了 大小 需要重新下载
                    try
                    {
                        Directory.Delete(_param.path_to_save, true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message);
                    }
                    Debug.LogWarning("file is bigger will delete and re-download  2");

                    try
                    {
                        file.Close();
                        file = null;
                        response.Close();
                        response = null;
                        request.Abort();
                        request = null;
                    }
                    catch (Exception e)
                    {
                    }

                    ThreadFunc(_param);
                    return;
                }
                //走到了这里 都当作文件下载成功 并且校验成功 可以开始安装了
                _Status = Status.OK;
                this.DownloadOK();
            }
            catch (Exception ee)
            {
                //整个下载流程出了异常错误 
                Debug.LogWarning(ee.Message);
                _Status = Status.Checking;
                try
                {
                    if (file != null)
                    {
                        file.Close();
                        file = null;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
                try
                {
                    File.Delete(_param.path_to_save + "/" + _param.file_name);
                }
                catch (Exception e) { }
                ThreadFunc(_param);
                return;
            }
        }
    }
    private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        return true;
    }
}