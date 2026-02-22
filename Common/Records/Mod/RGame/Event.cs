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

using Records.Pal;
using System.Text.Json.Serialization;

namespace Records.Mod.RGame;

public record class Event
{
    public string Name { get; set; } = null!;               // 名称
    public EventTrigger Trigger { get; set; } = null!;      // 触发器模式
    public EventSprite Sprite { get; set; } = null!;        // 形象参数
    public EventScript Script { get; set; } = null!;        // 各种脚本

    [JsonIgnore]
    public bool InVanishTime => Sprite.VanishTime > 0;
    [JsonIgnore]
    public bool IsVanish => Trigger.StateCode < Core.EventState.Hidden;
    [JsonIgnore]
    public bool IsDisplay => Trigger.StateCode > Core.EventState.Hidden;
    [JsonIgnore]
    public bool IsObstacle => Trigger.StateCode >= Core.EventState.Obstacle;
}

public record class EventTrigger
{
    public Core.EventState StateCode { get; set; }      // 状态码，0 = 隐藏，1 = 显示+漂浮（队伍可穿过）
                                                        // ≥ 2 为显示+实体（队伍不可穿过）
                                                        // 梦幻版领悟“忘剑五诀”条件为 StatusCode == 5）
    public bool IsAutoTrigger { get; set; }             // 是否领队走进范围自动触发
    public int Range { get; set; }                      // 触发范围（-1 = 无法触发，0 = 重合）
}

public record class EventSprite
{
    public int SpriteId { get; set; }               // 形象
    public int FramesPerDirection { get; set; }     // 形象每个方向的帧数
    public int Layer { get; set; }                  // 图层
    public int VanishTime { get; set; }             // 正数为剩余隐匿帧数，负数为逃跑后僵直帧数
                                                    // （一般为战斗事件）
    public Trail Trail { get; set; } = new();       // X 坐标
};

public record class EventScript
{
    public string TriggerTag { get; set; } = null!;     // 触发脚本
    [JsonIgnore]
    public int Trigger { get; set; }
    public string AutoTag { get; set; } = null!;        // 自动脚本
    [JsonIgnore]
    public int Auto { get; set; }
    public int TriggerIdleFrame { get; set; }           // 触发脚本累计被触发次数
    public int AutoIdleFrame { get; set; }              // 自动脚本累计被触发次数
}
