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
using System.Runtime.InteropServices;

namespace Lib.Pal;

public static unsafe partial class PalUtil
{
    const string LibraryName = "PalLibrary.dll";

    public enum PalErrno : int
    {
        Ok              = 0,
        OutOfMemory     = 12,
        InvalidData     = 1024,
        EmptyPointer    = 1025,
        NotEnoughSpace  = 1026,
        InvalidFormat   = 1027,
    }

    [LibraryImport(LibraryName, EntryPoint = "decodeyj1")]
    public static partial PalErrno Decodeyj1(nint Source, out nint Destination, ref uint Length);

    [LibraryImport(LibraryName, EntryPoint = "encodeyj1")]
    public static partial PalErrno Encodeyj1(void* Source, uint SourceLength, void** Destination, uint* Length);

    [LibraryImport(LibraryName, EntryPoint = "decodeyj2")]
    public static partial PalErrno Decodeyj2(nint Source, out nint Destination, ref uint Length);

    [LibraryImport(LibraryName, EntryPoint = "encodeyj2")]
    public static partial PalErrno Encodeyj2(void* Source, uint SourceLength, void** Destination, uint* Length, int bCompatible);

    [LibraryImport(LibraryName, EntryPoint = "decoderng")]
    public static partial PalErrno Decoderng(nint Source, nint PrevFrame);

    [LibraryImport(LibraryName, EntryPoint = "encoderng")]
    public static partial PalErrno Encoderng(nint PrevFrame, nint CurFrame, out nint Destination, ref uint Length);

    [LibraryImport(LibraryName, EntryPoint = "decoderle")]
    public static partial PalErrno Decoderle(nint Rle, nint Destination, int Stride, int Width, int Height, int x, int y);

    [LibraryImport(LibraryName, EntryPoint = "encoderle")]
    public static partial PalErrno Encoderle(nint Source, nint Base, int Stride, int Width, int Height, out nint Destination, ref uint Length);

    [LibraryImport(LibraryName, EntryPoint = "encoderlet")]
    public static partial PalErrno Encoderlet(nint Source, byte TransparentColor, int Stride, int Width, int Height, out nint Destination, ref uint Length);
}
