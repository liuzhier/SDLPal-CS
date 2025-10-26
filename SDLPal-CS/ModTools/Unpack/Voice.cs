﻿using SDLPal;
using SimpleUtility;

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

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Sound>");

        //
        // 创建输出目录
        //
        pathOut = Global.WorkPath.Game.Voice;
        COS.Dir(pathOut);

        //
        // 打开音效文件
        //
        pathIn = Config.WorkPath.DataBase.Voice.PathName;
        fileIn = Util.BinaryRead(pathIn);
        len = Util.GetMkfChunkCount(fileIn);
        pathIn = Config.WorkPath.DataBase.Voice.Suffix;

        //
        // 解包音效文件到输出目录
        //
        for (i = 0; i < len; i++)
        {
            //
            // 读取 MKF 文件中的分块
            //
            (pBuf, size) = Util.ReadMkfChunk(fileIn, i);

            //
            // 导出二进制文件到输出目录
            //
            pathOutFull = $@"{pathOut}\{i:D5}.{pathIn}";
            fileOut = Util.BinaryWrite(pathOutFull);
            fileOut.Write(new ReadOnlySpan<byte>((void*)pBuf, size));

            //
            // 关闭导出的文件
            //
            Util.CloseBinary(fileOut);
        }

        if (S.FileExist(Config.WorkPath.Music.PathName, isAssert: false))
        {
            //
            // 复制整个音乐文件夹到输出目录
            //
            S.DirCopy(
                Config.WorkPath.Music.PathName,
                $@"*.{pathIn}",
                Global.WorkPath.Game.Musics
            );

            //
            // 导出索引文件
            //
            S.IndexFileSave([.. Message.Music.Values], Config.WorkPath.Music.PathName);
        }
    }
}
