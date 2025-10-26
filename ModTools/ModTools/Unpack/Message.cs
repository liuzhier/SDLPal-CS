using SimpleUtility;
using System.IO;
using System.Text;
using static ModTools.Record.Core;
using static ModTools.Record.Entity;

namespace ModTools.Unpack;

public static unsafe class Message
{
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

    /// <summary>
    /// 初始化全局信息数据。
    /// </summary>
    public static void Init()
    {
        string      path;

        S.Log("Init messages");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (Config.IsDosGame)
            _encoding = _encodingGbk = Encoding.GetEncoding("BIG5");
        else
            _encoding = _encodingGbk = Encoding.GetEncoding("GBK");

        EntityNames = InitEntityNames(Config.WorkPath.DataBase.EntityName);
        Dialogues = InitDialogues(Config.WorkPath.DataBase.Message);

        path = @"Dependency\Script\Message";
        Descriptions = InitDocument($@"{path}\DESC.txt");
        HeroAttackEffect = InitCodeDocument($@"{path}\HeroAttackEffectID.txt");
        CD = InitCodeDocument($@"{path}\CDID.txt");
        Music = InitCodeDocument($@"{path}\MusicID.txt");
        Palette = InitCodeDocument($@"{path}\PaletteID.txt");
        Scene = InitCodeDocument($@"{path}\SceneID{(Config.IsDosGame ? "" : "_Win")}.txt");
        Sprite = InitCodeDocument($@"{path}\SpriteID.txt");
        Background = InitCodeDocument($@"{path}\BattleFieldID{(Config.IsDosGame ? "" : "_Win")}.txt");
    }

    /// <summary>
    /// 将 Span 数组从 Big5 码转换为 GBK 码
    /// </summary>
    /// <param name="span">Span 字节数组</param>
    /// <returns>转换后的 GBK 码字节数组</returns>
    public static byte[] Big5ToGbk(Span<byte> span) => Encoding.Convert(_encoding, _encodingGbk, span.ToArray());

    /// <summary>
    /// 读取实体对象名称。
    /// </summary>
    /// <param name="name">文件路径</param>
    /// <returns>名称列表</returns>
    public static List<string> InitEntityNames(string name)
    {
        BinaryReader        file;
        Span<byte>          span;
        List<string>        entityNames;
        string              str;

        //
        // 读取文字文件
        //
        file = Util.BinaryRead(name);
        span = new(new byte[10]);
        entityNames = [];

        //
        // 读取文字
        //
        while (file.Read(span) == 10)
        {
            //
            // 若为 DOS 版则转换编码为 GBK
            //
            if (Config.IsDosGame)
                str = _encodingGbk.GetString(Big5ToGbk(span)).TrimEnd();
            else
                str = _encodingGbk.GetString(span).TrimEnd();

            entityNames.Add(str);
            span.Clear();
        }

        return entityNames;
    }

    /// <summary>
    /// 读取对话文件内容。
    /// </summary>
    /// <param name="name">文件路径</param>
    /// <returns>对话列表</returns>
    public static List<string> InitDialogues(string name)
    {
        BinaryReader        msg, core;
        int                 unitSize, count, i;
        List<string>        dialogues;
        string              str;
        Span<byte>          span;

        //
        // 读取对话文件
        //
        msg = Util.BinaryRead(name);
        core = Util.BinaryRead(Config.WorkPath.DataBase.Core);
        unitSize = (int)Util.UnitSize.DialogIndex;
        count = Util.GetMkfChunkSize(core, 3) / unitSize - 1;
        Util.SeekMkfChunk(core, 3);
        dialogues = [];

        //
        // 分割对话文字
        //
        i = 0;
        while ((span = msg.ReadBytes(-(core.ReadInt32() - core.ReadInt32()))).Length != 0)
        {
            //
            // 若为 DOS 版则转换编码为 GBK
            //
            if (Config.IsDosGame)
                str = _encodingGbk.GetString(Big5ToGbk(span));
            else
                str = _encodingGbk.GetString(span);

            dialogues.Add(_encoding.GetString(span).TrimEnd());
            core.BaseStream.Seek(-unitSize, SeekOrigin.Current);

            //
            // 检查是否读取完毕
            //
            if (++i == count)
                break;
        }

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

        return dict;
    }

    /// <summary>
    /// 获取 Item 或 Magic 的描述信息
    /// </summary>
    /// <param name="entityId">对应的 Entity 编号</param>
    /// <param name="isItemOrMagic">true 为 Item，false 为 Magic</param>
    /// <returns>描述信息</returns>
    public static string[]? GetDescriptions(int entityId, bool isItemOrMagic)
    {
        List<string>        descriptions;
        int                 addr;
        nint                pNative;
        CScript*            pScript, pThis;

        //
        // 整理描述信息，区分游戏资源版本
        //
        if (Config.IsDosGame)
        {
            Descriptions.TryGetValue(entityId, out var value);
            return value;
        }

        //
        // 获取 Win 版描述脚本的地址
        //
        addr = (isItemOrMagic) ?
            Entity.CoreWin[entityId].Item.ScriptDesc :
            Entity.CoreWin[entityId].Magic.ScriptDesc;

        //
        // 读取 Scirpt 数据
        //
        (pNative, _) = Util.ReadMkfChunk(Config.FileCore, 4);
        pScript = (CScript*)pNative;

        //
        // 开始整理
        //
        descriptions = [];
        while ((pThis = &pScript[addr++])->Command != 0x0000)
            if (pThis->Command == 0xFFFF)
                descriptions.Add(Dialogues[pThis->Args[0]]);

        return [.. descriptions];
    }
}
