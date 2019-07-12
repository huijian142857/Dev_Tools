/*
* Author:  caoshanshan
* Email:   me@dreamyouxi.com

 */
using UnityEngine;
using System.Collections;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Collections.Generic;

public enum MsgType
{
    MIN = 5,//do not modify this value
    NONE = 6,//do not modify this value
    CUSTOM_CMD = 7,//do not modify this value  use for client and server

    MMSTREAM_TEST_MSG,//8

    //-------------------msg type to send other client------------------
    //follow type will direct send to other's client via server or(P2P),
    //in follow type if want to create MsgType use class Msg , if want to read MsgType please see how to create ,
    //param sequence is same with how to read,see uaeage(BattleServer.MsgLoop)
    UPDATE_TRANSFORM,
    UPDATE_PLAYER_STATUS,
    PLAYER_ATTACK,//player fire attack msg
    PLAYER_ATTACK_SHOOT, // SHOOT GUN WEAPON
    PLAYER_TAKE_DAMAGE,
    PLAYER_ADD_FORCE,
    //   PLAYER_WEAPON_PICKUP,
    //  PLAYER_REQUEST_HOST_WRAPON_PICKUP,//send request weapon pick to host and host check
    //   PLAYER_THROW_WEAPON, // when bullet use out use this msg
    ////  PLAYER_THROW_WEAPON2, // when has bullet left , use this msg @see use
    //  PLAYER_DROP_WEAPON, //drop weapon will pre show ,beacuse of net work event can call drop werapon , so just use this to spawn a weapon
    //  PLAYER_DROP_WEAPON2,  // this is for notify host will drop weapon

    GAME_DESTROY_PIECE, //destroy piece
    //  GAME_SPAWN_WEAPON,

    GAME_CHANGE_MAP,//change map  host call all clients
    GAME_START_GAME, //start game , this should wait count down msg done
    GAME_INIT_INFO, // init game info when join a room  
    GAME_PLAY_COUNT_DOWN,//play count down  just animation 
    PLAYER_CHANGE_TO_BOSS,//change to boss
    PLAYER_CHANGE_TO_BOSS_NORMAL, // change to boss normal ... not boss is boss normal then in boss battle
    GAME_MAP_TAKE_DAMAGE,// syncablemapobject take damage
    GAME_MAP_CUSTOM_CMD,//for map object custom cmd for a best physics or others
    GAME_MAP_TRANSFORM,//for map lerp linear
    GAME_SNAKE_NEW_TARGET,// snake find a new target
    GAME_SNAKE_SPAWN, // spawn a new snake
    GAME_VOICE_MESSAGE,// game voice message
    GAME_SPAWN_OBJECT,//dunamic spawn object whether is syncableXXX or others

    GAME_STATISTICS_SYNC,//sync host statistics info to other clients after GAME_START_GAME

    GAME_PARKOUR_SYNC_STATUS,//sync game status of parkour .such as time
    GAME_PARKOUR_REACH_FINAL_POINT,//one player reach final point
    GAME_PARKOUR_PROCESS_RESULT,//game over process game result and disconnedted
    GAME_PARKOUR_GAMERESTART, // gamerestart

    GAME_JOIN_OR_CREATE_ROOM_SETTING_DONE_NORMAL,//normal mode setting done 
    GAME_JOIN_OR_CREATE_ROOM_SETTING_DONE_PARKOUR,//parkour mode setting done

    GAME_JOIN_OR_CREATE_ROOM_PUSH_MAP,//sync map from one player with brocast server will catch this
    GAME_JOIN_OR_CREATE_ROOM_START_LOADING,//start loading  host click start-game
    GAME_JOIN_OR_CREATE_ROOM_CHANGE_HOST,//change host msg from new host 


    GAME_PLAYER_INFO,//sync one player info such as  uuid head name sex and so on

    GAME_PLAYER_OVER_REWARD,//sync self game over reward such as coin,

    GAME_PLAYER_USE_FACIAL, // player send facial, 
    GAME_SPAWN_BLACKHOLE,  // Generate a black hole

    GAME_PHYSICE_BAD, // 
    GAME_PLAYER_BLOCK_TAKE_DAMAGE,//block handler take damage just for sync block-hp

    GAME_JOIN_OR_CREATE_ROOM_KICK_PLAYER,//kick one player in room
    GAME_BOSS3_MAXHEALTH,// boss3 max health
    GAME_3V1BOSS_MAXHEALTH,// 3v1 boss max health

    GAME_PARKOUR_ROUND,

    GAME_PLAYER_START_BLOCK,//block handler start block
    GAME_PLAYER_BLOCK_TIMEOUT,//block time out

    GAME_BLACKHOLE_KILL_SOMEONE,//blackhole kill someone
    GAME_PLAYER_RELIVE,         //玩家复活

    LUA_MSG = 240,// the message will send to lua when battle
    MAX = 255,//do not modufy this if want to add length will change
}


//same with DataPack.h enum juist copy code
//do not modify order, just add  at last
//when add a new type, please add param sequence and comment,if none param just add //param:none
public enum CustomMsgType
{

    none = 1,
    //game logic will via server-verification
    //level-two msg type is MsgTypeGameLogic
    _game_logic_root_ = 2,// !!!!!Donot modify this value
    you_are_host,// notify client is host client
    //param:none
    kick_you_host,//
    //param:none
    join_room,//self enter room 
    //param:room_id(int)
    new_player_enter,//new player enter room
    //param:player_id(int)
    player_leave,// leave room
    //param:player_id(int)
    enter_room_error,
    //param:none
    ping, // ping msg
    //c-s param:int  want to set
    // c-s param int result

    load_scene_level,//current game loaded game level
    //param:int level send with host client

    sync_king_ids,//sync king count
    //param:int int int int  is mean to player id = 1 2  3 4
    //s-c int type,if 1 then enable, other is disable

    game_round_over,//single match game over  or time-out for ResultType is time

    game_round_count,//current round count and max
    //s-c
    lua_msg,//will send to lua-custom msg
    //c-s
    game_mode_parkour,//send this to server 

    game_self_has_create,//send to server this player has been create

    game_join_or_create_push_map,// all clients send this to server his map_uuid
    game_join_or_create_room_setting_normal,//host send to server current game setting
    game_join_or_create_room_setting_parkour,//host send to server current game setting
    game_sync_time,//sync game time if ResultType is time

    game_round_over_force,//c-s client request server force game round over  just for Team2V2 has one team left will force round over
    game_boss_info,//c-s or c-s for boss info init and controll

    //c-s load map done will report server 
    //s-c play 321-fight animation next 3-second wil SyncGameStart(game_state_game)
    game_state_load_done,
    game_state_game,//s-c SyncGameStart
    //c-s request sync current game status
    //s-c current game status
    game_sync_background,

    //s-c notify client udp info
    //c-s request varify udp info
    udp_info,

    //s-c load map some client is loading please wait
    game_load_map_waiting,

    //s-c preloading those map
    //param int16 size
    //param int64 customId
    game_preloading_map,
    //player status
    player_status,

