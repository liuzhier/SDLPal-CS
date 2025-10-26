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

using SimpleUtility;

namespace Lib.Mod;

public static unsafe class ModUtil
{
    /// <summary>
    /// 获取文件序列数量
    /// </summary>
    /// <param name="fileSequencePath">文件序列路径，序列格式 “:D5”</param>
    /// <param name="beginId">文件序列起始编号</param>
    /// <param name="digitNumber">文件序列对齐位数</param>
    /// <param name="filePrefix">文件序列前缀</param>
    /// <param name="fileSuffix">文件序列后缀</param>
    /// <returns>文件序列数量</returns>
    public static int GetFileSequenceCount(string fileSequencePath, int beginId, int digitNumber = 5, string filePrefix = "", string fileSuffix = ".json")
    {
        int     i;

        for (i = beginId; ; i++)
            if (!S.FileExist($@"{fileSequencePath}\{filePrefix}{string.Format($"{{0:D{digitNumber}}}", i)}{fileSuffix}", isAssert: false))
                break;

        return i - beginId;
    }

    /// <summary>
    /// 获取文件夹序列数量
    /// </summary>
    /// <param name="directorySequencePath">文件夹序列路径，序列格式 “:D5”</param>
    /// <param name="beginId">文件夹序列起始编号</param>
    /// <param name="digitNumber">文件夹序列对齐位数</param>
    /// <returns>文件夹序列数量</returns>
    public static int GetDirectorySequenceCount(string directorySequencePath, int beginId, int digitNumber = 5)
    {
        int     i;

        for (i = beginId; ; i++)
            if (!S.DirExist($@"{directorySequencePath}\{string.Format($"{{0:D{digitNumber}}}", i)}", isAssert: false))
                break;

        return i - beginId;
    }
}
