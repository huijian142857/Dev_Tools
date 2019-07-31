/*
* Author:  tangyanzhong
* Email:   rellikt@qq.com
*/

using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;


public class BuildLogUtil
{
    public class AssetInfo
    {
        public string name;
        public float size;
    }

    [MenuItem("配置工具/出包日志分析", false, 100)]
    public static void AnalysisBuildLog()
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)      
        var logPath = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("LocalLow"));
        logPath += "Local/Unity/Editor/Editor.log";
        var logContent = File.ReadAllLines(logPath);
        List<string> warningAssetsLst = new List<string>();
        List<float> sizeLst = new List<float>();
        string head = "每次生成日志前，请先关闭编辑器，然后再打开编辑器，然后选择全部打包";
        warningAssetsLst.Add(head);
        sizeLst.Add(10000f);
        bool isBuildLogStarted = false;
        for (int i = 0; i < logContent.Length; i++)
        {
            if (!isBuildLogStarted && logContent[i].StartsWith("Bundle Name"))
                isBuildLogStarted = true;
            if (isBuildLogStarted && logContent[i].StartsWith("Refresh:"))
                isBuildLogStarted = false;
            float size = 0;
            if (isBuildLogStarted)
            {
                bool marked = false;
                if (logContent[i].StartsWith(" "))
                {
                    var contents = logContent[i].Split(' ');
                    if (contents.Length > 2)
                    {
                        if (contents[2].StartsWith("mb"))
                        {
                            size = float.Parse(contents[1]);
                            marked = true;
                        }
                    }
                }
                if (marked)
                {
                    sizeLst.Add(size);
                    warningAssetsLst.Add(logContent[i]);
                }
            }
        }
        var assetInfoLst = new List<AssetInfo>();
        for (int i = 0; i < warningAssetsLst.Count; i++)
        {
            assetInfoLst.Add(new AssetInfo { name = warningAssetsLst[i], size = sizeLst[i] });
        }
        assetInfoLst.Sort((x, y) => {
            return (x.size - y.size > 0) ? 1 : -1;
        });
        var retLst = new List<string>();
        foreach (var info in assetInfoLst)
            retLst.Add(info.name);
        File.WriteAllLines("../大于1mb的资源.txt", retLst.ToArray());
        Debug.Log("finish output ab log:" + Application.dataPath + "/../大于1mb的资源.txt");
        Debug.Log("applicaton log path:" + logContent.Length);
#endif
    }

    [MenuItem("配置工具/SetImage日志分析", false, 101)]
    public static void AnalysisInfoLog()
    {
        var logPath = Application.dataPath + "/../../SetImageInfo.txt";
        var outputPath = Application.dataPath + "/../../SetImageLua.txt";
        Debug.Log("set log path:" + logPath);
        var lines = File.ReadAllLines(logPath);
        var lst = new List<string>(lines);
        lst.RemoveAll(it => !it.Contains("SetImage("));
        for (int i = 0; i < lst.Count; i++)
        {
            var item = lst[i];
            if (item.Contains(","))
            {
                var items = item.Split(',');
                var str = items[items.Length - 1];
                str = str.Trim();
                //Debug.Log(str);
                if (str.Contains("\""))
                {
                    str = str.Substring(str.IndexOf('"'));
                    str = str.Substring(0, str.LastIndexOf('"') + 1);
                    lst[i] = str;
                }
                else
                {
                    //lst[i] = str.Substring(0, str.LastIndexOf(')'));
                }
            }            
        }        
        File.WriteAllLines(outputPath, lst.ToArray());
        Debug.Log("finish process setimageinfo!");
    }
}
