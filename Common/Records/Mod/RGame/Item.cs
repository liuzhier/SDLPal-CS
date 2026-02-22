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

using System.Text.Json.Serialization;

namespace Records.Mod.RGame;

public record class Item
{
    public string Name { get; set; } = null!;           // 名称
    public string[]? Description { get; set; }          // 描述
    public int BitmapId { get; set; }                   // 图像
    public int Price { get; set; }                      // 售价（典当半价）
    public ItemScript Script { get; set; } = null!;     // 脚本
    public ItemScope Scope { get; set; } = null!;       // 作用域
}

public record class ItemScript
{
    public string UseTag { get; set; } = null!;                // 使用脚本
    [JsonIgnore]
    public int Use { get; set; }
    [JsonIgnore]
    public string EquipTag { get; set; } = null!;              // 装备脚本
    [JsonIgnore]
    public int Equip { get; set; }
    public string ThrowTag { get; set; } = null!;              // 投掷脚本
    [JsonIgnore]
    public int Throw { get; set; }
    //public string Description { get; set; } = null!;        // 描述脚本（不仅能显示描述，还能进行其他的奇葩操作）
    //                                                        // （如比如查看说明顺便全体回血，但是试了一下，没法中毒--）
    //                                                        // 暂不支持！描述脚本由程序自动生成
}

public record class ItemScope
{
    public bool Usable { get; set; }                        // 可使用
    public bool Equipable { get; set; }                     // 可装备
    public bool Throwable { get; set; }                     // 可投掷
    public bool Consuming { get; set; }                     // 使用后减少
    public bool NeedSelectTarget { get; set; }              // 需要选择目标
    public bool Sellable { get; set; }                      // 可典当
    public string WhoCanEquip { get; set; } = null!;        // 可装备者，例：
                                                            // 丝衣-李赵林（123）
                                                            // 圣灵珠-赵巫（24）
                                                            // 豹牙手环-全体（#）
                                                            // 竹笛-巫后除外（#4）
                                                            // 丝衣-女性（#1）
                                                            // 凤纹披风-女性，巫后除外（#14）
}
