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

using Records.Mod.RGame;
using SDLPal;
using System;
using System.Collections.Generic;
using static Records.Mod.SetupLog;
using static Records.Pal.Base;
using static Records.Pal.Entity;
using EntityBeginId = Records.Pal.Entity.BeginId;
using RGame = Records.Mod.RGame;

namespace ModTools.Unpack;

public static unsafe class Hero
{
    /// <summary>
    /// 解档 Hero 实体对象。
    /// </summary>
    public static void Process()
    {
        string                          pathOut;
        nint                            pNative, pNative2;
        CLevelUpMagic*                  pMagicLearnable, pThisMagic;
        CHero*                          pBase;
        short[]                         magicLearned;
        Span<short>                     magicLearnedSpan;
        List<HeroMagicLearnable>        magicLearnable;
        string[]                        indexContent;
        int                             count, i, j, end, k, id;
        RGame.Hero                      hero;

        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Entity: Hero>");

        //
        // 创建输出目录 Hero
        //
        pathOut = PalConfig.ModWorkPath.Assets.Data.Entity.Hero;
        COS.Dir(pathOut);

        //
        // 读取可习得的仙术, 并计算有多少组
        //
        (pNative, count) = PalConfig.MkfBase.ReadChunk(6);
        count /= sizeof(CLevelUpMagic) * MaxHeroesInTeam;
        pMagicLearnable = (CLevelUpMagic*)pNative;

        //
        // 读取 Hero 数据块
        //
        (pNative2, _) = PalConfig.MkfBase.ReadChunk(3);
        pBase = (CHero*)pNative2;

        //
        // 处理 Hero 实体对象
        //
        magicLearnedSpan = magicLearned = new short[MaxHeroMagic];
        magicLearnable = [];
        indexContent = new string[MaxHero + 1];
        end = (int)EntityBeginId.System2;
        for (i = (int)EntityBeginId.Hero, j = 0; i < end; i++, j++)
        {
            id = j + 1;

            //
            // 清空仙术缓存
            //
            magicLearnedSpan.Clear();
            magicLearnable.Clear();

            //
            // 整理 Hero 已经领悟的法术
            //
            for (k = 0; k < magicLearned.Length; k++)
                magicLearned[k] = PalConfig.GetSoftMagicId(pBase->Magic[j, k]);

            //
            // 整理 Hero 可领悟的法术
            //
            for (k = 0; k < count; k++)
            {
                pThisMagic = &pMagicLearnable[k * MaxHeroesInTeam + j];

                if (pThisMagic->MagicId != 0)
                    magicLearnable.Add(new()
                    {
                        Level = pThisMagic->Level,
                        MagicId = PalConfig.GetSoftMagicId(pThisMagic->MagicId)
                    });
            }

            //
            // 记录 Hero 名称
            //
            indexContent[id] = PalMessage.GetEntityName(pBase->Name[j]);

            hero = new()
            {
                AvatarId = pBase->AvatarId[j],
                SpriteIdInBattle = pBase->SpriteIdInBattle[j],
                SpriteId = pBase->SpriteId[j],
                Name = indexContent[id],
                CanAttackAll = pBase->AttackAll[j] != 0,
                Level = pBase->Level[j],
                MaxHP = pBase->MaxHP[j],
                HP = pBase->HP[j],
                MaxMP = pBase->MP[j],
                MP = pBase->MP[j],
                CoveredBy = pBase->CoveredBy[j],
                FramesPerDirection = pBase->FramesPerDirection[j],
                Equipment = new()
                {
                    Head = pBase->Equipment[0, j],
                    Cloak = pBase->Equipment[1, j],
                    Body = pBase->Equipment[2, j],
                    Hand = pBase->Equipment[3, j],
                    Foot = pBase->Equipment[4, j],
                    Ornament = pBase->Equipment[5, j],
                },
                BaseAttribute = new()
                {
                    AttackStrength = pBase->Attribute.AttackStrength[j],
                    MagicStrength = pBase->Attribute.MagicStrength[j],
                    Defense = pBase->Attribute.Defense[j],
                    Dexterity = pBase->Attribute.Dexterity[j],
                    FleeRate = pBase->Attribute.FleeRate[j],
                },
                Resistance = new()
                {
                    Poison = pBase->PoisonResistance[j] / 10.0f,
                    Elemental = new()
                    {
                        Wind = pBase->ElementalResistance[j, 0] / 10.0f,
                        Thunder = pBase->ElementalResistance[j, 1] / 10.0f,
                        Water = pBase->ElementalResistance[j, 2] / 10.0f,
                        Fire = pBase->ElementalResistance[j, 3] / 10.0f,
                        Earth = pBase->ElementalResistance[j, 4] / 10.0f,
                    }
                },
                Sound = new()
                {
                    Death = pBase->DeathSound[j],
                    Attack = pBase->AttackSound[j],
                    Weapon = pBase->WeaponSound[j],
                    Critical = pBase->CriticalSound[j],
                    Magic = pBase->MagicSound[j],
                    Cover = pBase->CoverSound[j],
                    Dying = pBase->DyingSound[j],
                },
                Script = new()
                {
                    FriendDeathTag = PalConfig.AddAddress(
                        PalConfig.IsDosGame ?
                        PalConfig.CoreDos[i].Hero.ScriptOnFriendDeath :
                        PalConfig.CoreWin[i].Hero.ScriptOnFriendDeath,
                        $"Hero_{id:D5}_Death",
                        Address.AddrType.Hero
                    ),
                    DyingTag = PalConfig.AddAddress(
                        PalConfig.IsDosGame ?
                        PalConfig.CoreDos[i].Hero.ScriptOnDying :
                        PalConfig.CoreWin[i].Hero.ScriptOnDying,
                        $"Hero_{id:D5}_Dying",
                        Address.AddrType.Hero
                    )
                },
                Magic = new()
                {
                    Cooperative = PalConfig.GetSoftMagicId(pBase->CooperativeMagic[j]),
                    Learned = [.. magicLearned],
                    Learnable = magicLearnable
                },
            };

            //
            // 导出 JSON 文件到输出目录
            //
            S.JsonSave(hero, $@"{pathOut}\{id:D5}.json");
        }

        //
        // 导出索引文件
        //
        S.IndexFileSave(indexContent, pathOut);

        //
        // 释放非托管内存
        //
        C.free(pNative);
        C.free(pNative2);
    }
}
