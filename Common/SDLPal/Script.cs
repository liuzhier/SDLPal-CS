using ModTools.Util;
using Records.Mod.RGame;
using Records.Pal;
using Records.Ts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SDLPal;

public static partial class PalScript
{
    static Script[] Scripts { get; set; } = [];
    static List<FunctionEntry> FunctionEntries { get; set; } = [];
    static Script Script { get; set; } = null!;
    static FunctionEntry Entry { get; set; } = null!;
    static string Name => Entry.Name;
    static string[] Args => Entry.Args;
    static int ArgId { get; set; }
    static ushort Address { get; set; }
    static LogWriter ScriptLogger { get; set; } = new($@"{PalConfig.LogOutPath}\Script.txt");

    //
    // 将脚本编译为可用的 ASM 指令数据
    //
    public static void Init()
    {
        //
        // 预处理脚本文件
        //
        Preprocessing();

        //
        // 将脚本编译为 ASM 指令
        //
        CompileAsm();
    }

    public static void Free() => ScriptLogger?.Dispose();

    /// <summary>
    /// 预处理各种脚本
    /// </summary>
    static void Preprocessing()
    {
        string      rootPath, scenePath, mainPath;
        int         i, endId;

        //
        // 获取脚本根目录
        //
        rootPath = PalConfig.ModWorkPath.Assets.Data.Script;
        mainPath = $@"{rootPath}\src";
        scenePath = $@"{mainPath}\Scene";

        //
        // 初始化默认空地址
        //
        Address = 1;
        PalConfig.AddNewAddress("", 0);
        PalConfig.AddNewEventId(0xFFFF, 0xFFFF, 0xFFFF);
        PalConfig.AddNewEventId(0, 0, 0);

        //
        // 处理 Scene.ts
        //
        {
            //
            // 输出处理进度
            //
            S.Log("Process Scene.ts/Event.ts.");

            const   int     beginId = 1;

            //
            // 检查有多少项 Scene.ts
            //
            endId = ModUtil.GetFileSequenceCount(scenePath, beginId, fileSuffix: ".ts");

            //
            // 输出 Scene.ts 计数
            //
            S.Log($"Find {endId} scene typescript files. There are actually {endId = Math.Min(endId, Base.MaxEffectiveScenes)} valid ones");

            for (i = beginId; i <= endId; i++)
                //
                // 处理脚本文件
                //
                PreprocessingTypeScript($@"{scenePath}\{i:D5}.ts");
        }

        //
        // 处理 Public.ts
        //
        {
            //
            // 输出处理进度
            //
            S.Log("Process Public.ts.");

            //
            // 处理脚本文件
            //
            PreprocessingTypeScript($@"{mainPath}\Public.ts");
        }

        //
        // 处理 Enemy.ts
        //
        {
            //
            // 输出处理进度
            //
            S.Log("Process Enemy.ts.");

            //
            // 处理脚本文件
            //
            PreprocessingTypeScript($@"{mainPath}\Enemy.ts");
        }

        //
        // 处理 Hero.ts
        //
        {
            //
            // 输出处理进度
            //
            S.Log("Process Hero.ts.");

            //
            // 处理脚本文件
            //
            PreprocessingTypeScript($@"{mainPath}\Hero.ts");
        }

        //
        // 处理 Poison.ts
        //
        {
            //
            // 输出处理进度
            //
            S.Log("Process Poison.ts.");

            //
            // 处理脚本文件
            //
            PreprocessingTypeScript($@"{mainPath}\Poison.ts");
        }

        //
        // 处理 Item.ts
        //
        {
            //
            // 输出处理进度
            //
            S.Log("Process Item.ts.");

            //
            // 处理脚本文件
            //
            PreprocessingTypeScript($@"{mainPath}\Item.ts");
        }

        //
        // 处理 Magic.ts
        //
        {
            //
            // 输出处理进度
            //
            S.Log("Process Magic.ts.");

            //
            // 处理脚本文件
            //
            PreprocessingTypeScript($@"{mainPath}\Magic.ts");
        }
    }

