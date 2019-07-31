/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

 */
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;



namespace EditorBuild
{
    public class BuildAssetBundle
    {
        //ab包输出根目录
        const string OutputRootDir = "AssetBundles";
        //根目录
        const string __path = "Assets/RawResources";

        //扩展名加入ab包 白名单
        static string[] valid_files_extensions =
        {
            ".png",
            ".prefab",
            ".jpg",
            ".png",
            ".asset",
            ".bytes",
            ".ttf",
            ".shader",
            ".lua",
            ".ogg",
            ".mp3",
            ".wav",
            ".txt",
            ".mat",
            ".anim",
            ".controller",
            ".tga",
            //".fbx",
            ".fontsettings",
            ".psd" //psd是不应该上传到资源目录的，等美术改了以后，应该移除
        };

        //目录 黑名单
        static string[] invalid_dir_names =
        {
            ".vscode",
            ".svn",
             "obj"
        };

        //视频文件格式 白名单
        static string[] valid_video_format =
        {
            ".mp4",
        };

        private static List<AssetBundleBuild> abs = new List<AssetBundleBuild>();
        private static List<string> needed_dir = new List<string>();
        private static Dictionary<string, List<string>> beDepInfoDict = new Dictionary<string, List<string>>();

        public static void CopyDir(string src, string des)
        {
            try
            {
                if (des[des.Length - 1] != Path.DirectorySeparatorChar)
                {
                    des += Path.DirectorySeparatorChar;
                }
                if (!Directory.Exists(des))
                {
                    Directory.CreateDirectory(des);
                }
                string[] fileList = Directory.GetFileSystemEntries(src);
                foreach (string file in fileList)
                {
                    if (Directory.Exists(file))
                    {
                        CopyDir(file, des + Path.GetFileName(file));
                    }
                    else
                    {
                        File.Copy(file, des + Path.GetFileName(file), true);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("copy dir error " + e.Message);
            }
        }

        static void TryDeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        static void _InnerBuildAssetBundle(string path, BuildTarget target, bool forceRebuild = true)
        {
            Directory.CreateDirectory(path);

            foreach (var p in needed_dir)
            {
                Directory.CreateDirectory(path + "/" + p);
            }
            if (forceRebuild)
                BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression |
                    BuildAssetBundleOptions.DeterministicAssetBundle |
                    BuildAssetBundleOptions.ForceRebuildAssetBundle, target);
            else
                BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression |
                    BuildAssetBundleOptions.DeterministicAssetBundle, target);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            //删除 不必要的文件
            foreach (var p in Directory.GetFiles("AssetBundles", "*.manifest",SearchOption.AllDirectories))
            {
                File.Delete(p);
            }
        }

        public static void _InnerBuildAllBundle(BuildTarget target, bool forceRebuild = true)
        {
            SetupABNames();
            //rebuild  lua
            SetupLuaABNames();
            //开始输出
            _InnerBuildAssetBundle(OutputRootDir, target, forceRebuild);
            //abs.Clear();
            needed_dir.Clear();
            //copy video files
            //_BuildVideos(); // disable by hanyingjun at 3/25/2019, because skingallery has destroy.
            EditorUtils.Utils.RemoveEmptyDir(OutputRootDir);
            //删除 不必要的文件
            foreach (var p in Directory.GetFiles("AssetBundles", "*.manifest", SearchOption.AllDirectories))
            {
                File.Delete(p);
            }
            //Debug.Log("BuildAssetBundle finished");
        }

        //被依赖的信息字典，key存储最基本文件（这些文件只能被依赖）的文件名，value储存依赖k的文件的列表，
        //如果发现一个基本文件被两个或者以上的文件依赖，则需要把这个文件打成依赖包。
        //使用的时候把所有需要打成ab的文件传入即可。
        public static void MarkDependencies(string fileName)
        {
            if (!fileName.EndsWith(".prefab")) return;
            string[] dependencies = AssetDatabase.GetDependencies(fileName);
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (dependencies[i].EndsWith(".cs"))
                    continue;
                if (!string.IsNullOrEmpty(dependencies[i]))
                {
                    var assetImporter = AssetImporter.GetAtPath(dependencies[i]);
                    if (beDepInfoDict.ContainsKey(dependencies[i]))
                    {
                        //assetImporter.assetBundleName = dependencies[i];
                        beDepInfoDict[dependencies[i]].Add(fileName);
                    }
                    else
                    {
                        beDepInfoDict[dependencies[i]] = new List<string>();
                        beDepInfoDict[dependencies[i]].Add(fileName);
                    }
                }
            }
        }

