/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Reflection;
using System.Net;


namespace EditorBuild
{
    public static class FTP
    {
        [MenuItem("补丁工具/FTP Upload")]
        public static void Upload()
        {
            Debug.LogError(UploadPatch("tmp_svn_diff_out.txt", "web/tmp_svn_diff_out.txt"));
        }

   //     [MenuItem("补丁工具/FTP Upload Android Patch with VERSION.txt")]
          [MenuItem("补丁工具/FTP上传Android补丁包 VERSION.txt")]

        public static void Upload_Android_Patch()
        {
            string version = File.ReadAllText("VersionFiles/VERSION.txt");
            Debug.LogError(UploadPatch("VersionFiles/" + version + "/android_" + version + ".zip", "patch/" + "android_" + version + ".zip"));

            //TODO auto sync cdn status use CDN.API
        }
       // [MenuItem("补丁工具/FTP Upload IOS Patch with VERSION.txt")]
        [MenuItem("补丁工具/FTP上传IOS补丁包 VERSION.txt")]

        public static void Upload_IOS_Patch()
        {
            string version = File.ReadAllText("VersionFiles/VERSION.txt");
            Debug.LogError(UploadPatch("VersionFiles/" + version + "/ios_" + version + ".zip", "patch/" + "ios_" + version + ".zip"));
        }


        public static string UploadPatch(string local_file, string target)
        {
            string error_pre = "FTP Upload local_file=" + local_file + "  error:";

            string error = "";
            string local_md5 = "";
            try
            {
                local_md5 = MD5Code.GetMD5HashFromFile(local_file);
                if (string.IsNullOrEmpty(local_md5))
                {
                    return "local md5 error";
                }
                var ins = File.ReadAllLines("ftp_config.txt");
                string SERVER = ins[0];
                string NAME = ins[1];
                string PWD = ins[2];

                FileInfo f = new FileInfo(local_file);


                var uri = new Uri(SERVER + "/" + target);
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(uri);
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(NAME, PWD);
                reqFtp.KeepAlive = false;
                //   reqFtp.EnableSsl = true;
                reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
                reqFtp.ContentLength = f.Length;

                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                FileStream fs = f.OpenRead();

                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();

                //反向下载 并且检查 文件MD5是否正确
                // Get the object used to communicate with the server.
                WebClient request = new WebClient();

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(NAME, PWD);
                try
                {
                    byte[] newFileData = request.DownloadData(uri.ToString());
                    string fileString = System.Text.Encoding.UTF8.GetString(newFileData);
                    string tmp_file_name = "tmp_ftp_md5_file.zip";
                    EditorUtils.Utils.TryDeleteFile(tmp_file_name);
                    var x = File.Create(tmp_file_name);
                    x.Write(newFileData, 0, newFileData.Length);
                    x.Flush();
                    x.Close();

                    string remote_md5 = MD5Code.GetMD5HashFromFile(tmp_file_name);

                    if (remote_md5 != local_md5)
                    {
                        return error_pre + "remote md5 error re-try-upload-operation local=" + local_md5 + "  remote=" + remote_md5;
                    }
                }
                catch (WebException e)
                {
                    return error_pre + e.ToString();
                }
                error = "ok";
            }
            catch (Exception e)
            {
                error = error_pre + " error " + e.Message;
            }
            return error;

        }
    }
}