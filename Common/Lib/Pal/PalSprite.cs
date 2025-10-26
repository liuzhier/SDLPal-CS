using SimpleUtility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Lib.Pal;

public unsafe class PalSprite((nint buffer, int size) data) : IDisposable
{
    ~PalSprite() => Dispose();
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        C.free(Buffer);
    }

    public  nint Buffer { get; init; } = (nint)(void*)data.buffer;
    int _size { get; init; } = data.size;
    byte* _pInt8 => (byte*)Buffer;
    ushort* _pInt16 => (ushort*)Buffer;
    public int PackCount => *_pInt16;

    /// <summary>
    /// 检查帧编号是否超出最大帧，超出则抛出异常。
    /// </summary>
    /// <param name="packId">帧编号</param>
    void CheckFrameValidity(int packId)
    {
        if (packId >= PackCount)
            throw new Exception($"PalSprite: 帧索引越界（{packId + 1}/{PackCount}）。");
    }

    public int GetFrameSize(int packId)
    {
        int      iOffset, iNextOffset, size;

        iOffset = 0;
        iNextOffset = 0;

        //
        // 检查 Chunk 索引是否越界
        //
        if (packId >= PackCount)
            return -1;

        //
        // 获取指定包和下一个包的偏移量
        //
        iOffset = _pInt16[packId] << 1;
        iNextOffset = _pInt16[packId + 1] << 1;

        if (iOffset == 0)
            return -1;

        if (iOffset == 0x18444)
            iOffset = (ushort)iOffset;

        if (iNextOffset == 0x18444)
            iNextOffset = (ushort)iNextOffset;

        if (iNextOffset == 0 || packId == PackCount
           || iNextOffset < iOffset || iNextOffset > _size)
        {
            iNextOffset = _size;
        }

        size = iNextOffset - iOffset;

        if (size > _size)
            return -1;

        //
        // 返回块长度
        //
        return size;
    }

    public nint GetFrame(int frameId)
    {
        int offset;

        //
        // 血口虫 bug（Hack）
        //
        //   imagecount = (lpSprite[0] | (lpSprite[1] << 8)) - 1;

        if (frameId < 0 || frameId >= PackCount)
            //
            // 帧不存在
            //
            return 0;

        //
        // 获取帧偏移
        //
        if ((offset = (_pInt16[frameId] << 1)) == 0x18444)
            offset = (ushort)offset;

        return Buffer + offset;
    }

    public int GetFrameWidth(int frameId) => ((ushort*)GetFrame(frameId))[0];
    public int GetFrameHeight(int frameId) => ((ushort*)GetFrame(frameId))[1];
}
