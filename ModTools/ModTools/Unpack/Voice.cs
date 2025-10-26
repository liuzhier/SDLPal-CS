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
using System;

namespace ModTools.Unpack;

public static unsafe class Voice
{
    /// <summary>
    /// 解包游戏音效。
    /// </summary>
    public static void Process()
    {
        int                 i, len, size;
        string              suffix, pathOut, pathOutFull;
        nint                pBuf, pFree;
        MkfReader           mkf;
        FileWriter          fileOut;
        bool                musicIsUnpacked;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Voice>");

        //
        // 创建输出目录
        //
        pathOut = Config.ModWorkPath.Game.Voice;
        COS.Dir(pathOut);

        //
        // 打开音效文件
        //
        //fileIn = PalUtil.FileReader(Config.PalWorkPath.DataBase.Voice.PathName);
        //len = PalUtil.GetMkfChunkCount(fileIn);
        //pathIn = Config.PalWorkPath.DataBase.Voice.Suffix;
        mkf = new(Config.PalWorkPath.DataBase.Voice.PathName);
        len = mkf.GetChunkCount();
        suffix = Config.PalWorkPath.DataBase.Voice.Suffix;

        //
        // 解包音效文件到输出目录
        //
        for (i = 1; i < len; i++)
        {
            //
            // 读取 MKF 文件中的分块
            //
            (pBuf, size) = mkf.ReadChunk(i);

            if (Config.IsDosGame)
            {
                //
                // 将 DOS 版的 VOC 音频转为 WAV
                //
                (pBuf, size) = PalUtil.VoiceToWave(pFree = pBuf, size);
                C.free(pFree);
            }

            //
            // 导出二进制文件到输出目录
            //
            //pathOutFull = $@"{pathOut}\{i:D5}.{pathIn}";
            pathOutFull = $@"{pathOut}\{i:D5}.{suffix}";
            fileOut = new(pathOutFull);
            fileOut.Write(new ReadOnlySpan<byte>((void*)pBuf, size));

            //
            // 关闭导出的文件
            //
            fileOut?.Dispose();
        }

        //
        // 关闭音效档
        //
        mkf?.Dispose();

        musicIsUnpacked = false;
        if (Config.IsDosGame)
        {
            //
            // 输出处理进度
            //
            Util.Log("Unpack the game data. <Music>");

            //
            // 创建输出目录
            //
            pathOut = Config.ModWorkPath.Game.Music;
            COS.Dir(pathOut);

            //
            // 打开 DOS 版音乐文件
            //
            mkf = new(Config.PalWorkPath.DataBase.Music.PathName);
            suffix = Config.PalWorkPath.DataBase.Music.Suffix[0];

            len = mkf.GetChunkCount();
            for (i = 1; i < len; i++)
            {
                //
                // 读取 MKF 文件中的分块
                //
                (pBuf, size) = mkf.ReadChunk(i);

                //
                // 导出二进制文件到输出目录
                //
                pathOutFull = $@"{pathOut}\{i:D5}.{suffix}";
                fileOut = new(pathOutFull);
                fileOut.Write(new ReadOnlySpan<byte>((void*)pBuf, size));

                //
                // 关闭导出的文件
                //
                fileOut?.Dispose();
            }

            //
            // 关闭 Rix 音乐档
            //
            mkf?.Dispose();

            //
            // 将音乐解档状态标记为成功
            //
            musicIsUnpacked = true;
        }
        else if (S.DirExist(Config.PalWorkPath.DataBase.Music.PathName, isAssert: false))
        {
            //
            // 复制整个音乐文件夹到输出目录
            //
            S.DirCopy(
                Config.PalWorkPath.DataBase.Music.PathName,
                Config.PalWorkPath.DataBase.Music.Suffix,
                Config.ModWorkPath.Game.Music
            );

            //
            // 将音乐解档状态标记为成功
            //
            musicIsUnpacked = true;
        }

        //
        // 导出索引文件
        //
        if (musicIsUnpacked)
            S.IndexFileSave([.. Message.Music.Values], Config.ModWorkPath.Game.Music);
    }
}
