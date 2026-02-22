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

using SDLPal;
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
        UiUtil.Log("Unpack the game data. <Entity>");

        //
        // 创建输出目录
        //
        path = $"{PalConfig.ModWorkPath.Assets.Data.Entity}";
        COS.Dir(path);

        //
        // 将数据读入非托管内存
        //
        (BinData, binDataLength) = PalConfig.MkfCore.ReadChunk(2);
        if (PalConfig.IsDosGame)
        {
            CoreDataCount = binDataLength / sizeof(EntityDos);
            PalConfig.CoreDos = (EntityDos*)BinData;
        }
        else
        {
            CoreDataCount = binDataLength / sizeof(EntityWin);
            PalConfig.CoreWin = (EntityWin*)BinData;
        }

        //
        // 处理实体对象数据
        //
        System.Process();
        Poison.Process();
        Item.Process();
        Magic.Process();
        Enemy.Process();
        Hero.Process();

        //
        // 释放非托管内存
        //
        C.free(BinData);
    }
}
