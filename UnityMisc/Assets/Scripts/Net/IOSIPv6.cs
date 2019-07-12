using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IOSIPv6
{
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string IOSGetAddressInfo(string host );  
        /*
        public static IPAddress[] ResolveIOSAddress(string host, out AddressFamily af)
        {
            af = AddressFamily.InterNetwork;
            var outstr = IOSGetAddressInfo(host);
            Debug.Log("IOSGetAddressInfo"  + outstr);
            if (outstr.StartsWith ("ERROR")) 
            {
                return null;
            }
            var addressliststr = outstr.Split('|');
            var addrlist = new List<IPAddress>();
            foreach (string s in addressliststr)
            {
                if (String.IsNullOrEmpty(s.Trim()))
                    continue;
                switch( s )
                {
                    case "ipv6":
                        {                        
                            af = AddressFamily.InterNetworkV6;
                        }
                        break;
                    case "ipv4":
                        {
                            af = AddressFamily.InterNetwork;
                        }
                        break;
                    default:
                        {
                            addrlist.Add(IPAddress.Parse(s));
                        }
                        break;
                }
            }
            return addrlist.ToArray();
        }
        */


        private static bool getIPv6IP(string ip , out string newIP)
        {
            var outstr = IOSGetAddressInfo(ip);
            Debug.Log("IOSGetAddressInfo"  + outstr);
            if (outstr.StartsWith ("ERROR")) 
            {
                newIP = ip;
                return false;
            }

            string[] addressliststr = outstr.Split('|');
            for (int i = 0 ; i < addressliststr.Length ; i++)
            {
                string s = addressliststr[i];

                if (String.IsNullOrEmpty(s.Trim()))
                    continue;

                if(s == "ipv6")
                {
                    newIP = addressliststr[i + 1];
                    return true ;
                }
            }

            newIP = ip ;
            return false ;
        }
#endif

    // add ipv6 support for ios
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