using Lib.Mod;
using Lib.Pal;
using Records.Mod.RGame;
using Records.Pal;
using SimpleUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModTools.Compile;

public unsafe static class Script
{
    const string _functionNameEnd = "End";
    const string _functionNameDialogue = "Dialogue";
    static ushort _address = 0;
    static List<FunctionEntry> _functionEntries { get; set; } = [];

    public static void Save()
    {
        using FileWriter coreFile = new(Config.PalWorkPath.DataBase.Core);
        coreFile.Write(new Span<byte>(Message.PalFile.Core.Script, Message.PalFile.Core.ScriptCount));
    }

    /// <summary>
    /// 读取阉割语法后的 typescript
    /// </summary>
    static void ProcessTypeScript(string filePath)
    {
        int                         lineId, charBegin, charEnd;
        ReadOnlySpan<char>          line, arguments;
        List<string>                stringArgs;
        List<ushort>                ushortArgs;
        StreamReader                reader;
        string                      addressTag, functionName;

        //
        // 打开 typescript 文件
        //
        reader = new(filePath, Encoding.UTF8);

        for (lineId = 1; ; lineId++)
        {
            //
            // 读取数据块，并等待异步处理完成
            //
            line = reader.ReadLine().AsSpan().Trim();

            //
            // 逐行读取缓冲区中的内容
            // 按情况出现频率进行先后判断
            //
            functionName = default!;
            stringArgs = [];
            ushortArgs = [];
            if (line.Contains('(') && line.EndsWith(");")
               && line.IndexOf('(') < line.IndexOf(')'))
            {
                //
                // 情况 1：函数调用
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
                foreach (Range range in arguments.Split(' '))
                    //
                    // 将每个参数都放入列表
                    //
                    stringArgs.Add(arguments[range].ToString());
            }
            else if (line.StartsWith("//"))
            {
                //
                // 情况 2：对话文本
                // 匹配 //xxxx 格式
                //

                //
                // 将 //xxxx 的 xxxx 提取为对话，并放入对话列表
                //
                Message.Dialogues.Add(line[2..].ToString());

                //
                // 空行代表脚本上下文结束
                //
                functionName = _functionNameDialogue;
                ushortArgs.Add((ushort)(Message.Dialogues.Count - 1));
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
                Config.AddNewAddress(addressTag, _address);

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
                functionName = _functionNameEnd;
            }
            else if (line.IsEmpty)
                //
                // 情况 4：内容为 null
                // 文件读取完毕，结束读取
                //
                break;
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
            _functionEntries.Add(new(
                Name: functionName,
                Args: [.. stringArgs]
            ));

            //
            // 分配下一条地址
            //
            S.Failed(
                "Script.ProcessTypeScript",
                $"The number of lines of the script exceeds the maximum limit!\n{filePath}:\nLine {lineId}: '{line}'",
                ++_address < 0xFFFF
            );
        }
    }

    /// <summary>
    /// 编译 Base Data 和 Core Data
    /// </summary>
    public static void Process()
    {
        string      rootPath, scenePath, mainPath;
        int         i, endId;

        //
        // 获取脚本根目录
        //
        rootPath = Config.ModWorkPath.Game.Data.Script;
        mainPath = $@"{rootPath}\src";
        scenePath = $@"{mainPath}\Scene";

        //
        // 初始化默认空地址
        //
        Config.AddNewAddress("", _address);

        //
        // 处理 Scene.ts
        //
        {
            //
            // 输出处理进度
            //
            Util.Log("Process Scene.ts/Event.ts.");

            const   int     beginId = 1;

            //
            // 检查有多少项 Scene.ts
            //
            endId = ModUtil.GetFileSequenceCount(scenePath, beginId, fileSuffix: ".ts");

            //
            // 输出 Scene.ts 计数
            //
            Util.Log($"Find {endId} scene typescript files. There are actually {endId = Math.Min(endId, Base.MaxEffectiveScenes)} valid ones");

            for (i = beginId; i <= endId; i++)
                //
                // 处理脚本文件
                //
                ProcessTypeScript($@"{scenePath}\{i:D5}.ts");
        }

        //
        // 处理 Public.ts
        //
        {
            //
            // 输出处理进度
            //
            Util.Log("Process Public.ts.");

            //
            // 处理脚本文件
            //
            ProcessTypeScript($@"{mainPath}\Public.ts");
        }

        //
        // 处理 Enemy.ts
        //
        {
            //
            // 输出处理进度
            //
            Util.Log("Process Enemy.ts.");

            //
            // 处理脚本文件
            //
            ProcessTypeScript($@"{mainPath}\Enemy.ts");
        }

        //
        // 处理 Hero.ts
        //
        {
            //
            // 输出处理进度
            //
            Util.Log("Process Hero.ts.");

            //
            // 处理脚本文件
            //
            ProcessTypeScript($@"{mainPath}\Hero.ts");
        }

        //
        // 处理 Poison.ts
        //
        {
            //
            // 输出处理进度
            //
            Util.Log("Process Poison.ts.");

            //
            // 处理脚本文件
            //
            ProcessTypeScript($@"{mainPath}\Poison.ts");
        }

        //
        // 处理 Item.ts
        //
        {
            //
            // 输出处理进度
            //
            Util.Log("Process Item.ts.");

            //
            // 处理脚本文件
            //
            ProcessTypeScript($@"{mainPath}\Item.ts");
        }

        //
        // 处理 Magic.ts
        //
        {
            //
            // 输出处理进度
            //
            Util.Log("Process Magic.ts.");

            //
            // 处理脚本文件
            //
            ProcessTypeScript($@"{mainPath}\Magic.ts");
        }
    }

