using SDLPal;
using SimpleUtility;
using static ModTools.Record.Core;

namespace ModTools.Unpack;

public static unsafe class Script
{
    public  const   string      Placeholder = "ADDR_NAME";
    public static Dictionary<int, string> AddressDict { get; set; } = [];

    public static void Init() => AddAddrress(0, string.Empty);

    /// <summary>
    /// 若地址字典中已存在该地址则将地址名称覆盖到 addressName，否则将新记录添加到字典。
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="addressName">地址的名称</param>
    public static string AddAddrress(int address, string? addressName = null)
    {
        string      oldName;

        //
        // 若地址名称为空则自动生成
        //
        addressName ??= $"0x{address:X4}";

        oldName = addressName;
        if (!AddressDict.TryGetValue(address, out addressName!))
        {
            //
            // 查找失败，将新记录放入字典
            //
            addressName = oldName;
            AddressDict[address] = addressName;
        }

        return addressName;
    }

    /// <summary>
    /// 解档 Scirpt 条目。
    /// </summary>
    public static void Process()
    {
        static string Addr(int address) => AddAddrress(address);

        string              pathOut, scriptText;
        nint                pNative;
        CScript*            pScript, pThis;
        int                 i, count, progress, end;
        CScriptArgs*        pArgs;
        string              name;
        List<string>        args;

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Scirpt>");

        //
        // 创建输出目录 Scirpt
        //
        pathOut = Global.WorkPath.Game.Data.Script;
        COS.Dir(pathOut);

        //
        // 读取 Scirpt 数据
        //
        (pNative, count) = Util.ReadMkfChunk(Config.FileCore, 4);
        pScript = (CScript*)pNative;
        count /= sizeof(CScript);

        //
        // 打开阉割语法后的 typescript 文件
        //
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

            name = scriptText = string.Empty;
            args = [];
            switch (pThis->Command)
            {
                case 0x0000:
                    scriptText = $"\n['{Addr(i + 1)}']";
                    break;

                case 0xFFFF:
                    scriptText = $@"//{pArgs[0].Dialog}";
                    break;
            }

            //
            // 拼合脚本条目
            //
            scriptText ??= $"{name}({string.Join(", ", args)});";

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
