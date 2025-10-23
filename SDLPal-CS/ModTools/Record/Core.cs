using ModTools.Unpack;
using System.Runtime.InteropServices;

namespace ModTools.Record;

public static unsafe class Core
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CScene
    {
        public  ushort      MapId;                  // 实际地图
        public  ushort      ScriptOnEnter;          // 脚本：进入场景
        public  ushort      ScriptOnTeleport;       // 脚本：脱离场景（引路蜂、土灵珠）
        public  ushort      EventObjectIndex;       // 事件起始索引，实际索引为（EventObjectIndex + 1）
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CEvent
    {
        public  short           VanishTime;             // 剩余隐匿帧数（一般为战斗事件）
        public  short           X;                      // X 坐标
        public  short           Y;                      // Y 坐标
        public  short           Layer;                  // 图层
        public  ushort          TriggerScript;          // 触发脚本
        public  ushort          AutoScript;             // 自动脚本
        public  short           State;                  // 触发状态
        public  ushort          TriggerMode;            // 触发模式
        public  ushort          SpriteId;               // 形象
        public  ushort          SpriteFrames;           // 形象每个方向的帧数
        public  ushort          Direction;              // 当前面朝方向
        public  ushort          CurrentFrameNum;        // 当前帧数（当前方向上的）
        public  ushort          TriggerIdleFrame;       // 触发脚本累计被触发次数
        readonly    ushort      _unknown;               // 未知数据
        readonly    ushort      _spriteFramesAuto;      // 形象总帧数（自动计算，只在内存中有意义）
        public  ushort          AutoIdleFrame;          // 自动脚本累计被触发次数
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CScript
    {
        public  ushort              Command;
        public  fixed   ushort      Args[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CScriptArgs
    {
        readonly    ushort      Value;

        public readonly byte Byte => (byte)Value;
        public readonly short Short => (short)Value;
        public readonly ushort UShort => Value;
        public readonly short X => Short;
        public readonly short Y => Short;
        public readonly byte BX => Byte;
        public readonly byte BY => Byte;
        public readonly byte BH => Byte;
        public readonly ushort Scene => UShort;
        public readonly ushort Event => UShort;
        public readonly string Addr => Script.AddAddrress(UShort);
        public readonly string Dialog => Message.Dialogues[UShort];
    }
}