    public static ushort AddDescriptionScript(string[]? descriptions)
    {
        ushort      address;

        if (descriptions is null)
            //
            // 无描述，返回空地址
            //
            return 0;

        //
        // 获取脚本地址
        //
        address = (ushort)(Message.Dialogues.Count);

        foreach (string description in descriptions)
        {
            //
            // 将对话放入对话列表
            //
            Message.Dialogues.Add(description);

            //
            // 添加显示对话函数
            //
            _functionEntries.Add(new(
                Name: _functionNameDialogue,
                Args: [$"{Message.Dialogues.Count - 1}"]
            ));
        }

        //
        // 添加脚本上下文结束标志
        //
        _functionEntries.Add(new(
            Name: _functionNameEnd,
            Args: []
        ));

        return address;
    }

    static Core.CScript* CScript { get; set; }
    static FunctionEntry Entry { get; set; } = null!;
    static string[] Args => Entry.Args;
    static int ArgCount => Args.Length;
    static int ArgId { get; set; }

    public static void Compile()
    {
        ushort GetValue(string textValue) => S.StrToUShort(textValue);
        void Command(int commandId = 0) => CScript->Command = GetValue(Message.GetFunctionName(Entry.Name)[commandId]);
        void Val() => CScript->Args[ArgId] = GetValue(Entry.Args[ArgId++]);
        void CheckoutArgId(ref int argId) => argId = (argId == -1) ? ArgId : argId;
        void CheckoutArgIds(ref int argId1, ref int argId2)
        {
            CheckoutArgId(ref argId1);
            CheckoutArgId(ref argId2);
        }
        ushort GetAddress(int argId) => Config.GetNewAddress(Args[argId]);
        void Addr(int srcArgId = -1, int destArgId = -1)
        {
            CheckoutArgIds(ref srcArgId, ref destArgId);
            CScript->Args[destArgId] = GetAddress(srcArgId);
            ArgId++;
        }
        void Event(int srcArgId = -1, int destArgId = -1)
        {
            CheckoutArgIds(ref srcArgId, ref destArgId);
            CScript->Args[srcArgId] = Config.GetNewEventId(GetValue(Entry.Args[ArgId++]), GetValue(Entry.Args[ArgId++]));
        }

        int                 i, count;

        //
        // 申请内存空间
        //
        Message.PalFile.Core.ScriptCount = count = _functionEntries.Count;
        Message.PalFile.Core.Script = (Core.CScript*)C.malloc(sizeof(Core.CScript) * count);

        //
        // 将“助记符”转换为“伪定长汇编码”
        //
        for (i = 0; i < count; i++)
        {
            //
            // 获取当前条目
            //
            Entry = _functionEntries[i];
            CScript = &Message.PalFile.Core.Script[i];
            ArgId = 0;

            //
            // 编译为默认命令
            //
            Command();

            switch (Entry.Name)
            {
                case "Dialogue": // 0xFFFF
                    Val();
                    break;

                case "End":                 // 0x0000
                case "ReplaceAndPause":     // 0x0001
                    break;

                case "ReplaceAndPauseWithNop":      // 0x0002
                case "GotoWithNop":                 // 0x0003
                    Addr();
                    Val();
                    break;

                case "Call": // 0x0004
                    Addr();
                    if (ArgCount > 1)
                        Event();
                    break;
            }
        }
    }
}
