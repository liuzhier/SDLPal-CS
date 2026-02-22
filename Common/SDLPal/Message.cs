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

using ModTools.Util;
using Records.Pal;
using Records.Ts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static ModTools.Util.MkfReader;
using static Records.Pal.Core;
using RPalPath = Records.Pal.WorkPath;

namespace SDLPal;

public static unsafe class PalMessage
{
    static List<string> EntityNames { get; set; } = [];
    static List<string> Dialogues { get; set; } = [];
    static FuncName FuncNameDict { get; set; } = new();
    static Dictionary<string, bool> TypeSignDict { get; set; } = [];
    static Dictionary<string, Range[]> EntityRangeDict { get; set; } = [];
    static Dictionary<string, FuncData> FuncDict { get; set; } = [];
    static Dictionary<string, EnumData> EnumDict { get; set; } = [];
    static Dictionary<string, string[]> Descriptions { get; set; } = [];
    static Encoding EncodingGbk { get; set; } = Encoding.GetEncoding("BIG5");
    static Encoding EncodingBig5 { get; set; } = Encoding.GetEncoding("GBK");
    static Encoding Encoding { get; set; } = PalConfig.IsDosGame ? EncodingBig5 : EncodingGbk;

    /// <summary>
    /// 初始化全局信息数据。
    /// </summary>
    /// <param name="isDosGame">游戏资源是否是 Dos 版</param>
    /// <param name="TsLibraryPath">typescript 库路径</param>
    /// <param name="palWorkPath">游戏工作目录对象</param>
    public static void Init(bool isDosGame, string TsLibraryPath, RPalPath palWorkPath)
    {
        //
        // 初始化字符串编码器
        //
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        EncodingBig5 = Encoding.GetEncoding("BIG5");
        EncodingGbk = Encoding.GetEncoding("GBK");
        Encoding = isDosGame ? EncodingBig5 : EncodingGbk;

        //
        // 加载对话和实体名称
        //
        var isUnpack = (palWorkPath == null);
        EntityNames = isUnpack ? [] : InitEntityNames(palWorkPath!, isDosGame);
        Dialogues = isUnpack ? [] : InitDialogues(palWorkPath!, isDosGame);

        //
        // 加载 Ts 库数据
        //
        ReadEnum($@"{TsLibraryPath}\Enum.ts");
        ReadType($@"{TsLibraryPath}\Type.ts");
        ReadFunction($@"{TsLibraryPath}\Library.ts");
    }

    /// <summary>
    /// 清除全局信息数据。
    /// </summary>
    public static void Free()
    {
        EntityNames?.Clear();
        Dialogues?.Clear();
        Descriptions?.Clear();
    }

    /// <summary>
    /// 将 Span 数组从 Big5 码转换为 GBK 码
    /// </summary>
    /// <param name="span">Span 字节数组</param>
    /// <returns>转换后的 GBK 码字节数组</returns>
    public static byte[] Big5ToGbk(Span<byte> span) =>
        Encoding.Convert(EncodingBig5, EncodingGbk, span.ToArray());

