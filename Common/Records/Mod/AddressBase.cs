using System.Text.Json.Serialization;

namespace Records.Mod;

public class AddressBase
{
    public string Tag { get; init; } = null!;       // 地址标签
    [JsonIgnore]
    public int Value { get; set; }                  // 实际地址
}
