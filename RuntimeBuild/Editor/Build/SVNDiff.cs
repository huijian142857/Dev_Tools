/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using UnityEditor;
using System.IO;
namespace EditorBuild
{
    // 先输出到AssetBundl 目录 然后检查 SVN文件状态 然后提取非 missing 和 delete的文件 copy到临时目录
    //开始对该目录打包
    public static class SVNDiff
    {
        [MenuItem("补丁工具/CheckSvn", false, 2002)]
        public static void CheckFiles()
        {
            string path_pre_name = "VersionFiles/AssetBundles/Android/";

            string path = EditorUtils.Utils.GetCurrentWorkingPath() + "/VersionFiles/AssetBundles/Android/AssetBundles";

            File.Delete("out.txt");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            string appPath = "Assets/Editor/Build/SVN/SvnDiffDLL.exe";
            process.StartInfo.FileName = appPath;
            process.StartInfo.Arguments = "run " + path + " out.txt";    //"process " + i.ToString();

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.StartInfo.RedirectStandardOutput = false;
            process.Start();
            if (process.WaitForExit(30000))//30s
            {//ok  read file

                var files = File.ReadAllLines("out.txt");
                //拿到diff文件后 输出到另外的目录 准备打包
                if (files.Length > 0)
                {
                    if (Directory.Exists(path_pre_name + "AssetBundles_diff"))
                    {
                        Directory.Delete(path_pre_name + "AssetBundles_diff", true);
                    }
                    Directory.CreateDirectory(path_pre_name + "AssetBundles_diff");

                    //先输出完整目录
                    //    


                    var tmp_dirs = Directory.GetDirectories(path_pre_name + "AssetBundles", "*", SearchOption.AllDirectories);
                    foreach (var p in tmp_dirs)
                    {
                        Debug.LogError("1111 " + p);

                        Directory.CreateDirectory(p.Replace(path_pre_name + "AssetBundles", path_pre_name + "AssetBundles_diff"));
                    }
                    //先copy到临时目录

                    foreach (var p in files)
                    {
                        string t = p;
                        if (Path.HasExtension(p))
                        {
                            string pre = p.Substring(0, p.IndexOf("AssetBundles"));
                            string aft = p.Substring(p.IndexOf("AssetBundles") + "AssetBundles".Length + 1, p.Length - p.IndexOf("AssetBundles") - "AssetBundles".Length - 1);
                            Debug.LogError(pre + "  ...2222.....  " + aft);

                            t = pre + path_pre_name + "AssetBundles_diff/" + aft;
                        }
                        else
                        {
                            if (Path.GetFileName(p) == Path.GetFileNameWithoutExtension(p))
                            {
                                string pre = p.Substring(0, p.IndexOf("AssetBundles"));
                                string aft = p.Substring(p.IndexOf("AssetBundles") + "AssetBundles".Length + 1, p.Length - p.IndexOf("AssetBundles") - "AssetBundles".Length - 1);
                                Debug.LogError(pre + "  ...111.....  " + aft);

                                t = pre + path_pre_name + "AssetBundles_diff/" + aft;
                            }

                        }

                        Debug.LogError(p);
                        Debug.LogError(t);
                        Debug.LogError("...........");
                        File.Copy(p, t);

                    }
                    EditorUtils.Utils.RemoveEmptyDir(path_pre_name + "AssetBundles_diff");

                    //找出差异 文件  并且复制到临时目录后 可以开始打包流程了



                }
                else
                {
                    Debug.LogError("svn diff file is empty");
                    return;
                }
            }
            else
            {
                Debug.LogError("run exe error ");
                return;
            }

            AssetDatabase.Refresh();
        }



        [MenuItem("补丁工具/Check_Svn_Diff_Files android", false, 2003)]
        public static void Check_Svn_Diff_Files()
        {
            string target_path_pre_name = "VersionFiles/AssetBundles/Android/";
            string path = EditorUtils.Utils.GetCurrentWorkingPath() + "/VersionFiles/AssetBundles/Android/AssetBundles";
            string tmp_out_file_path = "tmp_svn_diff_out.txt";

            CheckFiles(path, target_path_pre_name, tmp_out_file_path);
        }


        //
        public static void CheckFiles(string path, string target_path_pre_name, string tmp_out_file_path)
        {
            /* string path_pre_name = "VersionFiles/AssetBundles/Android/";
             string path = "C:/Users/caoshanshan/Desktop/demo_cat/stick/Develop/UnityClient2/UnityClient/VersionFiles/AssetBundles/Android/AssetBundles";*/
            //tmp file out.txt
            File.Delete(tmp_out_file_path);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            string appPath = "Assets/Editor/Build/SVN/SvnDiffDLL.exe";
            process.StartInfo.FileName = appPath;
            process.StartInfo.Arguments = "run " + path + " " + tmp_out_file_path;    //"process " + i.ToString();

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.StartInfo.RedirectStandardOutput = false;
            process.Start();
            if (process.WaitForExit(300000))//300s
            {//ok  read file

                var files = File.ReadAllLines(tmp_out_file_path);
                //拿到diff文件后 输出到另外的目录 准备打包
                if (files.Length > 0)
                {
                    if (Directory.Exists(target_path_pre_name + "AssetBundles_diff"))
                    {
                        Directory.Delete(target_path_pre_name + "AssetBundles_diff", true);
                    }
                    Directory.CreateDirectory(target_path_pre_name + "AssetBundles_diff");

                    var tmp_dirs = Directory.GetDirectories(target_path_pre_name + "AssetBundles", "*", SearchOption.AllDirectories);
                    foreach (var p in tmp_dirs)
                    {
                        Directory.CreateDirectory(p.Replace(target_path_pre_name + "AssetBundles", target_path_pre_name + "AssetBundles_diff"));
                    }
                    //先copy到临时目录

                    foreach (var p in files)
                    {
                        if (Directory.Exists(p))
                        {
                            //is dir ignore
                            foreach(var pp in  Directory.GetFiles(p,"*", SearchOption.AllDirectories))
                            {
                                if(Directory.Exists(pp))
                                {
                                    Debug.LogError("error dir has sub dir just not suport please check it");
                                    return;
                                }
                                else
                                {
                                    string pre1 = path.Substring(0, path.Length - "AssetBundles".Length);
                                    string last1 = pp.Substring(path.Length, pp.Length - path.Length);
                                    File.Copy(pp, pre1 + "AssetBundles_diff" + last1);
                                }
                            }
                            continue;
                        }
                        string pre = path.Substring(0, path.Length - "AssetBundles".Length);
                        string last = p.Substring(path.Length, p.Length - path.Length);
                        File.Copy(p, pre + "AssetBundles_diff" + last);
                    }
                    EditorUtils.Utils.RemoveEmptyDir(target_path_pre_name + "AssetBundles_diff");
                }
                else
                {
                    Debug.LogError("svn diff file is empty");
                    return;
                }
            }
            else
            {
                Debug.LogError("run exe error ");
                return;
            }

            Debug.Log("SVNDiff: check done and copy file to AssetBundles_diff");
        }




    }
}