using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Misc
{
    public partial class Utils
    {
        //是否是同一天
        public static bool IsSameDay(int t1, int t2)
        {
            return (t1 / 86400 == t2 / 86400);
        }

        //28800
        public static bool IsSameDayUTC_8_Hour(int t1, int t2)
        {
            t1 += 28800;
            t2 += 28800;
            return (t1 / 86400 == t2 / 86400);
        }
        public static bool IsSameDayUTC_N_Hour(int t1, int t2, int time_zone)
        {
            t1 += time_zone;
            t2 += time_zone;
            return (t1 / 86400 == t2 / 86400);
        }

        //获得周数  (默认为北京时间)
        public static int GetWeekUTC_8_Hour(int t1, int time_zone = 28800)
        {
            return ((t1 + time_zone) / 86400 - 4) / 7;
        }
        //服务器时间戳(默认为北京时间) 的周数
        public static int GetCurrentServerTimeWeek(int time_zone = 28800)
        {
            //return ((Utils.GetServerTimestampSeconds() + time_zone) / 86400 - 4) / 7;
            return 0;
        }
        //服务器时间戳（默认为北京时间）获取当前周过去的时间
        public static int GetCurrentServerWeekPassTime(int time_zone = 28800)
        {
            //return Utils.GetServerTimestampSeconds() + time_zone - 4 * 86400 - GetCurrentServerTimeWeek(time_zone) * 7 * 86400;
            return 0;
        }
        //指定的 时间戳(服务器时间) 是是否同一周  用于跨周 判定  (默认为北京时间)
        //UTC时间戳 原点 是周4
        public static bool IsSameWeekCurrentServerTime(int t, int time_zone = 28800)
        {
            //return ((Utils.GetServerTimestampSeconds() + time_zone) / 86400 - 4) / 7 == ((t + time_zone) / 86400 - 4) / 7;
            return true;
        }

        public static int GetServerTimeLocalDayOfWeek()
        {
            //返回校对后的本地时间
            var t = DateTime.Now;
            t = t.AddSeconds(-ServerTimeDelta);
            return (int)t.DayOfWeek;
        }
        public static int GetServerTimeLocalDay()
        {
            //返回校对后的本地时间
            var t = DateTime.Now;
            t = t.AddSeconds(-ServerTimeDelta);
            return t.Day;
        }
        public static int GetServerTimeLocalDayOfYear()
        {
            //返回校对后的本地时间
            var t = DateTime.Now;
            t = t.AddSeconds(-ServerTimeDelta);
            return t.DayOfYear;
        }
        public static int GetServerTimeLocalYear()
        {
            //返回校对后的本地时间
            var t = DateTime.Now;
            t = t.AddSeconds(-ServerTimeDelta);
            return t.Year;
        }
        public static int GetServerTimeLocalMonth()
        {
            //返回校对后的本地时间
            var t = DateTime.Now;
            t = t.AddSeconds(-ServerTimeDelta);
            return t.Month;
        }
        public static int GetServerTimeLocalHour()
        {
            //返回校对后的本地时间
            var t = DateTime.Now;
            t = t.AddSeconds(-ServerTimeDelta);
            return t.Hour;
        }
        public static int GetServerTimeLocalSecond()
        {
            //返回校对后的本地时间
            var t = DateTime.Now;
            t = t.AddSeconds(-ServerTimeDelta);
            return t.Second;
        }
        public static DateTime GetServerTimeLocalDateTime()
        {
            //返回校对后的本地时间
            var t = DateTime.Now;
            return t.AddSeconds(-ServerTimeDelta);
        }
        //----------------end of 



        public static int ServerTime = 0;
        public static int ServerTimeDelta = 0;
        static DateTime start_timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        //返回 秒 时间戳
        public static long GetTimestampSeconds()
        {
            TimeSpan ts = DateTime.UtcNow - start_timestamp;
            return (long)ts.TotalSeconds;
        }
        public static int GetTimestampSecondsInt()
        {
            TimeSpan ts = DateTime.UtcNow - start_timestamp;
            return (int)ts.TotalSeconds;
        }
        //返回毫秒时间戳
        public static long GetTimestampMiliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - start_timestamp;
            return (long)ts.TotalMilliseconds;
        }
        public static double GetTimestampMilisecondsD()
        {
            TimeSpan ts = DateTime.UtcNow - start_timestamp;
            return ts.TotalMilliseconds;
        }
        //获取当前本机时间距离 指定时间的秒数
        public static long GetLocalLeftTimeSeconds(DateTime when)
        {
            var ts = when - DateTime.Now;
            return (long)ts.TotalSeconds;
        }
    }
}