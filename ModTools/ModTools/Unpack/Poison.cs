using SDLPal;
using SimpleUtility;
using static ModTools.Record.Entity;
using EntityBeginId = ModTools.Record.Entity.BeginId;
using RGame = SDLPal.Record.RGame;

namespace ModTools.Unpack;

public static unsafe class Poison
{
    /// <summary>
    /// 解档 Poison 实体对象。
    /// </summary>
    public static void Process()
    {
        string              pathOut;
        int                 i, begin, j, id;
        string[]            indexContent;
        PoisonCommon*       pPoison;
        RGame.Poison        posion;

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Entity: Poison>");

        //
        // 创建输出目录 Posion
        //
        pathOut = Global.WorkPath.Game.Data.Entity.Poison;
        COS.Dir(pathOut);

        //
        // 处理 Posion 实体对象
        //
        begin = (int)EntityBeginId.Poison;
        indexContent = new string[Entity.CoreDataCount - begin];
        for (i = begin, j = 0; i < Entity.CoreDataCount; i++, j++)
        {
            //
            // 获取当前 Posion
            //
            pPoison = (Config.IsDosGame) ? &Entity.CoreDos[i].Poison: &Entity.CoreWin[i].Poison;

            //
            // 记录 Posion 名称
            //
            indexContent[j] = Message.EntityNames[i];

            id = j + 1;
            posion = new RGame.Poison(
                Name: indexContent[j],
                Level: pPoison->Level,
                Color: pPoison->Color,
                Script: new RGame.PosionScript
                {
                    PlayerTag = Script.AddAddress(
                        pPoison->PlayerScript,
                        $"Poison_{id:D5}_Player",
                        Script.AddressType.Poison
                    ),
                    EnemyTag = Script.AddAddress(
                        pPoison->EnemyScript,
                        $"Poison_{id:D5}_Enemy",
                        Script.AddressType.Poison
                    )
                }
            );

            //
            // 导出 JSON 文件到输出目录
            //
            S.JsonSave(posion, $@"{pathOut}\{id:D5}.json");
        }

        //
        // 导出索引文件
        //
        S.IndexFileSave(indexContent, pathOut);
    }
}
