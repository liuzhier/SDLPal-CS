using Records.Mod;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SDLPal;

public static unsafe class PalResource
{
    static LoadFlag TypeFlag { get; set; } = LoadFlag.None;
    static Dictionary<int, SceneSprite> RegisteredSprites { get; set; } = [];
    public static SceneSprite[] HeroSprites { get; set; } = null!;
    public static SceneSprite[] FollowerSprites { get; set; } = null!;
    public static SceneSprite[] EventSprites { get; set; } = null!;

    public enum LoadFlag
    {
        None           = 0,
        GlobalData     = (1 << 0),      // 加载全局数据
        Scene          = (1 << 1),      // 加载场景
        HeroSprite     = (1 << 2),      // 加载 Hero、Follower 贴图
    }

    public enum SpriteType
    {
        Event,
        Hero,
        Follower,
    }

    /// <summary>
    /// 初始化游戏资源子系统
    /// </summary>
    public static void Init()
    {
        //
        // 初始化全局图集模块
        //
        PalAtlas.Init();

        //
        // 初始化地图模块
        //
        PalMap.Init();

        //
        // 初始化场景模块
        //
        //PalScene.Init();

        //
        // 初始化全局数据模块
        //
        PalGlobal.Init();

        //
        // 初始化全局 UI 模块
        //
        PalUiGame.Init();

        //
        // 初始化脚本模块
        //
        PalScript.Init();

        //
        // 初始化文本绘制模块
        //
        PalText.Init();

        //
        // 初始化对话框模块
        //
        PalDialog.Init();
    }

    /// <summary>
    /// 销毁游戏资源子系统
    /// </summary>
    public static void Free()
    {
        //
        // 销毁文本绘制模块
        //
        PalText.Free();

        //
        // 销毁所有贴图
        //
        FreeRegisteredSprites();

        //
        // 销毁地图模块
        //
        PalMap.Free();

        //
        // 销毁全局图集模块
        //
        PalAtlas.Free();

        //
        // 销毁全局数据模块
        //
        PalGlobal.Free();
        PalUiGame.Free();

        //
        // 销毁脚本模块
        //
        PalScript.Free();
    }

    /// <summary>
    /// 释放所有 NPC 贴图
    /// </summary>
    /// <param name="sprites">NPC 贴图组</param>
    public static void FreeRegisteredSprites()
    {
        if (RegisteredSprites == null) return;

        foreach (var (key, value) in RegisteredSprites)
        {
            if (value.Sprite == null) continue;

            foreach (var sprite in value.Sprite)
            {
                FOS.Surface(sprite);
            }
        }

        RegisteredSprites.Clear();
    }

    /// <summary>
    /// 设置需要加载的资源类型
    /// </summary>
    /// <param name="flag">资源类型标志</param>
    public static void SetLoadFlags(LoadFlag flag) => TypeFlag |= flag;

    public static void Load()
    {
        if (TypeFlag == LoadFlag.None)
            //
            // 没有需要加载的资源
            //
            return;

        //
        // 加载全局游戏数据
        //
        if ((TypeFlag & LoadFlag.GlobalData) != 0)
        {
            //
            // 读取存档
            //
            PalGlobal.LoadSave(PalGlobal.CurrentSaveId);
            PalAudio.PlayMusic(S.Save.MusicId);
        }

        //
        // 加载场景
        //
        if ((TypeFlag & LoadFlag.Scene) != 0)
        {
            //
            // 销毁已经注册的贴图
            //
            FreeRegisteredSprites();

            //
            // 销毁场景贴图（NPC 贴图和地图瓦片）
            //
            PalMap.FreeTile();

            //
            // 加载地图
            //
            var scene = S.CurrScene;
            PalMap.Load(scene.MapId);

            //
            // bug：允许黑屏中走一格
            //
            PalScene.UpdateHeroTeam();

            //
            // 加载 NPC 贴图
            //
            var events = scene.Events;
            EventSprites = new SceneSprite[events.Count];

            for (var i = 1; i < EventSprites.Length; i++)
            {
                var spriteId = events[i].Sprite.SpriteId;

                if (spriteId == 0) continue;

                events[i].Trail.SpriteFramesAuto = LoadNpcSprites(out EventSprites[i], spriteId);
            }

            //
            // 所有贴图都被释放了，Hero 贴图也需要重新读取
            //
            SetLoadFlags(LoadFlag.HeroSprite);
        }

        //
        // 加载 Hero 贴图
        //
        if ((TypeFlag & LoadFlag.HeroSprite) != 0)
        {
            //
            // 初始化 Hero 贴图列表
            //
            var heroTeamLength = S.Save.HeroTeamLength;
            HeroSprites = new SceneSprite[heroTeamLength];

            //
            // 加载 Hero 贴图
            //
            for (var i = 0; i < heroTeamLength; i++)
            {
                var heroTeam = S.GetMember(i);
                var hero = S.Entity.Heroes[heroTeam.HeroId];
                var spriteId = hero.Sprite.SpriteId;

                heroTeam.Trail.SpriteFramesAuto = LoadNpcSprites(out HeroSprites[i], spriteId);
                hero.Sprite.FramesPerDirection = (heroTeam.Trail.SpriteFramesAuto / 4) switch
                {
                    >= 9 => 9,
                    >= 3 => 3,
                    _ => 0,
                };
            }

            //
            // 初始化随从贴图列表
            //
            var followers = S.Save.Followers;
            FollowerSprites = new SceneSprite[followers.Count];

            //
            // 加载随从贴图
            //
            for (var i = 0; i < followers.Count; i++)
            {
                var follower = followers[i];
                var spriteId = follower.SpriteId;

                //spriteId = 2;

                follower.Trail.SpriteFramesAuto = LoadNpcSprites(out FollowerSprites[i], spriteId);
            }
        }

        //
        // 清除所有加载标志
        //
        TypeFlag = LoadFlag.None;
    }

