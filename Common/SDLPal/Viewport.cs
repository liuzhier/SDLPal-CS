using Records.Mod.RGame;
using SDL3;

namespace SDLPal;

public static class PalViewport
{
    public static Pos Offset { get; set; } = new(0, 0);     // 视口偏移

    public static readonly SDL.Rect Rect = new()
    {
        X = -S.UnRatio(PalVideo.Width) / 2,
        Y = -(S.UnRatio(PalVideo.Height) / 2 + S.Ratio(10)),
        W = S.UnRatio(PalVideo.Width),
        H = S.UnRatio(PalVideo.Height),
        //X = -(384 / 2),
        //Y = -(280 / 2 + 16 + 4),
        //W = 384,
        //H = 280,
    };

    public static int MaxX => PalMap.Width - S.UnRatio(PalVideo.Width) - 32;
    public static int MaxY => PalMap.Height - S.UnRatio(PalVideo.Height) - 16;

    static Pos PositionViewport { get; set; } = new Pos(0, 0);
    public static Pos PosV
    {
        get
        {
            PositionViewport.X = Rect.X + Offset.X;
            PositionViewport.Y = Rect.Y + Offset.Y;

            return PositionViewport;
        }
    }

    static Pos Position { get; set; } = new(0, 0);
    public static Pos Pos
    {
        get
        {
            var heroTeamPos = S.GetHeroTeamPos();

            Position.X = PosV.X + heroTeamPos.X;
            Position.Y = PosV.Y + heroTeamPos.Y;

            Position.X = int.Max(Position.X, 0);
            Position.Y = int.Max(Position.Y, 0);

            Position.X = int.Min(Position.X, MaxX);
            Position.Y = int.Min(Position.Y, MaxY);

            return Position;
        }
    }

    static Pos PositionRatio { get; set; } = new(0, 0);
    public static Pos PosR
    {
        get
        {
            PositionRatio.X = S.Ratio(Pos.X);
            PositionRatio.Y = S.Ratio(Pos.Y);

            return PositionRatio;
        }
    }
}
