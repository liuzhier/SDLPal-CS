using System;
using System.Text.Json.Serialization;

namespace SDLPal.Record.RGame;

public record class Event(
    string Name,            // 名称
    short VanishTime,       // 剩余隐匿帧数（一般为战斗事件）
    short X,                // X 坐标
    short Y,                // Y 坐标
    short Layer,            // 图层
    short State,            // 触发状态
    ushort TriggerMode,     // 触发模式
    EventSprite Sprite,     // 形象参数
    EventScript Script      // 各种脚本
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


