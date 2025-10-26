using System;
using System.Text.Json.Serialization;

namespace PatchPackageTool.Records;

public record class PatchInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("create_date")]
    public string CreateDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("compatibility")]
    public string Compatibility { get; set; } = "仙剑98 v1.0";
}
