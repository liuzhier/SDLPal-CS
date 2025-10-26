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

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SimpleUtility;

/// <summary>
/// 旨在为程序提供类似于 C/C++ 系统 API 的功能。
/// </summary>
public unsafe class C
{
    #region File IO API

    public static FileStream fopen(string name, FileMode mode = FileMode.Open) => File.Open(name, mode);

    public static void fclose(FileStream file) => file.Dispose();

    public static long fseek(FileStream file, int offset, SeekOrigin origin) => file.Seek(offset, origin);

    public static int fread(FileStream file, int size, nint dest)
    {
        if (size > 0)
            return file.Read(new Span<byte>((void*)dest, size));

        return -2;
    }

    public static void fwrite(FileStream file, nint src, int size)
    {
        if (size > 0)
            file.Write(new Span<byte>((void*)src, size));
    }

    #endregion File IO API


    #region Memory R/W API

    public static void free(nint pSrc)
    {
        if (pSrc != 0) NativeMemory.Free((void*)pSrc);
    }

    public static void free(ref nint pSrc)
    {
        if (pSrc != 0)
        {
            free(pSrc);

            pSrc = 0;
        }
    }

    public static void free(void* lpSrc)
    {
        if (lpSrc != null)
            NativeMemory.Free(lpSrc);
    }

    public static nint memcpy(nint pSrc, nint pDest, nuint len)
    {
        NativeMemory.Copy((void*)pSrc, (void*)pDest, len);
        return pDest;
    }

    public static nint memset(nint pDest, byte value, long count)
    {
        if (pDest != 0)
            NativeMemory.Fill((void*)pDest, (nuint)count, value);

        return pDest;
    }

    public static nint memmove(nint pSrc, nint pDest, long len)
    {
        NativeMemory.Copy((void*)pSrc, (void*)pDest, (nuint)len);
        return pDest;
    }

    public static void* malloc(nuint len)
    {
        void*       pNative;

        S.Failed(
            "Util.UnpackDos",
            "申请缓冲区失败",
            (pNative = NativeMemory.AllocZeroed(len)) != null
        );

        return pNative;
    }

    public static void* realloc(nint src, nuint len)
    {
        void*       pNative;

        S.Failed(
            "Util.UnpackDos",
            "申请缓冲区失败",
            (pNative = NativeMemory.Realloc((void*)src, len)) != null
        );

        return pNative;
    }

    public static nint malloc(int len) =>
        (nint)malloc((nuint)len);

    public static nint memcpy(nint pSrc, nint pDest, int len) =>
        memcpy(pSrc, pDest, (nuint)len);

    public static nint realloc(nint src, int len) =>
        (nint)realloc(src, (nuint)len);

    #endregion Memory R/W API
}
