using Records.Mod.RGame;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Records.Mod;

public abstract class Fighter
{
    [JsonPropertyOrder(-1)]
    public string Name { get; set; } = null!;                               // 名字
    [JsonPropertyOrder(-1)]
    public int Exp { get; set; }                                            // 敌人：可得经验值；我方：当前经验值
    [JsonPropertyOrder(-1)]
    public int Level { get; set; }                                          // 修行
    [JsonPropertyOrder(-1)]
    public int SuperMagicId { get; set; }                                   // 敌人：当前法术；我方：合体法术
    [JsonPropertyOrder(-1)]
    public int AttackEquivItemId { get; set; }                              // 普攻附带道具
    [JsonPropertyOrder(-1)]
    public float AttackEquivItemRate { get; set; }                          // 普攻附带道具概率
    [JsonPropertyOrder(-1)]
    public FighterPower Power { get; set; } = null!;                        // HMP
    [JsonPropertyOrder(-1)]
    public FighterAttribute Attribute { get; set; } = null!;                // 五维（武灵防速逃）
    [JsonPropertyOrder(-1)]
    public FighterResistance Resistance { get; set; } = null!;              // 抗性
    [JsonPropertyOrder(-1)]
    public FighterSprite Sprite { get; set; } = null!;                      // 贴图参数
    [JsonPropertyOrder(-1)]
    public FighterVoice Voice { get; set; } = null!;                        // 行动音效
    [JsonPropertyOrder(-1)]
    public FighterStatus Status { get; set; } = new();                      // 特殊状态的回合数
    [JsonPropertyOrder(-1)]
    public List<FighterPoisonStatus> PoisonStatus { get; set; } = [];       // 中了那些毒

    /// <summary>
    /// 移除指定状态
    /// </summary>
    /// <param name="status">指定状态</param>
    public void RemoveStatus(PalStatus status)
    {
        if ((status & PalStatus.AttackFriends) != 0) Status.AttackFriends = 0;
        if ((status & PalStatus.CannotAction) != 0) Status.CannotAction = 0;
        if ((status & PalStatus.Sleep) != 0) Status.Sleep = 0;
        if ((status & PalStatus.CannotUseMagic) != 0) Status.CannotUseMagic = 0;
        if ((status & PalStatus.DeceasedCanAttack) != 0) Status.DeceasedCanAttack = 0;
        if ((status & PalStatus.MorePhysicalAttacks) != 0) Status.MorePhysicalAttacks = 0;
        if ((status & PalStatus.MoreDefense) != 0) Status.MoreDefense = 0;
        if ((status & PalStatus.ActionsFaster) != 0) Status.ActionsFaster = 0;
        if ((status & PalStatus.DualAttack) != 0) Status.DualAttack = 0;
    }

    /// <summary>
    /// 移除全部状态
    /// </summary>
    public void RemoveAllStatus() => Status = new();

    /// <summary>
    /// 清除指定的毒性
    /// </summary>
    /// <param name="poisonId">毒性实体编号</param>
    public void RemovePoisonStatus(int poisonId) => PoisonStatus.RemoveAll(x => x.PoisonId == poisonId);

    /// <summary>
    /// 清除指定烈度的毒性
    /// </summary>
    /// <param name="level">毒性烈度</param>
    public void RemovePoisonStatusByLevel(int level) => PoisonStatus.RemoveAll(x => x.Poison.Level == level);

    /// <summary>
    /// 清除所有的毒性
    /// </summary>
    public void RemoveAllPoisonStatus() => PoisonStatus.Clear();
}

public class FighterPower
{
    public int HP { get; set; }                 // 当前体力（生命）
    public int MaxHP { get; set; }              // 最大体力（生命）
    public int MP { get; set; } = -1;           // 当前真气（魔力）
    public int MaxMP { get; set; } = -1;        // 最大真气（魔力）
}

public class FighterAttribute
{
    public int PhysicalPower { get; set; }      // 武术
    public int MagicPower { get; set; }         // 灵力
    public int Defense { get; set; }            // 防御
    public int Dexterity { get; set; }          // 身法
    public int FleeRate { get; set; }           // 吉运
}

public class FighterResistance
{
    public float Physical { get; set; }     // 物抗
    public float Poison { get; set; }       // 毒抗
    public float Sorcery { get; set; }      // 巫抗
    public FighterElementalResistance Elemental { get; set; } = null!;      // 灵抗
}

public class FighterElementalResistance
{
    public float Wind { get; set; }         // 风抗
    public float Thunder { get; set; }      // 雷抗
    public float Water { get; set; }        // 水抗
    public float Fire { get; set; }         // 火抗
    public float Earth { get; set; }        // 土抗
}

public class FighterSprite
{
    public int SpriteIdInBattle { get; set; }       // 形象
    public int IdleFrames { get; set; } = 1;        // 原地蠕动帧数
    public int MagicFrames { get; set; } = 1;       // 施法帧数
    public int AttackFrames { get; set; } = 1;      // 攻击帧数
    public int IdleAnimSpeed { get; set; } = 1;     // 原地蠕动动画速度
    public int ActWaitFrames { get; set; } = 1;     // 行动动画（施法/攻击）每帧间固定帧间隔
    public int OffsetY { get; set; }                // Y 轴偏移
}

public class FighterVoice
{
    public int Attack { get; set; }     // 普攻
    public int Action { get; set; }     // 行动
    public int Magic { get; set; }      // 施法
    public int Death { get; set; }      // 阵亡
    public int Call { get; set; }       // 进入战场时呼喊
}

public class FighterStatus
{
    public int AttackFriends { get; set; }              // 疯魔（乱）
    public int CannotAction { get; set; }               // 定身（定）
    public int Sleep { get; set; }                      // 昏眠（眠）
    public int CannotUseMagic { get; set; }             // 咒封（封）
    public int DeceasedCanAttack { get; set; }          // 傀儡（暂时使死者继续普攻）
    public int MorePhysicalAttacks { get; set; }        // 天罡（暂时提升物理攻击）
    public int MoreDefense { get; set; }                // 金刚（暂时提升防御）
    public int ActionsFaster { get; set; }              // 仙风（暂时提升身法）
    public int DualAttack { get; set; }                 // 醉仙（两次普攻）
}

public class FighterPoisonStatus
{
    public int PoisonId { get; set; }
    [JsonIgnore]
    public Poison Poison => S.Entity.Poisons[PoisonId];
}
