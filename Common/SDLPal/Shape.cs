using Records.Mod;
using SDL3;

namespace SDLPal;

/// <summary>
/// 用于绘制图形
/// </summary>
public static unsafe class PalShape
{
    /// <summary>
    /// 绘制边框
    /// </summary>
    /// <param name="rect">区域，包括边框</param>
    /// <param name="borderWidth">边框厚度</param>
    public static void DrawBox(ShapePack pack)
    {
        //
        // 创建背景区域，填充背景颜色
        //
        var surface = COS.Surface(10, 10, PalVideo.DefaultFormat);
        SDL.FillSurfaceRect(surface, 0, pack.Background);

        //
        // 绘制背景区域
        //
        var backgroundPack = new AtlasPack()
        {
            Rect = pack.Rect,
            HorizontalAlign = pack.HorizontalAlign,
            VerticalAlign = pack.VerticalAlign,
        };
        PalAtlas.Scene.Add(new(surface, needFree: true), backgroundPack);

        //
        // 创建边框，填充边框颜色
        //
        surface = COS.Surface(10, 10, PalVideo.DefaultFormat);
        SDL.FillSurfaceRect(surface, 0, pack.BorderColor);

        //
        // 绘制边框
        //
        var rect = pack.Rect;
        PalAtlas.Scene.AddSprite(new(surface, needFree: true));
        PalAtlas.Scene.AddPacks(pack.VerticalAlign switch
        {
            PalVerticalAlign.Middle => [
                new()
                {
                    Rect = new()
                    {
                        X = pack.Rect.X - pack.HorizontalAlign switch
                        {
                            PalHorizontalAlign.Right => pack.Rect.W,
                            PalHorizontalAlign.Middle => pack.Rect.W / 2,
                            _ => 0,
                        },
                        Y = pack.Rect.Y,
                        W = pack.BorderWidth,
                        H = rect.H,
                    },
                    HorizontalAlign = pack.HorizontalAlign,
                    VerticalAlign = pack.VerticalAlign,
                },
                new()
                {
                    Rect = new()
                    {
                        X = pack.Rect.X + pack.HorizontalAlign switch
                        {
                            PalHorizontalAlign.Left => pack.Rect.W,
                            PalHorizontalAlign.Middle => pack.Rect.W / 2,
                            _ => 0,
                        },
                        Y = pack.Rect.Y,
                        W = pack.BorderWidth,
                        H = rect.H,
                    },
                    HorizontalAlign = pack.HorizontalAlign,
                    VerticalAlign = pack.VerticalAlign,
                },
            ],
            _ => [
                new()
                {
                    Rect = new()
                    {
                        X = pack.Rect.X - pack.HorizontalAlign switch
                        {
                            PalHorizontalAlign.Right => pack.Rect.W,
                            _ => 0,
                        },
                        Y = pack.Rect.Y - pack.VerticalAlign switch
                        {
                            PalVerticalAlign.Middle => pack.Rect.H / 2,
                            _ => 0,
                        },
                        W = pack.Rect.W,
                        H = pack.BorderWidth,
                    },
                    HorizontalAlign = pack.HorizontalAlign,
                    VerticalAlign = pack.VerticalAlign,
                },
            ],
        });
    }
}
