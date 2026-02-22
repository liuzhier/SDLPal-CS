#region License
/*
 * Copyright (c) 2025, liuzhier <lichunxiao_lcx@qq.com>.
 * 
 * This file is part of SDLPAL-CS.
 * 
 * SDLPAL-CS is free software = you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 3
 * as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http =//www.gnu.org/licenses/>.
 * 
 */
#endregion License

using Records.Mod.RGame;
using SDLPal;
using static Records.Pal.Entity;
using EntityBeginId = Records.Pal.Entity.BeginId;
using ItemMask = Records.Pal.Entity.ItemMask;
using RGame = Records.Mod.RGame;

namespace ModTools.Unpack;

public static unsafe class Item
{
    /// <summary>
    /// 解档 Item 实体对象。
    /// </summary>
    public static void Process()
    {
        string          pathOut, whoCanEquip;
        string[]        indexContent;
        int             i, len, id, k, heroMask;
        ItemMask        mask;
        RGame.Item      item;
        ItemDos*        itemDos;
        ItemWin*        itemWin;

        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Entity = Item>");

        //
        // 创建输出目录 Item
        //
        pathOut = PalConfig.ModWorkPath.Assets.Data.Entity.Item;
        COS.Dir(pathOut);

        //
        // 处理 Item 实体对象
        //
        itemDos = null;
        itemWin = null;
        len = (int)EntityBeginId.Magic;
        indexContent = new string[len - (int)EntityBeginId.Item + 1];
        for (i = (int)EntityBeginId.Item, id = 1; i < len; i++, id++)
        {
            //
            // 获取当前 Item
            //
            if (PalConfig.IsDosGame)
                itemDos = &PalConfig.CoreDos[i].Item;
            else
                itemWin = &PalConfig.CoreWin[i].Item;

            //
            // 记录 Item 名称
            //
            indexContent[id] = PalMessage.GetEntityName(i);

            //
            // 获取掩码，计算该装备能被谁装备
            //
            mask = PalConfig.IsDosGame ? itemDos->Flags : itemWin->Flags;
            whoCanEquip = "";
            for (k = 0; k < Records.Pal.Base.MaxHero; k++)
            {
                heroMask = (int)ItemMask.EquipableByHeroFirst << k;

                if ((mask & (ItemMask)heroMask) != 0)
                    whoCanEquip += k + 1;
            }

            item = new()
            {
                Name = indexContent[id],
                Description = PalMessage.GetDescriptions(i, isItemOrMagic: true, isDosGame: PalConfig.IsDosGame),
                BitmapId = PalConfig.IsDosGame ? itemDos->BitmapId : itemWin->BitmapId,
                Price = PalConfig.IsDosGame ? itemDos->Price : itemWin->Price,
                EquipmentPart = i switch
                {
                    (>= 0x00C4 and <= 0x00CF) => PalEquipmentPart.Head,
                    (>= 0x00D0 and <= 0x00E0) => PalEquipmentPart.Cloak,
                    (>= 0x00E1 and <= 0x00EA) => PalEquipmentPart.Body,
                    (>= 0x00A3 and <= 0x00C3) => PalEquipmentPart.Hand,
                    (>= 0x00EB and <= 0x00F7) => PalEquipmentPart.Foot,
                    (>= 0x00F8 and <= 0x010B) or 0x10D or 0x0121 => PalEquipmentPart.Ornament,
                    _ => PalEquipmentPart.None,
                },
                Script = new()
                {
                    Use = new()
                    {
                        Tag = PalConfig.AddAddress(
                            PalConfig.IsDosGame ? itemDos->ScriptOnUse : itemWin->ScriptOnUse,
                            $"Item_{id:D5}_Use",
                            Address.AddrType.Item
                        ),
                    },
                    Equip = new()
                    {
                        Tag = PalConfig.AddAddress(
                            PalConfig.IsDosGame ? itemDos->ScriptOnEquip : itemWin->ScriptOnEquip,
                            $"Item_{id:D5}_Equip",
                            Address.AddrType.Item
                        ),
                    },
                    Throw = new()
                    {
                        Tag = PalConfig.AddAddress(
                            PalConfig.IsDosGame ? itemDos->ScriptOnThrow : itemWin->ScriptOnThrow,
                            $"Item_{id:D5}_Throw",
                            Address.AddrType.Item
                        ),
                    },
                    //Description = Config.AddAddress(0)
                },
                Scope = new()
                {
                    Usable = (mask & ItemMask.Usable) != 0,
                    Equipable = (mask & ItemMask.Equipable) != 0,
                    Throwable = (mask & ItemMask.Throwable) != 0,
                    Consuming = (mask & ItemMask.Consuming) != 0,
                    NeedSelectTarget = (mask & ItemMask.SkipTargetSelection) == 0,
                    Sellable = (mask & ItemMask.Sellable) != 0,
                    WhoCanEquip = whoCanEquip,
                }
            };

            //
            // 导出 JSON 文件到输出目录
            //
            S.JsonSave(item, $@"{pathOut}\{id:D5}.json");
        }

        //
        // 导出索引文件
        //
        S.IndexFileSave(indexContent, pathOut);
    }
}
