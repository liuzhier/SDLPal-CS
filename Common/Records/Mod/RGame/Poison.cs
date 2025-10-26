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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Records.Mod.RGame;

public record class Poison(
    string Name,            // 名称
    ushort Level,           // 级别/烈度
    ushort Color,           // 肖像颜色
    PosionScript Script     // 各种脚本
);

public record class PosionScript
{
    [JsonIgnore]
    public ushort Player { get; set; } = 0;        // 我方中毒脚本（每次回合结束执行）
    public string? PlayerTag { get; init; }

    [JsonIgnore]
    public ushort Enemy { get; set; } = 0;     // 敌方中毒脚本（每次回合结束执行）
    public string? EnemyTag { get; init; }
}
