namespace Records.Mod.RGame;

public class Pos(int X = 0, int Y = 0)
{
    public int X { get; set; } = X;
    public int Y { get; set; } = Y;

    public static Pos Zero => new();

    public Pos Clone() => new(X, Y);

    public static Pos FromBlockPos(BlockPos bpos, bool applyRatio = false) =>
        FromBlockPos(bpos.X, bpos.Y, bpos.H, applyRatio);

    public static Pos FromBlockPos(byte x, byte y, byte h, bool applyRatio = false)
    {
        int      w32, w16, h16, h8;

        w32 = applyRatio ? W32 : 32;
        w16 = applyRatio ? W16 : 16;
        h16 = applyRatio ? H16 : 16;
        h8 = applyRatio ? H8 : 8;

        return new(x * w32 + h * w16, y * h16 + h * h8);
    }

    public override string ToString() => $"({X}, {Y})";
}
