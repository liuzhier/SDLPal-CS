using ModTools.Unpack;
using ModTools.Util;
using Records.Mod;
using Records.Mod.RGame;
using SDL3;
using System.IO;

namespace SDLPal;

public static unsafe class PalMap
{
    public const short
      Width     = 2064,       // 地图宽度
      Height    = 2055,       // 地图高度
      Ratio     = 2;          // 瓦片贴图放大到的比率

    public static int MapID { get; set; }
    public static Tile[,,] Tiles { get; set; } = new Tile[128, 64, 2];
    public static nint TileSprite { get; set; }
    public static nint Palette { get; set; }
    public static nint TileRect { get; set; }
    public static nint MapTile { get; set; }
    public static nint MapTile2 { get; set; }
    public static nint[] TileSurfaces { get; set; } = null!;

    /// <summary>
    /// 初始化地图模块
    /// </summary>
    public static void Init()
    {
        string            path;
        byte[]            arrData;
        int               len;

        //
        // 如果地图调色板文件不存在，将会出现错误提示，并且游戏将会退出。
        //
        S.FileExist(path = S.ModPath.Assets.MapData.Palette);

        //
        // 初始化地图调色板
        //
        arrData = File.ReadAllBytes(path);
        fixed (byte* bpTmp = arrData)
        {
            (Palette, len) = COS.Palette((nint)(bpTmp + sizeof(uint) * 2), arrData.Length);
        }

        TileRect = COS.Rect(new()
        {
            X = 0,
            Y = 0,
            W = W32,
            H = H15,
        });

#if DEBUG
        //
        // 初始化障碍块瓦片
        //
        MapTile = COS.Texture($@"{S.ModPath.Temp}\MapTile.png");
#endif // DEBUG
    }

    /// <summary>
    /// 销毁地图模块
    /// </summary>
    public static void Free()
    {
        FreeTile();
        FOS.Palette(Palette);
        C.free(TileRect);
    }

    /// <summary>
    /// 销毁所有瓦片贴图
    /// </summary>
    public static void FreeTile()
    {
        //
        // 销毁所有瓦片贴图
        //
        if (TileSurfaces != null)
            for (var i = 0; i < TileSurfaces.Length; i++)
                FOS.Texture(TileSurfaces[i]);
#if DEBUG
        //
        // 销毁障碍块瓦片
        //
        FOS.Texture(MapTile);
#endif // DEBUG
    }

