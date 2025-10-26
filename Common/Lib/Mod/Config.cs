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

using Lib.Pal;
using Records.Mod.RGame;
using SimpleUtility;
using System;
using System.Collections.Generic;
using EntityDos = Records.Pal.Entity.Dos;
using EntityWin = Records.Pal.Entity.Win;
using RModWorkPath = Records.Mod.WorkPath;
using RPalWorkPath = Records.Pal.WorkPath;

namespace Lib.Mod;

public static unsafe class Config
{
    public static bool IsDosGame = true;
    public static RPalWorkPath PalWorkPath { get; set; } = null!;
    public static RModWorkPath ModWorkPath { get; set; } = null!;
    public static MkfReader MkfBase { get; set; } = null!;
    public static MkfReader MkfCore { get; set; } = null!;
    public static ushort[] SceneEventIndexs { get; set; } = null!;
    public static Dictionary<int, int> NewMagicId { get; set; } = [];
    public static EntityDos* CoreDos { get; set; }
    public static EntityWin* CoreWin { get; set; }
    static Dictionary<int, Address> AddressDict { get; set; } = [];
    static Dictionary<string, ushort> NewAddressDict { get; set; } = [];
    static Dictionary<uint, ushort> NewEventIdDict { get; set; } = [];

    /// <summary>
    /// 初始化游戏全局数据
    /// </summary>
    /// <param name="palPath">Pal 游戏目录</param>
    /// <param name="modPath">Mod 工作目录</param>
    /// <param name="isDosGame">游戏版本，解包时传入 null，编译时指定版本</param>
    public static void Init(string palPath, string modPath, bool? isDosGame = null)
    {
        bool        isForceSetVerison;

        isForceSetVerison = (isDosGame != null);

        if (isForceSetVerison)
            //
            // 强制指定游戏资源版本
            //
            IsDosGame = (bool)isDosGame!;

        //
        // 初始化工作目录
        //
        PalWorkPath = new RPalWorkPath(palPath, ref isDosGame);
        ModWorkPath = new RModWorkPath(modPath);

        if (!isForceSetVerison)
            //
            // 游戏资源版本未被强制指定，需要重新设置版本
            //
            IsDosGame = (bool)isDosGame!;

        //
        // 打开数据文件
        //
        if (!isForceSetVerison)
        {
            MkfBase = new(PalWorkPath.DataBase.Base);
            MkfCore = new(PalWorkPath.DataBase.Core);
        }

        //
        // 初始化信息文件
        //
        Message.Init(
            IsDosGame,
            $@"{(isForceSetVerison ? $@"{ModWorkPath.Game.Data.Script}" : @"Dependency\Script")}\Message",
            palWorkPath: isForceSetVerison ? null! : PalWorkPath
        );
    }

    /// <summary>
    /// 释放全局数据
    /// </summary>
    public static void Free()
    {
        //
        // 清空 Address 字典
        //
        AddressDict.Clear();

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
        else
        {
            for (sceneId = 0; sceneId < SceneEventIndexs.Length; sceneId++)
                if (originEventId < SceneEventIndexs[sceneId])
                    break;

            eventId = (short)(originEventId - SceneEventIndexs[--sceneId]);
        }

        return (sceneId, eventId);
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
    public static ushort GetNewAddress(string addressTag)
    {
        //
        // 查找失败则报错退出
        //
        S.Failed(
            "Config.GetNewAddress",
            $"The address tag '{addressTag}' does not exist",
            NewAddressDict.TryGetValue(addressTag, out ushort address)
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
            $"The address tag '{oldEventId = (uint)(sceneId << 16) | eventId}' does not exist",
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
    public static ushort GetNewEventId(ushort sceneId, ushort eventId)
    {
        uint        oldEventId;

        //
        // 查找失败则报错退出
        //
        S.Failed(
            "Config.GetNewAddress",
            $"The address tag '{oldEventId = (uint)(sceneId << 16) | eventId}' does not exist",
            NewEventIdDict.TryGetValue(oldEventId, out ushort newEventId)
        );

        //
        // 查找成功
        //
        return newEventId;
    }
}
