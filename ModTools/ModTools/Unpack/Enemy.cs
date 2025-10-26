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
using ModTools;
using Records.Mod.RGame;
using SimpleUtility;
using static Records.Pal.Data;
using static Records.Pal.Entity;
using EntityBeginId = Records.Pal.Entity.BeginId;
using RGame = Records.Mod.RGame;

namespace ModTools.Unpack;

public static unsafe class Enemy
{
    /// <summary>
    /// 解档 Enemy 实体对象。
    /// </summary>
    public static void Process()
    {
        string              pathOut;
        nint                pNative;
        CEnemy*             pBase, pThisBase;
        int                 i, end, j, id;
        RGame.Enemy         enemy;
        EnemyCommon*        pEnemy;
        string[]            indexContent;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Entity: Enemy>");

        //
        // 创建输出目录 Enemy
        //
        pathOut = Config.ModWorkPath.Game.Data.Entity.Enemy;
        COS.Dir(pathOut);

        //
        // 读取基础 Enemy 数据
        //
        (pNative, _) = Config.MkfBase.ReadChunk(1);
        pBase = (CEnemy*)pNative;

        //
        // 处理 Enemy 实体对象
        //
        end = (int)EntityBeginId.Poison;
        indexContent = new string[end - (int)EntityBeginId.Enemy];
        for (i = (int)EntityBeginId.Enemy, j = 0; i < end; i++, j++)
        {
            //
            // 获取当前 Enemy
            //
            pEnemy = (Config.IsDosGame) ? &Config.CoreDos[i].Enemy : &Config.CoreWin[i].Enemy;
            pThisBase = &pBase[pEnemy->EnemyDataId];

            //
            // 记录 Enemy 名称
            //
            indexContent[j] = Message.GetEntityName(i);

            id = j + 1;
            enemy = new RGame.Enemy(
                Name: indexContent[j],
                Health: pThisBase->Health,
                Exp: pThisBase->Exp,
                Cash: pBase->Cash,
                Level: pThisBase->Level,
                MagicId: pThisBase->MagicId,
                MagicRate: pThisBase->MagicRate,
                AttackEquivItemId: pThisBase->AttackEquivItemId,
                AttackEquivItemRate: pThisBase->AttackEquivItemRate,
                StealItemId: pThisBase->StealItemId,
                StealItemCount: pBase->StealItemCount,
                DualMove: pBase->DualMove != 0,
                CollectValue: pBase->CollectValue,
                BaseAttribute: new(
                    AttackStrength: pThisBase->Attribute.AttackStrength,
                    MagicStrength: pThisBase->Attribute.MagicStrength,
                    Defense: pThisBase->Attribute.Defense,
                    Dexterity: pThisBase->Attribute.Dexterity,
                    FleeRate: pThisBase->Attribute.FleeRate
                ),
                Resistance: new(
                    Physical: pThisBase->PhysicalResistance,
                    Poison: pThisBase->PoisonResistance,
                    Sorcery: pEnemy->ResistanceToSorcery,
                    Elemental: new(
                        Wind: pThisBase->ElementalResistance[0],
                        Thunder: pThisBase->ElementalResistance[1],
                        Water: pThisBase->ElementalResistance[2],
                        Fire: pThisBase->ElementalResistance[3],
                        Earth: pThisBase->ElementalResistance[4]
                    )
                ),
                Effect: new(
                    EffectId: pEnemy->EnemyDataId,
                    IdleFrames: pThisBase->IdleFrames,
                    MagicFrames: pThisBase->MagicFrames,
                    AttackFrames: pThisBase->AttackFrames,
                    IdleAnimSpeed: pThisBase->IdleAnimSpeed,
                    ActWaitFrames: pThisBase->ActWaitFrames,
                    YPosOffset: pThisBase->YPosOffset
                ),
                Sound: new(
                    AttackSound: pThisBase->AttackSound,
                    ActionSound: pThisBase->ActionSound,
                    MagicSound: pThisBase->MagicSound,
                    DeathSound: pThisBase->DeathSound,
                    CallSound: pThisBase->CallSound
                ),
                Script: new RGame.EnemyScript
                {
                    TurnStartTag = Config.AddAddress(
                        pEnemy->ScriptOnTurnStart,
                        $"Enemy_{id:D5}_TurnStart",
                        Address.AddrType.Enemy
                    ),
                    BattleWonTag = Config.AddAddress(
                        pEnemy->ScriptOnBattleWon,
                        $"Enemy_{id:D5}_Won",
                        Address.AddrType.Enemy
                    ),
                    ActionTag = Config.AddAddress(
                        pEnemy->ScriptOnAction,
                        $"Enemy_{id:D5}_Action",
                        Address.AddrType.Enemy
                    )
                }
            );

            //
            // 导出 JSON 文件到输出目录
            //
            S.JsonSave(enemy, $@"{pathOut}\{id:D5}.json");
        }

        //
        // 导出索引文件
        //
        S.IndexFileSave(indexContent, pathOut);

        //
        // 释放非托管内存
        //
        C.free(pNative);
    }
}
