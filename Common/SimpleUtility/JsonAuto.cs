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

using PatchPackageTool.Records;
using Records.Mod.RGame;
using Records.Patch;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace SimpleUtility;

[JsonSerializable(typeof(short[]))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(List<string[]>))]
[JsonSerializable(typeof(HashSet<string[]>))]
[JsonSerializable(typeof(short[]))]
[JsonSerializable(typeof(BattleField))]
[JsonSerializable(typeof(HeroActionEffect))]
[JsonSerializable(typeof(EnemyPosition))]
[JsonSerializable(typeof(LevelUpExp))]
[JsonSerializable(typeof(Hero))]
[JsonSerializable(typeof(Item))]
[JsonSerializable(typeof(Magic))]
[JsonSerializable(typeof(SummonGold))]
[JsonSerializable(typeof(Enemy))]
[JsonSerializable(typeof(Poison))]
[JsonSerializable(typeof(Scene))]
[JsonSerializable(typeof(Event))]
[JsonSerializable(typeof(Event))]
[JsonSerializable(typeof(PatchConfig))]
[JsonSerializable(typeof(PatchFileInfo))]
[JsonSerializable(typeof(PatchInfo))]
[JsonSerializable(typeof(PatchIntegrity))]
public partial class JsonAuto : JsonSerializerContext;
public class JsonAutoEncoder : JavaScriptEncoder
{
    public override int MaxOutputCharactersPerInputCharacter => 1;

    public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
    {
        // 返回 -1 表示不编码任何字符
        return -1;
    }

    public override bool WillEncode(int unicodeScalar)
    {
        // 返回 false 表示不编码任何字符
        return false;
    }

    public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
    {
        numberOfCharactersWritten = 0;
        return false;
    }
}
