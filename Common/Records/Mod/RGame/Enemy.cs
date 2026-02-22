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

namespace Records.Mod.RGame;

public class Enemy : Fighter
{
    public int Cash { get; set; }                           // 战利品：金钱
    public float MagicRate { get; set; }                    // 施法概率
    public int StealItemId { get; set; }                    // 可偷道具
    public int StealItemCount { get; set; }                 // 可偷道具数量
    public int MomovableCount { get; set; }                 // 每回合能连续行动多少次
    public int CollectValue { get; set; }                   // 灵葫能量
    public EnemyScript Script { get; set; } = null!;        // 各种脚本
}

public class EnemyScript
{
    public AddressBase TurnStart { get; set; } = null!;      // 回合开始脚本
    public AddressBase BattleWon { get; set; } = null!;      // 战斗胜利脚本
    public AddressBase Action { get; set; } = null!;         // 回合行动脚本
}
