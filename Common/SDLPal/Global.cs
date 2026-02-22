using ModTools.Util;
using Records.Mod;
using Records.Mod.RGame;
using Records.Pal;
using System.IO;

namespace SDLPal;

public static unsafe class PalGlobal
{
    public static GameSave Save { get; set; } = new();
    public static GameConst Const { get; set; } = new();
    public static MoviePlayer MoviePlayer { get; set; } = null!;
    public static LogWriter Logger { get; set; } = new($@"{S.ModPath.Log}\Debug.txt");
    public static int CurrentSaveId { get; set; }
    public static long FrameNum { get; set; }
    public static bool EnterScene { get; set; }
    public static bool NeedToFadeIn { get; set; }
    public static Item SceneUseItem { get; set; } = null!;
    public static int CurrentAnimationId { get; set; }
    public static bool InBattle { get; set; }
    public static bool IsPlayingCurrentAnimationId { get; set; }        // 允许在播放 RNG 动画时绘制文本
    static bool DrawSceneDebugData { get; set; }
    public static bool NeedDrawSceneDebugData
    {
        get => S.Setup.Debug.DrawSceneData && DrawSceneDebugData;
        set => DrawSceneDebugData = value;
    }

    /// <summary>
    /// 初始化全局数据模块
    /// </summary>
    public static void Init() => LoadConstData();

    /// <summary>
    /// 销毁全局数据模块
    /// </summary>
    public static void Free()
    {
        Logger?.Dispose();
    }

    /// <summary>
    /// 加载全局恒不变数据，这些数据只被加载一次
    /// </summary>
    public static void LoadConstData()
    {
        //
        // 加载商店
        //
        var path = S.ModDataPath.Shop;
        for (var i = 1; ; i++)
        {
            var pathFull = $@"{path}\{i:D5}.json";
            if (!File.Exists(pathFull))
                //
                // 文件序列中断，停止读取
                //
                break;

            //
            // 解析 json 数据
            //
            S.JsonLoad(out int[] shop, pathFull);
            Const.Shops.Add(shop);
        }

        //
        // 加载敌方队列
        //
        path = S.ModDataPath.EnemyTeam;
        for (var i = 1; ; i++)
        {
            var pathFull = $@"{path}\{i:D5}.json";
            if (!File.Exists(pathFull))
                //
                // 文件序列中断，停止读取
                //
                break;

            //
            // 解析 json 数据
            //
            S.JsonLoad(out int[] enemyTeam, pathFull);
            Const.EnemyTeams.Add(enemyTeam);
        }
    }

    /// <summary>
    /// 加载存档数据
    /// </summary>
    /// <param name="saveSlotId">存档编号，缺省则为新建存档</param>
    public static void LoadSave(int saveSlotId = -1)
    {
        CurrentSaveId = saveSlotId;

        if (CurrentSaveId == -1 || true)
        {
            //
            // 新建存档
            //
            CreateSave();

            //Save.HeroTeamLength = 1;

            //Save.HeroTeamLength = Base.MaxHero;
            //Save.SceneId = 21;
            //S.SetHeroTeamPos(new(576, 1536));
            //Save.Money = 1000000;
            //Save.Inventories.Add(new(212, 1));
            //Save.Inventories.Add(new(219, 1));
        }
        else
        {
            //
            // 读取存档
            //
            var saveFilePath = $@"{S.ModPath.Save}\{CurrentSaveId:D5}";
        }
    }

