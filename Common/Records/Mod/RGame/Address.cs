using System;
using System.Collections.Generic;
using System.Text;

namespace Records.Mod.RGame;

public record struct Address
{
    public enum AddrType
    {
        Public,
        Hero,
        Item,
        Magic,
        Enemy,
        Poison,
        Scene,
    }

    public string Tag { get; init; }        // 地址标签
    public AddrType Type { get; init; }     // 地址类型
    public int ObjectId { get; init; }      // 对象编号
}
