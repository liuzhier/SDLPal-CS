using SimpleUtility;
using System;
using System.IO;
using static Lib.Pal.PalUtil;

namespace Lib.Pal;

public unsafe class MkfReader(string path) : IDisposable
{
    ~MkfReader() => Dispose();
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _file?.Dispose();
    }

    public enum UnitSize
    {
        MKF         = sizeof(int),
        YJ_1        = sizeof(int),
        WinPack     = sizeof(int),
        SMKF        = sizeof(short),
        DialogIndex = sizeof(int),
    }

    FileReader _file { get; init; } = new(path);

    public int GetChunkCount()
    {
        //
        // 将文件光标定位到 Sub32 块开头
        //
        SeekHeader(0);

        //
        // 读取文件头部第一个整数，计算块数量
        //
        return ReadInt32() / (int)UnitSize.MKF - 1;
    }

    public string Name => _file.Name;
    public void Seek(long offset, SeekOrigin flag) => _file.Seek(offset, flag);
    public int Read(Span<byte> buffer) => _file.Read(buffer);
    public int ReadInt32() => _file.ReadInt32();

    /// <summary>
    /// 将文件光标定位到 MKF 文件中指定块的索引位置。
    /// </summary>
    /// <param name="chunkId">块编号</param>
    void SeekHeader(int chunkId) => _file.Seek(chunkId * (int)UnitSize.MKF, SeekOrigin.Begin);

    /// <summary>
    /// 将文件光标定位到 MKF 文件中的指定块。
    /// </summary>
    /// <param name="chunkId">块编号</param>
    public void SeekChunk(int chunkId)
    {
        //
        // 将文件光标定位到文件头指定块索引处
        //
        SeekHeader(chunkId);

        //
        // 将光标定位到块
        //
        Seek(ReadInt32(), SeekOrigin.Begin);
    }

    /// <summary>
    /// 检查块编号是否超出最大块，超出则抛出异常。
    /// </summary>
    /// <param name="chunkId">块编号</param>
    void CheckChunkValidity(int chunkId)
    {
        if (chunkId >= GetChunkCount())
            throw new Exception($"MkfReader[{Name}]: 块索引越界（{chunkId + 1}/{GetChunkCount()}）。");
    }

    /// <summary>
    /// 获取 MKF 中指定块的大小。
    /// </summary>
    /// <param name="chunkId">块编号</param>
    /// <returns>块大小</returns>
    public int GetChunkSize(int chunkId)
    {
        //
        // 检查块
        //
        CheckChunkValidity(chunkId);

        //
        // 将文件光标定位到文件头部中的指定块索引
        //
        SeekHeader(chunkId);

        //
        // 计算块大小
        //
        return -(ReadInt32() - ReadInt32());
    }

    /// <summary>
    /// 获取 MKF 中的指定子块
    /// </summary>
    /// <param name="chunkId">块编号</param>
    /// <returns>子块非托管内存地址</returns>
    public (nint, int) ReadChunk(int chunkId)
    {
        int             chunkLen;
        nint            pDest;

        pDest = 0;

        chunkLen = GetChunkSize(chunkId);

        if (chunkLen > 0)
        {
            SeekChunk(chunkId);
            Read(new Span<byte>((void*)(pDest = C.malloc(chunkLen)), chunkLen));
        }

        return (pDest, chunkLen);
    }

    /// <summary>
    /// 获取 Sub32 块的帧数量。
    /// </summary>
    /// <param name="chunkId">块编号</param>
    /// <returns>帧数量</returns>
    public int GetRngFrameCount(int chunkId)
    {
        //
        // 将文件光标定位到 Sub32 块开头
        //
        SeekChunk(chunkId);

        //
        // 读取文件头部第一个整数，计算块数量
        //
        return ReadInt32() / (int)UnitSize.MKF - 1;
    }

    /// <summary>
    /// 将文件光标定位到动画的头部指定帧索引处
    /// </summary>
    /// <param name="rngId">动画编号</param>
    /// <param name="frameId">动画帧编号</param>
    public void SeekRngHeader(int rngId, int frameId)
    {
        //
        // 将光标定位到 MKF 子块
        //
        SeekChunk(rngId);

        //
        // 将光标定位到动画的头部指定帧索引处
        //
        Seek(frameId * (int)UnitSize.MKF, SeekOrigin.Current);
    }

    /// <summary>
    /// 将文件光标定位动画的指定帧
    /// </summary>
    /// <param name="rngId">动画编号</param>
    /// <param name="frameId">动画帧编号</param>
    public void SeekRngFrame(int rngId, int frameId)
    {
        int     offset;

        //
        // 将文件光标定位到动画头部指定帧索引处
        //
        SeekRngHeader(rngId, frameId);

        //
        // 将光标定位到 Sub32 块
        //
        offset = ReadInt32();
        SeekChunk(rngId);
        Seek(offset, SeekOrigin.Current);
    }

    /// <summary>
    /// 获取动画大小
    /// </summary>
    /// <param name="rngId">动画编号</param>
    /// <param name="frameId">动画帧编号</param>
    /// <returns>动画大小</returns>
    public int GetRngSize(int rngId, int frameId)
    {
        //
        // 检查块
        //
        CheckChunkValidity(rngId);

        //
        // 将文件光标等位到文件头部中的指定块索引
        //
        SeekRngHeader(rngId, frameId);

        //
        // 计算块大小
        //
        return -(ReadInt32() - ReadInt32());
    }

    /// <summary>
    /// 读取指定动画帧
    /// </summary>
    /// <param name="rngId">动画编号</param>
    /// <param name="frameId">动画帧编号</param>
    /// <returns>动画内存地址</returns>
    public (nint, int) ReadRngFrame(int rngId, int frameId)
    {
        int             chunkLen;
        nint            pDest;

        pDest = 0;

        //
        // Get the length of the MKF chunk.
        //
        chunkLen = GetChunkSize(rngId);

        if (chunkLen != 0)
        {
            //
            // Get the length of the Sub32 chunk.
            //
            chunkLen = GetRngSize(rngId, frameId);

            SeekRngFrame(rngId, frameId);

            Read(new Span<byte>((void*)(pDest = C.malloc(chunkLen)), chunkLen));
        }

        return (pDest, chunkLen);
    }
}
