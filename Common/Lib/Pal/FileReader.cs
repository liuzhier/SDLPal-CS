using SimpleUtility;
using System;
using System.IO;

namespace Lib.Pal;

public unsafe class FileReader : IDisposable
{
    public FileReader(string path)
    {
        _stream = File.OpenRead(path);
        _file = new(_stream);
    }

    ~FileReader() => Dispose();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _file?.Dispose();
    }

    FileStream _stream { get; init; }
    BinaryReader _file { get; init; }
    public string Name => _stream.Name;
    public long Length => _stream.Length;
    public void Seek(long offset, SeekOrigin origin) => _file.BaseStream.Seek(offset, origin);
    public int Read(Span<byte> buffer) => _file.Read(buffer);
    public (nint buffer, int length) ReadAll(int length = -1)
    {
        nint        buffer;

        if (length == -1) length = (int)Length;

        Seek(0, SeekOrigin.Begin);
        Read(new Span<byte>((void*)(buffer = C.malloc((int)length)), (int)length));

        return (buffer, (int)length);
    }
    public int ReadInt32() => _file.ReadInt32();
    public Span<byte> ReadBytes(int count) => _file.ReadBytes(count);
}
