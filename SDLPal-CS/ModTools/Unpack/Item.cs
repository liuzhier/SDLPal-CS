using SDLPal;
using SDLPal.Record.RGame;
using SimpleUtility;
using static ModTools.Record.Data;
using static ModTools.Record.Entity;
using EntityBeginId = ModTools.Record.Entity.BeginId;
using ItemMask = ModTools.Record.Entity.ItemMask;
using RGame = SDLPal.Record.RGame;

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
        int             i, len, j, id, k;
        ItemMask        mask;
        RGame.Item      item;
        ItemDos*        itemDos;
        ItemWin*        itemWin;

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Entity: Item>");

        //
        // 创建输出目录 Item
        //
        pathOut = Global.WorkPath.Game.Data.Entity.Item;
        COS.Dir(pathOut);

        //
        // 处理 Item 实体对象
        //
        itemDos = null;
        itemWin = null;
        len = (int)EntityBeginId.Magic;
        indexContent = new string[len - (int)EntityBeginId.Item];
        for (i = (int)EntityBeginId.Item, j = 0; i < len; i++, j++)
        {
            //
            // 获取当前 Item
            //
            if (Config.IsDosGame)
                itemDos = &Entity.CoreDos[i].Item;
            else
                itemWin = &Entity.CoreWin[i].Item;

            //
            // 记录 Item 名称
            //
            indexContent[j] = Message.EntityNames[i];

            //
            // 获取掩码，计算该装备能被谁装备
            //
            mask = (ItemMask)(Config.IsDosGame ? itemDos->Flags : itemWin->Flags);
            whoCanEquip = "";
            for (k = 0; k < MaxHero; k++)
            {
                id = (int)ItemMask.EquipableByHeroFirst << k;

                if ((mask & (ItemMask)id) != 0)
                    whoCanEquip += k + 1;
            }

            id = j + 1;
            item = new(
                Name: indexContent[j],
                Description: Message.GetDescriptions(i, isItemOrMagic: true),
                BitmapId: Config.IsDosGame ? itemDos->BitmapId : itemWin->BitmapId,
                Price: Config.IsDosGame ? itemDos->Price : itemWin->Price,
                Script: new ItemScript
                {
                    UseTag = Script.AddAddress(
                        Config.IsDosGame ? itemDos->ScriptOnUse : itemWin->ScriptOnUse,
                        $"Item_{id:D5}_Use",
                        Script.AddressType.Item
                    ),
                    EquipTag = Script.AddAddress(
                        Config.IsDosGame ? itemDos->ScriptOnEquip : itemWin->ScriptOnEquip,
                        $"Item_{id:D5}_Equip",
                        Script.AddressType.Item
                    ),
                    ThrowTag = Script.AddAddress(
                        Config.IsDosGame ? itemDos->ScriptOnThrow : itemWin->ScriptOnThrow,
                        $"Item_{id:D5}_Throw",
                        Script.AddressType.Item
                    ),
                    DescriptionTag = Script.AddAddress(0),
                },
                Scope: new(
                    Usable: (mask & ItemMask.Usable) != 0,
                    Equipable: (mask & ItemMask.Usable) != 0,
                    Throwable: (mask & ItemMask.Throwable) != 0,
                    Consuming: (mask & ItemMask.Consuming) != 0,
                    ApplyToAll: (mask & ItemMask.ApplyToAll) != 0,
                    Sellable: (mask & ItemMask.Sellable) != 0,
                    WhoCanEquip: whoCanEquip
                )
            );

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
