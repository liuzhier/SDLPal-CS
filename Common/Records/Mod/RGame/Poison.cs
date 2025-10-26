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
