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
using Lib.Pal;
using Records.Mod.RGame;
using SimpleUtility;
using System;
using static Records.Pal.Base;

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
        MkfReader       mkf;
        nint            pData;
        Span<short>     spanItems;
        string[]        indexContent;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Base Data>");

        //
        // 打开基础数据文件
        //
        mkf = new(Config.PalWorkPath.DataBase.Base);

        //
        // Shop
        //
        {
            CShop*      pShop;
            short[]     items;

            //
            // 输出处理进度
            //
            Util.Log("Unpack the game data. <Base Data: Shop>");

            //
            // 创建输出目录 Shop
            //
            pathOut = Config.ModWorkPath.Game.Data.Shop;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(0);

            //
            // 非托管内存数据封包
            //
            pShop = (CShop*)pData;
            items = new short[MaxShopItem];
            spanItems = new(items);

            len /= sizeof(CShop);
            for (i = 0; i < len; i++)
            {
                spanItems.Clear();

                for (j = 0; j < items.Length; j++)
                {
                    val = short.Max(pShop[i].Items[j], 0);

                    if (val > 0)
                        items[j] = (short)(val - (short)Records.Pal.Entity.BeginId.Item + 1);
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
            CEnemyTeam*     pEnemyTeam;
            short[]         enemyTeam;

            //
            // 输出处理进度
            //
            Util.Log("Unpack the game data. <Base Data: EnemyTeam>");

            //
            // 创建输出目录 EnemyTeam
            //
            pathOut = Config.ModWorkPath.Game.Data.EnemyTeam;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(2);

            //
            // 非托管内存数据封包
            //
            pEnemyTeam = (CEnemyTeam*)pData;
            enemyTeam = new short[MaxEnemysInTeam];

            len /= sizeof(CEnemyTeam);
            for (i = 0; i < len; i++)
            {
                for (j = 0; j < enemyTeam.Length; j++)
                    enemyTeam[j] = (short)(pEnemyTeam[i].EnemyIds[j] - (short)Records.Pal.Entity.BeginId.Enemy + 1);

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
            CBattleField*       pBattleField, pThis;
            BattleField         battleField;

            //
            // 输出处理进度
            //
            Util.Log("Unpack the game data. <Base Data: BattleField>");

            //
            // 创建输出目录 BattleField
            //
            pathOut = Config.ModWorkPath.Game.Data.BattleField;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(5);

            //
            // 非托管内存数据封包
            //
            pBattleField = (CBattleField*)pData;

            len /= sizeof(CBattleField);
            indexContent = new string[len];
            for (i = 0; i < len; i++)
            {
                pThis = &pBattleField[i];

                //
                // 记录战场名称
                //
                if (Message.Background.TryGetValue(i, out _))
                    indexContent[i] = Message.Background[i];

                battleField = new(
                    Name: indexContent[i],
                    ScreenWave: pThis->ScreenWave,
                    ElementalEffect: new(
                        Wind: pThis->ElementalEffect[0],
                        Thunder: pThis->ElementalEffect[1],
                        Water: pThis->ElementalEffect[2],
                        Fire: pThis->ElementalEffect[3],
                        Earth: pThis->ElementalEffect[4]
                    )
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
            CHeroActionEffect*      pHeroActionEffect, pThis;
            HeroActionEffect        heroActionEffect;

            //
            // 输出处理进度
            //
            Util.Log("Unpack the game data. <Base Data: HeroActionEffect>");

            //
            // 创建输出目录 HeroActionEffect
            //
            pathOut = Config.ModWorkPath.Game.Data.HeroActionEffect;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(11);

            //
            // 非托管内存数据封包
            //
            pHeroActionEffect = (CHeroActionEffect*)pData;

            len /= sizeof(CHeroActionEffect);
            indexContent = new string[len];
            for (i = 0; i < len; i++)
            {
                //
                // 获取当前 Effect 对象
                //
                pThis = &pHeroActionEffect[i];

                //
                // 记录概述名称
                //
                indexContent[i] = $"{Message.HeroAttackEffect[pThis->Magic + 10]}|{Message.HeroAttackEffect[pThis->Attack]}";

                heroActionEffect = new HeroActionEffect(
                    Name: indexContent[i],
                    Magic: pThis->Magic,
                    Attack: pThis->Attack
                );

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
            CEnemyPosition*     pEnemyPosition, pThis;
            EnemyPosition       enemyPosition;

            //
            // 输出处理进度
            //
            Util.Log("Unpack the game data. <Base Data: EnemyPosition>");

            //
            // 创建输出目录 EnemyPosition
            //
            pathOut = Config.ModWorkPath.Game.Data.EnemyPosition;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(13);

            //
            // 非托管内存数据封包
            //
            pEnemyPosition = (CEnemyPosition*)pData;

            len /= sizeof(CEnemyPosition);
            indexContent = new string[len];
            for (i = 0; i < len; i++)
            {
                //
                // 获取每个修行段对应的经验
                //
                pThis = &pEnemyPosition[i];
                enemyPosition = new EnemyPosition(
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
            CLevelUpExp*        pLevelUpExp;
            LevelUpExp          levelUpExp;

            //
            // 输出处理进度
            //
            Util.Log("Unpack the game data. <Base Data: LevelUpExp>");

            //
            // 创建输出目录 LevelUpExp
            //
            pathOut = Config.ModWorkPath.Game.Data.LevelUpExp;
            COS.Dir(pathOut);

            //
            // 将数据读入非托管内存
            //
            (pData, len) = mkf.ReadChunk(14);

            //
            // 非托管内存数据封包
            //
            pLevelUpExp = (CLevelUpExp*)pData;

            len /= sizeof(CLevelUpExp);
            indexContent = new string[len];
            for (i = 0; i < len; i++)
            {
                //
                // 获取每个修行段对应的经验
                //
                levelUpExp = new LevelUpExp(
                    Level: (byte)i,
                    Exp: pLevelUpExp->Exp[i]
                );

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(levelUpExp, $@"{pathOut}\{i:D5}.json");
            }

            //
            // 释放非托管内存
            //
            C.free(pData);
        }
    }
}
