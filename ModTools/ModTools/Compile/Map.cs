using Common.Lib.Mod;
using Lib.Mod;
using Lib.Pal;
using ModTools;
using SimpleUtility;
using System;
using System.IO;

namespace ModTools.Compile;

public static unsafe class Map
{
    /// <summary>
    /// 解档基础游戏数据。
    /// </summary>
    public static void Process()
    {
        string              pathMap, pathTile;
        int                 beginId, endId, headerSize, i, j, chunkSize, len;
        BinaryReader        fileInMap, fileInTile;
        BinaryWriter        fileOutMap, fileOutTile;
        nint                pBinary, pFree;

        //
        // 输出处理进度
        //
        Util.Log("Compiling \"MAP.MKF\".");

        //
        // 获取 MAP TILE 目录
        //
        pathMap = Config.ModWorkPath.Game.MapData.Map;
        pathTile = Config.ModWorkPath.Game.MapData.Tile;

        //
        // 创建地图数据（MAP.MKF）和地图瓦片（GOP.MKF）文件
        //
        fileOutMap = PalUtil.BinaryWrite(Config.PalWorkPath.DataBase.Map);
        fileOutTile = PalUtil.BinaryWrite(Config.PalWorkPath.Spirit.Tile);

        //
        // 检查两文件子块数是否相同
        beginId = 1;
        endId = ModUtil.GetFileSequenceCount(pathMap, beginId, filePrefix: "Map", fileSuffix: "");
        S.Failed(
            "Map.Process",
            "Map 与 Tile 数量不相同！",
            endId == ModUtil.GetFileSequenceCount(pathMap, beginId, filePrefix: "Tile", fileSuffix: "")
        );
        headerSize = endId * sizeof(int);
        ModUtil.WriteMkfHeader(fileOutTile, 0, headerSize);

        //
        // 开始处理地图数据
        //
        endId++;
        for (i = beginId, j = 0; i <= endId; i++, j++)
        {
            //
            // 创建地图数据文件和瓦片贴图文件
            //
            fileInMap = PalUtil.BinaryRead($@"{pathMap}\Map{i:D4}");
            fileInTile = PalUtil.BinaryRead($@"{pathTile}\Tile{i:D4}");

            //
            // 将 Chunk 地址写入 GOP.MKF 文件头
            //
            ModUtil.WriteMkfHeader(fileOutTile, j, (int)fileOutTile.BaseStream.Length);

            //
            // 将 Chunk 写入 GOP.MKF 尾部
            //
            (pBinary, len) = ModUtil.ReadBinary($@"{pathTile}\Tile{i:D4}");
            ModUtil.AppendBinary(fileOutTile, new Span<byte>((void*)pBinary, len)[sizeof(int)..]);

            //
            // 保存并关闭文件
            //
            PalUtil.CloseBinary(fileInMap);
            PalUtil.CloseBinary(fileInTile);
        }

        //
        // 保存并关闭文件
        //
        PalUtil.CloseBinary(fileOutMap);
        PalUtil.CloseBinary(fileOutTile);
    }
}
