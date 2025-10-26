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
using System.Collections.Generic;
using EntityBeginId = Records.Pal.Entity.BeginId;

namespace ModTools.Unpack;

public static unsafe class System
{
    /// <summary>
    /// 处理系统实体。
    /// </summary>
    public static void Process()
    {
        List<string>        names;
        EntityBeginId       i;

        //
        // 处理实体对象名称
        //
        names = [];
        names.AddRange(Message.EntityNames[..(int)EntityBeginId.Hero]);
        for (i = EntityBeginId.Hero; i < EntityBeginId.System2; i++)
            names.Add($"0x{((int)i):x4}");
        names.AddRange(Message.EntityNames[(int)EntityBeginId.System2..(int)EntityBeginId.Item]);

        //
        // 导出 JSON 文件到输出目录
        //
        S.JsonSave(names.ToArray(), $"{Config.ModWorkPath.Game.Data.Entity.System}.json");
    }
}
