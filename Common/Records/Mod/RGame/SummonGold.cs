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

using static Records.Pal.Base;

namespace Records.Mod.RGame;

public record class SummonGold(
    string Name,                    // 名称
    string[]? Description,          // 描述
    ushort CostMP,                  // MP 损耗
    ushort BaseDamage,              // 基础伤害
    ushort SoundId,                 // 仙术音效
    MagicType Type,                 // 仙术系属
    SummonGoldEffect Effect,        // 特效参数
    MagicScript Script,             // 脚本
    MagicScope Scope                // 作用域
);

/// <summary>
/// 特效参数
/// </summary>
public record class SummonGoldEffect(
    ushort SpriteId,            // 召唤神形象
    ushort EffectId,            // 动画
    short XOffset,              // X 轴偏移
    short YOffset,              // Y 轴偏移
    ushort IdleFrames,          // 原地蠕动帧数
    ushort MagicFrames,         // 施法帧数
    ushort AttackFrames,        // 攻击帧数
    short ColorShift,           // 重复次数（0 = 不重复）
    ushort Shake,               // 屏幕震动
    ushort Wave                 // 屏幕波动
);
