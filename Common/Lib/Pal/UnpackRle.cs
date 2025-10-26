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

using SimpleUtility;

namespace Lib.Pal;

public static unsafe partial class PalUtil
{
    public static (nint, int) UnpackRle(nint src)
    {
        int         width, height, pixelsDecoded, len;
        byte        count;
        byte*       pSrc, pPixels;
        nint        dest;

        //
        // 检查源数据是否为空
        //
        if (src == 0) return (0, 0);
        pSrc = (byte*)src;

        //
        // 跳过 0x00000002 开头
        //
        if (*(int*)pSrc == 0x00000002)
            pSrc += 4;

        //
        // 计算图像像素数，每个像素占一字节
        //
        width = ((ushort*)pSrc)[0];
        height = ((ushort*)pSrc)[1];
        len = width * height;
        len = width * height;
        dest = C.malloc(len);
        C.memset(dest, 0xFF, len);
        pPixels = (byte*)dest;

        //
        // 定位到 Rle 编码
        //
        pSrc += 4;

        //
        // 开始处理 Rle 编码
        //
        pixelsDecoded = 0;
        while (pixelsDecoded < len)
        {
            // 读取控制字节
            count = *pSrc++;

            if ((count & 0x80) != 0 && count <= 0x80 + width)
            {
                // 跳过指令
                int skipCount = count - 0x80;

                // 1. 目标画布指针前进
                pPixels += skipCount;

                // 2. 更新已解码的像素数
                pixelsDecoded += skipCount;
            }
            else
            {
                // 绘制指令
                // 循环 count 次，绘制像素
                for (int j = 0; j < count; j++)
                {
                    *pPixels++ = *pSrc++;
                }

                // 更新已解码的像素数
                pixelsDecoded += count;
            }
        }

        return (dest, len);
    }
}
