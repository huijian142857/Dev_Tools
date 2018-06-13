using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public partial class AB2UnityEditorWindow : EditorWindow
{
    public static AB2UnityEditorWindow window;

    public static void Open()
    {
        window = EditorWindow.GetWindow<AB2UnityEditorWindow>("AssetBundle转Unity编辑器窗口");
        window.Show();
    }



    public string assetbundleRoot = "../../zhouyou";
    public string assetbundleMain = "../../zhouyou/StreamingAssets";
    public string assetbundleLua = "../../zhouyou/lua/lua.unity3d";
    public string assetbundleSprite = "../../zhouyou/sprite";
    public string assetbundleTexture = "../../zhouyou/texture";

    public string exportLua = "../LuaBytes";
    public string exportResources = "Assets/MResources";
    public string exportAtlasRootPath = "Assets/MResources/Atlas";
    

    private bool fold_test = true;

    void OnGUI()
    {

        EditorGUILayout.BeginVertical();

        GUILayout.Space(20);
        assetbundleMain = EditorGUILayout.TextField("Assetbundle Main:", assetbundleMain, GUILayout.ExpandWidth(true));
        assetbundleRoot = EditorGUILayout.TextField("Assetbundle Root:", assetbundleRoot, GUILayout.ExpandWidth(true));
        assetbundleLua = EditorGUILayout.TextField("Assetbundle Lua:", assetbundleLua, GUILayout.ExpandWidth(true));
        assetbundleSprite = EditorGUILayout.TextField("Assetbundle Sprite:", assetbundleSprite, GUILayout.ExpandWidth(true));
        assetbundleTexture = EditorGUILayout.TextField("Assetbundle Texture:", assetbundleTexture, GUILayout.ExpandWidth(true));
        exportLua = EditorGUILayout.TextField("Export Lua:", exportLua, GUILayout.ExpandWidth(true));
        exportResources = EditorGUILayout.TextField("Export Resources:", exportResources, GUILayout.ExpandWidth(true));
        exportAtlasRootPath = EditorGUILayout.TextField("Export Atlas:", exportAtlasRootPath, GUILayout.ExpandWidth(true));

        GUILayout.Space(20);
        if (GUILayout.Button("清空缓存", GUILayout.Height(30)))
        {
            Caching.CleanCache();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("查看AssetBundleManifest的AssetBundle列表", GUILayout.Height(30)))
        {
            LookAssetBundleManifest_AssetBundleList();
        }


        GUILayout.Space(10);
        if (GUILayout.Button("查看AssetBundleManifest的AssetBundle的依赖列表", GUILayout.Height(30)))
        {
            LookAssetBundleManifest_AssetBundleDependencies();
        }


        GUILayout.Space(10);
        if (GUILayout.Button("查看AssetBundleManifest的AssetBundle的资源列表", GUILayout.Height(30)))
        {
            LookAssetBundleManifest_AssetBundleAssets();
        }


        GUILayout.Space(10);
        if (GUILayout.Button("导出Lua", GUILayout.Height(30)))
        {
            ExportLua();
        }


        GUILayout.Space(10);
        if (GUILayout.Button("导出Sprite", GUILayout.Height(30)))
        {
            ExportSprite();
        }


        GUILayout.Space(10);
        if (GUILayout.Button("设置Sprite九宫格", GUILayout.Height(30)))
        {
            SetSpriteBorder();
        }


        GUILayout.Space(10);
        if (GUILayout.Button("导出UI", GUILayout.Height(30)))
        {
            ExportUI();
        }


        GUILayout.Space(20);

        fold_test = EditorGUILayout.Foldout(fold_test, "测试");
        if (fold_test)
        {
            DrawTest();
        }


        EditorGUILayout.EndVertical();

    }


    public static void CheckPath(string path, bool isFile = true)
    {
        if (isFile) path = path.Substring(0, path.LastIndexOf('/'));
        string[] dirs = path.Split('/');
        string target = "";

        bool first = true;
        foreach (string dir in dirs)
        {
            if (first)
            {
                first = false;
                target += dir;
                continue;
            }

            if (string.IsNullOrEmpty(dir)) continue;
            target += "/" + dir;
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }
        }
    }

    /// <summary>
    /// 查看AssetBundleManifest的AssetBundle列表
    /// </summary>
    void LookAssetBundleManifest_AssetBundleList()
    {
        AssetBundle assetBundleMain = AssetBundle.LoadFromFile(assetbundleMain);
        AssetBundleManifest assetBundleManifest = assetBundleMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] assetBundles = assetBundleManifest.GetAllAssetBundles();
        assetBundleMain.Unload(true);

        string path = "../Logs/AssetBundleManifest的AssetBundle列表.txt";
        CheckPath(path);
        File.WriteAllText(path, String.Join("\n", assetBundles));
        Shell.RevealInFinder(path);

    }



    /// <summary>
    /// 查看AssetBundleManifest的AssetBundle的依赖列表
    /// </summary>
    void LookAssetBundleManifest_AssetBundleDependencies()
    {
        AssetBundle assetBundleMain = AssetBundle.LoadFromFile(assetbundleMain);
        AssetBundleManifest assetBundleManifest = assetBundleMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] assetBundles = assetBundleManifest.GetAllAssetBundles();

        StringWriter sw = new StringWriter();
        for (int i = 0; i < assetBundles.Length; i ++)
        {

            string[] dependencies =  assetBundleManifest.GetAllDependencies(assetBundles[i]);
            sw.WriteLine(assetBundles[i]);
            sw.Write("        " );
            sw.Write(String.Join("\n        ", dependencies));
            sw.WriteLine();
            sw.WriteLine();
        }

        assetBundleMain.Unload(true);
        string path = "../Logs/AssetBundleManifest的AssetBundle的依赖列表.txt";
        CheckPath(path);
        File.WriteAllText(path, sw.ToString());
        Shell.RevealInFinder(path);
    }


    /// <summary>
    /// 查看AssetBundleManifest的AssetBundle的资源列表
    /// </summary>
    void LookAssetBundleManifest_AssetBundleAssets()
    {
        AssetBundle assetBundleMain = AssetBundle.LoadFromFile(assetbundleMain);
        AssetBundleManifest assetBundleManifest = assetBundleMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] assetBundles = assetBundleManifest.GetAllAssetBundles();

        StringWriter sw = new StringWriter();
        for (int i = 0; i < assetBundles.Length; i++)
        {

            string[] dependencies = assetBundleManifest.GetAllDependencies(assetBundles[i]);
            sw.WriteLine(assetBundles[i]);
            if(dependencies.Length > 0)
            {
                sw.WriteLine("[依赖列表]");
                sw.Write("        ");
                sw.Write(String.Join("\n        ", dependencies));
                sw.Write("\n");
            }


            String abpath = Path.Combine(assetbundleRoot, assetBundles[i]);
            if(File.Exists(abpath))
            {
                AssetBundle ab = AssetBundle.LoadFromFile(abpath);
               string[] assetNames = ab.GetAllAssetNames();
                if(assetNames.Length > 0)
                {
                    sw.WriteLine("[资源列表]");
                    sw.Write("        ");
                    sw.Write(String.Join("\n        ", assetNames));
                    sw.WriteLine();
                }

                ab.Unload(false);
            }
            else
            {

                sw.WriteLine("不存在该AssetBundle文件");
            }

            sw.WriteLine();
            sw.WriteLine();
            sw.WriteLine();

        }

        assetBundleMain.Unload(true);
        string path = "../Logs/AssetBundleManifest的AssetBundle的资源列表.txt";
        CheckPath(path);
        File.WriteAllText(path, sw.ToString());
        Shell.RevealInFinder(path);
    }

    /// <summary>
    /// 导出Lua
    /// </summary>
    void ExportLua()
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetbundleLua);
        string[] assetNames = assetBundle.GetAllAssetNames();

        string root = exportLua;
        CheckPath(root, false);
        File.WriteAllText(root + "/文件列表.txt", String.Join("\n", assetNames));
        for (int i = 0; i < assetNames.Length; i ++)
        {
            TextAsset textAsset = assetBundle.LoadAsset<TextAsset>(assetNames[i]);
            string path = root + "/" + assetNames[i];

            CheckPath(path, true);
            File.WriteAllBytes(path, textAsset.bytes);
        }

        Shell.RevealInFinder(root);
        

        assetBundle.Unload(true);
    }


    /// <summary>
    /// 导出Sprite
    /// </summary>
    void ExportSprite()
    {
        ABExportHelper.ExportAssetBundleSpriteFolder(assetbundleSprite, exportResources, exportAtlasRootPath);
        ABExportHelper.ExportAssetBundleSpriteFolder(assetbundleTexture, exportResources, exportAtlasRootPath);
    }



    /// <summary>
    /// 设置Sprite九宫格
    /// </summary>
    void SetSpriteBorder()
    {
        ABExportHelper.SettingSpriteFolder(assetbundleSprite, exportResources, exportAtlasRootPath);
        ABExportHelper.SettingSpriteFolder(assetbundleTexture, exportResources, exportAtlasRootPath);
    }


    /// <summary>
    /// 导出UI
    /// </summary>
    void ExportUI()
    {
        AssetBundle assetBundleMain = AssetBundle.LoadFromFile(assetbundleMain);
        AssetBundleManifest assetBundleManifest = assetBundleMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] assetBundles = assetBundleManifest.GetAllAssetBundles();

        StringWriter sw = new StringWriter();
        for (int i = 0; i < assetBundles.Length; i++)
        {

            string[] dependencies = assetBundleManifest.GetAllDependencies(assetBundles[i]);
            sw.WriteLine(assetBundles[i]);
            sw.WriteLine("        ");
            sw.Write(String.Join("\n        ", dependencies));
            sw.WriteLine();
            sw.WriteLine();
        }

        assetBundleMain.Unload(true);
        string path = "../Logs/AssetBundleManifest的AssetBundle的依赖列表.txt";
        CheckPath(path);
        File.WriteAllText(path, sw.ToString());
        Shell.RevealInFinder(path);
    }
}