    benchmark_first = 240,
    benchmark_datapack = 241,
    max = 255,
}


//游戏逻辑的相关指令 为了方便服务器校验管理 都统一走二级 消息  _game_logic_root_
//max is int16 (short)
public enum MsgTypeGameLogic
{
    _invalid_ = 0,
    _min_ = 1,
    _none_ = 2,

    //c-s
    //s-c sync all player staus in one package
    //param player status array
    player_status,

    //c-s request spawn weapon
    //param int-array
    //s-c
    //param int(weapon count)
    //param int object_id
    //param int weapon_index
    //param float position_z
    //param float position_y
    spawn_weapon,

    //c-s request pick up weapon
    //param int object_id
    //s-c 
    //param id
    //param object_id
    //param index
    //param bullet count
    pcik_up_weapon,
    //c-s request throw weapon
    //param float position z y
    //param float rotation  z y
    //s-c
    //param is the same
    throw_weapon,

    //c-s drop weapon when bullet use out
    //s-c the same
    //param int16 id_in_room
    drop_weapon,

    //c-s notify server self is change to boss or de-change to boss
    //if is boss,server will allow all Fire pass and pick up one weapon auto
    set_boss_status,

    _max_ = 0x6fff,//max value is int16 (short) (0x6fff)
};


//原始字节流的第一个字符表示是否是CUSTOM_CMD; 后面全是相应的数据
//7 CUSTOM_CMD else is MStream
//完整的消息包结构是这样 头部，4字节表示包体大小，1字节 1 2 各自表示是stream 还是string（custom cmd）类型 ，接着是具体的数据


//TODO use memstream  to instead of string
public static class Msg
{
    //note param sequence is read sequence  SEE useage
    public static MsgStream Transform(int id, Vector3 posiiton, Vector3 rotation)
    {
        MsgStream ret = MsgStream.Create(MsgType.UPDATE_TRANSFORM);
        return ret.Write(id).Write(posiiton).Write(rotation);
    }
    public static MsgStream PlayerStatus(Vector3 posiiton, float yValue, byte move, Vector3 rotationAim)//, byte fightstate)
    {
        MsgStream ret1 = MsgStream.Create(MsgType.CUSTOM_CMD, CustomMsgType.player_status);
        ret1.Write(posiiton).//Write(rotation).
            WriteShortFloat(yValue).Write(move).WriteShortFloat(rotationAim.y).WriteShortFloat(rotationAim.z);//.Write(fightstate);//.Write(1.23f);
        return ret1;

        /*   var ss = msg;
           int id = ss.Int;
           Vector3 position = ss.Vector3;
           float yValue = ss.Float;
           int move = ss.Byte;
           Vector3 aim = ss.Vector3;
           int fightstate = ss.Byte;
         */


        //强行使用 CustomMsgObject 只是为了举例子
        /*   var msg = new Msgs.MsgPlayerStatus();
           msg.id = id;
           msg.position = posiiton;
           msg.yValue = yValue;
           msg.move = move;
           msg.rotationAim = rotationAim;
           msg.fightstate = fightstate;
           MsgStream ret = MsgStream.Create(msg);
           return ret;*/

    }
    // for normal attack
    public static MsgStream MsgPlayerAttack(int id, Vector3 position, Vector3 direction)//,Vector3 position,Vector3 rotation)
    {
        MsgStream ret = MsgStream.Create(MsgType.PLAYER_ATTACK);
        ret.Write(id).Write(position).Write(direction);
        return ret;

    }
    // for shoot
    public static MsgStream MsgPlayerAttackShoot(int id, Vector3 position, Vector3 direction)//,Vector3 position,Vector3 rotation)
    {
        MsgStream ret = MsgStream.Create(MsgType.PLAYER_ATTACK_SHOOT);
        ret.Write(id).Write(position).Write(direction);
        return ret;
    }

