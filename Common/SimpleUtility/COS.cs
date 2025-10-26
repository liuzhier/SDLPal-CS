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

using System;
using System.IO;

namespace SimpleUtility;

/// <summary>
/// COS 类名称是 Create objects safely 的缩写，
/// 其旨在为程序“安全地创建对象”。
/// </summary>
public static class COS
{
    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="isDelExists">当该值为 true 时，若文件存在，则自动删除后再创建它</param>
    public static void Dir(string path, bool isDelExists = true)
    {
        try
        {
            if (isDelExists)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                Directory.CreateDirectory(path);
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }
        catch (Exception e)
        {
            S.Failed(
               "COS.Dir",
               e.Message
            );
        }
    }
}