    /// <summary>
    /// 预处理阉割语法后的 typescript
    /// </summary>
    static void PreprocessingTypeScript(string filePath)
    {
        int                 lineId, charBegin, charEnd;
        FastChars           line, arguments, arg;
        List<string>        args;
        string              addressTag, functionName, typeName, entryName;

        //
        // 打开 typescript 文件
        //
        using StreamReader reader = new(filePath, Encoding.UTF8);

        var isRunning = true;
        for (lineId = 1; isRunning; lineId++)
        {
            //
            // 读取数据块
            //
            line = reader.ReadLine().AsSpan().Trim();

            //
            // 逐行读取缓冲区中的内容
            // 按情况出现频率进行先后判断
            //
            functionName = default!;
            args = [];
            if (line.StartsWith("//"))
            {
                //
                // 情况 1：对话文本
                // 匹配 //xxxx 格式
                //

                //
                // 将 //xxxx 的 xxxx 提取为对话，并放入对话列表
                //
                var message = line[2..];

                //
                // 空行代表脚本上下文结束
                //
                functionName = PalMessage.GetFuncName(0xFFFF);
                args.Add($"{message}");
            }
            else if (line.Contains('(') && line.EndsWith(");") && line.IndexOf('(') < line.IndexOf(')'))
            {
                //
                // 情况 2：函数调用
                // 匹配 xxxx(xxxx, xxxx, xxxx); 格式
                //

                //
                // 提取函数名
                //
                charEnd = line.IndexOf('(');
                functionName = line[..charEnd].Trim().ToString();

                //
                // 提取参数
                //
                charBegin = line.IndexOf('(') + 1;
                charEnd = line.IndexOf(')');
                arguments = line[charBegin..charEnd];
                foreach (Range range in arguments.Split(','))
                {
                    //
                    // 分割出每个参数
                    //
                    arg = arguments[range].Trim();

                    if (arg.StartsWith('"') && arg.StartsWith('"'))
                    {
                        //
                        // 地址类型，直接去掉引号
                        //
                        arg = arg[1..^1];
                    }
                    else if (arg.Contains('.'))
                    {
                        //
                        // 枚举类型，转换为数值
                        //
                        charEnd = arg.IndexOf('.');
                        typeName = arg[..charEnd].ToString();
                        charBegin = charEnd + 1;
                        entryName = arg[charBegin..].ToString();

                        //
                        // 枚举类型
                        //
                        S.Failed(
                            "Script.Process",
                            $"There is no entry named '{typeName} in the enumeration type '{entryName}'",
                            PalMessage.TryGetEnumValue(typeName, entryName, out var value)
                        );

                        //
                        // 解引用，获取对应的枚举值
                        //
                        args.Add($"{value}");

                        continue;
                    }

                    args.Add(arg.ToString());
                }
            }
            else if (line.StartsWith("['") && line.EndsWith("'];"))
            {
                //
                // 情况 3：地址标签
                // 匹配 ['xxxx'] 格式
                //

                //
                // 将 ['xxxx'] 内的 xxxx 提取为脚本标签
                //
                charBegin = line.IndexOf("['") + 2;
                charEnd = line.IndexOf("']");
                addressTag = line[charBegin..charEnd].Trim().ToString();

                //
                // 将脚本标签 xxxx 作为键，新分配的地址作为值
                //
                PalConfig.AddNewAddress(addressTag, Address);

                //
                // 跳过地址分配
                //
                continue;
            }
            else if (line == "")
            {
                //
                // 情况 4：空行
                // 代表脚本上下文结束
                //

                //
                // 记录为脚本块结束标志
                //
                functionName = PalMessage.GetFuncName(0x0000);
            }
            else if (line.IsEmpty)
            {
                //
                // 情况 4：内容为 null
                // 文件读取完毕，结束读取
                //
                isRunning = false;

                //
                // 补充脚本块结束标志，以免脚本执行越界
                //
                functionName = PalMessage.GetFuncName(0x0000);
            }
            else
                //
                // 情况 5：内容为 null
                // 格式不匹配，报错退出
                //
                S.Failed(
                    "Script.ProcessTypeScript",
                    $"There is a syntax error in the script!\n{filePath}:\nLine {lineId}: '{line}'"
                );

            //
            // 将函数条目放入列表
            //
            FunctionEntries.Add(new(
                Name: functionName,
                Args: [.. args]
            ));

            //
            // 分配下一条地址
            //
            S.Failed(
                "Script.ProcessTypeScript",
                $"The number of lines of the script exceeds the maximum limit!\n{filePath}:\nLine {lineId}: '{line}'",
                Address++ < 0xFFFF
            );
        }
    }