    public static MsgStream MsgPlayerTakeDamage(int id, int attacker, int new_hp, int damage)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.PLAYER_TAKE_DAMAGE);
        ret.Write(id).Write(attacker).Write(new_hp).Write(damage);
        return ret;
    }
    public static MsgStream MsgPlayerAddForce(int id, int index, Vector3 force, ForceMode mode)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.PLAYER_ADD_FORCE);
        ret.Write(id).Write(index).Write(force).Write((byte)mode);
        return ret;
    }
    public static MsgStream MsgPlayerChangeToBoss(int id)
    {
        MsgStream ret = MsgStream.Create(MsgType.PLAYER_CHANGE_TO_BOSS);
        ret.Write(id);
        return ret;
    }
    public static MsgStream MsgPlayerChangeToBossNormal(int id)
    {
        MsgStream ret = MsgStream.Create(MsgType.PLAYER_CHANGE_TO_BOSS_NORMAL);
        ret.Write(id);
        return ret;
    }

    public static MsgStream MsgGameDestroyPiece(int spawnId, Vector3 force, float multiplier, bool networkForce)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_DESTROY_PIECE);
        ret.Write(spawnId).Write(force).Write(multiplier).Write(networkForce);
        return ret;

    }
    public static MsgStream MsgGameChangeMap(int nextLevel, int kingId, long customsId)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_CHANGE_MAP);
        ret.Write(nextLevel).Write(kingId).Write(customsId);
        return ret;
    }
    public static MsgStream MsgGameInitInfo(int currentLevelIndex, bool isInLobby)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_INIT_INFO);
        ret.Write(currentLevelIndex).Write(isInLobby);
        return ret;

    }
    /*    public static MsgStream MsgGameStartGame(int loadMapIndex, long customsId, int kingId)// 
        {
            MsgStream ret = MsgStream.Create(MsgType.GAME_START_GAME).Write(loadMapIndex).Write(customsId).Write(kingId);
            return ret;

        }*/
    /*   public static MsgStream MsgGamePlayCountDown(int loadMapIndex, long customsId, int kingId)// 
       {
           MsgStream ret = MsgStream.Create(MsgType.GAME_PLAY_COUNT_DOWN).Write(loadMapIndex).Write(customsId).Write(kingId);
           return ret;
       }*/
    public static MsgStream MsgGameMapCustomCmd(int id, long cmd)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_MAP_CUSTOM_CMD);
        ret.Write(id).Write(cmd);
        return ret;


    }
    public static MsgStream MsgGameMapTakeDamage(int id, int damage_type, long who_uuid, int new_hp)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_MAP_TAKE_DAMAGE);
        ret.Write(id).Write(damage_type).Write(who_uuid).Write(new_hp);
        return ret;
    }
    public static MsgStream MsgGameMapTransform(int id, Vector3 position, Vector3 rotation)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_MAP_TRANSFORM);
        ret.Write(id).Write(position).Write(rotation);
        return ret;
    }

    public static MsgStream MsgGameSnakeSpawn(int id, int type, Vector3 position, Vector3 rotation, long owner_uuid, int owner_id_in_room)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_SNAKE_SPAWN);
        ret.Write(id).Write(type).Write(position).Write(rotation).Write(owner_uuid).Write(owner_id_in_room);
        return ret;
    }

    public static MsgStream MsgGameBlackHole(Vector3 position, int type = 0)
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_SPAWN_BLACKHOLE);
        ret.Write(position).Write(type);
        return ret;
    }


    public static MsgStream MsgGameSnakeNewTarget(int id, int targetId)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_SNAKE_NEW_TARGET);
        ret.Write(id).Write(targetId);
        return ret;
    }
    public static MsgStream MsgGameVoiceMessage(int id, string fileid)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_VOICE_MESSAGE);
        ret.Write(id).Write(fileid);
        return ret;
    }
    public static MsgStream MsgGameSpawnObject(int id, int asset_id, Vector3 position, Vector3 rotation)// 
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_SPAWN_OBJECT);
        ret.Write(id).Write(asset_id).Write(position).Write(rotation);
        return ret;
    }

    public static MsgStream MsgLuaMsg()// 
    {
        MsgStream ret = MsgStream.Create(MsgType.LUA_MSG);
        return ret;
    }
    public static MsgStream MsgLuaMsgCustom()
    {
        MsgStream ret = MsgStream.Create(MsgType.CUSTOM_CMD, CustomMsgType.lua_msg);
        return ret;
    }

    public static MsgStream MsgGameModeParkour()
    {
        MsgStream ret = MsgStream.Create(MsgType.CUSTOM_CMD, CustomMsgType.game_mode_parkour);
        return ret;
    }
    public static MsgStream MsgGameParkourSyncStatus()
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_PARKOUR_SYNC_STATUS);
        return ret;
    }

    public static MsgStream MsgGameParkourRound()
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_PARKOUR_ROUND);
        return ret;
    }
    public static MsgStream MsgGameParkourReachFinalPoint(int id_in_room, float time)
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_PARKOUR_REACH_FINAL_POINT);
        ret.WriteByte((byte)id_in_room).WriteFloat(time);
        return ret;
    }
    public static MsgStream MsgGameParkourProcessResult()
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_PARKOUR_PROCESS_RESULT);
        return ret;
    }

    public static MsgStream MsgGameParkourGameStart()
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_PARKOUR_GAMERESTART);
        return ret;
    }

    public static MsgStream MsgGameSelfHasCreate()
    {
        MsgStream ret = MsgStream.Create(MsgType.CUSTOM_CMD, CustomMsgType.game_self_has_create);
        return ret;
    }

    public static MsgStream MsgBoss3Maxhealth(int maxhealth)
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_BOSS3_MAXHEALTH);
        ret.WriteInt32(maxhealth);
        return ret;
    }
    public static MsgStream Msg3V1BossMaxhealth(float maxhealth)
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_3V1BOSS_MAXHEALTH);
        ret.WriteFloat(maxhealth);
        return ret;
    }


    /*  public static MsgStream MsgGameJoinOrCreateRoomLoading(int id_in_room, float progress)
      {
          MsgStream ret = MsgStream.Create(MsgType.GAME_JOIN_OR_CREATE_ROOM_LOADING);
          ret.WriteByte((byte)id_in_room).WriteFloat(progress);
          return ret;
      }*/
    //map_1 Official 官方地图
    //map_2 GeneralPlayer 玩家地图
    //map_3 MyMap 房主的地图
    //map_4 OtherPlayer 房间内其他玩家发布的地图
    //result_type 结算方式 1 时间 2 局数
    //result_value 结算值  对应局数或者 分钟数
    //kill_score 击杀得分
    //king_score 胜利得分
    public static MsgStream MsgGameJoinOrCreateRoomSettingDoneNormal(bool map_1, bool map_2, bool map_3, bool map_4, int result_type, int result_value, int kill_score, int king_score, bool is_room_open)
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_JOIN_OR_CREATE_ROOM_SETTING_DONE_NORMAL);
        //  地图包
        ret.WriteBool(map_1);
        ret.WriteBool(map_2);
        ret.WriteBool(map_3);
        ret.WriteBool(map_4);
        //结算方式
        ret.WriteByte((byte)result_type);
        //结算值 
        ret.WriteByte((byte)result_value);
        //击杀分数
        ret.WriteByte((byte)kill_score);
        //胜利分数
        ret.WriteByte((byte)king_score);
        //房间是否公开
        ret.WriteBool(is_room_open);
        return ret;
    }

    //GAME_JOIN_OR_CREATE_ROOM_SETTING_DONE_PARKOUR

    //result_time 结算时间 分钟
    //revive_type  复活类型  1 表示墓碑 2表示 出生点复活
    //revive_time 复活时间 秒
    public static MsgStream MsgGameJoinOrCreateRoomSettingDoneParkour(int result_time, int revive_type, int revive_time, bool is_room_open, int roundnum)
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_JOIN_OR_CREATE_ROOM_SETTING_DONE_PARKOUR);
        ret.WriteInt32(result_time);
        ret.WriteByte((byte)revive_type);
        ret.WriteByte((byte)revive_time);
        ret.WriteBool(is_room_open);
        ret.WriteByte((byte)roundnum);
        return ret;
    }

    public static MsgStream MsgGameJoinOrCreateRoomKickPlayer(long luuid)
    {
        MsgStream ret = MsgStream.Create(MsgType.GAME_JOIN_OR_CREATE_ROOM_KICK_PLAYER);
        ret.Write(luuid);
        return ret;
    }

    //@param weapon_index start with 1
    public static MsgStream GameLogicSpawnWeapon(int weapon_index, Vector3 position)
    {
        var msg = MsgStream.Create(MsgTypeGameLogic.spawn_weapon);
        msg.WriteInt16(1);
        msg.WriteInt16((short)weapon_index);
        msg.Write(position);
        return msg;
    }

    public static MsgStream GameLogicPickUpWeapon(int object_id)// 
    {
        MsgStream ret = MsgStream.Create(MsgTypeGameLogic.pcik_up_weapon);
        ret.Write(object_id);
        return ret;
    }

    public static MsgStream GameLogicThrowWeapon(int weapon_id, Vector3 position, Vector3 direction)// 
    {
        MsgStream ret = MsgStream.Create(MsgTypeGameLogic.throw_weapon);
        ret.WriteInt16((short)weapon_id).Write(position).Write(direction);
        return ret;
    }
}


// MemStream 的一个另外的实现方式 为了能 该类型的消息服务器(C++)也能解析 做反外挂处理(事件树,消息拦截)
//对于服务器来说 并不需要一个完整的消息 来做校验信息 可能只是一个消息的部分信息 因此需要提供一种机制
//并且约束每种类型的 byte(unsigned char)大小  默认都按照C++ 大小来 比如int是4字节 long long是8字节 MoveInt 来跳过一个int
//只有2种大的数据格式  stream型 和string 型   暂不考虑支持protobuf 
//基本思想是按照unsigned char(byte) 来拆分和组装 模式按照小端 即 0xff00aaee  第一个字节为ee 第四个字节为ff 即 buffer[0]=ee
//partial 可以自己扩展 比如WriteVector3 WriteMap WriteProtobuf
////如果越界 那么会返回请求数据类型的默认值 int32 为0  string为""
//TODO 关于bytes 的 GC压力 比如写入float 会转换为bytes 可以考虑优化一下
public partial class MMStream : IDisposable
{
    public static bool DisableOutError = false;
    public void Dispose()
    {
        //recycle
        if (buffer != null && buffer.Length == DefaultBufferSize)
        {
            Base.MemoryPool.Recycle(ref buffer);
        }
        _cache_double = null;
        _cache_float = null;
        buffer = null;
    }
    //TODO 为了减少碎片 可以考虑重复利用 在默认大小范围内的 MMStream 即 MMStreamPool:ObjectPool 
    public const int DefaultBufferSize = 128;
    //real buffer will grow when default size is not enough
    public byte[] buffer = null;
    //this is buffer real buffer length;
    private int length = 0;

