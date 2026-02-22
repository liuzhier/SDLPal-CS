using Records.Mod;

namespace SDLPal;

public static class PalAtlas
{
    //public static Atlas Scene { get; set; } = null!;
    public static Atlas Scene = null!;
    public static Atlas Text = null!;

    /// <summary>
    /// 初始化全局图集模块
    /// </summary>
    public static void Init()
    {
        Scene = new(PalScreen.Main);
        Text = new(PalScreen.Text);
    }

    /// <summary>
    /// 销毁全局图集模块
    /// </summary>
    public static void Free()
    {
        Scene?.Dispose();
    }
}
