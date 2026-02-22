using Records.Mod.RGame;
using SDL3;
using SDLPal;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Records.Mod;

public class Atlas(nint screen) : IDisposable
{
    static readonly int[] DefaultIndicesForPack = [ 0, 1, 2, 0, 2, 3 ];

    int W { get; set; }
    int H { get; set; }
    nint Surface { get; set; }
    List<AtlasUV> UVs { get; set; } = [];
    List<AtlasSprite> Sprites { get; set; } = [];
    List<AtlasPack> Packs { get; set; } = [];
    public nint Screen { get; init; } = screen;
    public nint Texture { get; private set; }
    public SDL.Vertex[] Vertexs { get; set; } = [];
    public int[] Indices { get; set; } = null!;
    public int LastSpriteId => Sprites.Count - 1;
    public int SpriteMargin => (S.Setup.Window.ScaleMode == SDL.ScaleMode.Nearest) ? 1 : 2;

    /// <summary>
    /// 销毁所有数据
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var sprite in Sprites)
            if (sprite.NeedFree)
                FOS.Surface(sprite.Surface);

        DisposeMain();
    }

    /// <summary>
    /// 只销毁主要数据
    /// </summary>
    public void DisposeMain()
    {
        FOS.Texture(Texture);
        Texture = 0;
    }

    /// <summary>
    /// 重新初始化图集
    /// </summary>
    /// <param name="atlas">图集</param>
    public Atlas(Atlas atlas) : this(atlas?.Screen ?? PalScreen.Main) => atlas?.Dispose();

    /// <summary>
    /// 将形象对象放入列表
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns>是否放入成功</returns>
    public bool AddSprite(AtlasSprite sprite)
    {
        if (sprite.Surface == 0) return false;

        Sprites.Add(sprite);

        return true;
    }

    public void AddSprites(List<AtlasSprite> sprites) => Sprites.AddRange(sprites);

    public void AddPack(AtlasPack pack)
    {
        if (pack == null) return;
        pack.SpriteId = LastSpriteId;
        Packs.Add(pack);
    }

    public void AddPacks(List<AtlasPack> packs)
    {
        if (packs == null) return;
        foreach (var pack in packs) AddPack(pack);
    }

    public void Add(AtlasSprite sprite, AtlasPack pack)
    {
        if (AddSprite(sprite))
            AddPack(pack);
    }

    public void MergeSpriteFactions(Atlas atlas)
    {
        foreach (var sprite in atlas.Sprites)
            Sprites.AddRange(sprite);
    }

    public void Merge(Atlas atlas)
    {
        MergeSpriteFactions(atlas);
        Packs.AddRange(atlas.Packs);
    }

    AtlasSprite GetSurface(int spriteId) => (spriteId == -1) ? null! : Sprites[spriteId];

    public nint Build()
    {
        //
        // 估算最终尺寸
        //
        var marginLeft = 0;
        var marginTop = 0;
        var colMaxWidth = 0;
        var registeredSprites = (HashSet<nint>)[];
        foreach (var sprite in Sprites)
        {
            var surface = sprite.Surface;
            if (registeredSprites.TryGetValue(surface, out var _))
                //
                // 已注册该表面，跳过
                //
                continue;

            //
            // 注册贴图，避免重复合并
            //
            registeredSprites.Add(surface);
            S.GetSurfaceSize(sprite.Surface, out var w, out var h);

            //
            // 判断是否需要换列
            //
            if (marginTop != 0 && marginTop + h > 15000)
            {
                marginLeft += colMaxWidth;
                marginTop = 0;
                colMaxWidth = 0;
            }

            // 放到当前列
            marginTop += (h + SpriteMargin);
            colMaxWidth = Math.Max(colMaxWidth, (w + SpriteMargin));

            // 更新最终尺寸
            W = Math.Max(W, marginLeft + colMaxWidth);
            H = Math.Max(H, marginTop);
        }

        //
        // 生成 Surface
        //
        Surface = COS.Surface(W, H, PalVideo.DefaultFormat);

        //
        // 将所有 Surface 合成为一整个 Atlas
        //
        var mergedSprites = (Dictionary<nint, AtlasUV>)[];
        Indices = new int[Packs.Count * DefaultIndicesForPack.Length];
        Vertexs = new SDL.Vertex[Packs.Count * 4];
        var rect = new SDL.Rect();
        var maxWidth = 0;
        foreach (var sprite in Sprites)
        {
            var surface = sprite.Surface;
            if (mergedSprites.TryGetValue(surface, out var mergedUv))
            {
                //
                // 该精灵已经被合并，直接将现成的 uv 放进去，然后处理下一个
                //
                UVs.Add(mergedUv);
                continue;
            }

            S.GetSurfaceSize(sprite.Surface, out var w, out var h);

            //
            // 放置前判断是否换列
            //
            if (rect.Y != 0 && rect.Y + h > 15000)
            {
                rect.X += maxWidth;  // 推进上一列的最大宽
                rect.Y = 0;
                maxWidth = 0;
            }

            //
            // 将画面拷贝到大贴图
            //
            SDL.BlitSurface(sprite.Surface, 0, Surface, rect);

            //
            // 计算贴图的 UV
            //
            var uv = new AtlasUV()
            {
                U0 = 1.0f * rect.X / W,
                V0 = 1.0f * rect.Y / H,
                U1 = 1.0f * (rect.X + w) / W,
                V1 = 1.0f * (rect.Y + h) / H,
            };

            //
            // 注册 UV，避免重复计算
            //
            UVs.Add(mergedSprites[surface] = uv);

            //
            // rect 移动到下一幅图的位置
            //
            rect.Y += (h + SpriteMargin);
            maxWidth = int.Max(maxWidth, (w + SpriteMargin));
        }

        for (var i = 0; i < Packs.Count; i++)
        {
            var pack = Packs[i];
            var colorMask = pack.ColorMask;
            var uv = UVs[pack.SpriteId];
            S.GetSurfaceSize(Sprites[pack.SpriteId].Surface, out var w, out var h);
            if (pack.Rect.W != 0) w = pack.Rect.W;
            if (pack.Rect.H != 0) h = pack.Rect.H;
            w = pack.ActualRect.W = (int)(w * pack.StretchFactor);
            h = pack.ActualRect.H = (int)(h * pack.StretchFactor);
            pack.ActualRect.X = pack.Rect.X;
            pack.ActualRect.Y = pack.Rect.Y;

            //
            // 根据对齐方式自动进行排版
            //
            var drawW = 0;
            var drawH = 0;
            var screenW = 0;
            var screenH = 0;
            var parent = pack.ParentPack;

            if (parent == null)
            {
                //
                // 获取主屏尺寸
                //
                S.GetTexSize(Screen, out var texW, out var texH);
                screenW += (int)texW;
                screenH += (int)texH;
            }
            else
            {
                //
                // 获取父图形尺寸
                //
                screenW = parent.ActualRect.W;
                screenH = parent.ActualRect.H;

                //
                // 相对于父图形进行位置偏移
                //
                var parentRect = parent.ActualRect;
                pack.ActualRect.X += parentRect.X;
                pack.ActualRect.Y += parentRect.Y;
            }

            switch (pack.HorizontalAlign)
            {
                case PalHorizontalAlign.Left:
                    drawW = pack.ActualRect.X + w;
                    break;

                case PalHorizontalAlign.Middle:
                    pack.ActualRect.X += (screenW - w) / 2;
                    goto case PalHorizontalAlign.Left;

                case PalHorizontalAlign.Right:
                    pack.ActualRect.X += (screenW - w);
                    goto case PalHorizontalAlign.Left;

                case PalHorizontalAlign.Stretch:
                    pack.ActualRect.X = 0;
                    drawW = screenW;
                    break;
            }

            switch (pack.VerticalAlign)
            {
                case PalVerticalAlign.Top:
                    drawH = pack.ActualRect.Y + h;
                    break;

                case PalVerticalAlign.Middle:
                    pack.ActualRect.Y += (screenH - h) / 2;
                    goto case PalVerticalAlign.Top;

                case PalVerticalAlign.Bottom:
                    pack.ActualRect.Y += screenH - h;
                    goto case PalVerticalAlign.Top;

                case PalVerticalAlign.Stretch:
                    pack.ActualRect.Y = 0;
                    drawH = screenH;
                    break;
            }

            //
            // 计算顶点
            //
            var vertexsId = i * 4;
            Vertexs[vertexsId] = new()
            {
                TexCoord = new()
                {
                    X = uv.U0,
                    Y = uv.V0,
                },
                Position = new()
                {
                    X = pack.ActualRect.X,
                    Y = pack.ActualRect.Y,
                },
                Color = colorMask,
            };
            Vertexs[vertexsId + 1] = new()
            {
                TexCoord = new()
                {
                    X = uv.U1,
                    Y = uv.V0,
                },
                Position = new()
                {
                    X = drawW,
                    Y = pack.ActualRect.Y,
                },
                Color = colorMask,
            };
            Vertexs[vertexsId + 2] = new()
            {
                TexCoord = new()
                {
                    X = uv.U1,
                    Y = uv.V1,
                },
                Position = new()
                {
                    X = drawW,
                    Y = drawH,
                },
                Color = colorMask,
            };
            Vertexs[vertexsId + 3] = new()
            {
                TexCoord = new()
                {
                    X = uv.U0,
                    Y = uv.V1,
                },
                Position = new()
                {
                    X = pack.ActualRect.X,
                    Y = drawH,
                },
                Color = colorMask,
            };

            //
            // 分配顶点索引
            //
            for (var j = 0; j < DefaultIndicesForPack.Length; j++)
                Indices[i * DefaultIndicesForPack.Length + j] = DefaultIndicesForPack[j] + 4 * i;
        }

        //
        // 重新初始化最终纹理
        //
        FOS.Texture(Texture);
        Texture = COS.Texture(Surface);
        Surface = 0;

        return Texture;
    }

    public void DrawGeometry()
    {
        Build();
        PalScreen.DrawGeometry(this, Screen);
    }
}

