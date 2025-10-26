using Lib.Pal;
using SimpleUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Lib.Mod;

public static unsafe class ModUtil
{
    /// <summary>
    /// 获取文件序列数量
    /// </summary>
    /// <param name="fileSequencePath">文件序列路径</param>
    /// <param name="beginId">文件序列起始编号</param>
    /// <param name="filePrefix">文件序列前缀</param>
    /// <param name="fileSuffix">文件序列后缀</param>
    /// <returns>文件序列数量</returns>
    public static int GetFileSequenceCount(string fileSequencePath, int beginId, string filePrefix = "", string fileSuffix = ".json")
    {
        int     i;

        for (i = beginId; ; i++)
            if (!S.FileExist($"{filePrefix}{i:D5}{fileSuffix}", isAssert: false))
                break;

        return i - beginId;
    }

    /// <summary>
    /// 将块地址写入文件头
    /// </summary>
    /// <param name="file">二进制文件对象</param>
    /// <param name="headerId">头部块编号</param>
    /// <param name="chunkAddress">块地址</param>
    public static void WriteMkfHeader(BinaryWriter file, int headerId, int chunkAddress)
    {
        //
        // 文件光标定位到文件头 headerId 处
        //
        file.Seek(headerId * sizeof(uint), SeekOrigin.Begin);

        //
        // 将块地址写入此处
        //
        file.Write(chunkAddress);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file">二进制文件对象</param>
    /// <returns></returns>
    public static (nint, int) ReadBinary(BinaryReader file)
    {
        int         len;
        nint        buffer;

        len = (int)file.BaseStream.Length;

        buffer = C.malloc(len);

        file.Read(new Span<byte>((void*)buffer, len));

        return (buffer, len);
    }

    /// <summary>
    /// 读取二进制文件数据到二进制文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns></returns>
    public static (nint, int) ReadBinary(string filePath)
    {
        BinaryReader        file;
        (nint, int)         result;

        file = PalUtil.BinaryRead(filePath);

        result = ReadBinary(file);

        PalUtil.CloseBinary(file);

        return result;
    }

    /// <summary>
    /// 将 Span 二进制流写入二进制文件末尾
    /// </summary>
    /// <param name="file"></param>
    /// <param name="data"></param>
    public static void AppendBinary(BinaryWriter file, Span<byte> data)
    {
        file.Seek(0, SeekOrigin.End);
        file.Write(data);
    }
}