    //---------------for write
    //一次最大写入大小
    const int MAX_WRITE_BYTES = 20480;//20kb
    public MMStream()
    {
        buffer = //new byte[DefaultBufferSize];
        Base.MemoryPool.Alloc(DefaultBufferSize);
    }
    //for read
    public MMStream(byte[] bytes, int len)
    {
        this.buffer = bytes;
        this.length = len;// bytes.Length;
    }
    public int Length
    {
        get
        {
            return length;
        }
    }

    //if default size is not enough will grow with double-size
    private void CheckSize(int size)
    {
        if (size > MAX_WRITE_BYTES || length > MAX_WRITE_BYTES)
        {//illegal operation
            //report this issue to bugly
            Debug.LogError("MMStream.CheckSize size error size=" + size + "  length=" + length);
            return;
        }
        if (length + size > buffer.Length)
        {
            //overflow auto grow
            var new_buffer = new byte[buffer.Length + Mathf.Max(DefaultBufferSize, size)];
            buffer.CopyTo(new_buffer, 0);
            if (buffer.Length == DefaultBufferSize)
            {
                Base.MemoryPool.Recycle(ref buffer);
            }
            buffer = null;
            this.buffer = new_buffer;
            //TODO move old buffer to pool
        }
        else
        {
            //enough do-nothing
        }
    }
    private void WriteDone(int use_size)
    {
        this.length += use_size;
    }
    //-----------------for write
    public void WriteByte(byte data)
    {
        this.CheckSize(1);
        buffer[length + 0] = data;
        this.WriteDone(1);
    }
    public void WriteBool(bool data)
    {
        this.CheckSize(1);
        buffer[length + 0] = (byte)(data ? 1 : 0);
        this.WriteDone(1);
    }

    public void WriteInt16(short data)
    {
        this.CheckSize(2);
        buffer[length + 0] = (byte)(data & 0xff);
        buffer[length + 1] = (byte)((data >> 8) & 0xff);
        this.WriteDone(2);
    }

    public void WriteInt32(int data)
    {
        this.CheckSize(4);
        buffer[length + 0] = (byte)(data & 0xff);
        buffer[length + 1] = (byte)((data >> 8) & 0xff);
        buffer[length + 2] = (byte)((data >> 16) & 0xff);
        buffer[length + 3] = (byte)((data >> 24) & 0xff);
        this.WriteDone(4);
    }
    public void WriteInt64(long data)
    {
        this.CheckSize(8);
        buffer[length + 0] = (byte)(data & 0xff);
        buffer[length + 1] = (byte)((data >> 8) & 0xff);
        buffer[length + 2] = (byte)((data >> 16) & 0xff);
        buffer[length + 3] = (byte)((data >> 24) & 0xff);
        buffer[length + 4] = (byte)((data >> 32) & 0xff);
        buffer[length + 5] = (byte)((data >> 40) & 0xff);
        buffer[length + 6] = (byte)((data >> 48) & 0xff);
        buffer[length + 7] = (byte)((data >> 56) & 0xff);
        this.WriteDone(8);
    }
    byte[] _cache_float = null;
    public unsafe void WriteFloat(float data)
    {
        if (_cache_float == null)
        {
            _cache_float = new byte[4];
        }
        //for avoid GC
        fixed (byte* b = _cache_float)
            *((int*)b) = (*(int*)&data);
        this.WriteBytes(_cache_float, 4);
        //this.WriteBytes(BitConverter.GetBytes(data), 4);
    }
    byte[] _cache_double = null;
    public unsafe void WriteDouble(double data)
    {
        if (_cache_double == null)
        {
            _cache_double = new byte[8];
        }
        //for avoid GC
        fixed (byte* b = _cache_double)
            *((long*)b) = (*(long*)&data);
        this.WriteBytes(_cache_double, 8);
        //  this.WriteBytes(BitConverter.GetBytes(data), 8);
    }
    public void WriteBytes(byte[] data)
    {
        if (MAX_WRITE_BYTES < data.Length)
        {
#if UNITY_EDITOR
            if (!DisableOutError)
                Debug.LogError("MMStream write error size is too lage " + data.Length);
#endif
            return;
        }
        this.CheckSize(data.Length);
        Buffer.BlockCopy(data, 0, buffer, length, data.Length);
        this.WriteDone(data.Length);
    }
    //如果要 自定义写入 请用WriteBytesWithHead
    private void WriteBytes(byte[] data, int size)
    {
        if (MAX_WRITE_BYTES < size)
        {
#if UNITY_EDITOR
            Debug.LogError("MMStream write error size is too lage " + size);
#endif
            return;
        }
        this.CheckSize(size);
        Buffer.BlockCopy(data, 0, buffer, length, size);
        this.WriteDone(size);
    }
    //带头大小的bytes  
    public void WriteBytesWithHead(byte[] data, int size)
    {
        if (MAX_WRITE_BYTES < size)
        {
#if UNITY_EDITOR
            Debug.LogError("MMStream write error size is too lage " + size);
#endif
            return;
        }
        this.WriteInt32(size);
        this.WriteBytes(data, size);
    }
    public void WriteBytesWithHead(byte[] data)
    {
        this.WriteBytesWithHead(data, data.Length);
    }
    public void WriteString(string data)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
        if (MAX_WRITE_BYTES < bytes.Length)
        {
#if UNITY_EDITOR
            Debug.LogError("MMStream write error size is too lage " + bytes.Length);
#endif
            return;
        }
        this.WriteInt32(bytes.Length + 1);
        this.WriteBytes(bytes, bytes.Length);
        this.WriteByte(0);//  '\0';
    }

    /*  //default encode to write string
      public void WriteStringDefault(string data)
      {
          byte[] bytes = System.Text.Encoding.Default.GetBytes(data);
          this.WriteInt32(bytes.Length + 1);
          this.WriteBytes(bytes, bytes.Length);
          this.WriteByte(0);//  '\0';
      }
      public void WriteString(string data)
      {
          this.WriteStringDefault(data);
      }

      public void WriteStringUTF8(string data)
      {
          byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
          this.WriteInt32(bytes.Length + 1);
          this.WriteBytes(bytes, bytes.Length);
          this.WriteByte(0);//  '\0';
      }*/
    //--------------------for read
    int read_pos = 0;
    int offset = 0;
    const int MAX_READ_BYTES = 20480;//00 KB
    public bool Readable(int size)
    {
        if (read_pos + size <= buffer.Length)
        {
            return true;
        }
#if UNITY_EDITOR
        if (!DisableOutError)
            Debug.LogError("Readable error is end of stream check this operation is valid?");
#endif
        return false;
    }
    public MMStream(byte[] bytes, int len, int offset)
    {
        this.buffer = bytes;
        read_pos = offset;
        this.offset = offset;
    }
    public byte ReadByte()
    {
        if (!Readable(1)) return 0;
        return buffer[read_pos++];
    }
    public bool ReadBool()
    {
        if (!Readable(1)) return false;
        return ReadByte() == 1 ? true : false;
    }
    public int ReadInt32()
    {
        if (!Readable(4)) return 0;
        int byte1 = buffer[read_pos++];
        int byte2 = buffer[read_pos++];
        int byte3 = buffer[read_pos++];
        int byte4 = buffer[read_pos++];
        return byte1 | (byte2 << 8) | (byte3 << 16) | (byte4 << 24);
    }
    public short ReadInt16()
    {
        if (!Readable(2)) return 0;
        int byte1 = buffer[read_pos++];
        int byte2 = buffer[read_pos++];
        return (short)(byte1 | (byte2 << 8));
    }

    public long ReadInt64()
    {
        if (!Readable(8)) return 0;
        long ret = 0;
        long byten = buffer[read_pos++];
        ret = ret | byten;

        byten = buffer[read_pos++];
        ret = ret | (byten << 8);

        byten = buffer[read_pos++];
        ret = ret | (byten << 16);

        byten = buffer[read_pos++];
        ret = ret | (byten << 24);

        byten = buffer[read_pos++];
        ret = ret | (byten << 32);

        byten = buffer[read_pos++];
        ret = ret | (byten << 40);

        byten = buffer[read_pos++]; ;
        ret = ret | (byten << 48);

        byten = buffer[read_pos++];
        ret = ret | (byten << 56);

        return ret;
    }
    public float ReadFloat()
    {
        if (Readable(4))
        {
            read_pos += 4;
            return BitConverter.ToSingle(buffer, read_pos - 4);
        }
        return 0.0f;
    }
    public double ReadDouble()
    {
        if (Readable(8))
        {
            read_pos += 8;
            return BitConverter.ToDouble(buffer, read_pos - 8);
        }
        return 0.0f;
    }
    public byte[] ReadBytes(int size)
    {
        if (Readable(size) && size < MAX_READ_BYTES)
        {
            byte[] ret = new byte[size];
            Buffer.BlockCopy(buffer, read_pos, ret, 0, size);
            read_pos += size;
            return ret;
        }
        return null;
    }
    //will read head 4 size
    public byte[] ReadBytes()
    {
        if (Readable(4))
        {
            int size = ReadInt32();
            if (size < MAX_READ_BYTES)
            {
                byte[] ret = new byte[size];
                Buffer.BlockCopy(buffer, read_pos, ret, 0, size);
                read_pos += size;
                return ret;
            }
            return null;
        }
        return null;
    }
    public string ReadString()
    {
        if (Readable(4))
        {
            int size = ReadInt32();
            var buf = ReadBytes(size);
            if (buf != null)
            {
                return System.Text.Encoding.UTF8.GetString(buf);
            }
        }
        return "";
    }
    //读取的时候 跳过一定字节数 如果有字符串的话 就不能一次性跳过了
    public void Skip(int size)
    {
        if (Readable(size))
        {
            read_pos += size;
        }
    }
    //跳过一个string
    public void SkipString()
    {
        if (Readable(4))
        {
            int size = ReadInt32();
            if (Readable(size))
            {
                read_pos += size;
            }
        }
    }
    public void SkipBytes()
    {
        if (Readable(4))
        {
            int size = ReadInt32();
            if (Readable(size))
            {
                read_pos += size;
            }
        }
    }
    //重置读取流
    public void ResetReader()
    {
        read_pos = offset;
    }

}

