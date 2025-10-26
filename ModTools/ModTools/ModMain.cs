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

using ModTools.Compile;
using ModTools.Unpack;
using System.Threading.Tasks;

namespace ModTools;

public static class ModMain
{
    /// <summary>
    /// 解包游戏资源
    /// </summary>
    /// <param name="palPath">游戏资源目录</param>
    /// <param name="modPath">MOD 输出目录</param>
    public static async Task GoUnpack(string palPath, string modPath) =>
        await UnpackMain.Process(palPath, modPath);

    /// <summary>
    /// 编译 MOD 资源
    /// </summary>
    /// <param name="modPath">MOD 资源目录</param>
    /// <param name="compiledPath">资源编译输出目录</param>
    /// <param name="isDosGame">欲编译的游戏版本</param>
    public static async Task GoCompile(string modPath, string compiledPath, bool? isDosGame) =>
        await CompileMain.Process(modPath, compiledPath, isDosGame);
}
