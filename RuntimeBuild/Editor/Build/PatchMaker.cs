/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

 */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System;

namespace EditorBuild
{
    public class VersionList
    {
        public List<Patches.Version> versions = new List<Patches.Version>();
    }

    public class PatchMaker
    {
        [MenuItem("补丁工具/ **************使用方法和注意事项请看VersionFiles目录下的readme.txt*********", false, 0)]
        static void ________________________()
        {

        }

        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android
        //---------------------------------------------Android


        [MenuItem("补丁工具/--------------android tools--------split line--------------")]
        static void ________Android___________()
        {
        }


        //    [MenuItem("补丁工具/------one key BuildPatch_Android with VERSION.txt")]
        [MenuItem("补丁工具/------一键生成Android补丁信息 VERSION.txt")]

        //该函数分为3步 1.生成AssetBundles 2.检查svn文件状态找出diff到AssetBundles_diff目录 3.生成zip文件和json文件
        public static void Build_Patch_Android()
        {
            EditorUtils.Utils.ClearConsole();
            _Build_Patch__Android_Step_1();
            _Build_Patch__Android_Step_2();
            _Build_Patch__Android_Step_3();
            Debug.Log("--------------build patch done");
            EditorUtils.Utils.ShowDialog("build android done");
        }
        [MenuItem("补丁工具/---1---BuildPatch_Android Step1 build AssetBundle")]
        private static void _Build_Patch__Android_Step_1()
        {
            //build assetbundle
            EditorBuild.BuildAssetBundle._BuildAssetBundle_Android();
        }
        [MenuItem("补丁工具/---2---BuildPatch_Android Step2 check svn diff files")]
        private static void _Build_Patch__Android_Step_2()
        {
            //check svn
            string target_path_pre_name = "VersionFiles/AssetBundles/Android/";
            string path = EditorUtils.Utils.GetCurrentWorkingPath() + "/VersionFiles/AssetBundles/Android/AssetBundles";
            string tmp_out_file_path = "tmp_svn_diff_out.txt";
            SVNDiff.CheckFiles(path, target_path_pre_name, tmp_out_file_path);
        }
        [MenuItem("补丁工具/---3---BuildPatch_Android Step3 build zip to dir VersionFiles")]
        private static void _Build_Patch__Android_Step_3()
        {
            //build zip
            _BuildZip_Android();
        }

