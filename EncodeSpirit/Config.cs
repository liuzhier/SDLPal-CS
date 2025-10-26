using Lib.Mod;
using Lib.Pal;
using SimpleUtility;
using System;
using System.Collections.Generic;
using System.IO;
using RModWorkPath = Records.Mod.WorkPath;
using RWorkPathSpirit = Records.Mod.WorkPathSpirit;
using RWorkPathFightSpirit = Records.Mod.WorkPathFightSpirit;
using RWorkPathUiSpirit = Records.Mod.WorkPathUiSpirit;

namespace EncodeSpirit;

public static unsafe class Config
{
    public static Dictionary<uint, byte>[] Palettes { get; set; } = null!;
    public static RModWorkPath WorkPathIn { get; set; } = null!;
    public static RModWorkPath WorkPathOut { get; set; } = null!;

    /// <summary>
    /// 初始化 MOD 全局数据
    /// </summary>
    public static void Init(bool needRecompile)
    {
        const   int     beginId = 0, colorCount = 0x100, fileSize = colorCount * 3;

        string                      path, suffix;
        int                         count, i, j;
        FileReader                  file;
        nint                        buffer;
        byte*                       pUint8;
        Span<byte>                  span;
        byte                        r, g ,b;
        uint                        color;
        RWorkPathSpirit             spirit;
        RWorkPathFightSpirit        fightSpirit;
        RWorkPathUiSpirit           uiSpirit;

        //
        // 初始化 MOD 目录
        //
#if DEBUG
        path = @"E:\PAL98_v0.89a\palmod";
#else
        path = @".\PalMod";
#endif //DEBUG
        WorkPathIn = new RModWorkPath(path);
        WorkPathOut = new RModWorkPath(path, isTempCompile: true);

        //
        // 检查调色板序列数量
        //
        path = WorkPathIn.Game.Palette.PathName;
        suffix = WorkPathIn.Game.Palette.Suffix;
        count = ModUtil.GetFileSequenceCount(path, beginId, digitNumber: 2, filePrefix: "A", fileSuffix: $".{suffix}");

        //
        // 初始化调色板
        //
        Palettes = new Dictionary<uint, byte>[count];

        //
        // 将颜色载入调色板
        //
        span = new Span<byte>((byte*)(buffer = C.malloc(fileSize)), fileSize);
        for (i = beginId; i < count; i++)
        {
            //
            // 打开颜色表文件
            //
            file = new($@"{path}\A{i:D2}.{suffix}");

            //
            // 读取调色板数据
            //
            span.Clear();
            file.Read(span);

            //
            // 关闭文件
            //
            file?.Dispose();

            //
            // 将颜色放入字典
            //
            Palettes[i] = [];
            pUint8 = (byte*)buffer;
            for (j = 0; j < colorCount; j++, pUint8 += 3)
            {
                r = pUint8[0];
                g = pUint8[1];
                b = pUint8[2];
                color = (uint)(b | (g << 8) | (r << 16) | 0xFF000000);

                Palettes[i][color] = (byte)j;
            }
        }

        //
        // 释放资源
        //
        C.free(buffer);

        //
        // 创建所有目录
        //
        spirit = WorkPathOut.Game.Spirit;
        COS.Dir(spirit.Avatar, isDelExists: needRecompile);
        COS.Dir(spirit.Character, isDelExists: needRecompile);
        COS.Dir(spirit.Item, isDelExists: needRecompile);
        fightSpirit = spirit.Fight;
        COS.Dir(fightSpirit.HeroActionEffect, isDelExists: needRecompile);
        COS.Dir(fightSpirit.Background, isDelExists: needRecompile);
        COS.Dir(fightSpirit.Enemy, isDelExists: needRecompile);
        COS.Dir(fightSpirit.Hero, isDelExists: needRecompile);
        COS.Dir(fightSpirit.Magic, isDelExists: needRecompile);
        uiSpirit = spirit.Ui;
        COS.Dir(uiSpirit.Menu, isDelExists: needRecompile);
        COS.Dir(uiSpirit.DialogueCursor, isDelExists: needRecompile);
    }
}
