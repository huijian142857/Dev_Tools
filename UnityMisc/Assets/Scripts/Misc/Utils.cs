using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Misc
{
    public partial class Utils : Singleton<Utils>
    {
        public static bool IPV4ToIPV6(string ip, out IPAddress ipAddress, out AddressFamily af)
        {
            bool ok = false;
#if UNITY_EDITOR
            Debug.Log("********get ip******** " + ip);
#endif
#if UNITY_IPHONE && !UNITY_EDITOR
            ipAddress = IPAddress.Parse(ip);
            af = AddressFamily.InterNetworkV6;
            if(ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                string newIP;
                if (getIPv6IP(ip, out newIP))
                {
                    Debug.Log(" get ipv6 succeess, old ip address " + ip + " new ip address =  " + newIP);
                    ipAddress = IPAddress.Parse(newIP);
            ok=true;
                }
                else
                {
                    Debug.Log("ios get ipv6 failed");
                    ipAddress = IPAddress.Parse(ip);
                    af = AddressFamily.InterNetwork;
            ok=false;
                }
            }
#else
            ipAddress = IPAddress.Parse(ip);
            af = ipAddress.AddressFamily;
#endif
            return ok;
        }
    }
}

