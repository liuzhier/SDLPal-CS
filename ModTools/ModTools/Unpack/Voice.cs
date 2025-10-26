using Lib.Mod;
using Lib.Pal;
using ModTools;
using SimpleUtility;
using System;
using System.IO;

namespace ModTools.Unpack;

static unsafe class Voice
{
    /// <summary>
    /// 解包游戏音效。
    /// </summary>
    public static void Process()
    {
        int                 i, len, size;
        string              pathIn, pathOut, pathOutFull;
        nint                pBuf;
        BinaryReader        fileIn;
        BinaryWriter        fileOut;
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
        pathIn = Config.PalWorkPath.DataBase.Voice.PathName;
        fileIn = PalUtil.BinaryRead(pathIn);
        len = PalUtil.GetMkfChunkCount(fileIn);
        pathIn = Config.PalWorkPath.DataBase.Voice.Suffix;

        //
        // 解包音效文件到输出目录
        //
        for (i = 1; i < len; i++)
        {
            //
            // 读取 MKF 文件中的分块
            //
            (pBuf, size) = PalUtil.ReadMkfChunk(fileIn, i);

            //
            // 导出二进制文件到输出目录
            //
            pathOutFull = $@"{pathOut}\{i:D5}.{pathIn}";
            fileOut = PalUtil.BinaryWrite(pathOutFull);
            fileOut.Write(new ReadOnlySpan<byte>((void*)pBuf, size));

            //
            // 关闭导出的文件
            //
            PalUtil.CloseBinary(fileOut);
        }

        //
        // 关闭音效档
        //
        PalUtil.CloseBinary(fileIn);

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
            pathIn = Config.PalWorkPath.DataBase.Music.PathName;
            fileIn = PalUtil.BinaryRead(pathIn);
            len = PalUtil.GetMkfChunkCount(fileIn);
            pathIn = Config.PalWorkPath.DataBase.Music.Suffix;

            len = PalUtil.GetMkfChunkCount(fileIn);
            for (i = 1; i < len; i++)
            {
                //
                // 读取 MKF 文件中的分块
                //
                (pBuf, size) = PalUtil.ReadMkfChunk(fileIn, i);

                //
                // 导出二进制文件到输出目录
                //
                pathOutFull = $@"{pathOut}\{i:D5}.{pathIn}";
                fileOut = PalUtil.BinaryWrite(pathOutFull);
                fileOut.Write(new ReadOnlySpan<byte>((void*)pBuf, size));

                //
                // 关闭导出的文件
                //
                PalUtil.CloseBinary(fileOut);
            }

            //
            // 关闭 Rix 音乐档
            //
            PalUtil.CloseBinary(fileIn);

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
                [$@"*.{pathIn}", $@"*.{Config.PalWorkPath.DataBase.Music.Suffix}"],
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