// C# MemoryStream just client to client
public class MStream : IDisposable
{
    /*  public MStream()// for read
      {
          //  stream = new MemoryStream(1000);
          //   writer = new BinaryWriter(stream);
      }
      */
    MemoryStream stream = null;
    BinaryWriter writer = null;
    BinaryReader reader = null;

    // -----------------------for write
    public MsgType Type;
    public MStream(MsgType type)
    {
        this.Type = type;
        stream = new MemoryStream(1000);
        writer = new BinaryWriter(stream);
        writer.Write((byte)type); // make sure type
        if (type == MsgType.CUSTOM_CMD)
        {
            Debug.LogError(" can not************************ ");
        }
    }
    public void Dispose()
    {
        this.Release();
    }
    const bool OptimNetSize = false;
    int length = 0;
    public MStream Write(float msg)
    {
        if (OptimNetSize == false)
        {
            writer.Write(msg);
            length += 4;
            return this;
        }
        writer.Write((short)(msg * 100f));
        length += 2;
        return this;
    }
    public MStream Write(int msg)
    {
        if (OptimNetSize == false)
        {
            writer.Write(msg);
            length += 4;
            return this;
        }
        writer.Write((short)(msg));
        length += 2;
        return this;
    }
    public MStream Write(long msg)
    {
        if (OptimNetSize == false)
        {
            writer.Write(msg);
            length += 8;
            return this;
        }
        writer.Write((int)(msg));
        length += 4;
        return this;
    }
    public MStream Write(byte msg)
    {
        writer.Write((msg));
        length += 1;
        return this;
    }
    public MStream Write(bool _bool)
    {
        writer.Write(_bool);
        length += 1;
        return this;
    }
    public MStream Write(Quaternion rotation)
    {
        this.Write(rotation.x);
        this.Write(rotation.y);
        this.Write(rotation.z);
        this.Write(rotation.w);
        return this;
    }

    public MStream Write(string msg)
    {
        writer.Write(msg);
        length += msg.Length;
        return this;
    }
    public MStream Write(MMStream msg)
    {
        if (OptimNetSize == false)
        {
            writer.Write(msg.Length);
            writer.Write(msg.buffer, 0, msg.Length);
            length += 4;
            length += msg.Length;
            return this;
        }
    }
    public MStream Write(Vector3 vector3)
    {
        //    writer.Write((short)(vector3.x * 100));
        //   writer.Write((short)(vector3.y * 100));
        // writer.Write((short)(vector3.z * 100));
        this.Write(vector3.y);
        this.Write(vector3.z);

        //  length += 4;
        return this;
    }

    public byte[] bytess = null;
    // ---- for read 
    public MStream(byte[] bytes)
    {
        stream = new MemoryStream(bytes);
        reader = new BinaryReader(stream);
        var tag = this.Byte;// 1 is curtom  2 is mstream  ,just ignore
        this.Type = (MsgType)this.Byte;
        bytess = bytes;
    }

