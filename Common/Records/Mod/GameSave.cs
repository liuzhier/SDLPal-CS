using Records.Mod.RGame;
using Records.Pal;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json.Serialization;

namespace Records.Mod;

public class GameSave
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
    public PalFilter TimeFilter { get; set; } = PalFilter.Noon;                 // 当前时间滤镜
    public ScreenWave SceneWave { get; set; } = new();                          // 场景扭曲参数
    public Member[] Members { get; set; } = new Member[Base.MaxHero];           // 角色队伍
    public List<Follower> Followers { get; set; } = [];                         // NPC 随从

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
        for (var i = 0; i < Members.Length; i++) Members[i] = new();
    }

    /// <summary>
    ///   可以向库存中添加或移除道具
    /// </summary>
    /// <param name="itemId">道具实体编号</param>
    /// <param name="num">需要添加数量（正值）或需要删除的数量（负值）</param>
    /// 
    /// <returns>IsSuccess: 操作是否成功<br/>Count: 当需要删除的数量不足时，返回不足的数量</returns>
    public (bool IsSuccess, int Count) AddInventory(int itemId, int num)
    {
        var isSuccess = true;
        var count = 0;

        if (itemId == 0 || num == 0)
            //
            // 无需进行任何操作
            //
            goto Return;

        var inventories = Inventories;
        var item = inventories.Find(x => x.ItemId == itemId);

        if (num > 0)
        {
            //
            // 数量为正数，直接增加
            //
            if (item == null) inventories.Add(new(itemId, num));
            else item.Amount += num;

            isSuccess = true;
        }
        else
        {
            //
            // 剩下的情况只有负数了，现有数量减少
            //
            if (item != null)
            {
                isSuccess = item.Amount >= num;
                item.Amount += num;

                if (item.Amount <= 0)
                {
                    //
                    // 库存物品的数量可能不足，
                    // 所缺失的数量需要进行记录。
                    //
                    count = Math.Abs(item.Amount);
                    isSuccess = (count == 0);
                    inventories.Remove(item);
                }
            }

            //
            // 防止道具菜单下角标越界
            //
            var save = S.Save;
            save.ItemCursorId = int.Max(save.ItemCursorId, 0);
            save.ItemCursorId = int.Min(save.ItemCursorId, save.Inventories.Count - 1);
        }

    Return:
        return (isSuccess, count);
    }

    /// <summary>统计库存中以及玩家装备中指定的道具数量</summary>
    /// <param name="itemId">道具实体编号</param>
    /// <returns>道具数量</returns>
    public int GetInventoryCount(int itemId)
    {
        //
        // 在库存中查找指定的道具
        //
        var inventory = Inventories.Find(x => x.ItemId == itemId);
        var count = 0;
        if (inventory != null)
            //
            // 此物品存在于库存中
            //
            count += inventory.Amount;

        //
        // 在团队成员身上的装备中查找指定的道具
        //
        var item = S.Entity.Items[itemId];
        for (var i = 0; i < HeroTeamLength; i++)
        {
            var hero = Members[i].Hero;
            var equip = hero.Equipment;

            if (hero.Equipped(item)) count++;
        }

        return count;
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

public class Member
{
    public int HeroId { get; set; } = 1;
    public Trail Trail { get; set; } = new();

    [JsonIgnore]
    public Hero Hero => S.Entity.Heroes[HeroId];
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

public unsafe class Entity
{
    public List<Hero> Heroes { get; set; } = [null!];

    public List<Magic> Magics { get; set; } = [null!];

    public List<Item> Items { get; set; } = [null!];

    public List<Enemy> Enemies { get; set; } = [null!];

    public List<Poison> Poisons { get; set; } = [null!];
}
