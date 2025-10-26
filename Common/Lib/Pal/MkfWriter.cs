using SimpleUtility;
using System;
using System.IO;

namespace Lib.Pal;

public unsafe class MkfWriter(string path) : IDisposable
{
    ~MkfWriter() => Dispose();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _file?.Dispose();
    }

    FileWriter _file { get; init; } = new(path);
    public long Length => _file.Length;
    public void Seek(int offset, SeekOrigin origin) => _file.Seek(offset, origin);
    public void Write(ReadOnlySpan<byte> buffer) => _file.Write(buffer);
    public void Write(int value) => _file.Write(value);
    public void SetLength(long value) => _file.SetLength(value);

    /// <summary>
    /// 将块地址写入文件头
    /// </summary>
    /// <param name="headerId">头部块编号</param>
    /// <param name="chunkAddress">块地址</param>
    public void WriteHeader(int headerId, int chunkAddress)
    {
        //
        // 文件光标定位到文件头 headerId 处
        //
        Seek(headerId * sizeof(uint), SeekOrigin.Begin);

        //
        // 将块地址写入此处
        //
        Write(chunkAddress);
    }

    /// <summary>
    /// 将 Span 二进制流写入二进制文件末尾
    /// </summary>
    /// <param name="data">二进制数组</param>
    public void Append(byte[] data)
    {
        Seek(0, SeekOrigin.End);
        Write(data);
    }

    /// <summary>
    /// 将 Span 二进制流写入二进制文件末尾，自动释放内存
    /// </summary>
    /// <param name="buffer">二进制流</param>
    /// <param name="beginPos">从 buffer 的哪个位置开始写入 mkf，默认为 0</param>
    /// <param name="autoFree">释放自动释放内存，默认为 true</param>
    public void Append((nint pBinary, int length) buffer, int beginPos = 0, bool autoFree = true)
    {
        Seek(0, SeekOrigin.End);
        Write(new Span<byte>((void*)buffer.pBinary, buffer.length)[beginPos..]);
        if(autoFree) C.free(buffer.pBinary);
    }

    /// <summary>
    /// 将 Span 二进制流写入二进制文件末尾，自动释放内存
    /// </summary>
    /// <param name="buffer">二进制流</param>
    /// <param name="beginPos">从 buffer 的哪个位置开始写入 mkf，默认为 0</param>
    /// <param name="autoFree">释放自动释放内存，默认为 true</param>
    public void AppendPalette((nint pBinary, int length) buffer, bool autoFree = true)
    {
        Span<byte>      span;
        int             i;

        span = new Span<byte>((void*)buffer.pBinary, buffer.length);

        for (i = 0; i < span.Length; i++) span[i] >>= 2;

        Seek(0, SeekOrigin.End);
        Write(span);
        if (autoFree) C.free(buffer.pBinary);
    }
}
