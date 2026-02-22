namespace Records.Mod;

public class GenerateCommandMenuPack(bool Enabled = true, bool Hidden = false)
{
    public string Text { get; set; } = null!;                   // 选项标题
    public bool Enabled { get; set; } = Enabled && !Hidden;     // 启用
    public bool Hidden { get; set; } = Hidden;                  // 启用
}