    public Quaternion Quaternion
    {
        get
        {
            return new Quaternion(this.Float, this.Float, this.Float, this.Float);
        }
    }
    public Vector3 Vector3
    {
        get
        {
            return new Vector3(0f,//(float)reader.ReadInt16() / 100f,
          this.Float,
       this.Float);
        }
    }
    public int Int
    {
        get
        {
            if (OptimNetSize == false)
            {
                return reader.ReadInt32();
            }
            return reader.ReadInt16();
        }
    }
    public long Long
    {
        get
        {
            if (OptimNetSize == false)
            {
                return reader.ReadInt64();
            }
            return reader.ReadInt32();
        }
    }

    public byte Byte
    {
        get
        {
            return reader.ReadByte();
        }
    }

    public float Float
    {
        get
        {
            if (OptimNetSize == false)
            {
                return reader.ReadSingle();
            }
            return (float)reader.ReadInt16() / 100f;
        }
    }

    public bool Bool
    {
        get
        {
            return reader.ReadBoolean();
        }
    }

    public string String
    {
        get
        {
            return reader.ReadString();
        }
    }


    public byte[] GetBytes()
    {
        return stream.ToArray();
    }
    public bool Readable()
    {
        return this.reader.PeekChar() != -1;
    }
    public byte[] ToArray()
    {
        if (stream.Length < TcpSocket.MAX_VALID_BUFFER_LEN)
        {
            return stream.ToArray();
        }
        Debug.LogError("Msg.cs invalid buffer length");
        return null;
    }
    ~MStream()
    {
        this.Release();
    }
    public void Release()
    {
        if (hasRelease) return;
        hasRelease = true;
        if (stream != null) { stream.Dispose(); stream = null; }
        if (writer != null) { writer.Close(); writer = null; }
        if (reader != null) { reader.Close(); reader = null; }
    }
    bool hasRelease = false;
    /*   public void Init(byte[] datas)
       {
           this.stream = new MemoryStream(datas);
           this.reader = new BinaryReader(stream);
           // check type 


       }*/
    /*  public string ToString()
      {
          return Convert.ToBase64String(stream.ToArray());
             MemoryStream ms = new MemoryStream();
             GZipStream xx = new GZipStream(ms, CompressionMode.Compress);
             var d = stream.ToArray();
             xx.Write(d, 0, d.Length);
             xx.Close();

             var str_zip = Convert.ToBase64String(ms.ToArray());
             var str = Convert.ToBase64String(stream.ToArray());

             //  Debug.LogError(" to string:" + length + "   string.length:" + sss.Length);

             //  Debug.LogError(" send leng:" + sss.Length + "  data len:" + length+"  stream:" + d.Length + "  zip:" + ms.Length);
             Debug.LogError("orign len=" + length + "  zip:" + str_zip.Length + "   none:" + str.Length);

             return str;
           
          return string.Empty;
          //  return System.Text.Encoding.Default.GetString(stream.ToArray(), 0, length);
      }
      public byte[] ToBytes(string data)
      {//base64 编码并没有'|' 因此安全
          var d = Convert.FromBase64String(data);
          //    byte[] buffer =System.Text.Encoding.Default.GetBytes(    System.Text.Encoding.Default.GetString(d, 0, d.Length)  );
          var buffer = d;
          //  this.buffer = buffer;
          this.stream = new MemoryStream(buffer);
          this.reader = new BinaryReader(stream);
          Debug.LogError("read length =" + buffer.Length + "  " + this.stream.CanRead);
          return buffer;
      }*/
}



public enum MsgStreamType
{
    MMStream,
    MStream,
    Protobuf,
}


//@note this is not thread-safe
public class MsgStreamPool
{
    public void PreAlloc()
    {
        for (int i = 0; i < 30; i++)
        {
            _queue.Enqueue(new MsgStream());
        }
    }
    public MsgStream Alloc()
    {
        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }
        return new MsgStream();
    }
    public void DeAlloc(MsgStream msg)
    {
        _queue.Enqueue(msg);
    }
    public void Relase()
    {
        while (_queue.Count > 0)
        {
            _queue.Dequeue().Dispose();
        }
    }
    private Queue<MsgStream> _queue = new Queue<MsgStream>();
}
public class MsgStream : IDisposable
{
    public void Dispose()
    {
        if (stream != null)
        {
            stream.Dispose();
            stream = null;
        }
    }
    public MMStream stream = null;
    public MsgType mType = MsgType.NONE;
    public MsgType Type
    {
        get
        {
            return mType;
        }
    }
    CustomMsgType mcType = CustomMsgType.none;
    public CustomMsgType CustomType
    {
        get
        {
            return mcType;
        }
    }
    public bool IsCustomCmd = false;
    //---------------------------create for write
    public static MsgStream Create(MsgType type, CustomMsgType custom = CustomMsgType.none)
    {
        MsgStream ret = new MsgStream();
        ret.stream = new MMStream();
        ret.stream.WriteByte(1); //占位符 可以暂时理解为消息格式 MMStream 还是protobuf  还是服务器不可读的数据(MStream) 仅限于客户端之间的交互 比如MemoryStream的数据 不过他们都可以通过MMStream来中转
        //如果容量255 还不够 直接该这里扩展即可  注意服务器 还要改FastCheck接口
        ret.stream.WriteByte((byte)type);//消息类型
        if (type == MsgType.CUSTOM_CMD)  // 7 is custom can fast check
        {
            ret.stream.WriteByte((byte)custom);//custom消息类型   该类型是服务器和客户端交互专有消息
        }
        ret.mType = type;
        return ret;
    }

    public static MsgStream Create(MsgTypeGameLogic msg)
    {
        var ret = MsgStream.Create(MsgType.CUSTOM_CMD, CustomMsgType._game_logic_root_);
        ret.WriteInt16((short)msg);
        return ret;
    }

    //create with CustomMsgObject and can send this to server
    public static MsgStream Create(CustomMsgObject type)
    {
        MsgStream ret = new MsgStream();
        ret.stream = new MMStream();
        ret.stream.WriteByte(1); //占位符 可以暂时理解为消息格式 MMStream 还是protobuf  还是服务器不可读的数据(MStream) 仅限于客户端之间的交互 比如MemoryStream的数据 不过他们都可以通过MMStream来中转
        //如果容量255 还不够 直接该这里扩展即可  注意服务器 还要改FastCheck接口
        //will be init in Encode();
        //    ret.stream.WriteByte((byte)type.type);//消息类型
        //  ret.mType = type.type;
        type.Encode(ret);
        type.CustomEncode(ret);
        return ret;
    }



