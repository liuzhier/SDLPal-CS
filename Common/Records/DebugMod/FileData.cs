using Records.Pal;
using SimpleUtility;
using System;
using System.Runtime.InteropServices;

namespace Records.DebugMod;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PalFile : IDisposable
{
    public  FileBase        Base;
    public  FileCore        Core;

    public void Dispose()
    {
        Base.Dispose();
        Core.Dispose();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FileBase : IDisposable
{
    public  Base.CShop*                     Shop;
    public  int                             ShopCount;
    public  Base.CEnemy*                    Enemy;
    public  int                             EnemyCount;
    public  Base.CEnemyTeam*                EnemyTeam;
    public  int                             EnemyTeamCount;
    public  Base.CHero*                     Hero;
    public  Base.CMagic*                    Magic;
    public  int                             MagicCount;
    public  Base.CBattleField*              BattleField;
    public  int                             BattleFieldCount;
    public  Base.CLevelUpMagicGroup*        LevelUpMagicGroup;
    public  int                             LevelUpMagicGroupCount;
    public  Base.CHeroActionEffect*         HeroActionEffect;
    public  int                             HeroActionEffectCount;
    public  Base.CEnemyPositionGroup*       EnemyPositionGroup;
    public  Base.CLevelUpExp*               LevelUpExp;

    public void Dispose()
    {
        C.free(Shop);
        C.free(Enemy);
        C.free(EnemyTeam);
        C.free(Hero);
        C.free(Magic);
        C.free(BattleField);
        C.free(LevelUpMagicGroup);
        C.free(HeroActionEffect);
        C.free(EnemyPositionGroup);
        C.free(LevelUpExp);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FileCore : IDisposable
{
    public  Core.CEvent*        Event;
    public  int                 EventCount;
    public  Core.CScene*        Scene;
    public  int                 SceneCount;
    public  Entity.Dos*         EntityDos;
    public  Entity.Win*         EntityWin;
    public  int                 EntityCount;
    public  uint*               MessageIndex;
    public  int                 MessageIndexCount;
    public  Core.CScript*       Script;
    public  int                 ScriptCount;

    public void Dispose()
    {
        C.free(Event);
        C.free(Scene);
        C.free(EntityDos);
        C.free(EntityWin);
        C.free(MessageIndex);
        C.free(Script);
    }
}
