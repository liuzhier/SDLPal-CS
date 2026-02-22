using SDL3;

namespace Records.Mod;

public class ShapePack
{
    public SDL.Rect Rect { get; set; }
    public int BorderWidth { get; set; } = 4;
    public uint Background { get; set; } = 0x00_00_FF_5F;
    public uint BorderColor { get; set; } = 0x00_FF_00_FF;
    public PalHorizontalAlign HorizontalAlign { get; set; }
    public PalVerticalAlign VerticalAlign { get; set; }
}
