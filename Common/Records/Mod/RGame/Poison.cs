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

public record class Poison
{
    public string Name { get; set; } = null!;               // 名称
    public int Level { get; set; }                          // 级别/烈度
    public int Color { get; set; }                          // 肖像颜色
    public PoisonScript Script { get; set; } = null!;       // 各种脚本
}

public record class PoisonScript
{
    public string PlayerTag { get; set; } = null!;      // 我方中毒脚本（每次回合结束执行）
    public int Player { get; set; }
    public string EnemyTag { get; set; } = null!;       // 敌方中毒脚本（每次回合结束执行）
    public int Enemy { get; set; }
}
