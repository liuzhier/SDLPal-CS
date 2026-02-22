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

using Records.Pal;
using SimpleUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Vanara.PInvoke;

namespace Records.Ts;

public class FuncData
{
    public string[] ArgType { get; set; } = null!;
    public bool[] ArgCanBeOmitted { get; set; } = null!;
    public FuncInformation[] Ts { get; set; } = null!;
    public TsCases TsCases { get; set; } = null!;
    public AssemblyCases AssemblyCases { get; set; } = null!;
}

public class FuncInformation
{
    public enum SpecialType
    {
        Default,            // 默认使用函数声明的类型
        Address,            // 地址
        Scene,              // 场景
        Event,              // 事件
        SceneEvent,         // 场景 + 事件
        TriggerMode,        // 事件触发器模式
        TriggerRange,       // 事件触发器范围
        EventTrigger,       // 事件触发器（触发器模式 + 触发器范围）
        HeroEntity,         // Hero 实体
        ItemEntity,         // 道具实体
        MagicEntity,        // 仙术实体
        EnemyEntity,        // 敌人实体
        PoisonEntity,       // 毒性实体
        Entity,             // 实体
    }

    static readonly Dictionary<string, SpecialType> _typeNames = new()
    {
        ["Default"] = SpecialType.Default,
        ["Address"] = SpecialType.Address,
        ["Scene"] = SpecialType.Scene,
        ["Event"] = SpecialType.Event,
        ["SceneEvent"] = SpecialType.SceneEvent,
        ["TriggerMode"] = SpecialType.TriggerMode,
        ["TriggerRange"] = SpecialType.TriggerRange,
        ["TriggerModeTriggerRange"] = SpecialType.EventTrigger,
        ["HeroEntity"] = SpecialType.HeroEntity,
        ["ItemEntity"] = SpecialType.ItemEntity,
        ["MagicEntity"] = SpecialType.MagicEntity,
        ["EnemyEntity"] = SpecialType.EnemyEntity,
        ["PoisonEntity"] = SpecialType.PoisonEntity,
        ["Entity"] = SpecialType.Entity,
    };

    public int ArgId { get; set; } = -1;
    public int[] ArgIds { get; set; } = null!;
    public SpecialType Type { get; set; } = SpecialType.Default;

    public void SetTypeName(string typeName)
    {
        S.Failed(
            "FunctionInformation.GetTypeNames",
            $"Unknown type '{typeName}'",
            _typeNames.TryGetValue(typeName, out var type)
        );

        Type = type;
    }

    public void SetTypeNames(List<string> typeNames)
    {
        string      typeName;

        typeName = "";
        foreach (var name in typeNames)
            typeName += name;

        S.Failed(
            "FunctionInformation.GetTypeNames",
            $"Unknown type '{typeName}'",
            _typeNames.TryGetValue(typeName, out var type)
        );

        Type = type;
    }
}

public abstract class BidirectionalDictionary<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public Dictionary<TKey, TValue> Forwards { get; set; } = [];
    public Dictionary<TValue, TKey> Reverses { get; set; } = [];

    /// <summary>
    /// 设置新的映射
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="val">值</param>
    void Modif(TKey key, TValue val)
    {
        Forwards[key] = val;
        Reverses[val] = key;
    }

    /// <summary>
    /// 添加索引器，支持 "xxxx[key] = value" 语法
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>值</returns>
    public virtual TValue this[TKey key]
    {
        get
        {
            S.Failed(
                "BidirectionalDictionary.ValueGetter",
                $"The key '{key}' does not exist in the dictionary",
                Forwards.TryGetValue(key, out var value)
            );

            return value!;
        }
        set
        {
            //
            // 如果键已存在，则先清理反向映射
            //
            if (Forwards.TryGetValue(key, out var oldValue))
                Reverses.Remove(oldValue);

            //
            // 设置新的映射
            //
            Modif(key, value);
        }
    }

    /// <summary>
    /// 添加索引器，支持 "xxxx[value] = key" 语法
    /// </summary>
    /// <param name="val">值</param>
    /// <returns>键</returns>
    public virtual TKey this[TValue val]
    {
        get
        {
            S.Failed(
                "BidirectionalDictionary.KeyGetter",
                $"There is no entry in the dictionary with the value '{val}'",
                Reverses.TryGetValue(val, out var value)
            );

            return value!;
        }
        set
        {
            // 如果键已存在，需要先清理反向映射
            if (Reverses.TryGetValue(val, out var oldValue))
                Forwards.Remove(oldValue);

            //
            // 设置新的映射
            //
            Modif(value, val);
        }
    }

    /// <summary>
    /// 清空双向字典
    /// </summary>
    public void Clear()
    {
        Forwards.Clear();
        Reverses.Clear();
    }
}

public class EnumData : BidirectionalDictionary<string, int>;

public class FuncName : BidirectionalDictionary<ushort, List<string>>;

public class AssemblyCases : BidirectionalDictionary<int, ushort>
{
    public const int OtherCode = 0x10000;
    public int ArgId { get; set; } = -1;
}

public class TsCases : BidirectionalDictionary<Entity.Type, string>
{
    public int ArgId { get; set; } = -1;
}
