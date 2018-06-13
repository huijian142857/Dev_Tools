using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ihaiu.Editors.AnalyzeProjects
{
    public class AnalyzeProjectEditor
    {
        private static string outRoot = "../AnalyzeProjectAsset/";
        public static string outFile_data { get { return outRoot + "data.json"; } }
        public static string outFile_data_js { get { return outRoot + "data.js"; } }
        public static string outFile_xlsx { get { return outRoot + "资源分析报告"+ DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"; } }


        [MenuItem("Game/Analyze/打开分析报告目录", false, 1)]
        public static void OpenAnalyzeProjectMenu()
        {
            Shell.RevealInFinder(outFile_data);
        }


        [MenuItem("Game/Analyze/AnalyzeSettings", false, 1)]
        public static void GenerateSettings()
        {
            AnalyzeSettings.EditSettings();
        }



        [MenuItem("Game/Analyze/生成分析项目资源", false, 1)]
        public static void GenerateAnalyzeProjectMenu()
        {
            AnalyzeProjectInfo project = new AnalyzeProjectInfo();
            AnalyzeProjectEditorTexture.Report(project);
            AnalyzeProjectEditorMesh.Report(project);


            AnalyzeProjectInfo unitProject = new AnalyzeProjectInfo();
            AnalyzeProjectEditorForUnit.Report(unitProject, AnalyzeSettings.Instance.unitRoots);


            AnalyzeProjectInfo fxProject = new AnalyzeProjectInfo();
            AnalyzeProjectEditorForUnit.Report(fxProject, AnalyzeSettings.Instance.fxRoots);

            AnalyzeProjectInfo uiProject = new AnalyzeProjectInfo();
            AnalyzeProjectEditorForUnit.Report(uiProject, AnalyzeSettings.Instance.uiPrefabRoots);

            project.Dict2List();

            if (!Directory.Exists(outRoot))
            {
                Directory.CreateDirectory(outRoot);
            }


            string json = JsonUtility.ToJson(project, true);

            File.WriteAllText(outFile_data, json);
            File.WriteAllText(outFile_data_js, "var guidData = " + json);

            string xlsx = outFile_xlsx;
            AnalyzeProjectEditorSaveXLSX.Save(xlsx, project, unitProject, fxProject, uiProject);

            xlsx = Path.GetFullPath(xlsx);
            Shell.RevealInFinder(xlsx);
            OpenAssetSettings.Instance.OpenFile(xlsx);

            Debug.Log(outFile_data);
        }


    }
}