using SimpleUtility;
using RConfig = SDLPal.Record.RConfig;
using RWorkPath = SDLPal.Record.RWorkPath;

namespace SDLPal;

public static class Global
{
    public static RConfig.Root? Config = null!;
    public static RWorkPath.Root WorkPath { get; set; } = null!;

    /// <summary>
    /// 初始化游戏全局数据
    /// </summary>
    /// <param name="modPath">mod 工作目录</param>
    public static void Init(string modPath)
    {
        //
        // 初始化全局配置
        //
        InitConfig();

        //
        // 初始化游戏路径
        //
        InitWorkPath(modPath);
    }

    /// <summary>
    /// 初始化游戏全局配置
    /// </summary>
    static void InitConfig()
    {
        string      path;

        path = "Config.json";

        if (S.FileExist(path, isAssert: false))
            //
            // 文件存在，直接读取 json 文件
            //
            S.JsonLoad(out Config, path);
        else
        {
            //
            // 文件不存在，初始化默认配置
            //
            Config = new RConfig.Root(
                Game: new RConfig.Game(
#if DEBUG && TRUE
                    LogLevel: Logger.Level.All
#else
                    LogLevel: Logger.Level.Warning
#endif // DEBUG
                ),
                Video: new RConfig.Video(
                    Width: 1280,
                    Height: 960,
                    FullScreen: false,
                    KeepAspectRatio: true
                    //ScaleMode: SDL.ScaleMode.Nearest
                ),
                Input: new RConfig.Input(
                    EnableKeyRepeat: true
                )
            );
        }
    }

    /// <summary>
    /// 初始化游戏 mod 工作目录
    /// </summary>
    /// <param name="modPath">mod 工作目录</param>
    static void InitWorkPath(string modPath)
    {
        string RootPath(string path = "") => S.Paths(modPath, path);
        string GamePath(string path = "") => S.Paths(RootPath("Game"), path);
        string MapDataPath(string path = "") => S.Paths(GamePath("MapData"), path);
        string SpiritPath(string path = "") => S.Paths(GamePath("Spirit"), path);
        string DataPath(string path = "") => S.Paths(GamePath("Data"), path);
        string EntityPath(string path = "") => S.Paths(DataPath("Entity"), path);

        string      path;

        path = "MapEditor(hack).exe";

        WorkPath = new(
            Log: "Log",
            Screenshot: "Screenshot",
            Save: "Save",
            Game: new(
                PathName: GamePath(),
                Voice: GamePath("Voice"),
                Musics: GamePath("Musics"),
                MapData: new RWorkPath.MapData(
                    PathName: MapDataPath(),
                    Map: MapDataPath("Map"),
                    Tile: MapDataPath("Tile"),
                    Palette: MapDataPath("Palette"),
                    MapEditorName: path,
                    MapEditor: MapDataPath(path)
                ),
                Spirit: new RWorkPath.Spirit(
                    PathName: SpiritPath(),
                    Character: SpiritPath("Character"),
                    Avatar: SpiritPath("Avatar"),
                    Item: SpiritPath("Item"),
                    FightBackPicture: SpiritPath("FightBackPicture"),
                    Animation: SpiritPath("Animation")
                ),
                Data: new RWorkPath.Data(
                    PathName: DataPath(),
                    Shop: DataPath("Shop"),
                    EnemyTeam: DataPath("EnemyTeam"),
                    BattleField: DataPath("BattleField"),
                    HeroActionEffect: DataPath("HeroActionEffect"),
                    Scene: DataPath("Scene"),
                    Script: DataPath("Script"),
                    Entity: new RWorkPath.Entity(
                        PathName: EntityPath(),
                        System: EntityPath("System"),
                        Hero: EntityPath("Hero"),
                        Item: EntityPath("Item"),
                        Magic: EntityPath("Magic"),
                        SummonGold: EntityPath("SummonGold"),
                        Enemy: EntityPath("Enemy"),
                        Poison: EntityPath("Poison")
                    )
                )
            )
        );
    }
}
