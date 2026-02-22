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

public record class Enemy
{
    public string Name { get; set; } = null!;                       // 名字
    public int Health { get; set; }                                 // 体力
    public int Exp { get; set; }                                    // 战利品：经验值
    public int Cash { get; set; }                                   // 战利品：金钱
    public int Level { get; set; }                                  // 修行
    public int MagicId { get; set; }                                // 法术
    public float MagicRate { get; set; }                            // 施法概率
    public int AttackEquivItemId { get; set; }                      // 普攻附带道具
    public float AttackEquivItemRate { get; set; }                  // 普攻附带道具概率
    public int StealItemId { get; set; }                            // 可偷道具
    public int StealItemCount { get; set; }                         // 可偷道具数量
    public int MomovableCount { get; set; }                         // 每回合能连续行动多少次
    public int CollectValue { get; set; }                           // 灵葫能量
    public BaseAttribute BaseAttribute { get; set; } = null!;       // 五维（武灵防速逃）
    public EnemyResistance Resistance { get; set; } = null!;        // 抗性
    public EnemyEffect Effect { get; set; } = null!;                // 特效参数
    public EnemySound Sound { get; set; } = null!;                  // 行动音效
    public EnemyScript Script { get; set; } = null!;                // 各种脚本
}

public record class EnemySound
{
    public int AttackSound { get; set; }        // 普攻
    public int ActionSound { get; set; }        // 行动
    public int MagicSound { get; set; }         // 施法
    public int DeathSound { get; set; }         // 阵亡
    public int CallSound { get; set; }          // 进入战场时呼喊
}

public record class EnemyResistance
{
    public float Physical { get; set; }                             // 物抗
    public float Poison { get; set; }                               // 毒抗
    public float Sorcery { get; set; }                              // 巫抗
    public ElementalResistance Elemental { get; set; } = null!;     // 灵抗
}

public record class EnemyEffect
{
    public int EffectId { get; set; }           // 形象
    public int IdleFrames { get; set; }         // 原地蠕动帧数
    public int MagicFrames { get; set; }        // 施法帧数
    public int AttackFrames { get; set; }       // 攻击帧数
    public int IdleAnimSpeed { get; set; }      // 原地蠕动动画速度
    public int ActWaitFrames { get; set; }      // 行动动画每帧间固定帧间隔
    public int YPosOffset { get; set; }         // Y 轴偏移
}

public record class EnemyScript
{
    public string TurnStartTag { get; set; } = null!;       // 回合开始脚本
    [JsonIgnore]
    public int TurnStart { get; set; }
    public string BattleWonTag { get; set; } = null!;       // 战斗胜利脚本
    [JsonIgnore]
    public int BattleWon { get; set; }
    public string ActionTag { get; set; } = null!;          // 回合行动脚本
    [JsonIgnore]
    public int Action { get; set; }
}
