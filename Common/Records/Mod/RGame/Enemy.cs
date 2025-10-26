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

public record class Enemy(
    string Name,                        // 名字
    ushort Health,                      // 体力
    ushort Exp,                         // 战利品：经验值
    ushort Cash,                        // 战利品：金钱
    ushort Level,                       // 修行
    ushort MagicId,                     // 法术
    ushort MagicRate,                   // 施法概率
    ushort AttackEquivItemId,           // 普攻附带道具
    ushort AttackEquivItemRate,         // 普攻附带道具概率
    ushort StealItemId,                 // 可偷道具
    ushort StealItemCount,              // 可偷道具数量
    bool DualMove,                      // 每回合是否能连续行动两次
    ushort CollectValue,                // 灵葫能量
    BaseAttribute BaseAttribute,        // 五维（武灵防速逃）
    EnemyResistance Resistance,         // 抗性
    EnemyEffect Effect,                 // 特效参数
    EnemySound Sound,                   // 行动音效
    EnemyScript Script                  // 各种脚本
);

public record class EnemySound(
    short AttackSound,      // 普攻
    short ActionSound,      // 行动
    short MagicSound,       // 施法
    short DeathSound,       // 阵亡
    short CallSound         // 进入战场时呼喊
);

public record class EnemyResistance(
    short Physical,                     // 物抗
    short Poison,                       // 毒抗
    short Sorcery,                      // 巫抗
    ElementalResistance Elemental       // 灵抗
);

public record class EnemyEffect(
    ushort EffectId,            // 形象
    ushort IdleFrames,          // 原地蠕动帧数
    ushort MagicFrames,         // 施法帧数
    ushort AttackFrames,        // 攻击帧数
    ushort IdleAnimSpeed,       // 原地蠕动动画速度
    ushort ActWaitFrames,       // 行动动画每帧间固定帧间隔
    short YPosOffset            // Y 轴偏移
);

public record class EnemyScript(
    string TurnStart,       // 回合开始脚本
    string BattleWon,       // 战斗胜利脚本
    string Action           // 回合行动脚本
);
