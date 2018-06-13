using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public partial class AB2UnityEditorWindow
{
    string assetbundleTestSprite = "sprite/bisaidengji.unity3d";
    string assetbundleTestUI = "ui/audit_2dbuild.unity3d";
    AssetBundle testAssetBundle;
    AssetBundle testAssetBundleManifest;
    AssetBundleManifest testManifest;
    List<AssetBundle> testAssetBundleList = new List<AssetBundle>();
    void DrawTest()
    {

        GUILayout.Space(20);

        if (GUILayout.Button("assetBundle Unload", GUILayout.Height(30)))
        {
            TestUnload();
        }


        if (GUILayout.Button("Load AssetBundleManifest", GUILayout.Height(30)))
        {
            TestLoadAssetBundleManifest();
        }

        GUILayout.Space(20);
        assetbundleTestSprite = EditorGUILayout.TextField("assetbundleTestSprite:", assetbundleTestSprite, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("测试Sprite", GUILayout.Height(30)))
        {
            TestSprite();
        }


        GUILayout.Space(20);
        assetbundleTestUI = EditorGUILayout.TextField("assetbundleTestUI:", assetbundleTestUI, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("测试UI", GUILayout.Height(30)))
        {
            TestUI();
        }

    }

    void TestUnload()
    {


        if (testAssetBundleManifest != null)
        {
            testAssetBundleManifest.Unload(true);
            testAssetBundleManifest = null;
        }

        for(int i = 0; i < testAssetBundleList.Count; i ++)
        {
            if(testAssetBundleList[i] != null)
                testAssetBundleList[i].Unload(true);
        }
        testAssetBundleList.Clear();
    }


    void TestLoadAssetBundleManifest()
    {
        testAssetBundleManifest = AssetBundle.LoadFromFile(assetbundleMain);
        testManifest = testAssetBundleManifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    void TestCheckAssetBundleManifest()
    {
        if (testManifest == null)
            TestLoadAssetBundleManifest();
    }

    void TestSprite()
    {



        string abpath = assetbundleRoot + "/" + assetbundleTestSprite;

        AssetBundle assetBundle = AssetBundle.LoadFromFile(abpath);
        ABExportHelper.ExportAssetBundleMultipleSprite(assetBundle, exportResources);

        string[] assetNames = assetBundle.GetAllAssetNames();
        for (int i = 0; i < assetNames.Length; i++)
        {
            string name = assetNames[i];
            UnityEngine.Object obj = assetBundle.LoadAsset<UnityEngine.Object>(name);
            Sprite sprite = obj as Sprite;


            if (sprite != null)
            {
                GameObject go = new GameObject(name);
                SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;

                //string path = resources + "/" + assetbundleTestSprite.Replace(".unity3d", ".png");
                //ABExportHelper.ExportSpriteAtlas(sprite, path);
                //Debug.Log(name + ", " + sprite.associatedAlphaSplitTexture.name + ", " + path);
                //break;
            }
        }

        assetBundle.Unload(false);
    }



    void TestUI()
    {
        TestCheckAssetBundleManifest();
        string[] dependencies = testManifest.GetAllDependencies(assetbundleTestUI);
        Dictionary<UnityEngine.Object, string> abObjectDict = new Dictionary<UnityEngine.Object, string>();
        for (int i = 0; i < dependencies.Length; i++)
        {
            string path = assetbundleRoot + "/" + dependencies[i];
            Debug.Log("依赖:" + path);
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            if (ab != null)
            {
                testAssetBundleList.Add(ab);
                string[] names = ab.AllAssetNames();
                foreach(string name in names)
                {
                    UnityEngine.Object o = ab.LoadAsset<UnityEngine.Object>(name);
                    if(o != null)
                        abObjectDict.Add(o, name);
                }
            }
            else
                Debug.LogError("不存在:" + path);
        }

        GameObject canvas = GameObject.Find("Canvas");
        Transform canvasTransform = canvas != null ? canvas.transform : null;


        string abpath = assetbundleRoot + "/" + assetbundleTestUI;
        testAssetBundle = AssetBundle.LoadFromFile(abpath);

        testAssetBundleList.Add(testAssetBundle);

        string[] assets = testAssetBundle.GetAllAssetNames();

        for(int i = 0; i < assets.Length; i ++)
        {
            string name = assets[i];
            GameObject prefab = testAssetBundle.LoadAsset<GameObject>(name);

            if(prefab != null)
            {
                GameObject go = GameObject.Instantiate<GameObject>(prefab);
                if (canvasTransform != null)
                    go.transform.SetParent(canvasTransform, false);

                Image[] images = go.GetComponentsInChildren<Image>();
                for(int j = 0; j < images.Length; j ++)
                {
                    Image image = images[j];
                    if (image.sprite == null) continue;
                    string p = abObjectDict[image.sprite];
                    Debug.LogFormat("{0} node:{1}   name:{2}    path:{3}", j, image.name, image.sprite.name, p);

                    p = exportResources + "/" + p;
                    if(File.Exists(p))
                    {
                        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                    }
                    else
                    {
                        Debug.LogError("不存在文件:" + p);
                    }


                    //Debug.LogFormat("{0}    InstanceID:{1}  HashCode:{2}    node:{3}    name:{4}    path:{5}", j, image.sprite.GetInstanceID(), image.sprite.GetHashCode(), image.name, image.sprite.name,  name);
                }
            }
        }

    }
}


