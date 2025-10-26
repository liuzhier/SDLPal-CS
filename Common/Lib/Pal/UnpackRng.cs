using SimpleUtility;
using System;
using System.IO;

namespace Lib.Pal;

/// <summary>
/// 这部分代码是 SDLPAL rngplay.c 的一部分
/// https://github.com/sdlpal/sdlpal
/// </summary>
public static unsafe partial class PalUtil
{
    /// <summary>
    /// 将文件光标定位到 MKF 文件中指定的 Sub32 块索引位置。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    /// <param name="sub32chunkId">块中 Sub32 子块编号</param>
    public static void SeekChunkSub32Header(BinaryReader binaryReader, int chunkId, int sub32chunkId)
    {
        //
        // 将光标定位到 MKF 子块
        //
        SeekMkfChunk(binaryReader, chunkId);

        //
        // 将光标定位到 MKF 子块的 Sub32 子块的索引位置
        //
        binaryReader.BaseStream.Seek(sub32chunkId * (int)UnitSize.MKF, SeekOrigin.Current);
    }

    /// <summary>
    /// 将文件光标定位到 MKF 文件中指定的 Sub32 块。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    /// <param name="sub32chunkId">块中 Sub32 子块编号</param>
    public static void SeekMkfChunkSub32(BinaryReader binaryReader, int chunkId, int sub32chunkId)
    {
        int     offset;

        //
        // 将文件光标定位到 Sub32 块头指定块索引处
        //
        SeekChunkSub32Header(binaryReader, chunkId, sub32chunkId);

        //
        // 将光标定位到 Sub32 块
        //
        offset = binaryReader.ReadInt32();
        SeekMkfChunk(binaryReader, chunkId);
        binaryReader.BaseStream.Seek(offset, SeekOrigin.Current);
    }

    /// <summary>
    /// 获取 Sub32 块的帧数量。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    /// <returns>帧数量</returns>
    public static int GetChunkSub32Count(BinaryReader binaryReader, int chunkId)
    {
        //
        // 将文件光标定位到 Sub32 块开头
        //
        SeekMkfChunk(binaryReader, chunkId);

        //
        // 读取文件头部第一个整数，计算块数量
        //
        return binaryReader.ReadInt32() / (int)UnitSize.MKF - 1;
    }

    /// <summary>
    /// 获取 MKF 中指定的 Sub32 块大小。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    /// <param name="sub32chunkId">块中 Sub32 子块编号</param>
    /// <returns>块大小</returns>
    public static int GetMkfChunkSub32Size(BinaryReader binaryReader, int chunkId, int sub32chunkId)
    {
        //
        // 检查块
        //
        SafetyCheckMkfChunk(binaryReader, chunkId);

        //
        // 将文件光标等位到文件头部中的指定块索引
        //
        SeekChunkSub32Header(binaryReader, chunkId, sub32chunkId);

        //
        // 计算块大小
        //
        return -(binaryReader.ReadInt32() - binaryReader.ReadInt32());
    }

    /// <summary>
    /// 获取 MKF 中的指定块的 Sub32 子块
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    /// <param name="sub32chunkId">块中 Sub32 子块编号</param>
    /// <returns>子块非托管内存地址</returns>
    /// <exception cref="Exception">文件为空</exception>
    public static (nint, int) ReadMkfChunkSub32(BinaryReader binaryReader, int chunkId, int sub32chunkId)
    {
        int             chunkLen;
        nint            pDest;
        Span<byte>      span;

        if (binaryReader == null)
            throw new Exception("Unpak.ReadMKFChunk: The file pointer is empty");

        pDest = 0;

        //
        // Get the length of the MKF chunk.
        //
        chunkLen = GetMkfChunkSize(binaryReader, chunkId);

        if (chunkLen != 0)
        {
            //
            // Get the length of the Sub32 chunk.
            //
            chunkLen = GetMkfChunkSub32Size(binaryReader, chunkId, sub32chunkId);

            pDest = C.malloc(chunkLen);
            span = new Span<byte>((void*)pDest, chunkLen);

            SeekMkfChunkSub32(binaryReader, chunkId, sub32chunkId);

            binaryReader.Read(span);
        }

        return (pDest, chunkLen);
    }

    const   int     _pitch = 320, _pixelSize = _pitch * 200;
    static nint _pRngFrame { get; set; }

    /// <summary>
    /// 初始化 Rng 画布
    /// </summary>
    public static void InitRngFrame() => _pRngFrame = C.malloc(_pixelSize);

    /// <summary>
    /// 刷新帧画面，将帧画面填充为黑色
    /// </summary>
    public static void FlushRngFrame() => C.memset(_pRngFrame, 0, _pixelSize);

    /// <summary>
    /// 释放 Rng 画布
    /// </summary>
    public static void FreeRngFrame() => C.free(_pRngFrame);

