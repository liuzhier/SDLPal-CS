using Lib.Mod;
using Lib.Pal;
using ModTools;
using SimpleUtility;
using SkiaSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;
using RFightSpirit = Records.Mod.WorkPathFightSpirit;
using RSpirit = Records.Mod.WorkPathSpirit;
using RUiSpirit = Records.Mod.WorkPathUiSpirit;

namespace ModTools.Unpack;

public static unsafe class Spirit
{
    static  readonly    int[]       _rngPaletteID   = [0, 0, 0, 2, 0, 0, 3, 6, 0, 0, 8, 0];
    static  nint        _palettes;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Palette
    {
        public fixed byte       Colors[256 * 4];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PaletteGroup
    {
        public Palette      Day;
        public Palette      Night;
    }

    /// <summary>
    /// 初始化位图导出工具。
    /// </summary>
    public static void Init()
    {
        const int           paletteSize = 256 * 3;

        string              pathOut;
        BinaryWriter        fileOut;
        int                 i, len, size, j, k, l;
        byte                r, g, b;
        BinaryReader        filePalette;
        PaletteGroup*       pPaletteGroup;
        nint                pPat;
        bool                isScenePalette;
        Span<byte>          span;
        SKColor*            pColor;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Palette>");

        //
        // 创建输出目录 Palette
        //
        pathOut = Config.ModWorkPath.Game.Palette;
        COS.Dir(pathOut);

        //
        // 打开调色板文件 PAT.MKF
        //
        filePalette = PalUtil.BinaryRead(Config.PalWorkPath.DataBase.Palette);

        //
        // 拆分 PAL.MKF 并保存到 .ACT 文件
        //
        len = PalUtil.GetMkfChunkCount(filePalette);
        _palettes = C.malloc(sizeof(PaletteGroup) * len);
        for (i = 0; i < len; i++)
        {
            (pPat, size) = PalUtil.ReadMkfChunk(filePalette, i);
            isScenePalette = size > paletteSize;
            pPaletteGroup = ((PaletteGroup*)_palettes) + i;

            j = 0;
            do
            {
                fileOut = PalUtil.BinaryWrite($@"{pathOut}\{(j == 0 ? 'A' : 'B')}{i:D2}.ACT");

                span = new((byte*)pPat + (j * paletteSize), paletteSize);

                pColor = (SKColor*)(((j == 0) ? &pPaletteGroup->Day : &pPaletteGroup->Night)->Colors);

                for (l = 0, k = 0; k < span.Length; l++)
                {
                    r = (span[k++] <<= 2);
                    g = (span[k++] <<= 2);
                    b = (span[k++] <<= 2);
                    pColor[l] = new SKColor(r, g, b);
                }

                fileOut.Write(span);

                PalUtil.CloseBinary(fileOut);
            } while (isScenePalette && (++j) == 1);

            //
            // 释放非托管内存
            //
            C.free(pPat);
        }

        //
        // 关闭调色板文件
        //
        PalUtil.CloseBinary(filePalette);
    }

    public static void Free() => C.free(_palettes);

    /// <summary>
    /// 对 RNG 图像进行导出
    /// </summary>
    /// <param name="pathIn">MKF 文件路径</param>
    /// <param name="pathOut">导出路径</param>
    static void ProcessRng(string pathIn, string pathOut)
    {
        BinaryReader        fileIn;
        int                 i, count, j, frameCount, packSize, frameSize;
        nint                pChunk, pRng, pPack, pFrame;

        //
        // 初始化 Rng 画布
        //
        PalUtil.InitRngFrame();

        fileIn = PalUtil.BinaryRead(pathIn);
        count = PalUtil.GetMkfChunkCount(fileIn);
        for (i = 0; i < count; i++)
        {
            //
            // 创建输出目录 Animation
            //
            COS.Dir($@"{pathOut}\{i:D5}");

            //
            // 更新 Rng 画布
            //
            PalUtil.FlushRngFrame();

            (pChunk, _) = PalUtil.ReadMkfChunk(fileIn, i);
            frameCount = PalUtil.GetChunkSub32Count(fileIn, i) - 1;
            for (j = 0; j < frameCount; j++)
            {
                //
                // 解出一帧的图像
                //
                (pRng, _) = PalUtil.ReadMkfChunkSub32(fileIn, i, j);
                (pPack, packSize) = PalUtil.Unpack(pRng, Config.IsDosGame);
                (pFrame, frameSize) = PalUtil.UnpackRng(pPack, packSize);

                //
                // 导出为 png
                //
                SaveAsPng(
                    savePath: $@"{pathOut}\{i:D5}\{j:D5}.png",
                    width: 320,
                    height: 200,
                    srcPixel: pFrame,
                    paletteId: _rngPaletteID[i],
                    isNight: i == 1,
                    keyColorId: -1
                );

                //
                // 释放内存
                //
                C.free(pRng);
                C.free(pPack);
            }
        }

        //
        // 释放 Rng 帧内存
        //
        PalUtil.FreeRngFrame();
    }

    /// <summary>
    /// 对标准游戏图像进行导出
    /// </summary>
    /// <param name="pathIn">MKF 文件路径</param>
    /// <param name="pathOut">导出路径</param>
    /// <param name="isCompressedPack">是否为压缩档</param>
    static void ProcessPack(string pathIn, string pathOut, int beginId = 0, int endId = -1, bool isCompressedPack = true, bool isFrameSequence = true, bool isRlePack = true, bool haveSubPath = true)
    {
        BinaryReader        fileIn;
        int                 i, count, j, packSize, frameSize, w, h;
        nint                pChunk, pFrames, pFramePak, pFrame;
        string              path;

        fileIn = PalUtil.BinaryRead(pathIn);
        count = PalUtil.GetMkfChunkCount(fileIn);
        if (endId == -1 || endId > count)
            endId = count - 1;
        for (i = beginId; i <= endId; i++)
        {
            //
            // 检查 MKF 子块是否有效
            //
            (pChunk, packSize) = PalUtil.ReadMkfChunk(fileIn, i);
            if (packSize == 0)
                continue;

            if (haveSubPath && isFrameSequence)
            {
                //
                // 创建帧序列输出目录
                //
                COS.Dir($@"{pathOut}\{i:D5}");
            }

            //
            // 逐帧导出
            //
            if (isCompressedPack)
            {
                (pFrames, packSize) = PalUtil.Unpack(pChunk, Config.IsDosGame);
            }
            else
                pFrames = pChunk;
            j = -1;
            do
            {
                if (isFrameSequence)
                {
                    j++;

                    //
                    // 检查下角标是否越界
                    //
                    if ((frameSize = PalUtil.GetSub16Size(pFrames, packSize, j)) <= 0)
                        break;

                    //
                    // 解出一帧的图像
                    //
                    pFramePak = PalUtil.GetSub16Pointer(pFrames, j);
                }
                else
                    pFramePak = pFrames;

                if (isRlePack)
                {
                    //
                    // 获取并检查图像尺寸是否不正常
                    //
                    w = PalUtil.GetSpriteWidth(pFramePak);
                    h = PalUtil.GetSpriteHeight(pFramePak);

                    if (w <= 0 || h <= 0 || w > 320 || h > 200)
                        continue;

                    //
                    // 对 Rle 进行解码
                    //
                    (pFrame, _) = PalUtil.UnpackRle(pFramePak);
                }
                else
                {
                    w = 320;
                    h = 200;
                    pFrame = pFrames;
                }

                //
                // 导出为 png
                //
                path = !haveSubPath ? $@"{j:D5}" : ($"{i:D5}{(isFrameSequence ? $@"\{j:D5}" : "")}");
                SaveAsPng(
                    savePath: $@"{pathOut}\{path}.png",
                    width: w,
                    height: h,
                    srcPixel: pFrame,
                    paletteId: isRlePack ? 0 : i switch
                    {
                        3 => 1,
                        4 => 1,
                        69 => 4,
                        70 => 4,
                        71 => 5,
                        72 => 5,
                        73 => 5,
                        74 => 5,
                        75 => 5,
                        77 => 5,
                        _ => 0,
                    },
                    isNight: false,
                    keyColorId: 0xFF
                );

                //
                // 释放内存
                //
                if (isRlePack)
                    C.free(pFrame);
            } while (isFrameSequence);

            //
            // 释放内存
            //
            if (isCompressedPack)
                C.free(pFrames);
            C.free(pChunk);
        }
    }

    static void SaveAsPng(string savePath, int width, int height, nint srcPixel, int paletteId, bool isNight = false, int keyColorId = -1)
    {
        PaletteGroup*       pPaletteGroup;
        SKColor*            pColor, pDestPixelInt32;
        bool                haveKeyColor;
        nint                destPixel;
        byte*               pSrcPixelInt8;
        int                 row, col, index;
        byte                colorIndex;

        var imageInfo = new SKImageInfo(
            width: width,
            height: height,
            colorType: SKColorType.Bgra8888,
            alphaType: SKAlphaType.Unpremul
        );

        using var bitmap = new SKBitmap(imageInfo);
        using var image = SKImage.FromBitmap(bitmap);

        pPaletteGroup = ((PaletteGroup*)_palettes) + paletteId;
        pColor = (SKColor*)((isNight) ? &pPaletteGroup->Night : &pPaletteGroup->Day)->Colors;

        haveKeyColor = keyColorId > 0 && keyColorId <= 0xFF;

        destPixel = C.malloc(width * height * sizeof(SKColor));
        pDestPixelInt32 = (SKColor*)destPixel;

        pSrcPixelInt8 = (byte*)srcPixel;
        for (row = 0; row < height; row++)
        {
            for (col = 0; col < width; col++)
            {
                index = row * width + col;

                colorIndex = pSrcPixelInt8[index];

                pDestPixelInt32[index] = (haveKeyColor && colorIndex == keyColorId) ? SKColors.Transparent : pColor[colorIndex];
            }
        }

        bitmap.SetPixels(destPixel);

        //
        // 现在像素数据已经设置到位图中，然后编码保存
        //
        using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = File.OpenWrite(savePath))
            data.SaveTo(stream);

        C.free(destPixel);
    }

