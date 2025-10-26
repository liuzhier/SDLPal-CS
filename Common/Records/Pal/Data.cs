using SimpleUtility;
using System.Runtime.InteropServices;

namespace Records.Pal;

public static unsafe class Data
{
    public const   int
        MaxShopItem         = 9,
        MaxEnemysInTeam     = 5,
        MaxHero             = 6,
        MaxHeroesInTeam     = 5,
        MagicElementalNum   = 5,
        MaxHeroMagic        = 32,
        MaxHeroEquipments   = 6,
        MaxScenes           = 300;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CShop
    {
        public  fixed   short       Items[MaxShopItem];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CEnemyAttribute
    {
        public  short       AttackStrength;     // 武术
        public  short       MagicStrength;      // 灵力
        public  short       Defense;            // 防御
        public  short       Dexterity;          // 身法
        public  short       FleeRate;           // 吉运
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CEnemy
    {
        public  ushort              IdleFrames;                                 // 原地蠕动帧数
        public  ushort              MagicFrames;                                // 施法帧数
        public  ushort              AttackFrames;                               // 攻击帧数
        public  ushort              IdleAnimSpeed;                              // 原地蠕动动画速度
        public  ushort              ActWaitFrames;                              // 行动动画每帧间固定帧间隔
        public  short               YPosOffset;                                 // Y 轴偏移
        public  short               AttackSound;                                // 音效：普攻
        public  short               ActionSound;                                // 音效：行动
        public  short               MagicSound;                                 // 音效：施法
        public  short               DeathSound;                                 // 音效：阵亡
        public  short               CallSound;                                  // 音效：进入战场时呼喊
        public  ushort              Health;                                     // 体力
        public  ushort              Exp;                                        // 战利品：经验值
        public  ushort              Cash;                                       // 战利品：金钱
        public  ushort              Level;                                      // 修行
        public  ushort              MagicId;                                    // 法术
        public  ushort              MagicRate;                                  // 施法概率
        public  ushort              AttackEquivItemId;                          // 普攻附带道具
        public  ushort              AttackEquivItemRate;                        // 普攻附带道具概率
        public  ushort              StealItemId;                                // 可偷道具
        public  ushort              StealItemCount;                             // 可偷道具数量
        public  CEnemyAttribute     Attribute;                                  // 五维（武灵防速逃）
        public  short               PoisonResistance;                           // 毒抗
        public  fixed   short       ElementalResistance[MagicElementalNum];     // 灵抗
        public  short               PhysicalResistance;                         // 物抗
        public  ushort              DualMove;                                   // 每回合是否能连续行动两次
        public  ushort              CollectValue;                               // 灵葫能量
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CEnemyTeam
    {
        public  fixed   short       EnemyIds[MaxEnemysInTeam];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CHeroEquipment
    {
        fixed   ushort      _data[_lenght];

        const   ushort      _lenLow     = MaxHero;
        const   ushort      _lenHigh    = MaxHeroEquipments;
        const   ushort      _lenght     = _lenLow * _lenHigh;

        public ushort this[int equipmentId, int heroId]
        {
            get
            {
                S.CheckoutArrayIndex2(_lenght, ref equipmentId, ref heroId);

                return _data[equipmentId * _lenLow + heroId];
            }
            set
            {
                S.CheckoutArrayIndex2(_lenght, ref equipmentId, ref heroId);

                _data[equipmentId * _lenLow + heroId] = value;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CHeroAttribute
    {
        public  fixed   short       AttackStrength[MaxHero];        // 武术
        public  fixed   short       MagicStrength[MaxHero];         // 灵力
        public  fixed   short       Defense[MaxHero];               // 防御
        public  fixed   short       Dexterity[MaxHero];             // 身法
        public  fixed   short       FleeRate[MaxHero];              // 吉运
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CElementalResistance
    {
        fixed   short       _data[_lenght];

        const   ushort      _lenLow     = MaxHero;
        const   ushort      _lenHigh    = MagicElementalNum;
        const   ushort      _lenght     = _lenLow * _lenHigh;

        public short this[int heroId, int elementId]
        {
            get
            {
                S.CheckoutArrayIndex2(_lenght, ref elementId, ref heroId);

                return _data[elementId * _lenLow + heroId];
            }
            set
            {
                S.CheckoutArrayIndex2(_lenght, ref elementId, ref heroId);

                _data[elementId * _lenLow + heroId] = value;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CHeroMagic
    {
        fixed   ushort      _data[_lenght];

        const   ushort      _lenLow     = MaxHero;
        const   ushort      _lenHigh    = MaxHeroMagic;
        const   ushort      _lenght  = _lenLow * _lenHigh;

        public ushort this[int magicId, int heroId]
        {
            get
            {
                S.CheckoutArrayIndex2(_lenght, ref magicId, ref heroId);

                return _data[magicId * _lenLow + heroId];
            }
            set
            {
                S.CheckoutArrayIndex2(_lenght, ref magicId, ref heroId);

                _data[magicId * _lenLow + heroId] = value;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CHero
    {
        public  fixed   ushort              AvatarId[MaxHero];              // 肖像（显示于“状态”页面）
        public  fixed   ushort              SpriteIdInBattle[MaxHero];      // 战斗形象（在 F.MKF）
        public  fixed   ushort              SpriteId[MaxHero];              // 行走形象（在 MGO.MKF）
        public  fixed   ushort              Name[MaxHero];                  // 名称 Tag（在 WORD.DAT）
        public  fixed   ushort              AttackAll[MaxHero];             // 普攻可攻击敌方全体
        fixed   ushort                      _unknown1[MaxHero];             // 未知数据 1
        public  fixed   ushort              Level[MaxHero];                 // 修行
        public  fixed   ushort              MaxHP[MaxHero];                 // 最大体力
        public  fixed   ushort              MaxMP[MaxHero];                 // 最大真气
        public  fixed   ushort              HP[MaxHero];                    // 当前体力
        public  fixed   ushort              MP[MaxHero];                    // 当前真气
        public  CHeroEquipment              Equipment;                      // 装备
        public  CHeroAttribute              Attribute;                      // 五维（武灵防速逃）
        public  fixed   short               PoisonResistance[MaxHero];      // 毒抗
        public  CElementalResistance        ElementalResistance;            // 灵抗
        fixed   ushort                      _unknown2[MaxHero];             // 未知数据 2
        fixed   ushort                      _unknown3[MaxHero];             // 未知数据 3
        fixed   ushort                      _unknown4[MaxHero];             // 未知数据 4
        public  fixed   ushort              CoveredBy[MaxHero];             // 虚弱时受谁援护
        public  CHeroMagic                  Magic;                          // 已领悟的仙术
        public  fixed   ushort              WalkFrames[MaxHero];            // 行走形象每个方向的帧计数
        public  fixed   ushort              CooperativeMagic[MaxHero];      // 合体法术
        fixed   ushort                      _unknown5[MaxHero];             // 未知数据 5
        fixed   ushort                      _unknown6[MaxHero];             // 未知数据 6
        public  fixed   ushort              DeathSound[MaxHero];            // 阵亡音效
        public  fixed   ushort              AttackSound[MaxHero];           // 普攻音效
        public  fixed   ushort              WeaponSound[MaxHero];           // 武器挥砍音效
        public  fixed   ushort              CriticalSound[MaxHero];         // 普攻暴击音效
        public  fixed   ushort              MagicSound[MaxHero];            // 施法音效
        public  fixed   ushort              CoverSound[MaxHero];            // 武器格挡音效
        public  fixed   ushort              DyingSound[MaxHero];            // 濒死音效
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CMagic
    {
        public  ushort      SpriteId;           // 仙术动画（在 FIRE.MKF）
        public  ushort      Type;               // 仙术类型（9 = 召唤神）
        public  short       XOffset;            // X 轴偏移
        public  short       YOffset;            // Y 轴偏移
        public  short       LayerOffset;        // 图层偏移
                                                // 实际图层 = Pos.Y + MagicBase1.YOffset + LayerOffset
        public  short       Speed;              // 播放速度
        public  short       KeepEffect;         // 是否将仙术动画最后一帧留在地面上（-1 = 是）
        public  ushort      SoundDelay;         // 音效延迟
        public  ushort      EffectTimes;        // 重复次数（0 = 不重复）
        public  ushort      Shake;              // 屏幕震动
        public  ushort      Wave;               // 屏幕波动
        ushort      _unknown;                   // 未知数据
        public  ushort      CostMP;             // MP 损耗
        public  short       BaseDamage;         // 基础伤害
        public  ushort      Elemental;          // 五灵系属（0 = 非五灵系, last = 毒系）
        public  ushort      SoundId;            // 仙术音效
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CSummon
    {
        public  ushort      MagicDataId;        // 仙术数据 ID（同在 DATA.MKF #4）
        public  ushort      Type;               // 仙术类型（9 = 召唤神）
        public  ushort      XOffset;            // X 轴偏移
        public  ushort      YOffset;            // Y 轴偏移
        public  ushort      SpriteId;           // 召唤神动画（在 F.MKF）
        public  ushort      IdleFrames;         // 原地蠕动帧总数
        public  ushort      MagicFrames;        // 施法帧总数
        public  ushort      AttackFrames;       // 普攻帧总数
        public  ushort      ColorShift;         // 战斗背景调色板偏移
        public  ushort      Shake;              // 屏幕震动
        public  ushort      Wave;               // 屏幕波动
        ushort      _unknown;                   // 未知数据
        public  ushort      CostMP;             // MP 损耗
        public  ushort      BaseDamage;         // 基础伤害
        public  ushort      Elemental;          // 五灵系属（0 = 非五灵系, last = 毒系）
        public  ushort      SoundId;            // 仙术音效
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CBattleField
    {
        public          ushort      ScreenWave;                             // 屏幕波动级别
        public  fixed   short       ElementalEffect[MagicElementalNum];     // 战场地形对五灵的影响
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CLevelUpMagic
    {
        public  ushort      Level;          // 所需等级
        public  ushort      MagicId;        // 仙术
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CHeroActionEffect
    {
        public  ushort      Magic;      // 施法集气特效
        public  ushort      Attack;     // 普攻破空特效
    }
}
