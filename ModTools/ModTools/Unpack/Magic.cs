using SDLPal;
using SimpleUtility;
using static ModTools.Record.Data;
using static ModTools.Record.Entity;
using EntityBeginId = ModTools.Record.Entity.BeginId;
using RGame = SDLPal.Record.RGame;

namespace ModTools.Unpack;

public static unsafe class Magic
{
    /// <summary>
    /// 解档 Magic 实体对象。
    /// </summary>
    public static void Process()
    {
        string          pathOut;
        nint            pNative;
        int             end, i, j, id;
        MagicMask       mask;
        CMagic*         pBase, pThisBase;
        MagicDos*       magicDos;
        MagicWin*       magicWin;
        string[]        indexContent;
        RGame.Magic     magic;

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Entity: Magic>");

        //
        // 创建输出目录 Magic
        //
        pathOut = Global.WorkPath.Game.Data.Entity.Magic;
        COS.Dir(pathOut);

        //
        // 读取基础仙术数据
        //
        (pNative, _) = Util.ReadMkfChunk(Config.FileBase, 4);
        pBase = (CMagic*)pNative;

        //
        // 处理 Magic 实体对象
        // 最先处理特殊的仙术“投掷”
        //
        magicDos = null;
        magicWin = null;
        end = (int)EntityBeginId.Enemy;
        indexContent = new string[end - (int)EntityBeginId.Magic + 1];
        for (i = 0x18, j = 0; i < end;)
        {
            //
            // 获取当前 Magic
            //
            if (Config.IsDosGame)
                magicDos = &Entity.CoreDos[i].Magic;
            else
                magicWin = &Entity.CoreWin[i].Magic;

            pThisBase = &pBase[Config.IsDosGame ? magicDos->MagicDataId : magicWin->MagicDataId];

            //
            // 记录 Magic 名称
            //
            indexContent[j] = Message.EntityNames[i];

            //
            // 获取掩码
            //
            mask = (MagicMask)(Config.IsDosGame ? magicDos->Flags : magicWin->Flags);

            id = j + 1;
            magic = new RGame.Magic(
                Name: indexContent[j],
                Description: Message.GetDescriptions(i, isItemOrMagic: false),
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
                    UseTag = Script.AddAddress(
                        Config.IsDosGame ? magicDos->ScriptOnUse : magicWin->ScriptOnUse,
                        $"Magic_{id:D5}_Use",
                        Script.AddressType.Magic
                    ),
                    SuccessTag = Script.AddAddress(
                        Config.IsDosGame ? magicDos->ScriptOnSuccess : magicWin->ScriptOnSuccess,
                        $"Magic_{id:D5}_Success",
                        Script.AddressType.Magic
                    ),
                    DescriptionTag = Script.AddAddress(0),
                },
                Scope: new RGame.MagicScope(
                    UsableOutsideBattle: (mask & MagicMask.UsableOutsideBattle) != 0,
                    UsableInBattle: (mask & MagicMask.UsableInBattle) != 0,
                    UsableToEnemy: (mask & MagicMask.UsableToEnemy) != 0,
                    NeedSelectRole: (mask & MagicMask.ApplyToAll) == 0
                )
            );

            //
            // 导出 JSON 文件到输出目录
            //
            S.JsonSave(magic, $@"{pathOut}\{id:D5}.json");

            i = (int)EntityBeginId.Magic + (j++);
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
