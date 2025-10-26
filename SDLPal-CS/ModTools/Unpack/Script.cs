using SDLPal;
using SimpleUtility;
using System;
using System.IO;
using static ModTools.Record.Core;

namespace ModTools.Unpack;

public static unsafe class Script
{
    static Dictionary<int, string> _addressDict { get; set; } = [];
    static HashSet<string[]> _functionNames = null!;

    /// <summary>
    /// 初始化脚本转换系统
    /// </summary>
    public static void Init()
    {
        string      pathOut, pathDependency;

        //
        // 创建输出目录 Scirpt
        //
        pathOut = Global.WorkPath.Game.Data.Script;
        COS.Dir(pathOut);

        //
        // 复制脚本工作目录
        //
        pathDependency = "Dependency";
        S.FileCopy($@"{pathDependency}\Script", "Library.ts", pathOut);
        S.DirCopy($@"{pathDependency}\Script\.vscode", "*.code-workspace", $@"{pathOut}\.vscode");

        //
        // 加载指令对应的函数名
        //
        S.JsonLoad(out _functionNames, $@"{pathDependency}\Func.json");

        //
        // 初始化脚本地址列表
        //
        AddAddress(0, string.Empty);
    }

    /// <summary>
    /// 若地址字典中已存在该地址则将地址名称覆盖到 addressName，否则将新记录添加到字典。
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="addressName">地址的名称</param>
    public static string AddAddress(int address, string? addressName = null)
    {
        string      oldName;

        //
        // 若地址名称为空则自动生成
        //
        addressName ??= $"@{address:X4}";

        oldName = addressName;
        if (!_addressDict.TryGetValue(address, out addressName!))
        {
            //
            // 查找失败，将新记录放入字典
            //
            addressName = oldName;
            _addressDict[address] = addressName;
        }

        return addressName;
    }

    static readonly string[] _directions = ["Direction", "Current", "West", "North", "East", "South"];

    /// <summary>
    /// 获取方向枚举字符串
    /// </summary>
    static string GetDirection(Input.Direction direction) => $"{_directions[0]}.{_directions[(int)direction + 2]}";

    static List<string> _args { get; set; }

