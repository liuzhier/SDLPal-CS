using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Records.Patch;

public record class PatchIntegrity
{
    [JsonPropertyName("zip_checksum")]
    public string ZipChecksum { get; set; } = string.Empty;

    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = "md5";

    [JsonPropertyName("total_size")]
    public long TotalSize { get; set; }

    [JsonPropertyName("file_count")]
    public int FileCount { get; set; }
}