    /// <summary>
    /// 将 Span 数组从 GBK 码转换为 Big5 码
    /// </summary>
    /// <param name="span">Span 字节数组</param>
    /// <returns>转换后的 Big5 码字节数组</returns>
    public static byte[] GbkToBig5(Span<byte> span) =>
        Encoding.Convert(EncodingGbk, EncodingBig5, span.ToArray());

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
            str = EncodingGbk.GetString(isDosGame ? Big5ToGbk(span) : span).TrimEnd();

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
                str = EncodingGbk.GetString(isDosGame ? Big5ToGbk(span) : span).TrimEnd();
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
    /// 读取 typescript 全局枚举文件中的枚举数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    static void ReadEnum(string filePath)
    {
        int                         value, count, index;
        Range[]                     ranges;
        Index                       beginOfChar, endOfChar, begin, end;
        FastChars                   line, valuesChars, valueChars;
        string                      enumName, keyName;
        string[]                    desc;
        EnumData                    enumData;
        Dictionary<string, int>     enumForward;
        Dictionary<int, string>     enumReverse;
        bool                        isInEnumContext, isStringEnum;

        //
        // 打开 typescript 全局枚举文件
        //
        using StreamReader reader = new(filePath, Encoding.UTF8);

        //
        // 预初始化变量
        //
        value = default;
        enumName = default!;
        desc = default!;
        enumData = default!;
        enumForward = default!;
        enumReverse = default!;
        isInEnumContext = default;
        isStringEnum = default;

        //
        // 开始解析 typescript 全局枚举文件
        //
        while (true)
        {
            //
            // 读取下一行内容
            //
            line = reader.ReadLine().AsSpan().Trim();

            if (line.StartsWith("//"))
            {
                //
                // 情况 1：新的实体范围被定义
                // 匹配以下格式
                // "// xxxx[xxxx..xxxx]"
                //

                //
                // 去掉开头的 "//"
                //
                beginOfChar = line.IndexOf("//") + 2;
                line = line[beginOfChar..].Trim();

                //
                // 获取实体名称
                //
                endOfChar = line.IndexOf('[');
                valueChars = line[..endOfChar].Trim();
                enumName = valueChars.ToString();

                //
                // 获取所有范围
                //
                beginOfChar = endOfChar.Value + 1;
                endOfChar = line.IndexOf(']');
                valuesChars = line[beginOfChar..endOfChar].Trim();

                //
                // 分割出每个范围
                //
                count = line.Count(',') + 1;
                EntityRangeDict[enumName] = ranges = new Range[count];
                index = 0;
                foreach (var range in valuesChars.Split(','))
                {
                    //
                    // 获取每个范围
                    //
                    valueChars = valuesChars[range];

                    //
                    // 检查范围参数 "begin"
                    //
                    if (valueChars.StartsWith(".."))
                        //
                        // 参数 "begin" 缺省，从头开始
                        //
                        begin = 0;
                    else
                    {
                        //
                        // 截取出范围的起始
                        //
                        endOfChar = valueChars.IndexOf("..");
                        begin = S.StrToUInt16(valueChars[..endOfChar]);
                    }

                    //
                    // 检查范围参数 "end"
                    //
                    if (valueChars.EndsWith(".."))
                        //
                        // 参数 "end" 缺省，直达末尾
                        //
                        end = ^0;
                    else
                    {
                        //
                        // 截取出范围的末尾
                        //
                        beginOfChar = valueChars.IndexOf("..") + 2;
                        end = S.StrToUInt16(valueChars[beginOfChar..]);
                    }

                    //
                    // 将范围放入范围池
                    //
                    ranges[index++] = begin..end;
                }
            }
            else if (line.StartsWith("enum") && line.EndsWith('{'))
            {
                //
                // 情况 2：新的枚举被定义，进入枚举上下文
                // 匹配以下格式
                // "enum xxxx {"
                //
                isInEnumContext = true;

                //
                // 提取枚举单元名
                //
                beginOfChar = line.IndexOf("enum") + 4;
                enumName = line[beginOfChar..^1].Trim().ToString();

                //
                // 初始化新的枚举单元
                //
                value = 0;
                enumData = new();
                enumForward = enumData.Forwards;
                enumReverse = enumData.Reverses;
            }
            else if (isInEnumContext && line.Contains('}'))
            {
                //
                // 情况 3：枚举单元上下文结束
                // 匹配以下格式
                // "}"
                //
                isInEnumContext = false;
                isStringEnum = false;

                //
                // 将解析完成的枚举单元放入枚举池
                //
                EnumDict[enumName] = enumData;
            }
            else if (isInEnumContext && line.Contains(','))
            {
                //
                // 情况 4：在枚举单元上下文中，读取枚举条目
                // 匹配以下格式
                // xxxx = xxxx, // xxxx
                // xxxx = xxxx,
                // xxxx, // xxxx
                // xxxx,
                //
                if (line.IndexOf("//") != -1)
                {
                    //
                    // 去掉行末尾的注释
                    //
                    endOfChar = line.IndexOf("//");
                    line = line[..endOfChar].TrimEnd();
                }

                if (line.Contains('='))
                {
                    //
                    // 判断 = 后面的是字符串还是数值
                    //
                    isStringEnum = line.Contains('"');

                    if (isStringEnum)
                    {
                        //
                        // 字符串
                        //
                        beginOfChar = line.IndexOf('"') + 1;
                        valuesChars = line[beginOfChar..^2].TrimEnd();

                        //
                        // 将字符串分割为数组
                        //
                        count = valuesChars.Count('*') + 1;
                        desc = new string[count];
                        index = 0;
                        foreach (var range in valuesChars.Split('*'))
                            desc[index++] = valuesChars[range].Trim().ToString();
                    }
                    else
                    {
                        //
                        // 数值
                        //
                        beginOfChar = line.IndexOf('=') + 1;
                        value = S.StrToInt32(line[beginOfChar..^1].TrimEnd());
                    }

                    //
                    // 定位枚举条目名结束处
                    //
                    endOfChar = line.IndexOf("=");
                }
                else
                {
                    //
                    // 用户未显式给予值，自动生成
                    //
                    if (isStringEnum)
                        value++;
                    else
                        desc = null!;

                    //
                    // 定位枚举条目名结束处
                    //
                    endOfChar = ^1;
                }

                //
                // 获取条目名
                //
                keyName = line[..endOfChar].TrimEnd().ToString();

                //
                // 将枚举条目放入枚举单元
                //
                if (isStringEnum)
                    Descriptions[keyName] = desc;
                else
                {
                    enumForward[keyName] = value;
                    enumReverse[value] = keyName;
                }
            }
            else if (line == "")
                //
                // 情况 5：空行
                // 直接跳过
                //
                continue;
            else if (line.IsEmpty)
                //
                // 情况 6：内容为 null
                // 文件读取完毕，结束读取
                //
                break;
        }
    }

