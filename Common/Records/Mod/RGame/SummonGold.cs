namespace Records.Mod.RGame;

public record class SummonGold(
    string Name,                    // 名称
    string[]? Description,          // 描述
    ushort CostMP,                  // MP 损耗
    ushort BaseDamage,              // 基础伤害
    ushort SoundId,                 // 仙术音效
    MagicType Type,                 // 仙术系属
    SummonGoldEffect Effect,        // 特效参数
    MagicScript Script,             // 脚本
    MagicScope Scope                // 作用域
);

/// <summary>
/// 特效参数
/// </summary>
public record class SummonGoldEffect(
    ushort SpiritId,            // 召唤神形象
    ushort EffectId,            // 动画
    short XOffset,              // X 轴偏移
    short YOffset,              // Y 轴偏移
    ushort IdleFrames,          // 原地蠕动帧数
    ushort MagicFrames,         // 施法帧数
    ushort AttackFrames,        // 攻击帧数
    short ColorShift,           // 重复次数（0 = 不重复）
    ushort Shake,               // 屏幕震动
    ushort Wave                 // 屏幕波动
);
