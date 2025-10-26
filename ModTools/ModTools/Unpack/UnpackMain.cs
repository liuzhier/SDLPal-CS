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

using Lib.Mod;
using SimpleUtility;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RFightSpirit = Records.Mod.WorkPathFightSpirit;
using RSpirit = Records.Mod.WorkPathSpirit;
using RUiSpirit = Records.Mod.WorkPathUiSpirit;

namespace ModTools.Unpack;

public static class UnpackMain
{
    /// <summary>
    /// 解档所有位图。
    /// </summary>
    public static async Task ProcessSprite()
    {
        RSpirit             spirit;
        RFightSpirit        fightSpirit;
        RUiSpirit           uiSpirit;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Spirit>");

        //
        // 创建输出目录 Spirit
        //
        spirit = Config.ModWorkPath.Game.Spirit;
        COS.Dir(spirit.PathName);
        COS.Dir(spirit.Animation);
        COS.Dir(spirit.Avatar);
        COS.Dir(spirit.Character);
        COS.Dir(spirit.Item);
        fightSpirit = spirit.Fight;
        COS.Dir(fightSpirit.PathName);
        COS.Dir(fightSpirit.HeroActionEffect);
        COS.Dir(fightSpirit.Background);
        COS.Dir(fightSpirit.Enemy);
        COS.Dir(fightSpirit.Hero);
        COS.Dir(fightSpirit.Magic);
        uiSpirit = spirit.Ui;
        COS.Dir(uiSpirit.PathName);
        COS.Dir(uiSpirit.Menu);
        COS.Dir(uiSpirit.DialogueCursor);

        //await Task.Run(async () =>
        //{
        //    Spirit.ProcessPack(Config.PalWorkPath.Spirit.Item, spirit.Item, isCompressedPack: false, isFrameSequence: false);
        //});
        //return;

        //
        // 自动管理线程并发
        //
        Parallel.For(0, 11, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount // 限制并发数
        }, i =>
        {
            switch (i)
            {
                case 0:
                    //
                    // 导出动画图像
                    //
                    Util.Log("Unpack the game data. <Spirit: Animation>");
                    Spirit.ProcessRng(Config.PalWorkPath.Spirit.Animation, spirit.Animation);
                    break;

                case 1:
                    Util.Log("Unpack the game data. <Spirit: Enemy>");
                    Spirit.ProcessPack(Config.PalWorkPath.Spirit.Enemy, fightSpirit.Enemy);
                    break;

                case 2:
                    Util.Log("Unpack the game data. <Spirit: Item>");
                    Spirit.ProcessPack(Config.PalWorkPath.Spirit.Item, spirit.Item, isCompressedPack: false, isFrameSequence: false);
                    break;

                case 3:
                    Util.Log("Unpack the game data. <Spirit: HeroFight>");
                    Spirit.ProcessPack(Config.PalWorkPath.Spirit.HeroFight, fightSpirit.Hero);
                    break;

                case 4:
                    Util.Log("Unpack the game data. <Spirit: FightBackPicture>");
                    Spirit.ProcessPack(Config.PalWorkPath.Spirit.FightBackPicture, fightSpirit.Background, isFrameSequence: false, isRlePack: false);
                    break;

                case 5:
                    Util.Log("Unpack the game data. <Spirit: FightEffect>");
                    Spirit.ProcessPack(Config.PalWorkPath.Spirit.FightEffect, fightSpirit.Magic);
                    break;

                case 6:
                    Util.Log("Unpack the game data. <Spirit: Character>");
                    Spirit.ProcessPack(Config.PalWorkPath.Spirit.Character, spirit.Character);
                    break;

                case 7:
                    Util.Log("Unpack the game data. <Spirit: Avatar>");
                    Spirit.ProcessPack(Config.PalWorkPath.Spirit.Avatar, spirit.Avatar, isCompressedPack: false, isFrameSequence: false);
                    break;

                case 8:
                    Util.Log("Unpack the game data. <Spirit: Ui.Menu>");
                    Spirit.ProcessPack(Config.PalWorkPath.DataBase.Base, spirit.Ui.Menu, beginId: 9, endId: 9, isCompressedPack: false, haveSubPath: false);
                    break;

                case 9:
                    Util.Log("Unpack the game data. <Spirit: HeroActionEffect>");
                    Spirit.ProcessPack(Config.PalWorkPath.DataBase.Base, fightSpirit.HeroActionEffect, beginId: 10, endId: 10, isCompressedPack: false, haveSubPath: false);
                    break;

                case 10:
                    Util.Log("Unpack the game data. <Spirit: DialogueCursor>");
                    Spirit.ProcessPack(Config.PalWorkPath.DataBase.Base, spirit.Ui.DialogueCursor, beginId: 12, endId: 12, isCompressedPack: false, haveSubPath: false);
                    break;

                default:
                    break;
            }
        });
    }

    /// <summary>
    /// 解包游戏资源
    /// </summary>
    /// <param name="palPath">游戏资源目录</param>
    /// <param name="modPath">MOD 输出目录</param>
    public static async Task Process(string palPath, string modPath)
    {
        //
        // 初始化全局配置
        //
        Util.Log("Initialize the global data.");
        Config.Init(palPath, modPath);
        Util.Log("Initialize the map data.");
        Map.Init();
        Util.Log("Initialize the script data.");
        Script.Init();
        Util.Log("Initialize the Sprite data.");
        Spirit.Init();

        //
        // 开始解档
        //
        Voice.Process();
        Map.Process();
        Data.Process();
        Entity.Process();
        Scene.Process();
        Script.Process();

        //
        // 开始解包图像
        //
        var timer = Stopwatch.StartNew();
        await ProcessSprite();
        timer.Stop();
        Util.Log($"Unpacking the image takes {timer.ElapsedMilliseconds / 1000},{timer.ElapsedMilliseconds % 1000},{timer.ElapsedTicks % 10000} ticks.");

        //
        // 释放全局数据
        //
        Config.Free();
        Message.Free();

        //
        // 解包完毕
        //
        Util.Log("The game resources have been unpacked successfully!");
    }
}
