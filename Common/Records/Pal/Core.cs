using Lib.Mod;
using System.Runtime.InteropServices;
using static Records.Pal.Entity;

namespace Records.Pal;

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

    public enum EventState : ushort
    {
        Hidden          = 0,        // 隐藏
        NonObstacle     = 1,        // 漂浮
        Obstacle        = 2,        // 障碍（阻碍领队通过）
    }

    public enum EventTriggerMode : ushort
    {
        None                = 0,        // 无法触发
        SearchNear          = 1,        // 手动触发，范围 0
        SearchNormal        = 2,        // 手动触发，范围 1
        SearchFar           = 3,        // 手动触发，范围 2
        TouchNear           = 4,        // 自动触发，范围 0
        TouchNormal         = 5,        // 自动触发，范围 1
        TouchFar            = 6,        // 自动触发，范围 2
        TouchFarther        = 7,        // 自动触发，范围 3
        TouchFarthest       = 8,        // 自动触发，范围 4
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CEvent
    {
        public  short           VanishTime;             // 正数为剩余隐匿帧数，负数为逃跑后僵直帧数（一般为战斗事件）
        public  short           X;                      // X 坐标
        public  short           Y;                      // Y 坐标
        public  short           Layer;                  // 图层
        public  ushort          TriggerScript;          // 触发脚本
        public  ushort          AutoScript;             // 自动脚本
        public  ushort          State;                  // 触发状态
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CScriptArgs
    {
        readonly    ushort      Value;

        public readonly byte Byte => (byte)Value;
        public readonly short Short => (short)Value;
        public readonly ushort UShort => Value;
        public readonly bool Bool => UShort != 0;
        public readonly short X => Short;
        public readonly short Y => Short;
        public readonly byte BX => Byte;
        public readonly byte BY => Byte;
        public readonly byte BH => Byte;
        public readonly (short, short) SceneEvent => Config.GetSoftSceneEventId(Short);
        public readonly string Addr => Config.AddAddress(UShort);
        public readonly ushort HeroEntity => (ushort)(UShort - BeginId.Hero + 1);
        public readonly short ItemEntity => (Short > 0) ? (short)(UShort - BeginId.Item + 1) : Short;
        public readonly short MagicEntity => (Short > 0) ? (short)((UShort == 0x0018) ? 1 : (short)(UShort - BeginId.Magic + 2)) : Short;
        public readonly short EnemyEntity => (Short > 0) ? (short)(UShort - BeginId.Enemy + 1) : Short;
        public readonly short PoisonEntity => (Short > 0) ? (short)(UShort - BeginId.Poison + 1) : Short;
        public readonly string Dialog => Message.GetDialog(UShort);
        public readonly string Music => $"Music.{Message.Music[UShort]}";
        public readonly string CD => $"CD.{Message.CD[Short]}";
        public readonly string Fbp => $"Fbp{(Config.IsDosGame ? "Dos" : "Win")}.{Message.Background[Short]}";
        public readonly string Palette => $"Palette.{Message.Palette[UShort]}";
        public readonly (bool, short) EventTrigger => (UShort >= (ushort)EventTriggerMode.TouchNear, (short)(UShort - ((UShort >= (ushort)EventTriggerMode.TouchNear) ? (ushort)EventTriggerMode.TouchNear : (ushort)EventTriggerMode.SearchNear)));
    }
}
