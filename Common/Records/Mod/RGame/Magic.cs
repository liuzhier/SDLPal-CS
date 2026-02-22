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
public record class Magic
{
    public string Name { get; set; } = null!;               // 名称
    public string[]? Description { get; set; }              // 描述
    public int CostMP { get; set; }                         // MP 损耗
    public int BaseDamage { get; set; }                     // 基础伤害
    public int SoundId { get; set; }                        // 仙术音效
    public MagicType Type { get; set; }                     // 仙术系属
    public MagicActionType ActionType { get; set; }         // 行动类型
    public MagicEffect Effect { get; set; } = null!;        // 特效参数
    public MagicScript Script { get; set; } = null!;        // 脚本
    public MagicScope Scope { get; set; } = null!;          // 作用域
}

/// <summary>
/// 特效参数
/// </summary>
public record class MagicEffect
{
    public int EffectId { get; set; }           // 动画
    public int XOffset { get; set; }            // X 轴偏移
    public int YOffset { get; set; }            // Y 轴偏移
    public int LayerOffset { get; set; }        // 图层偏移（实际图层 = Pos.Y + YOffset + LayerOffset）
    public int FrameDelay { get; set; }         // 额外帧延迟（实际帧延迟 = 50ms + FrameDelay * 10ms）
    public int KeepEffect { get; set; }         // 是否将仙术动画最后一帧留在地面上（0 = 否 ）
    public int PreviewFrames { get; set; }      // 预演帧数
                                                // 施法者于 PreviewFrames 期间保持施法集气动作，播放后续帧数时才换为释放法术的动作
                                                // DOS 版音效会等待 PreviewFrames 结束，播放后续帧数时才播放【与后续帧数同步】
                                                // WIN 版则是直接开始播放音效，不等待 PreviewFrames 结束
    public int EffectTimes { get; set; }        // 重复次数（0 = 不重复）
    public int Shake { get; set; }              // 屏幕震动
    public int Wave { get; set; }               // 屏幕波动
}

/// <summary>
/// 脚本
/// </summary>
public record class MagicScript
{
    public string SuccessTag { get; set; } = null!;     // 后序脚本（前序脚本成功才执行）
    [JsonIgnore]
    public int Success { get; set; }
    public string UseTag { get; set; } = null!;         // 前序脚本（一般用于检查是否具备施法条件）
    [JsonIgnore]
    public int Use { get; set; }
    //public string Description { get; set; }             // 描述脚本（不仅能显示描述，还能进行其他的奇葩操作）
    //                                                    // （如比如查看说明顺便全体回血，但是试了一下，没法中毒--）
    //                                                    // 暂不支持！描述脚本由程序自动生成
}

/// <summary>
/// 作用域
/// </summary>
public record class MagicScope
{
    public bool UsableOutsideBattle { get; set; }       // 战斗外可用
    public bool UsableInBattle { get; set; }            // 战斗中可用
    public bool UsableToEnemy { get; set; }             // 作用于敌方
    public bool NeedSelectTarget { get; set; }          // 需要选择目标
}
