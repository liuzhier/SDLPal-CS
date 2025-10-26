using ModTools.Unpack;
using SimpleUtility;
using RWorkPath = ModTools.Record.RWorkPath;

namespace ModTools;

public static class Config
{
    public static bool IsDosGame { get; set; } = false;
    public static RWorkPath.Root WorkPath { get; set; } = null!;
    public static BinaryReader FileBase { get; set; } = null!;
    public static BinaryReader FileCore { get; set; } = null!;

    /// <summary>
    /// 初始化游戏全局数据
    /// </summary>
    /// <param name="palPath">pal 游戏目录</param>
    public static void Init(string palPath)
    {
        //
        // 初始化工作目录
        //
        InitWorkPath(palPath);

        //
        // 打开数据文件
        //
        FileBase = Util.BinaryRead(WorkPath.DataBase.Base);
        FileCore = Util.BinaryRead(WorkPath.DataBase.Core);
    }

    /// <summary>
    /// 释放全局数据
    /// </summary>
    public static void Free()
    {
        //
        // 关闭数据文件
        //
        Util.CloseBinary(FileBase);
        Util.CloseBinary(FileCore);
    }

    /// <summary>
    /// 初始化工作目录
    /// </summary>
    /// <param name="palPath">pal 游戏目录</param>
    static void InitWorkPath(string gamePath)
    {
        string GamePath(string path) => S.Paths(gamePath, path);

        int     initializationCount;

        initializationCount = 0;

        do
        {
            WorkPath = new(
                PathName: "",
                Music: new(
                    PathName: GamePath("Musics"),
                    Suffix: IsDosGame ? "mid" : "rix"
                ),
                Bitmap: new(
                    Enemy: GamePath("ABC.MKF"),
                    Item: GamePath("BALL.MKF"),
                    HeroFight: GamePath("F.MKF"),
                    FightBackPicture: GamePath("FBP.MKF"),
                    FightEffect: GamePath("FIRE.MKF"),
                    Tile: GamePath("GOP.MKF"),
                    Character: GamePath("MGO.MKF"),
                    Avatar: GamePath("RGM.MKF"),
                    Animation: GamePath("RNG.MKF")
                ),
                DataBase: new(
                    Base: GamePath("DATA.MKF"),
                    Core: GamePath("SSS.MKF"),
                    Map: GamePath("MAP.MKF"),
                    Palette: GamePath("PAT.MKF"),
                    Message: GamePath("M.MSG"),
                    EntityName: GamePath("WORD.DAT"),
                    Voice: new RWorkPath.Voice(
                        PathName: GamePath($"{(IsDosGame ? "VOC" : "Sounds")}.MKF"),
                        Suffix: IsDosGame ? "VOC" : "WAV"
                    )
                )
            );
        } while ((IsDosGame = Util.CheckVersion(WorkPath)) && ++initializationCount < 2);

        //
        // 初始化信息文件
        //
        Message.Init();
    }
}
