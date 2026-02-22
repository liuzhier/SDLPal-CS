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

using Records.Pal;
using System.Collections.Generic;
using static Records.Pal.Base;
using static Records.Pal.Entity;
using PalEntity = Records.Pal.Entity;
using EntityBeginId = Records.Pal.Entity.BeginId;
using RGame = Records.Mod.RGame;
using SDLPal;

namespace ModTools.Unpack;

public static unsafe class Magic
{
    /// <summary>
    /// 解档 Magic 实体对象。
    /// </summary>
    public static void Process()
    {
        string                      magicPath, summonGoldPath;
        nint                        pNative;
        int                         end, i, j, magicId, summonGoldId;
        MagicMask                   mask;
        CMagic*                     pBase, pThisBase;
        CSummon*                    pSummon;
        MagicDos*                   magicDos;
        MagicWin*                   magicWin;
        List<string>                magicIndexContent, summonGoldIndexContent;
        RGame.Magic                 magic;
        RGame.SummonGold            summonGold;
        Dictionary<int, int>        magicDataId;
        ushort                      effectId;

        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Entity: Magic>");

        //
        // 创建输出目录 Magic
        //
        magicPath = PalConfig.ModWorkPath.Assets.Data.Entity.Magic;
        COS.Dir(magicPath);
        summonGoldPath = PalConfig.ModWorkPath.Assets.Data.Entity.SummonGold;
        COS.Dir(summonGoldPath);

        //
        // 读取基础仙术数据
        //
        (pNative, _) = PalConfig.MkfBase.ReadChunk(4);
        pBase = (CMagic*)pNative;

        //
        // 记录特殊法术
        // “-1：本回合什么都不做”
        // “0：本回合普攻”
        //
        magicDataId = [];
        PalConfig.AddSoftMagicId(-1, -1);
        PalConfig.AddSoftMagicId(0, 0);

        //
        // 先收集 Entity 对应的 Magic Base Data
        //
        magicDos = null;
        magicWin = null;
        magicIndexContent = [null!];
        summonGoldIndexContent = [null!];
        end = (int)EntityBeginId.Enemy;
        for (i = 0x18, j = 0, summonGoldId = magicId = 1; i < end; i = ((int)EntityBeginId.Magic + (j++)))
        {
            //
            // 获取当前 Magic
            //
            if (PalConfig.IsDosGame)
                magicDos = &PalConfig.CoreDos[i].Magic;
            else
                magicWin = &PalConfig.CoreWin[i].Magic;

            //
            // 获取法术特效参数
            //
            pThisBase = &pBase[PalConfig.IsDosGame ? magicDos->MagicDataId : magicWin->MagicDataId];

            //
            // 检查是仙术还是召唤神法术
            //
            if (pThisBase->ActionType != Base.MagicActionType.Summon)
            {
                //
                // 记录 Magic 新编号
                //
                effectId = PalConfig.IsDosGame ? magicDos->MagicDataId : magicWin->MagicDataId;
                magicDataId[effectId] = magicId;
                PalConfig.AddSoftMagicId((short)i, (short)magicId++);

                //
                // 记录 Magic 名称
                //
                magicIndexContent.Add(PalMessage.GetEntityName(i));
            }
            else
            {
                //
                // 记录 Summon Gold 新编号
                //
                PalConfig.AddSoftMagicId((short)i, (short)(PalEntity.SummonGoldCodeHead | summonGoldId++));

                //
                // 记录 Summon Gold 名称
                //
                summonGoldIndexContent.Add(PalMessage.GetEntityName(i));
            }
        }

        //
        // 导出索引文件
        //
        S.IndexFileSave([.. magicIndexContent], magicPath);
        S.IndexFileSave([.. summonGoldIndexContent], summonGoldPath);

        //
        // 处理 Magic 实体对象
        // 最先处理特殊的仙术“投掷”
        //
        for (i = 0x18, j = 0, summonGoldId = magicId = 1; i < end; i = ((int)EntityBeginId.Magic + (j++)))
        {
            //
            // 获取当前 Magic
            //
            if (PalConfig.IsDosGame)
                magicDos = &PalConfig.CoreDos[i].Magic;
            else
                magicWin = &PalConfig.CoreWin[i].Magic;

            //
            // 获取法术特效参数
            //
            pThisBase = &pBase[PalConfig.IsDosGame ? magicDos->MagicDataId : magicWin->MagicDataId];

            //
            // 获取掩码
            //
            mask = PalConfig.IsDosGame ? magicDos->Flags : magicWin->Flags;

            //
            // 检查是仙术还是召唤神法术
            //
            if (pThisBase->ActionType != Base.MagicActionType.Summon)
            {
                magic = new()
                {
                    Name = magicIndexContent[magicId],
                    Description = PalMessage.GetDescriptions(i, isItemOrMagic: false, isDosGame: PalConfig.IsDosGame),
                    CostMP = pThisBase->CostMP,
                    BaseDamage = pThisBase->BaseDamage,
                    SoundId = pThisBase->SoundId,
                    Type = pThisBase->Type,
                    ActionType = pThisBase->ActionType,
                    Effect = new()
                    {
                        EffectId = PalConfig.IsDosGame ? magicDos->MagicDataId : magicWin->MagicDataId,
                        XOffset = pThisBase->XOffset,
                        YOffset = pThisBase->YOffset,
                        LayerOffset = pThisBase->LayerOffset,
                        FrameDelay = pThisBase->FrameDelay,
                        KeepEffect = pThisBase->KeepEffect,
                        PreviewFrames = pThisBase->PreviewFrames,
                        EffectTimes = pThisBase->EffectTimes,
                        Shake = pThisBase->Shake,
                        Wave = pThisBase->Wave,
                    },
                    Script = new()
                    {
                        Use = new()
                        {
                            Tag = PalConfig.AddAddress(
                                PalConfig.IsDosGame ? magicDos->ScriptOnUse : magicWin->ScriptOnUse,
                                $"Magic_{magicId:D5}_Use",
                                RGame.Address.AddrType.Magic
                            ),
                        },
                        Success = new()
                        {
                            Tag = PalConfig.AddAddress(
                                PalConfig.IsDosGame ? magicDos->ScriptOnSuccess : magicWin->ScriptOnSuccess,
                                $"Magic_{magicId:D5}_Success",
                                RGame.Address.AddrType.Magic
                            ),
                        },
                        //Description = Config.AddAddress(0),
                    },
                    Scope = new()
                    {
                        UsableOutsideBattle = (mask & MagicMask.UsableOutsideBattle) != 0,
                        UsableInBattle = (mask & MagicMask.UsableInBattle) != 0,
                        UsableToEnemy = (mask & MagicMask.UsableToEnemy) != 0,
                        NeedSelectTarget = (mask & MagicMask.SkipTargetSelection) == 0,
                    },
                };

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(magic, $@"{magicPath}\{(magicId++):D5}.json");
            }
            else
            {
                //
                // 获取召唤神特效参数
                //
                pSummon = (CSummon*)pThisBase;

                summonGold = new RGame.SummonGold(
                    Name: summonGoldIndexContent[summonGoldId],
                    Description: PalMessage.GetDescriptions(i, isItemOrMagic: false, isDosGame: PalConfig.IsDosGame),
                    CostMP: pSummon->CostMP,
                    BaseDamage: pSummon->BaseDamage,
                    SoundId: pSummon->SoundId,
                    Type: pSummon->Type,
                    Effect: new RGame.SummonGoldEffect(
                        SpriteId: pSummon->SpriteId,
                        EffectId: (ushort)magicDataId[pSummon->MagicDataId],
                        XOffset: pSummon->XOffset,
                        YOffset: pSummon->YOffset,
                        IdleFrames: pSummon->IdleFrames,
                        MagicFrames: pSummon->MagicFrames,
                        AttackFrames: pSummon->AttackFrames,
                        ColorShift: pSummon->ColorShift,
                        Shake: pSummon->Shake,
                        Wave: pSummon->Wave
                    ),
                    Script: new()
                    {
                        Use = new()
                        {
                            Tag = PalConfig.AddAddress(
                                PalConfig.IsDosGame ? magicDos->ScriptOnUse : magicWin->ScriptOnUse,
                                $"SummonGold_{summonGoldId:D5}_Use",
                                RGame.Address.AddrType.Magic
                            ),
                        },
                        Success = new()
                        {
                            Tag = PalConfig.AddAddress(
                                PalConfig.IsDosGame ? magicDos->ScriptOnSuccess : magicWin->ScriptOnSuccess,
                                $"SummonGold_{summonGoldId:D5}_Success",
                                RGame.Address.AddrType.Magic
                            ),
                        },
                        //Description: Config.AddAddress(0)
                    },
                    Scope: new()
                    {
                        UsableOutsideBattle = (mask & MagicMask.UsableOutsideBattle) != 0,
                        UsableInBattle = (mask & MagicMask.UsableInBattle) != 0,
                        UsableToEnemy = (mask & MagicMask.UsableToEnemy) != 0,
                        NeedSelectTarget = (mask & MagicMask.SkipTargetSelection) == 0
                    }
                );

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(summonGold, $@"{summonGoldPath}\{(summonGoldId++):D5}.json");
            }
        }

        //
        // 释放非托管内存
        //
        C.free(pNative);
    }
}
