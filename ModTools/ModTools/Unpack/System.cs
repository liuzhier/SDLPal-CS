using Lib.Mod;
using SimpleUtility;
using System.Collections.Generic;
using EntityBeginId = Records.Pal.Entity.BeginId;

namespace ModTools.Unpack;

public static unsafe class System
{
    /// <summary>
    /// 处理系统实体。
    /// </summary>
    public static void Process()
    {
        List<string>        names;
        EntityBeginId       i;

        //
        // 处理实体对象名称
        //
        names = [];
        names.AddRange(Message.EntityNames[..(int)EntityBeginId.Hero]);
        for (i = EntityBeginId.Hero; i < EntityBeginId.System2; i++)
            names.Add($"0x{((int)i):x4}");
        names.AddRange(Message.EntityNames[(int)EntityBeginId.System2..(int)EntityBeginId.Item]);

        //
        // 导出 JSON 文件到输出目录
        //
        S.JsonSave(names.ToArray(), $"{Config.ModWorkPath.Game.Data.Entity.System}.json");
    }
}