    /// <summary>
    /// 解档所有位图。
    /// </summary>
    public static void Process()
    {
        RSpirit             spirit;
        RFightSpirit        fightSpirit;
        RUiSpirit           uiSpirit;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Spirit>");

        //
        // 创建输出目录 Spirit
        //
        spirit = Config.ModWorkPath.Game.Spirit;
        COS.Dir(spirit.PathName);
        COS.Dir(spirit.Animation);
        COS.Dir(spirit.Avatar);
        COS.Dir(spirit.Character);
        COS.Dir(spirit.Item);
        fightSpirit = spirit.Fight;
        COS.Dir(fightSpirit.PathName);
        COS.Dir(fightSpirit.HeroActionEffect);
        COS.Dir(fightSpirit.Background);
        COS.Dir(fightSpirit.Enemy);
        COS.Dir(fightSpirit.Hero);
        COS.Dir(fightSpirit.Magic);
        uiSpirit = spirit.Ui;
        COS.Dir(uiSpirit.PathName);
        COS.Dir(uiSpirit.Menu);
        COS.Dir(uiSpirit.DialogueCursor);

        //
        // 导出动画图像
        //
        Util.Log("Unpack the game data. <Spirit: Animation>");
        ProcessRng(Config.PalWorkPath.Spirit.Animation, spirit.Animation);

        //
        // 导出其他图像
        //
        Util.Log("Unpack the game data. <Spirit: Enemy>");
        ProcessPack(Config.PalWorkPath.Spirit.Enemy, fightSpirit.Enemy);
        Util.Log("Unpack the game data. <Spirit: Item>");
        ProcessPack(Config.PalWorkPath.Spirit.Item, spirit.Item, isCompressedPack: false, isFrameSequence: false);
        Util.Log("Unpack the game data. <Spirit: HeroFight>");
        ProcessPack(Config.PalWorkPath.Spirit.HeroFight, fightSpirit.Hero);
        Util.Log("Unpack the game data. <Spirit: FightBackPicture>");
        ProcessPack(Config.PalWorkPath.Spirit.FightBackPicture, fightSpirit.Background, isFrameSequence: false, isRlePack: false);
        Util.Log("Unpack the game data. <Spirit: FightEffect>");
        ProcessPack(Config.PalWorkPath.Spirit.FightEffect, fightSpirit.Magic);
        Util.Log("Unpack the game data. <Spirit: Character>");
        ProcessPack(Config.PalWorkPath.Spirit.Character, spirit.Character);
        Util.Log("Unpack the game data. <Spirit: Avatar>");
        ProcessPack(Config.PalWorkPath.Spirit.Avatar, spirit.Avatar, isCompressedPack: false, isFrameSequence: false);
        Util.Log("Unpack the game data. <Spirit: Ui.Menu>");
        ProcessPack(Config.PalWorkPath.DataBase.Base, spirit.Ui.Menu, beginId: 9, endId: 9, isCompressedPack: false, haveSubPath: false);
        Util.Log("Unpack the game data. <Spirit: HeroActionEffect>");
        ProcessPack(Config.PalWorkPath.DataBase.Base, fightSpirit.HeroActionEffect, beginId: 10, endId: 10, isCompressedPack: false, haveSubPath: false);
        Util.Log("Unpack the game data. <Spirit: DialogueCursor>");
        ProcessPack(Config.PalWorkPath.DataBase.Base, spirit.Ui.DialogueCursor, beginId: 12, endId: 12, isCompressedPack: false, haveSubPath: false);
    }
}
