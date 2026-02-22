using System;
using System.IO;
using System.Reflection.Metadata;

namespace ModTools.Util;

public class LogWriter(string path) : IDisposable
{
    ~LogWriter() => Dispose();
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Writer?.Dispose();
    }

    StreamWriter Writer { get; init; } = new(File.OpenWrite(path));
    public long Length => Writer.BaseStream.Length;

    public void Write(string text)
    {
        Console.Write(text);
        Writer.Write(text);
        Flush();
    }

    public void WriteLine(string text)
    {
        Console.WriteLine(text);
        Writer.WriteLine(text);
        Flush();
    }

    void Flush() => Writer?.Flush();
}
