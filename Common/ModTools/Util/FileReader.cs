using System;
using System.IO;

namespace ModTools.Util;

public unsafe class FileReader : IDisposable
{
    public FileReader(string path)
    {
        Stream = File.OpenRead(path);
        Reader = new(Stream);
    }

    ~FileReader() => Dispose();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Reader?.Dispose();
    }

    FileStream Stream { get; init; }
    BinaryReader Reader { get; init; }
    public string Name => Stream.Name;
    public long Length => Stream.Length;
    public void Seek(long offset, SeekOrigin origin) => Reader.BaseStream.Seek(offset, origin);
    public int Read(Span<byte> buffer) => Reader.Read(buffer);
    public (nint buffer, int length) ReadAll(int length = -1)
    {
        nint        buffer;

        if (length == -1) length = (int)Length;

        Seek(0, SeekOrigin.Begin);
        Read(new Span<byte>((void*)(buffer = C.malloc(length)), length));

        return (buffer, length);
    }
    public byte ReadByte() => Reader.ReadByte();
    public int ReadInt32() => Reader.ReadInt32();
    public Span<byte> ReadBytes(int count) => Reader.ReadBytes(count);
    public Span<byte> ReadAllBytes() => ReadBytes((int)Length);
}
