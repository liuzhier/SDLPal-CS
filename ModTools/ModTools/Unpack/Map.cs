using Lib.Mod;
using Lib.Pal;
using ModTools;
using SimpleUtility;
using System;
using System.IO;

namespace ModTools.Unpack;

public static unsafe class Map
{
    /// <summary>
    /// 初始化地图资源
    /// </summary>
    public static void Init()
    {
        string              pathOut;
        BinaryReader        fileIn;
        nint                pPalette;
        BinaryWriter        fileOut;
        uint                offset;
        const int           paletteLength = 256 * 3;

        //
        // 创建输出目录 MapData
        //
        pathOut = Config.ModWorkPath.Game.MapData.PathName;
        COS.Dir(pathOut);

        //
        // 将地图编辑器复制到工作目录
        //
        S.FileCopy("Dependency", Config.ModWorkPath.Game.MapData.MapEditorName, pathOut);

        //
        // 将阉割后的地图调色板复制到工作目录
        //
        {
            //
            // 读取 PAT.MKF
            //
            fileIn = PalUtil.BinaryRead(Config.PalWorkPath.DataBase.Palette);

            //
            // 读取第一个子块
            //
            (pPalette, _) = PalUtil.ReadMkfChunk(fileIn, 0);

            //
            // 创建阉割后的 PAT.MKF
            //
            fileOut = PalUtil.BinaryWrite(Config.ModWorkPath.Game.MapData.Palette);

            //
            // 先将 MKF 文件头写入阉割后的 PAT.MKF
            //
            offset = sizeof(uint) * 2;
            fileOut.Write(offset);
            offset += paletteLength;
            fileOut.Write(offset);

            //
            // 最后将 MKF 文件头写入阉割后的 PAT.MKF
            //
            fileOut.Write(new ReadOnlySpan<byte>((void*)pPalette, paletteLength));

            //
            // 保存并关闭文件
            //
            PalUtil.CloseBinary(fileOut);
            PalUtil.CloseBinary(fileIn);

            //
            // 释放非托管内存
            //
            C.free(ref pPalette);
        }
    }

    /// <summary>
    /// 解档基础游戏数据。
    /// </summary>
    public static void Process()
    {
        string              pathMap, pathTile;
        int                 i, size, len;
        BinaryReader        fileInMap, fileInTile;
        BinaryWriter        fileOutMap, fileOutTile;
        nint                pBinary, pFree;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <MapData>");

        //
        // 创建输出目录 MapData
        //
        pathMap = Config.ModWorkPath.Game.MapData.Map;
        COS.Dir(pathMap);
        pathTile = Config.ModWorkPath.Game.MapData.Tile;
        COS.Dir(pathTile);

        //
        // 打开地图数据和地图瓦片文件
        //
        fileInMap = PalUtil.BinaryRead(Config.PalWorkPath.DataBase.Map);
        fileInTile = PalUtil.BinaryRead(Config.PalWorkPath.Spirit.Tile);

        //
        // 检查两文件子块数是否相同
        size = PalUtil.GetMkfChunkCount(fileInMap);
        S.Failed(
            "Map.Process",
            $"{pathTile} 与 {fileInTile} 子块数量不相同！",
            size == PalUtil.GetMkfChunkCount(fileInTile)
        );

        //
        // 开始处理地图数据
        //
        size--;
        for (i = 1; i < size; i++)
        {
            //
            // 创建地图数据文件和瓦片贴图文件
            //
            fileOutMap = PalUtil.BinaryWrite($@"{pathMap}\Map{i:D4}");
            fileOutTile = PalUtil.BinaryWrite($@"{pathTile}\Tile{i:D4}");

            //
            // 将数据写入文件
            //
            (pBinary, _) = PalUtil.ReadMkfChunk(fileInMap, i);
            if (pBinary != 0)
            {
                pFree = pBinary;
                (pBinary, len) = PalUtil.Unpack(pBinary, Config.IsDosGame);
                fileOutMap.Write(new ReadOnlySpan<byte>((void*)pBinary, len));
                fileOutMap.Write(new ReadOnlySpan<byte>((void*)pBinary, len));
                C.free(ref pFree);
                C.free(ref pBinary);
            }

            (pBinary, len) = PalUtil.ReadMkfChunk(fileInTile, i);
            fileOutTile.Write(len);
            fileOutTile.Write(new ReadOnlySpan<byte>((void*)pBinary, len));
            C.free(ref pBinary);

            //
            // 保存并关闭文件
            //
            PalUtil.CloseBinary(fileOutMap);
            PalUtil.CloseBinary(fileOutTile);
        }
    }
}
