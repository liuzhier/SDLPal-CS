using System.Text.Json.Serialization;

namespace SDLPal.Record.RGame;

public enum MagicType
{
    None        = 0,        // 无
    Wind        = 1,        // 风
    Thunder     = 2,        // 雷
    Water       = 3,        // 水
    Fire        = 4,        // 火
    Earth       = 5,        // 土
    Poison      = 6,        // 毒
    NonAttack   = 7,        // 非攻击性
}

public enum MagicActionType
{
    Normal          = 0,        // 攻击单人，单特效
    AttackAll       = 1,        // 攻击全体，三特效
    AttackWhole     = 2,        // 攻击全体，单特效
    AttackField     = 3,        // 攻击全体，整屏特效
    ApplyToPlayer   = 4,        // 我方单人，单特效
    ApplyToParty    = 5,        // 我方全体，三特效
    Trance          = 8,        // 施法者本身
    Summon          = 9,        // 召唤神
}

public record class Magic(
    string Name,                    // 名称
    string[]? Description,          // 描述
    ushort CostMP,                  // MP 损耗
    short BaseDamage,               // 基础伤害
    ushort SoundId,                 // 仙术音效
    MagicType Type,                 // 仙术系属
    MagicActionType ActionType,     // 行动类型
    MagicEffect Effect,             // 特效参数
    MagicScript Script,             // 脚本
    MagicScope Scope                // 作用域
);

public record class MagicEffect(
    short XOffset,          // X 轴偏移
    short YOffset,          // Y 轴偏移
    short LayerOffset,      // 图层偏移
                            // 实际图层 = Pos.Y + MagicBase1.YOffset + LayerOffset
    short Speed,            // 播放速度
    short KeepEffect,       // 是否将仙术动画最后一帧留在地面上（0 = 否 ）
    ushort SoundDelay,      // 音效延迟
    ushort EffectTimes,     // 重复次数（0 = 不重复）
    ushort Shake,           // 屏幕震动
    ushort Wave             // 屏幕波
);

public record class MagicScript
{
    [JsonIgnore]
    public ushort Success { get; set; } = 0;    // 后序脚本（前序脚本成功才执行）
    public string? SuccessTag { get; init; }

    [JsonIgnore]
    public ushort Use { get; set; } = 0;    // 前序脚本（一般用于检查是否具备施法条件）
    public string? UseTag { get; init; }

    [JsonIgnore]
    public ushort Description { get; set; } = 0;    // 描述脚本（不仅能显示描述，还能进行其他的奇葩操作）
    public string? DescriptionTag { get; init; }    // （如比如查看说明顺便全体回血，但是试了一下，没法中毒--）
}

public record class MagicScope(
    bool UsableOutsideBattle,
    bool UsableInBattle,
    bool UsableToEnemy,
    bool NeedSelectRole
);
