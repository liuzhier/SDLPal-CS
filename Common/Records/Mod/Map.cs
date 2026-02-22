using Records.Mod.RGame;
using Records.Ts;
using SDL3;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Records.Mod;

public class MapSprite
{
    public nint Sprite { get; set; }
    public Pos Pos { get; set; } = null!;
    public int LayerHeight { get; set; }
    public bool NeedStretch { get; set; } = true;
    public SDL.FColor ColorMask { get; set; } = DefaultColorMask;
    public TextDrawInfo[] TextDrawInfo { get; set; } = null!;
}

public class SceneSprite
{
    public bool NeedStretch { get; set; } = true;
    public List<nint> Sprite { get; set; } = [];
}

public class Tile
{
    public TileData Low { get; set; } = null!;
    public TileData High { get; set; } = null!;
    public bool IsObstacle { get; set; }
}

public class TileData
{
    public short SpriteId { get; set; }
    public short Height { get; set; }
}

[StructLayout(LayoutKind.Sequential, Size = 3)]
public struct Rgb24
{
    public byte R;
    public byte G;
    public byte B;

    public Rgb24(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public Rgb24(uint rgb)
    {
        R = (byte)((rgb >> 16) & 0xff);
        G = (byte)((rgb >> 8) & 0xff);
        B = (byte)(rgb & 0xff);
    }
}
