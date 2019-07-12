using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Msgs
{
    /*
    //!!!!! 暂时不要使用 MsgDataObject 因为对于服务器的代码自动生成还未处理 不然服务器开发的 看起来很别扭
    
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
    public class MsgPlayerStatus : CustomMsgObject
    {
        public MsgType type = MsgType.UPDATE_PLAYER_STATUS;
        //------------add your own  field in here 
        public int id;
        public Vector3 position;
        public float yValue;
        public byte move;
        public Vector3 rotationAim;
        public byte fightstate;

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
            ms.Write(this.id);
            ms.Write(this.position);
            ms.Write(this.yValue);
            ms.Write(this.move);
            ms.Write(this.rotationAim);
            ms.Write(this.fightstate);
            //field name=  type    type=  MsgType     has not auto-gen
        }
        //从stream中读取数据以初始化field
        public void Decode(MsgStream ms)
        {
            this.id = ms.Int;
            this.position = ms.Vector3;
            this.yValue = ms.Float;
            this.move = ms.Byte;
            this.rotationAim = ms.Vector3;
            this.fightstate = ms.Byte;
        }
        //---Auto Generate Code End---
    }
}