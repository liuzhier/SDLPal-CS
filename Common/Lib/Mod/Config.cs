using Lib.Pal;
using Records.Mod.RGame;
using System.Collections.Generic;
using System.IO;
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
    public static BinaryReader FileBase { get; set; } = null!;
    public static BinaryReader FileCore { get; set; } = null!;
    public static ushort[] SceneEventIndexs { get; set; } = null!;
    public static Dictionary<int, Address> AddressDict { get; set; } = [];
    public static EntityDos* CoreDos { get; set; }
    public static EntityWin* CoreWin { get; set; }

    /// <summary>
    /// 初始化游戏全局数据
    /// </summary>
    /// <param name="palPath">Pal 游戏目录</param>
    /// <param name="modPath">Mod 工作目录</param>
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
            FileBase = PalUtil.BinaryRead(PalWorkPath.DataBase.Base);
            FileCore = PalUtil.BinaryRead(PalWorkPath.DataBase.Core);
        }

        //
        // 初始化信息文件
        //
        Message.Init(PalWorkPath, IsDosGame);
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
        PalUtil.CloseBinary(FileBase);
        PalUtil.CloseBinary(FileCore);
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
    /// 若地址字典中已存在该地址则将地址名称覆盖到 addressName，否则将新记录添加到字典。
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="addressName">地址的名称</param>
    public static string AddAddress(int address, string? addressName = null, Address.AddrType type = Address.AddrType.Public, int objectId = -1)
    {
        //
        // 若地址名称为空则自动生成
        //
        addressName ??= $"@{address:X4}";

        if (AddressDict.TryGetValue(address, out var addr))
        {
            //
            // 查找成功，返回字典里的地址标签
            //
            addressName = addr.Tag;
        }
        else
        {
            //
            // 查找失败，将新记录放入字典
            //
            AddressDict[address] = new()
            {
                Tag = addressName,
                Type = type,
                ObjectId = objectId,
            };
        }

        return addressName;
    }
}