    /// <summary>
    /// 读取 typescript 全局自定义类型文件中的类型数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    static void ReadType(string filePath)
    {
        FastChars       line, valuesChars, valueChars;
        Index           beginOfChar, endOfChar;
        string          typeName;
        bool            typeIsSigned;

        //
        // 打开 typescript 全局枚举文件
        //
        using StreamReader reader = new(filePath, Encoding.UTF8);

        //
        // 开始解析 typescript 全局自定义类型文件
        //
        while (true)
        {
            //
            // 读取下一行内容
            //
            line = reader.ReadLine().AsSpan().Trim();

            if (line.StartsWith("type") && line.Contains('=') && line.EndsWith(';'))
            {
                //
                // 情况 1：新的类型被定义，进入类型上下文
                // 匹配 type xxxx = xxxx; 格式
                //

                //
                // 截取类型名称
                //
                endOfChar = line.IndexOf('=');
                typeName = line[4..endOfChar].Trim().ToString();

                //
                // 获取类型对应的所有数值
                //
                beginOfChar = endOfChar.Value + 1;
                valuesChars = line[beginOfChar..^1].Trim();

                //
                // 判断该数值类型是否有符号
                //
                typeIsSigned = false;
                foreach (var range in valuesChars.Split('|'))
                {
                    //
                    // 获取单个值
                    //
                    valueChars = valuesChars[range].Trim();

                    if (valueChars.Contains('-'))
                    {
                        //
                        // 情况 1：有符号的数值
                        // 匹配 -xxxx
                        //
                        typeIsSigned = true;
                        break;
                    }
                    else if (TypeSignDict.TryGetValue(valueChars.ToString(), out typeIsSigned) && typeIsSigned)
                        //
                        // 情况 2：其他自定义类型
                        //
                        break;
                }

                //
                // 将类型条目放入类型单元
                //
                TypeSignDict[typeName] = typeIsSigned;
            }
            else if (line == "")
                //
                // 情况 4：空行
                // 直接跳过
                //
                continue;
            else if (line.IsEmpty)
                //
                // 情况 2：内容为 null
                // 文件读取完毕，结束读取
                //
                break;
        }
    }

