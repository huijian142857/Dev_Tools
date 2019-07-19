# coding=utf-8
# ---------------------------------------------
# Name: build-client-ldhcr
# Author: YangQijun
# Create：20/08/2018
# Description：Learn from GRSM， Thanks zouxulu
# ---------------------------------------------
import os
import sys
import time
import shutil
import re
import getopt
parent_path = os.path.realpath(os.path.dirname(__file__))
sys.path.append(parent_path)
from buildutil import Util

class Build(Util):
    def __init__(self):
        super(Build, self).__init__()
        self.PROJECT_AB_PATH = os.path.join(self.PROJECT_PATH, "AssetBundles")
        self.PROJECT_PACKLOG_PATH = os.path.join(self.PROJECT_PATH, "packlog.txt")
        self.PROJECT_BUILDLOG_PATH = os.path.join(self.PROJECT_PATH, "buildlog.txt")
        self.PROJECT_STREAMINGASSETS = os.path.join(self.PROJECT_PATH,"Assets","StreamingAssets")
        self.APK_PATH = os.path.join(self.PROJECT_PATH, "RuntimeBuild", "Apk_IL2CPP")
        self.UNITY_PATH = '"C:\\Program Files\\Unity\\Editor\\Unity.exe"' if  sys.platform.startswith('win32') else "/Applications/Unity/Unity.app/Contents/MacOS/Unity"

    def clearproject(self):
        self.remove_if_exists(self.PROJECT_AB_PATH)
        self.remove_if_exists(self.PROJECT_TEMP_PATH)
        self.remove_if_exists(self.PROJECT_PACKLOG_PATH)
        self.remove_if_exists(self.APK_PATH)

        self.printgreen("Finish clear Project!")

    def svnup(self):
        cmd_clean = "svn cleanup %s" % self.PROJECT_ROOT
        cmd_revert = "svn revert -R %s" %self.PROJECT_PATH
        cmd_svnup = "svn up %s --username yangqijun --password 711244148232" %self.PROJECT_PATH
        os.system(cmd_clean)
        os.system(cmd_revert)
        os.system(cmd_svnup)

        self.printgreen("Svn Up Success!")

    def packab(self,platform):
        """ package ab source，using Unity Command Line Tools """
        self.printblue("Start package ab file, Please wait!")
        starttime = int(time.time())
        cmd ="%s %s -batchmode -projectPath %s  -quit -executeMethod %s -logFile %s" \
             %(self.UNITY_PATH, platform, self.PROJECT_PATH, 'EditorBuild.BuildAssetBundle._BuildAssetBundle_Android', self.PROJECT_PACKLOG_PATH)
        retcode = os.system(cmd)
        endtime = int(time.time())
        usetime = self.usetime(starttime,endtime)
        if retcode != 0:
            self.unityexceptioncall()
            self.printred("EditorBuild.BuildAssetBundle._BuildAssetBundle_Android failed!")
        else:
            self.printgreen("BuildAssetBundle_Android successed! Use Time %s" %usetime)

        return retcode

    def devconfig(self, buildconf='debug'):
        """ Modify the devconfig.lua file """
        self.printblue("Start Modify the devconfig.lua file --> build mode:%s!" %buildconf)
        devconfig = os.path.join(self.PROJECT_PATH, "Assets", "RawResources", "Lua", "DevConfig.lua")
        devconfig_bak = os.path.join(self.PROJECT_PATH, "Assets", "RawResources", "Lua", "DevConfig_bak.lua")
        regex = re.compile("^t.IS_DEBUG")
        shutil.copyfile(devconfig, devconfig_bak)

        # modify t.IS_DEBUG
        if sys.platform.startswith("win32"):
            # python 3.6.4
            with open(devconfig, 'r', encoding="utf-8") as f1, open(devconfig_bak, "w") as f2:
                if buildconf == 'debug':
                    for line in f1:
                        if re.match(regex, line) and ('false' in line):
                            line = line.replace('false', 'true')
                        f2.write(line)

                elif buildconf == 'release':
                    for line in f1:
                        if re.match(regex, line) and ('true' in line):
                            line = line.replace('true', 'false')
                        f2.write(line)
        else:
            with open(devconfig, 'r') as f1, open(devconfig_bak, "w") as f2:
                if buildconf == 'debug':
                    for line in f1:
                        if re.match(regex, line) and ('false' in line):
                            line = line.replace('false', 'true')
                        f2.write(line)

                elif buildconf == 'release':
                    for line in f1:
                        if re.match(regex, line) and ('true' in line):
                            line = line.replace('true', 'false')
                        f2.write(line)
        f1.close()
        f2.close()

        # rename file
        os.remove(devconfig)
        shutil.move(devconfig_bak, devconfig)

    def copyabtostreamassert(self):
        """ copy ab files to streamingassets """
        if os.path.exists(self.PROJECT_STREAMINGASSETS):
            shutil.rmtree(self.PROJECT_STREAMINGASSETS)
        os.mkdir(self.PROJECT_STREAMINGASSETS)

        if os.path.exists(self.PROJECT_AB_PATH):
            shutil.move(self.PROJECT_AB_PATH,self.PROJECT_STREAMINGASSETS)

    def build_android(self, buildconf):
        """ build apk il2cpp"""
        self.printblue("Start build apk!")
        self.remove_if_exists(self.PROJECT_BUILDLOG_PATH)
        starttime = int(time.time())
        cmd = '%s -%s -android -batchmode -projectPath %s -logFile %s -executeMethod %s -quit' \
              %(self.UNITY_PATH,buildconf.lower(),self.PROJECT_PATH,self.PROJECT_BUILDLOG_PATH,"EditorBuild.RuntimeMaker.BuildApkIL2CPP")
        retcode = os.system(cmd)
        endtime = int(time.time())
        usetime = self.usetime(starttime, endtime)
        if retcode != 0:
            self.unityexceptioncall()
            self.printred("EditorBuild.RuntimeMaker.BuildApkIL2CPP build failed!")
        else:
            self.printgreen("BuildApkIL2CPP build successed! Use Time %s" % usetime)

        return retcode

if __name__ == "__main__":
    build_client = Build()
    if "clear" in sys.argv:
        build_client.clearproject()
    if "svnup" in sys.argv:
        build_client.svnup()
    if "copyabtostreamassert" in sys.argv:
        build_client.copyabtostreamassert()
    if "devconfig"in sys.argv:
        try:
            opts, args = getopt.getopt(sys.argv[1:],"",["buildconf="])
        except getopt.GetoptError:
            sys.exit(1)
        buildconf = opts[0][1]  # [('--buildconf', 'release')]
        build_client.devconfig(buildconf=buildconf)

    if "ptab_android" in sys.argv:
        retcode = build_client.packab("Android")
        if retcode != 0:
            retcode = 1
        sys.exit(retcode)
    if "build_android" in sys.argv:
        try:
            opts, args = getopt.getopt(sys.argv[1:],"",["buildconf="])
        except getopt.GetoptError:
            sys.exit(1)

        buildconf = opts[0][1] #[('--buildconf', 'release')]
        retcode = build_client.build_android(buildconf)
        if retcode != 0:
            retcode = 1
        sys.exit(retcode)
