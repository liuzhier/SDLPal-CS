using PatchPackageTool.Records;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Records.Patch;

public record class PatchConfig
{
    [JsonPropertyName("patch_info")]
    public PatchInfo PatchInfo { get; set; } = new PatchInfo();

    [JsonPropertyName("files")]
    public List<PatchFileInfo> Files { get; set; } = [];

    [JsonPropertyName("integrity")]
    public PatchIntegrity Integrity { get; set; } = new PatchIntegrity();
}
