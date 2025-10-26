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

using Lib.Pal;
using Records.DebugMod;
using SimpleUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Lib.Pal.MkfReader;
using static Records.Pal.Core;
using RPalPath = Records.Pal.WorkPath;

namespace Lib.Mod;

public static unsafe class Message
{
    public static PalFile PalFile = new();
    public static HashSet<string[]> FunctionNames = null!;
    public static List<string> EntityNames { get; set; } = null!;
    public static List<string> Dialogues { get; set; } = null!;
    public static Dictionary<int, string> CD { get; set; } = null!;
    public static Dictionary<int, string> HeroAttackEffect { get; set; } = null!;
    public static Dictionary<int, string> Music { get; set; } = null!;
    public static Dictionary<int, string> Palette { get; set; } = null!;
    public static Dictionary<int, string> Scene { get; set; } = null!;
    public static Dictionary<int, string> Sprite { get; set; } = null!;
    public static Dictionary<int, string> Background { get; set; } = null!;
    static Dictionary<int, string[]> Descriptions { get; set; } = null!;
    static Encoding _encoding { get; set; } = null!;
    static Encoding _encodingGbk { get; set; } = null!;
    static Encoding _encodingBig5 { get; set; } = null!;

    /// <summary>
    /// 初始化全局信息数据。
    /// </summary>
    /// <param name="palWorkPath">游戏工作目录对象</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版</param>
    public static void Init(bool isDosGame, string messagePath, RPalPath palWorkPath = null!)
    {
        string      path;

        //
        // 初始化字符串编码器
        //
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _encodingBig5 = Encoding.GetEncoding("BIG5");
        _encodingGbk = Encoding.GetEncoding("GBK");
        _encoding = isDosGame ? _encodingBig5 : _encodingGbk;

        EntityNames = (palWorkPath == null) ? [] : InitEntityNames(palWorkPath, isDosGame);
        Dialogues = (palWorkPath == null) ? [] : InitDialogues(palWorkPath, isDosGame);

        path = messagePath;
        Descriptions = InitDocument($@"{path}\DESC.txt");
        HeroAttackEffect = InitCodeDocument($@"{path}\HeroAttackEffectID.txt");
        CD = InitCodeDocument($@"{path}\CDID.txt");
        Music = InitCodeDocument($@"{path}\MusicID.txt");
        Palette = InitCodeDocument($@"{path}\PaletteID.txt");
        Scene = InitCodeDocument($@"{path}\SceneID{(isDosGame ? "" : "_Win")}.txt");
        Sprite = InitCodeDocument($@"{path}\SpriteID.txt");
        Background = InitCodeDocument($@"{path}\BattleFieldID{(isDosGame ? "" : "_Win")}.txt");
    }

    /// <summary>
    /// 清除全局信息数据。
    /// </summary>
    public static void Free()
    {
        PalFile.Dispose();
        EntityNames.Clear();
        Dialogues.Clear();
        Descriptions.Clear();
        HeroAttackEffect.Clear();
        CD.Clear();
        Music.Clear();
        Palette.Clear();
        Scene.Clear();
        Sprite.Clear();
        Background.Clear();
    }

    /// <summary>
    /// 将 Span 数组从 Big5 码转换为 GBK 码
    /// </summary>
    /// <param name="span">Span 字节数组</param>
    /// <returns>转换后的 GBK 码字节数组</returns>
    public static byte[] Big5ToGbk(Span<byte> span) =>
        Encoding.Convert(_encodingBig5, _encodingGbk, span.ToArray());

    /// <summary>
    /// 将 Span 数组从 GBK 码转换为 Big5 码
    /// </summary>
    /// <param name="span">Span 字节数组</param>
    /// <returns>转换后的 Big5 码字节数组</returns>
    public static byte[] GbkToBig5(Span<byte> span) =>
        Encoding.Convert(_encodingGbk, _encodingBig5, span.ToArray());

    /// <summary>
    /// 读取实体对象名称。
    /// </summary>
    /// <param name="palWorkPath">游戏工作目录对象</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版</param>
    /// <returns>名称列表</returns>
    public static List<string> InitEntityNames(RPalPath palWorkPath, bool isDosGame)
    {
        MkfReader           mkf;
        Span<byte>          span;
        List<string>        entityNames;
        string              str;

        //
        // 读取文字文件
        //
        mkf = new(palWorkPath.DataBase.EntityName);
        span = new(new byte[10]);
        entityNames = [];

        //
        // 读取文字
        //
        while (mkf.Read(span) == 10)
        {
            //
            // 若为 DOS 版则转换编码为 GBK
            //
            str = _encodingGbk.GetString(isDosGame ? Big5ToGbk(span) : span).TrimEnd();

            entityNames.Add(str);
            span.Clear();
        }

        mkf?.Dispose();

        return entityNames;
    }

