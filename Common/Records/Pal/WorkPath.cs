using Lib.Pal;
using SimpleUtility;

namespace Records.Pal;

public record class WorkPath
{
    public string PathName { get; init; }
    public WorkPathMusic Music { get; init; }
    public WorkPathSpirit Spirit { get; init; }
    public WorkPathDataBase DataBase { get; init; }

    /// <summary>
    /// 初始化工作目录
    /// </summary>
    /// <param name="palPath">pal 游戏目录</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版本</param>
    public WorkPath(string gamePath, ref bool isDosGame)
    {
        string GamePath(string path) => S.Paths(gamePath, path);

        int     initializationCount;

        initializationCount = 0;

        do
        {
            PathName = "";
            Music = new(
                PathName: GamePath("Musics"),
                Suffix: isDosGame ? "mid" : "rix"
            );
            Spirit = new(
                Enemy: GamePath("ABC.MKF"),
                Item: GamePath("BALL.MKF"),
                HeroFight: GamePath("F.MKF"),
                FightBackPicture: GamePath("FBP.MKF"),
                FightEffect: GamePath("FIRE.MKF"),
                Tile: GamePath("GOP.MKF"),
                Character: GamePath("MGO.MKF"),
                Avatar: GamePath("RGM.MKF"),
                Animation: GamePath("RNG.MKF")
            );
            DataBase = new(
                Base: GamePath("DATA.MKF"),
                Core: GamePath("SSS.MKF"),
                Map: GamePath("MAP.MKF"),
                Palette: GamePath("PAT.MKF"),
                Message: GamePath("M.MSG"),
                EntityName: GamePath("WORD.DAT"),
                Voice: new WorkPathVoice(
                    PathName: GamePath($"{(isDosGame ? "VOC" : "Sounds")}.MKF"),
                    Suffix: isDosGame ? "VOC" : "WAV"
                )
            );
        } while ((isDosGame = PalUtil.CheckVersion(this)) && ++initializationCount < 2);
    }
}

public record class WorkPathMusic(
    string PathName,
    string Suffix
);

public record class WorkPathSpirit(
    string Enemy,
    string Item,
    string HeroFight,
    string FightBackPicture,
    string FightEffect,
    string Tile,
    string Character,
    string Avatar,
    string Animation
);

public record class WorkPathDataBase(
    string Base,
    string Core,
    string Map,
    string Palette,
    string Message,
    string EntityName,
    WorkPathVoice Voice
);

public record class WorkPathVoice(
    string PathName,
    string Suffix
);
