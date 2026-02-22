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
        Save?.Dispose();
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
            Save.Inventories.Add(new(212, 1));
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
        int             i, j;
        string          pathFile;

        //Save.listHeroExperience = PalUnpak.Json2Obj<List<ExperienceAll>>($@"{DATA_PATH}\");

        var data = S.ModDataPath;

        //
        // 读取场景数据
        //
        var path = data.Scene;
        for (i = 1; ; i++)
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
            script.Enter = PalConfig.GetNewAddress(script.EnterTag);
            script.Teleport = PalConfig.GetNewAddress(script.TeleportTag);

            //
            // 读取事件数据
            //
            var events = scene.Events;
            for (j = 1; ; j++)
            {
                pathFile = $@"{pathFull}\{j:D5}.json";

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
                script2.Trigger = PalConfig.GetNewAddress(script2.TriggerTag);
                script2.Auto = PalConfig.GetNewAddress(script2.AutoTag);
            }
        }

        //
        // 读取 Hero 实体数据
        //
        var entity = Save.Entity;
        path = $@"{data.Entity}\Hero";
        for (i = 1; ; i++)
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

            //
            // 对象转非托管结构体数据
            //
            var equip = hero.Equipment;
            var attr = hero.BaseAttribute;
            var elemental = hero.Resistance.Elemental;
            var sound = hero.Sound;
            var heroBaseRaw = (HeroBaseRaw*)C.malloc(S.SizeOf<HeroBaseRaw>());
            *heroBaseRaw = new()
            {
                AvatarId = hero.AvatarId,
                SpriteIdInBattle = hero.SpriteIdInBattle,
                SpriteId = hero.SpriteId,
                AttackAll = hero.CanAttackAll ? 1 : 0,
                Level = hero.Level,
                MaxHP = hero.MaxHP,
                MaxMP = hero.MaxMP,
                HP = hero.HP,
                MP = hero.MP,
                Equipment = new()
                {
                    Head = equip.Head,
                    Cloak = equip.Cloak,
                    Body = equip.Body,
                    Hand = equip.Hand,
                    Foot = equip.Foot,
                    Ornament = equip.Ornament,
                },
                Attribute = new()
                {
                    AttackStrength = attr.AttackStrength,
                    MagicStrength = attr.MagicStrength,
                    Defense = attr.Defense,
                    Dexterity = attr.Dexterity,
                    FleeRate = attr.FleeRate,
                },
                PoisonResistance = hero.Resistance.Poison,
                ElementalResistance = new()
                {
                    Wind = elemental.Wind,
                    Thunder = elemental.Thunder,
                    Water = elemental.Water,
                    Fire = elemental.Fire,
                    Earth = elemental.Earth,
                },
                CoveredBy = hero.CoveredBy,
                FramesPerDirection = hero.FramesPerDirection,
                CooperativeMagic = hero.Magic.Cooperative,
                Sound = new()
                {
                    Death = sound.Death,
                    Attack = sound.Attack,
                    Weapon = sound.Weapon,
                    Critical = sound.Critical,
                    Magic = sound.Magic,
                    Cover = sound.Cover,
                    Dying = sound.Dying,
                },
            };

            entity.Heroes.Add(new()
            {
                Name = hero.Name,
                Raw = heroBaseRaw,
            });

            //
            // 将脚本标签转换为 ASM 地址
            //
            var script = entity.Heroes[^1].Script ??= new();
            script.FriendDeath = PalConfig.GetNewAddress(script.FriendDeathTag);
            script.Dying = PalConfig.GetNewAddress(script.DyingTag);
        }

        //
        // 读取 Magic 实体数据
        //
        path = $@"{data.Entity}\Magic";
        for (i = 1; ; i++)
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
            script.Success = PalConfig.GetNewAddress(script.SuccessTag);
            script.Use = PalConfig.GetNewAddress(script.UseTag);
        }

        //
        // 读取 Item 实体数据
        //
        path = $@"{data.Entity}\Item";
        for (i = 1; ; i++)
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
            script.Use = PalConfig.GetNewAddress(script.UseTag);
            script.Equip = PalConfig.GetNewAddress(script.EquipTag);
            script.Throw = PalConfig.GetNewAddress(script.ThrowTag);
        }

        //
        // 读取 Enemy 实体数据
        //
        path = $@"{data.Entity}\Enemy";
        for (i = 1; ; i++)
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
            script.TurnStart = PalConfig.GetNewAddress(script.TurnStartTag);
            script.BattleWon = PalConfig.GetNewAddress(script.BattleWonTag);
            script.Action = PalConfig.GetNewAddress(script.ActionTag);
        }

        //
        // 读取 Poison 实体数据
        //
        path = $@"{data.Entity}\Poison";
        for (i = 1; ; i++)
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
            script.Enemy = PalConfig.GetNewAddress(script.EnemyTag);
            script.Player = PalConfig.GetNewAddress(script.PlayerTag);
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
