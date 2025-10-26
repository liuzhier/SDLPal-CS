using System.Text.Json.Serialization;

namespace SDLPal.Record.RGame;

public record class Scene
{
    public string? Name { get; init; }              // 名称
    public ushort MapId { get; set; }               // 实际地图
    public SceneScript? Script { get; set; }        // 各种脚本

    [JsonIgnore]
    public Event[]? Events { get; set; }            // 事件列表
}

public record class SceneScript
{
    [JsonIgnore]
    public ushort Enter { get; set; } = 0;     // 脚本：进入场景
    public string? EnterTag { get; init; }

    [JsonIgnore]
    public ushort Teleport { get; set; } = 0;      // 脚本：脱离场景（引路蜂、土灵珠）
    public string? TeleportTag { get; init; }
}