    /// <summary>
    /// 读取 typescript 全局 api 文件中的函数数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    static void ReadFunction(string filePath)
    {
        FastChars               otherChars, line, valuesChars, valueChars, dictChars, argChars;
        Index                   beginOfChar, endOfChar;
        string                  functionName, arg, entityName, caseValue;
        Entity.Type             entityType;
        FuncData                functionData;
        int                     argCount, argId, subArgId, key;
        FuncInformation[]       informations;
        FuncInformation         information;
        List<string>            specialTypeNames;
        TsCases                 tsCases;
        AssemblyCases           assemblyCases;
        ushort                  value;
        ushort[]                commands;

        //
        // 打开 typescript 全局 api 文件
        //
        using StreamReader reader = new(filePath, Encoding.UTF8);

        //
        // 预初始化变量
        //
        otherChars = "other";
        commands = default!;
        functionName = default!;
        functionData = default!;

        //
        // 开始解析 typescript 全局 api 文件
        //
        while (true)
        {
            //
            // 读取下一行内容
            //
            line = reader.ReadLine().AsSpan().Trim();

            if (line.StartsWith('*') && line.Contains('-') && (line.IndexOf("0x") != -1))
            {
                //
                // 情况 1：函数对应的伪汇编指令集，匹配以下格式
                // "* - 0xFFFF；0xFFFF"
                // "* - 0xFFFF"
                //

                //
                // 获取指令集串
                //
                beginOfChar = line.IndexOf("-") + 1;
                line = line[beginOfChar..].Trim();
                commands = new ushort[line.Count(';') + 1];

                //
                // 分割出每个指令
                //
                argId = 0;
                foreach (var range in line.Split(';'))
                {
                    valueChars = line[range].Trim();
                    commands[argId++] = S.StrToUInt16(valueChars);
                }
            }
            else if (line.StartsWith("function") && line.EndsWith(": void;"))
            {
                //
                // 情况 2：函数，匹配以下格式
                // "function xxxx(xxxx: xxxx): void;"
                //

                //
                // 获取函数名称
                //
                endOfChar = line.IndexOf('(');
                functionName = line[8..endOfChar].Trim().ToString();
                functionData = FuncDict[functionName] = new();

                //
                // 记录该函数对应的所有伪汇编指令
                //
                argId = 0;
                foreach (var command in commands)
                {
                    if (!TryGetFuncName(command, out var _))
                        //
                        // 检查字典是否存在此键，不存在则创建
                        //
                        FuncNameDict[command] = [];

                    FuncNameDict[command].Add(functionName);
                }

                //
                // 获取所有参数
                //
                beginOfChar = endOfChar.Value + 1;
                endOfChar = line.IndexOf(')');
                line = line[beginOfChar..endOfChar];

                //
                // 获取参数数量
                //
                if (!line.IsEmpty)
                {
                    argCount = line.Count(',') + 1;
                    functionData.ArgType = new string[argCount];
                    functionData.ArgCanBeOmitted = new bool[argCount];

                    //
                    // 分割出每个参数的类型
                    //
                    argId = 0;
                    foreach (var range in line.Split(','))
                    {
                        //
                        // 检查此参数是否可缺省
                        //
                        valuesChars = line[range].Trim();
                        functionData.ArgCanBeOmitted[argId] = valuesChars.Contains('?');

                        //
                        // 获取参数类型
                        //
                        beginOfChar = valuesChars.IndexOf(':') + 1;
                        valuesChars = valuesChars[beginOfChar..].Trim();

                        //
                        // 将参数类型放入类型池
                        //
                        arg = valuesChars.ToString();
                        //if (TryGetTypeSign(arg, out var typeIsSigned))
                            //
                            // 获取成功，自定义类型，判断超集
                            //
                            //functionData.ArgType[argId++] = typeIsSigned ? "int" : "uint";
                        //else
                            //
                            // 获取失败，基础数据类型（bool 或 string）
                            //
                            functionData.ArgType[argId++] = arg;
                    }
                }
            }
            else if (line.StartsWith("//"))
            {
                //
                // 情况 3：额外处理信息，匹配以下格式
                // "// xxxx"
                //
                line = line.Trim();

                //
                // 获取里面每个参数的
                //
                if (line.Contains('(') && line.Contains(')'))
                {
                    //
                    // 情况 1：Ts 和伪汇编互转换逻辑
                    //
                    beginOfChar = line.IndexOf("//") + 2;
                    line = line[beginOfChar..].Trim();

                    //
                    // 获取所有的转换参数
                    //
                    beginOfChar = line.IndexOf('(') + 1;
                    endOfChar = line.IndexOf(')');
                    valuesChars = line[beginOfChar..endOfChar].Trim();

                    //
                    // 获取参数数量
                    //
                    argCount = valuesChars.Count(',') + 1;
                    informations = new FuncInformation[argCount];
                    argId = 0;
                    foreach (var range in valuesChars.Split(','))
                    {
                        //
                        // 获取单个转换参数
                        //
                        valueChars = valuesChars[range].Trim();

                        //
                        // 判断是否为场景事件合并参数
                        //
                        information = informations[argId++] = new();
                        if (valueChars.Contains('&'))
                        {
                            //
                            // 情况 1：自定义类型的合并式参数，匹配以下格式
                            // "x@xxxx&x@xxxx"
                            //

                            //
                            // 带 "&" 说明有多个参数
                            //
                            argCount = valueChars.Count('&') + 1;
                            information.ArgIds = new int[argCount];
                            specialTypeNames = [];

                            //
                            // 分割出合并类型的每个参数类型
                            //
                            subArgId = 0;
                            foreach (var range2 in valueChars.Split("&"))
                            {
                                //
                                //
                                //
                                argChars = valueChars[range2].Trim();

                                //
                                // 取出相关参数
                                //
                                endOfChar = argChars.IndexOf('@');
                                information.ArgIds[subArgId] = S.StrToInt32(argChars[..endOfChar]);

                                //
                                // 取出类型
                                //
                                beginOfChar = argChars.IndexOf('@') + 1;
                                argChars = argChars[beginOfChar..].Trim();
                                specialTypeNames.Add(argChars.ToString());

                                subArgId++;
                            }
                            information.SetTypeNames(specialTypeNames);
                        }
                        else if (valueChars.Contains('@'))
                        {
                            //
                            // 情况 2：自定义类型的单参数，匹配以下格式
                            // "x@xxxx"
                            //

                            //
                            // 获取引用到的伪汇编参数编号
                            //
                            endOfChar = valueChars.IndexOf('@');
                            beginOfChar = endOfChar.Value - 1;
                            information.ArgId = S.StrToInt32(valueChars[beginOfChar..endOfChar]);
                            beginOfChar = endOfChar.Value + 1;
                            information.SetTypeName(valueChars[beginOfChar..].ToString());
                        }
                        else
                            //
                            // 情况 2：默认使用函数声明的类型，匹配以下格式
                            // "x"
                            //
                            // 获取引用到的伪汇编参数编号
                            //
                            information.ArgId = S.StrToInt32(valueChars);
                    }

                    if (line.StartsWith("Ts"))
                        //
                        // 情况 1：伪汇编转换 Ts，匹配以下格式
                        // "// Ts(xxxx, xxxx)"
                        //
                        functionData.Ts = informations;
                }
                else if (line.Contains('[') && line.Contains('['))
                {
                    //
                    // 情况 2：该 Ts 函数对应多种伪汇编指令，匹配以下格式
                    // "// xxxx[xxxx=xxxx, xxxx=xxxx]"
                    //

                    //
                    // 获取关联的 Ts 函数参数
                    //
                    beginOfChar = line.IndexOf("//") + 2;
                    endOfChar = line.IndexOf('[');
                    valuesChars = line[beginOfChar..endOfChar].Trim();
                    assemblyCases = functionData.AssemblyCases = new();
                    assemblyCases.ArgId = S.StrToInt32(valuesChars);

                    //
                    // 获取所有的转换参数
                    //
                    beginOfChar = line.IndexOf('[') + 1;
                    endOfChar = line.IndexOf(']');
                    valuesChars = line[beginOfChar..endOfChar].Trim();

                    //
                    // 分割出每个转换参数
                    //
                    foreach (var range in valuesChars.Split(','))
                    {
                        //
                        // 获取单个转换参数
                        //
                        valueChars = valuesChars[range].Trim();

                        //
                        // 分割出键和值
                        //
                        endOfChar = valueChars.IndexOf('=');
                        dictChars = valueChars[..endOfChar].Trim();
                        key = (dictChars.SequenceEqual(otherChars)) ? AssemblyCases.OtherCode : S.StrToInt32(dictChars);
                        beginOfChar = endOfChar.Value + 1;
                        dictChars = valueChars[beginOfChar..].Trim();
                        value = S.StrToUInt16(dictChars);

                        //
                        // 将转换参数条目放入转换参数池
                        //
                        assemblyCases.Forwards[key] = value;
                        assemblyCases.Reverses[value] = key;
                    }
                }
                else if (line.Contains('{') && line.Contains('}'))
                {
                    //
                    // 情况 3：根据伪汇编参数取值范围来决定使用哪个 Ts 函数，匹配以下格式
                    // "// xxxx{@xxxx=xxxx, @xxxx=xxxx}"
                    //

                    //
                    // 获取关联的伪汇编参数
                    //
                    beginOfChar = line.IndexOf("//") + 2;
                    endOfChar = line.IndexOf('{');
                    valuesChars = line[beginOfChar..endOfChar].Trim();
                    tsCases = functionData.TsCases = new();
                    tsCases.ArgId = S.StrToInt32(valuesChars);

                    //
                    // 获取所有的转换参数
                    //
                    beginOfChar = line.IndexOf('{') + 1;
                    endOfChar = line.IndexOf('}');
                    valuesChars = line[beginOfChar..endOfChar].Trim();

                    //
                    // 分割出每个转换参数
                    //
                    foreach (var range in valuesChars.Split(','))
                    {
                        //
                        // 获取单个转换参数
                        //
                        valueChars = valuesChars[range].Trim();

                        //
                        // 分割出键和值
                        //
                        beginOfChar = valueChars.IndexOf('@') + 1;
                        endOfChar = valueChars.IndexOf('=');
                        dictChars = valueChars[beginOfChar..endOfChar].Trim();
                        entityName = dictChars.ToString();
                        beginOfChar = endOfChar.Value + 1;
                        dictChars = valueChars[beginOfChar..].Trim();
                        caseValue = dictChars.ToString();

                        //
                        // 将转换参数条目放入转换参数池
                        //
                        entityType = GetEntityType(entityName);
                        tsCases.Forwards[entityType] = caseValue;
                        tsCases.Reverses[caseValue] = entityType;
                    }
                }
            }
            else if (line == "")
                //
                // 情况 4：空行
                // 直接跳过
                //
                continue;
            else if (line.IsEmpty)
                //
                // 情况 5：内容为 null
                // 文件读取完毕，结束读取
                //
                break;
        }
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
        string              entityName;
        List<string>        descriptions;
        int                 addr;
        nint                pNative;
        CScript*            pScript, pThis;

        //
        // 整理描述信息，区分游戏资源版本
        //
        if (isDosGame)
        {
            entityName = GetEntityName(entityId);
            Descriptions.TryGetValue(entityName, out var value);
            return value;
        }

        //
        // 获取 Win 版描述脚本的地址
        //
        addr = (isItemOrMagic) ?
            PalConfig.CoreWin[entityId].Item.ScriptDesc :
            PalConfig.CoreWin[entityId].Magic.ScriptDesc;

        //
        // 读取 Scirpt 数据
        //
        (pNative, _) = PalConfig.MkfCore.ReadChunk(4);
        pScript = (CScript*)pNative;

        //
        // 开始整理
        //
        descriptions = [];
        while ((pThis = &pScript[addr++])->Command != 0x0000)
            if (pThis->Command == 0xFFFF)
                descriptions.Add(GetDialogue(pThis->Args[0], "未知实体描述"));

        return [.. descriptions];
    }