        //根据被依赖信息去反向查找引用当前资源的资源，如果当前文件没有被标注AB name, 同时存在两个及以上的资源同时引用当前文件，
        //且这些资源的ab name不一致，或者其中一个资源存放在Resources目录下或者被Resources目录下的资源引用，
        //则判定该资源被重复引用，打包的时候会出现冗余资源。
        private static void CheckDuplicatedAutoDepFile()
        {
            List<string> retLst = new List<string>();
            foreach (var entry in beDepInfoDict)
            {
                if (entry.Value.Count > 1)
                {
                    var importer = AssetImporter.GetAtPath(entry.Key);
                    if (string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        var abNameLst = new List<string>();

                        foreach (var file in entry.Value)
                        {
                            var fileImporter = AssetImporter.GetAtPath(file);
                            if (fileImporter != null)
                            {
                                if (file.StartsWith("Assets/Resources"))
                                {
                                    if (!abNameLst.Contains("Resources"))
                                    {
                                        abNameLst.Add("Resources");
                                    }
                                }
                                else
                                {
                                    if (!abNameLst.Contains(fileImporter.assetBundleName))
                                    {
                                        abNameLst.Add(fileImporter.assetBundleName);
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogErrorFormat("<color=red>file:{0} is not a asset.</color>", file);
                            }
                        }
                        if (abNameLst.Count > 1 || entry.Key.StartsWith("Assets/Resources"))
                        {
                            if (entry.Key.StartsWith("Assets/Resources") &&
                                abNameLst.Count == 1 && abNameLst[0] == "Resources")
                                return;
                            var str1 = string.Format("发现自动依赖的未标记AB Name资源（重复资源）：{0}, 依赖次数：{1} 详细：", entry.Key, abNameLst.Count);
                            foreach (var abNm in abNameLst)
                            {
                                str1 += abNm + ",";
                            }
                            //Debug.LogError(str1);
                            retLst.Add(str1);
                        }
                    }
                }
            }
            File.WriteAllLines(Application.dataPath + "/../重复冗余资源列表.txt", retLst.ToArray());
        }

        //这里我们可以认为resource目录下的文件也会被打包成一个独立的ab包，并且整个resource目录和resource目录下文件依赖的资源都是自动依赖的。
        //所以这里需要对resource目录做一下标记。
        static void MarkResourceFolder()
        {
            var files = GetFiles("Assets/Resources");
            foreach (var file in files)
            {
                MarkDependencies(file);
            }
        }

        static void SetFileABName(string abName, string[] files)
        {
            //Debug.LogFormat("<color=orange>ab name:{0}</color>", abName);
            foreach (var file in files)
            {
                var importer = AssetImporter.GetAtPath(file);
                importer.assetBundleName = abName;
                MarkDependencies(file);
                //Debug.LogFormat("<color=blue>file name:{0}</color>", file);
            }
        }

        //获取某目录下的所有文件
        static string[] GetFiles(string path)
        {
            var ret = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            List<string> list = new List<string>();
            foreach (var p in ret)
            {
                if (!IsValidFile(p)) continue;
                list.Add(p.Replace('\\', '/'));
            }
            return list.ToArray();
        }

        static AssetBundleBuild GenABBuild(string abname, string[] files)
        {
            AssetBundleBuild ret = new AssetBundleBuild();
            ret.assetBundleName = abname.Replace('\\', '/');
            ret.assetNames = files;
            return ret;
        }

        //输出一个地图目录的ab
        static List<AssetBundleBuild> GenABBuildMap(string filePath, string ABPath)
        {
            var tmp_files = Directory.GetFiles(filePath, "*.unity", SearchOption.TopDirectoryOnly);
            List<string> files = new List<string>();
            foreach (var d in tmp_files)
            {
                files.Add(d.Replace('\\', '/'));
            }
            List<AssetBundleBuild> rets = new List<AssetBundleBuild>();
            foreach (var file in files)
            {
                string abname = file.Substring(file.LastIndexOf('/') + 1, file.Length - file.LastIndexOf('/') - 1 - 6);
                //needed_dir.Add(ABPath.ToLower());
                abname = ABPath + '/' + abname;
                var ab = GenABBuild(abname.ToLower() + ".ab", new string[] { file });
                rets.Add(ab);
                //SetFileABName(ab.assetBundleName, ab.assetNames);
            }
            return rets;
        }


        //输出一个二级文件夹 的ab
        static List<AssetBundleBuild> GenABBuild2(string path)
        {
            var filePah = __path + "/" + path;
            var tmp_dirs = Directory.GetDirectories(filePah, "*", SearchOption.TopDirectoryOnly);
            List<string> dirs = new List<string>();

            foreach (var d in tmp_dirs)
            {
                if (IsValidDir(d))
                {
                    dirs.Add(d.Replace('\\', '/'));
                }
            }
            List<AssetBundleBuild> rets = new List<AssetBundleBuild>();

            foreach (var p in dirs)
            {
                //  needed_dir.Add(path);
                string abname = p.Substring(p.LastIndexOf('/') + 1, p.Length - p.LastIndexOf('/') - 1);
                needed_dir.Add(path.ToLower());
                abname = path + '/' + abname;
                var ab = GenABBuild(abname.ToLower() + ".ab", GetFiles(p));
                rets.Add(ab);
                //abs.Add(ab);
                //SetFileABName(ab.assetBundleName, ab.assetNames);
            }
            return rets;
        }

        //输出一个三级文件夹 的ab
        static List<AssetBundleBuild> GenABBuild3(string path)
        {
            needed_dir.Add(path.ToLower());

            var tmp_dirs = Directory.GetDirectories(__path + "/" + path, "*", SearchOption.TopDirectoryOnly);
            List<string> dirs = new List<string>();

            foreach (var d in tmp_dirs)
            {
                if (IsValidDir(d))
                {
                    dirs.Add(d.Replace('\\', '/'));
                }
            }
            List<AssetBundleBuild> rets = new List<AssetBundleBuild>();

            foreach (var p in dirs)
            {
                //  needed_dir.Add(path);
                string abname = p.Substring(p.LastIndexOf('/') + 1, p.Length - p.LastIndexOf('/') - 1);
                rets.AddRange(GenABBuild2(path + "/" + abname));
            }

            return rets;
        }

        static bool IsValidFile(string filename)
        {
            if (!File.Exists(filename)) return false;

            string shortName = Path.GetFileNameWithoutExtension(filename);
            if (shortName == "" || shortName.Trim() == "") return false;

            string ex = Path.GetExtension(filename);
            ex = ex.ToLower();

            //白名单开放添加到ab的文件格式，一般这里只需要填写unitiy内部格式即可，
            //如果是图片需要动态加载的话 也放行，一般只需要预设即可，预设关联的fbx等unity会自动处理到ab包里面 站这里无需重复添加
            foreach (var ext in valid_files_extensions)
            {
                if (ex.ToLower().Equals(ext))
                    return true;
            }
            return false;
        }

        static bool isValidVideo(string filename)
        {
            if (!File.Exists(filename)) return false;

            string shortName = Path.GetFileNameWithoutExtension(filename);
            if (shortName == "" || shortName.Trim() == "") return false;

            string ex = Path.GetExtension(filename);
            ex = ex.ToLower();

            //白名单开放添加到ab的文件格式，一般这里只需要填写unitiy内部格式即可，
            //如果是图片需要动态加载的话 也放行，一般只需要预设即可，预设关联的fbx等unity会自动处理到ab包里面 站这里无需重复添加
            foreach (var ext in valid_video_format)
            {
                if (ex.Equals(ext)) return true;
            }
            return false;
        }

        public static bool IsValidDir(string dirname)
        {
            if (dirname == null || dirname == "" || dirname.Trim() == "") return false;
            //if (!Directory.Exists(dirname))return false;

            foreach (var p in invalid_dir_names)
            {
                if (dirname.EndsWith(p)) return false;
            }
            return true;
        }

        //[MenuItem("AB包工具/单步执行/拷贝AB包到StreamingAssets目录", false, 17)]
        //private static void CopyAssetBundle()
        //{
        //    FileUtil.CopyFileOrDirectory("Assets/../AssetBundles", "Assets/StreamingAssets/AssetBundles");
        //} 

        [MenuItem("AB包工具/单步执行/快速输出当前平台AB包", false, 13)]
        private static void _QuickBuildAssetBundle()
        {
            _InnerBuildAssetBundle(OutputRootDir, EditorUserBuildSettings.activeBuildTarget, false);
            Debug.Log("finish quick build asset bundles!");
        }

        [MenuItem("AB包工具/单步执行/清除AB打包配置", false, 10)]
        public static void ClearAllAssetBundleNames()
        {
            var names = AssetDatabase.GetAllAssetBundleNames();
            foreach (var name in names)
                AssetDatabase.RemoveAssetBundleName(name, true);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            var anames = AssetDatabase.GetAllAssetBundleNames();
            UnityEngine.Debug.Log("ab name clean finished count:" + anames.Length);
            Debug.Log("finish clear asset bundle asset bundle name!");
        }

        [MenuItem("AB包工具/单步执行/设置AB打包配置", false, 11)]
        public static void SetupABNames()
        {
            var setting = ABSetting.GetSetting();
            beDepInfoDict.Clear();
            int progressCount = 0;
            AssetDatabase.StartAssetEditing();
            foreach (var info in setting.infos)
            {
                progressCount++;
                EditorUtility.DisplayProgressBar("设置AB包配置", "设置" + info.ABName, (float)progressCount / (float)setting.infos.Count);

                //添加一个文件夹是 一个ab包资源
                if (info.subFolderLevel == 0)
                {
                    for (int j = 0; j < info.paths.Count; j++)
                    {
                        var path = AssetDatabase.GetAssetPath(info.paths[j]).Replace("\\", "/");
                        var ab = GenABBuild(info.ABName, GetFiles(path));
                        SetFileABName(ab.assetBundleName, ab.assetNames);
                    }
                }
                //添加一个二级文件夹是一个ab包资源
                if (info.subFolderLevel == 1)
                {
                    //这里的ab name必须路径的子目录一个名字。
                    var ab = GenABBuild2(info.ABName);
                    foreach (var a in ab)
                        SetFileABName(a.assetBundleName, a.assetNames);
                }
                //添加一个三级文件夹是一个ab包资源
                if (info.subFolderLevel == 2)
                {
                    //这里的ab name必须路径的子目录一个名字。
                    var ab = GenABBuild3(info.ABName);
                    foreach (var a in ab)
                        SetFileABName(a.assetBundleName, a.assetNames);
                }
                //添加一个场景文件夹
                if (info.subFolderLevel == 3)
                {
                    foreach (var folder in info.paths)
                    {
                        var path = AssetDatabase.GetAssetPath(folder).Replace("\\", "/");
                        var ab = GenABBuildMap(path, info.ABName);
                        foreach (var a in ab)
                            SetFileABName(a.assetBundleName, a.assetNames);
                    }
                }
            }
            MarkResourceFolder();
            AssetDatabase.StopAssetEditing();
            EditorUtility.DisplayProgressBar("设置AB包配置", "完成设置", 1f);
            CheckDuplicatedAutoDepFile();
            AssetDatabase.RemoveUnusedAssetBundleNames();
            var allABNames = AssetDatabase.GetAllAssetBundleNames();
            //UnityEngine.Debug.Log("total ab name count:" + allABNames.Length);
            EditorUtility.ClearProgressBar();
            Debug.Log("finish set asset bundle name!");
        }

        [MenuItem("AB包工具/单步执行/设置lua打包配置", false, 12)]
        private static void SetupLuaABNames()
        {
            //lua ab单独实现
            try
            {
                Directory.Delete(ResourceLoaderBase.AssetBundlePreName + "lua");
            }
            catch
            {
            }
            try
            {
                Directory.CreateDirectory(ResourceLoaderBase.AssetBundlePreName + "lua");
            }
            catch
            {
            }
            ToLuaMenu.BuildJitBundlesExt();
            Debug.Log("finish build lua!");
        }

        //视频文件采用直接复制文件的方式，视频文件暂时不支持打成ab包
        [MenuItem("AB包工具/单步执行/输出视频资源包", false, 14)]
        private static void _BuildVideos()
        {
            try
            {
                Directory.Delete(ResourceLoaderBase.AssetBundlePreName + "videos");
            }
            catch
            {
            }
            try
            {
                Directory.CreateDirectory(ResourceLoaderBase.AssetBundlePreName + "videos");
            }
            catch
            {
            }
            String VideoSourcePath = __path + "/Videos";
            String VideoTargetPath = OutputRootDir + "/videos";
            //删除已有文件
            var existFileList = Directory.GetFiles(VideoTargetPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var p in existFileList)
            {
                File.Delete(p);
            }
            //复制新文件
            var ret = Directory.GetFiles(VideoSourcePath, "*.*", SearchOption.TopDirectoryOnly);
            List<string> list = new List<string>();
            foreach (var p in ret)
            {
                if (!isValidVideo(p)) continue;
                String fileName = Path.GetFileName(p);
                File.Copy(p, VideoTargetPath + "/" + fileName);
                Debug.Log("finish build video!");
            }
        }

        [MenuItem("AB包工具/单步执行/输出代码资源包", false, 15)]
        //打包C#代码 的热更文件
        public static void BuildApkMonoHelper()
        {
            RuntimeMaker.BuildApkMonoHelper();
            Debug.Log("finish build mono helper!");

        }

        [MenuItem("AB包工具/完整输出当前平台AB包", false, 100)]
        public static void _BuildAssetBundle()
        {
            //SetupABNames();
            _InnerBuildAllBundle(EditorUserBuildSettings.activeBuildTarget);
            Debug.Log("finish build all asset bundle!");
        }

        [MenuItem("AB包工具/一键输出Android", false, 101)]
        public static void _BuildAssetBundle_Android()
        {
            TryDeleteDir("AssetBundles");
            //SetupABNames();
            _InnerBuildAllBundle(BuildTarget.Android);
            CopyDir("AssetBundles", "VersionFiles/AssetBundles/Android/AssetBundles");
        }


        [MenuItem("AB包工具/一键输出IOS", false, 102)]
        public static void _BuildAssetBundle_IOS()
        {
            Directory.Delete("AssetBundles", true);
            //SetupABNames();
            _InnerBuildAllBundle(BuildTarget.iOS);
            CopyDir("AssetBundles", "VersionFiles/AssetBundles/IOS/AssetBundles");
        }

        [MenuItem("AB包工具/缩包输出", false, 103)]
        public static void BuildAllProcedure()
        {
            _BuildAssetBundle();
#if UNITY_ANDROID 
            BuildApkMonoHelper();
#endif
            PatchMaker._BuildFileVersionList();
            PatchMaker._BuildFSFirm();
            RuntimeMaker.BuildApkMono();
        }

        [MenuItem("AB包工具/整包输出", false, 104)]
        public static void BuildAllProcedureFull()
        {
            _BuildAssetBundle();
            //#if UNITY_ANDROID
            //BuildApkMonoHelper();
            //#endif
            PatchMaker._BuildFileVersionList();
            PatchMaker._BuildFSFirmFull();
            RuntimeMaker.BuildApkIL2CPP();
            //RuntimeMaker.BuildApkMono();
        }

        [MenuItem("AB包工具/半包输出", false, 105)]
        public static void BuildAllProcedureHalf()
        {
            _BuildAssetBundle();
#if UNITY_ANDROID
            BuildApkMonoHelper();
#endif
            PatchMaker._BuildFileVersionList();
            PatchMaker._BuildFSFirmHalf();
            RuntimeMaker.BuildApkMono();
        }

        [MenuItem("AB包工具/强制使用AB模式", false, 200)]
        static void enableForceUseAB()
        {
            SetEditorBuildSettingRelease();
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            defines.Add("FORCE_USE_AB");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
        }

        [MenuItem("AB包工具/强制使用AB模式", true)]
        static bool enableForceUseABValidate()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            return !defines.Contains("FORCE_USE_AB");
        }

        [MenuItem("AB包工具/禁止使用AB模式", false, 201)]
        static void disableForceUseAB()
        {
            SetEditorBuildSettingDebug();
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            while (defines.Remove("FORCE_USE_AB")) ;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
            SetEditorBuildSettingDebug();
        }

        [MenuItem("AB包工具/禁止使用AB模式", true)]
        static bool disableForceUseABValidate()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            return defines.Contains("FORCE_USE_AB");
        }

