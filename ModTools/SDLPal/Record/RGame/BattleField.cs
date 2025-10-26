using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModTools.Record.Data;

namespace SDLPal.Record.RGame;

public record class BattleField(
    string Name,
    ushort ScreenWave,
    ElementalResistance ElementalEffect
);
