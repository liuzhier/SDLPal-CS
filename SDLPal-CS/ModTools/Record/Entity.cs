using System.Runtime.InteropServices;

namespace ModTools.Record;

public static unsafe class Entity
{
    public enum BeginId : ushort
    {
        System  = 0x0000,
        Hero    = 0x0024,
        System2 = 0x002A,
        Item    = 0x003D,
        Magic   = 0x0127,
        Enemy   = 0x018E,
        Poison  = 0x0227,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HeroCommon
    {
        fixed   ushort      _reserved[2];               // 无效数据
        public  ushort      ScriptOnFriendDeath;        // 友方阵亡脚本
        public  ushort      ScriptOnDying;              // 濒死脚本
    }
    public enum ItemMask
    {
        Usable          = (1 << 0),             // 可使用
        Equipable       = (1 << 1),             // 可装备
        Throwable       = (1 << 2),             // 可投掷
        Consuming       = (1 << 3),             // 使用后减少
        ApplyToAll      = (1 << 4),             // 作用于全体
        Sellable        = (1 << 5),             // 可典当
        EquipableByHeroFirst  = (1 << 6),       // 李逍遥可装备（后面省略了剩下的 Hero）
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemDos
    {
        public  ushort      BitmapId;           // 图像（在 BALL.MKF）
        public  ushort      Price;              // 售价（典当半价）
        public  ushort      ScriptOnUse;        // 使用脚本
        public  ushort      ScriptOnEquip;      // 装备脚本
        public  ushort      ScriptOnThrow;      // 投掷脚本
        public  ushort      Flags;              // 二进制掩码参数
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemWin
    {
        public  ushort      BitmapId;           // 图像（在 BALL.MKF）
        public  ushort      Price;              // 售价（典当半价）
        public  ushort      ScriptOnUse;        // 使用脚本
        public  ushort      ScriptOnEquip;      // 装备脚本
        public  ushort      ScriptOnThrow;      // 投掷脚本
        public  ushort      ScriptDesc;         // 描述脚本
        public  ushort      Flags;              // 二进制掩码参数
    }

    public enum MagicMask : ushort
    {
        UsableOutsideBattle     = (1 << 0),
        UsableInBattle          = (1 << 1),
        UsableToEnemy           = (1 << 3),
        ApplyToAll              = (1 << 4),
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MagicDos
    {
        public  ushort          MagicDataId;            // 仙术基础数据（在 DATA.MKF #3）
        readonly    ushort      _reserved1;             // 无效数据
        public  ushort          ScriptOnSuccess;        // 后序脚本，前序脚本成功后执行
        public  ushort          ScriptOnUse;            // 前序脚本
        readonly    ushort      _reserved2;             // 无效数据
        public  ushort          Flags;                  // 二进制掩码参数
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MagicWin
    {
        public  ushort          MagicDataId;            // 仙术基础数据（在 DATA.MKF #3）
        readonly    ushort      _reserved;              // 无效数据
        public  ushort          ScriptOnSuccess;        // 后序脚本，前序脚本成功后执行
        public  ushort          ScriptOnUse;            // 前序脚本
        readonly    ushort      _reserved2;             // 无效数据
        public  ushort          ScriptDesc;             // 描述脚本
        public  ushort          Flags;                  // 二进制掩码参数
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EnemyCommon
    {
        public  ushort      EnemyDataId;                // 敌方基础数据（在 DATA.MKF #1）
                                                        // 同时也代表敌方图像（在 ABC.MKF）
        public  short       ResistanceToSorcery;        // 巫抗（0～10）
        public  ushort      ScriptOnTurnStart;          // 回合开始脚本
        public  ushort      ScriptOnBattleWon;          // 战斗结算脚本
        public  ushort      ScriptOnAction;             // 回合行动脚本（出招脚本）
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PoisonCommon
    {
        public  ushort          Level;              // 级别/烈度
        public  ushort          Color;              // 肖像颜色
        public  ushort          PlayerScript;       // 我方中毒脚本（每次回合结束执行）
        readonly    ushort      _reserved;          // 无效数据
        public  ushort          EnemyScript;        // 敌方中毒脚本（每次回合结束执行）
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Dos
    {
        [FieldOffset(0)]
        fixed    ushort             _undefined[6];

        [FieldOffset(0)]
        public   HeroCommon         Hero;

        [FieldOffset(0)]
        public   ItemDos            Item;

        [FieldOffset(0)]
        public   MagicDos           Magic;

        [FieldOffset(0)]
        public   EnemyCommon        Enemy;

        [FieldOffset(0)]
        public   PoisonCommon       Poison;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Win
    {
        [FieldOffset(0)]
        fixed    ushort             _undefined[7];

        [FieldOffset(0)]
        public   HeroCommon         Hero;

        [FieldOffset(0)]
        public   ItemWin            Item;

        [FieldOffset(0)]
        public   MagicWin           Magic;

        [FieldOffset(0)]
        public   EnemyCommon        Enemy;

        [FieldOffset(0)]
        public   PoisonCommon       Poison;
    }
}
