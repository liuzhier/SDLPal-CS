using SDLPal;
using SimpleUtility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.IO;
using System.Runtime.InteropServices;
using RFightSpirit = SDLPal.Record.RWorkPath.FightSpirit;
using RSpirit = SDLPal.Record.RWorkPath.Spirit;
using RUiSpirit = SDLPal.Record.RWorkPath.UiSpirit;

namespace ModTools.Unpack;

public static unsafe class Spirit
{
    static  readonly    int[]       _rngPaletteID   = [0, 0, 0, 2, 0, 0, 3, 6, 0, 0, 8, 0];
    static  PaletteGroup[]          _paletteGroups  = null!;
    static  PngEncoder              _encoder        = null!;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PaletteGroup
    {
        public  Color[]     Day;
        public  Color[]     Night;
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
        byte                a, r, g, b;
        BinaryReader        filePalette;
        nint                pPalette;
        bool                isScenePalette;
        Span<byte>          span;
        Color[]             palette;

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Palette>");

        //
        // 创建输出目录 Palette
        //
        pathOut = Global.WorkPath.Game.Palette;
        COS.Dir(pathOut);

        //
        // 打开调色板文件 PAT.MKF
        //
        filePalette = Util.BinaryRead(Config.WorkPath.DataBase.Palette);

        //
        // 拆分 PAL.MKF 并保存到 .ACT 文件
        //
        len = Util.GetMkfChunkCount(filePalette);
        _paletteGroups = new PaletteGroup[len];
        for (i = 0; i < len; i++)
        {
            _paletteGroups[i].Day = new Color[256];
            _paletteGroups[i].Night = new Color[256];

            (pPalette, size) = Util.ReadMkfChunk(filePalette, i);
            isScenePalette = size > paletteSize;

            j = 0;
            do
            {
                fileOut = Util.BinaryWrite($@"{pathOut}\{(j == 0 ? 'A' : 'B')}{i:D2}.ACT");

                span = new((byte*)pPalette + (j * paletteSize), paletteSize);

                palette = (j == 0) ? _paletteGroups[i].Day : _paletteGroups[i].Night;

                for (l = 0, k = 0; k < span.Length; l++)
                {
                    r = (span[k++] <<= 2);
                    g = (span[k++] <<= 2);
                    b = (span[k++] <<= 2);
                    a = 0xFF;
                    palette[l] = Color.FromRgba(r, g, b, a);
                }

                fileOut.Write(span);

                Util.CloseBinary(fileOut);
            } while (isScenePalette && (++j) == 1);

            //
            // 释放非托管内存
            //
            C.free(pPalette);
        }

        //
        // 关闭调色板文件
        //
        Util.CloseBinary(filePalette);

        //
        // 初始化 png-8 编码器
        //
        _encoder = new PngEncoder
        {
            ColorType = PngColorType.Palette,
            BitDepth = PngBitDepth.Bit8,
        };
    }

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

