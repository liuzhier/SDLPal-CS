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

using ModTools.Util;
using Records.Mod.RGame;
using Records.Pal;
using System;
using System.Collections.Generic;
using EntityDos = Records.Pal.Entity.Dos;
using EntityWin = Records.Pal.Entity.Win;
using RPalWorkPath = Records.Pal.WorkPath;
using RModWorkPath = Records.Mod.WorkPath;
using RSetup = Records.Mod.Setup;
using System.IO;

namespace SDLPal;

public static unsafe class PalConfig
{
    public static bool IsDosGame { get; set; } = false;
    public static string Version => IsDosGame ? "Dos" : "Win";
    public static string LogOutPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public static RPalWorkPath PalWorkPath { get; set; } = null!;
    public static RModWorkPath ModWorkPath { get; set; } = null!;
    public static RSetup Setup { get; set; } = null!;
    public static MkfReader MkfBase { get; set; } = null!;
    public static MkfReader MkfCore { get; set; } = null!;
    public static EntityDos* CoreDos { get; set; }
    public static EntityWin* CoreWin { get; set; }
    public static ushort[] SceneEventIndexs { get; set; } = null!;
    static Dictionary<short, short> SoftMagicId { get; set; } = [];
    static Dictionary<int, Address> AddressDict { get; set; } = [];
    static Dictionary<Entity.Type, int> NewEntityBeginId { get; set; } = [];
    static Dictionary<string, int> NewAddressDict { get; set; } = [];
    static Dictionary<uint, int> NewEventIdDict { get; set; } = [];

    /// <summary>
    /// 初始化游戏全局数据
    /// </summary>
    /// <param name="palPath">Pal 游戏目录</param>
    /// <param name="modPath">Mod 工作目录</param>
    public static void Init(string palPath, string modPath)
    {
        //
        // 初始化 Mod 工作目录
        //
        ModWorkPath = new(modPath);

        var isUnpack = (palPath != null);
        if (isUnpack)
        {
            var isDosGame = IsDosGame;

            //
            // 初始化 Pal 游戏目录
            //
            PalWorkPath = new(palPath!, ref isDosGame);

            IsDosGame = isDosGame;

            //
            // 打开数据文件
            //
            MkfBase = new(PalWorkPath.DataBase.Base);
            MkfCore = new(PalWorkPath.DataBase.Core);

            //
            // 创建 SDLPal-CS 配置文件
            //
            S.JsonSave(new RSetup(), ModWorkPath.Setup);
        }
        else
        {
            // 
            // 加载游戏配置
            //
            if (File.Exists(ModWorkPath.Setup))
            {
                S.JsonLoad(out RSetup setup, ModWorkPath.Setup);
                Setup = setup;
            }
            else
                Setup = new();
        }

        //
        // 初始化信息文件
        //
        PalMessage.Init(
            IsDosGame,
            $@"{(isUnpack ? @"Dependency\Script" : $@"{ModWorkPath.Assets.Data.Script}")}\include",
            PalWorkPath
        );
    }

    /// <summary>
    /// 初始化游戏全局数据
    /// </summary>
    /// <param name="palPath">Pal 游戏目录</param>
    /// <param name="modPath">Mod 工作目录</param>
    public static void Init(string modPath) => Init(null!, modPath);

    /// <summary>
    /// 释放全局数据
    /// </summary>
    public static void Free()
    {
        //
        // 清空所有字典
        //
        SceneEventIndexs = default!;
        SoftMagicId.Clear();
        NewEntityBeginId.Clear();
        AddressDict.Clear();
        NewAddressDict.Clear();
        NewEventIdDict.Clear();

        //
        // 关闭数据文件
        //
        MkfBase?.Dispose();
        MkfCore?.Dispose();
    }

