using Lib.Mod;
using ModTools;
using SimpleUtility;
using static Records.Pal.Entity;
using EntityBeginId = Records.Pal.Entity.BeginId;
using RGame = Records.Mod.RGame;

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
        Util.Log("Unpack the game data. <Entity: Poison>");

        //
        // 创建输出目录 Posion
        //
        pathOut = Config.ModWorkPath.Game.Data.Entity.Poison;
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
            pPoison = (Config.IsDosGame) ? &Config.CoreDos[i].Poison : &Config.CoreWin[i].Poison;

            //
            // 记录 Posion 名称
            //
            indexContent[j] = Message.GetEntityName(i);

            id = j + 1;
            posion = new RGame.Poison(
                Name: indexContent[j],
                Level: pPoison->Level,
                Color: pPoison->Color,
                Script: new RGame.PosionScript
                {
                    PlayerTag = Config.AddAddress(
                        pPoison->PlayerScript,
                        $"Poison_{id:D5}_Player",
                        RGame.Address.AddrType.Poison
                    ),
                    EnemyTag = Config.AddAddress(
                        pPoison->EnemyScript,
                        $"Poison_{id:D5}_Enemy",
                        RGame.Address.AddrType.Poison
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
