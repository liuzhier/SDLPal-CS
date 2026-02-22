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
using Records.Mod.RGame;
using Records.Pal;
using SDLPal;
using System;
using static Records.Pal.Base;
using EntityType = Records.Pal.Entity.Type;

namespace ModTools.Unpack;

public static unsafe class Data
{
    /// <summary>
    /// 解档基础游戏数据。
    /// </summary>
    public static void Process()
    {
        string          pathOut;
        int             i, j, len, val;
        nint            pData;
        Span<int>       spanItems;
        string[]        indexContent;

        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Base Data>");

        //
        // 打开基础数据文件
        //
        using var mkf = new MkfReader(PalConfig.PalWorkPath.DataBase.Base);
        var assets = PalConfig.ModWorkPath.Assets;

        //
        // Shop
        //
        {
            //
            // 输出处理进度
            //
            UiUtil.Log("Unpack the game data. <Base Data: Shop>");

            //
            // 创建输出目录 Shop
            //
            pathOut = assets.Data.Shop;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(0);

            //
            // 非托管内存数据封包
            //
            var pShop = (CShop*)pData;
            var items = new int[MaxShopItem];
            spanItems = new(items);

            len /= sizeof(CShop);
            for (i = 0; i < len; i++)
            {
                spanItems.Clear();

                for (j = 0; j < items.Length; j++)
                {
                    val = int.Max(pShop[i].Items[j], 0);

                    if (val > 0)
                        items[j] = PalConfig.GetSoftEntityId(EntityType.Item, val);
                }

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(items, $@"{pathOut}\{i:D5}.json");
            }

            //
            // 释放非托管内存
            //
            C.free(pData);
        }

        //
        // EnemyTeam
        //
        {
            //
            // 输出处理进度
            //
            UiUtil.Log("Unpack the game data. <Base Data: EnemyTeam>");

            //
            // 创建输出目录 EnemyTeam
            //
            pathOut = assets.Data.EnemyTeam;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(2);

            //
            // 非托管内存数据封包
            //
            var pEnemyTeam = (CEnemyTeam*)pData;
            var enemyTeam = new int[MaxEnemysInTeam];

            len /= sizeof(CEnemyTeam);
            for (i = 0; i < len; i++)
            {
                for (j = 0; j < enemyTeam.Length; j++)
                    enemyTeam[j] = (short)PalConfig.GetSoftEntityId(EntityType.Enemy, pEnemyTeam[i].EnemyIds[j]);

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(enemyTeam, $@"{pathOut}\{i:D5}.json");
            }

            //
            // 释放非托管内存
            //
            C.free(pData);
        }

        //
        // BattleField
        //
        {
            //
            // 输出处理进度
            //
            UiUtil.Log("Unpack the game data. <Base Data: BattleField>");

            //
            // 创建输出目录 BattleField
            //
            pathOut = assets.Data.BattleField;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(5);

            //
            // 非托管内存数据封包
            //
            var pBattleField = (CBattleField*)pData;

            len /= sizeof(CBattleField);
            indexContent = new string[len];
            for (i = 0; i < len; i++)
            {
                var pThis = &pBattleField[i];

                //
                // 记录战场名称
                //
                PalMessage.TryGetEnumEntry($"Fbp{PalConfig.Version}", i, out indexContent[i]);

                var battleField = new BattleField(
                    Name: indexContent[i],
                    ScreenWave: pThis->ScreenWave,
                    ElementalEffect: new()
                    {
                        Wind = pThis->ElementalEffect[0] / 10.0f,
                        Thunder = pThis->ElementalEffect[1] / 10.0f,
                        Water = pThis->ElementalEffect[2] / 10.0f,
                        Fire = pThis->ElementalEffect[3] / 10.0f,
                        Earth = pThis->ElementalEffect[4] / 10.0f
                    }
                );

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(battleField, $@"{pathOut}\{i:D5}.json");
            }

            //
            // 导出索引文件
            //
            S.IndexFileSave(indexContent, pathOut);

            //
            // 释放非托管内存
            //
            C.free(pData);
        }

        //
        // HeroActionEffect
        //
        {
            //
            // 输出处理进度
            //
            UiUtil.Log("Unpack the game data. <Base Data: HeroActionEffect>");

            //
            // 创建输出目录 HeroActionEffect
            //
            pathOut = assets.Data.HeroActionEffect;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(11);

            //
            // 非托管内存数据封包
            //
            var pHeroActionEffect = (CHeroActionEffect*)pData;

            len /= sizeof(CHeroActionEffect);
            indexContent = new string[len];
            for (i = 0; i < len; i++)
            {
                //
                // 获取当前 Effect 对象
                //
                var pThis = &pHeroActionEffect[i];

                //
                // 记录概述名称
                //
                PalMessage.TryGetEnumEntry("HeroActionEffect", pThis->Magic + 10, out var magicActionEffect);
                PalMessage.TryGetEnumEntry("HeroActionEffect", pThis->Attack, out var attackActionEffect);
                indexContent[i] = $"{magicActionEffect}|{attackActionEffect}";

                var heroActionEffect = new HeroActionEffect()
                {
                    Name = indexContent[i],
                    Magic = pThis->Magic,
                    Attack = pThis->Attack
                };

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(heroActionEffect, $@"{pathOut}\{i:D5}.json");
            }

            //
            // 导出索引文件
            //
            S.IndexFileSave(indexContent, pathOut);

            //
            // 释放非托管内存
            //
            C.free(pData);
        }

        //
        // EnemyPosition
        //
        {
            //
            // 输出处理进度
            //
            UiUtil.Log("Unpack the game data. <Base Data: EnemyPosition>");

            //
            // 创建输出目录 EnemyPosition
            //
            pathOut = assets.Data.EnemyPosition;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(13);

            //
            // 非托管内存数据封包
            //
            var pEnemyPosition = (CEnemyPosition*)pData;

            len /= sizeof(CEnemyPosition);
            indexContent = new string[len];
            for (i = 0; i < len; i++)
            {
                //
                // 获取每个修行段对应的经验
                //
                var pThis = &pEnemyPosition[i];
                var enemyPosition = new EnemyPosition(
                    Pos5: new Pos(
                        X: pThis->Pos5.X,
                        Y: pThis->Pos5.Y
                    ),
                    Pos4: new Pos(
                        X: pThis->Pos4.X,
                        Y: pThis->Pos4.Y
                    ),
                    Pos3: new Pos(
                        X: pThis->Pos3.X,
                        Y: pThis->Pos3.Y
                    ),
                    Pos2: new Pos(
                        X: pThis->Pos2.X,
                        Y: pThis->Pos2.Y
                    ),
                    Pos1: new Pos(
                        X: pThis->Pos1.X,
                        Y: pThis->Pos1.Y
                    )
                );

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(enemyPosition, $@"{pathOut}\{i:D5}.json");
            }

            //
            // 释放非托管内存
            //
            C.free(pData);
        }

        //
        // LevelUpExp
        //
        {
            //
            // 输出处理进度
            //
            UiUtil.Log("Unpack the game data. <Base Data: LevelUpExp>");

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(14);

            //
            // 非托管内存数据封包
            //
            var pLevelUpExp = (CLevelUpExp*)pData;

            //
            // 获取每个修行段对应的经验
            //
            var levelUpExp = new LevelUpExp[Base.MaxLevel];
            for (i = 0; i < Base.MaxLevel; i++)
                levelUpExp[i] = new(
                    Level: i + 1,
                    Exp: pLevelUpExp->Exp[i]
                );

            //
            // 导出 JSON 文件到输出目录
            //
            S.JsonSave(levelUpExp, $@"{assets.Data.LevelUpExp}.json");

            //
            // 释放非托管内存
            //
            C.free(pData);
        }
    }
}