    /// <summary>
    /// 加载指定的地图以及瓦片
    /// </summary>
    /// <param name="mapId">地图编号</param>
    public static void Load(int mapId)
    {
        //
        // 读取贴图数据
        // 每 4 个字节为一组数据（一块），以下为每个位的意义
        // －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
        // １１１１１１１１　００１１１１１１　１１１１１１１１　０００１１１１１　（无效数据位）
        // 　　　　　　　　　￣￣　　　　　　　　　　　　　　　　￣￣￣
        // 　　　　　　　　　　↓　　　　　　　　　　　　　　　　　↓
        // 　　　　　　　　无效数据位　　　　　　　　　　　　　无效数据位
        // －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
        // １１１１１１１１　００１１１１１１　１１１１１１１１　０００１１１１１　（障碍块）
        // 　　　　　　　　　　　￣
        // 　　　　　　　　　　　↓
        // 　　　　　　　　　是否为障碍块
        // －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
        // １１１１１１１１　００１１１１１１　１１１１１１１１　０００１１１１１　（Low Tile Sprite ID，最大为 511）
        // ￣￣￣￣￣￣￣￣　　　　￣　　　　　　　　　　　　　　　　　　　　　　　（注意与 High Tile Sprite ID 区别）
        // 　　　　↓　　　　　　　↓　　　　　　　　　　　　　　　　　　　　　　　（Low Tile 是必须存在的）
        // 　　　低８位　　　　　高１位
        // －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
        // １１１１１１１１　００１１１１１１　１１１１１１１１　０００１１１１１　（Low Tile 图层/高度，最大为 15）
        // 　　　　　　　　　　　　　￣￣￣￣
        // 　　　　　　　　　　　　　　↓
        // 　　　　　　　　　　　　　只有低４位
        // －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
        // １１１１１１１１　００１１１１１１　１１１１１１１１　０００１１１１１　（High Tile Sprite ID，最大为 510）
        // 　　　　　　　　　　　　　　　　　　￣￣￣￣￣￣￣￣　　　　￣　　　　　（注意与 Low Tile Sprite ID 区别）
        // 　　　　　　　　　　　　　　　　　　　　　　↓　　　　　　　↓　　　　　（实际存储数值 + 1，0 代表无贴图）
        // 　　　　　　　　　　　　　　　　　　　　　低８位　　　　　高１位　　　　（因为 High Tile 不是必须存在的）
        // －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
        // １１１１１１１１　００１１１１１１　１１１１１１１１　０００１１１１１　（High Tile 图层/高度，最大为 15）
        // 　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　￣￣￣￣
        // 　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　↓
        // 　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　只有低４位
        // －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
        // 每两组为一对（8 字节），每 128 组（0x200字节）为一行。组对方式：(0, 1) (2, 3) (4, 5) (6, 7) ... (126, 127)
        // 以下是每对数据在地图上的排列方式（左上/右下）。每对数据的 XY 相同，但 H 区别（左上 = 0，右下 = 1）
        // ＋－－－－－－－－－－－－－－－－－－－－－－－→Ｘ
        // ｜0   　2   　4   　6   　8   　10  　...　126　　(y = 0, h = 0)
        // ｜　1   　3   　5   　7   　9   　11  　...　127　(y = 0, h = 1)
        // ｜129 　131 　133 　135 　137 　139 　...　254　　(y = 1, h = 0)
        // ｜　130 　132 　134 　136 　138 　140 　...　255　(y = 1, h = 1)
        // ｜．．．．．．．．．．．．．．．．．．．．．．．　(y = 2, h = 0)
        // ｜．．．．．．．．．．．．．．．．．．．．．．．　(y = 2, h = 1)
        // ↓
        // Ｙ
        //
        var path = $@"{S.ModPath.Assets.MapData.Map}\map{mapId:D4}";
        S.FileExist(path);
        using (FileReader file = new(path))
            for (var y = 0; y < Tiles.GetLength(0); y++)
                for (var x = 0; x < Tiles.GetLength(1); x++)
                    for (var h = 0; h < Tiles.GetLength(2); h++)
                    {
                        byte[] bytes = [file.ReadByte(), file.ReadByte(), file.ReadByte(), file.ReadByte()];
                        Tiles[y, x, h] = new()
                        {
                            IsObstacle = (bytes[1] & 0b_0010_0000) != 0,
                            Low = new()
                            {
                                SpriteId = (short)(bytes[0] | ((bytes[1] & 0b_0001_0000) << 4)),
                                Height = (byte)(bytes[1] & 0b_0000_1111),
                            },
                            High = new()
                            {
                                SpriteId = (short)((bytes[2] | ((bytes[3] & 0b_0001_0000) << 4)) - 1),
                                Height = (byte)(bytes[3] & 0b_0000_1111),
                            },
                        };
                    }

        //
        // 读取瓦片贴图
        //
        S.FileExist(path = $@"{S.ModPath.Assets.MapData.Tile}\Tile{mapId:D4}");
        int tileSpriteSize;
        using (FileReader file = new(path))
        {
            //
            // 跳过文件头的文件长度
            //
            file.Seek(sizeof(int), SeekOrigin.Begin);

            //
            // 开始读取数据
            //
            tileSpriteSize = (int)file.Length - sizeof(int);
            file.Read(new((void*)(TileSprite = C.malloc(tileSpriteSize)), tileSpriteSize));
        }

        //
        // 初始化临时瓦片 Surface
        // 设置调色板，将调色板最后一种颜色映射为透明色
        //
        var surface = COS.Surface(32, 15, SDL.PixelFormat.Index8);
        SDL.SetSurfacePalette(surface, Palette);
        SDL.SetSurfaceColorKey(surface, true, 0xFF);

        //
        // 初始化瓦片贴图列表
        //
        TileSurfaces = new nint[PalUtil.GetSub16Count(TileSprite)];

        //
        // 初始化所有地图瓦片纹理
        //
        using var sprite = new PalSprite((TileSprite, tileSpriteSize));
        for (var i = 0; i < TileSurfaces.Length; i++)
        {
            //
            // 初始化 8 位地图瓦片表面，将地图瓦片绘制上去
            //
            var surfaceScaled = COS.Surface(32, 15, SDL.PixelFormat.RGBA8888);
            SDL.FillSurfaceRect(surface, 0, 0xFF);
            PalUtil.UnpackRle(sprite.GetFrame(i), ((SDL.Surface*)surface)->Pixels);

            //
            // 将 8 位 Surface 绘制到 32 位 Surface
            //
            SDL.FillSurfaceRect(surfaceScaled, 0, 0x00000000);
            SDL.BlitSurfaceScaled(surface, 0, surfaceScaled, 0, SDL.ScaleMode.Nearest);

            //
            // 将瓦片放入列表中
            //
            TileSurfaces[i] = surfaceScaled;
        }

        //
        // 销毁临时 Surface
        //
        FOS.Surface(surface);
    }