    /// <summary>
    /// 读取对话文件内容。
    /// </summary>
    /// <param name="palWorkPath">游戏工作目录对象</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版</param>
    /// <returns>对话列表</returns>
    public static List<string> InitDialogues(RPalPath palWorkPath, bool isDosGame)
    {
        FileReader          msg;
        MkfReader           mkfCore;
        int                 unitSize, count, i, charCount;
        List<string>        dialogues;
        string              str;
        Span<byte>          span;

        //
        // 读取对话文件
        //
        msg = new(palWorkPath.DataBase.Message);
        mkfCore = new(palWorkPath.DataBase.Core);
        unitSize = (int)UnitSize.DialogIndex;
        count = mkfCore.GetChunkSize(3) / unitSize - 1;
        mkfCore.SeekChunk(3);
        dialogues = [];

        //
        // 分割对话文字
        //
        i = 0;
        for (i = 0; i < count; i++)
        {
            if (((charCount = -(mkfCore.ReadInt32() - mkfCore.ReadInt32())) > 0)
                && (span = msg.ReadBytes(charCount)).Length > 0)
                //
                // 若为 DOS 版则转换编码为 GBK
                //
                str = _encodingGbk.GetString(isDosGame ? Big5ToGbk(span) : span).TrimEnd();
            else
                //
                // 空对话
                //
                str = "";

            dialogues.Add(str);
            mkfCore.Seek(-unitSize, SeekOrigin.Current);
        }

        msg?.Dispose();
        mkfCore?.Dispose();

        return dialogues;
    }

    /// <summary>
    /// 读取描述文件内容。
    /// </summary>
    /// <param name="name">文件路径</param>
    /// <returns>描述信息列表</returns>
    public static Dictionary<int, string[]> InitDocument(string name)
    {
        TextReader                      file;
        string?                         line;
        Dictionary<int, string[]>       dict;
        string[]                        contents;

        //
        // 打开描述文件
        //
        file = File.OpenText(name);

        //
        // 读取描述内容
        //
        dict = [];
        while ((line = file.ReadLine()) != null)
        {
            contents = line.Split('\t', '|');

            dict.Add(int.Parse(contents[0]), contents[1..]);
        }

        file?.Dispose();

        return dict;
    }

    /// <summary>
    /// 读取注释文档内容。
    /// </summary>
    /// <param name="name">文件路径</param>
    /// <returns>注释信息列表</returns>
    public static Dictionary<int, string> InitCodeDocument(string name)
    {
        TextReader                  file;
        string?                     line;
        Dictionary<int, string>     dict;
        string[]                    contents;

        //
        // 打开注释文档
        //
        file = File.OpenText(name);

        //
        // 读取注释内容
        //
        dict = [];
        while ((line = file.ReadLine()) != null)
        {
            contents = line.Split('\t');

            dict.Add(int.Parse(contents[0]), contents[1]);
        }

        file?.Dispose();

        return dict;
    }

    /// <summary>
    /// 获取函数名称
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string[] GetFunctionName(string name)
    {
        string[]?       result;

        result = default;
        S.Failed(
            "Message.GetFunctionName",
            $"'{name}' is not a function",
            (result = FunctionNames.FirstOrDefault(x => x.Contains(name))) != default
        );

        return result!;
    }

    /// <summary>
    /// 获取 Item 或 Magic 的描述信息
    /// </summary>
    /// <param name="entityId">对应的 Entity 编号</param>
    /// <param name="isItemOrMagic">true 为 Item，false 为 Magic</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版</param>
    /// <returns>描述信息</returns>
    public static string[]? GetDescriptions(int entityId, bool isItemOrMagic, bool isDosGame)
    {
        List<string>        descriptions;
        int                 addr;
        nint                pNative;
        CScript*            pScript, pThis;

        //
        // 整理描述信息，区分游戏资源版本
        //
        if (isDosGame)
        {
            Descriptions.TryGetValue(entityId, out var value);
            return value;
        }

        //
        // 获取 Win 版描述脚本的地址
        //
        addr = (isItemOrMagic) ?
            Config.CoreWin[entityId].Item.ScriptDesc:
            Config.CoreWin[entityId].Magic.ScriptDesc;

        //
        // 读取 Scirpt 数据
        //
        (pNative, _) = Config.MkfCore.ReadChunk(4);
        pScript = (CScript*)pNative;

        //
        // 开始整理
        //
        descriptions = [];
        while ((pThis = &pScript[addr++])->Command != 0x0000)
            if (pThis->Command == 0xFFFF)
                descriptions.Add((pThis->Args[0] < Dialogues.Count) ? Dialogues[pThis->Args[0]] : "未知实体描述");

        return [.. descriptions];
    }

    /// <summary>
    /// 获取对话
    /// </summary>
    /// <param name="EntityId">对话编号</param>
    /// <returns>对话，若查找失败则返回 "未知对话"</returns>
    public static string GetDialog(int EntityId) =>
        (EntityId < Dialogues.Count) ? Dialogues[EntityId] : "未知对话";

    /// <summary>
    /// 获取 Entity 名称
    /// </summary>
    /// <param name="EntityId">Entity 编号</param>
    /// <returns>Entity 名称，若查找失败则返回 "未知实体"</returns>
    public static string GetEntityName(int EntityId) =>
        (EntityId < EntityNames.Count) ? EntityNames[EntityId] : "未知实体";

    /// <summary>
    /// 获取场景名称
    /// </summary>
    /// <param name="SceneId">场景编号</param>
    /// <returns>场景名称，若查找失败则返回 "未知场景"</returns>
    public static string GetScene(int SceneId) =>
        Scene.TryGetValue(SceneId, out _) ? Scene[SceneId] : "未知场景";
}
