#region License
/*
 * Copyright (c) 2025, liuzhier <lichunxiao_lcx@qq.com>.
 * 
 * This file is part of SDLPAL-CS.
 * 
 * SDLPAL-CS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 3
 * as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */
#endregion License

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Records.Mod.RGame;

public class Hero : Fighter
{
    public int AvatarId { get; set; }                               // 肖像（显示于“状态”页面）
    public bool CanAttackAll { get; set; }                          // 普攻是否可攻击敌方全体
    public int CoveredBy { get; set; }                              // 虚弱时受谁援护
    public HeroEquipment Equipment { get; set; } = null!;           // 装备
    public HeroMagic Magic { get; set; } = null!;                   // 法术
    public HeroCurrentExp CurrentExp { get; set; } = new();         // 当前修行、经验值
    public new HeroSprite Sprite { get; set; } = null!;             // 贴图参数
    public new HeroVoice Voice { get; set; } = null!;               // 行动音效
    public HeroScript Script { get; set; } = null!;                 // 脚本
    //public HeroActionEffect ActionEffect { get; set; } = null!;     // 战斗行动特效参数

    /// <summary>
    /// 检查是否装备了指定的道具
    /// </summary>
    /// <param name="item">道具实体编号</param>
    /// <returns>是否装备了指定的道具</returns>
    public bool Equipped(Item item)
    {
        var equip = Equipment;
        var part = item.EquipmentPart;
        var equipment = part switch
        {
            PalEquipmentPart.Head => equip.Head,
            PalEquipmentPart.Cloak => equip.Cloak,
            PalEquipmentPart.Body => equip.Body,
            PalEquipmentPart.Hand => equip.Hand,
            PalEquipmentPart.Foot => equip.Foot,
            PalEquipmentPart.Ornament => equip.Ornament,
            PalEquipmentPart.Temp => equip.Temp,
            _ => null!,
        };

        return (equipment != null!) && (equipment.ItemBase.Item == item);
    }

    /// <summary>
    /// 卸下装备
    /// </summary>
    /// <param name="item">道具实体编号</param>
    /// <returns>是否成功卸下</returns>
    public bool RemoveEquipment(Item item)
    {
        var success = Equipped(item);
        if (success)
        {
            var equip = Equipment;
            var part = item.EquipmentPart;

            _ = part switch
            {
                PalEquipmentPart.Head => equip.Head = new(),
                PalEquipmentPart.Cloak => equip.Cloak = new(),
                PalEquipmentPart.Body => equip.Body = new(),
                PalEquipmentPart.Hand => equip.Hand = new(),
                PalEquipmentPart.Foot => equip.Foot = new(),
                PalEquipmentPart.Ornament => equip.Ornament = new(),
                PalEquipmentPart.Temp => equip.Temp = new(),
                _ => throw S.Failed("Hero.RemoveEquipment", $"Undefined enumeration value '{part}'"),
            };
        }

        return success;
    }

    /// <summary>
    /// 清除装备效果
    /// </summary>
    /// <param name="part">装备部位编号</param>
    public void RemoveEquipmentEffect(PalEquipmentPart part)
    {
        if ((part & PalEquipmentPart.Head) != 0) Equipment.Head.Effect = new();
        if ((part & PalEquipmentPart.Cloak) != 0) Equipment.Cloak.Effect = new();
        if ((part & PalEquipmentPart.Body) != 0) Equipment.Body.Effect = new();
        if ((part & PalEquipmentPart.Hand) != 0) Equipment.Hand.Effect = new();
        if ((part & PalEquipmentPart.Foot) != 0) Equipment.Foot.Effect = new();
        if ((part & PalEquipmentPart.Ornament) != 0) Equipment.Ornament.Effect = new();
        if ((part & PalEquipmentPart.Temp) != 0) Equipment.Temp.Effect = new();
    }

    /// <summary>
    /// 领悟一项仙术
    /// </summary>
    /// <param name="magicId">仙术实体编号</param>
    public void AddMagic(int magicId)
    {
        if (!Magic.Learned.Any(x => x.Id == magicId)) Magic.Learned.Add(new(magicId));
    }

    /// <summary>
    /// 失去一项仙术
    /// </summary>
    /// <param name="magicId">仙术实体编号</param>
    public void RemoveMagic(int magicId) => Magic.Learned.RemoveAll(x => x.Id == magicId);

