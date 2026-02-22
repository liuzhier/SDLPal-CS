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
using static Records.Pal.Base;
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
        int                 i, end, id;
        RGame.Enemy         enemy;
        EnemyCommon*        pEnemy;
        string[]            indexContent;

        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Entity: Enemy>");

        //
        // 创建输出目录 Enemy
        //
        pathOut = PalConfig.ModWorkPath.Assets.Data.Entity.Enemy;
        COS.Dir(pathOut);

        //
        // 读取基础 Enemy 数据
        //
        (pNative, _) = PalConfig.MkfBase.ReadChunk(1);
        pBase = (CEnemy*)pNative;

        //
        // 处理 Enemy 实体对象
        //
        end = (int)EntityBeginId.Poison;
        indexContent = new string[end - (int)EntityBeginId.Enemy + 1];
        for (i = (int)EntityBeginId.Enemy, id = 1; i < end; i++, id++)
        {
            //
            // 获取当前 Enemy
            //
            pEnemy = (PalConfig.IsDosGame) ? &PalConfig.CoreDos[i].Enemy : &PalConfig.CoreWin[i].Enemy;
            pThisBase = &pBase[pEnemy->EnemyDataId];

            //
            // 记录 Enemy 名称
            //
            indexContent[id] = PalMessage.GetEntityName(i);

            enemy = new()
            {
                Name = indexContent[id],
                Health = pThisBase->Health,
                Exp = pThisBase->Exp,
                Cash = pBase->Cash,
                Level = pThisBase->Level,
                MagicId = PalConfig.GetSoftMagicId(pThisBase->MagicId),
                MagicRate = pThisBase->MagicRate / 10.0f,
                AttackEquivItemId = PalConfig.GetSoftEntityId(Type.Item, pThisBase->AttackEquivItemId),
                AttackEquivItemRate = pThisBase->AttackEquivItemRate / 10.0f,
                StealItemId = PalConfig.GetSoftEntityId(Type.Item, pThisBase->StealItemId),
                StealItemCount = pBase->StealItemCount,
                MomovableCount = pBase->DualMove,
                CollectValue = pBase->CollectValue,
                BaseAttribute = new()
                {
                    AttackStrength = pThisBase->Attribute.AttackStrength,
                    MagicStrength = pThisBase->Attribute.MagicStrength,
                    Defense = pThisBase->Attribute.Defense,
                    Dexterity = pThisBase->Attribute.Dexterity,
                    FleeRate = pThisBase->Attribute.FleeRate
                },
                Resistance = new()
                {
                    Physical = pThisBase->PhysicalResistance / 10.0f,
                    Poison = pThisBase->PoisonResistance / 10.0f,
                    Sorcery = pEnemy->ResistanceToSorcery / 10.0f,
                    Elemental = new()
                    {
                        Wind = pThisBase->ElementalResistance[0] / 10.0f,
                        Thunder = pThisBase->ElementalResistance[1] / 10.0f,
                        Water = pThisBase->ElementalResistance[2] / 10.0f,
                        Fire = pThisBase->ElementalResistance[3] / 10.0f,
                        Earth = pThisBase->ElementalResistance[4] / 10.0f,
                    },
                },
                Effect = new()
                {
                    EffectId = pEnemy->EnemyDataId,
                    IdleFrames = pThisBase->IdleFrames,
                    MagicFrames = pThisBase->MagicFrames,
                    AttackFrames = pThisBase->AttackFrames,
                    IdleAnimSpeed = pThisBase->IdleAnimSpeed,
                    ActWaitFrames = pThisBase->ActWaitFrames,
                    YPosOffset = pThisBase->YPosOffset,
                },
                Sound = new()
                {
                    AttackSound = pThisBase->AttackSound,
                    ActionSound = pThisBase->ActionSound,
                    MagicSound = pThisBase->MagicSound,
                    DeathSound = pThisBase->DeathSound,
                    CallSound = pThisBase->CallSound,
                },
                Script = new()
                {
                    TurnStartTag = PalConfig.AddAddress(
                        pEnemy->ScriptOnTurnStart,
                        $"Enemy_{id:D5}_TurnStart",
                        RGame.Address.AddrType.Enemy
                    ),
                    BattleWonTag = PalConfig.AddAddress(
                        pEnemy->ScriptOnBattleWon,
                        $"Enemy_{id:D5}_Won",
                        RGame.Address.AddrType.Enemy
                    ),
                    ActionTag = PalConfig.AddAddress(
                        pEnemy->ScriptOnAction,
                        $"Enemy_{id:D5}_Action",
                        RGame.Address.AddrType.Enemy
                    )
                },
            };

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
