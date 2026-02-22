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

using Records.Pal;
using System.Runtime.InteropServices;
using System.Text;

namespace Records.Mod.RGame;

public class Script
{
    public ushort Command { get; set; }
    public Arg[] Args { get; set; }

    public Script(int argCount)
    {
        Args = new Arg[argCount];

        for (var i = 0; i < argCount; i++) Args[i] = new();
    }

    public string GetArgsString()
    {
        var sb = new StringBuilder();

        for (var i = 0; i < Args.Length; i++) sb.Append($"{Args[i].Int} ");

        return sb.ToString();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"0x{Command:X4}");

        var i = 0;
        for (; i < Args.Length; i++) sb.Append($" {Args[i].Int:X8}");
        for (; i < 5; i++) sb.Append($" {0:X8}");

        return sb.ToString();
    }

    public class Arg
    {
        public ArgRaw Raw = new();
        public string String { get; set; } = null!;
        public byte Byte => Raw.Byte;
        public short Short => Raw.Short;
        public int Int => Raw.Int;
        public bool Bool => Raw.Bool;
        public short X => Short;
        public short Y => Short;
        public byte BX => Byte;
        public byte BY => Byte;
        public byte BH => Byte;
        public int Scene => Int;
        public int Event => Int;
        public int Address => S.GetAddr(String);
        public PalDirection Direction => (PalDirection)Int;
        public PalFilter TimeFilter => (PalFilter)Int;
        public Core.EventState StateCode => (Core.EventState)Int;
        public Item Item => S.Entity.Items[Int];
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ArgRaw
    {
        [FieldOffset(0)]
        public  char        Char;
        [FieldOffset(0)]
        public  byte        Byte;
        [FieldOffset(0)]
        public  short       Short;
        [FieldOffset(0)]
        public  ushort      Word;
        [FieldOffset(0)]
        public  int         Int;
        [FieldOffset(0)]
        public  uint        DWord;
        [FieldOffset(0)]
        public  long        Long;
        [FieldOffset(0)]
        public  ulong       QWord;
        [FieldOffset(0)]
        public  bool        Bool;
        [FieldOffset(0)]
        public  float       Single;
    }

    public enum ArgType : byte
    {
        Short,
        UShort,
        Bool,
        String,
        Address,
        X,
        Y,
        BX,
        BY,
        BH,
        Scene,
        Event,
        Direction,
    }
}
