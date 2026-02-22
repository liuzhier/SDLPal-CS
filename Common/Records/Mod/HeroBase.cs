using Records.Mod.RGame;
using Records.Pal;
using System.Runtime.InteropServices;

namespace Records.Mod;

public unsafe class HeroBase
{
    public string Name { get; set; } = null!;
    public HeroScript Script { get; set; } = null!;
    public HeroBaseRaw* Raw { get; set; }
    public EquipmentEffect EquipmentEffect { get; set; } = new();
}

public class EquipmentEffect
{
    public HeroBaseRaw Head { get; set; }
    public HeroBaseRaw Body { get; set; }
    public HeroBaseRaw Armour { get; set; }
    public HeroBaseRaw Backside { get; set; }
    public HeroBaseRaw Hand { get; set; }
    public HeroBaseRaw Foot { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct HeroBaseRaw
{
    public int                      AvatarId;                       // 肖像（显示于状态页面和对话框）
    public int                      SpriteIdInBattle;               // 战斗形象（在 F.MKF）
    public int                      SpriteId;                       // 行走形象（在 MGO.MKF）
    readonly int                    _unknown;                       // 遗弃的字段，实体名称（在 WORD.DAT）
    public int                      AttackAll;                      // 普攻可攻击敌方全体
    readonly int                    _unknown1;                      // 未知数据 1
    public int                      Level;                          // 修行
    public int                      MaxHP;                          // 最大体力
    public int                      MaxMP;                          // 最大真气
    public int                      HP;                             // 当前体力
    public int                      MP;                             // 当前真气
    public HeroBaseEquipment        Equipment;                      // 装备
    public HeroBaseAttribute        Attribute;                      // 五维（武灵防速逃）
    public float                    PoisonResistance;               // 毒抗
    public ElementalResistance      ElementalResistance;            // 灵抗
    readonly int                    _unknown2;                      // 未知数据 2
    readonly int                    _unknown3;                      // 未知数据 3
    readonly int                    _unknown4;                      // 未知数据 4
    public int                      CoveredBy;                      // 虚弱时受谁援护
    public fixed int                Magics[Base.MaxHeroMagic];      // 已领悟的仙术
    public int                      FramesPerDirection;             // 行走形象每个方向的帧计数
    public int                      CooperativeMagic;               // 合体法术
    readonly int                    _unknown5;                      // 未知数据 5
    readonly int                    _unknown6;                      // 未知数据 6
    public HeroBaseSound            Sound;                          // 各种音效
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HeroBaseEquipment
{
    public  float   Head;           // 头戴
    public  float   Cloak;          // 披挂
    public  float   Body;           // 身穿
    public  float   Hand;           // 手持
    public  float   Foot;           // 脚穿
    public  float   Ornament;       // 佩带
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HeroBaseAttribute
{
    public  int     AttackStrength;     // 武术
    public  int     MagicStrength;      // 灵力
    public  int     Defense;            // 防御
    public  int     Dexterity;          // 身法
    public  int     FleeRate;           // 吉运
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ElementalResistance
{
    public  float   Wind;           // 风抗
    public  float   Thunder;        // 雷抗
    public  float   Water;          // 水抗
    public  float   Fire;           // 火抗
    public  float   Earth;          // 土抗
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HeroBaseSound
{
    public  int     Death;          // 阵亡音效
    public  int     Attack;         // 普攻音效
    public  int     Weapon;         // 武器挥砍音效
    public  int     Critical;       // 普攻暴击音效
    public  int     Magic;          // 施法音效
    public  int     Cover;          // 武器格挡音效
    public  int     Dying;          // 濒死音效
}
