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
 * Portions based on PalLibrary by Lou Yihua <louyihua@21cn.com>.
 * Copyright (c) 2006-2007, Lou Yihua.
 * 
 * Ported to C# from C by Li Chunxiao <lichunxiao_lcx@qq.com>.
 * 
 */
#endregion License

using SimpleUtility;
using System;
using System.IO;

namespace Lib.Pal;

public static unsafe partial class PalUtil
{
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
