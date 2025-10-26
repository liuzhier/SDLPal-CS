using Lib.Mod;
using Lib.Pal;
using Records.Mod.RGame;
using SimpleUtility;
using System;
using System.IO;
using static Records.Pal.Data;

namespace ModTools.Unpack;

public static unsafe class Data
{
    /// <summary>
    /// 解档基础游戏数据。
    /// </summary>
    public static void Process()
    {
        string              pathOut;
        int                 i, j, len, val;
        BinaryReader        file;
        nint                pData;
        Span<short>         spanItems;
        string[]            indexContent;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Base Data>");

        //
        // 打开基础数据文件
        //
        file = PalUtil.BinaryRead(Config.PalWorkPath.DataBase.Base);

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
            (pData, len) = PalUtil.ReadMkfChunk(file, 0);

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
                S.JsonSave(items, $@"{pathOut}\{(i + 1):D5}.json");
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
            (pData, len) = PalUtil.ReadMkfChunk(file, 2);

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
                S.JsonSave(enemyTeam, $@"{pathOut}\{(i + 1):D5}.json");
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
            (pData, len) = PalUtil.ReadMkfChunk(file, 5);

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
                S.JsonSave(battleField, $@"{pathOut}\{(i + 1):D5}.json");
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
            (pData, len) = PalUtil.ReadMkfChunk(file, 11);

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
                S.JsonSave(heroActionEffect, $@"{pathOut}\{(i + 1):D5}.json");
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
    }
}