        [MenuItem("补丁工具/BuildZip_Android")]
        public static void _BuildZip_Android()
        {
            string path_pre_name = "VersionFiles/AssetBundles/Android/";

            string path = EditorUtils.Utils.GetCurrentWorkingPath() + "/VersionFiles/AssetBundles/Android/AssetBundles_diff";

            //需要构建的版本号

            string v = File.ReadAllText("VersionFiles/VERSION.txt");


            string out_zip_full_name = "android_" + v + ".zip"; // "VersionFiles/" + v + "/android_" + v + ".zip";
            using (ZipFile zip = ZipFile.Create(out_zip_full_name))
            {
                zip.BeginUpdate();
                zip.AddDirectory("AssetBundles");
                try
                {
                    Directory.Delete("AssetBundles_diff", true);
                }
                catch (Exception e)
                {

                }
                //这样绕一下处理diff是因为 lua/lua这个文件 在 原路径下 打包zip会出错(ziplib的BUG) 
                EditorUtils.Utils.CopyDir("VersionFiles/AssetBundles/Android/AssetBundles_diff", "AssetBundles_diff");

                foreach (var d in Directory.GetDirectories("AssetBundles_diff"))
                {
                    zip.AddDirectory(d.Replace("AssetBundles_diff", "AssetBundles"));
                }
                foreach (var p in Directory.GetFiles("AssetBundles_diff", "*", SearchOption.AllDirectories))
                {
                    zip.Add(p, p.Replace("AssetBundles_diff", "AssetBundles"));
                }
                zip.CommitUpdate();
                Directory.Delete("AssetBundles_diff", true);
            }
            if (File.Exists("VersionFiles/" + v + "/android_" + v + ".zip"))
                File.Delete("VersionFiles/" + v + "/android_" + v + ".zip");
            if (Directory.Exists("VersionFiles/" + v) == false)
            {
                Directory.CreateDirectory("VersionFiles/" + v);

            }

            File.Move(out_zip_full_name, "VersionFiles/" + v + "/android_" + v + ".zip");
            File.Delete(out_zip_full_name);

            out_zip_full_name = "VersionFiles/" + v + "/android_" + v + ".zip";

            string md5 = MD5Code.GetMD5HashFromFile(out_zip_full_name);
            int size = 0;
            using (var zip = File.OpenRead(out_zip_full_name))
            {
                size = (int)zip.Length;
            }
            Patches.Version this_version = new Patches.Version();
            Debug.Log("Patch Zip:will build version = " + v);
            this_version.Parse(v);
            this_version.MD5 = md5;
            this_version.FileSize = size;

            //  Debug.LogError(LitJson.JsonMapper.ToJson(this_version));


            {
                var ss = File.CreateText("VersionFiles/" + v + "/android_" + v + ".json");


                ss.Write(LitJson.JsonMapper.ToJson(this_version));
                ss.Flush();
                ss.Close();
            }

            //resfresh all 
            {
                //      Debug.LogError(pat);
                var patches = Directory.GetDirectories("VersionFiles", this_version.MainVersion + ".*.*", SearchOption.AllDirectories);
                List<Patches.Version> _patch_vs = new System.Collections.Generic.List<Patches.Version>();
                foreach (var pp in patches)
                {
                    var p = Path.GetFileName(pp);
                    Patches.Version vv = new Patches.Version();
                    vv.Parse(p);
                    _patch_vs.Add(vv);
                }
                _patch_vs.Sort();
                VersionList _VL = new VersionList();

                foreach (var p in _patch_vs)
                {
                    string ver = p.MainVersion.ToString() + "." + p.SubVersion.ToString() + "." + p.PatchVersion.ToString() + "/android_" + p.MainVersion.ToString() + "." + p.SubVersion.ToString() + "." + p.PatchVersion.ToString() + ".json";
                    var this_v = File.ReadAllText("VersionFiles/" + ver);
                    Debug.LogError(ver);

                    _VL.versions.Add(LitJson.JsonMapper.ToObject<Patches.Version>(this_v));
                }

                {
                    var ss = File.CreateText("VersionFiles/FULL_VERSIONS.json");

                    ss.Write(LitJson.JsonMapper.ToJson(_VL));
                    ss.Flush();
                    ss.Close();
                }

            }
            Debug.Log("Patch Zip:build android done ");


        }

        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android
        //-----------------------------------------------IOS   code is the same with android


        [MenuItem("补丁工具/--------------ios tools--------split line--------------")]
        static void ________IOS___________()
        {
        }
        // [MenuItem("补丁工具/------one key BuildPatch_IOS with VERSION.txt")]
        [MenuItem("补丁工具/------一键生成IOS补丁信息 VERSION.txt")]
        //该函数分为3步 1.生成AssetBundles 2.检查svn文件状态找出diff到AssetBundles_diff目录 3.生成zip文件和json文件
        public static void Build_Patch_IOS()
        {
            EditorUtils.Utils.ClearConsole();
            _Build_Patch__IOS_Step_1();
            _Build_Patch__IOS_Step_2();
            _Build_Patch__IOS_Step_3();
            Debug.Log("--------------build patch done");
        }
        [MenuItem("补丁工具/---1---BuildPatch_IOS Step1 build AssetBundle")]
        private static void _Build_Patch__IOS_Step_1()
        {
            //build assetbundle
            EditorBuild.BuildAssetBundle._BuildAssetBundle_IOS();
        }
        [MenuItem("补丁工具/---2---BuildPatch_IOS Step2 check svn diff files")]
        private static void _Build_Patch__IOS_Step_2()
        {
            //check svn
            string target_path_pre_name = "VersionFiles/AssetBundles/IOS/";
            string path = EditorUtils.Utils.GetCurrentWorkingPath() + "/VersionFiles/AssetBundles/IOS/AssetBundles";
            string tmp_out_file_path = "tmp_svn_diff_out.txt";
            SVNDiff.CheckFiles(path, target_path_pre_name, tmp_out_file_path);
        }
        [MenuItem("补丁工具/---3---BuildPatch_IOS Step3 build zip to dir VersionFiles")]
        private static void _Build_Patch__IOS_Step_3()
        {
            //build zip
            _BuildZip_IOS();
        }

