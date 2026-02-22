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

using ModTools.Util;

namespace Records.Pal;

public record class WorkPath
{
    public string PathName { get; init; }
    public WorkPathSprite Sprite { get; init; }
    public WorkPathDataBase DataBase { get; init; }

    /// <summary>
    /// 初始化工作目录
    /// </summary>
    /// <param name="gamePath">pal 游戏目录</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版本</param>
    public WorkPath(string gamePath, ref bool isDosGame)
    {
        string GamePath(string path) => S.Paths(PathName, path);

        var initializationCount = 0;
        do
        {
            PathName = gamePath;
            Sprite = new(
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
                    PathName: GamePath($"{(isDosGame ? "MUS.MKF" : "Musics")}"),
                    Suffix: isDosGame ? ["RIX"] : ["MID", "WAV", "MP3"]
                ),
                Voice: new WorkPathVoice(
                    PathName: GamePath($"{(isDosGame ? "VOC" : "SOUNDS")}.MKF"),
                    Suffix: "WAV"
                )
            );
        } while (initializationCount++ < 1 && (isDosGame = PalUtil.CheckVersion(this)));
    }
}

public record class WorkPathSprite(
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
    string[] Suffix
);

public record class WorkPathVoice(
    string PathName,
    string Suffix
);
