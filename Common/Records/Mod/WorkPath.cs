using SimpleUtility;

namespace Records.Mod;

public record class WorkPath
{
    public string Log { get; init; } = "Log";

    public string Screenshot { get; init; } = "Screenshot";

    public string Save { get; init; } = "Save";

    public WorkPathGame Game { get; init; }

    /// <summary>
    /// 初始化游戏 mod 工作目录
    /// </summary>
    /// <param name="modPath">mod 工作目录</param>
    public WorkPath(string modPath)
    {
        string RootPath(string path = "") => S.Paths(modPath, path);
        string GamePath(string path = "") => S.Paths(RootPath("Game"), path);
        string MapDataPath(string path = "") => S.Paths(GamePath("MapData"), path);
        string SpiritPath(string path = "") => S.Paths(GamePath("Spirit"), path);
        string FightSpiritPath(string path = "") => S.Paths(SpiritPath("Fight"), path);
        string UiSpiritPath(string path = "") => S.Paths(SpiritPath("Ui"), path);
        string DataPath(string path = "") => S.Paths(GamePath("Data"), path);
        string EntityPath(string path = "") => S.Paths(DataPath("Entity"), path);

        string      path;

        path = "MapEditor(hack).exe";

        Game = new(
            PathName: GamePath(),
            Musics: GamePath("Musics"),
            Palette: GamePath("Palette"),
            Voice: GamePath("Voice"),
            MapData: new WorkPathMapData(
                PathName: MapDataPath(),
                Map: MapDataPath("Map"),
                Tile: MapDataPath("Tile"),
                Palette: MapDataPath("Palette"),
                MapEditorName: path,
                MapEditor: MapDataPath(path)
            ),
            Spirit: new WorkPathSpirit(
                PathName: SpiritPath(),
                Animation: SpiritPath("Animation"),
                Avatar: SpiritPath("Avatar"),
                Character: SpiritPath("Character"),
                Item: SpiritPath("Item"),
                Ui: new WorkPathUiSpirit(
                    PathName: UiSpiritPath(),
                    Menu: UiSpiritPath("Menu"),
                    DialogueCursor: UiSpiritPath("DialogueCursor")
                ),
                Fight: new WorkPathFightSpirit(
                    PathName: FightSpiritPath(),
                    HeroActionEffect: FightSpiritPath("HeroActionEffect"),
                    Background: FightSpiritPath("Background"),
                    Enemy: FightSpiritPath("Enemy"),
                    Hero: FightSpiritPath("Hero"),
                    Magic: FightSpiritPath("Magic")
                )
            ),
            Data: new WorkPathData(
                PathName: DataPath(),
                Shop: DataPath("Shop"),
                EnemyTeam: DataPath("EnemyTeam"),
                BattleField: DataPath("BattleField"),
                HeroActionEffect: DataPath("HeroActionEffect"),
                Scene: DataPath("Scene"),
                Script: DataPath("Script"),
                Entity: new WorkPathEntity(
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
        );
    }
}

public record class WorkPathGame(
    string PathName,
    string Musics,
    string Palette,
    string Voice,
    WorkPathMapData MapData,
    WorkPathSpirit Spirit,
    WorkPathData Data
);

public record class WorkPathMapData(
    string PathName,
    string Map,
    string Tile,
    string Palette,
    string MapEditorName,
    string MapEditor
);

public record class WorkPathSpirit(
    string PathName,
    string Animation,
    string Avatar,
    string Character,
    string Item,
    WorkPathFightSpirit Fight,
    WorkPathUiSpirit Ui
);

public record class WorkPathFightSpirit(
    string PathName,
    string HeroActionEffect,
    string Background,
    string Enemy,
    string Hero,
    string Magic
);

public record class WorkPathUiSpirit(
    string PathName,
    string Menu,
    string DialogueCursor
);

public record class WorkPathData(
    string PathName,
    string Shop,
    string EnemyTeam,
    string BattleField,
    string HeroActionEffect,
    string Scene,
    string Script,
    WorkPathEntity Entity
);

public record class WorkPathEntity(
    string PathName,
    string System,
    string Hero,
    string Item,
    string Magic,
    string SummonGold,
    string Enemy,
    string Poison
);
