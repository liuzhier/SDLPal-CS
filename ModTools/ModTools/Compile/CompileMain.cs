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
using System.Threading.Tasks;

namespace ModTools.Compile;

public static class CompileMain
{
    /// <summary>
    /// 编译 MOD 资源
    /// </summary>
    /// <param name="modPath">MOD 资源目录</param>
    /// <param name="compiledPath">资源编译输出目录</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版本</param>
    public static async Task Process(string modPath, string compiledPath, bool? isDosGame)
    {
        //
        // 初始化全局配置
        //
        Util.Log("Initialize the global data.");
        Config.Init(compiledPath, modPath, isDosGame);

        //
        // 创建游戏目录
        //
        COS.Dir(Config.PalWorkPath.PathName);

        //
        // 开始编译
        //
        //Voice.Process();
        //Map.Process();
        //Palette.Process();


        //
        // 释放全局数据
        //
        Config.Free();
        Message.Free();


        //
        // 编译完毕
        //
        Util.Log("The game resources have been compiled successfully!");
    }
}
