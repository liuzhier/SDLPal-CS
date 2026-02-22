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

using SDLPal;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RFightSprite = Records.Mod.WorkPathFightSprite;
using RSprite = Records.Mod.WorkPathSprite;
using RUiSprite = Records.Mod.WorkPathUiSprite;

namespace ModTools.Unpack;

public static class UnpackMain
{
    /// <summary>
    /// 解档所有位图。
    /// </summary>
    public static async Task ProcessSprite()
    {
        RSprite             sprite;
        RFightSprite        fightSprite;
        RUiSprite           uiSprite;

        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Sprite>");

        //
        // 创建输出目录 Sprite
        //
        sprite = PalConfig.ModWorkPath.Assets.Sprite;
        COS.Dir($"{sprite}");
        COS.Dir(sprite.Animation);
        COS.Dir(sprite.Avatar);
        COS.Dir(sprite.Character);
        COS.Dir(sprite.Item);
        fightSprite = sprite.Fight;
        COS.Dir($"{fightSprite}");
        COS.Dir(fightSprite.HeroActionEffect);
        COS.Dir(fightSprite.Background);
        COS.Dir(fightSprite.Enemy);
        COS.Dir(fightSprite.Hero);
        COS.Dir(fightSprite.Magic);
        uiSprite = sprite.Ui;
        COS.Dir($"{uiSprite}");
        COS.Dir(uiSprite.Menu);
        COS.Dir(uiSprite.DialogueCursor);

        //await Task.Run(async () =>
        //{
        //    Sprite.ProcessPack(Config.PalWorkPath.Sprite.Item, Sprite.Item, isCompressedPack: false, isFrameSequence: false);
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
                    UiUtil.Log("Unpack the game data. <Sprite: Animation>");
                    Sprite.ProcessRng(PalConfig.PalWorkPath.Sprite.Animation, sprite.Animation);
                    break;

                case 1:
                    UiUtil.Log("Unpack the game data. <Sprite: Enemy>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.Sprite.Enemy, fightSprite.Enemy);
                    break;

                case 2:
                    UiUtil.Log("Unpack the game data. <Sprite: Item>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.Sprite.Item, sprite.Item, isCompressedPack: false, isFrameSequence: false);
                    break;

                case 3:
                    UiUtil.Log("Unpack the game data. <Sprite: HeroFight>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.Sprite.HeroFight, fightSprite.Hero);
                    break;

                case 4:
                    UiUtil.Log("Unpack the game data. <Sprite: FightBackPicture>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.Sprite.FightBackPicture, fightSprite.Background, isFrameSequence: false, isRlePack: false);
                    break;

                case 5:
                    UiUtil.Log("Unpack the game data. <Sprite: FightEffect>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.Sprite.FightEffect, fightSprite.Magic);
                    break;

                case 6:
                    UiUtil.Log("Unpack the game data. <Sprite: Character>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.Sprite.Character, sprite.Character);
                    break;

                case 7:
                    UiUtil.Log("Unpack the game data. <Sprite: Avatar>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.Sprite.Avatar, sprite.Avatar, isCompressedPack: false, isFrameSequence: false);
                    break;

                case 8:
                    UiUtil.Log("Unpack the game data. <Sprite: Ui.Menu>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.DataBase.Base, sprite.Ui.Menu, beginId: 9, endId: 9, isCompressedPack: false, haveSubPath: false);
                    break;

                case 9:
                    UiUtil.Log("Unpack the game data. <Sprite: HeroActionEffect>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.DataBase.Base, fightSprite.HeroActionEffect, beginId: 10, endId: 10, isCompressedPack: false, haveSubPath: false);
                    break;

                case 10:
                    UiUtil.Log("Unpack the game data. <Sprite: DialogueCursor>");
                    Sprite.ProcessPack(PalConfig.PalWorkPath.DataBase.Base, sprite.Ui.DialogueCursor, beginId: 12, endId: 12, isCompressedPack: false, haveSubPath: false);
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
        // 
        //
        COS.Dir(modPath);

        //
        // 初始化全局配置
        //
        UiUtil.Log("Initialize the global data.");
        PalConfig.Init(palPath, modPath);
        UiUtil.Log("Initialize the map data.");
        Map.Init();
        UiUtil.Log("Initialize the script data.");
        Script.Init();
        UiUtil.Log("Initialize the Sprite data.");
        Sprite.Init();

        //
        // 开始解档
        //
        Voice.Process();
        Map.Process();
        Entity.Process();
        Data.Process();
        Scene.Process();
        Script.Process();

#if !DEBUG
        //
        // 开始解包图像
        //
        var timer = Stopwatch.StartNew();
        await ProcessSprite();
        timer.Stop();
        UiUtil.Log($"Unpacking the image takes {timer.ElapsedMilliseconds / 1000},{timer.ElapsedMilliseconds % 1000},{timer.ElapsedTicks % 10000} ticks.");
#endif // false

        //
        // 释放全局数据
        //
        PalConfig.Free();
        PalMessage.Free();

        //
        // 解包完毕
        //
        UiUtil.Log("The game resources have been unpacked successfully!");
    }
}
