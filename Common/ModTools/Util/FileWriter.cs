using System;
using System.IO;

namespace ModTools.Util;

public class FileWriter(string path) : IDisposable
{
    ~FileWriter() => Dispose();
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Writer?.Dispose();
    }

    BinaryWriter Writer { get; init; } = new(File.OpenWrite(path));
    public long Length => Writer.BaseStream.Length;
    public void Seek(int offset, SeekOrigin origin) => Writer.Seek(offset, origin);
    public void SetLength(long value) => Writer.BaseStream.SetLength(value);
    public void Write(ReadOnlySpan<byte> buffer) => Writer.Write(buffer);
    public void Write(int value) => Writer.Write(value);
    public void Write(uint value) => Writer.Write(value);
}
