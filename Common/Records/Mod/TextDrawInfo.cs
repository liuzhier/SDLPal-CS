using Records.Mod.RGame;
using SDL3;
using SDLPal;

namespace Records.Mod;

public class TextDrawInfo
{
    public string Text { get; set; } = null!;
    public int FontSize { get; set; } = PalText.DefaultSize;
    public Pos PosOffset { get; set; } = null!;
    public SDL.Color Foreground { get; set; } = ColorWhite;
    public SDL.Color Background { get; set; } = ColorBlack;
    public PalHorizontalAlign HorizontalAlign { get; set; }
    public PalVerticalAlign VerticalAlign { get; set; }
    public TTF.Direction Direction { get; set; } = TTF.Direction.LTR;
    public AtlasPack ParentPack { get; set; } = null!;

    public TextDrawInfo Clone() => new()
    {
        Text = Text,
        FontSize = FontSize,
        PosOffset = PosOffset.Clone(),
        Foreground = Foreground,
        Background = Background,
        HorizontalAlign = HorizontalAlign,
        VerticalAlign = VerticalAlign,
        Direction = Direction,
        ParentPack = ParentPack,
    };
}
