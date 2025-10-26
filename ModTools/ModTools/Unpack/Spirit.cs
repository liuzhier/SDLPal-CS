#region License
/*
 * Copyright (c) 2025, liuzhier <lichunxiao_lcx@qq.com>.
 * 
 * This file is part of SDLPAL-CS.
 * 
 * SDLPAL-CS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 3
 * as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */
#endregion License

using Lib.Mod;
using Lib.Pal;
using SimpleUtility;
using SkiaSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;

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
        FileWriter          fileOut;
        int                 i, len, size, j, k, l;
        byte                r, g, b;
        MkfReader           mkf;
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
        pathOut = Config.ModWorkPath.Game.Palette.PathName;
        COS.Dir(pathOut);

        //
        // 打开调色板文件 PAT.MKF
        //
        mkf = new(Config.PalWorkPath.DataBase.Palette);

        //
        // 拆分 PAL.MKF 并保存到 .ACT 文件
        //
        len = mkf.GetChunkCount();
        _palettes = C.malloc(sizeof(PaletteGroup) * len);
        for (i = 0; i < len; i++)
        {
            (pPat, size) = mkf.ReadChunk(i);
            isScenePalette = size > paletteSize;
            pPaletteGroup = ((PaletteGroup*)_palettes) + i;

            j = 0;
            do
            {
                fileOut = new($@"{pathOut}\{(j == 0 ? 'A' : 'B')}{i:D2}.ACT");

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

                fileOut?.Dispose();
            } while (isScenePalette && (++j) == 1);

            //
            // 释放非托管内存
            //
            C.free(pPat);
        }

        //
        // 关闭调色板文件
        //
        mkf?.Dispose();
    }

    public static void Free() => C.free(_palettes);

    /// <summary>
    /// 对 RNG 图像进行导出
    /// </summary>
    /// <param name="pathIn">MKF 文件路径</param>
    /// <param name="pathOut">导出路径</param>
    public static void ProcessRng(string pathIn, string pathOut)
    {
        MkfReader       mkf;
        int             i, count, j, frameCount, packSize;
        nint            pRng, pPack, pFrame;

        //
        // 初始化 Rng 画布
        //
        PalUtil.InitRngFrame();

        mkf = new(pathIn);
        count = mkf.GetChunkCount();
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

            frameCount = mkf.GetRngFrameCount(i) - 1;
            for (j = 0; j < frameCount; j++)
            {
                //
                // 解出一帧的图像
                //
                (pRng, _) = mkf.ReadRngFrame(i, j);
                (pPack, packSize) = PalUtil.Unpack(pRng, Config.IsDosGame);
                (pFrame, _) = PalUtil.UnpackRng(pPack, packSize);

                //
                // 导出为 png
                //
                SaveAsPng(
                    savePath: $@"{pathOut}\{i:D5}\{j:D5}.png",
                    width: 320,
                    height: 200,
                    pixel: pFrame,
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

        //
        // 关闭动画文件 RNG.MKF
        //
        mkf?.Dispose();
    }

    /// <summary>
    /// 对标准游戏图像进行导出
    /// </summary>
    /// <param name="pathIn">MKF 文件路径</param>
    /// <param name="pathOut">导出路径</param>
    /// <param name="isCompressedPack">是否为压缩档</param>
    public static void ProcessPack(string pathIn, string pathOut, int beginId = 0, int endId = -1, bool isCompressedPack = true, bool isFrameSequence = true, bool isRlePack = true, bool haveSubPath = true)
    {
        MkfReader       mkf;
        PalSprite       sprite;
        int             i, count, j, packSize, w, h;
        bool            isEmptyImage;
        nint            pChunk, pFramePak, pFrame;
        string          path;

        mkf = new(pathIn);
        count = mkf.GetChunkCount();
        if (endId == -1 || endId > count)
            endId = count - 1;
        for (i = beginId; i <= endId; i++)
        {
            //
            // 检查 MKF 子块是否为空
            //
            (pChunk, packSize) = mkf.ReadChunk(i);
            isEmptyImage = (packSize == 0);

            if (haveSubPath && isFrameSequence)
            {
                //
                // 创建帧序列输出目录
                //
                COS.Dir($@"{pathOut}\{i:D5}");

                if (isEmptyImage)
                    //
                    // 只为空图像创建一个空目录
                    //
                    continue;
            }

            //
            // 逐帧导出
            //
            sprite = (isEmptyImage) ? null! : new((isCompressedPack) ? PalUtil.Unpack(pChunk, Config.IsDosGame) : (pChunk, packSize));

            j = -1;
            do
            {
                if (isEmptyImage)
                {
                    //
                    // 空图像，直接导出
                    //
                    w = 1;
                    h = 1;
                    C.memset((pFrame = C.malloc(count = w * h)), 0xFF, count);
                    goto startSave;
                }

                if (isFrameSequence)
                {
                    j++;

                    //
                    // 检查下角标是否越界
                    //
                    if (sprite.GetFrameSize(j) <= 0)
                        break;

                    //
                    // 解出一帧的图像
                    //
                    pFramePak = sprite.GetFrame(j);
                }
                else
                    pFramePak = sprite.Buffer;

                if (isRlePack)
                {
                    //
                    // 获取并检查图像尺寸是否不正常
                    //
                    w = sprite.GetFrameWidth((j == -1) ? 0 : j);
                    h = sprite.GetFrameHeight((j == -1) ? 0 : j);

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
                    pFrame = sprite.Buffer;
                }

            startSave:
                //
                // 导出为 png
                //
                path = !haveSubPath ? $@"{j:D5}" : ($"{i:D5}{(isFrameSequence ? $@"\{j:D5}" : "")}");
                SaveAsPng(
                    savePath: $@"{pathOut}\{path}.png",
                    width: w,
                    height: h,
                    pixel: pFrame,
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
                    keyColorId: isRlePack ? 0xFF : -1
                );

                //
                // 释放内存
                //
                if (isRlePack) C.free(pFrame);
            } while (isFrameSequence);

            //
            // 释放内存
            //
            sprite?.Dispose();
            if (isCompressedPack) C.free(pChunk);
        }

        //
        // 关闭图像文件
        //
        mkf?.Dispose();
    }

    static void SaveAsPng(string savePath, int width, int height, nint pixel, int paletteId, bool isNight = false, int keyColorId = -1)
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

        pSrcPixelInt8 = (byte*)pixel;
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
}