    /// <summary>
    /// 获取对话
    /// </summary>
    /// <param name="dialogueId">对话编号</param>
    /// <returns>对话，若查找失败则返回 "未知对话"</returns>
    public static string GetDialogue(int dialogueId, string defaultDlg = "未知对话") =>
        (dialogueId < Dialogues.Count) ? Dialogues[dialogueId] : defaultDlg;

    /// <summary>
    /// 将新对话放入列表
    /// </summary>
    /// <param name="dialogue">新的对话</param>
    public static void AddDialogue(string dialogue) =>
        Dialogues.Add(dialogue);

    /// <summary>
    /// 获取对话数量
    /// </summary>
    public static int DialogueCount => Dialogues.Count;

    /// <summary>
    /// 获取 Entity 名称
    /// </summary>
    /// <param name="entityId">Entity 编号</param>
    /// <param name="defaultName">默认返回的 Entity 名称</param>
    /// <returns>Entity 名称，若查找失败则返回 "未知实体"</returns>
    public static string GetEntityName(int entityId, string defaultName = "未知实体") =>
        (0 <= entityId && entityId < EntityNames.Count) ? EntityNames[entityId] : defaultName;

    public static List<string> GetEntityName(Range range) =>
        EntityNames[range];

    /// <summary>
    /// 将新 Entity 名称放入列表
    /// </summary>
    /// <param name="entityName">Entity 名称</param>
    public static void AddEntityName(string entityName) =>
        EntityNames.Add(entityName);

