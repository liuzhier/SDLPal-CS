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

using Avalonia.Controls;
using AvaloniaControl;
using Lib.Ala;
using System;

namespace ModTools;

public static partial class Util
{
    public static TextBox LogBox_TextBox { get; set; } = null!;

    /// <summary>
    /// 在控制台输出日志信息
    /// </summary>
    /// <param name="message">欲输出的信息</param>
    public static void Log(string message) =>
        AlaUtil.UpdateUi(() =>
        {
            LogBox_TextBox.Text += $"{Environment.NewLine}{message}";
            LogBox_TextBox.ScrollToLine(LogBox_TextBox.GetLineCount() - 1);
        });
}
