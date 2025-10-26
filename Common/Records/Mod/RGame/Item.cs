using System.Text.Json.Serialization;

namespace Records.Mod.RGame;

public record class Item(
    string Name,                // 名称
    string[]? Description,      // 描述
    ushort BitmapId,            // 图像
    ushort Price,               // 售价（典当半价）
    ItemScript Script,          // 脚本
    ItemScope Scope             // 作用域
);

public record class ItemScript
{
    [JsonIgnore]
    public ushort Use { get; set; } = 0;            // 使用脚本
    public string? UseTag { get; init; }

    [JsonIgnore]
    public ushort Equip { get; set; } = 0;          // 装备脚本
    public string? EquipTag { get; init; }

    [JsonIgnore]
    public ushort Throw { get; set; } = 0;          // 投掷脚本
    public string? ThrowTag { get; init; }

    [JsonIgnore]
    public ushort Description { get; set; } = 0;    // 描述脚本（不仅能显示描述，还能进行其他的奇葩操作）
    public string? DescriptionTag { get; init; }    // （如比如查看说明顺便全体回血，但是试了一下，没法中毒--）
}

public record class ItemScope(
    bool Usable,            // 可使用
    bool Equipable,         // 可装备
    bool Throwable,         // 可投掷
    bool Consuming,         // 使用后减少
    bool ApplyToAll,        // 作用于全体
    bool Sellable,          // 可典当
    string WhoCanEquip      // 可装备者，例：
                            // 丝衣-李赵林（123）
                            // 圣灵珠-赵巫（24）
                            // 豹牙手环-全体（#）
                            // 竹笛-巫后除外（#4）
                            // 丝衣-女性（#1）
                            // 鳳紋披風-女性，巫后除外（#14）
);
