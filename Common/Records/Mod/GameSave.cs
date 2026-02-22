using Records.Mod.RGame;
using Records.Pal;
using SDLPal;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Records.Mod;

public class GameSave : IDisposable
{
    public int SavedTimes { get; set; }                 // 存档次数
    public int Money { get; set; }                      // 现有金钱
    public int CollectValue { get; set; }               // 现有灵葫能量
    public int SceneId { get; set; } = 1;               // 当前场景编号
    public int MusicId { get; set; }                    // 场景音乐编号
    public int BattleMusicId { get; set; }              // 战斗音乐编号
    public int BattleFieldId { get; set; } = 1;         // 战斗环境（背景）编号
    public int HeroTeamLength { get; set; } = 1;        // 角色队伍人数
    public int HeroTeamLayerOffset { get; set; }        // 角色队伍图层偏移
    public int ChaseRange { get; set; } = 1;            // 敌人追击（警戒）范围
    public int ChaseCycles { get; set; }                // 敌人追击/昏厥的时间
    public int ItemCursorId { get; set; }               // 仓库光标位置
    public int[] HeroMagicCursorIds { get; set; } = new int[Base.MaxHero];          // 各 Hero 仙术光标位置
    public PalFilter TimeFilter { get; set; } = PalFilter.Noon;                     // 当前时间滤镜
    public ScreenWave SceneWave { get; set; } = new();                              // 场景扭曲参数
    public HeroTeam[] HeroTeams { get; set; } = new HeroTeam[Base.MaxHero];         // 角色队伍
    public List<Follower> Followers { get; set; } = [];                             // NPC 随从

    [JsonIgnore]
    public List<Inventory> Inventories { get; set; } = [];      // 库存数据
    [JsonIgnore]
    public Entity Entity { get; set; } = new();                 // 实体对象数据
    [JsonIgnore]
    public List<Scene> Scenes { get; set; } = [null!];          // 场景数据

    public GameSave()
    {
        //
        // 初始化队伍，默认全部为李逍遥
        //
        for (var i = 0; i < HeroTeams.Length; i++)
            HeroTeams[i] = new();
    }

    /// <summary>
    /// 销毁非托管资源
    /// </summary>
    ~GameSave() => Dispose();
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Entity.Dispose();
    }
}

public class Trail
{
    public Pos Pos { get; set; } = Pos.Zero;                                // 坐标
    public PalDirection Direction { get; set; } = PalDirection.South;       // 面朝方向
    public int FrameId { get; set; }                                        // 当前方向上的帧编号

    [JsonIgnore]
    public int WalkCount { get; set; }                  // 步数统计
    [JsonIgnore]
    public int SpriteFramesAuto { get; set; } = 1;      // 贴图的帧总数，自动计算得，自动脚本使用
    [JsonIgnore]
    public BlockPos BPos => BlockPos.FromPos(Pos);      // 地图区块的坐标
    [JsonIgnore]
    static Pos PosRatio { get; set; } = Pos.Zero;       // 坐标（按地图比率缩放后）
    [JsonIgnore]
    public Pos PosR
    {
        get
        {
            PosRatio.X = S.Ratio(Pos.X);
            PosRatio.Y = S.Ratio(Pos.Y);

            return PosRatio;
        }
    }
}

public unsafe class HeroTeam
{
    public int HeroId { get; set; } = 1;
    public Trail Trail { get; set; } = new();

    [JsonIgnore]
    public HeroBase Hero => S.Entity.Heroes[HeroId];
}

public class Follower
{
    public int SpriteId { get; set; }
    public Trail Trail { get; set; } = new();
}

public class Inventory(int ItemId, int Amount)
{
    public int ItemId { get; set; } = ItemId;       // 道具编号
    public int Amount { get; set; } = Amount;       // 道具数量
    public int AmountInUse { get; set; }            // 正在使用的数量（已经选好的投掷/使用动作等）

    [JsonIgnore]
    public Item Item => S.Entity.Items[ItemId];      // 道具实体对象
}

public unsafe class Entity : IDisposable
{
    public List<HeroBase> Heroes { get; set; } = [null!];

    public List<Magic> Magics { get; set; } = [null!];

    public List<Item> Items { get; set; } = [null!];

    public List<Enemy> Enemies { get; set; } = [null!];

    public List<Poison> Poisons { get; set; } = [null!];

    /// <summary>
    /// 销毁非托管资源
    /// </summary>
    ~Entity() => Dispose();
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var hero in Heroes)
            if (hero != null)
                C.free(hero.Raw);
    }
}
