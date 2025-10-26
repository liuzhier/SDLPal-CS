using Lib.Pal;
using SimpleUtility;

namespace Records.Pal;

public record class WorkPath
{
    public string PathName { get; init; }
    public WorkPathSpirit Spirit { get; init; }
    public WorkPathDataBase DataBase { get; init; }

    /// <summary>
    /// 初始化工作目录
    /// </summary>
    /// <param name="gamePath">pal 游戏目录</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版本</param>
    public WorkPath(string gamePath, ref bool? isDosGame)
    {
        string GamePath(string path) => S.Paths(PathName, path);

        bool        isDosVersion;
        int         initializationCount;

        initializationCount = 0;

        do
        {
            isDosVersion = (isDosGame == null) || (bool)isDosGame;

            PathName = gamePath;
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
                EntityName: GamePath("WORD.DAT"),
                Map: GamePath("MAP.MKF"),
                Message: GamePath("M.MSG"),
                Palette: GamePath("PAT.MKF"),
                Music: new WorkPathMusic(
                    PathName: GamePath($"{(isDosVersion ? "MUS.MKF" : "Musics")}"),
                    Suffix: isDosVersion ? "RIX" : "MID"
                ),
                Voice: new WorkPathVoice(
                    PathName: GamePath($"{(isDosVersion ? "VOC" : "Sounds")}.MKF"),
                    Suffix: isDosVersion ? "VOC" : "WAV"
                )
            );
        } while ((isDosGame == null) && !(bool)(isDosGame = PalUtil.CheckVersion(this)) && ++initializationCount < 2);
    }
}

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
    string EntityName,
    string Map,
    string Message,
    string Palette,
    WorkPathMusic Music,
    WorkPathVoice Voice
);

public record class WorkPathMusic(
    string PathName,
    string Suffix
);

public record class WorkPathVoice(
    string PathName,
    string Suffix
);