    /// <summary>
    /// 将原游戏的硬 EventId 转换为软 SceneId 和软 EventId
    /// </summary>
    /// <param name="originEventId">原游戏的硬 EventId</param>
    /// <returns>软 SceneId 和软 EventId</returns>
    public static (short, short) GetSoftSceneEventId(short originEventId)
    {
        short      sceneId, eventId;

        if (originEventId == -1)
            sceneId = eventId = -1;
        else if(originEventId == 0)
            sceneId = eventId = 0;
        else
        {
            eventId = -2;

            for (sceneId = 0; sceneId < SceneEventIndexs.Length; sceneId++)
                if (originEventId <= SceneEventIndexs[sceneId])
                {
                    eventId = (short)(originEventId - SceneEventIndexs[sceneId - 1]);
                    break;
                }

            S.Failed(
                "Config.GetSoftSceneEventId",
                "The soft number calculation failed",
                eventId != -2
            );
        }

        return (sceneId, eventId);
    }

    public static ushort GetSoftEntityId(Entity.Type type, int entityId)
    {
        Entity.BeginId      beginId;

        if (entityId == -1 || entityId == 0)
            return (ushort)entityId;

        beginId = type switch
        {
            Entity.Type.System => Entity.BeginId.System,
            Entity.Type.Hero => Entity.BeginId.Hero,
            Entity.Type.Item => Entity.BeginId.Item,
            Entity.Type.Enemy => Entity.BeginId.Enemy,
            Entity.Type.Poison => Entity.BeginId.Poison,
            _ => throw new NotImplementedException(),
        };

        return (ushort)(entityId - (int)beginId + 1);
    }

    public static void AddSoftMagicId(short hardId, short softId) =>
        SoftMagicId[hardId] = softId;

    public static short GetSoftMagicId(short hardId)
    {
        S.Failed(
            "Config.GetNewAddress",
            $"The magic '{hardId}' is undefined.",
            SoftMagicId.TryGetValue(hardId, out var softId)
        );

        return softId;
    }

    /// <summary>
    /// 若地址字典中已存在该地址则将地址标签覆盖到 addressName，否则将新记录添加到字典。
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="addressTag">地址标签</param>
    /// <param name="type">地址类型</param>
    /// <param name="objectId">对象编号</param>
    /// <returns>实际放入字典的地址标签</returns>
    public static string AddAddress(int address, string? addressTag = null, Address.AddrType type = Address.AddrType.Public, int objectId = -1)
    {
        //
        // 若地址标签为空则自动生成
        //
        addressTag ??= $"@{address:X4}";

        if (AddressDict.TryGetValue(address, out var addr))
        {
            //
            // 查找成功，返回字典里的地址标签
            //
            addressTag = addr.Tag;
        }
        else
        {
            //
            // 查找失败，将新记录放入字典
            //
            AddressDict[address] = new()
            {
                Tag = addressTag,
                Type = type,
                ObjectId = objectId,
            };
        }

        return addressTag;
    }

    /// <summary>
    /// 获取地址
    /// </summary>
    /// <param name="address"></param>
    /// <param name="addr"></param>
    /// <returns></returns>
    public static bool GetAddress(int address, out Address addr) =>
        AddressDict.TryGetValue(address, out addr);

    /// <summary>
    /// 将重新分配的新地址添加到字典
    /// </summary>
    /// <param name="addressTag">地址标签</param>
    /// <param name="address">实际地址</param>
    public static void AddNewAddress(string addressTag, ushort address)
    {
        //
        // 若标签已在字典中（重复）则报错退出
        //
        S.Failed(
            "Config.GetNewAddress",
            $"The address tag '{addressTag}' does not exist",
            !NewAddressDict.TryGetValue(addressTag, out _)
        );

        NewAddressDict[addressTag] = address;
    }

