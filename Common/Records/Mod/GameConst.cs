using Records.Mod.RGame;
using System.Collections.Generic;

namespace Records.Mod;

public class GameConst
{
    public List<int[]> Shops { get; set; } = [null!];
    public List<int[]> EnemyTeams { get; set; } = [null!];
    public List<BattleField> BattleFields { get; set; } = [null!];
}