        [MenuItem("AB包工具/设置加载场景模式到调试模式", false, 302)]
        static void SetEditorBuildSettingDebug()
        {
            var lst = EditorBuildSettings.scenes;
            var ret = new List<EditorBuildSettingsScene>();
            foreach (var value in lst)
            {
                if (value.path.Contains("CaptureTest"))
                    ret.Add(new EditorBuildSettingsScene(value.path, false));
                else
                    ret.Add(new EditorBuildSettingsScene(value.path, true));
            }
            EditorBuildSettings.scenes = ret.ToArray();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("AB包工具/设置加载场景模式到调试模式", true)]
        static bool SetEditorBuildSettingDebugValidate()
        {
            var lst = EditorBuildSettings.scenes;
            var ret = false;
            foreach (var value in lst)
            {
                if (value.path.Contains("CaptureTest"))
                    continue;
                if (value.path.Contains("FirstScene"))
                    continue;
                if (!value.enabled)
                    return true;
            }
            return ret;
        }

        [MenuItem("AB包工具/设置加载场景模式到AB模式", false, 301)]
        static void SetEditorBuildSettingRelease()
        {
            var lst = EditorBuildSettings.scenes;
            var ret = new List<EditorBuildSettingsScene>();
            foreach (var value in lst)
            {
                if (value.path.Contains("FirstScene") || value.path.Contains("bg"))
                    ret.Add(new EditorBuildSettingsScene(value.path, true));
                else
                    ret.Add(new EditorBuildSettingsScene(value.path, false));
            }
            EditorBuildSettings.scenes = ret.ToArray();
            AssetDatabase.SaveAssets();
        }


        [MenuItem("AB包工具/设置加载场景模式到AB模式", true)]
        static bool SetEditorBuildSettingReleaseValidate()
        {
            return !SetEditorBuildSettingDebugValidate();
        }


        // lebian 测试服
        [MenuItem("打包工具/使用LBClientChId_Tencent", false, 305)]
        static void EnableLBTencent()
        {
            SetEditorBuildSettingRelease();
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            defines.Add("LEBIAN_TENCENT");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
        }

        [MenuItem("打包工具/使用LBClientChId_Tencent", true)]
        static bool EnableLBTencentValidate()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            return !defines.Contains("LEBIAN_TENCENT");
        }

        // lebian 测试服
        [MenuItem("打包工具/不使用LBClientChId_Tencent", false, 306)]
        static void DisableLBTencent()
        {
            SetEditorBuildSettingDebug();
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            while (defines.Remove("LEBIAN_TENCENT")) ;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
            SetEditorBuildSettingDebug();
        }

        [MenuItem("打包工具/不使用LBClientChId_Tencent", true)]
        static bool DisableLBTencentValidate()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            return defines.Contains("LEBIAN_TENCENT");
        }
    }
}