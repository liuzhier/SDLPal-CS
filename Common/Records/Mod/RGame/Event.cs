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

public record class Event(
    string Name,                // 名称
    short VanishTime,           // 正数为剩余隐匿帧数，负数为逃跑后僵直帧数（一般为战斗事件）
    short X,                    // X 坐标
    short Y,                    // Y 坐标
    short Layer,                // 图层
    EventTrigger Trigger,       // 触发器模式
    EventSprite Sprite,         // 形象参数
    EventScript Script          // 各种脚本
);

public record class EventTrigger(
    ushort StateCode,       // 状态码（0 = 隐藏，1 = 显示+漂浮（队伍可穿过），≥ 2 为显示+实体（队伍不可穿过），梦幻版领悟“忘剑五诀”条件为 StatusCode == 5）
    bool IsAutoTrigger,     // 是否领队走进范围自动触发
    ushort Range            // 触发范围（-1 = 无法触发，0 = 重合）
);

public record class EventSprite
{
    public ushort SpriteId { get; init; }               // 形象
    public ushort SpriteFrames { get; set; }            // 形象每个方向的帧数
    public ushort Direction { get; set; }               // 当前面朝方向
    public ushort CurrentFrameNum { get; init; }        // 当前帧数（当前方向上的）

    [JsonIgnore]
    public ushort SpriteFramesAuto { get; set; }        // 形象总帧数（自动计算，只在内存中有意义）
};

public record class EventScript
{
    [JsonIgnore]
    public ushort Trigger { get; set; } = 0;        // 触发脚本
    public string? TriggerTag { get; init; }

    [JsonIgnore]
    public ushort Auto { get; set; } = 0;       // 自动脚本
    public string? AutoTag { get; init; }

    [JsonIgnore]
    public ushort TriggerIdleFrame { get; set; }        // 触发脚本累计被触发次数

    [JsonIgnore]
    public ushort AutoIdleFrame { get; set; }           // 自动脚本累计被触发次数
}