        fileIn = Util.BinaryRead(pathIn);
        count = Util.GetMkfChunkCount(fileIn);
        for (i = 0; i < count; i++)
        {
            //
            // 创建输出目录 Animation
            //
            COS.Dir($@"{pathOut}\{i:D5}");

            //
            // 更新 Rng 画布
            //
            Util.FlushRngFrame();

            (pChunk, _) = Util.ReadMkfChunk(fileIn, i);
            frameCount = Util.GetChunkSub32Count(fileIn, i) - 1;
            for (j = 0; j < frameCount; j++)
            {
                //
                // 解出一帧的图像
                //
                (pRng, _) = Util.ReadMkfChunkSub32(fileIn, i, j);
                (pPack, packSize) = Util.Unpack(pRng, Config.IsDosGame);
                (pFrame, frameSize) = Util.UnpackRng(pPack, packSize);

                //
                // 导出为 png
                //
                SaveAsPng(
                    savePath: $@"{pathOut}\{i:D5}\{j:D5}.png",
                    width: 320,
                    height: 200,
                    pPixel: pFrame,
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
        Util.FreeRngFrame();
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

        fileIn = Util.BinaryRead(pathIn);
        count = Util.GetMkfChunkCount(fileIn);
        if (endId == -1 || endId > count)
            endId = count - 1;
        for (i = beginId; i <= endId; i++)
        {
            //
            // 检查 MKF 子块是否有效
            //
            (pChunk, packSize) = Util.ReadMkfChunk(fileIn, i);
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
                (pFrames, packSize) = Util.Unpack(pChunk, Config.IsDosGame);
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
                    if ((frameSize = Util.GetSub16Size(pFrames, packSize, j)) <= 0)
                        break;

                    //
                    // 解出一帧的图像
                    //
                    pFramePak = Util.GetSub16Pointer(pFrames, j);
                }
                else
                    pFramePak = pFrames;

                if (isRlePack)
                {
                    //
                    // 获取并检查图像尺寸是否不正常
                    //
                    w = Util.GetSpriteWidth(pFramePak);
                    h = Util.GetSpriteHeight(pFramePak);

                    if (w <= 0 || h <= 0 || w > 320 || h > 200)
                        continue;

                    //
                    // 对 Rle 进行解码
                    //
                    (pFrame, _) = Util.UnpackRle(pFramePak);
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
                    pPixel: pFrame,
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

        //
        // 释放 Rng 帧内存
        //
        Util.FreeRngFrame();
    }

    static void SaveAsPng(string savePath, int width, int height, nint pPixel, int paletteId, bool isNight = false, int keyColorId = -1)
    {
        Color[]         palette;
        Rgba32          keyColor;
        bool            haveKeyColor;
        int             row, col;
        byte*           pPixelInt8;

        using var image = new Image<Rgba32>(width, height);

        palette = (isNight) ? _paletteGroups[paletteId].Night : _paletteGroups[paletteId].Day;

        keyColor = Color.DeepPink;
        haveKeyColor = keyColorId > 0 && keyColorId <= 0xFF;

        if (haveKeyColor)
        {
            keyColor = palette[keyColorId].ToPixel<Rgba32>();
            keyColor.A = 0x00;
            palette[keyColorId] = Color.FromPixel(keyColor);
        }

        pPixelInt8 = (byte*)pPixel;
        for (row = 0; row < image.Height; row++)
        {
            for (col = 0; col < image.Width; col++)
            {
                image[col, row] = palette[*pPixelInt8++];
            }
        }

        image.Mutate(x => x.Quantize(new PaletteQuantizer(palette)));

        image.SaveAsPng(savePath, _encoder);

        if (haveKeyColor)
        {
            keyColor.A = 0xFF;
            palette[keyColorId] = Color.FromPixel(keyColor);
        }
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
        S.Log("Unpack the game data. <Bitmap>");

        //
        // 创建输出目录 Spirit
        //
        spirit = Global.WorkPath.Game.Spirit;
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
        COS.Dir(uiSpirit.Talk);

        //
        // 导出动画图像
        //
        ProcessRng(Config.WorkPath.Bitmap.Animation, spirit.Animation);

        //
        // 导出其他图像
        //
        ProcessPack(Config.WorkPath.Bitmap.Enemy, fightSpirit.Enemy);
        ProcessPack(Config.WorkPath.Bitmap.Item, spirit.Item, isCompressedPack: false, isFrameSequence: false);
        ProcessPack(Config.WorkPath.Bitmap.HeroFight, fightSpirit.Hero);
        ProcessPack(Config.WorkPath.Bitmap.FightBackPicture, fightSpirit.Background, isFrameSequence: false, isRlePack: false);
        ProcessPack(Config.WorkPath.Bitmap.FightEffect, fightSpirit.Magic);
        ProcessPack(Config.WorkPath.Bitmap.Character, spirit.Character);
        ProcessPack(Config.WorkPath.Bitmap.Avatar, spirit.Avatar, isCompressedPack: false, isFrameSequence: false);
        ProcessPack(Config.WorkPath.DataBase.Base, spirit.Ui.Menu, beginId: 9, endId: 9, isCompressedPack: false, haveSubPath: false);
        ProcessPack(Config.WorkPath.DataBase.Base, fightSpirit.HeroActionEffect, beginId: 10, endId: 10, isCompressedPack: false, haveSubPath: false);
        ProcessPack(Config.WorkPath.DataBase.Base, spirit.Ui.Talk, beginId: 12, endId: 12, isCompressedPack: false, haveSubPath: false);
    }
}