    /// <summary>
    /// 将新 Entity 名称数组插入列表末尾
    /// </summary>
    /// <param name="entityName">Entity 名称</param>
    public static void AddEntityNames(string[] entityNames) =>
        EntityNames.AddRange(entityNames);

    /// <summary>
    /// 获取 Entity 名称数量
    /// </summary>
    public static int EntityNameCount => EntityNames.Count;

    /// <summary>
    /// 尝试获取函数名称，失败则返回字符串默认值
    /// </summary>
    /// <param name="command">函数对应的伪汇编命令</param>
    /// <param name="funcName">函数名称</param>
    /// <returns>是否获取成功了</returns>
    public static bool TryGetFuncName(ushort command, out List<string> funcName) =>
        FuncNameDict.Forwards.TryGetValue(command, out funcName!);

    /// <summary>
    /// 获取函数名称
    /// </summary>
    /// <param name="command">函数对应的伪汇编命令</param>
    /// <returns>伪汇编命令对应的函数名称</returns>
    public static string GetFuncName(ushort command)
    {
        S.Failed(
            "Message.GetFuncName",
            $"'0x{command:X4}' is not a valid command.",
            TryGetFuncName(command, out var funcName)
        );

        return funcName![0];
    }

    /// <summary>
    /// 尝试获取函数名称对应的伪汇编命令
    /// </summary>
    /// <param name="funcName"></param>
    /// <returns></returns>
    public static bool TryGetAssemblyCommand(string funcName, out ushort command)
    {
        bool ret;

        var kvp = FuncNameDict.Reverses.FirstOrDefault(kvp => kvp.Key.Contains(funcName));

        command = (ret = (kvp.Key != null)) ? kvp.Value : default;

        return ret;
    }

