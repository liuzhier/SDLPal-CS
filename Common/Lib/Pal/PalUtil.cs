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

using Records.Pal;
using SimpleUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Lib.Pal;

public static unsafe partial class PalUtil
{
    /// <summary>
    /// 打开一组二进制文件。
    /// </summary>
    /// <param name="filePath">各个文件所在路径</param>
    /// <returns>返回 BinaryReader[] 对象</returns>
    public static MkfReader[] FileReaderGroup(params string[] filePath)
    {
        List<MkfReader>     gameFileList;

        gameFileList = [];

        foreach (var path in filePath)
            gameFileList.Add(new(path));

        return [.. gameFileList];
    }

    /// <summary>
    /// 关闭一组二进制文件。
    /// </summary>
    /// <param name="fileList">欲关闭的那一组文件</param>
    public static void CloseFileGroup(MkfReader[] fileList)
    {
        foreach (var file in fileList)
            file?.Dispose();
    }

    /// <summary>
    /// 检查 PAL 资源版本。
    /// </summary>
    /// <param name="gamePath">游戏所在路径</param>
    /// <returns>若为 DOS 版游戏资源，返回 true，否则返回 false</returns>
    public static bool CheckVersion(WorkPath workPath)
    {
        bool                isDosGame;
        WorkPathSpirit      spirit;
        MkfReader[]         mkfList;
        int                 count, i, dosFlagCount, dataSize;
        MkfReader           mkfCore;

        //
        // 打开部分资源文件，检查版本
        //
        spirit = workPath.Spirit;
        mkfList = FileReaderGroup(
            spirit.Enemy,
            workPath.DataBase.Map,
            spirit.HeroFight,
            spirit.FightBackPicture,
            spirit.FightEffect,
            spirit.Character
        );

        //
        // 检查有个 DOS 游戏资源文件
        //
        isDosGame = false;
        dosFlagCount = 0;
        foreach (var mkf in mkfList)
        {
            //
            // 获取文件中块的数量
            //
            count = mkf.GetChunkCount();

            //
            // 查找文件中首块非空二进制流
            //
            for (i = 0; i < count && mkf.GetChunkSize(i) < 4; i++) ;

            if (i >= count)
                goto EndCheckVersion;

            if (i < count)
            {
                //
                // 将文件光标定位到块
                //
                mkf.SeekChunk(i);

                //
                // 检查块是否以 YJ_1 开头
                //
                if (mkf.ReadInt32() != 0x315f4a59)
                    goto EndCheckVersion;

                dosFlagCount++;
            }
        }

        //
        // 计算核心文件中对象数据区块的大小
        //
        mkfCore = new(workPath.DataBase.Core);
        dataSize = mkfCore.GetChunkSize(2);
        mkfCore?.Dispose();

        //
        // 检查核心文件中第二个块是否能和 DOS 版资源结构对齐
        //
        if (dosFlagCount == mkfList.Length && dataSize % 12 == 0)
        {
            isDosGame = true;
        }

    EndCheckVersion:
        //
        // 关闭所有二进制文件
        //
        CloseFileGroup(mkfList);

        return isDosGame;
    }

    public static int GetSub16Count(nint pBuffer)
    {
        byte*    lpSprite;

        if (pBuffer == 0)
            return 0;

        lpSprite = (byte*)pBuffer;

        return (short)(lpSprite[0] | (lpSprite[1] << 8));
    }

    /// <summary>
    /// 根据游戏资源版本来选择解码方法
    /// </summary>
    /// <param name="source">源二进制流</param>
    /// <returns>解码后的二进制流和流长度</returns>
    public static (nint buffer, int bufferSize) Unpack(nint source, bool isDosGame) =>
        isDosGame ? UnpackDos(source) : UnpackWin(source);

    /// <summary>
    /// 根据游戏资源版本来选择编码方法
    /// </summary>
    /// <param name="source">源二进制流</param>
    /// <param name="isDosGame">游戏版本</param>
    /// <returns>解码后的二进制流和流长度</returns>
    public static (nint, int) Encode(nint source, int buffSize, bool isDosGame) =>
        isDosGame ? Encodeyj1(source, buffSize) : Encodeyj2(source, buffSize);