        [MenuItem("补丁工具/BuildZip_IOS")]
        public static void _BuildZip_IOS()
        {
            string path_pre_name = "VersionFiles/AssetBundles/IOS/";

            string path = EditorUtils.Utils.GetCurrentWorkingPath() + "/VersionFiles/AssetBundles/IOS/AssetBundles_diff";

            //需要构建的版本号

            string v = File.ReadAllText("VersionFiles/VERSION.txt");


            string out_zip_full_name = "ios_" + v + ".zip"; // "VersionFiles/" + v + "/android_" + v + ".zip";
            using (ZipFile zip = ZipFile.Create(out_zip_full_name))
            {

                zip.BeginUpdate();
                zip.AddDirectory("AssetBundles");
                try
                {
                    Directory.Delete("AssetBundles_diff", true);
                }
                catch (Exception e)
                {

                }
                //这样绕一下处理diff是因为 lua/lua这个文件 在 原路径下 打包zip会出错(ziplib的BUG) 
                EditorUtils.Utils.CopyDir("VersionFiles/AssetBundles/IOS/AssetBundles_diff", "AssetBundles_diff");

                foreach (var d in Directory.GetDirectories("AssetBundles_diff"))
                {
                    zip.AddDirectory(d.Replace("AssetBundles_diff", "AssetBundles"));
                }
                foreach (var p in Directory.GetFiles("AssetBundles_diff", "*", SearchOption.AllDirectories))
                {
                    zip.Add(p, p.Replace("AssetBundles_diff", "AssetBundles"));
                }
                zip.CommitUpdate();
                Directory.Delete("AssetBundles_diff", true);
            }
            if (File.Exists("VersionFiles/" + v + "/ios_" + v + ".zip"))
                File.Delete("VersionFiles/" + v + "/ios_" + v + ".zip");
            if (Directory.Exists("VersionFiles/" + v) == false)
            {
                Directory.CreateDirectory("VersionFiles/" + v);

            }

            File.Move(out_zip_full_name, "VersionFiles/" + v + "/ios_" + v + ".zip");
            File.Delete(out_zip_full_name);

            out_zip_full_name = "VersionFiles/" + v + "/ios_" + v + ".zip";

            string md5 = MD5Code.GetMD5HashFromFile(out_zip_full_name);
            int size = 0;
            using (var zip = File.OpenRead(out_zip_full_name))
            {
                size = (int)zip.Length;
            }
            Patches.Version this_version = new Patches.Version();
            Debug.Log("Patch Zip:will build version = " + v);
            this_version.Parse(v);
            this_version.MD5 = md5;
            this_version.FileSize = size;

            //  Debug.LogError(LitJson.JsonMapper.ToJson(this_version));


            {
                var ss = File.CreateText("VersionFiles/" + v + "/ios_" + v + ".json");


                ss.Write(LitJson.JsonMapper.ToJson(this_version));
                ss.Flush();
                ss.Close();
            }

            //resfresh all 
            {
                string pat = this_version.MainVersion.ToString() + "." + this_version.SubVersion.ToString() + ".";
                //      Debug.LogError(pat);
                var patches = Directory.GetDirectories("VersionFiles", this_version.MainVersion + "." + this_version.SubVersion + "." + "*", SearchOption.AllDirectories);
                List<int> _patch_vs = new System.Collections.Generic.List<int>();
                foreach (var pp in patches)
                {
                    var p = Path.GetFileName(pp);
                    Patches.Version vv = new Patches.Version();
                    //    Debug.LogError(pp + "     " + p);
                    vv.Parse(p);
                    _patch_vs.Add(vv.PatchVersion);
                }
                _patch_vs.Sort();
                VersionList _VL = new VersionList();

                foreach (var p in _patch_vs)
                {
                    string ver = this_version.MainVersion.ToString() + "." + this_version.SubVersion.ToString() + "." + p.ToString() + "/ios_" + this_version.MainVersion.ToString() + "." + this_version.SubVersion.ToString() + "." + p.ToString() + ".json";

                    var this_v = File.ReadAllText("VersionFiles/" + ver);
                    //   Debug.LogError(ver);
                    Patches.Version x = new Patches.Version();
                    _VL.versions.Add(LitJson.JsonMapper.ToObject<Patches.Version>(this_v));
                }

                {
                    var ss = File.CreateText("VersionFiles/FULL_VERSIONS_IOS.json");

                    ss.Write(LitJson.JsonMapper.ToJson(_VL));
                    ss.Flush();
                    ss.Close();
                }

            }
            Debug.Log("Patch Zip:build ios done ");

        }


