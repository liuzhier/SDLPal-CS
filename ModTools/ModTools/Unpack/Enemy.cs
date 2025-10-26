using SDLPal;
using SimpleUtility;
using static ModTools.Record.Data;
using static ModTools.Record.Entity;
using EntityBeginId = ModTools.Record.Entity.BeginId;
using RGame = SDLPal.Record.RGame;

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
        S.Log("Unpack the game data. <Entity: Enemy>");

        //
        // 创建输出目录 Enemy
        //
        pathOut = Global.WorkPath.Game.Data.Entity.Enemy;
        COS.Dir(pathOut);

        //
        // 读取基础 Enemy 数据
        //
        (pNative, _) = Util.ReadMkfChunk(Config.FileBase, 1);
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
            pEnemy = (Config.IsDosGame) ? &Entity.CoreDos[i].Enemy : &Entity.CoreWin[i].Enemy;
            pThisBase = &pBase[pEnemy->EnemyDataId];

            //
            // 记录 Enemy 名称
            //
            indexContent[j] = Message.EntityNames[i];

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
                    TurnStartTag = Script.AddAddress(
                        pEnemy->ScriptOnTurnStart,
                        $"Enemy_{id:D5}_TurnStart",
                        Script.AddressType.Enemy
                    ),
                    BattleWonTag = Script.AddAddress(
                        pEnemy->ScriptOnBattleWon,
                        $"Enemy_{id:D5}_Won",
                        Script.AddressType.Enemy
                    ),
                    ActionTag = Script.AddAddress(
                        pEnemy->ScriptOnAction,
                        $"Enemy_{id:D5}_Action",
                        Script.AddressType.Enemy
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