public struct AtlasUV
{
    public float U0, V0, U1, V1;
}

public class AtlasSprite(nint surface, bool needFree = false)
{
    public nint Surface { get; set; } = surface;
    public bool NeedFree { get; set; } = needFree;

    public bool Equals(AtlasSprite? other) => other is not null && Surface == other.Surface;
    public override bool Equals(object? obj) => obj is AtlasSprite other && Equals(other);
    public override int GetHashCode() => Surface.GetHashCode();
}

public class AtlasPack
{
    public SDL.Rect Rect;
    public SDL.Rect ActualRect;
    public int SpriteId { get; set; }
    public Range VertexsId { get; set; }
    public SDL.FColor ColorMask { get; set; } = DefaultColorMask;
    public float StretchFactor { get; set; } = 1;
    public PalHorizontalAlign HorizontalAlign { get; set; }
    public PalVerticalAlign VerticalAlign { get; set; }
    public AtlasPack ParentPack { get; set; } = null!;

    //public SDL.Rect GetActualRect() => new()
    //{
    //    X = Rect.X + ActualRect.X,
    //    Y = Rect.Y + ActualRect.Y,
    //    W = ActualRect.W,
    //    H = ActualRect.H,
    //};

    public AtlasPack Clone() => new()
    {
        Rect = Rect,
        ActualRect = ActualRect,
        SpriteId = SpriteId,
        VertexsId = VertexsId,
        ColorMask = ColorMask,
        StretchFactor = StretchFactor,
        HorizontalAlign = HorizontalAlign,
        VerticalAlign = VerticalAlign,
        ParentPack = ParentPack,
    };
}