    //------------------------create for read
    public static MsgStream Create(byte[] bytes, int len)
    {
        MsgStream ret = new MsgStream();
        ret.stream = new MMStream(bytes, len);
        ret.stream.ReadByte(); //占位符 可以暂时理解为消息格式 MMStream 还是protobuf 还是服务器不可读的数据 仅限于客户端之间的交互 比如MemoryStream的数据 不过他们都可以通过MMStream来中转

        //如果容量255 还不够 直接该这里扩展即可  注意服务器 还要改FastCheck接口
        ret.mType = (MsgType)ret.stream.ReadByte();
        if (ret.mType == MsgType.CUSTOM_CMD)
        {
            ret.mcType = (CustomMsgType)ret.stream.ReadByte();
            ret.IsCustomCmd = true;
        }
#if UNITY_EDITOR
        if (ret.Type == MsgType.CUSTOM_CMD && ret.mcType != CustomMsgType.ping && ret.mcType != CustomMsgType._game_logic_root_ && ret.mcType!= CustomMsgType.player_status)
        {
            Debug.LogWarning("MsgStream recv Cmd:" + ret.mcType + "  msg:" + ret.mcType);
        }
        if (ret.Type != MsgType.CUSTOM_CMD && ret.Type != MsgType.UPDATE_TRANSFORM && ret.Type != MsgType.UPDATE_PLAYER_STATUS && ret.Type != MsgType.GAME_MAP_TRANSFORM)
        {
            Debug.LogWarning("MsgStream recv Sync:" + ret.Type);
        }
#endif
        return ret;
    }




    //--------------------------------------wrapper for MMStream or MStream hight-level data such vector3

    //-------------------for write
    public MsgStream Write(object data)
    {
        throw new NotSupportedException("you can not write data whic was none-suprot");
    }
    public MsgStream Write(int data)
    {
        this.stream.WriteInt32(data);
        return this;
    }
    public MsgStream Write(byte data)
    {
        this.stream.WriteByte(data);
        return this;
    }
    public MsgStream Write(short data)
    {
        this.stream.WriteInt16(data);
        return this;
    }
    public MsgStream Write(bool data)
    {
        this.stream.WriteBool(data);
        return this;
    }

    public MsgStream Write(long data)
    {
        this.stream.WriteInt64(data);
        return this;
    }
    public MsgStream Write(float data)
    {
        this.stream.WriteFloat(data);
        return this;
    }
    public MsgStream Write(double data)
    {
        this.stream.WriteDouble(data);
        return this;
    }

    public MsgStream Write(string data)
    {
        this.stream.WriteString(data);
        return this;
    }
    /*   public MsgStream Write(MsgDataObject data)
       {
           data.Encode(this);
           return this;
       }*/
    public MsgStream Write(Vector3 data)
    {
        this.stream.WriteFloat(data.y);
        this.stream.WriteFloat(data.z);
        return this;
    }
    public MsgStream Write(Quaternion rotation)
    {
        this.Write(rotation.x);
        this.Write(rotation.y);
        this.Write(rotation.z);
        this.Write(rotation.w);
        return this;
    }



    public MsgStream WriteInt32(int data)
    {
        this.stream.WriteInt32(data);
        return this;
    }
    public MsgStream WriteByte(byte data)
    {
        this.stream.WriteByte(data);
        return this;
    }
    public MsgStream WriteInt16(short data)
    {
        this.stream.WriteInt16(data);
        return this;
    }
    public MsgStream WriteBool(bool data)
    {
        this.stream.WriteBool(data);
        return this;
    }

    public MsgStream WriteInt64(long data)
    {
        this.stream.WriteInt64(data);
        return this;
    }
    public MsgStream WriteFloat(float data)
    {
        this.stream.WriteFloat(data);
        return this;
    }
    public MsgStream WriteDouble(double data)
    {
        this.stream.WriteDouble(data);
        return this;
    }
    public MsgStream WriteShortVector3(Vector3 data)
    {
        this.WriteShortFloat(data.y);
        this.WriteShortFloat(data.z);
        return this;
    }
    public MsgStream WriteShortQuaternion(Quaternion rotation)
    {
        this.WriteShortFloat(rotation.x);
        this.WriteShortFloat(rotation.y);
        this.WriteShortFloat(rotation.z);
        this.WriteShortFloat(rotation.w);
        return this;
    }
    public MsgStream WriteShortFloat(float data)
    {
        this.stream.WriteInt16((short)(data * 100.0));
        return this;
    }

    public MsgStream WriteVector3(float x, float y, float z)
    {
        this.Write(new Vector3(x, y, z));
        return this;
    }
    public MsgStream WriteQuaternion(float x, float y, float z, float w)
    {
        this.Write(new Quaternion(x, y, z, w));
        return this;
    }



    /* public MsgStream WriteCustomMsgObject(CustomMsgObject data)
     {
         data.Encode(this);
         data.CustomEncode(this);
         return this;
     }
     */
    //-----------------for read
    public void Skip(int size)
    {
        this.stream.Skip(size);
    }
    public int Int
    {
        get
        {
            return stream.ReadInt32();
        }
    }
    public byte Byte
    {
        get
        {
            return stream.ReadByte();
        }
    }
    public short Short
    {
        get
        {
            return stream.ReadInt16();
        }
    }
    public bool Bool
    {
        get
        {
            return stream.ReadBool();
        }
    }
    public string String
    {
        get
        {
            return stream.ReadString();
        }
    }
    public long Long
    {
        get
        {
            return stream.ReadInt64();
        }
    }
    public float Float
    {
        get
        {
            return stream.ReadFloat();
        }
    }

    public float ShortFloat
    {
        get
        {
            return ((float)this.Short) / 100.0f;
        }
    }
    public Vector3 ShortVector3
    {
        get
        {
            float y = ((float)this.Short) / 100.0f;
            float z = ((float)this.Short) / 100.0f;
            return new Vector3(0f, y, z);
        }
    }
    public Vector3 Vector3
    {
        get
        {
            float y = this.Float;
            float z = this.Float;
            return new Vector3(0f, y, z);
        }
    }
    public MsgStream MsgDataObject
    {
        get
        {
            return this;
        }
    }

    public Vector3 Position
    {
        get
        {
            return this.Vector3;
        }
    }


    public double Double
    {
        get
        {
            return stream.ReadDouble();
        }
    }
    public Quaternion Rotation
    {
        get
        {
            return this.Quaternion;
        }
    }
    public Quaternion Quaternion
    {
        get
        {
            float x = this.Float;
            float y = this.Float;
            float z = this.Float;
            float w = this.Float;
            return new Quaternion(x, y, z, w);
        }
    }
    //Decode MsgStream to CustomMsgObject
    //ReadIntXXXX
    public T Decode<T>() where T : CustomMsgObject, new()
    {
        var ret = new T();
        ret.Decode(this);
        ret.CustomDecode(this);
        return ret;
    }
    /* public T DecodeData<T>() where T : MsgDataObject, new()
     {
         var ret = new T();
         ret.Decode(this);
         return ret;
     }*/
}


//可传输序列化的 对象 目前只考虑客户端之间使用 服务器不参与兼容 lua的table也可以参与 提供更高级的抽象传输对象
//原理基于 反射？ json？ 还是普通的手动写入基础数据对象？
//暂时选用方案3
// 消息 可用函数式构建(  Msg.XXXX()   ) 也可以用该接口 构建

//encode decode 由代码自动生成
public interface CustomMsgObject
{
    //encode class to Msgtream WriteXXXX
    void Encode(MsgStream ms);
    //init class member from MsgStream  ReadInt32XXX
    void Decode(MsgStream ms);


