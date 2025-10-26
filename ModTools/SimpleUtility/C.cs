using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace SimpleUtility;

/// <summary>
/// 旨在为程序提供类似于 C/C++ 系统 API 的功能。
/// </summary>
public unsafe class C
{
    #region File IO API

    public static FileStream fopen(string name, FileMode mode = FileMode.Open) => File.Open(name, mode);

    public static void fclose(FileStream file) => file.Dispose();

    public static long fseek(FileStream file, int offset, SeekOrigin origin) => file.Seek(offset, SeekOrigin.Begin);

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
        if (pSrc != 0)
           NativeMemory.Free((void*)pSrc);
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

    public static nint memset(Array arr, byte value, long len = -1)
    {
        if (len == -1)
            len = arr.LongLength;

        fixed (byte* pTmp = (byte[])arr)
            return memset((nint)pTmp, value, len);
    }

    public static nint memmove(nint pSrc, nint pDest, long len)
    {
        NativeMemory.Copy((void*)pSrc, (void*)pDest, (nuint)len);
        return pDest;
    }

    public static nint malloc(int len)
    {
        nint        pNative;

        S.Failed(
            "Util.UnpackDos",
            "申请缓冲区失败",
            (pNative = (nint)NativeMemory.AllocZeroed((nuint)len)) != 0
        );

        return pNative;
    }

    #endregion Memory R/W API
}
