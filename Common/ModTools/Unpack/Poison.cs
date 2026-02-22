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
using System.Numerics;
using System.Xml.Linq;
using static Records.Mod.SetupLog;
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
        int                 i, begin, id;
        string[]            indexContent;
        PoisonCommon*       pPoison;
        RGame.Poison        posion;

        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Entity: Poison>");

        //
        // 创建输出目录 Posion
        //
        pathOut = PalConfig.ModWorkPath.Assets.Data.Entity.Poison;
        COS.Dir(pathOut);

        //
        // 处理 Posion 实体对象
        //
        begin = (int)EntityBeginId.Poison;
        indexContent = new string[Entity.CoreDataCount - begin + 1];
        for (i = begin, id = 1; i < Entity.CoreDataCount; i++, id++)
        {
            //
            // 获取当前 Posion
            //
            pPoison = (PalConfig.IsDosGame) ? &PalConfig.CoreDos[i].Poison : &PalConfig.CoreWin[i].Poison;

            //
            // 记录 Posion 名称
            //
            indexContent[id] = PalMessage.GetEntityName(i);

            posion = new()
            {
                Name = indexContent[id],
                Level = pPoison->Level,
                Color = pPoison->Color,
                Script = new()
                {
                    PlayerTag = PalConfig.AddAddress(
                        pPoison->PlayerScript,
                        $"Poison_{id:D5}_Player",
                        RGame.Address.AddrType.Poison
                    ),
                    EnemyTag = PalConfig.AddAddress(
                        pPoison->EnemyScript,
                        $"Poison_{id:D5}_Enemy",
                        RGame.Address.AddrType.Poison
                    )
                },
            };

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