    void CustomEncode(MsgStream ms);
    void CustomDecode(MsgStream ms);
}
/*
//高级消息对象 用于对于非简单数据类型的消息进行再次封装
public interface MsgDataObject
{
    //encode class to Msgtream
    void Encode(MsgStream ms);
    //init class member from MsgStream
    void Decode(MsgStream ms);
}

*/
/*

//please use this class  only read or write
public class MsgStr111eam : IDisposable
{
    //--- protocol index means:
    //0  msg type  to MsgTye
    //1 ......

    //--------------------------for write
    //--------------------------for write
    //--------------------------for write
    //--------------------------for write
    //--------------------------for write
    //--------------------------for write

    private string msg = string.Empty;
    public bool IsCustomCmd = false;
    public static MsgStream Create(MsgType type, CustomMsgType custom = CustomMsgType.none)
    {
        MsgStream ret = new MsgStream();
        string x = string.Empty;
        x += ((int)(type)).ToString();
        ret.mType = type;
        if (custom != CustomMsgType.none)
        {
            x += SPLIT;
            x += (int)custom;
            ret.IsCustomCmd = true;
            ret.stream = null;
        }
        else
        {// mem stream
            ret.stream = new MStream(type);
        }
        ret.msg = x;
        return ret;
    }

    public MsgStream Write(int msg)
    {
        if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
        {
            stream.Write(msg);
            return this;
        }
        this.msg += SPLIT + msg.ToString();
        return this;
    }
    public MsgStream Write(long msg)
    {
        if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
        {
            stream.Write(msg);
            return this;
        }
        this.msg += SPLIT + msg.ToString();
        return this;
    }
    public MsgStream Write(bool msg)
    {
        if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
        {
            stream.Write(msg);
            return this;
        }
        this.msg += SPLIT + msg.ToString();
        return this;
    }
    public MsgStream Write(float msg)
    {
        if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
        {
            stream.Write(msg);
            return this;
        }
        this.msg += SPLIT + msg.ToString();
        return this;
    }
    public MsgStream Write(string msg)
    {
        if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
        {
            stream.Write(msg);
            return this;
        }
        this.msg += SPLIT + msg.ToString();
        return this;
    }
    public void Write(MStream stream)
    {
        if (this.stream != null) stream.Dispose();
        this.stream = stream;
    }
    public MStream stream = null;
    ~MsgStream()
    {
        this.Release();
    }
    public void Dispose()
    {
        this.Release();
    }
    bool hasRelease = false;
    public void Release()
    {
        if (hasRelease) return;
        hasRelease = true;
        if (stream != null)
        {
            stream.Dispose();
            stream = null;
        }
    }
    public MsgStream Write(Vector3 position)
    {
        if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
        {
            stream.Write(position);
            return this;
        }
        return this.Write(position.x).Write(position.y).Write(position.z);
    }
    public MsgStream Write(Quaternion rotation)
    {
        if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
        {
            stream.Write(rotation);
            return this;
        }
        return this.Write(rotation.x).Write(rotation.y).Write(rotation.z).Write(rotation.w);
    }

    public string Msg
    {
        get
        {
            return msg;
        }
    }

    //------------------------------for read
    //------------------------------for read
    //------------------------------for read
    //------------------------------for read
    //------------------------------for read


    MsgType mType = MsgType.NONE;
    public MsgType Type
    {
        get
        {
            if (BattleServer.EnableMStream && stream != null)
            {
                return stream.Type;
            }
            return mType;
        }
    }
    public int Length
    {
        get
        {
            return split.Length;
        }
    }
    const char SPLIT = '|';
    string[] split = null;
    int index = 0;
    string raw = string.Empty;
    public static MsgStream Create(string msg)
    {
        MsgStream ret = new MsgStream();
        if (string.IsNullOrEmpty(msg))
        {
            Debug.LogError("MsgStream recv:null");
            return ret;
        }
        ret.raw = msg;
        ret.split = msg.Split(SPLIT);
        int t = 0;
        int.TryParse(ret.split[0], out t);
        ret.mType = (MsgType)t;
        ret.index = 1;
#if UNITY_EDITOR
        if (ret.Type == MsgType.CUSTOM_CMD && ret.Length >= 2 && msg.Substring(0, 3) != "7|9")// "7|9" is ping cmd
        {
            int type;
            int.TryParse(ret.split[1], out type);
            Debug.LogWarning("MsgStream recv Cmd:" + (CustomMsgType)type + "  msg:" + msg);
        }
        if (ret.Type != MsgType.CUSTOM_CMD && ret.Type != MsgType.UPDATE_TRANSFORM && ret.Type != MsgType.UPDATE_PLAYER_STATUS)
        {
            Debug.LogWarning("MsgStream recv Sync:" + ret.Type + "  msg:" + msg);
        }
#endif
        return ret;
    }


    // -----for read 
    public static MsgStream Create(MStream msg)
    {
        MsgStream ret = new MsgStream();
        ret.stream = msg;
        ret.IsCustomCmd = false;

        ret.mType = msg.Type;
        //   Debug.LogWarning("MsgStream Recv: " + ret.Type);
        return ret;

 
        return ret;
    }



    // for read
    public string RawMsg
    {
        get
        {
            return this.raw;
        }
    }
    public bool IsEnd
    {
        get
        {
            return (index >= split.Length);
        }
    }
    public int Int
    {
        get
        {
            if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
            {
                return stream.Int;
            }
            if (index >= split.Length) return 0;
            int ret = 0;
            return int.TryParse(split[index++], out ret) ? ret : 0;
        }
    }
    public long Long
    {
        get
        {
            if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
            {
                return stream.Long;
            }
            if (index >= split.Length) return 0;
            long ret = 0;
            return long.TryParse(split[index++], out ret) ? ret : 0;
        }
    }
    public bool Bool
    {
        get
        {
            if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
            {
                return stream.Bool;
            }
            if (index >= split.Length) return false;
            bool ret = false;
            return bool.TryParse(split[index++], out ret) ? ret : false;
        }
    }
    public float Float
    {
        get
        {
            if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
            {
                return stream.Float;
            }
            if (index >= split.Length) return 0f;
            float ret = 0f;
            return float.TryParse(split[index++], out ret) ? ret : 0f;
        }
    }

    public string String
    {
        get
        {
            if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
            {
                return stream.String;
            }
            if (index >= split.Length) return "0";
            return split[index++];
        }
    }
    public byte Byte
    {
        get
        {
            if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
            {
                return stream.Byte;
            }
            return 0;
        }
    }
    public Vector3 Vector3
    {
        get
        {
            if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
            {
                return stream.Vector3;
            }
            return new Vector3(this.Float, this.Float, this.Float);
        }
    }
    public Vector3 Position
    {
        get
        {
            return this.Vector3;
        }
    }
    public Quaternion Rotation
    {
        get
        {
            return this.Quaternion;
        }
    }
    public Quaternion Quaternion
    {
        get
        {
            if (BattleServer.EnableMStream && this.Type != MsgType.CUSTOM_CMD)
            {
                return stream.Quaternion;
            }
            return new Quaternion(this.Float, this.Float, this.Float, this.Float);
        }
    }
    public void ResetReader()
    {
        index = 1;
    }
}

*/
