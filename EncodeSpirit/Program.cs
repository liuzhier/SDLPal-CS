using Lib.Pal;
using SimpleUtility;
using SkiaSharp;
using System;
using System.IO;
using RWorkPathFightSpirit = Records.Mod.WorkPathFightSpirit;
using RWorkPathSpirit = Records.Mod.WorkPathSpirit;
using RWorkPathUiSpirit = Records.Mod.WorkPathUiSpirit;

namespace EncodeSpirit;

public unsafe class Program
{
    public static int PaletteId;

    public static (nint buffer, int width, int height) GetBitmapPixels(string pathIn, string pathOut)
    {

        int         width, height, len;
        nint        src, dest;
        uint*       pUInt32;
        byte*       pDestUint8;

        using SKBitmap bitmap = SKBitmap.Decode(pathIn);

        width = bitmap.Width;
        height = bitmap.Height;
        len = width * height;
        pUInt32 = (uint*)(src = bitmap.GetPixels());
        pDestUint8 = (byte*)(dest = C.malloc(len));

        for (int i = 0; i < len; i++)
        {
            pDestUint8[i] = ((pUInt32[i] & 0xFF000000) != 0) ? Config.Palettes[0][pUInt32[i]] : (byte)0xFF;
        }
        bitmap?.Dispose();

        return (dest, width, height);
    }

    public static void Process(string pathIn, string pathOut, bool isRle = true)
    {
        int         width, height, len;
        nint        src, dest;

        //
        // 获取所有像素索引
        //
        (src, width, height) = GetBitmapPixels(pathIn, pathOut);

        if (isRle)
            //
            // 开始编码
            //
            (dest, len) = PalUtil.Encoderle(src, width, height);
        else
        {


            dest = src;
            len = width * height;
        }

        //
        // 将 rle 缓冲区写入文件
        //
        File.WriteAllBytes(pathOut, new Span<byte>((void*)dest, len));
    }

    public static void ProcessRle(string pathIn, string pathOut, bool isSequencePath = true, bool isRle = true)
    {
        int         i, j;
        string      bitmapPath, bitmapDirPath, rlePath, rleDirPath;

        //
        // 处理整个目录
        //
        i = 0;
        do
        {
            if (isSequencePath)
            {
                if (!Directory.Exists(bitmapPath = $@"{pathIn}\{i:D5}"))
                    //
                    // 文件夹不存在，中断处理
                    //
                    return;

                if (!Directory.Exists(rlePath = $@"{pathOut}\{i:D5}"))
                    //
                    // rle 文件夹不存在，创建
                    //
                    COS.Dir(rlePath);
            }
            else
            {
                bitmapPath = pathIn;
                rlePath = pathOut;
            }

            for (j = 0; ; j++)
            {
                if (!File.Exists(bitmapDirPath = $@"{bitmapPath}\{j:D5}.png"))
                    //
                    // 文件不存在，中断处理
                    //
                    break;

                if (File.Exists(rleDirPath = $@"{rlePath}\{j:D5}.rle"))
                    //
                    // rle 包已存在，跳过处理
                    //
                    continue;

                PaletteId = (isRle) ? 0 : i switch
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
                };

                //
                // 将位图压缩为 rle
                //
                Process(bitmapDirPath, rleDirPath);
            }

            i++;
        } while (isSequencePath);
    }

    public static void Main(string[] args)
    {
        RWorkPathSpirit             spiritIn, spiritOut;
        RWorkPathFightSpirit        fightSpiritIn, fightSpiritOut;
        RWorkPathUiSpirit           uiSpiritIn, uiSpiritOut;

#if DEBUG
        Config.Init(true);
#else
        Config.Init(args.Length > 0);
#endif // DEBUG

        spiritIn = Config.WorkPathIn.Game.Spirit;
        spiritOut = Config.WorkPathOut.Game.Spirit;
        ProcessRle(spiritIn.Avatar, spiritOut.Avatar, isSequencePath: false);
        ProcessRle(spiritIn.Character, spiritOut.Character);
        ProcessRle(spiritIn.Item, spiritOut.Item, isSequencePath: false);
        fightSpiritIn = spiritIn.Fight;
        fightSpiritOut = spiritOut.Fight;
        ProcessRle(fightSpiritIn.HeroActionEffect, fightSpiritOut.HeroActionEffect, isSequencePath: false);
        //ProcessRle(fightSpiritIn.Background, fightSpiritOut.Background, isSequencePath: false, isRle: false);
        ProcessRle(fightSpiritIn.Enemy, fightSpiritOut.Enemy);
        ProcessRle(fightSpiritIn.Hero, fightSpiritOut.Hero);
        ProcessRle(fightSpiritIn.Magic, fightSpiritOut.Magic);
        uiSpiritIn = spiritIn.Ui;
        uiSpiritOut = spiritOut.Ui;
        ProcessRle(uiSpiritIn.Menu, uiSpiritOut.Menu, isSequencePath: false);
        ProcessRle(uiSpiritIn.DialogueCursor, uiSpiritOut.DialogueCursor, isSequencePath: false);
    }
}
