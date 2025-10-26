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

using Avalonia.Controls.Shapes;
using Lib.Mod;
using Lib.Pal;
using SimpleUtility;
using System;
using System.IO;

namespace ModTools.Compile;

public static unsafe class Map
{
    /// <summary>
    /// 编译 MAP 和 TILE 数据
    /// </summary>
    public static void Process()
    {
        const int       beginId = 1;

        string          pathMap, pathTile;
        int             endId, i, j;
        FileReader      fileMap, fileTile;
        MkfWriter       mkfMap, mkfTile;

        //
        // 输出处理进度
        //
        Util.Log("Compiling \"MAP.MKF\" and \"GOP.MKF\".");

        //
        // 获取 MAP TILE 目录
        //
        pathMap = Config.ModWorkPath.Game.MapData.Map;
        pathTile = Config.ModWorkPath.Game.MapData.Tile;

        //
        // 创建地图数据（MAP.MKF）和地图瓦片（GOP.MKF）文件
        //
        mkfMap = new(Config.PalWorkPath.DataBase.Map);
        mkfTile = new(Config.PalWorkPath.Spirit.Tile);
        fileTile = null!;

        //
        // 检查两文件子块数是否相同
        //
        endId = ModUtil.GetFileSequenceCount(pathMap, beginId, digitNumber: 4 ,filePrefix: "Map", fileSuffix: "");
        S.Failed(
            "Map.Process",
            "Map 与 Tile 数量不相同！",
            endId == ModUtil.GetFileSequenceCount(pathTile, beginId, digitNumber: 4, filePrefix: "Tile", fileSuffix: "")
        );

        //
        // 创建文件头
        //
        mkfMap.SetLength(sizeof(int) * (beginId + endId + 1));
        mkfTile.SetLength(sizeof(int) * (beginId + endId + 1));

        //
        // 开始处理地图数据
        //
        for (i = 0, j = 0; i <= endId; i++, j++)
        {
            //
            // 将 Chunk 地址写入 MKF 文件头
            //
            mkfMap.WriteHeader(i, (int)mkfMap.Length);
            mkfTile.WriteHeader(i, (int)mkfTile.Length);

            if (i < beginId)
                //
                // 跳过不存在的文件
                //
                continue;

            //
            // 读取地图数据文件和瓦片贴图文件
            //
            fileMap = new($@"{pathMap}\Map{i:D4}");
            fileTile = new($@"{pathTile}\Tile{i:D4}");
            if (fileMap.Length == 0 || fileTile.Length == 0)
                //
                // 跳过空文件
                //
                goto loop_continue;

            //
            // 将 Chunk 写入 MKF 尾部（MAP 数据只有前半部是实际的，后半部分是模板）
            //
            mkfMap.Append(PalUtil.Encode(fileMap.ReadAll(0x10000), Config.IsDosGame));
            mkfTile.Append(fileTile.ReadAll(), beginPos: sizeof(int));

        loop_continue:
            //
            // 关闭 MOD 数据文件
            //
            fileMap?.Dispose();
            fileTile?.Dispose();
        }

        //
        // MKF 文件头最后的数据是文件长度
        //
        mkfMap.WriteHeader(i, (int)mkfMap.Length);
        mkfTile.WriteHeader(i, (int)mkfTile.Length);

        //
        // 保存并关闭文件
        //
        mkfMap?.Dispose();
        mkfTile?.Dispose();
    }
}
