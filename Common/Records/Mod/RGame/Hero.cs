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
using System.Text.Json.Serialization;

namespace Records.Mod.RGame;

public class Hero
{
    public int AvatarId { get; set; }                               // 肖像（显示于“状态”页面）
    public int SpriteIdInBattle { get; set; }                       // 战斗形象（在 F.MKF）
    public int SpriteId { get; set; }                               // 行走形象（在 MGO.MKF）
    public string Name { get; set; } = null!;                       // 名称 Tag（在 WORD.DAT）
    public bool CanAttackAll { get; set; }                          // 普攻是否可攻击敌方全体
    public int Level { get; set; }                                  // 修行
    public int MaxHP { get; set; }                                  // 最大 HP
    public int HP { get; set; }                                     // 当前 HP
    public int MaxMP { get; set; }                                  // 最大 MP
    public int MP { get; set; }                                     // 当前 MP
    public int CoveredBy { get; set; }                              // 虚弱时受谁援护
    public int FramesPerDirection { get; set; }                     // 行走形象每个方向的帧计数
    public HeroEquipment Equipment { get; set; } = null!;           // 装备
    public BaseAttribute BaseAttribute { get; set; } = null!;       // 五维（武灵防速逃）
    public HeroResistance Resistance { get; set; } = null!;         // 抗性
    public HeroSound Sound { get; set; } = null!;                   // 行动音效
    public HeroScript Script { get; set; } = null!;                 // 脚本
    public HeroMagic Magic { get; set; } = null!;                   // 法术
}

public record class HeroEquipment
{
    public int Head { get; set; }           // 头戴
    public int Cloak { get; set; }          // 披挂
    public int Body { get; set; }           // 身穿
    public int Hand { get; set; }           // 手持
    public int Foot { get; set; }           // 脚穿
    public int Ornament { get; set; }       // 佩带
}

public record class HeroResistance
{
    public float Poison { get; set; }                               // 毒抗
    public ElementalResistance Elemental { get; set; } = null!;     // 灵抗
}

public record class HeroMagicLearnable
{
    public int Level { get; set; }          // 所需等级
    public int MagicId { get; set; }        // 仙术
}

public record class HeroMagic
{
    public int Cooperative { get; set; }                              // 合体法术
    public List<int> Learned { get; set; } = [];                      // 已领悟的仙术
    public List<HeroMagicLearnable> Learnable { get; set; } = [];       // 可领悟的仙术
}

public record class HeroSound
{
    public int Death { get; set; }          // 阵亡音效
    public int Attack { get; set; }         // 普攻音效
    public int Weapon { get; set; }         // 武器挥砍音效
    public int Critical { get; set; }       // 普攻暴击音效
    public int Magic { get; set; }          // 施法音效
    public int Cover { get; set; }          // 武器格挡音效
    public int Dying { get; set; }          // 濒死音效
}

public record class HeroScript
{
    public string FriendDeathTag { get; set; } = null!;        // 友方阵亡脚本
    [JsonIgnore]
    public int FriendDeath { get; set; }
    public string DyingTag { get; set; } = null!;              // 濒死脚本
    [JsonIgnore]
    public int Dying { get; set; }
}

public record class HeroActionEffect
{
    public string Name { get; set; } = null!;       // 特效概述
    public int Magic { get; set; }                  // 施法集气特效
    public int Attack { get; set; }                 // 普攻破空特效
}
