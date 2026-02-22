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

using Records.Mod;
using SDL3;
using SDLPal;
using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleUtility;

/// <summary>
/// COS 类名称是 Create objects safely 的缩写，
/// 其旨在为程序“安全地创建对象”。
/// </summary>
public unsafe static class COS
{
    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="isDelExists">当该值为 true 时，若文件存在，则自动删除后再创建它</param>
    public static void Dir(string path, bool isDelExists = false)
    {
        try
        {
            if (isDelExists)
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);

                Directory.CreateDirectory(path);
            }
            else if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
        catch (Exception e)
        {
            S.Failed(
               "COS.Dir",
               e.Message
            );
        }
    }

    public static nint Rect(SDL.Rect rect)
    {
        var lpRect = (SDL.Rect*)C.malloc(sizeof(SDL.Rect));

        lpRect->X = rect.X;
        lpRect->Y = rect.Y;
        lpRect->W = rect.W;
        lpRect->H = rect.H;

        return (nint)lpRect;
    }

    public static nint FRect(SDL.FRect frect)
    {
        var lpFRect = (SDL.FRect*)C.malloc(sizeof(SDL.FRect));

        lpFRect->X = frect.X;
        lpFRect->Y = frect.Y;
        lpFRect->W = frect.W;
        lpFRect->H = frect.H;

        return (nint)lpFRect;
    }


    public static nint Surface(int w, int h, SDL.PixelFormat format)
    {
        nint        surface;

        S.SDLFailed(
           "COS.Surface",
           surface = SDL.CreateSurface(w, h, format)
        );

        return surface;
    }

    public static nint Surface(string path)
    {
        nint        surface;

        S.SDLFailed(
           "COS.Surface",
           surface = Image.Load(path)
        );

        return surface;
    }

    static nint Texture(int w, int h, SDL.PixelFormat format, SDL.TextureAccess access)
    {
        nint        texture;

        S.SDLFailed(
           "COS.Texture",
           texture = SDL.CreateTexture(PalVideo.Renderer, format, access, w, h)
        );

        return texture;
    }

    public static nint Texture(int w, int h, SDL.PixelFormat format, int lockedMod = 0) =>
        Texture(w, h, format, (SDL.TextureAccess)lockedMod);

    static nint TextureL(nint surface, bool free = true)
    {
        nint        texture;

        S.SDLFailed(
           "COS.Texture",
           texture = SDL.CreateTextureFromSurface(PalVideo.Renderer, surface)
        );

        if (free) FOS.Surface(ref surface);

        return texture;
    }

    //public static nint Texture(nint surface, SDL.PixelFormat? format = null, int lockedMod = 0, bool free = true)
    public static nint Texture(nint surface, int lockedMod = 0, bool free = true)
    {
        nint                texture;

        texture = TextureL(surface, free);

        if (lockedMod != 0)
        {
            var pSurface = (SDL.Surface*)surface;

            //texture = Texture(pSurface->Width, pSurface->Height, (format ??= pSurface->Format), lockedMod);
            var temp = Texture(pSurface->Width, pSurface->Height, pSurface->Format, lockedMod);
            PalScreen.Copy(texture, temp);

            if (free) FOS.Surface(ref surface);
            if (free) FOS.Texture(ref texture);

            texture = temp;
        }

        return texture;
    }

    //public static nint Texture(nint surface, int lockedMod = 0, bool free = true) =>
    //    Texture(surface, ((SDL.Surface*)surface)->Format, lockedMod, free);

    public static nint Texture(string path) => TextureL(Surface(path));

    public static (nint, int) Palette(nint paletteBuffer, int len, int colorKey = 0xFF)
    {
        int             i;
        Rgb24*          pRgb24;
        SDL.Color[]     colors;
        nint            palette;

        pRgb24 = (Rgb24*)paletteBuffer;
        len = int.Min(len / sizeof(Rgb24), 256);

        colors = new SDL.Color[len];

        for (i = 0; i < len; i++)
        {
            colors[i].R = (byte)(pRgb24[i].R << 2);
            colors[i].G = (byte)(pRgb24[i].G << 2);
            colors[i].B = (byte)(pRgb24[i].B << 2);
            colors[i].A = 0xFF;
        }

        //
        // 设置透明色映射
        //
        colors[colorKey].A = 0;

        //
        // 创建调色板
        //
        palette = SDL.CreatePalette(colors.Length);
        SDL.SetPaletteColors(palette, colors, 0, colors.Length);

        return (palette, len);
    }

    public static SDL.Color Color(uint val) => new()
    {
        R = (byte)(val >> (3 * 8)),
        G = (byte)(val >> (2 * 8)),
        B = (byte)(val >> (1 * 8)),
        A = (byte)val,
    };

    /// <summary>
    /// 获取文本像素宽度（去掉特殊字符后的宽度）
    /// </summary>
    /// <param name="text">文本</param>
    /// <param name="fontSize">字体大小</param>
    /// <param name="color">字体颜色</param>
    /// <returns>文本像素宽度</returns>
    public static nint TextSurface(string text, int fontSize, SDL.Color color) =>
        TTF.RenderTextBlended(PalText.FontDict[fontSize], text, 0, color);
}
