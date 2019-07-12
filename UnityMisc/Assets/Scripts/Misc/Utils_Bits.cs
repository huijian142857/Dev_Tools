using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//位运算支持
//只支持 31位的位运算
public partial class Utils_Bits
{
    //------------------------------base-operation
    //------------------------------base-operation
    //------------------------------base-operation
    public static int AND(int a, int b)
    {
        return (a & b) & 0x7fffffff;
    }
    public static int OR(int a, int b)
    {
        return (a | b) & 0x7fffffff;
    }
    public static int NOT(int a)
    {
        return (~a) & 0x7fffffff;
    }
    public static int XOR(int a, int b)
    {
        return (a ^ b) & 0x7fffffff;
    }
    //右移
    public static int SHIFTR(int a, int bit_num)
    {
        return (a >> bit_num) & 0x7fffffff;
    }
    //左移
    public static int SHIFTL(int a, int bit_num)
    {
        return (a << bit_num) & 0x7fffffff;
    }

    //----------------------------helper-function
    //----------------------------helper-function
    //----------------------------helper-function
    //指定 比特位 是否是0
    //@param bit_num start with 0
    public static bool BitIsZero(int a, int bit_num)
    {
        int num = 1;
        num <<= bit_num;
        return (num & a) == 0;
    }
    //设置比特位为0
    //@param bit_num start with 0
    public static int SetBitZero(int a, int bit_num)
    {
        int num = 1;
        num <<= bit_num;
        return (((~num) & a) & 0x7fffffff);
    }
    //设置比特位为1
    //@param bit_num start with 0
    public static int SetBitOne(int a, int bit_num)
    {
        int num = 1;
        num <<= bit_num;
        return ((num | a) & 0x7fffffff);
    }
}
