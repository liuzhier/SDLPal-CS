using Avalonia.Media;
using Lib.Mod;
using Lib.Pal;
using ModTools;
using SimpleUtility;
using System.Collections.Generic;
using System.Linq;
using static Records.Pal.Data;
using static Records.Pal.Entity;
using EntityBeginId = Records.Pal.Entity.BeginId;
using RGame = Records.Mod.RGame;

namespace ModTools.Unpack;

public static unsafe class Magic
{
    /// <summary>
    /// 解档 Magic 实体对象。
    /// </summary>
    public static void Process()
    {
        string                  magicPath, summonGoldPath;
        nint                    pNative;
        int                     end, i, j, magicId, summonGoldId;
        MagicMask               mask;
        CMagic*                 pBase, pThisBase;
        CSummon*                pSummon;
        MagicDos*               magicDos;
        MagicWin*               magicWin;
        List<string>            magicIndexContent, summonGoldIndexContent;
        RGame.Magic             magic;
        RGame.SummonGold        summonGold;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Entity: Magic>");

        //
        // 创建输出目录 Magic
        //
        magicPath = Config.ModWorkPath.Game.Data.Entity.Magic;
        COS.Dir(magicPath);
        summonGoldPath = Config.ModWorkPath.Game.Data.Entity.SummonGold;
        COS.Dir(summonGoldPath);

        //
        // 读取基础仙术数据
        //
        (pNative, _) = PalUtil.ReadMkfChunk(Config.FileBase, 4);
        pBase = (CMagic*)pNative;

        //
        // 处理 Magic 实体对象
        // 最先处理特殊的仙术“投掷”
        //
        magicDos = null;
        magicWin = null;
        end = (int)EntityBeginId.Enemy;
        magicIndexContent = new();
        summonGoldIndexContent = new();
        for (i = 0x18, j = magicId = summonGoldId = 0; i < end; i = ((int)EntityBeginId.Magic + (j++)))
        {
            //
            // 获取当前 Magic
            //
            if (Config.IsDosGame)
                magicDos = &Config.CoreDos[i].Magic;
            else
                magicWin = &Config.CoreWin[i].Magic;

            //
            // 获取法术特效参数
            //
            pThisBase = &pBase[Config.IsDosGame ? magicDos->MagicDataId : magicWin->MagicDataId];

            //
            // 获取掩码
            //
            mask = (MagicMask)(Config.IsDosGame ? magicDos->Flags : magicWin->Flags);

            //
            // 检查是仙术还是召唤神法术
            //
            if ((RGame.MagicActionType)pThisBase->Type != RGame.MagicActionType.Summon)
            {
                //
                // 记录 Magic 名称
                //
                magicIndexContent.Add(Message.GetEntityName(i));

                magic = new RGame.Magic(
                    Name: magicIndexContent.Last(),
                    Description: Message.GetDescriptions(i, isItemOrMagic: false, isDosGame: Config.IsDosGame),
                    CostMP: pThisBase->CostMP,
                    BaseDamage: pThisBase->BaseDamage,
                    SoundId: pThisBase->SoundId,
                    Type: (RGame.MagicType)pThisBase->Elemental,
                    ActionType: (RGame.MagicActionType)pThisBase->Type,
                    Effect: new RGame.MagicEffect(
                        EffectId: Config.IsDosGame ? magicDos->MagicDataId : magicWin->MagicDataId,
                        XOffset: pThisBase->XOffset,
                        YOffset: pThisBase->YOffset,
                        LayerOffset: pThisBase->LayerOffset,
                        Speed: pThisBase->Speed,
                        KeepEffect: pThisBase->KeepEffect,
                        SoundDelay: pThisBase->SoundDelay,
                        EffectTimes: pThisBase->EffectTimes,
                        Shake: pThisBase->Shake,
                        Wave: pThisBase->Wave
                    ),
                    Script: new RGame.MagicScript
                    {
                        UseTag = Config.AddAddress(
                            Config.IsDosGame ? magicDos->ScriptOnUse : magicWin->ScriptOnUse,
                            $"Magic_{magicId:D5}_Use",
                            RGame.Address.AddrType.Magic
                        ),
                        SuccessTag = Config.AddAddress(
                            Config.IsDosGame ? magicDos->ScriptOnSuccess : magicWin->ScriptOnSuccess,
                            $"Magic_{magicId:D5}_Success",
                            RGame.Address.AddrType.Magic
                        ),
                        DescriptionTag = Config.AddAddress(0),
                    },
                    Scope: new RGame.MagicScope(
                        UsableOutsideBattle: (mask & MagicMask.UsableOutsideBattle) != 0,
                        UsableInBattle: (mask & MagicMask.UsableInBattle) != 0,
                        UsableToEnemy: (mask & MagicMask.UsableToEnemy) != 0,
                        NeedSelectTarget: (mask & MagicMask.ApplyToAll) == 0
                    )
                );

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(magic, $@"{magicPath}\{(++magicId):D5}.json");
            }
            else
            {
                //
                // 获取召唤神特效参数
                //
                pSummon = (CSummon*)pThisBase;

                //
                // 记录 Magic 名称
                //
                summonGoldIndexContent.Add(Message.GetEntityName(i));

                summonGold = new RGame.SummonGold(
                    Name: summonGoldIndexContent.Last(),
                    Description: Message.GetDescriptions(i, isItemOrMagic: false, isDosGame: Config.IsDosGame),
                    CostMP: pSummon->CostMP,
                    BaseDamage: pSummon->BaseDamage,
                    SoundId: pSummon->SoundId,
                    Type: (RGame.MagicType)pSummon->Elemental,
                    Effect: new RGame.SummonGoldEffect(
                        SpiritId: pSummon->SpriteId,
                        EffectId: pSummon->MagicDataId,
                        XOffset: pSummon->XOffset,
                        YOffset: pSummon->YOffset,
                        IdleFrames: pSummon->IdleFrames,
                        MagicFrames: pSummon->MagicFrames,
                        AttackFrames: pSummon->AttackFrames,
                        ColorShift: pSummon->ColorShift,
                        Shake: pSummon->Shake,
                        Wave: pSummon->Wave
                    ),
                    Script: new RGame.MagicScript
                    {
                        UseTag = Config.AddAddress(
                            Config.IsDosGame ? magicDos->ScriptOnUse : magicWin->ScriptOnUse,
                            $"Magic_{summonGoldId:D5}_Use",
                            RGame.Address.AddrType.Magic
                        ),
                        SuccessTag = Config.AddAddress(
                            Config.IsDosGame ? magicDos->ScriptOnSuccess : magicWin->ScriptOnSuccess,
                            $"Magic_{summonGoldId:D5}_Success",
                            RGame.Address.AddrType.Magic
                        ),
                        DescriptionTag = Config.AddAddress(0),
                    },
                    Scope: new RGame.MagicScope(
                        UsableOutsideBattle: (mask & MagicMask.UsableOutsideBattle) != 0,
                        UsableInBattle: (mask & MagicMask.UsableInBattle) != 0,
                        UsableToEnemy: (mask & MagicMask.UsableToEnemy) != 0,
                        NeedSelectTarget: (mask & MagicMask.ApplyToAll) == 0
                    )
                );

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(summonGold, $@"{summonGoldPath}\{(++summonGoldId):D5}.json");
            }
        }

        //
        // 导出索引文件
        //
        S.IndexFileSave([.. magicIndexContent], magicPath);
        S.IndexFileSave([.. summonGoldIndexContent], summonGoldPath);

        //
        // 释放非托管内存
        //
        C.free(pNative);
    }
}
