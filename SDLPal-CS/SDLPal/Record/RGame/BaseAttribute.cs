namespace SDLPal.Record.RGame;

public record class BaseAttribute(
    ushort AttackStrength,      // 武术
    ushort MagicStrength,       // 灵力
    ushort Defense,             // 防御
    ushort Dexterity,           // 身法
    ushort FleeRate             // 吉运
);
