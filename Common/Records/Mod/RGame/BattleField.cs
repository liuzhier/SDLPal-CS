namespace Records.Mod.RGame;

public record class BattleField(
    string Name,
    ushort ScreenWave,
    ElementalResistance ElementalEffect
);