    /// <summary>
    /// 解档 Scirpt 条目。
    /// </summary>
    public static void Process()
    {
        void Str(string text) => _args.Add(text);
        void StrTuple((ushort, ushort) texts) => _args.AddRange([texts.Item1.ToString(), texts.Item2.ToString()]);
        void Addr(CScriptArgs arg) => Str($"\"{arg.Addr}\"");
        void Num(CScriptArgs arg) => Str(arg.Short.ToString());
        void UNum(CScriptArgs arg) => Str(arg.UShort.ToString());
        void BX(CScriptArgs arg) => Str(arg.BX.ToString());
        void BY(CScriptArgs arg) => Str(arg.BY.ToString());
        void BH(CScriptArgs arg) => Str(arg.BH.ToString());
        void X(CScriptArgs arg) => Str(arg.X.ToString());
        void Y(CScriptArgs arg) => Str(arg.Y.ToString());
        void SceneEvent(CScriptArgs arg) => StrTuple(arg.SceneEvent);
        void Scene(CScriptArgs arg) => Str(arg.Scene.ToString());
        void Event(CScriptArgs arg) => Str(arg.Event.ToString());
        void Bool(CScriptArgs arg) => Str(arg.Bool ? "true" : "false");
        void Direction(CScriptArgs arg) => Str(GetDirection(arg.Direction));
        void WalkSpeed(string functionName, ushort speed) => Str($"{functionName}Speed._{speed}");

        string                  pathOut, scriptText;
        nint                    pNative;
        CScript*                pScript, pThis;
        int                     i, count, progress, end;
        CScriptArgs*            pArgs;
        string[]?               names;
        string                  name;

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Scirpt>");

        //
        // 读取 Scirpt 数据
        //
        (pNative, count) = Util.ReadMkfChunk(Config.FileCore, 4);
        pScript = (CScript*)pNative;
        count /= sizeof(CScript);

        //
        // 打开阉割语法后的 typescript 文件
        //
        pathOut = Global.WorkPath.Game.Data.Script;
        using StreamWriter file = File.CreateText($@"{pathOut}\Pal.ts");

        //
        // 处理 Scirpt
        //
        progress = (count / 10);
        end = count - 1;
        for (i = 0; i <= end; i++)
        {
            //
            // 输出处理进度
            //
            if (i % progress == 0 || i == end)
                S.Log($"Unpack the game data. <Scirpt: {((float)i / count * 100):f2}%>");

            //
            // 获取当前 Scirpt 条目
            //
            pThis = &pScript[i];
            pArgs = (CScriptArgs*)pThis->Args;

            scriptText = null;
            _args = [];
            names = _functionNames.First(x => x.Contains($"0x{pThis->Command:X4}"));
            name = names[0];
            switch (name)
            {
                case "End": // 0x0000
                    scriptText = $"\n['{AddAddress(i + 1)}']";
                    if (i == 0)
                        scriptText += ';';
                    break;

                case "ReplaceAndPause":     // 0x0001
                case "Replace":             // 0x0008
                    break;

                case "ReplaceAndPauseWithNop": // 0x0002
                    Addr(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "GotoWithNop": // 0x0003
                    Addr(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "Call": // 0x0004
                    Addr(pArgs[0]);
                    Scene(pArgs[1]);
                    Event(pArgs[1]);
                    break;

                case "VideoUpdate": // 0x0005
                    UNum(pArgs[1]);
                    Bool(pArgs[2]);
                    break;

                case "GotoWithProbability": // 0x0006
                    UNum(pArgs[0]);
                    Addr(pArgs[1]);
                    break;

                case "BattleStart": // 0x0007
                    UNum(pArgs[0]);
                    Addr(pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "WaitEventAutoScriptRun": // 0x0009
                    UNum(pArgs[0]);
                    Bool(pArgs[1]);
                    Bool(pArgs[2]);
                    break;

                case "GotoWithSelect": // 0x000A
                    Addr(pArgs[0]);
                    break;

                case "EventAnimate": // 0x000B, 0x000C, 0x000D, 0x000E, 0x0087
                    Direction(pArgs[0]);
                    break;

                case "NpcSetDirFrame": // 0x000F
                    Direction(pArgs[0]);
                    Num(pArgs[1]);
                    break;

                case "NpcMoveToBlock": // 0x0010; 0x0082
                    BX(pArgs[0]);
                    BY(pArgs[1]);
                    BH(pArgs[2]);
                    switch (pThis->Command)
                    {
                        case 0x0010:
                            WalkSpeed(name, 3);
                            break;

                        case 0x0082:
                            WalkSpeed(name, 8);
                            break;
                    }
                    break;

                case "NpcMoveToBlockMutexLock": // 0x0011; 0x007C
                    BX(pArgs[0]);
                    BY(pArgs[1]);
                    BH(pArgs[2]);
                    switch (pThis->Command)
                    {
                        case 0x0011:
                            WalkSpeed(name, 3);
                            break;

                        case 0x007C:
                            WalkSpeed(name, 8);
                            break;
                    }
                    break;

                case "EventSetPosRelToParty": // 0x0012
                    SceneEvent(pArgs[0]);
                    X(pArgs[1]);
                    Y(pArgs[2]);
                    break;

                case "Dialogue": // 0xFFFF
                    scriptText = $@"//{pArgs[0].Dialog}";
                    break;
            }

            //
            // 拼合脚本条目
            //
            scriptText ??= $"{name}({string.Join(", ", _args)});";

            //
            // 写入一行内容
            //
            file.WriteLine(scriptText);
        }

        //
        // 释放非托管内存
        //
        C.free(pNative);
    }
}
