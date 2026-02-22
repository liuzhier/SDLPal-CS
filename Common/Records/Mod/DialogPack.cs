using SDL3;
using static SDLPal.PalDialog;

namespace Records.Mod;

public class DialogPack
{
    public SDL.Rect Rect { get; set; }
    public BackgroundColor BackgroundColor { get; set; }
    public PalHorizontalAlign HorizontalAlign { get; set; }
    public PalVerticalAlign VerticalAlign { get; set; }
}