    public static void CompileAsm()
    {
        int GetValue(string textValue) => S.StrToInt32(textValue);
        int GetBoolean(string textValue) => ((textValue == "true") ? 1 : 0);
        int GetArgVal(int argId) => GetValue(Args[argId]);
        int GetArgBool(int argId) => GetBoolean(Args[argId]);
        void Command(int command = -1) => Script.Command = (ushort)((command == -1) ? PalMessage.GetAssemblyCommand(Name) : command);
        void Value(int value) => Script.Args[ArgId++].Raw.Int = value;
        void Val(int argId) => Value(GetArgVal(argId));
        void String(string text) => Script.Args[ArgId++].String = text;
        void Str(int argId) => String(Args[argId]);
        void Bool(int argId) => Value(GetArgBool(argId));

        //
        // 将“助记符（Ts 脚本）”转换为“伪定长汇编码（ASM）”
        //
        var count = (ushort)(FunctionEntries.Count + 1);
        Scripts = new Script[count];
        var progress = (count / 10);
        var end = count - 1;
        for (var i = 1; i <= end; i++)
        {
            //
            // 输出处理进度
            //
            //if (i % progress == 0 || i == end) S.Log($"Compiling the game data. <Scirpt Addr: {((float)i / count * 100):f2}%>");

            //
            // 获取当前条目
            //
            Entry = FunctionEntries[i - 1];
            Script = Scripts[i] = new(Entry.Args.Length);
            ArgId = 0;

            //
            // 编译为默认 ASM 指令
            //
            Command();

            //
            // 检查函数与 ASM 指令关系是否为一对多
            //
            var funcData = PalMessage.GetFuncData(Name);
            var asmCase = funcData.AssemblyCases;
            if (asmCase != null)
            {
                //
                // 获取决定二者关系的 Ts 参数编号
                //
                var argId = asmCase.ArgId;

                if (asmCase.Forwards.TryGetValue(GetArgVal(argId), out var command))
                    //
                    // 根据参数值找到了对应的 ASM 指令
                    // 匹配以下格式:
                    // "XXXX=0xXXXX"
                    //
                    Command(command);
                else
                    //
                    // 
                    // 匹配以下格式:
                    // "other=0xXXXX"
                    //
                    Command(asmCase.Forwards[AssemblyCases.OtherCode]);
            }

            for (var argId = 0; (funcData.ArgType != null) && (argId < funcData.ArgType.Length); argId++)
            {
                //
                // 其他基础类型、自定义数值等类型
                //
                var typeName = funcData.ArgType[argId];
                switch (typeName)
                {
                    case "string":
                        Str(argId);
                        break;

                    case "boolean":
                        Bool(argId);
                        break;

                    default:
                        Val(argId);
                        break;
                }
            }

            //
            // 打印编译日志
            //
            //LogAsmMessage(i);
        }
    }

    public static string MakeScript(int address) => $"0x{address:X4}";

    static int MaxConsoleAlignSize { get; set; } = 0;

    /// <summary>
    /// 计算字符串在控制台显示时的视觉宽度
    /// 规则：英文/数字占1位，中文占2位
    /// </summary>
    static string Format(string prefix, string text)
    {
        var width = 1;

        foreach (char c in prefix)
            // 判断是否为全角字符（中文、全角符号等）
            // 简单判断：ASCII码 > 127 通常视为双宽字符
            width += (c > 127) ? 2 : 1;

        MaxConsoleAlignSize = int.Max(MaxConsoleAlignSize, width);

        return $"{prefix}{new string('─', MaxConsoleAlignSize - width + 1)}＞{text}";
    }

    public static void LogAsmMessage(int address, string prefix = "")
    {
        var script = Scripts[address];
        var args = script.Args;

        //
        // 根据 Asm 码生成注释
        //
        var typeName = script.Command switch
        {
            0xFFFF => $"显示对话   >{args[0].String}",
            0x0000 => "==================================================",
            0x0001 => "暂停执行，将 调用地址 替换为 下一条命令",
            0x0002 => $"暂停执行，将 调用地址 替换为地址 {args[0].Int:X4}。{((args[1].Int != 0) ? $"累计触发 {args[1].Int} 次后，将 调用地址 替换为 下一条命令" : "")}",
            0x0003 => $"跳转到地址 {args[0].Int:X4}，完成跳转地址的执行后脚本结束。{((args[1].Int != 0) ? $"累计触发 {args[1].Int} 次后，该指令将成为 NOP" : "")}",
            0x0004 => $"调用地址 {args[0].Int:X4}，完成调用后返回并继续执行；{((args[1].Int != 0) ? $"触发者：Event {args[1].Int}" : "")}",
            0x0005 => $"若屏幕上有对话则会先等待按下任意键，之后再更新画面，{((args[1].Int) != 0 ? "" : "不")}更新队伍步伐，更新后延迟 {args[0].Int * 60} ms",
            0x0015 => $"将 {args[2].Int} 的方向设置为 {args[0].Int}，帧号设置为 {args[1].Int}",
            0x001F => $"将道具 {args[0].Int} 添加到背包中，数量 {args[1].Int}",
            0x0020 => $"将道具 {args[0].Int} 在背包中删除，数量 {((args[1].Int != 0) ? args[1].Int : 1)}，背包中道具不足，则会从角色身上卸下；",
            0x003B => "设置对话框位置，中间",
            0x003C => "设置对话框位置，上部",
            0x003D => "设置对话框位置，下部",
            0x003E => "设置对话框位置，中间条幅",
            0x0058 => $"如果库存中的道具 {args[0].Int} 少于 {args[1].Int} 个，则跳转到 {args[2].Int:X4}",
            0x0086 => $"若所有队员中身上装备的 {args[0].Int} 数量不足 {args[1].Int}，则跳转到 {args[2].Int:X4}",
            0x008E => "还原屏幕",
            0x00A7 => "设置对话框位置，Item/Magic 描述",
            _ => $"{script.GetArgsString()}",
        };

        //
        // 打印本条指令的编译结果
        //
        ScriptLogger.WriteLine(Format(prefix, $"@{address:X8}: {script} : {typeName}"));
    }
}
