/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com
 */
// 打包步骤：
//  1.生成DevConfig_Auto_GEN.cs 文件 配置下载源和服务器 
//  1.解压广告SDK数据
//  1.生成apk
//

using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EditorBuild
{
    [Serializable]
    public class BuildAndriodTask
    {
        public string task_name = "官方";//只是看的
        public string package_name = "com.xxx.xxx";
        public int devconfig_download_sources = 0;
        //包含的广告SDK类型 
        public List<int> ad_sdk_type = new System.Collections.Generic.List<int>();
        public int process_NGAD_type = 0;//处理UD广告文件 类型 0为不处理 1为处理_1.ini文件 11为处理_11.ini文件 只有SDKtype包含了九游广告(AD type = 0)才会有效
        public string getui_app_id = "hwVrujpCloAVhiOEd4M0I3";
        public string getui_app_key = "a5dLjwYvrZ6ltzAOYbM9V9";
        public string getui_app_secret = "0p6j4apqbXAecUO9JTJVI9";
        public string gisight_app_id = "LH8vVVA1c76OQf8ReaCd58";
        public string gisight_channel = "0";
        public string applog_channel = "";
        public string applog_appId = "";
        public string applog_configValue = "default";
    }
    [Serializable]
    public class BuildAndriodCommon
    {
        public string __Version__ = "1.18.0";//游戏版本
        public string __ENGINE_VERSION__ = "1.18.0"; // C#引擎版本

        public string version = "1.18"; // android apk版本
        public string output_name_pre = "hcr"; // 生成apk名字前缀 后面是 "1.18.x.apk"
        public string devconfig_server_platform = "";//连接的服务器环境
        public int bundle_version_code = 1;
        public int minSdkVersion = 17;//最小android-SDK version

    }
    [Serializable]
    public class BuildAndriodTasks
    {
        public BuildAndriodCommon common = new BuildAndriodCommon();
        public List<BuildAndriodTask> tasks = new System.Collections.Generic.List<BuildAndriodTask>();
        public List<BuildAndriodTask> tasks_full = new System.Collections.Generic.List<BuildAndriodTask>();
    }

    public class BuildSettings
    {
        public static string[] ad_sdk_delete_dir =
        {
            "Assets/Plugins/Z_NGAD",
            "Assets/Plugins/Z_TTAD",
            "Assets/Plugins/Z_MTGAD",
            "Assets/Plugins/Z_JiuJiuAD",
        };

        public static string[] ad_sdk_id_2_zip =
        {
            "0Z_NGAD.zip", // 0 九游广告
            "1Z_TTAD.zip", // 今日头条广告
			"2Z_MTGAD.zip", // MTG
			"3Z_JiuJiuAD.zip", // 4399
		};
    }



    public static class RuntimeMaker
    {
        const string ROOT_DIR = "RuntimeBuild";

        const string APK_DIR_ROOT_MONO = "RuntimeBuild/Apk_Mono";
        const string APK_DIR_ROOT_IL2CPP = "RuntimeBuild/Apk_IL2CPP";
        //xxtea key
        public static byte[] ABC = System.Text.Encoding.ASCII.GetBytes("ewthqegwegqgbvGvappFSqw3pbs");

        public static string[] dll_list =
        {
            "assets/bin/Data/Managed/Assembly-CSharp-firstpass.dll",
            "assets/bin/Data/Managed/Assembly-CSharp.dll",
            "assets/bin/Data/Managed/GameEngine.dll",
         //   "assets/bin/Data/Managed/___03GameLogic.dll",

        };
        public static string[] engne_list =
        {
            "AssetBundles/engine/game0",
            "AssetBundles/engine/game1",
            "AssetBundles/engine/game2",    
          //  "AssetBundles/engine/game3",       
        };
#if !ENABLE_POSTPRROCESS
        [UnityEditor.Callbacks.PostProcessBuildAttribute(100)]
#endif
        public static void PostProcessBuild(BuildTarget target, string path)
        {
            ScriptingImplementation scriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            string apkRoot = scriptingBackend == ScriptingImplementation.IL2CPP ? APK_DIR_ROOT_IL2CPP : APK_DIR_ROOT_MONO;
            if (target == BuildTarget.Android)
            {

                string file_name = Path.GetFileNameWithoutExtension(path);


                if (scriptingBackend == ScriptingImplementation.IL2CPP)
                {
                    //copy IL2CPP symbol文件到目标目录
                    if (!Directory.Exists("IL2CPP_Symbols"))
                    {
                        Directory.CreateDirectory("IL2CPP_Symbols");
                    }

                    if (!Directory.Exists("IL2CPP_Symbols"))
                    {
                        Directory.CreateDirectory("IL2CPP_Symbols");
                    }

                    if (!Directory.Exists("IL2CPP_Symbols/" + file_name))
                    {
                        Directory.CreateDirectory("IL2CPP_Symbols/" + file_name);
                    }

                    File.Copy("Temp/StagingArea/symbols/armeabi-v7a/libil2cpp.so.debug", "IL2CPP_Symbols/" + file_name + "/libil2cpp.so.debug.so", true);
                    File.Copy("Temp/StagingArea/symbols/armeabi-v7a/libil2cpp.sym", "IL2CPP_Symbols/" + file_name + "/libil2cpp.sym.so", true);
                }
                if (Directory.Exists(file_name))
                {
                    try
                    {
                        Directory.Delete(file_name, true);
                    }
                    catch (Exception e) { }
                }
                try
                {
                    File.Delete(apkRoot + "/" + file_name + "_unsigned.apk");
                }
                catch (Exception e) { }

                //setp depress
                {
                    System.Diagnostics.Process proc = System.Diagnostics.Process.Start(Application.dataPath + "/../RuntimeBuild/tools/apktool.bat", "d " + apkRoot + "/" + file_name + ".apk");
                    proc.WaitForExit();
                    File.Delete(path);
                }

                // step rename package name
                {
                    string packageName = Application.identifier;
                    string androidManifest = File.ReadAllText(file_name + "/AndroidManifest.xml");
                    // 把其他渠道的包名都改成官网包包名
                    androidManifest = androidManifest.Replace(packageName, "com.example.gcloudu3ddemo");
                    //
                    androidManifest = androidManifest.Replace("com.example.gcloudu3ddemo", packageName);

                    string[] strs = file_name.Split('.');
                    int downloadChannel;
                    if (strs == null || strs.Length == 0 || !int.TryParse(strs[strs.Length - 1], out downloadChannel)) {
                        Debug.LogError("APK生成失败，APK输出名字格式错误(后缀应该是下载渠道).");
                    }
                    else {
                        BuildAndriodTasks tasks = JsonUtility.FromJson<BuildAndriodTasks>(File.ReadAllText(Application.dataPath + "/../RuntimeBuild/build_android_task.json"));
                        for (int i = 0; i < tasks.tasks_full.Count; i++) {
                            var task = tasks.tasks_full[i];
                            if (task.devconfig_download_sources == downloadChannel && task.package_name == packageName) {
                                // 个推 参数配置
                                androidManifest = androidManifest.Replace("GETUI_APP_ID_VALUE", task.getui_app_id);
                                androidManifest = androidManifest.Replace("GETUI_APP_KEY_VALUE", task.getui_app_key);
                                androidManifest = androidManifest.Replace("GETUI_APP_SECRET_VALUE", task.getui_app_secret);
                                androidManifest = androidManifest.Replace("GInsight_APP_ID_VALUE", task.gisight_app_id);
                                androidManifest = androidManifest.Replace("GT_INSTALL_CHANNEL_VALUE", task.gisight_channel);

                                // APPLOG 参数配置
                                // channel
                                androidManifest = androidManifest.Replace("official", task.applog_channel);
                                // appId
                                androidManifest = androidManifest.Replace("164262", task.applog_appId);
                                // applog_config_value
                                androidManifest = androidManifest.Replace("TTAL_15", "TTAL_" + task.applog_configValue);
                                break;
                            }
                        }
                    }

                    // 分享参数配置
                    if (packageName == "com.example.gcloudu3ddemo.aligames")
                    {
                        string mars_share_sdk = File.ReadAllText(file_name + "/assets/mars_share_sdk.xml");
                        // 微信分享
                        mars_share_sdk = mars_share_sdk.Replace("wx8cc3dd18515604d6", "wx3b5d79381953b954");
                        mars_share_sdk = mars_share_sdk.Replace("66782839cb8e3569a461e20964b53338", "8dbef4da7f345eaf3f51e8f46eb13847");
                        File.WriteAllText(file_name + "/assets/mars_share_sdk.xml", mars_share_sdk);
                    }

                    File.WriteAllText(file_name + "/AndroidManifest.xml", androidManifest);
                }

                {
                    //step rewrite yml
                    string yml = "";
                    var x = File.ReadAllLines(file_name + "/apktool.yml");
                    foreach (var p in x)
                    {
                        if (p.StartsWith("- assets/AssetBundle"))
                        {

                        }
                        else
                        {
                            yml += (p + "\n");
                        }
                    }
                    File.WriteAllText(file_name + "/apktool.yml", yml);
                }

                //setp  encode
                //foreach (var pp in dll_list)
                //{
                //    string p = file_name + "/" + pp;
                //    var data = File.ReadAllBytes(p);
                //    File.Delete(p);
                //    File.WriteAllBytes(p, Xxtea.XXTEA.Encrypt(data, ABC));
                //}

                //if (GetLastIsBuildEngineStandlone())
                //{
                //    if (!Directory.Exists("AssetBundles"))
                //    {
                //        Directory.CreateDirectory("AssetBundles");
                //    }
                //    if (Directory.Exists("AssetBundles/engine"))
                //    {
                //        Directory.Delete("AssetBundles/engine", true);
                //    }
                //    Directory.CreateDirectory("AssetBundles/engine");

                //    //last is build engine
                //    for (int i = 0; i < dll_list.Length; i++)
                //    {
                //        try
                //        {
                //            File.Delete(engne_list[i]);
                //        }
                //        catch (Exception e)
                //        {

                //        }
                //        File.Copy(file_name + "/" + dll_list[i], engne_list[i]);
                //    }
                //    SetLastIsBuildEngineStandlone(false);
                //    try
                //    {
                //        Directory.Delete(file_name, true);
                //    }
                //    catch (Exception e) { }
                //    return;
                //}

                string unsign_apk_path = apkRoot + "/" + file_name + "_unsigned.apk";
                //setp build apk
                {
                    System.Diagnostics.Process proc = System.Diagnostics.Process.Start(Application.dataPath + "/../RuntimeBuild/tools/apktool.bat", "b " + file_name + " -o " + unsign_apk_path);
                    proc.WaitForExit();
                    try
                    {
                        Directory.Delete(file_name, true);
                    }
                    catch (Exception e) { }
                }

                // sign & zliap
                {
                    try
                    {
                        string sign_apk_path = apkRoot + "/" + file_name + ".apk";
                        if (File.Exists(unsign_apk_path))
                        {
                            Process proc = Process.Start(Application.dataPath + "/../RuntimeBuild/tools/jarsigner.bat", unsign_apk_path + " " + sign_apk_path);
                            proc.WaitForExit();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.StackTrace);
                    }
                }

                // del unsigned
                {
                    if (File.Exists(unsign_apk_path))
                    {
                        File.Delete(unsign_apk_path);
                    }
                }
            }
        }

        private static bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting = true)
        {
            bool ret = false;
            try
            {
                if (!SourcePath.EndsWith("/")) SourcePath = SourcePath + "/";
                if (!DestinationPath.EndsWith("/")) DestinationPath = DestinationPath + "/";
                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(DestinationPath) == false)
                        Directory.CreateDirectory(DestinationPath);

                    foreach (string fls in Directory.GetFiles(SourcePath))
                    {
                        FileInfo flinfo = new FileInfo(fls);
                        Debug.Log("copy file" + flinfo.Name);
                        flinfo.CopyTo(DestinationPath + flinfo.Name, overwriteexisting);
                    }
                    foreach (string drs in Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo drinfo = new DirectoryInfo(drs);
                        if (CopyDirectory(drs, DestinationPath + drinfo.Name, overwriteexisting) == false)
                            return false;
                    }
                    ret = true;
                }
                else
                {
                    Debug.LogError("dir not exist:" + SourcePath);
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                ret = false;
            }
            return ret;
        }

        static void Process_DevCinfig_Auto_GEN(int ddownload, string server, string version, string engine_version)
        {
            //DownloadSources ss = (DownloadSources)(ddownload);
            string script = "public partial class DevConfig\n{\n";

            Patches.Version v = new Patches.Version();
            v.Parse(engine_version);

            script += "    public static int __ENGINE_VERSION__ = " + (v.MainVersion * 100000 + v.SubVersion) + ";\n}";

            File.WriteAllText("Assets/DevConfig_Auto_GEN.cs", script);

            File.WriteAllText("Assets/Resources/__DOWNLOAD_SOURCES__.txt", ddownload.ToString());
            File.WriteAllText("Assets/Resources/__SERVER_TYPE__.txt", ((int)((ServerPlatform)Enum.Parse(typeof(ServerPlatform), server))).ToString());
            File.WriteAllText("Assets/Resources/__VERSION__.txt", version);

            AssetDatabase.Refresh();
        }
        class ADSdkType
        {
            public List<int> ad_sdk_type = new System.Collections.Generic.List<int>();
        }
        static void Process_AD_SDK(BuildAndriodTask task)
        {
            foreach (var p in BuildSettings.ad_sdk_delete_dir)
            {
                try
                {
                    Directory.Delete(p, true);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
            try
            {
                File.Delete("Assets/Plugins/Android/assets/UCGameConfig.ini");
            }
            catch (Exception e)
            {

            }
            foreach (var sdk_id in task.ad_sdk_type)
            {
                if (sdk_id == 0 && task.process_NGAD_type != 0)
                {
                    //process_NGAD_type==0表示 无需ini文件
                    //type ==0 表示是九游的广告 需要特殊配置文件
                    File.Copy("PlatformFiles/Android/UCGameConfig_" + task.process_NGAD_type + ".ini", "Assets/Plugins/Android/assets/UCGameConfig.ini", true);
                }
            }
            ADSdkType type = new ADSdkType();
            type.ad_sdk_type = task.ad_sdk_type;

            var str = JsonUtility.ToJson(type);
            try
            {
                File.Delete("Assets/Resources/ad_sdk_type.txt");
            }
            catch (Exception e)
            {

            }
            if (str.Length < 5)
            {
                throw new NullReferenceException("json error");
            }
            File.WriteAllText("Assets/Resources/ad_sdk_type.txt", str);

            //remake 
            foreach (var id in task.ad_sdk_type)
            {
                string path = "Assets/Plugins/";
                string file = BuildSettings.ad_sdk_id_2_zip[id];  // "TTAD.zip";
                try
                {
                    //没有存储目录应创建
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string full_name = path + "/" + file;

                    using (var zipFileStream = File.OpenRead("Assets/AdZipFile/" + file))
                    {
                        using (ZipInputStream unzipStream = new ZipInputStream(zipFileStream))
                        {
                            ZipEntry theEntry;
                            while ((theEntry = unzipStream.GetNextEntry()) != null)
                            {
                                string fullName = path + theEntry.Name;
                                string dirName = Path.GetDirectoryName(fullName);
                                // throw new NullReferenceException();
                                if (!Directory.Exists(dirName))
                                {
                                    Directory.CreateDirectory(dirName);
                                }
                                if (theEntry.IsFile)
                                {
                                    try
                                    {
                                        FileStream fs = File.Create(fullName);
                                        byte[] content = new byte[theEntry.Size];
                                        int readlen = unzipStream.Read(content, 0, content.Length);
                                        fs.Write(content, 0, readlen);
                                        fs.Flush();

                                        fs.Close();
                                        fs.Dispose();
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError("unzip read errorMsg==={0}" + e.Message);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("unzip errorMsg==={0}" + e.Message);
                }
            }
        }

        static void Process_PUSH_SDK(BuildAndriodTask task)
        {
            string fileName = null;
            switch (task.devconfig_download_sources)
            {
                case 5:
                    fileName = "HGTPush_Support_HW.zip";
                    break;
                case 6:
                    fileName = "HGTPush_Support_XM.zip";
                    break;
                case 11:
                    fileName = "HGTPush_Support_OPPO.zip";
                    break;
                case 13:
                    fileName = "HGTPush_Support_MZ.zip";
                    break;
            }


            string root = Application.dataPath + "/Plugins/Push";
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }

            string ori_path = Application.dataPath + "/PushExtention";
            if (!string.IsNullOrEmpty(fileName))
            {
                Directory.CreateDirectory(root);

                if (File.Exists(ori_path + "/" + fileName))
                {
                    FileStream zipFileStream = File.OpenRead(ori_path + "/" + fileName);
                    ZipInputStream unzipStream = new ZipInputStream(zipFileStream);
                    ZipEntry theEntry;
                    while ((theEntry = unzipStream.GetNextEntry()) != null)
                    {
                        string fullName = root + "/" + theEntry.Name;
                        string dirName = Path.GetDirectoryName(fullName);
                        if (!Directory.Exists(dirName))
                        {
                            Directory.CreateDirectory(dirName);
                        }

                        if (theEntry.IsFile)
                        {
                            try
                            {
                                FileStream fs = File.Create(fullName);
                                byte[] content = new byte[theEntry.Size];
                                int readlen = unzipStream.Read(content, 0, content.Length);
                                fs.Write(content, 0, readlen);
                                fs.Flush();

                                fs.Close();
                                fs.Dispose();
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("unzip read errorMsg==={0}" + e.Message);
                            }
                        }
                    }
                    unzipStream.Close();
                    zipFileStream.Close();
                }
            }

            AssetDatabase.Refresh();
        }

        static void Process_LebianClientChId(BuildAndriodTask task)
        {
            try
            {
                string text = File.ReadAllText(Application.dataPath + "/Plugins/Android/lebian/lebian_app.gradle");
                int firstIndex = text.IndexOf('[');
                int lastIndex = text.LastIndexOf(']');
                if (firstIndex != -1 && lastIndex != -1)
                {
                    string match = text.Substring(firstIndex, (lastIndex - firstIndex) + 1);
                    string replace;
#if LEBIAN_TENCENT
                    if (task.devconfig_download_sources == 2 || task.devconfig_download_sources == 1)
                    {
                        replace = "[\"ClientChId\": \"" + "tencent" + "\"]";
                    }
#else
                    if (false) { }
#endif
                    else if (task.devconfig_download_sources == 0)
                    {
                        replace = "[\"ClientChId\": \"" + "Official" + "\"]";
                    }
                    else
                    {
                        replace = "[\"ClientChId\": \"" + task.devconfig_download_sources.ToString() + "\"]";
                    }
                    text = text.Replace(match, replace);
                    File.WriteAllText(Application.dataPath + "/Plugins/Android/lebian/lebian_app.gradle", text);
                }
                else
                {
                    Debug.LogError("Process_LebianClientChId error");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        [MenuItem("打包工具/ **************使用方法和注意事项请看RuntimeBuild目录下的readme.txt*********", false, 0)]
        static void ________________________()
        {
        }

        public static void BuildApk(string APK_ROOT, ScriptingImplementation scriptBackend, bool isDevelop = false)
        {
            try
            {
                try
                {
                    if (Directory.Exists(APK_ROOT))
                    {
                        Directory.Delete(APK_ROOT, true);
                    }
                    if (!Directory.Exists(APK_ROOT))
                    {
                        Directory.CreateDirectory(APK_ROOT);
                    }
                }
                catch (Exception e)
                {

                }

                BuildAndriodTasks tasks = JsonUtility.FromJson<BuildAndriodTasks>(File.ReadAllText(ROOT_DIR + "/build_android_task.json"));
                BuildAndriodCommon common = tasks.common;

                //设置 打包的场景
                foreach (var task in tasks.tasks)
                {
                    Thread.Sleep(1000);

                    //sync
                    AssetDatabase.Refresh(ImportAssetOptions.Default);
                    List<string> _scenes = new System.Collections.Generic.List<string>();
                    foreach (var sc in EditorBuildSettings.scenes)
                    {
                        if (sc.enabled == false) continue;
                        _scenes.Add(sc.path);

                    }
                    //设置apk 包体信息
                    if (task.devconfig_download_sources == 10)
                    {
                        // 百度
                        PlayerSettings.SplashScreen.show = false;
                    }
                    else
                    {
                        PlayerSettings.SplashScreen.show = true;
                    }
                    Patches.Version v = new Patches.Version();
                    v.Parse(common.__ENGINE_VERSION__);
                    PlayerSettings.Android.bundleVersionCode = common.bundle_version_code;
                    PlayerSettings.bundleVersion = string.Format("{0}.{1}.{2}", v.MainVersion, v.SubVersion, task.devconfig_download_sources);
                    PlayerSettings.applicationIdentifier = task.package_name;
                    PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)common.minSdkVersion;
                    //修正 target SDK version 的值
                    // if (common.devconfig_server_platform == "ServerRelease")
                    // {
                    // PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
                    // }
                    // else
                    // {
                    // PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
                    // }
                    Process_DevCinfig_Auto_GEN(task.devconfig_download_sources, common.devconfig_server_platform, common.__Version__, common.__ENGINE_VERSION__);
                    Process_AD_SDK(task);
                    Process_PUSH_SDK(task);
                    Process_LebianClientChId(task);
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, scriptBackend);

                    string file = APK_ROOT + "/" + common.output_name_pre + common.version + "." + task.devconfig_download_sources + ".apk";
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception e)
                    {

                    }
                    //sync
                    AssetDatabase.Refresh(ImportAssetOptions.Default);
                    /*UnityEditor.Build.Reporting.BuildReport error = */

                    if (isDevelop)
                    {
                        BuildPipeline.BuildPlayer(_scenes.ToArray(), file, BuildTarget.Android, BuildOptions.Development | BuildOptions.ConnectWithProfiler);
                    }
                    else
                    {
                        BuildPipeline.BuildPlayer(_scenes.ToArray(), file, BuildTarget.Android, BuildOptions.None);
                    }

                    /*  if (error.)
                      {
                          Debug.LogError("build faild " + error);
                          return;
                      }*/
                    Debug.Log("build   " + file + " ok");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("build faild " + e.Message);
            }
        }
        const string engine_build_tag_dir = "RuntimeBuild/_TagForBuildEngine";
        public static void SetLastIsBuildEngineStandlone(bool ok)
        {

            if (ok)
            {
                if (Directory.Exists(engine_build_tag_dir))
                {

                }
                else
                {
                    Directory.CreateDirectory(engine_build_tag_dir);
                }
            }
            else
            {
                if (Directory.Exists(engine_build_tag_dir))
                {
                    Directory.Delete(engine_build_tag_dir);
                }
            }
        }
        public static bool GetLastIsBuildEngineStandlone()
        {
            return Directory.Exists(engine_build_tag_dir);
        }

        ///for build engine to AssetBundle
        public static void BuildApkMonoHelper()
        {
            if (Directory.Exists("AssetBundles/engine"))
            {
                Directory.Delete("AssetBundles/engine", true);
            }
            SetLastIsBuildEngineStandlone(true);
            BuildApk(APK_DIR_ROOT_MONO, ScriptingImplementation.Mono2x);
        }



        [MenuItem("打包工具/Build Apk Mono")]
        public static void BuildApkMono()
        {
            BuildApk(APK_DIR_ROOT_MONO, ScriptingImplementation.Mono2x);
        }

        [MenuItem("打包工具/Build Apk Mono【DEBUG】")]
        public static void BuildApkMono_Develop()
        {
            BuildApk(APK_DIR_ROOT_MONO, ScriptingImplementation.Mono2x, true);
        }

        [MenuItem("打包工具/Build Apk IL2CPP")]
        public static void BuildApkIL2CPP()
        {
            BuildApk(APK_DIR_ROOT_IL2CPP, ScriptingImplementation.IL2CPP);
        }

        [MenuItem("打包工具/Full Build Apk Mono 完整的制作ab 包 然后生成apk")]
        static void FullBuildApkMono()
        {
            //删除ab根目录
            try
            {
                Directory.Delete("AssetBundles", true);
            }
            catch (Exception e)
            {

            }
            //删除streamass 目录
            try
            {
                Directory.Delete("Assets/StreamingAssets/AssetBundles", true);
            }
            catch (Exception e)
            {

            }
            //EditorBuild.BuildAssetBundle.SetupABNames();
            EditorBuild.BuildAssetBundle._InnerBuildAllBundle(BuildTarget.Android);

            EditorBuild.BuildAssetBundle.CopyDir("AssetBundles", "Assets/StreamingAssets/AssetBundles");
            //输出完成
            //打包
            BuildApkMono();
        }
        [MenuItem("打包工具/Full Build Apk IL2CPP 完整的制作ab 包 然后生成apk")]
        static void FullBuildApkIL2CPP()
        {
            //删除ab根目录
            try
            {
                Directory.Delete("AssetBundles", true);
            }
            catch (Exception e)
            {

            }
            //删除streamass 目录
            try
            {
                Directory.Delete("Assets/StreamingAssets/AssetBundles", true);
            }
            catch (Exception e)
            {

            }
            //EditorBuild.BuildAssetBundle.SetupABNames();
            EditorBuild.BuildAssetBundle._InnerBuildAllBundle(BuildTarget.Android);
            EditorBuild.BuildAssetBundle.CopyDir("AssetBundles", "Assets/StreamingAssets/AssetBundles");
            //输出完成
            //打包
            BuildApkIL2CPP();
        }

        //
        public static void OnGenXCodeProject()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.ztgame.jdhcr");
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            EditorBuild.BuildAssetBundle.CopyDir("AssetBundles", "Assets/StreamingAssets/AssetBundles");
            string[] levels = { "Assets/Develop/FirstScene.unity" };
            BuildPipeline.BuildPlayer(levels, RuntimeMaker.GetXCodeProjectPath() + "/proj_ios", BuildTarget.iOS, BuildOptions.ShowBuiltPlayer);
        }

        public static string GetXCodeProjectPath()
        {
            int tmp = Application.dataPath.LastIndexOf("/");
            string path = Application.dataPath.Substring(0, tmp);
            return path;
        }

        public static void OnGenXCodeProjectAppstore()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.ligames.jdhcr");
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            EditorBuild.BuildAssetBundle.CopyDir("AssetBundles", "Assets/StreamingAssets/AssetBundles");
            string[] levels = { "Assets/Develop/FirstScene.unity" };
            BuildPipeline.BuildPlayer(levels, RuntimeMaker.GetXCodeProjectPath() + "/proj_ios", BuildTarget.iOS, BuildOptions.ShowBuiltPlayer);
        }
    }

}