    /// <summary>
    /// 根据游戏资源版本来选择编码方法
    /// </summary>
    /// <param name="buffer">源二进制流</param>
    /// <param name="isDosGame">游戏版本</param>
    /// <param name="autoFree">释放自动释放内存，默认为 true</param>
    /// <returns>解码后的二进制流和流长度</returns>
    public static (nint, int) Encode((nint source, int buffSize) buffer, bool isDosGame, bool autoFree = true)
    {
        (nint, int)     outBuffer;

        outBuffer = Encode(buffer.source, buffer.buffSize, isDosGame);

        if (autoFree) C.free(buffer.source);

        return outBuffer;
    }

    public static (nint destination, int length) Decodeyj1(nint source)
    {
        nint        destination;
        uint        length;

        length = 0;
        S.Assert(Decodeyj1(source, out destination, ref length) == PalErrno.Ok);

        return (destination, (int)length);
    }

    public static (nint destination, int length) Encodeyj1(nint source, int bufferSize)
    {
        void*       destination;
        uint        length;

        S.Assert(Encodeyj1((void*)source, (uint)bufferSize, &destination, &length) == PalErrno.Ok);

        return ((nint)destination, (int)length);
    }

    public static (nint destination, int length) Decodeyj2(nint source)
    {
        nint        destination;
        uint        length;

        length = 0;
        S.Assert(Decodeyj2(source, out destination, ref length) == PalErrno.Ok);

        return (destination, (int)length);
    }

    public static (nint destination, int length) Encodeyj2(nint source, int bufferSize)
    {
        void*       destination;
        uint        length;

        S.Assert(Encodeyj2((void*)source, (uint)bufferSize, &destination, &length, 1) == PalErrno.Ok);

        return ((nint)destination, (int)length);
    }

    public static void Decoderng2(nint source, nint PrevFrame) =>
        S.Assert(Decoderng(source, PrevFrame) == PalErrno.Ok);

