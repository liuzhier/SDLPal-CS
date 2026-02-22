using Records.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ModTools.Util;

public static unsafe class PaletteManager
{
    const ushort _nightFlag = 0b_1000_0000_0000_0000;

    static Dictionary<int, Rgb24[]> Palettes { get; set; } = [];
    static short PaletteCount { get; set; } = 0;

    public static void InitFromActDir(string dir, string suffix)
    {
        for (PaletteCount = 0; ; PaletteCount++)
        {
            var path = $@"{dir}\A{PaletteCount:D2}.{suffix}";
            if (!File.Exists(path))
                //
                // 中断序列读取
                //
                break;

            //
            // 读取 ACT 颜色表数据 - 昼
            //
            using FileReader day = new(path);
            if (day.Length > 0)
                //
                // 将日间调色板放入列表
                //
                Palettes[PaletteCount] = [.. S.Cast<byte, Rgb24>(day.ReadAllBytes())];

            if (File.Exists(path = $@"{dir}\B{PaletteCount:D2}.{suffix}"))
            {
                //
                // 读取 ACT 颜色表数据 - 夜
                //
                using FileReader night = new(path);
                if (night.Length != 0)
                    //
                    // 将夜间调色板放入列表
                    //
                    Palettes[PaletteCount | _nightFlag] = [.. S.Cast<byte, Rgb24>(night.ReadAllBytes())];
            }
        }
    }

    public static void InitFromMkf(string path)
    {
        //
        // 打开 MKF 文件
        //
        using MkfReader mkf = new(path);

        var count = mkf.GetChunkCount();
        for (PaletteCount = 0; PaletteCount < count; PaletteCount++)
        {
            //
            // 获取每一块数据
            //
            (var buffer, var len) = mkf.ReadChunk(PaletteCount);
            var colors = new Span<Rgb24>((void*)buffer, len / sizeof(Rgb24));

            //
            // 每个颜色分量都要乘以 4
            //
            foreach (ref Rgb24 color in colors)
            {
                color.R <<= 2;
                color.G <<= 2;
                color.B <<= 2;
            }

            //
            // 读取日间调色板
            //
            Palettes[PaletteCount] = [..colors[..256]];

            if ((mkf.GetChunkSize(PaletteCount) / sizeof(Rgb24)) > 256)
                //
                // 读取夜间调色板
                //
                Palettes[PaletteCount | _nightFlag] = [.. colors[256..]];

            //
            // 释放调色板二进制流
            //
            C.free(buffer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paletteId"></param>
    /// <param name="ColorId"></param>
    /// <returns></returns>
    static bool TryGetPalette(int paletteId, out Rgb24[] palette, bool isNight = false)
    {
        if (isNight)
            //
            // 将索引转换为夜间调色板索引
            //
            paletteId |= _nightFlag;

        return Palettes.TryGetValue(paletteId, out palette!);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paletteId"></param>
    /// <param name="ColorId"></param>
    /// <returns></returns>
    static Rgb24[] GetPalette(int paletteId, bool isNight = false)
    {
        S.Failed(
            "PaletteManager.GetColor",
            $"The palette numbered '{paletteId}' cannot be found.",
            TryGetPalette(paletteId, out var palette, isNight)
        );

        return palette!;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paletteId"></param>
    /// <param name="ColorId"></param>
    /// <returns></returns>
    public static Rgb24 GetColor(int paletteId, int ColorId, bool isNight = false)
    {
        try
        {
            return GetPalette(paletteId, isNight)[ColorId];
        }
        catch
        {
            throw S.Failed(
                "PaletteManager.GetColor",
                $"The color with index '{ColorId}' cannot be found in the palette with index '{paletteId}'."
            );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paletteId"></param>
    /// <param name="ColorId"></param>
    /// <returns></returns>
    public static int GetColorId(int paletteId, Rgb24 ColorId, bool isNight = false)
    {
        try
        {
            return GetPalette(paletteId, isNight).IndexOf(ColorId);
        }
        catch
        {
            throw S.Failed(
                "PaletteManager.GetColor",
                $"The color with index '{ColorId}' cannot be found in the palette with index '{paletteId}'."
            );
        }
    }
}
