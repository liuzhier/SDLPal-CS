using RConfig = Records.Mod.Config;
using RWorkPath = Records.Mod.WorkPath;

namespace SDLPal;

public static class Global
{
    public static RConfig? Config = null!;
    public static RWorkPath WorkPath { get; set; } = null!;
}
