using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PatchPackageTool.Records;

public record class PatchFileInfo
{
    [JsonPropertyName("filename")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("checksum")]
    public string CheckSum { get; set; } = string.Empty;

    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = "md5";

    [JsonPropertyName("original_size")]
    public long OriginalSize { get; set; }

    [JsonPropertyName("patch_size")]
    public long PatchSize { get; set; }

    [JsonPropertyName("target_path")]
    public string TargetPath { get; set; } = ".";
}