    /// <summary>
    /// 增加/减少能量
    /// </summary>
    /// <param name="hp">欲增加/减少的体力</param>
    /// <param name="mp">欲增加/减少的真气</param>
    /// <returns>是否增加/减少成功</returns>
    public bool ModifyHPMP(int hp, int mp)
    {
        var success = false;
        var power = Power;
        var originHP = power.HP;
        var originMP = power.MP;

        //
        // 只作用于存活的玩家
        //
        if (power.HP > 0)
        {
            //
            // HP 变化
            //
            power.HP += hp;
            power.HP = int.Max(power.HP, 0);
            power.HP = int.Min(power.HP, power.MaxHP);

            //
            // MP 变化
            //
            power.MP += mp;
            power.MP = int.Max(power.MP, 0);
            power.MP = int.Min(power.MP, power.MaxMP);

            if (originHP != power.HP || originMP != power.MP)
                //
                // HP 或 MP 有变化，返回成功结果
                //
                success = true;
        }

        return success;
    }
}

public class HeroEquipment
{
    public Equipment Head { get; set; } = null!;            // 头戴
    public Equipment Cloak { get; set; } = null!;           // 披挂
    public Equipment Body { get; set; } = null!;            // 身穿
    public Equipment Hand { get; set; } = null!;            // 手持
    public Equipment Foot { get; set; } = null!;            // 脚穿
    public Equipment Ornament { get; set; } = null!;        // 佩带
    public Equipment Temp { get; set; } = null!;            // 战斗临时
}

public class HeroMagic
{
    public int MagicCursorId { get; set; }                              // 仙术菜单光标位置
    public List<MagicBase> Learned { get; set; } = [];                  // 已领悟的仙术
    public List<HeroMagicLearnable> Learnable { get; set; } = [];       // 可领悟的仙术
}

/// <summary>
/// 英雄可领悟的仙术
/// </summary>
/// <param name="Level">修行要求</param>
/// <param name="MagicId">仙术编号</param>
public record class HeroMagicLearnable(int Level, MagicBase Magic);

public class LevelExp
{
    public int Level { get; set; }              // 修行
    public int RequiredEXP { get; set; }        // 所需经历（经验）
    public int CurrentEXP { get; set; }     // 当前经历（经验）
}

public class HeroCurrentExp
{
    public LevelExp HP { get; set; } = new();                   // 体力（生命）修行
    public LevelExp MP { get; set; } = new();                   // 真气（魔力）修行
    public LevelExp PhysicalPower { get; set; } = new();        // 武术（物强）修行
    public LevelExp MagicPower { get; set; } = new();           // 灵力（法强）修行
    public LevelExp Defense { get; set; } = new();              // 防御修行
    public LevelExp Dexterity { get; set; } = new();            // 身法（速度）修行
    public LevelExp Flee { get; set; } = new();                 // 吉运（逃跑率）修行
}

public class HeroSprite : FighterSprite
{
    public int SpriteId { get; set; }               // 行走形象（在 MGO.MKF）
    public int FramesPerDirection { get; set; }     // 行走形象每个方向的帧计数
}

public class HeroVoice : FighterVoice
{
    public int Weapon { get; set; }         // 武器挥砍音效
    public int Critical { get; set; }       // 普攻暴击音效
    public int Cover { get; set; }          // 武器格挡音效
    public int Dying { get; set; }          // 濒死（虚弱）音效
}

public class HeroScript
{
    public AddressBase FriendDeath { get; set; } = null!;       // 友方阵亡脚本
    public AddressBase Dying { get; set; } = null!;             // 濒死脚本
}

public class HeroActionEffect
{
    public string Name { get; set; } = null!;       // 特效概述
    public int Magic { get; set; }                  // 施法集气特效编号
    public int Attack { get; set; }                 // 普攻破空特效编号
}

public class Equipment()
{
    public ItemBase ItemBase { get; set; } = null!;             // 道具数据
    [JsonIgnore]
    public EquipmentEffect Effect { get; set; } = null!;        // 装备效果

    public Equipment(int itemId = 0) : this() => ItemBase = new ItemBase(itemId);
}

public class EquipmentEffect
{
    public bool AttackAll { get; set; }                             // 普攻可攻击敌方全体
    public int Level { get; set; }                                  // 修行
    public FighterPower Power { get; set; } = null!;                // HMP 能量
    public FighterAttribute Attribute { get; set; } = null!;        // 五维（武灵防速逃）
    public FighterResistance Resistance { get; set; } = null!;      // 灵抗
    public HeroSprite Sprite { get; set; } = null!;                 // 贴图参数
    public HeroVoice Voice { get; set; } = null!;                   // 各种音效
}