    /// <summary>
    /// 获取函数名称对应的伪汇编命令
    /// </summary>
    /// <param name="funcName">函数名称</param>
    /// <returns>函数名称对应的伪汇编命令</returns>
    public static ushort GetAssemblyCommand(string funcName)
    {
        S.Failed(
            "Message.GetFuncName",
            $"The function '{funcName}' is undefined",
            TryGetAssemblyCommand(funcName, out var command)
        );

        return command;
    }

    /// <summary>
    /// 获取枚举值对应的条目名称，失败则崩溃
    /// </summary>
    /// <param name="enumName">枚举名称</param>
    /// <param name="enumValue">枚举值</param>
    /// <returns>枚举值对应的条目名称</returns>
    public static EnumData GetEnum(string enumName)
    {
        S.Failed(
            "Message.GetEnumValue",
            $"The enumeration '{enumName}' is undefined",
            EnumDict.TryGetValue(enumName, out var enumData)
        );

        return enumData!;
    }

    /// <summary>
    /// 尝试获取枚举值对应的条目名称，失败则返回字符串默认值
    /// </summary>
    /// <param name="enumName">枚举名称</param>
    /// <param name="enumValue">枚举值</param>
    /// <param name="entryName">枚举值对应的条目名称</param>
    /// <returns>是否获取成功了</returns>
    public static bool TryGetEnumEntry(string enumName, int enumValue, out string entryName)
    {
        entryName = "未知";

        return EnumDict.TryGetValue(enumName, out var enumData)
            && enumData!.Reverses.TryGetValue(enumValue, out entryName!);
    }

