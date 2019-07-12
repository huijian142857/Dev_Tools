using System;
using System.Linq;
using System.IO;
using UnityEngine;


public class XOREncrypt
{
    static byte[] key_encode = new byte[] { 89, 55, 47, 12, 53, 35, 67, 45 };
    static byte[] key_decode = new byte[] { 100, 32, 74, 11, 5, 89, 65, 67 };

    const int MISC_KEY = 771911064;
    public static void Encrypt(byte[] data, int size, int offset = 0)
    {
        int i = offset;
        int k = 0;
        for (; i < size; i++)
        {
            byte n = (byte)((i - offset) % 7 + 1); //移位长度
            byte b = (byte)((byte)(data[i] << n) | (byte)((data[i] >> (8 - n)))); // 向左循环移位
            data[i] = (byte)(b ^ key_encode[k]);
            k = ++k % 8;
        }
    }

    public static void Decrypt(byte[] data, int size, int offset = 0)
    {
        int i = offset;
        int k = 0;
        for (; i < size; i++)
        {
            byte b = (byte)(data[i] ^ key_decode[k]);
            byte n = (byte)((i - offset) % 7 + 1); //移位长度
            data[i] = (byte)((byte)(b >> n) | (byte)(b << (8 - n))); // 向右循环移位
            k = ++k % 8;
        }
    }
}
