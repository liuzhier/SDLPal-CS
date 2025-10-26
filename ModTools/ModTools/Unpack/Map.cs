#region License
/*
 * Copyright (c) 2025, liuzhier <lichunxiao_lcx@qq.com>.
 * 
 * This file is part of SDLPAL-CS.
 * 
 * SDLPAL-CS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 3
 * as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */
#endregion License

using Lib.Mod;
using Lib.Pal;
using SimpleUtility;
using System;

namespace ModTools.Unpack;

public static unsafe class Map
{
    /// <summary>
    /// 初始化地图资源
    /// </summary>
    public static void Init()
    {
        string          pathOut;
        MkfReader       mkf;
        nint            pPalette;
        FileWriter      fileOut;
        uint            offset;
        const int       paletteLength = 256 * 3;

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
            mkf = new(Config.PalWorkPath.DataBase.Palette);

            //
            // 读取第一个子块
            //
            (pPalette, _) = mkf.ReadChunk(0);

            //
            // 创建阉割后的 PAT.MKF
            //
            fileOut = new(Config.ModWorkPath.Game.MapData.Palette);

            //
            // 先将 MKF 文件头写入阉割后的 PAT.MKF
            //
            fileOut.Write(offset = sizeof(uint) * 2);
            fileOut.Write(offset += paletteLength);

            //
            // 最后将 MKF 文件头写入阉割后的 PAT.MKF
            //
            fileOut.Write(new ReadOnlySpan<byte>((void*)pPalette, paletteLength));

            //
            // 保存并关闭文件
            //
            fileOut?.Dispose();
            mkf?.Dispose();

            //
            // 释放非托管内存
            //
            C.free(pPalette);
        }
    }

    /// <summary>
    /// 解档基础游戏数据。
    /// </summary>
    public static void Process()
    {
        string          pathMap, pathTile;
        int             i, size, len;
        MkfReader       mkfMap, mkfTile;
        FileWriter      fileOutMap, fileOutTile;
        nint            pBinary, pFree;

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
        mkfMap = new(Config.PalWorkPath.DataBase.Map);
        mkfTile = new(Config.PalWorkPath.Spirit.Tile);

        //
        // 检查两文件子块数是否相同
        size = mkfMap.GetChunkCount();
        S.Failed(
            "Map.Process",
            $"{mkfMap.Name} 与 {mkfTile.Name} 子块数量不相同！",
            size == mkfTile.GetChunkCount()
        );

        //
        // 开始处理地图数据
        //
        for (i = 1; i < size; i++)
        {
            //
            // 创建地图数据文件和瓦片贴图文件
            //
            fileOutMap = new($@"{pathMap}\Map{i:D4}");
            fileOutTile = new($@"{pathTile}\Tile{i:D4}");

            //
            // 将数据写入文件
            //
            (pBinary, _) = mkfMap.ReadChunk(i);
            if (pBinary != 0)
            {
                pFree = pBinary;
                (pBinary, len) = PalUtil.Unpack(pBinary, Config.IsDosGame);
                fileOutMap.Write(new ReadOnlySpan<byte>((void*)pBinary, len));
                fileOutMap.Write(new ReadOnlySpan<byte>((void*)pBinary, len));
                C.free(ref pFree);
                C.free(ref pBinary);
            }

            (pBinary, len) = mkfTile.ReadChunk(i);
            fileOutTile.Write(len);
            fileOutTile.Write(new ReadOnlySpan<byte>((void*)pBinary, len));
            C.free(ref pBinary);

            //
            // 保存并关闭文件
            //
            fileOutMap?.Dispose();
            fileOutTile?.Dispose();
        }
    }
}
