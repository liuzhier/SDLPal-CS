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
using static Records.Pal.Base;

namespace Records.Mod.RGame;

/// <summary>
/// 主要数据
/// </summary>
public record class Magic(
    string Name,                    // 名称
    string[]? Description,          // 描述
    ushort CostMP,                  // MP 损耗
    short BaseDamage,               // 基础伤害
    ushort SoundId,                 // 仙术音效
    MagicType Type,                 // 仙术系属
    MagicActionType ActionType,     // 行动类型
    MagicEffect Effect,             // 特效参数
    MagicScript Script,             // 脚本
    MagicScope Scope                // 作用域
);

/// <summary>
/// 特效参数
/// </summary>
public record class MagicEffect(
    ushort EffectId,        // 动画
    short XOffset,          // X 轴偏移
    short YOffset,          // Y 轴偏移
    short LayerOffset,      // 图层偏移
                            // 实际图层 = Pos.Y + MagicBase1.YOffset + LayerOffset
    short Speed,            // 播放速度
    short KeepEffect,       // 是否将仙术动画最后一帧留在地面上（0 = 否 ）
    ushort SoundDelay,      // 音效延迟
    ushort EffectTimes,     // 重复次数（0 = 不重复）
    ushort Shake,           // 屏幕震动
    ushort Wave             // 屏幕波动
);

/// <summary>
/// 脚本
/// </summary>
public record class MagicScript(
    string Success,         // 后序脚本（前序脚本成功才执行）
    string Use              // 前序脚本（一般用于检查是否具备施法条件）
    //string Description      // 描述脚本（不仅能显示描述，还能进行其他的奇葩操作）
    //                        // （如比如查看说明顺便全体回血，但是试了一下，没法中毒--）
    //                        // 暂不支持！描述脚本由程序自动生成
);

/// <summary>
/// 作用域
/// </summary>
public record class MagicScope(
    bool UsableOutsideBattle,       // 战斗外可用
    bool UsableInBattle,            // 战斗中可用
    bool UsableToEnemy,             // 作用于敌方
    bool NeedSelectTarget           // 需要选择目标
);
