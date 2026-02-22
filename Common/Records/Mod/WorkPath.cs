#region License
/*
 * Copyright (c) 2025, liuzhier <lichunxiao_lcx@qq.com>.
 * 
 * This file is part of SDLPAL-CS.
 * 
 * SDLPAL-CS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 3
 * as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */
#endregion License

namespace Records.Mod;

public record class WorkPath
{
    public string PathName { get; init; } = @".\";
    public string Setup { get; init; } = $"SDLPal-CS.json";
    public string Temp { get; init; } = "Temp";
    public string Log { get; init; } = "Log";
    public string Screenshot { get; init; } = "Screenshot";
    public string Save { get; init; } = "Save";
    public WorkPathAssets Assets { get; init; }

    /// <summary>
    /// 初始化游戏 mod 工作目录
    /// </summary>
    /// <param name="modPath">mod 工作目录</param>
    public WorkPath(string modPath, bool isTempCompile = false)
    {
        string      spritePath = isTempCompile ? "SpritePack" : "Sprite";

        string RootPath(string path = "") => S.Paths(modPath, path);
        string LogPath(string path = "") => S.Paths(RootPath("Log"), path);
        string AssetsPath(string path = "") => S.Paths(RootPath("Assets"), path);
        string FontPath(string path = "") => S.Paths(AssetsPath("Font"), path);
        string MapDataPath(string path = "") => S.Paths(AssetsPath("MapData"), path);
        string SpritePath(string path = "") => S.Paths(AssetsPath(spritePath), path);
        string FightSpritePath(string path = "") => S.Paths(SpritePath("Fight"), path);
        string UiSpritePath(string path = "") => S.Paths(SpritePath("Ui"), path);
        string DataPath(string path = "") => S.Paths(AssetsPath("Data"), path);
        string EntityPath(string path = "") => S.Paths(DataPath("Entity"), path);

        var nameMapEditor = "MapEditor(hack).exe";

        PathName = RootPath();
        Setup = RootPath(Setup);
        Temp = RootPath(Temp);
        Log = LogPath(S.GetCurrDate());
        Screenshot = RootPath(Screenshot);
        Save = RootPath(Save);
        Assets = new(AssetsPath())
        {
            Music = AssetsPath("Musics"),
            Palette = new(AssetsPath("Palette"))
            {
                Suffix = "ACT"
            },
            Voice = AssetsPath("Voice"),
            Movie = AssetsPath("Movie"),
            Font = new(FontPath()),
            MapData = new(MapDataPath())
            {
                Map = MapDataPath("Map"),
                Tile = MapDataPath("Tile"),
                Palette = MapDataPath("Palette"),
                MapEditorName = nameMapEditor,
                MapEditor = MapDataPath(nameMapEditor)
            },
            Sprite = new(SpritePath())
            {
                Animation = SpritePath("Animation"),
                Avatar = SpritePath("Avatar"),
                Character = SpritePath("Character"),
                Item = SpritePath("Item"),
                Ui = new(UiSpritePath())
                {
                    Menu = UiSpritePath("Menu"),
                    DialogueCursor = UiSpritePath("DialogueCursor")
                },
                Fight = new(FightSpritePath())
                {
                    HeroActionEffect = FightSpritePath("HeroActionEffect"),
                    Background = FightSpritePath("Background"),
                    Enemy = FightSpritePath("Enemy"),
                    Hero = FightSpritePath("Hero"),
                    Magic = FightSpritePath("Magic")
                },
            },
            Data = new(DataPath())
            {
                Shop = DataPath("Shop"),
                EnemyTeam = DataPath("EnemyTeam"),
                BattleField = DataPath("BattleField"),
                HeroActionEffect = DataPath("HeroActionEffect"),
                LevelUpExp = DataPath("LevelUpExp"),
                Scene = DataPath("Scene"),
                Script = DataPath("Script"),
                Entity = new(EntityPath())
                {
                    System = EntityPath("System"),
                    Hero = EntityPath("Hero"),
                    Item = EntityPath("Item"),
                    Magic = EntityPath("Magic"),
                    SummonGold = EntityPath("SummonGold"),
                    Enemy = EntityPath("Enemy"),
                    Poison = EntityPath("Poison")
                },
            },
        };
    }

    public override string ToString() => PathName;
}

public class WorkPathAssets(string path)
{
    string PathName { get; set; } = path;
    public string Music { get; set; } = null!;
    public string Voice { get; set; } = null!;
    public string Movie { get; set; } = null!;
    public WorkPathFont Font { get; set; } = null!;
    public WorkPathMapData MapData { get; set; } = null!;
    public WorkPathPalette Palette { get; set; } = null!;
    public WorkPathSprite Sprite { get; set; } = null!;
    public WorkPathData Data { get; set; } = null!;

    public override string ToString() => PathName;
}

public class WorkPathFont(string path)
{
    string PathName { get; set; } = path;

    public override string ToString() => PathName;
}

public class WorkPathMapData(string path)
{
    string PathName { get; set; } = path;
    public string Map { get; set; } = null!;
    public string Tile { get; set; } = null!;
    public string Palette { get; set; } = null!;
    public string MapEditorName { get; set; } = null!;
    public string MapEditor { get; set; } = null!;

    public override string ToString() => PathName;
}

public class WorkPathPalette(string path)
{
    string PathName { get; set; } = path;
    public string Suffix { get; set; } = null!;

    public override string ToString() => PathName;
}

public class WorkPathSprite(string path)
{
    string PathName { get; set; } = path;
    public string Animation { get; set; } = null!;
    public string Avatar { get; set; } = null!;
    public string Character { get; set; } = null!;
    public string Item { get; set; } = null!;
    public WorkPathFightSprite Fight { get; set; } = null!;
    public WorkPathUiSprite Ui { get; set; } = null!;

    public override string ToString() => PathName;
}

public class WorkPathFightSprite(string path)
{
    string PathName { get; set; } = path;
    public string HeroActionEffect { get; set; } = null!;
    public string Background { get; set; } = null!;
    public string Enemy { get; set; } = null!;
    public string Hero { get; set; } = null!;
    public string Magic { get; set; } = null!;

    public override string ToString() => PathName;
}

public class WorkPathUiSprite(string path)
{
    string PathName { get; set; } = path;
    public string Menu { get; set; } = null!;
    public string DialogueCursor { get; set; } = null!;

    public override string ToString() => PathName;
}

public class WorkPathData(string path)
{
    string PathName { get; set; } = path;
    public string Shop { get; set; } = null!;
    public string EnemyTeam { get; set; } = null!;
    public string BattleField { get; set; } = null!;
    public string HeroActionEffect { get; set; } = null!;
    public string LevelUpExp { get; set; } = null!;
    public string Scene { get; set; } = null!;
    public string Script { get; set; } = null!;
    public WorkPathEntity Entity { get; set; } = null!;

    public override string ToString() => PathName;
}

public class WorkPathEntity(string path)
{
    string PathName { get; set; } = path;
    public string System { get; set; } = null!;
    public string Hero { get; set; } = null!;
    public string Item { get; set; } = null!;
    public string Magic { get; set; } = null!;
    public string SummonGold { get; set; } = null!;
    public string Enemy { get; set; } = null!;
    public string Poison { get; set; } = null!;

    public override string ToString() => PathName;
}