        [MenuItem("补丁工具/--------------misc tools--------split line--------------")]
        static void ________Misc___________()
        {
        }

        [MenuItem("补丁工具/BuildZip AssetBundle完整输出zip到对应目录 VERSION.txt 方便内部测试", false, 2000)]
        public static void _BuildZip()
        {
            //需要构建的版本号

            string v = File.ReadAllText("VersionFiles/VERSION.txt");


            string out_zip_full_name = "android_" + v + ".zip"; // "VersionFiles/" + v + "/android_" + v + ".zip";
            using (ZipFile zip = ZipFile.Create(out_zip_full_name))
            {
                zip.BeginUpdate();

                zip.AddDirectory("AssetBundles");

                foreach (var d in Directory.GetDirectories("AssetBundles"))
                {
                    zip.AddDirectory(d);
                }
                foreach (var p in Directory.GetFiles("AssetBundles", "*", SearchOption.AllDirectories))
                {
                    zip.Add(p);
                    //  Debug.LogError(p);
                }

                zip.CommitUpdate();

            }
            if (File.Exists("VersionFiles/" + v + "/android_" + v + ".zip"))
                File.Delete("VersionFiles/" + v + "/android_" + v + ".zip");
            if (Directory.Exists("VersionFiles/" + v) == false)
            {
                Directory.CreateDirectory("VersionFiles/" + v);

            }

            File.Move(out_zip_full_name, "VersionFiles/" + v + "/android_" + v + ".zip");
            File.Delete(out_zip_full_name);

            out_zip_full_name = "VersionFiles/" + v + "/android_" + v + ".zip";

            string md5 = MD5Code.GetMD5HashFromFile(out_zip_full_name);
            int size = 0;
            using (var zip = File.OpenRead(out_zip_full_name))
            {
                size = (int)zip.Length;
            }
            Patches.Version this_version = new Patches.Version();
            Debug.LogError(v);
            this_version.Parse(v);
            this_version.MD5 = md5;
            this_version.FileSize = size;
            Debug.LogError(LitJson.JsonMapper.ToJson(this_version));

            {
                var ss = File.CreateText("VersionFiles/" + v + "/android_" + v + ".json");
                ss.Write(LitJson.JsonMapper.ToJson(this_version));
                ss.Flush();
                ss.Close();
            }

            //resfresh all 
            {
                string pat = this_version.MainVersion.ToString() + "." + this_version.SubVersion.ToString() + ".";
                //      Debug.LogError(pat);
                var patches = Directory.GetDirectories("VersionFiles", this_version.MainVersion + "." + this_version.SubVersion + ".*", SearchOption.AllDirectories);
                List<int> _patch_vs = new System.Collections.Generic.List<int>();
                foreach (var pp in patches)
                {
                    var p = Path.GetFileName(pp);
                    Patches.Version vv = new Patches.Version();
                    Debug.LogError(pp + "     " + p);
                    vv.Parse(p);
                    _patch_vs.Add(vv.PatchVersion);


                }
                _patch_vs.Sort();
                VersionList _VL = new VersionList();

                foreach (var p in _patch_vs)
                {
                    string ver = this_version.MainVersion.ToString() + "." + this_version.SubVersion.ToString() + "." + p.ToString() + "/android_" + this_version.MainVersion.ToString() + "." + this_version.SubVersion.ToString() + "." + p.ToString() + ".json";

                    var this_v = File.ReadAllText("VersionFiles/" + ver);
                    Debug.LogError(ver);

                    Patches.Version x = new Patches.Version();

                    _VL.versions.Add(LitJson.JsonMapper.ToObject<Patches.Version>(this_v));
                }

                {
                    var ss = File.CreateText("VersionFiles/FULL_VERSIONS.json");

                    ss.Write(LitJson.JsonMapper.ToJson(_VL));
                    ss.Flush();
                    ss.Close();
                }
            }
        }

