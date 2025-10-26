﻿using SDLPal;
using SDLPal.Record.RGame;
using SimpleUtility;
using static ModTools.Record.Data;
using EntityBeginId = ModTools.Record.Entity.BeginId;
using RGame = SDLPal.Record.RGame;

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
        ushort[]                        magicLearned;
        Span<ushort>                    magicLearnedSpan;
        List<HeroMagicLearnable>        magicLearnable;
        string[]                        indexContent;
        int                             count, i, j, end, k, id;
        RGame.Hero                      hero;

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Entity: Hero>");

        //
        // 创建输出目录 Hero
        //
        pathOut = Global.WorkPath.Game.Data.Entity.Hero;
        COS.Dir(pathOut);

        //
        // 读取可习得的仙术, 并计算有多少组
        //
        (pNative, count) = Util.ReadMkfChunk(Config.FileBase, 6);
        count /= sizeof(CLevelUpMagic) * MaxHeroesInTeam;
        pMagicLearnable = (CLevelUpMagic*)pNative;

        //
        // 读取 Hero 数据块
        //
        (pNative2, _) = Util.ReadMkfChunk(Config.FileBase, 3);
        pBase = (CHero*)pNative2;

        //
        // 处理 Hero 实体对象
        //
        magicLearnedSpan = magicLearned = new ushort[MaxHeroMagic];
        magicLearnable = [];
        indexContent = new string[MaxHero];
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
                magicLearned[k] = pBase->Magic[k, j];

            //
            // 整理 Hero 可领悟的法术
            //
            for (k = 0; k < count; k++)
            {
                pThisMagic = &pMagicLearnable[k * MaxHeroesInTeam + j];

                if (pThisMagic->MagicId != 0)
                    magicLearnable.Add(new(
                        Level: pThisMagic->Level,
                        MagicId: pThisMagic->MagicId
                    ));
            }

            //
            // 记录 Hero 名称
            //
            indexContent[j] = Message.EntityNames[pBase->Name[j]];

            hero = new RGame.Hero(
                AvatarId: pBase->AvatarId[j],
                SpriteIdInBattle: pBase->SpriteIdInBattle[j],
                SpriteId: pBase->SpriteIdInBattle[j],
                Name: indexContent[j],
                CanAttackAll: pBase->AttackAll[j] != 0,
                Level: pBase->Level[j],
                MaxHP: pBase->MaxHP[j],
                HP: pBase->HP[j],
                MaxMP: pBase->MP[j],
                MP: pBase->MP[j],
                CoveredBy: pBase->CoveredBy[j],
                WalkFrames: pBase->WalkFrames[j],
                Equipment: new(
                    Head: pBase->Equipment[0, j],
                    Cloak: pBase->Equipment[1, j],
                    Body: pBase->Equipment[2, j],
                    Hand: pBase->Equipment[3, j],
                    Foot: pBase->Equipment[4, j],
                    Ornament: pBase->Equipment[5, j]
                ),
                BaseAttribute: new(
                    AttackStrength: pBase->Attribute.AttackStrength[j],
                    MagicStrength: pBase->Attribute.MagicStrength[j],
                    Defense: pBase->Attribute.Defense[j],
                    Dexterity: pBase->Attribute.Dexterity[j],
                    FleeRate: pBase->Attribute.FleeRate[j]
                ),
                Resistance: new(
                    Poison: pBase->PoisonResistance[j],
                    Elemental: new(
                        Wind: pBase->ElementalResistance[j, 0],
                        Thunder: pBase->ElementalResistance[j, 1],
                        Water: pBase->ElementalResistance[j, 2],
                        Fire: pBase->ElementalResistance[j, 3],
                        Earth: pBase->ElementalResistance[j, 4]
                    )
                ),
                Sound: new(
                    Death: pBase->DeathSound[j],
                    Attack: pBase->AttackSound[j],
                    Weapon: pBase->WeaponSound[j],
                    Critical: pBase->CriticalSound[j],
                    Magic: pBase->MagicSound[j],
                    Cover: pBase->CoverSound[j],
                    Dying: pBase->DyingSound[j]
                ),
                Script: new HeroScript
                {
                    FriendDeathTag = Script.AddAddress(
                        Config.IsDosGame ?
                        Entity.CoreDos[i].Hero.ScriptOnFriendDeath :
                        Entity.CoreWin[i].Hero.ScriptOnFriendDeath,
                        $"Hero_{id:D5}_Death"
                    ),
                    DyingTag = Script.AddAddress(
                        Config.IsDosGame ?
                        Entity.CoreDos[i].Hero.ScriptOnDying :
                        Entity.CoreWin[i].Hero.ScriptOnDying,
                        $"Hero_{id:D5}_Dying"
                    )
                },
                Magic: new(
                    CooperativeMagicId: pBase->CooperativeMagic[j],
                    Magic: magicLearned,
                    MagicLearnable: [.. magicLearnable]
                )
            );

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
