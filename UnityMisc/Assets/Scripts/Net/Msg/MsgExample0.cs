using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Msgs
{
    /*
    其他疑问或者设计不合理的地方 请联系 caoshanshan

    你需要做的事情：
        1.重新初始化变量type为你自己想要的消息
        2.如果预制的数据类型(简单数据类型)不满足see MsgCode.cs valid_gen_types 是支持的数据类型列表(请联系coashanshan 或者重写CustomEncode CustomDecode来达到目的)
        3.变量在stream中的顺序就是按照class变量申明的顺序
    注意事项：
        1.要生成代码的变量必须是非下划线开头
        2.如果是List什么的需要自己手动new出来 自动生成的代码不会new
        3.暂不支持嵌套使用 只有 MsgDataObject 能嵌套 如果需要高级数据请用MsgDataObject
        4.对于List等也只能是简答数据类型 如果是复杂的需要自己手动重写CustomDecode CustomEncode
        5.CustomDecode CustomEncode 会在Encode or Decode调用后才会立刻调用 因此要注意stream的顺序
        6.对于未生成的变量 会在Encode函数末尾注释出来
     */
    public class MsgExample0 : CustomMsgObject
    {
        public MsgType type = MsgType.MMSTREAM_TEST_MSG;
        //------------add your own  field in here 


        public int xxx;//满足条件 会自动生成
        public List<float> list = new List<float>();//需要new出来 也会自动生成
        public List<Vector3> lis1111t = new List<Vector3>();//需要new出来 也会自动生成
        public List<Quaternion> lis2312323t = new List<Quaternion>();//需要new出来 也会自动生成

        public float _name;//由于下划线开头 不会自动生成
        public Dictionary<string, float> kv = new Dictionary<string, float>();//不支持的数据类型 不会自动生成
        private float fqfffqf;// 会自动生成
        public Vector3 xxxxxx;
        public Quaternion plsqsfq;

        public string my_name;

        //------------end of your own field

        //////////////////add your own  method in here 



        //////////////////end of your method field

        //if you want to encode custom member or unsuport member please write your code here
        //会在Encode之后才调用
        public void CustomEncode(MsgStream ms)
        {

        }
        //会在Decode调用之后才会调用
        public void CustomDecode(MsgStream ms)
        {

        }
        /////////////-----------dont modify follow code your modifications will be loss
        /////////////-----------dont modify follow code your modifications will be loss
        /////////////-----------dont modify follow code your modifications will be loss
        /////////////-----------dont modify follow code your modifications will be loss
        /////////////-----------dont modify follow code your modifications will be loss
        /////////////-----------dont modify follow code your modifications will be loss    



        
        //---Auto Generate Code Start---
        //把该类的field 写入到stream中
        public void Encode(MsgStream ms)
        {
            ms.stream.WriteByte((byte)this.type);
            ms.mType = ((MsgType)this.type);
            ms.Write(this.xxx);
            ms.Write((short)this.list.Count);
            int ____COUNT___list = list.Count;
            for(int i=0;i<____COUNT___list;i++)
            {
                ms.Write(this.list[i]);
            };
            ms.Write((short)this.lis1111t.Count);
            int ____COUNT___lis1111t = lis1111t.Count;
            for(int i=0;i<____COUNT___lis1111t;i++)
            {
                ms.Write(this.lis1111t[i]);
            };
            ms.Write((short)this.lis2312323t.Count);
            int ____COUNT___lis2312323t = lis2312323t.Count;
            for(int i=0;i<____COUNT___lis2312323t;i++)
            {
                ms.Write(this.lis2312323t[i]);
            };
            ms.Write(this.xxxxxx);
            ms.Write(this.plsqsfq);
            ms.Write(this.my_name);
            //field name=  type    type=  MsgType     has not auto-gen
            //field name=  _name    type=  Single     has not auto-gen
            //field name=  kv    type=  Dictionary`2     has not auto-gen
        }
        //从stream中读取数据以初始化field
        public void Decode(MsgStream ms)
        {
            this.xxx = ms.Int;
            this.list.Clear();
            int ____COUNT___list = ms.Short;
            for(int i=0;i<____COUNT___list;i++)
            {
                this.list.Add(ms.Float);
            };
            this.lis1111t.Clear();
            int ____COUNT___lis1111t = ms.Short;
            for(int i=0;i<____COUNT___lis1111t;i++)
            {
                this.lis1111t.Add(ms.Vector3);
            };
            this.lis2312323t.Clear();
            int ____COUNT___lis2312323t = ms.Short;
            for(int i=0;i<____COUNT___lis2312323t;i++)
            {
                this.lis2312323t.Add(ms.Quaternion);
            };
            this.xxxxxx = ms.Vector3;
            this.plsqsfq = ms.Quaternion;
            this.my_name = ms.String;
        }
        //---Auto Generate Code End---
    }
}