    public static (nint destination, int length) Encoderle(nint source, int width, int height)
    {
        nint        dest;
        uint        len;

        len = 0;
        PalUtil.Encoderlet(source, 0xFF, width, width, height, out dest, ref len);

        return (dest, (int)len);
        /*
        nint        destination, temp;
        int         rowId, colId, colEndId, length, srcId, destId, count, beginPos;
        byte*       pSrcInt8, pDestInt8;
        ushort*     pSrcInt16, pDestInt16;
        bool        isKeyColor;

        length = width * height;
        destination = temp = C.malloc((length * 2) + (sizeof(ushort) * 2));
        pSrcInt8 = (byte*)source;
        pDestInt8 = (byte*)destination;
        pSrcInt16 = (ushort*)source;
        pDestInt16 = (ushort*)destination;

        //
        // 写入宽高
        //
        pDestInt16[0] = (ushort)width;
        pDestInt16[1] = (ushort)height;
        pDestInt16 = null;
        destId = sizeof(ushort) * 2;

        //if (aaaa++ > 0)
        //    aaaa = aaaa;

        //
        // 开始编码
        //
        for (rowId = 0; rowId < height; rowId++)
        {
            srcId = rowId * width;
            colEndId = srcId + width;

            for (colId = 0; colId < width; colId++)
            {
                //
                // 记录搜索前的位置
                //
                beginPos = srcId;

                //
                // 检查像素是否是透明色
                //
                isKeyColor = (pSrcInt8[srcId] == 0xFF);

                //
                // 统计连续像素个数
                //
                count = 1;
                while (srcId < colEndId && !(isKeyColor ^ (pSrcInt8[++srcId] == 0xFF)) && count < 0x7F)
                    //
                    // 透明色模式下读取到 0xFF 透明色
                    // 或
                    // 非透明色模式下读取到小于 0xFF，非透明色
                    // 像素统计数量增加
                    //
                    count++;

                if (isKeyColor)
                    //
                    // 是透明色，数量需要加上连续透明色的 flag 0x80
                    //
                    pDestInt8[destId++] = (byte)(count | 0x80);
                else
                {
                    //
                    // 非透明色，直接写入数量
                    //
                    pDestInt8[destId++] = (byte)count;

                    //
                    // 写入实际像素
                    //
                    C.memcpy(source + beginPos, destination + destId, count);

                    //
                    // 更新输出缓冲区内存光标
                    //
                    destId += count;
                }
            }
        }

        if (destId % 2 != 0)
            //
            // rle 长度必须为偶数字节，不足补 0
            //
            pDestInt8[++destId] = 0;

        //
        // 重新分配内存
        //
        length = destId + 1;
        destination = C.realloc(temp, length);

        return (destination, length);
        */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveHeader
    {
        public  readonly    uint        Id                  = 0x46464952;       // "RIFF"
        public  uint        Length;                 // 文件长度 - 8
        public  readonly    uint        WaveId              = 0x45564157;       // "WAVE"
        public  readonly    uint        FormatThunk         = 0x20746d66;       // "fmt "
        public  readonly    uint        FormatLength        = 16;               // fmt 块大小 (16)
        public  readonly    ushort      FormatTag           = 1;                // 格式标签 (1 for PCM)
        public  readonly    ushort      Channels            = 1;                // 声道数 (1 for mono)
        public  uint        SamplesPerSec;          // 采样率
        public  uint        AvgBytesPerSec;         // 平均字节率
        public  ushort      BlockAlign;             // 块对齐
        public  readonly    ushort      BitsPerSample       = 8;                // 位深
        public  readonly    uint        DataThunk           = 0x61746164;       // "data"
        public  uint        DataLength;             // data 块大小 

        public WaveHeader() { }
    }

    public static (nint, int) VoiceToWave(nint source, int srclength)
    {
        nint                destination;
        int                 destLength;
        byte*               pSrc, pDest;
        WaveHeader*         pWaveHeader;
        uint                totalAudioLength, sampleRate;
        byte                block;
        MemoryStream        audioDataStream;

        destination = 0;
        destLength = 0;
        audioDataStream = new MemoryStream();

        if (source == 0 || srclength < 0x1A)
            //
            // 源数据异常
            //
            goto end;

        pSrc = (byte*)(source + *(ushort*)(source + 0x14));
        totalAudioLength = 0;
        sampleRate = 0;

        while ((block = *pSrc++) != 0)
        {
            uint length = *(uint*)pSrc & 0x00FFFFFF;
            pSrc += 3;

            switch (block)
            {
                case 1:
                    sampleRate = (uint)(1000000 / (256 - *pSrc++));
                    pSrc++;
                    length -= 2;
                    goto case 2;

                case 2:
                    audioDataStream.Write(new ReadOnlySpan<byte>(pSrc, (int)length));
                    totalAudioLength += length;
                    break;
            }

            pSrc += length;
        }

        if (totalAudioLength == 0 || sampleRate == 0)
            //
            // 无效音频
            //
            goto end;

        destLength = (int)(sizeof(WaveHeader) + totalAudioLength);
        destination = C.malloc(destLength);
        pDest = (byte*)destination;
        *(pWaveHeader = (WaveHeader*)pDest) = new();
        pWaveHeader->SamplesPerSec = sampleRate;
        pWaveHeader->BlockAlign = (ushort)(pWaveHeader->Channels * pWaveHeader->BitsPerSample / 8);
        pWaveHeader->AvgBytesPerSec = pWaveHeader->BlockAlign * pWaveHeader->SamplesPerSec;
        pWaveHeader->DataLength = totalAudioLength;
        pWaveHeader->Length = (uint)sizeof(WaveHeader) - 8 + totalAudioLength;

        audioDataStream.Position = 0;
        audioDataStream.Read(new Span<byte>(pDest + sizeof(WaveHeader), (int)totalAudioLength));

    end:
        audioDataStream?.Dispose();
        return (destination, srclength);
    }
}