        [MenuItem("补丁工具/UnZip", false, 2001)]
        public static void _UnZip()
        {
            string desStoragePath = "zipp";

            //没有存储目录应创建
            if (!Directory.Exists(desStoragePath))
            {
                Directory.CreateDirectory(desStoragePath);
            }


            using (var zipFileStream = File.OpenRead("a.zip"))
            {
                using (ZipInputStream unzipStream = new ZipInputStream(zipFileStream))
                {
                    //totalByte
                    var totalByte = zipFileStream.Length;
                    var unzipByte = 0;

                    Debug.LogError(totalByte);
                    ZipEntry theEntry;
                    while ((theEntry = unzipStream.GetNextEntry()) != null)
                    {
                        Debug.LogError("loop");
                        string fullName = desStoragePath + theEntry.Name;
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

                                unzipByte += readlen;

                                fs.Write(content, 0, readlen);
                                fs.Flush();

                                fs.Close();
                                fs.Dispose();

                            }
                            catch (System.Exception e)
                            {

                                Debug.LogError("unzip read errorMsg===" + e.Message);
                            }
                        }
                    }
                }

            }
        }



        [MenuItem("补丁工具(新)/制作版本文件信息 FileVersionList.json", false, 2000)]
        public static void _BuildFileVersionList()
        {
#if UNITY_IOS
            string file_name = "FileVersionList_ios.json";
#else
            string file_name = "FileVersionList.json";
#endif

            try
            {
                File.Delete(file_name);
            }
            catch (Exception e)
            {

            }
            var files = Directory.GetFiles("AssetBundles", "*", System.IO.SearchOption.AllDirectories);
            FileVersionList list = new FileVersionList();
            foreach (var p in files)
            {
                FileVersion v = new FileVersion();
                v.file = p.Replace("\\", "/");
                v.md5 = MD5Code.GetMD5HashFromFile(p);
                FileInfo info = new FileInfo(v.file);
                v.size = (int)info.Length;
                list.files.Add(v);
            }
            var json = UnityEngine.JsonUtility.ToJson(list);
            File.WriteAllText(file_name, json);
        }
        [MenuItem("补丁工具(新)/生成固件(FSFirm)文件", false, 2000)]
        public static void _BuildFSFirm()
        {
            try
            {
                Directory.Delete("Assets/StreamingAssets");
            }
            catch (Exception e) { }
            try
            {
                Directory.CreateDirectory("Assets/StreamingAssets");
            }
            catch (Exception e) { }


            var files = Directory.GetFiles("AssetBundles", "*", System.IO.SearchOption.AllDirectories);

            FileVersionList fsfirm_list = new FileVersionList();

            foreach (var pp in files)
            {
                string p = pp.Replace("\\", "/");
                if (Patches.Patcher.ForceCheckMD5(p))
                {
                    FileVersion v = new FileVersion();
                    v.file = p;
                    v.size = (int)(new FileInfo(p)).Length;
                    v.md5 = MD5Code.GetMD5HashFromFile(p);

                    fsfirm_list.files.Add(v);

                    //engine 文件不需要打进去
                    if (p.Contains("/engine/")) continue;

                    string target = "Assets/StreamingAssets/" + p;
                    int found = target.LastIndexOf("/");
                    if (found != target.Length)
                    {
                        if (!Directory.Exists(target.Substring(0, found)))
                        {
                            Directory.CreateDirectory(target.Substring(0, found));
                        }
                    }
                    File.Copy(p, target, true);
                }
            }
            try
            {
                File.Delete(DevConfig.FSFirmVersionListFileName);
            }
            catch (Exception e) { }

            File.WriteAllText("Assets/Resources/" + DevConfig.FSFirmVersionListFileName, JsonUtility.ToJson(fsfirm_list));

            AssetDatabase.Refresh();
        }
        [MenuItem("补丁工具(新)/生成固件(FSFirm)文件 整包输出", false, 2000)]
        public static void _BuildFSFirmFull()
        {
            try
            {
                Directory.Delete("Assets/StreamingAssets");
            }
            catch (Exception e) { }
            try
            {
                Directory.CreateDirectory("Assets/StreamingAssets");
            }
            catch (Exception e) { }


            var files = Directory.GetFiles("AssetBundles", "*", System.IO.SearchOption.AllDirectories);

            FileVersionList fsfirm_list = new FileVersionList();

            foreach (var pp in files)
            {
                string p = pp.Replace("\\", "/");
                FileVersion v = new FileVersion();
                v.file = p;
                v.size = (int)(new FileInfo(p)).Length;
                v.md5 = MD5Code.GetMD5HashFromFile(p);

                fsfirm_list.files.Add(v);

                //engine 文件不需要打进去
                if (p.Contains("/engine/")) continue;

                string target = "Assets/StreamingAssets/" + p;
                int found = target.LastIndexOf("/");
                if (found != target.Length)
                {
                    if (!Directory.Exists(target.Substring(0, found)))
                    {
                        Directory.CreateDirectory(target.Substring(0, found));
                    }
                }
                File.Copy(p, target, true);
            }
            try
            {
                File.Delete(DevConfig.FSFirmVersionListFileName);
            }
            catch (Exception e) { }

            File.WriteAllText("Assets/Resources/" + DevConfig.FSFirmVersionListFileName, JsonUtility.ToJson(fsfirm_list));

            AssetDatabase.Refresh();
        }

        [MenuItem("补丁工具(新)/生成固件(FSFirm)文件 半包输出(不用重启客户端)", false, 2000)]
        public static void _BuildFSFirmHalf()
        {
            try
            {
                Directory.Delete("Assets/StreamingAssets");
            }
            catch (Exception e) { }
            try
            {
                Directory.CreateDirectory("Assets/StreamingAssets");
            }
            catch (Exception e) { }


            var files = Directory.GetFiles("AssetBundles", "*", System.IO.SearchOption.AllDirectories);

            FileVersionList fsfirm_list = new FileVersionList();

            foreach (var pp in files)
            {
                string p = pp.Replace("\\", "/");
                if (Patches.Patcher.ForceCheckMD5(p) || (p.Contains("/skin/") == false && p.Contains("/weapons/") == false))
                {
                    FileVersion v = new FileVersion();
                    v.file = p;
                    v.size = (int)(new FileInfo(p)).Length;
                    v.md5 = MD5Code.GetMD5HashFromFile(p);

                    fsfirm_list.files.Add(v);

                    //engine 文件不需要打进去
                    if (p.Contains("/engine/")) continue;
                    string target = "Assets/StreamingAssets/" + p;
                    int found = target.LastIndexOf("/");
                    if (found != target.Length)
                    {
                        if (!Directory.Exists(target.Substring(0, found)))
                        {
                            Directory.CreateDirectory(target.Substring(0, found));
                        }
                    }
                    File.Copy(p, target, true);
                }
            }
            try
            {
                File.Delete(DevConfig.FSFirmVersionListFileName);
            }
            catch (Exception e) { }

            File.WriteAllText("Assets/Resources/" + DevConfig.FSFirmVersionListFileName, JsonUtility.ToJson(fsfirm_list));

            AssetDatabase.Refresh();
        }


        [MenuItem("补丁工具(新)/DISABLE_FILE_VALID  Enable")]
        static void enableForceUseAB()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            defines.Add("DISABLE_FILE_VALID");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
        }

        [MenuItem("补丁工具(新)/DISABLE_FILE_VALID  Enable", true)]
        static bool enableForceUseABValidate()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            return !defines.Contains("DISABLE_FILE_VALID");
        }

        [MenuItem("补丁工具(新)/DISABLE_FILE_VALID  Disable")]
        static void disableForceUseAB()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            while (defines.Remove("DISABLE_FILE_VALID")) ;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
        }

        [MenuItem("补丁工具(新)/DISABLE_FILE_VALID  Disable", true)]
        static bool disableForceUseABValidate()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            return defines.Contains("DISABLE_FILE_VALID");
        }
    }
}