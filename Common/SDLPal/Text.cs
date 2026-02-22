using Records.Mod;
using Records.Mod.RGame;
using SDL3;
using System.Collections.Generic;

namespace SDLPal;

public unsafe static class PalText
{
    public const int
        DefaultSize             = 35,
        LargeSize               = 50,
        LargestSize             = 70,
        ShadowOffset            = 8,
        DefaultTextOffset       = DefaultSize + ShadowOffset + 14;

    public static readonly SDL.Color DefaultColor = ColorWhite;
    public static readonly Dictionary<int, nint> FontDict = [];
    static readonly int[] FontSizes = [25, 30, DefaultSize, 40, 45, LargeSize, 55, 60, 65, LargestSize];

    public static nint TextEngine { get; set; }

    /// <summary>
    /// 初始化文本绘制模块
    /// </summary>
    public static void Init()
    {
        //
        // 初始化 TTF 引擎
        //
        S.SDLFailed("TTF.Init", TTF.Init());
        TextEngine = TTF.CreateRendererTextEngine(PalVideo.Renderer);

        //
        // 制作字形对象
        //
        foreach (var fontSize in FontSizes)
            S.SDLFailed(
               "TTF.OpenFont",
               FontDict[fontSize] = TTF.OpenFont($@"{S.ModAssetsPath.Font}\{S.Setup.Font.TTFFontName}", fontSize)
            );

        //
        // 设置对话框默认位置到画面顶部
        //
        PalDialog.InitDefaultMesssge();
    }

    /// <summary>
    /// 销毁文本绘制模块
    /// </summary>
    public static void Free()
    {
        //
        // 销毁字形对象
        //
        foreach (var fontSize in FontSizes)
            TTF.CloseFont(FontDict[fontSize]);

        //
        // 销毁 TTF 引擎
        //
        TTF.DestroyRendererTextEngine(TextEngine);
        TTF.Quit();
    }

    /// <summary>
    /// 将绘制文本的动作录制到图集
    /// </summary>
    /// <param name="atlas">图集</param>
    /// <param name="infos">文本信息</param>
    /// <returns>图集</returns>
    public static Atlas DrawText(Atlas atlas, TextDrawInfo info)
    {
        if (info?.Text == null) goto Return;

        var text = S.GetTextActualContent(info.Text);
        var pos = info.PosOffset?.Clone() ?? Pos.Zero;

        if (info.VerticalAlign == PalVerticalAlign.Bottom)
            //
            // 画面底端对齐需要给文字阴影留出显示空间
            //
            pos.Y -= ShadowOffset;

        //
        // 先将字形阴影放入 Atlas
        //
        var surface = COS.TextSurface(text, info.FontSize, info.Background);
        var offsetY = ShadowOffset / 2;
        S.GetSurfaceSize(surface, out var w, out var h);
        atlas.AddSprite(new(surface, needFree: true));
        for (var j = 1; j <= ShadowOffset; j++)
            atlas.AddPack(new()
            {
                Rect = new()
                {
                    X = pos.X + j - offsetY,
                    Y = pos.Y + j - offsetY,
                },
                ParentPack = info.ParentPack,
                HorizontalAlign = info.HorizontalAlign,
                VerticalAlign = info.VerticalAlign,
            });

        //
        // 再放入字形本身
        //
        atlas.Add(new(COS.TextSurface(text, info.FontSize, info.Foreground), needFree: true), new()
        {
            Rect = new()
            {
                X = pos.X - offsetY,
                Y = pos.Y - offsetY,
            },
            ParentPack = info.ParentPack,
            HorizontalAlign = info.HorizontalAlign,
            VerticalAlign = info.VerticalAlign,
        });

    Return:
        return atlas;
    }

    /// <summary>
    /// 将绘制文本的动作录制到图集
    /// </summary>
    /// <param name="atlas">图集</param>
    /// <param name="infos"></param>
    /// <returns>图集</returns>
    public static Atlas DrawTexts(Atlas atlas, TextDrawInfo[] infos)
    {
        if (infos != null) foreach (var info in infos) DrawText(atlas, info);

        return atlas;
    }
}
