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

public record class Scene
{
    public string Name { get; init; } = null!;              // 名称
    public ushort MapId { get; set; }                       // 实际地图
    public SceneScript Script { get; set; } = null!;        // 各种脚本

    [JsonIgnore]
    public Event[]? Events { get; set; }                    // 事件列表
}

public record class SceneScript(
    string Enter,       // 脚本：进入场景
    string Teleport     // 脚本：脱离场景（引路蜂、土灵珠）
);