    /// <summary>
    /// 新建存档数据
    /// </summary>
    static void CreateSave()
    {
        //Save.listHeroExperience = PalUnpak.Json2Obj<List<ExperienceAll>>($@"{DATA_PATH}\");

        var data = S.ModDataPath;

        //
        // 读取场景数据
        //
        var path = data.Scene;
        for (var i = 1; ; i++)
        {
            var pathFull = $@"{path}\{i:D5}";
            if (!Directory.Exists(pathFull))
                //
                // 场景序列中断，停止读取
                //
                break;

            //
            // 解析场景 json 数据
            //
            S.JsonLoad(out Scene scene, $@"{pathFull}\Scene.json");
            Save.Scenes.Add(scene);

            //
            // 将脚本标签转换为 ASM 地址
            //
            var script= scene.Script;
            script.Enter.Value = PalConfig.GetNewAddress(script.Enter.Tag);
            script.Teleport.Value = PalConfig.GetNewAddress(script.Teleport.Tag);

            //
            // 读取事件数据
            //
            var events = scene.Events;
            for (var j = 1; ; j++)
            {
                var pathFile = $@"{pathFull}\{j:D5}.json";

                if (!File.Exists(pathFile))
                    //
                    // 事件序列中断，停止读取
                    //
                    break;

                //
                // 解析事件 json 数据
                //
                S.JsonLoad(out Event @event, pathFile);
                events.Add(@event);

                //
                // 将脚本标签转换为 ASM 地址
                //
                var script2= @event.Script ??= new();
                script2.Trigger.Value = PalConfig.GetNewAddress(script2.Trigger.Tag);
                script2.Auto.Value = PalConfig.GetNewAddress(script2.Auto.Tag);
            }
        }

        //
        // 读取 Hero 实体数据
        //
        var entity = Save.Entity;
        path = $@"{data.Entity}\Hero";
        for (var i = 1; ; i++)
        {
            var pathFull = $@"{path}\{i:D5}.json";
            if (!File.Exists(pathFull))
                //
                // 实体序列中断，停止读取
                //
                break;

            //
            // 解析 Hero 实体 json 数据
            //
            S.JsonLoad(out Hero hero, pathFull);

            entity.Heroes.Add(hero);

            //
            // 将脚本标签转换为 ASM 地址
            //
            var script = entity.Heroes[^1].Script ??= new();
            script.FriendDeath.Value = PalConfig.GetNewAddress(script.FriendDeath.Tag);
            script.Dying.Value = PalConfig.GetNewAddress(script.Dying.Tag);
        }

        //
        // 读取 Magic 实体数据
        //
        path = $@"{data.Entity}\Magic";
        for (var i = 1; ; i++)
        {
            var pathFull = $@"{path}\{i:D5}.json";
            if (!File.Exists(pathFull))
                //
                // 实体序列中断，停止读取
                //
                break;

            //
            // 解析 Magic 实体件 json 数据
            //
            S.JsonLoad(out Magic magic, pathFull);
            entity.Magics.Add(magic);

            //
            // 将脚本标签转换为 ASM 地址
            //
            var script = magic.Script ??= new();
            script.Success.Value = PalConfig.GetNewAddress(script.Success.Tag);
            script.Use.Value = PalConfig.GetNewAddress(script.Use.Tag);
        }

        //
        // 读取 Item 实体数据
        //
        path = $@"{data.Entity}\Item";
        for (var i = 1; ; i++)
        {
            var pathFull = $@"{path}\{i:D5}.json";
            if (!File.Exists(pathFull))
                //
                // 实体序列中断，停止读取
                //
                break;

            //
            // 解析 Item 实体件 json 数据
            //
            S.JsonLoad(out Item item, pathFull);
            entity.Items.Add(item);

            //
            // 将脚本标签转换为 ASM 地址
            //
            var script = item.Script ??= new();
            script.Use.Value = PalConfig.GetNewAddress(script.Use.Tag);
            script.Equip.Value = PalConfig.GetNewAddress(script.Equip.Tag);
            script.Throw.Value = PalConfig.GetNewAddress(script.Throw.Tag);
        }

        //
        // 读取 Enemy 实体数据
        //
        path = $@"{data.Entity}\Enemy";
        for (var i = 1; ; i++)
        {
            var pathFull = $@"{path}\{i:D5}.json";
            if (!File.Exists(pathFull))
                //
                // 实体序列中断，停止读取
                //
                break;

            //
            // 解析 Enemy 实体件 json 数据
            //
            S.JsonLoad(out Enemy enemy, pathFull);
            entity.Enemies.Add(enemy);

            //
            // 将脚本标签转换为 ASM 地址
            //
            var script = enemy.Script ??= new();
            script.TurnStart.Value = PalConfig.GetNewAddress(script.TurnStart.Tag);
            script.BattleWon.Value = PalConfig.GetNewAddress(script.BattleWon.Tag);
            script.Action.Value = PalConfig.GetNewAddress(script.Action.Tag);
        }

        //
        // 读取 Poison 实体数据
        //
        path = $@"{data.Entity}\Poison";
        for (var i = 1; ; i++)
        {
            var pathFull = $@"{path}\{i:D5}.json";
            if (!File.Exists(pathFull))
                //
                // 实体序列中断，停止读取
                //
                break;

            //
            // 解析 Poison 实体件 json 数据
            //
            S.JsonLoad(out Poison poison, pathFull);
            entity.Poisons.Add(poison);

            //
            // 将脚本标签转换为 ASM 地址
            //
            var script = poison.Script ??= new();
            script.Enemy.Value = PalConfig.GetNewAddress(script.Enemy.Tag);
            script.Player.Value = PalConfig.GetNewAddress(script.Player.Tag);
        }
    }

    /// <summary>
    /// 设置资源加载标志
    /// </summary>
    /// <param name="saveSlotId">存档编号，缺省则为新建存档</param>
    public static void ReloadInNextTick(int saveSlotId)
    {
        FrameNum = 0;
        CurrentSaveId = saveSlotId;
        EnterScene = true;
        NeedToFadeIn = true;

        PalResource.SetLoadFlags(
           PalResource.LoadFlag.GlobalData
           | PalResource.LoadFlag.Scene
           | PalResource.LoadFlag.HeroSprite
        );
    }

    /// <summary>
    /// 更新所有队员所装备物品的效果
    /// </summary>
    public static void UpdateEquipEffect()
    {
        //int      i, j, w;

        //memset(&(gpGlobals->rgEquipmentEffect), 0, sizeof(gpGlobals->rgEquipmentEffect));

        //for (i = 0; i < MAX_PLAYER_ROLES; i++)
        //{
        //   for (j = 0; j < MAX_PLAYER_EQUIPMENTS; j++)
        //   {
        //      w = gpGlobals->g.PlayerRoles.rgwEquipment[j][i];

        //      if (w != 0)
        //      {
        //         gpGlobals->g.rgObject[w].item.wScriptOnEquip =
        //            PAL_RunTriggerScript(gpGlobals->g.rgObject[w].item.wScriptOnEquip, (WORD)i);
        //      }
        //   }
        //}
    }
}