    /// <summary>
    /// 对二进制流进行解码
    /// </summary>
    /// <param name="pSrc">源二进制流</param>
    /// <param name="length">源二进制流的长度</param>
    /// <param name="checkError">是否检查错误</param>
    /// <returns>解码后的二进制流和流长度</returns>
    public static (nint, int) UnpackRng(nint pSrc, int length, bool checkError = false)
    {
        nint        pDest;
        int         ptr     = 0;
        int         dst_ptr = 0;
        ushort      wdata   = 0;
        int         x, y, i, n;
        byte        data;
        byte*       rng;

        //
        // Check for invalid parameters.
        //
        if (pSrc == 0 && length < 0)
            if (checkError)
                S.Failed(
                    "Util.DrawRNG",
                    "源数据错误"
                );
            else
                return (0, 0);

        //
        // Draw the frame to the surface.
        // FIXME: Dirty and ineffective code, needs to be cleaned up
        //
        rng = (byte*)pSrc;
        pDest = _pRngFrame;
        while (ptr < length)
        {
            data = rng[ptr++];

            switch (data)
            {
                case 0x00:
                case 0x13:
                    //
                    // End
                    //
                    goto end;

                case 0x02:
                    dst_ptr += 2;
                    break;

                case 0x03:
                    data = rng[ptr++];
                    dst_ptr += (data + 1) * 2;
                    break;

                case 0x04:
                    wdata = (ushort)(rng[ptr] | (rng[ptr + 1] << 8));
                    ptr += 2;
                    dst_ptr = (int)(dst_ptr + ((uint)wdata + 1) * 2);
                    break;

                case 0x0a:
                    x = dst_ptr % 320;
                    y = dst_ptr / 320;
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    if (++x >= 320)
                    {
                        x = 0;
                        ++y;
                    }
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    dst_ptr += 2;
                    goto case_0x09;

                case 0x09:
                case_0x09:
                    x = dst_ptr % 320;
                    y = dst_ptr / 320;
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    if (++x >= 320)
                    {
                        x = 0;
                        ++y;
                    }
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    dst_ptr += 2;
                    goto case_0x08;

                case 0x08:
                case_0x08:
                    x = dst_ptr % 320;
                    y = dst_ptr / 320;
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    if (++x >= 320)
                    {
                        x = 0;
                        ++y;
                    }
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    dst_ptr += 2;
                    goto case_0x07;

                case 0x07:
                case_0x07:
                    x = dst_ptr % 320;
                    y = dst_ptr / 320;
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    if (++x >= 320)
                    {
                        x = 0;
                        ++y;
                    }
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    dst_ptr += 2;
                    goto case_0x06;

                case 0x06:
                case_0x06:
                    x = dst_ptr % 320;
                    y = dst_ptr / 320;
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    if (++x >= 320)
                    {
                        x = 0;
                        ++y;
                    }
                    ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                    dst_ptr += 2;
                    break;

                case 0x0b:
                    data = *(rng + ptr++);
                    for (i = 0; i <= data; i++)
                    {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                        if (++x >= 320)
                        {
                            x = 0;
                            ++y;
                        }
                       ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                        dst_ptr += 2;
                    }
                    break;

                case 0x0c:
                    wdata = (ushort)(rng[ptr] | (rng[ptr + 1] << 8));
                    ptr += 2;
                    for (i = 0; i <= wdata; i++)
                    {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                        if (++x >= 320)
                        {
                            x = 0;
                            ++y;
                        }
                       ((byte*)(pDest))[y * _pitch + x] = rng[ptr++];
                        dst_ptr += 2;
                    }
                    break;

                case 0x0d:
                case 0x0e:
                case 0x0f:
                case 0x10:
                    for (i = 0; i < data - (0x0d - 2); i++)
                    {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((byte*)(pDest))[y * _pitch + x] = rng[ptr];
                        if (++x >= 320)
                        {
                            x = 0;
                            ++y;
                        }
                       ((byte*)(pDest))[y * _pitch + x] = rng[ptr + 1];
                        dst_ptr += 2;
                    }
                    ptr += 2;
                    break;

                case 0x11:
                    data = *(rng + ptr++);
                    for (i = 0; i <= data; i++)
                    {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((byte*)(pDest))[y * _pitch + x] = rng[ptr];
                        if (++x >= 320)
                        {
                            x = 0;
                            ++y;
                        }
                       ((byte*)(pDest))[y * _pitch + x] = rng[ptr + 1];
                        dst_ptr += 2;
                    }
                    ptr += 2;
                    break;

                case 0x12:
                    n = (rng[ptr] | (rng[ptr + 1] << 8)) + 1;
                    ptr += 2;
                    for (i = 0; i < n; i++)
                    {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((byte*)(pDest))[y * _pitch + x] = rng[ptr];
                        if (++x >= 320)
                        {
                            x = 0;
                            ++y;
                        }
                       ((byte*)(pDest))[y * _pitch + x] = rng[ptr + 1];
                        dst_ptr += 2;
                    }
                    ptr += 2;
                    break;
            }
        }

    end:
        return (pDest, _pixelSize);
    }
}
