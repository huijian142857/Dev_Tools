# coding=utf-8
# ---------------------------------------------
# Name: build-client-ldhcr
# Author: YangQijun
# Create：21/08/2018
# Description：Learn from GRSM， Thanks zouxulu
# ---------------------------------------------
import os
import sys
import re
import shutil
import time
parent_path = os.path.realpath(os.path.dirname(__file__))
sys.path.append(parent_path)
from buildclient import Build

class ReNameApk(Build):
    def __init__(self):
        super(ReNameApk, self).__init__()

    def getversion(self):
        """ get the lastest svn log version """
        cmd = "svn log -l1 %s --username yangqijun --password 711244148232" % self.PROJECT_PATH
        version = ''
        regex = re.compile("^r\d+")
        log = os.popen(cmd)
        for line in log:
            if re.match(regex,line):
                version = re.findall(regex,line)[0].lstrip("r")

        return version

    def rename(self):
        """ rename the apk """
        version = self.getversion()
        localtime = time.strftime("%Y-%m-%d-%H-%M-%S", time.localtime())
        file_name = ''

        if os.path.exists(self.APK_PATH):
            for root,dirs,files in os.walk(self.APK_PATH):
                if files != []:
                    for file in files:
                        if 'apk' in file:
                            file_name = file
                else:
                    print("There no apk files!")
                    sys.exit(1)
        old_file = os.path.join(self.APK_PATH, file_name)
        new_file_name = "ldhcr_" + version + '_' + localtime + '.apk'
        new_file = os.path.join(self.APK_PATH, new_file_name)
        shutil.move(old_file,new_file)

        return new_file

    def copyapktoshare(self,file):
        self.share_path = '/Volumes/share/hcr'
        if not os.path.exists(self.share_path) and os.path.exists(file):
            os.mkdir(self.share_path)

        cmd_copy = 'xcopy /y' if sys.platform.startswith('win32') else 'cp'
        os.system("{0} {1} {2}".format(cmd_copy,file,self.share_path))


if __name__ == "__main__":
    apk = ReNameApk()
    file = apk.rename()
    if not sys.platform.startswith("win32"):
        apk.copyapktoshare(file=file)