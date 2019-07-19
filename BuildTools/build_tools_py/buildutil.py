# coding=utf-8
# ---------------------------------------------
# Name: build-client-ldhcr
# Author: YangQijun
# Create：20/08/2018
# Description：Learn from GRSM， Thanks zouxulu
# ---------------------------------------------
import os
import sys
import shutil
import time

class Util(object):
    """
        class for util method!
    """

    def __init__(self):
        self.PROJECT_ROOT = os.path.join(os.path.dirname(os.path.dirname(os.path.realpath(os.path.dirname(__file__)))),"Develop")
        self.PROJECT_PATH = os.path.join(self.PROJECT_ROOT, "UnityClient", "UnityClient")
        self.PROJECT_TEMP_PATH = os.path.join(self.PROJECT_PATH, "Temp")
        self.PROCESS_NAME = "Unity.exe" if sys.platform.startswith('win32') else 'Unity'

    def remove_if_exists(self,path):
        """ if file or dir is exists, remove it! """
        if os.path.exists(path) and os.path.isfile(path):
            os.remove(path)
        elif os.path.exists(path) and os.path.isdir(path):
            shutil.rmtree(path)
        else:
            # not find path
            pass

    def unityexceptioncall(self):
        """
            deal with exception:
            1.kill Unity process
            2.remove the Temp dir
        """
        self.processkill(self.PROCESS_NAME)
        curtime = int(time.time())
        while True: # 1s
            if int(time.time()) - curtime > 0:
                break
        self.remove_if_exists(self.PROJECT_TEMP_PATH)

    def processkill(self,process):
        """ kill process """
        if sys.platform.startswith("win32"):
            cmd = "taskkill /f /fi 'imagename eq %s'" %process
            os.system(cmd)
        else:
            os.system("ps -u 'yangqijun' | grep -v PID | grep '%s' |grep -v grep | grep -v MonoDevelop| awk '{print $1}' | xargs kill -9" % (self.PROCESS_NAME))

    def usetime(self,starttime,endtime):
        usetime = endtime - starttime
        if  usetime <= 60:
            usetime = str(usetime) + "s"

            return usetime
        elif 60 < usetime <= 3600:
            minute = usetime // 60
            second = usetime % 60
            if second == 0:
                usetime = str(minute) + "m"
            else:
                usetime = str(minute) + 'm' + str(second) + 's'

            return usetime
        elif usetime > 3600:
            hour = usetime // 3600
            minute = (usetime % 3600) // 60
            if minute == 0:
                usetime = str(hour) + 'h'
            else:
                usetime = str(hour) + 'h' + str(minute) + 'm'

            return usetime

    def printred(self,msg):
        print("\033[31;1m%s 033[0m" %msg)

    def printgreen(self,msg):
        print("\033[32;1m%s\033[0m" %msg)

    def printblue(self,msg):
        print("\033[34;1m%s\033[0m" %msg)
