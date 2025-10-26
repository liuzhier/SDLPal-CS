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

public record class Item(
    string Name,                // 名称
    string[]? Description,      // 描述
    ushort BitmapId,            // 图像
    ushort Price,               // 售价（典当半价）
    ItemScript Script,          // 脚本
    ItemScope Scope             // 作用域
);

public record class ItemScript(
    string Use,             // 使用脚本
    string Equip,           // 装备脚本
    string Throw            // 投掷脚本
    //string Description      // 描述脚本（不仅能显示描述，还能进行其他的奇葩操作）
    //                        // （如比如查看说明顺便全体回血，但是试了一下，没法中毒--）
    //                        // 暂不支持！描述脚本由程序自动生成
);

public record class ItemScope(
    bool Usable,                // 可使用
    bool Equipable,             // 可装备
    bool Throwable,             // 可投掷
    bool Consuming,             // 使用后减少
    bool NeedSelectTarget,      // 需要选择目标
    bool Sellable,              // 可典当
    string WhoCanEquip          // 可装备者，例：
                                // 丝衣-李赵林（123）
                                // 圣灵珠-赵巫（24）
                                // 豹牙手环-全体（#）
                                // 竹笛-巫后除外（#4）
                                // 丝衣-女性（#1）
                                // 凤纹披风-女性，巫后除外（#14）
);
