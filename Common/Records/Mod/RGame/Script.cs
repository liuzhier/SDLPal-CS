using System.Security.AccessControl;

namespace Records.Mod.RGame;

public static class Script
{

    public enum ArgType : byte
    {
        Short,
        UShort,
        Bool,
        String,

        Address,
        X,
        Y,
        BX,
        BY,
        BH,
        Scene,
        Event,
        Direction,
    }

    public readonly static Dictionary<string, ArgType[]> FuncArgType = new()
    {
        ["End"] = [
            //
            // 0x0000
            //
        ],
        ["ReplaceAndPause"] = [
            //
            // 0x0001
            //
        ],
        ["ReplaceAndPauseWithNop"] = [
            //
            // 0x0002
            //
            ArgType.Address,        // ScrAddressess
            ArgType.UShort,         // Count
        ],
        ["GotoWithNop"] = [
            //
            // 0x0003
            //
            ArgType.Address,        // ScrAddressess
            ArgType.UShort,         // Count
        ],
        ["Call"] = [
            //
            // 0x0004
            //
            ArgType.Address,        // ScrAddressess
            ArgType.Scene,          // SceneID
            ArgType.Event,          // EventID
        ],
        ["VideoUpdate"] = [
            //
            // 0x0005
            //
            ArgType.UShort,     // Delay
            ArgType.Bool,       // UpdatePartyGestures
        ],
        ["GotoWithProbability"] = [
            //
            // 0x0006
            //
            ArgType.UShort,      // Probability
            ArgType.Address,     // ScrAddressess
        ],
        ["BattleStart"] = [
            //
            // 0x0007
            //
            ArgType.UShort,      // EnemyTeamID
            ArgType.Address,     // ScrDefeat
            ArgType.Address,     // ScrFleed
        ],
        ["Replace"] = [
            //
            // 0x0008
            //
        ],
        ["WaitEventAutoScriptRun"] = [
            //
            // 0x0009
            //
            ArgType.UShort,      // FrameNum
            ArgType.Bool,     // CanTriggerEvent
            ArgType.Bool,     // UpdatePartyGestures
        ],
        ["GotoWithSelect"] = [
            //
            // 0x000A
            //
            ArgType.Address,     // ScrAddressess
        ],
        ["EventAnimate"] = [
            //
            // 0x000B; 0x000C; 0x000D; 0x000E; 0x0087
            //
            ArgType.Direction,      // DirectionID
            ArgType.UShort,            // Speed
        ],
        ["NPCSetDirFrame"] = [
            //
            // 0x000F
            //
            ArgType.Direction,      // DirectionID
            ArgType.UShort,            // FrameID
        ],
        ["NPCMoveToBlock"] = [
            //
            // 0x0010; 0x0011; 0x007C; 0x0082
            //
            ArgType.BX,       // BX
            ArgType.BY,       // BY
            ArgType.BH,       // BH
            ArgType.UShort,      // Speed
            ArgType.Bool,     // Distinguish
        ],
        ["EventSetPosRelToParty"] = [
            //
            // 0x0012
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.X,        // X
            ArgType.Y,        // Y
        ],
        ["EventSetPos"] = [
            //
            // 0x0013
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.X,        // X
            ArgType.Y,        // Y
        ],
        ["NPCSetFrame"] = [
            //
            // 0x0014
            //
            ArgType.UShort,      // FrameID
        ],
        ["RoleSetDirFrame"] = [
            //
            // 0x0015
            //
            ArgType.UShort,            // PartyID
            ArgType.Direction,      // DirectionID
            ArgType.UShort,            // FrameID
        ],
        ["EventSetDirFrame"] = [
            //
            // 0x0016
            //
            ArgType.Scene,          // SceneID
            ArgType.Event,          // EventID
            ArgType.Direction,      // DirectionID
            ArgType.UShort,            // FrameID
        ],
        ["RoleSetAttrExtra"] = [
            //
            // 0x0017
            //
            ArgType.UShort,      // BodyID
            ArgType.UShort,      // AttrID
            ArgType.UShort,      // Value
        ],
        ["RoleInstallEquip"] = [
            //
            // 0x0018
            //
            ArgType.UShort,      // ItemID
            ArgType.UShort,      // BodyID
        ],
        ["RoleModifyAttr"] = [
            //
            // 0x0019
            //
            ArgType.UShort,      // RoleID
            ArgType.UShort,      // AttrID
            ArgType.UShort,      // Value
        ],
        ["RoleSetAttr"] = [
            //
            // 0x001A
            //
            ArgType.UShort,      // RoleID
            ArgType.UShort,      // AttrID
            ArgType.UShort,      // Value
        ],
        ["RoleModifyHP"] = [
            //
            // 0x001B
            //
            ArgType.UShort,      // Value
            ArgType.Bool,     // ApplyToAll
        ],
        ["RoleModifyMP"] = [
            //
            // 0x001C
            //
            ArgType.UShort,      // Value
            ArgType.Bool,     // ApplyToAll
        ],
        ["RoleModifyHPMP"] = [
            //
            // 0x001D
            //
            ArgType.UShort,      // Value
            ArgType.Bool,     // ApplyToAll
        ],
        ["CashModify"] = [
            //
            // 0x001E
            //
            ArgType.UShort,      // Value
            ArgType.Address,     // ScrAddressess
        ],
        ["AddItem"] = [
            //
            // 0x001F
            //
            ArgType.UShort,      // ItemID
            ArgType.UShort,      // Value
        ],
        ["RemoveItem"] = [
            //
            // 0x0020
            //
            ArgType.UShort,      // ItemID
            ArgType.UShort,      // Value
            ArgType.Address,     // ScrAddressess
        ],
        ["EnemyModifyHP"] = [
            //
            // 0x0021
            //
            ArgType.UShort,      // Value
            ArgType.Bool,     // ApplyToAll
        ],
        ["RoleRevive"] = [
            //
            // 0x0022
            //
            ArgType.UShort,      // Value
            ArgType.Bool,     // ApplyToAll
        ],
        ["RoleUninstallEquip"] = [
            //
            // 0x0023
            //
            ArgType.UShort,      // RoleID
            ArgType.UShort,      // BodyID
        ],
        ["EventSetAutoScript"] = [
            //
            // 0x0024
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.Address,     // ScrAddressess
        ],
        ["EventSetTriggerScript"] = [
            //
            // 0x0025
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.Address,     // ScrAddressess
        ],
        ["ShowBuyItemMenu"] = [
            //
            // 0x0026
            //
            ArgType.UShort,      // StoreID
        ],
        ["ShowSellItemMenu"] = [
            //
            // 0x0027
            //
        ],
        ["EnemyApplyPoison"] = [
            //
            // 0x0028
            //
            ArgType.UShort,      // PoisonID
            ArgType.Bool,     // ApplyToAll
        ],
        ["RoleApplyPoison"] = [
            //
            // 0x0029
            //
            ArgType.UShort,      // PoisonID
            ArgType.Bool,     // ApplyToAll
        ],
        ["EnemyCurePoisonByID"] = [
            //
            // 0x002A
            //
            ArgType.UShort,      // PoisonID
            ArgType.Bool,     // ApplyToAll
        ],
        ["RoleCurePoisonByID"] = [
            //
            // 0x002B
            //
            ArgType.UShort,      // PoisonID
            ArgType.Bool,     // ApplyToAll
        ],
        ["RoleCurePoisonByLevel"] = [
            //
            // 0x002C
            //
            ArgType.UShort,      // Level
            ArgType.Bool,     // ApplyToAll
        ],
        ["RoleSetStatus"] = [
            //
            // 0x002D
            //
            ArgType.Bool,     // IsGood
            ArgType.UShort,      // StatusID
            ArgType.UShort,      // Value
        ],
        ["EnemySetStatus"] = [
            //
            // 0x002E
            //
            ArgType.Bool,     // IsGood
            ArgType.UShort,      // StatusID
            ArgType.UShort,      // Value
        ],
        ["RoleRemoveStatus"] = [
            //
            // 0x002F
            //
            ArgType.Bool,     // IsGood
            ArgType.UShort,      // StatusID
        ],
        ["RoleModifyAttrTemp"] = [
            //
            // 0x0030
            //
            ArgType.UShort,      // RoleID
            ArgType.UShort,      // AttrID
            ArgType.UShort,      // Value
        ],
        ["RoleModifyBattleSpriteTemp"] = [
            //
            // 0x0031
            //
            ArgType.UShort,      // BattleSpriteID
        ],
        ["CaptureTheEnemy"] = [
            //
            // 0x0033
            //
            ArgType.Address,     // ScrAddressess
        ],
        ["MakeElixir"] = [
            //
            // 0x0034
            //
            ArgType.Address,     // ScrAddressess
        ],
        ["VideoShake"] = [
            //
            // 0x0035
            //
            ArgType.UShort,      // FrameNum
            ArgType.UShort,      // Level
        ],
        ["SetRNG"] = [
            //
            // 0x0036
            //
            ArgType.UShort,      // RNGID
        ],
        ["PlayRNG"] = [
            //
            // 0x0037
            //
            ArgType.UShort,      // BeginFrameID
            ArgType.UShort,      // EndFrameID
            ArgType.UShort,      // Speed
        ],
        ["SceneTeleport"] = [
            //
            // 0x0038
            //
            ArgType.Address,     // ScrAddressess
        ],
        ["DrainHPFromEnemy"] = [
            //
            // 0x0039
            //
            ArgType.UShort,      // Value
        ],
        ["RoleFleeBattle"] = [
            //
            // 0x003A
            //
            ArgType.Address,     // ScrAddressess
        ],
        ["SetDlgCenter"] = [
            //
            // 0x003B
            //
            ArgType.String,      // FaceID
            ArgType.UShort,      // ColorHex
            ArgType.Bool,     // RNGPlaying
        ],
        ["SetDlgUpper"] = [
            //
            // 0x003C
            //
            ArgType.String,      // FaceID
            ArgType.UShort,      // ColorHex
            ArgType.Bool,     // RNGPlaying
        ],
        ["SetDlgLower"] = [
            //
            // 0x003D
            //
            ArgType.String,      // FaceID
            ArgType.UShort,      // ColorHex
            ArgType.Bool,     // RNGPlaying
        ],
        ["SetDlgBox"] = [
            //
            // 0x003E
            //
            ArgType.UShort,      // ColorHex
        ],
        ["RideNPCToPos"] = [
            //
            // 0x003F; 0x0044; 0x0097
            //
            ArgType.BX,       // BX6
            ArgType.BY,       // BY
            ArgType.BH,       // BH
            ArgType.UShort,      // Speed
        ],
        ["EventSetTriggerMode"] = [
            //
            // 0x0040
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.Bool,     // IsAutoTrigger
            ArgType.UShort,      // TriggerDistance
        ],
        ["ScriptFailed"] = [
            //
            // 0x0041
            //
        ],
        ["SimulateRoleMagic"] = [
            //
            // 0x0042
            //
            ArgType.UShort,      // MagicID
            ArgType.UShort,      // EnemyID
            ArgType.UShort,      // Value
        ],
        ["MusicPlay"] = [
            //
            // 0x0043
            //
            ArgType.UShort,     // MusicID
            ArgType.Bool,       // Loop
        ],
        ["SetBattleMusic"] = [
            //
            // 0x0045
            //
            ArgType.UShort,      // MusicID
        ],
        ["PartySetPos"] = [
            //
            // 0x0046
            //
            ArgType.BX,    // BX
            ArgType.BY,    // BY
            ArgType.BH,    // BH
        ],
        ["PlaySound"] = [
            //
            // 0x0047
            //
            ArgType.UShort,      // SoundID
        ],
        ["EventSetState"] = [
            //
            // 0x0049
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.Bool,     // Display
            ArgType.Bool,     // IsObstacle
        ],
        ["SetBattlefield"] = [
            //
            // 0x004A
            //
            ArgType.UShort,      // BattlefieldID
        ],
        ["NPCSetStillTime"] = [
            //
            // 0x004B
            //
        ],
        ["NPCChase"] = [
            //
            // 0x004C
            //
            ArgType.UShort,      // Speed
            ArgType.UShort,      // Range
            ArgType.Bool,     // CanFly
        ],
        ["WaitForAnyKey"] = [
            //
            // 0x004D
            //
        ],
        ["LoadLastSave"] = [
            //
            // 0x004E
            //
        ],
        ["FadeToRed"] = [
            //
            // 0x004F
            //
        ],
        ["FadeOut"] = [
            //
            // 0x0050
            //
            ArgType.UShort,      // Delay
        ],
        ["FadeIn"] = [
            //
            // 0x0051
            //
            ArgType.UShort,      // Delay
        ],
        ["NPCSetVanishTime"] = [
            //
            // 0x0052
            //
            ArgType.UShort,      // FrameNum
        ],
        ["SetTimeFilter"] = [
            //
            // 0x0053; 0x0054
            //
            ArgType.UShort,      // TimeID
        ],
        ["RoleAddMagic"] = [
            //
            // 0x0055
            //
            ArgType.UShort,      // RoleID
            ArgType.UShort,      // MagicID
        ],
        ["RoleRemoveMagic"] = [
            //
            // 0x0056
            //
            ArgType.UShort,      // RoleID
            ArgType.UShort,      // MagicID
        ],
        ["MagicSetBaseDamageByMP"] = [
            //
            // 0x0057
            //
            ArgType.UShort,      // MagicID
            ArgType.UShort,      // Multiple
        ],
        ["JumpIfItemCountLessThan"] = [
            //
            // 0x0058
            //
            ArgType.UShort,      // ItemID
            ArgType.UShort,      // Value
            ArgType.Address,     // ScrAddressess
        ],
        ["SceneEnter"] = [
            //
            // 0x0059
            //
            ArgType.UShort,      // SceneID
        ],
        ["RoleHalveHP"] = [
            //
            // 0x005A
            //
        ],
        ["EnemyHalveHP"] = [
            //
            // 0x005B
            //
        ],
        ["BattleRoleVanish"] = [
            //
            // 0x005C
            //
            ArgType.UShort,      // Value
        ],
        ["JumpIfRoleNotPoisoned"] = [
            //
            // 0x005D
            //
            ArgType.UShort,      // PoisonID
            ArgType.Address,     // ScrAddressess
        ],
        ["JumpIfEnemyNotPoisoned"] = [
            //
            // 0x005E
            //
            ArgType.UShort,      // PoisonID
            ArgType.Address,     // ScrAddressess
        ],
        ["KillRole"] = [
            //
            // 0x005F
            //
        ],
        ["KillEnemy"] = [
            //
            // 0x0060
            //
        ],
        ["JumpIfRoleNotPoisonedByLevel"] = [
            //
            // 0x0061
            //
            ArgType.UShort,      // Level
            ArgType.Address,     // ScrAddressess
        ],
        ["NPCChaseSetRange"] = [
            //
            // 0x0062; 0x0063
            //
            ArgType.UShort,      // Level
            ArgType.UShort,      // FrameNum
        ],
        ["JumpIfEnemyHPMoreThanPercentage"] = [
            //
            // 0x0064
            //
            ArgType.UShort,      // Value
            ArgType.Address,     // ScrAddressess
        ],
        ["HeroSetSprite"] = [
            //
            // 0x0065
            //
            ArgType.UShort,      // HeroID
            ArgType.UShort,      // SpriteID
            ArgType.Bool,     // Update
        ],
        ["RoleThrowWeapon"] = [
            //
            // 0x0066
            //
            ArgType.UShort,      // MagicID
            ArgType.UShort,      // Value
        ],
        ["EnemySetMagic"] = [
            //
            // 0x0067
            //
            ArgType.UShort,      // MagicID
            ArgType.UShort,      // Value
        ],
        ["JumpIfEnemyTurn"] = [
            //
            // 0x0068
            //
            ArgType.Address,     // ScrAddressess
        ],
        ["BattleEnemyEscape"] = [
            //
            // 0x0069
            //
        ],
        ["BattleStealFromEnemy"] = [
            //
            // 0x006A
            //
            ArgType.UShort,      // Value
        ],
        ["BattleBlowAwayEnemy"] = [
            //
            // 0x006B
            //
            ArgType.UShort,      // FrameNum
        ],
        ["EventWalkOneStep"] = [
            //
            // 0x006C
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.X,        // X
            ArgType.Y,        // Y
        ],
        ["SceneSetScripts"] = [
            //
            // 0x006D
            //
            ArgType.UShort,      // SceneID
            ArgType.Address,     // ScrEnter
            ArgType.Address,     // ScrTeleport
        ],
        ["RoleMoveOneStep"] = [
            //
            // 0x006E
            //
            ArgType.X,        // X
            ArgType.Y,        // Y
            ArgType.UShort,      // Layer
        ],
        ["EventSyncState"] = [
            //
            // 0x006F
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.Bool,     // Display
            ArgType.Bool,     // IsObstacle
        ],
        ["PartyWalkToBlock"] = [
            //
            // 0x0070; 0x007A; 0x007B
            //
            ArgType.BX,       // BX
            ArgType.BY,       // BY
            ArgType.BH,       // BH
            ArgType.UShort,      // Speed
        ],
        ["VideoWave"] = [
            //
            // 0x0071
            //
            ArgType.UShort,      // Level
            ArgType.UShort,      // Progression
        ],
        ["FadeToScene"] = [
            //
            // 0x0073; 0x009B
            //
            ArgType.UShort,      // SceneID
            ArgType.UShort,      // Speed
        ],
        ["JumpIfNotAllRolesFullHP"] = [
            //
            // 0x0074
            //
            ArgType.Address,     // ScrAddressess
        ],
        ["PartySetRole"] = [
            //
            // 0x0075
            //
            ArgType.UShort,        // AllHeroID
        ],
        ["FadeFBP"] = [
            //
            // 0x0076
            //
            ArgType.UShort,      // FBPID
            ArgType.UShort,      // Speed
        ],
        ["MusicStop"] = [
            //
            // 0x0077
            //
        ],
        ["BattleEnd"] = [
            //
            // 0x0078
            //
        ],
        ["JumpIfHeroInParty"] = [
            //
            // 0x0079
            //
            ArgType.UShort,      // HeroID
            ArgType.Address,     // ScrAddressess
        ],
        ["EventModifyPos"] = [
            //
            // 0x007D
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.X,        // X
            ArgType.Y,        // Y
        ],
        ["EventSetLayer"] = [
            //
            // 0x007E
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.UShort,      // Layer
        ],
        ["ViewportMove"] = [
            //
            // 0x007F
            //
            ArgType.X,        // X
            ArgType.Y,        // Y
            ArgType.UShort,      // FrameNum
        ],
        ["ToggleDayNight"] = [
            //
            // 0x0080
            //
            ArgType.Bool,     // UpdateScene
        ],
        ["JumpIfPartyNotFacingEvent"] = [
            //
            // 0x0081
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.UShort,      // TriggerDistance
            ArgType.Address,     // ScrAddressess
        ],
        ["JumpIfEventNotInZone"] = [
            //
            // 0x0083
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.UShort,      // Range
            ArgType.Address,     // ScrAddressess
        ],
        ["EventSetPosToPartyAndObstacle"] = [
            //
            // 0x0084
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.Bool,     // Display
            ArgType.Bool,     // IsObstacle
            ArgType.Address,     // ScrAddressess
        ],
        ["Delay"] = [
            //
            // 0x0085
            //
            ArgType.UShort,      // Delay
        ],
        ["JumpIfItemNotEquipped"] = [
            //
            // 0x0086
            //
            ArgType.UShort,      // ItemID
            ArgType.UShort,      // Value
            ArgType.Address,     // ScrAddressess
        ],
        ["MagicSetBaseDamageByMoney"] = [
            //
            // 0x0088
            //
            ArgType.UShort,      // MagicID
        ],
        ["BattleSetResult"] = [
            //
            // 0x0089
            //
            ArgType.UShort,      // BattleResult
        ],
        ["BattleEnableAuto"] = [
            //
            // 0x008A
            //
        ],
        ["SetPalette"] = [
            //
            // 0x008B
            //
            ArgType.UShort,      // PaletteID
        ],
        ["FadeColor"] = [
            //
            // 0x008C
            //
            ArgType.UShort,      // Delay
            ArgType.UShort,      // ColorHex
            ArgType.Bool,     // IsFrom
        ],
        ["RoleModifyLevel"] = [
            //
            // 0x008D
            //
            ArgType.UShort,      // Value
        ],
        ["VideoRestore"] = [
            //
            // 0x008E
            //
        ],
        ["CashHalve"] = [
            //
            // 0x008F
            //
        ],
        ["ObjectSetScript"] = [
            //
            // 0x0090
            //
            ArgType.UShort,      // ObjectType
            ArgType.UShort,      // ObjectID
            ArgType.UShort,      // ScrType
            ArgType.Address,     // ScrAddressess
        ],
        ["JumpIfEnemyNotFirstOfKind"] = [
            //
            // 0x0091
            //
            ArgType.Address,     // ScrAddressess
        ],
        ["ShowRoleMagicAction"] = [
            //
            // 0x0092
            //
            ArgType.UShort,      // RoleID
        ],
        ["VideoFadeAndUpdate"] = [
            //
            // 0x0093
            //
            ArgType.UShort,      // Step
            ArgType.Bool,     // IsFadeOut
        ],
        ["JumpIfEventStateMatches"] = [
            //
            // 0x0094
            //
            ArgType.Scene,    // SceneID
            ArgType.Event,    // EventID
            ArgType.Bool,     // Display
            ArgType.Bool,     // IsObstacle
            ArgType.Address,     // ScrAddressess
        ],
        ["JumpIfCurrentSceneMatches"] = [
            //
            // 0x0095
            //
            ArgType.UShort,      // SceneID
            ArgType.Address,     // ScrAddressess
        ],
        ["ShowEndingAnimation"] = [
            //
            // 0x0096
            //
        ],
        ["PartySetFollower"] = [
            //
            // 0x0098
            //
            ArgType.UShort,     // SpriteID
            ArgType.UShort,     // SpriteID
        ],
        ["SceneSetMap"] = [
            //
            // 0x0099
            //
            ArgType.UShort,      // SceneID
            ArgType.UShort,      // MapID
        ],
        ["EventSetStateSequence"] = [
            //
            // 0x009A
            //
            ArgType.UShort,      // BeginSceneID
            ArgType.UShort,      // BeginEventID
            ArgType.UShort,      // EndSceneID
            ArgType.UShort,      // EndEventID
            ArgType.Bool,     // Display
            ArgType.Bool,     // IsObstacle
        ],
        ["EnemyClone"] = [
            //
            // 0x009C
            //
            ArgType.UShort,      // Value
            ArgType.Address,     // ScrAddressess
        ],
        ["EnemySummonMonster"] = [
            //
            // 0x009E
            //
            ArgType.UShort,      // EnemyID
            ArgType.UShort,      // Value
            ArgType.Address,     // ScrAddressess
        ],
        ["EnemyTransform"] = [
            //
            // 0x009F
            //
            ArgType.UShort,      // EnemyID
        ],
        ["QuitGame"] = [
            //
            // 0x00A0
            //
        ],
        ["PartySetPosToFirstRole"] = [
            //
            // 0x00A1
            //
        ],
        ["JumpToRandomInstruction"] = [
            //
            // 0x00A2
            //
            ArgType.UShort,      // Range
        ],
        ["PlayCDOrMusic"] = [
            //
            // 0x00A3
            //
            ArgType.UShort,      // CDID
            ArgType.UShort,      // MusicID
        ],
        ["ScrollFBP"] = [
            //
            // 0x00A4
            //
            ArgType.UShort,      // FBPID
            ArgType.UShort,      // Speed
        ],
        ["ShowFBPWithSprite"] = [
            //
            // 0x00A5
            //
            ArgType.UShort,      // FBPID
            ArgType.UShort,      // Speed
        ],
        ["ScreenBackup"] = [
            //
            // 0x00A6
            //
        ],
        ["DlgItem"] = [
            //
            // 0x00A7
            //
        ],
        ["Dlg"] = [
            //
            // 0xFFFF
            //
            ArgType.String,      // Msg
        ],
    };

}
