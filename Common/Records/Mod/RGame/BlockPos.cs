namespace Records.Mod.RGame;

public class BlockPos(byte X, byte Y, byte H)
{
    public byte X { get; set; } = X;
    public byte Y { get; set; } = Y;
    public byte H { get; set; } = H;

    public static BlockPos FromPos(Pos pos, bool applyRatio = false) =>
        FromPos(pos.X, pos.Y, applyRatio);

    public static BlockPos FromPos(int X, int Y, bool applyRatio = false)
    {
        var w32 = applyRatio ? W32 : 32;
        //var w16 = applyRatio ? W16 : 16;
        //var h16 = applyRatio ? H16 : 16;
        //var h8 = applyRatio ? H8 : 8;

        var h = (byte)(((X % w32) != 0) ? 1 : 0);
        var x = (byte)(X / 32);
        var y = (byte)(Y / 16);

        if (X < 0 || Y < 0)
        {
            x = 0xFF;
            y = 0xFF;
            h = 1;
        }

        return new BlockPos(x, y, h);
    }

    public override string ToString() => $"({X}, {Y}, {H})";
}