    /// <summary>
    /// 获取枚举值对应的条目名称，失败则崩溃
    /// </summary>
    /// <param name="enumName">枚举名称</param>
    /// <param name="enumValue">枚举值</param>
    /// <returns>枚举值对应的条目名称</returns>
    public static string GetEnumEntry(string enumName, int enumValue)
    {
        S.Failed(
            "Message.GetEnumEntry",
            $"The enumeration '{enumName}->{enumValue}' is undefined",
            TryGetEnumEntry(enumName, enumValue, out var entryName)
        );

        return entryName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enumName"></param>
    /// <param name="entryName"></param>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static bool TryGetEnumValue(string enumName, string entryName, out int enumValue)
    {
        enumValue = int.MinValue;

        return EnumDict.TryGetValue(enumName, out var enumData)
            && enumData!.Forwards.TryGetValue(entryName, out enumValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enumName"></param>
    /// <param name="entryName"></param>
    /// <returns></returns>
    public static int GetEnumValue(string enumName, string entryName)
    {
        S.Failed(
            "Message.GetEnumValue",
            $"The enumeration '{enumName}' is undefined",
            TryGetEnumValue(enumName, entryName, out var value)
        );

        return value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static bool GetTypeSign(string typeName)
    {
        S.Failed(
            "Message.GetTypeDict",
            $"Type '{typeName}' is undefined",
            TypeSignDict.TryGetValue(typeName, out var isSigned)
        );

        return isSigned;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="isSigned"></param>
    /// <returns></returns>
    public static bool TryGetTypeSign(string typeName, out bool isSigned) =>
        TypeSignDict.TryGetValue(typeName, out isSigned);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="funcName"></param>
    /// <returns></returns>
    public static FuncData GetFuncData(string funcName)
    {
        S.Failed(
            "Message.GetFuncData",
            $"The function '{funcName}' is undefined",
            FuncDict.TryGetValue(funcName, out var funcData)
        );

        return funcData!;
    }

    /// <summary>
    /// 获取实体编号对应的实体类型名称
    /// </summary>
    /// <param name="entityId">实体编号</param>
    /// <returns>实体类型名称</returns>
    public static string GetEntityTypeName(int entityId)
    {
        string      entityType;
        int         begin, end;

        //
        // 遍历每个实体类型的范围
        //
        foreach (var dictEntry in EntityRangeDict)
        {
            //
            // 获取实体类型名称
            //
            entityType = dictEntry.Key;

            //
            // 遍历该实体类型的范围
            //
            foreach (var ranges in dictEntry.Value)
            {
                //
                // 获取该范围准确的极限值
                //
                begin = ranges.Start.GetOffset(EntityNames.Count);
                end = ranges.End.GetOffset(EntityNames.Count);

                //
                // 检查实体编号是否在两极限值之间
                // （Range 是左闭右开的）
                //
                if ((begin <= entityId) && (entityId < end))
                    return entityType;
            }
        }

        //
        // 该实体索引不属于任何区间，直接崩溃
        //
        S.Failed(
            "Message.GetEntityType",
            $"Non-existent entity number '{entityId}'"
        );

        //
        // 不会真的执行到这里，因为程序会在上一语句崩溃
        //
        return null!;
    }

    /// <summary>
    /// 获取实体名称对应的实体类型
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <returns>实体类型</returns>
    public static Entity.Type GetEntityType(string entityName) =>
        entityName switch
        {
            "System" => Entity.Type.System,
            "Hero" => Entity.Type.Hero,
            "Item" => Entity.Type.Item,
            "Magic" => Entity.Type.Magic,
            "Enemy" => Entity.Type.Enemy,
            "Poison" => Entity.Type.Poison,
            _ => throw S.Failed("Message.GetEntityType", $"Non-existent entity '{entityName}'"),
        };

    /// <summary>
    /// 获取实体编号对应的实体类型
    /// </summary>
    /// <param name="entityId">实体编号</param>
    /// <returns>实体类型</returns>
    public static Entity.Type GetEntityType(int entityId) =>
        GetEntityType(GetEntityTypeName(entityId));
}