    /// <summary>
    /// 获取重新分配的新地址
    /// </summary>
    /// <param name="addressTag">地址标签</param>
    /// <returns>重新分配的新地址</returns>
    public static int GetNewAddress(string addressTag)
    {
        //
        // 保证字符串不为空对象
        //
        addressTag ??= "";

        //
        // 查找失败则报错退出
        //
        S.Failed(
            "Config.GetNewAddress",
            $"The address tag '{addressTag}' does not exist",
            NewAddressDict.TryGetValue(addressTag, out int address)
        );

        //
        // 查找成功
        //
        return address;
    }

    /// <summary>
    /// 将重新分配的新 Event 编号添加到字典
    /// </summary>
    /// <param name="sceneId">软 Scene 编号</param>
    /// <param name="eventId">软 Event 编号</param>
    /// <param name="newEventId">硬 Event 编号</param>
    public static void AddNewEventId(ushort sceneId, ushort eventId, ushort newEventId)
    {
        uint        oldEventId;

        //
        // 若标签已在字典中（重复）则报错退出
        //
        S.Failed(
            "Config.GetNewAddress",
            $"The address tag '{oldEventId = (uint)(sceneId * Math.Pow(10, 5)) + eventId}' does not exist",
            !NewEventIdDict.TryGetValue(oldEventId, out _)
        );

        NewEventIdDict[oldEventId] = newEventId;
    }

    /// <summary>
    /// 获取重新分配的新 Event 编号
    /// </summary>
    /// <param name="sceneId">软 Scene 编号</param>
    /// <param name="eventId">软 Event 编号</param>
    /// <returns>重新分配的新 Event 编号</returns>
    public static int GetNewEventId(ushort sceneId, ushort eventId)
    {
        uint        oldEventId;

        if (sceneId == 0 || eventId == 0)
            return 0;

        //
        // 查找失败则报错退出
        //
        S.Failed(
            "Config.GetNewEventId",
            $"Event '{oldEventId = (uint)(sceneId * Math.Pow(10, 5)) + eventId}' is undefined",
            NewEventIdDict.TryGetValue(oldEventId, out int newEventId)
        );

        //
        // 查找成功
        //
        return newEventId;
    }

    /// <summary>
    /// 获取重新分配的新 Event 编号
    /// </summary>
    /// <param name="sceneId">软 Scene 编号</param>
    /// <param name="eventId">软 Event 编号</param>
    /// <returns>重新分配的新 Event 编号</returns>
    public static int GetNewEventId(int sceneId, int eventId) =>
        GetNewEventId((ushort)sceneId, (ushort)eventId);

    /// <summary>
    /// 将重新分配的新 Entity 的起始编号添加到字典
    /// </summary>
    /// <param name="type">Entity 类型</param>
    /// <param name="beginId">起始编号</param>
    public static void AddNewEntityBeginId(Entity.Type type, int beginId) =>
        NewEntityBeginId[type] = beginId;

    /// <summary>
    /// 获取重新分配的新 Entity 编号
    /// </summary>
    /// <param name="type">Entity 类型</param>
    /// <param name="entityId">软 Entity 编号</param>
    /// <returns>新 Entity 编号</returns>
    public static short GetNewEntityId(Entity.Type type, int entityId)
    {
        if (entityId == 0)
            return 0;

        if (entityId == -1)
            return -1;

        S.Failed(
            "Config.GetNewEntityId",
            $"The entity type '{type}' is undefined",
            NewEntityBeginId.TryGetValue(type, out var beginId)
        );

        return (short)(beginId + entityId - 1);
    }

    /// <summary>
    /// 获取重新分配的仙术新 Entity 编号
    /// </summary>
    /// <param name="softMagicId">仙术软 Entity 编号</param>
    /// <returns>仙术新 Entity 编号</returns>
    public static short GetNewMagicId(short softMagicId)
    {
        bool        isSummonGold;

        if (softMagicId == 0)
            return 0;

        if (isSummonGold = softMagicId >= 30000)
            softMagicId -= 30000;

        return (short)GetNewEntityId(isSummonGold ? Entity.Type.SummonGold : Entity.Type.Magic, softMagicId);
    }
}
