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

using SDL3;
using static System.Net.Mime.MediaTypeNames;

namespace Records.Mod;

public class Setup
{
    public SetupDebug Debug { get; init; }
    public SetupWindow Window { get; init; }
    public SetupInput Input { get; init; }
    public SetupFont Font { get; init; }
    public SetupLog Log { get; init; }

    public Setup()
    {
        Debug ??= new()
        {
#if DEBUG
            DrawSceneData = true,
#else
            DrawSceneData = false,
#endif // DEBUG
        };

        Window ??= new()
        {
            Width = 1920,
            Height = 1080,
            ViewportWidth = 1920,
            ViewportHeight = 1080,
#if DEBUG
            FullScreen = true,
#else
            FullScreen = false,
#endif // DEBUG
            KeepAspectRatio = true,
            ScaleMode = SDL.ScaleMode.Nearest,
        };

        Input ??= new()
        {
            EnableKeyRepeat = true,
        };

        Font ??= new()
        {
            TTFFontName = "Font.ttf",
        };

        Log ??= new SetupLog
        {
#if DEBUG && TRUE
            LogLevel = SetupLog.Level.All,
#else
            LogLevel = SetupLog.Level.Warning,
#endif // DEBUG
        };
    }
}

public record class SetupDebug
{
    public bool DrawSceneData { get; set; }
}

public record class SetupWindow
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int ViewportWidth { get; set; }
    public int ViewportHeight { get; set; }
    public bool FullScreen { get; set; }
    public bool KeepAspectRatio { get; set; }
    public SDL.ScaleMode ScaleMode { get; set; }
}

public record class SetupInput
{
    public bool EnableKeyRepeat { get; set; }
}

public record class SetupFont
{
    public string TTFFontName { get; set; } = null!;
}

public record class SetupLog
{
    public enum Level
    {
        None    = 0,
        Error,
        Warning,
        Debug,
        Info,
        All,
    }

    public Level LogLevel { get; init; }
}

