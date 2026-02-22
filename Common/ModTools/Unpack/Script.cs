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

using Records.Ts;
using SDLPal;
using System;
using System.Collections.Generic;
using System.IO;
using static Records.Pal.Core;
using EntityType = Records.Pal.Entity.Type;
using RAddress = Records.Mod.RGame.Address;

namespace ModTools.Unpack;

public static unsafe class Script
{
    /// <summary>
    /// 初始化脚本转换系统
    /// </summary>
    public static void Init()
    {
        string      pathOut, pathDependency, pathIn;

        //
        // 创建输出目录 Scirpt
        //
        pathOut = PalConfig.ModWorkPath.Assets.Data.Script;
        COS.Dir(pathOut);

        //
        // 复制脚本工作目录
        //
        pathDependency = "Dependency";
        pathIn = $@"{pathDependency}\Script";
        S.DirCopy(pathIn, "*", pathOut);
        S.DirCopy($@"{pathIn}\.vscode", "*", $@"{pathOut}\.vscode");
        S.DirCopy($@"{pathIn}\include", "*", $@"{pathOut}\include");

        //
        // 创建输出目录 Scirpt\src
        //
        COS.Dir($@"{pathOut}\src");

        //
        // 初始化脚本地址列表
        //
        PalConfig.AddAddress(0, string.Empty);
    }

    /// <summary>
    /// 将 C# bool 转换为 typescript boolean
    /// </summary>
    /// <param name="value">待转换的布尔值</param>
    /// <returns>typescript boolean</returns>
    static string GetBoolean(bool value) => value ? "true" : "false";

    static List<string> _args { get; set; } = null!;