    /// <summary>
    /// 检查指定位置的瓷砖是否被阻挡
    /// </summary>
    /// <param name="block">快坐标</param>
    /// <returns></returns>
    public static bool TileIsObstacle(BlockPos block)
    {
        //
        // 检查参数有效性
        //
        if (block.X >= 64 || block.Y >= 128 || block.H > 1) return true;

        return Tiles[block.Y, block.X, block.H].IsObstacle;
    }

    /// <summary>
    /// 将绘制地图的动作录制到图集
    /// </summary>
    /// <param name="atlas">图集</param>
    /// <param name="layer">表示层。0 代表低层，1 代表高层</param>
    public static void Draw(Atlas atlas, int layer)
    {
        //
        // 计算实际的矩形面积
        //
        var ox = PalViewport.Pos.X;
        var oy = PalViewport.Pos.Y;

        //
        // 将坐标进行转换
        //
        var sy = oy / 16 - 1;
        var dy = (oy + PalViewport.Rect.H) / 16 + 2;
        var sx = ox / 32 - 1;
        var dx = (ox + PalViewport.Rect.W) / 32 + 2;

        //
        // 开始绘制
        //
        var yPos = sy * 16 - 8 - oy;
        for (var y = sy; y < dy; y++)
        {
            for (var h = 0; h < 2; h++, yPos += 8)
            {
                var xPos = sx * 32 + h * 16 - 16 - ox;
                for (var x = sx; x < dx; x++, xPos += 32)
                {
                    var surface = GetTileSurface(new BlockPos((byte)x, (byte)y, (byte)h), layer);
                    if (surface == 0)
                    {
                        //
                        // 未能获取区块图像时，直接使用 Tile 序列中的第一个图像
                        //
                        if (layer != 0) continue;

                        surface = GetTileSurface(new BlockPos(0, 0, 0), layer);
                    }

                    atlas.Add(new(surface), new()
                    {
                        Rect = new()
                        {
                            X = S.Ratio(xPos),
                            Y = S.Ratio(yPos),
                        },
                        StretchFactor = S.Ratio(1),
                    });
                }
            }
        }
    }

    /// <summary>
    /// 获取指定层中位于位置 (x, y, h) 处的瓷砖表面
    /// </summary>
    /// <param name="bpos">该瓦片的坐标</param>
    /// <param name="layer">表示层。0 代表低层，1 代表高层</param>
    /// <returns>指定位置的瓷砖表面</returns>
    public static nint GetTileSurface(BlockPos bpos, int layer)
    {
        //
        // 检查无效参数
        //
        if (bpos.X >= 64 || bpos.Y >= 128 || bpos.H > 1) return 0;

        //
        // 获取指定位置的图块数据
        //
        var tileData = Tiles[bpos.Y, bpos.X, bpos.H];

        if (layer != 0)
        {
            if (tileData.High.SpriteId < 0) return 0;

            return TileSurfaces[tileData.High.SpriteId];
        }
        else
            return TileSurfaces[tileData.Low.SpriteId];
    }

    /// <summary>
    /// 获取指定图块的逻辑高度值，此值用于判断图块位图是否应覆盖精灵
    /// </summary>
    /// <param name="bpos">块坐标</param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static int GetTileLayer(BlockPos bpos, int layer)
    {
        //
        // 检查无效参数，默认返回 0
        //
        if (bpos.X >= 64 || bpos.Y >= 128 || bpos.H > 1) return 0;

        //
        // 获取指定位置的图块数据
        //
        var tileData = Tiles[bpos.Y, bpos.X, bpos.H];

        return (layer != 0) ? tileData.High.Height : tileData.Low.Height;
    }
}
