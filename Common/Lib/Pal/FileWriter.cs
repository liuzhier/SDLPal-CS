using System;
using System.IO;

namespace Lib.Pal;

public class FileWriter(string path) : IDisposable
{
    ~FileWriter() => Dispose();
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _file?.Dispose();
    }

    BinaryWriter _file { get; init; } = new(File.OpenWrite(path));
    public long Length => _file.BaseStream.Length;
    public void Seek(int offset, SeekOrigin origin) => _file.Seek(offset, origin);
    public void SetLength(long value) => _file.BaseStream.SetLength(value);
    public void Write(ReadOnlySpan<byte> buffer) => _file.Write(buffer);
    public void Write(int value) => _file.Write(value);
    public void Write(uint value) => _file.Write(value);
}