    /// <summary>
    /// 解档 Scirpt 条目
    /// </summary>
    public static void Process()
    {
        void AddAddr(CScriptArgs arg) => PalConfig.AddAddress(arg.UShort);
        void Str(string text) => _args.Add(text);
        //void TupleNumNum((short, short) texts) => _args.AddRange([texts.Item1.ToString(), texts.Item2.ToString()]);
        //void TupleBoolNum((bool, int) texts) => _args.AddRange([GetBoolean(texts.Item1), texts.Item2.ToString()]);
        void String(string text) => Str($"\"{text}\"");
        void Addr(CScriptArgs arg) => String(arg.Addr);
        void Num(CScriptArgs arg) => Str(arg.Short.ToString());
        void UNum(CScriptArgs arg) => Str(arg.UShort.ToString());
        void Bool(CScriptArgs arg) => Str(GetBoolean(arg.Bool));
        //void SceneEvent(CScriptArgs arg) => TupleNumNum(arg.SceneEvent);
        void Scene(CScriptArgs arg) => Str(arg.SceneEvent.Item1.ToString());
        void Event(CScriptArgs arg) => Str(arg.SceneEvent.Item2.ToString());
        //void EventTrigger(CScriptArgs arg) => TupleBoolNum(arg.EventTrigger);
        void TriggerMode(CScriptArgs arg) => Str(GetBoolean(arg.TriggerMode));
        void TriggerRange(CScriptArgs arg) => Str(arg.TriggerRange.ToString());
        void HeroEntity(CScriptArgs arg) => Str(arg.HeroEntity.ToString());
        void ItemEntity(CScriptArgs arg) => Str(arg.ItemEntity.ToString());
        void MagicEntity(CScriptArgs arg) => Str(arg.MagicEntity.ToString());
        void EnemyEntity(CScriptArgs arg) => Str(arg.EnemyEntity.ToString());
        void PoisonEntity(CScriptArgs arg) => Str(arg.PoisonEntity.ToString());

        FuncInformation[]       ts;
        string?                 scriptText;
        bool                    needSelectFile, isValidScript;

        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Scirpt>");

        //
        // 读取 Scirpt 数据
        //
        var (pNative, count) = PalConfig.MkfCore.ReadChunk(4);
        var pScript = (CScript*)pNative;
        count /= sizeof(CScript);

        //
        // 创建场景脚本目录
        //
        var pathOut = PalConfig.ModWorkPath.Assets.Data.Script;
        COS.Dir($@"{pathOut}\src\Scene");

        //
        // 创建阉割语法后的 typescript 文件
        //
        using StreamWriter filePublic = File.CreateText($@"{pathOut}\src\Public.ts");
        using StreamWriter fileHero = File.CreateText($@"{pathOut}\src\Hero.ts");
        using StreamWriter fileItem = File.CreateText($@"{pathOut}\src\Item.ts");
        using StreamWriter fileMagic = File.CreateText($@"{pathOut}\src\Magic.ts");
        using StreamWriter fileEnemy = File.CreateText($@"{pathOut}\src\Enemy.ts");
        using StreamWriter filePoison = File.CreateText($@"{pathOut}\src\Poison.ts");
        var file = filePublic;
        var fileScenes = new StreamWriter[Records.Pal.Base.MaxScenes];
        COS.Dir($@"{pathOut}\src\Scene");
        for (var i = 1; i < fileScenes.Length; i++)
            fileScenes[i] = File.CreateText($@"{pathOut}\src\Scene\{i:D5}.ts");

        //
        // 处理 Scirpt
        //
        var enumTypeArgIds = (List<int>)[];
        var progress = (count / 10);
        var end = count - 1;
        var heroIdSequence = stackalloc char[3];
        for (var i = 1; i <= end; i++)
        {
            //
            // 输出处理进度
            //
            if (i % progress == 0 || i == end)
                UiUtil.Log($"Unpack the game data. <Scirpt Addr: {((float)i / count * 100):f2}%>");

            //
            // 获取当前 Scirpt 条目
            //
            var pThis = &pScript[i];
            var pArgs = (CScriptArgs*)pThis->Args;
            var name = PalMessage.GetFuncName(pThis->Command);

            //
            // 注册所有脚本中出现的地址
            //
            if ((ts = PalMessage.GetFuncData(name).Ts) != null)
                foreach (var information in ts)
                    if (information.Type == FuncInformation.SpecialType.Address)
                        AddAddr(pArgs[information.ArgId]);
        }
        needSelectFile = true;
        isValidScript = true;
        for (var i = 1; i <= end; i++)
        {
            //
            // 输出处理进度
            //
            if (i % progress == 0 || i == end)
                UiUtil.Log($"Unpack the game data. <Scirpt: {((float)i / count * 100):f2}%>");

            //
            // 获取当前 Scirpt 条目
            //
            var pThis = &pScript[i];
            var pArgs = (CScriptArgs*)pThis->Args;

            //
            // 检查脚本地址是否被注册
            //
            if (PalConfig.GetAddress(i, out var address))
            {
                if (needSelectFile)
                {
                    needSelectFile = false;

                    file = address.Type switch
                    {
                        RAddress.AddrType.Public => filePublic,
                        RAddress.AddrType.Hero => fileHero,
                        RAddress.AddrType.Item => fileItem,
                        RAddress.AddrType.Magic => fileMagic,
                        RAddress.AddrType.Enemy => fileEnemy,
                        RAddress.AddrType.Poison => filePoison,
                        RAddress.AddrType.Scene => fileScenes[address.ObjectId],
                        _ => throw new NotImplementedException(),
                    };
                }

                //
                // 写入脚本入口
                //
                file.WriteLine($"['{PalConfig.AddAddress(i)}'];");

                //
                // 将后续指令标记为有效脚本
                //
                isValidScript = true;
            }

            //
            // 处理当前脚本条目
            //
            enumTypeArgIds.Clear();
            scriptText = null;
            _args = [];
            var name = PalMessage.GetFuncName(pThis->Command);
            switch (pThis->Command)
            {
                case 0xFFFF:    // 显示对话
                    scriptText = $@"//{pArgs[0].Dialog}";
                    break;

                case 0x0000:    // 脚本块上下文结束
                    scriptText = "";
                    needSelectFile = true;
                    break;

                case 0x003B:    // 将对话框位置设置在画面中间
                    String(pArgs[0].Bool ? $"0x{pArgs[0]:X4}" : "0xFFFFFF");
                    Bool(pArgs[1]);
                    break;

                case 0x003C:    // 将对话框位置设置在画面上部
                case 0x003D:    // 将对话框位置设置在画面下部
                    UNum(pArgs[0]);
                    String(pArgs[1].Bool ? $"0x{pArgs[1]:X4}" : "0xFFFFFF");
                    Bool(pArgs[2]);
                    break;

                case 0x003E:    // 将对话框位置设置在画面中间的条幅里
                    String(pArgs[0].Bool ? $"0x{pArgs[0]:X4}" : "0xFFFFFF");
                    break;

                case 0x0075:    // 设置队伍中的队员，并自动计算和设置队伍人数
                    for (var argId = 0; argId < 3; argId++)
                        heroIdSequence[argId] = '\0';
                    var heroIdSequenceId = 0;
                    for (var argId = 0; argId < 3; argId++)
                    {
                        var arg = pThis->Args[argId];
                        if (arg != 0)
                            heroIdSequence[heroIdSequenceId++] = $"{arg}"[0];
                    }
                    _args.Add($"\"{new(heroIdSequence)}\"");
                    break;

                case 0x00A7:    // 将对话框设置在 Item/Magic 描述的位置
                    //
                    // 将后续指令标记为无效脚本
                    //
                    isValidScript = false;
                    break;

                default:        // 其他标准伪汇编指令码
                    {
                        //
                        // 检查该指令对应的是哪个函数
                        //
                        var funcData = PalMessage.GetFuncData(name);
                        var tsCases = funcData.TsCases;
                        if (tsCases != null)
                        {
                            //
                            // 获取决定最终函数名称的伪汇编参数编号
                            //
                            var argId = tsCases.ArgId;

                            //
                            // 根据实体类型名称来决定函数名称
                            //
                            name = tsCases[PalMessage.GetEntityType(pThis->Args[argId])];

                            //
                            // 重新获取对应的函数定义
                            //
                            funcData = PalMessage.GetFuncData(name);
                        }

                        //
                        // 检查该函数有没有自定义赋值参数
                        //
                        var argType = funcData.ArgType;
                        var asmCases = funcData.AssemblyCases;
                        if ((ts = funcData.Ts) != null)
                        {
                            foreach (var info in ts)
                            {
                                //
                                // 获取需要使用第几个伪汇编参数
                                //
                                var argId = info.ArgId;

                                switch (info.Type)
                                {
                                    case FuncInformation.SpecialType.Address:
                                        Addr(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.Scene:
                                        Scene(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.Event:
                                        Event(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.TriggerMode:
                                        TriggerMode(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.TriggerRange:
                                        TriggerRange(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.HeroEntity:
                                        HeroEntity(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.ItemEntity:
                                        ItemEntity(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.MagicEntity:
                                        MagicEntity(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.EnemyEntity:
                                        EnemyEntity(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.PoisonEntity:
                                        PoisonEntity(pArgs[argId]);
                                        break;

                                    case FuncInformation.SpecialType.Entity:
                                        //
                                        // 实体对象类型
                                        //
                                        var j = pThis->Args[argId];
                                        switch (PalMessage.GetEntityType(j))
                                        {
                                            case EntityType.Hero:
                                                HeroEntity(pArgs[argId]);
                                                break;

                                            case EntityType.Item:
                                                ItemEntity(pArgs[argId]);
                                                break;

                                            case EntityType.Magic:
                                                MagicEntity(pArgs[argId]);
                                                break;

                                            case EntityType.Enemy:
                                                EnemyEntity(pArgs[argId]);
                                                break;

                                            case EntityType.Poison:
                                                PoisonEntity(pArgs[argId]);
                                                break;

                                            case EntityType.System:
                                            default:
                                                //
                                                // 使用了没有脚本参数的实体，直接崩溃
                                                //
                                                S.Failed(
                                                    "Script.Process",
                                                    $"The entity numbered '{j}' does not contain a script."
                                                );
                                                break;
                                        }
                                        break;

                                    default:
                                        //
                                        // 其他基础类型、自定义数值等类型
                                        //
                                        funcData = PalMessage.GetFuncData(name);
                                        var typeName = funcData.ArgType[argId];
                                        switch (typeName)
                                        {
                                            case "boolean":
                                                Bool(pArgs[argId]);
                                                break;

                                            case "string":
                                                String($"{pArgs[argId].UShort}");
                                                break;

                                            default:
                                                if (PalMessage.TryGetTypeSign(typeName, out var isSigned))
                                                    if (isSigned)
                                                        Num(pArgs[argId]);
                                                    else
                                                        UNum(pArgs[argId]);
                                                else
                                                {
                                                    enumTypeArgIds.Add(argId);
                                                    argId = pArgs[argId].Short;

                                                    //
                                                    // 枚举类型
                                                    //
                                                    S.Failed(
                                                        "Script.Process",
                                                        $"The enumeration type '{typeName}' does not have an entry with the value '{argId}'",
                                                        PalMessage.TryGetEnumEntry(typeName, argId, out var entryName)
                                                    );

                                                    //
                                                    // 组合枚举引用
                                                    //
                                                    Str($"{typeName}.{entryName}");
                                                }
                                                break;
                                        }
                                        break;
                                }
                            }
                        }
                        else if (argType != null)
                        {
                            //
                            // 直接按原生函数声明转换参数
                            //
                            funcData = PalMessage.GetFuncData(name);
                            for (var argId = 0; argId < argType.Length; argId++)
                            {
                                var typeName = funcData.ArgType[argId];
                                switch (typeName)
                                {
                                    case "boolean":
                                        Bool(pArgs[argId]);
                                        break;

                                    case "string":
                                        String($"{pArgs[argId].UShort}");
                                        break;

                                    default:
                                        if (PalMessage.TryGetTypeSign(typeName, out var isSigned))
                                            if (isSigned)
                                                Num(pArgs[argId]);
                                            else
                                                UNum(pArgs[argId]);
                                        else
                                        {
                                            enumTypeArgIds.Add(argId);
                                            var value = pArgs[argId].Short;

                                            //
                                            // 枚举类型
                                            //
                                            S.Failed(
                                                "Script.Process",
                                                $"The enumeration type '{typeName}' does not have an entry with the value '{value}'",
                                                PalMessage.TryGetEnumEntry(typeName, value, out var entryName)
                                            );

                                            //
                                            // 组合枚举引用
                                            //
                                            Str($"{typeName}.{entryName}");
                                        }
                                        break;
                                }
                            }
                        }

                        //
                        // 根据对应的伪汇编指令来分配指定的参数
                        //
                        if (asmCases != null)
                            foreach (var asmCase in asmCases.Reverses)
                                if (asmCase.Key == pThis->Command && asmCase.Value != AssemblyCases.OtherCode)
                                {
                                    _args[asmCases.ArgId] = asmCase.Value.ToString();

                                    if (enumTypeArgIds.Contains(asmCases.ArgId))
                                    {
                                        var typeName = funcData.ArgType[asmCases.ArgId];
                                        var value = asmCase.Value;

                                        //
                                        // 枚举类型
                                        //
                                        S.Failed(
                                            "Script.Process",
                                            $"The enumeration type '{typeName}' does not have an entry with the value '{value}'",
                                            PalMessage.TryGetEnumEntry(typeName, value, out var entryName)
                                        );

                                        //
                                        // 组合枚举引用
                                        //
                                        _args[asmCases.ArgId] = $"{typeName}.{entryName}";
                                    }
                                    break;
                                }
                    }
                    break;
            }

            //
            // 跳过后续的无效脚本
            //
            if (!isValidScript)
                continue;

            //
            // 参数临时修正补丁
            //
            switch (pThis->Command)
            {
                case 0x0043:    // 设置场景音乐
                    _args[1] = GetBoolean(pArgs[1].UShort != 1);
                    _args[2] = GetBoolean((pArgs[1].UShort == 1) && (pArgs[1].UShort != 9));
                    break;
            }

            //
            // 拼合脚本条目
            //
            scriptText ??= $"{name}({string.Join(", ", _args)});";

            //
            // 写入脚本条目
            //
            file.WriteLine(scriptText);
        }

        //
        // 关闭所有场景脚本文件
        //
        foreach (var fileScene in fileScenes)
            fileScene?.Dispose();

        //
        // 释放非托管内存
        //
        C.free(pNative);
    }
}
