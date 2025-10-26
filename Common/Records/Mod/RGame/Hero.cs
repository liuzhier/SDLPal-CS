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

using System.Text.Json.Serialization;

namespace Records.Mod.RGame;

public record class Hero(
    ushort AvatarId,                // 肖像（显示于“状态”页面）
    ushort SpriteIdInBattle,        // 战斗形象（在 F.MKF）
    ushort SpriteId,                // 行走形象（在 MGO.MKF）
    string Name,                    // 名称 Tag（在 WORD.DAT）
    bool CanAttackAll,              // 普攻是否可攻击敌方全体
    ushort Level,                   // 修行
    ushort MaxHP,                   // 最大 HP
    ushort HP,                      // 当前 HP
    ushort MaxMP,                   // 最大 MP
    ushort MP,                      // 当前 MP
    ushort CoveredBy,               // 虚弱时受谁援护
    ushort WalkFrames,              // 行走形象每个方向的帧计数
    HeroEquipment Equipment,        // 装备
    BaseAttribute BaseAttribute,    // 五维（武灵防速逃）
    HeroResistance Resistance,      // 抗性
    HeroSound Sound,                // 行动音效
    HeroScript Script,              // 脚本
    HeroMagic Magic                 // 法术
);

public record class HeroEquipment(
    ushort Head,        // 头戴
    ushort Cloak,       // 披挂
    ushort Body,        // 身穿
    ushort Hand,        // 手持
    ushort Foot,        // 脚穿
    ushort Ornament     // 佩带
);

public record class HeroResistance(
    short Poison,                       // 毒抗
    ElementalResistance Elemental       // 灵抗
);

public record class HeroMagicLearnable(
    ushort Level,       // 所需等级
    ushort MagicId      // 仙术
);

public record class HeroMagic(
    ushort Cooperative,                 // 合体法术
    ushort[] Learned,                   // 已领悟的仙术
    HeroMagicLearnable[] Learnable      // 可领悟的仙术
);

public record class HeroSound(
    ushort Death,           // 阵亡音效
    ushort Attack,          // 普攻音效
    ushort Weapon,          // 武器挥砍音效
    ushort Critical,        // 普攻暴击音效
    ushort Magic,           // 施法音效
    ushort Cover,           // 武器格挡音效
    ushort Dying            // 濒死音效
);

public record class HeroScript(
    string FriendDeath,     // 友方阵亡脚本
    string Dying            // 濒死脚本
);

public record class HeroActionEffect(
    string Name,        // 特效概述
    ushort Magic,       // 施法集气特效
    ushort Attack       // 普攻破空特效
);
