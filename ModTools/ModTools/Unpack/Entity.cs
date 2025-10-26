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
using SimpleUtility;
using EntityDos = Records.Pal.Entity.Dos;
using EntityWin = Records.Pal.Entity.Win;

namespace ModTools.Unpack;

/// <summary>
/// 解档基础游戏数据。
/// </summary>
public static unsafe class Entity
{
    public static nint BinData { get; set; }
    public static int CoreDataCount { get; set; }

    /// <summary>
    /// 解档游戏实体对象。
    /// </summary>
    public static void Process()
    {
        string      path;
        int         binDataLength;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Entity>");

        //
        // 创建输出目录
        //
        path = Config.ModWorkPath.Game.Data.Entity.PathName;
        COS.Dir(path);

        //
        // 将数据读入非托管内存
        //
        (BinData, binDataLength) = Config.MkfCore.ReadChunk(2);
        if (Config.IsDosGame)
        {
            CoreDataCount = binDataLength / sizeof(EntityDos);
            Config.CoreDos = (EntityDos*)BinData;
        }
        else
        {
            CoreDataCount = binDataLength / sizeof(EntityWin);
            Config.CoreWin = (EntityWin*)BinData;
        }

        //
        // 处理实体对象数据
        //
        System.Process();
        Hero.Process();
        Item.Process();
        Magic.Process();
        Enemy.Process();
        Poison.Process();

        //
        // 释放非托管内存
        //
        C.free(BinData);
    }
}