    /// <summary>
    /// 加载 NPC 贴图
    /// </summary>
    /// <param name="sprite">要加载到哪个列表</param>
    /// <param name="spriteId">贴图编号</param>
    /// <returns>贴图帧数量</returns>
    public static int LoadNpcSprites(out SceneSprite sprite, int spriteId)
    {
        var path = S.ModPath.Assets.Sprite.Character;
        var pathNew = $@"{path}New";

        if (RegisteredSprites.TryGetValue(spriteId, out sprite!))
            //
            // 贴图已被注册，直接引用，避免重复读取
            //
            return sprite.Sprite.Count;

        //
        // 初始化列表
        //
        sprite = new();

        //
        // 检查是否有可将其代替的新版贴图
        //
        var loadNew = Directory.Exists($@"{pathNew}\{spriteId:D5}");
        if (loadNew)
        {
            //
            // 找到了新版贴图，直接将路径修改为新版贴图
            //
            path = pathNew;

            //
            // 将缩放因子设置为 1
            //
            sprite.NeedStretch = false;
        }

        path = $@"{path}\{spriteId:D5}";
        var i = 0;
        for (; ; i++)
        {
            var imgPath = $@"{path}\{i:D5}.png";

            if (!File.Exists(imgPath))
                //
                // 贴图序列中断，结束读取
                //
                break;

            //
            // 将贴图加载为表面
            //
            var surface = COS.Surface(imgPath);

            //
            // 将这帧贴图放入列表
            //
            sprite.Sprite.Add(surface);
        }

        //
        // 注册贴图，避免重复读取图像
        //
        RegisteredSprites[spriteId] = sprite;

        //
        // 返回贴图帧数量
        //
        return i;
    }

    /// <summary>
    /// 检查图像是否需要按场景比例缩放
    /// </summary>
    /// <param name="spriteId">贴图编号</param>
    /// <returns>是否需要按场景比例缩放</returns>
    public static bool CheckStretch(int spriteId)
    {
        RegisteredSprites.TryGetValue(spriteId, out var sprite);
        return sprite?.NeedStretch ?? true;
    }

    /// <summary>
    /// 获取一组贴图中的一帧
    /// </summary>
    /// <param name="spriteType">是则获取队员贴图，否则获取 NPC 贴图</param>
    /// <param name="spriteId">贴图编号</param>
    /// <param name="frameId">帧号</param>
    /// <returns>一组贴图中的一帧</returns>
    public static nint GetSpriteFrame(SpriteType spriteType, int spriteId, int frameId)
    {
        var sprites = spriteType switch
        {
            SpriteType.Event => EventSprites,
            SpriteType.Hero => HeroSprites,
            SpriteType.Follower => FollowerSprites,
            _ => throw S.Failed("PalResource.GetSpriteFrame", $"Unknown Sprite Type: '{spriteType}'"),
        };

        var sprite = sprites[spriteId].Sprite;

        return (frameId >= sprite.Count) ? 0 : sprite[frameId];
    }
}
