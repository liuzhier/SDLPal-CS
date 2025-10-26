using Lib.Mod;
using Lib.Pal;
using ModTools;
using SimpleUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Records.Pal.Core;
using static Records.Pal.Entity;
using RAddress = Records.Mod.RGame.Address;

namespace ModTools.Unpack;

public static unsafe class Script
{
    static HashSet<string[]> _functionNames = null!;

    /// <summary>
    /// 初始化脚本转换系统
    /// </summary>
    public static void Init()
    {
        string      pathOut, pathDependency, pathIn;

        //
        // 创建输出目录 Scirpt
        //
        pathOut = Config.ModWorkPath.Game.Data.Script;
        COS.Dir(pathOut);

        //
        // 复制脚本工作目录
        //
        pathDependency = "Dependency";
        pathIn = $@"{pathDependency}\Script";
        S.DirCopy(pathIn, "*", pathOut);
        S.DirCopy($@"{pathIn}\.vscode", "*", $@"{pathOut}\.vscode");
        S.DirCopy($@"{pathIn}\message", "*", $@"{pathOut}\message");
        S.DirCopy($@"{pathIn}\include", "*", $@"{pathOut}\include");

        //
        // 创建输出目录 Scirpt\src
        //
        COS.Dir($@"{pathOut}\src");

        //
        // 加载指令对应的函数名
        //
        S.JsonLoad(out _functionNames, $@"{pathDependency}\Func.json");

        //
        // 初始化脚本地址列表
        //
        Config.AddAddress(0, string.Empty);
    }

    /// <summary>
    /// 将 C# bool 转换为 typescript boolean
    /// </summary>
    /// <param name="value">待转换的布尔值</param>
    /// <returns>typescript boolean</returns>
    static string GetBoolean(bool value) => value ? "true" : "false";

    static readonly string[] _body = ["All", "Head", "Cloak", "Body", "Hand", "Foot", "Ornament", "Temp"];

    /// <summary>
    /// 获取 HeroBody 字符串
    /// </summary>
    /// <param name="bodyId">身体部位编号</param>
    /// <returns>HeroBody 字符串</returns>
    static string GetBody(int bodyId) => _body[bodyId];

    static readonly string[] _attribute = [
        "Attribute", "AvatarId", "SpriteIdInBattle", "SpriteId", "Name", "AttackAll",
        "_unknown1", "Level", "MaxHP", "MaxMP", "HP", "MP", "EquipHead", "EquipCloak",
        "EquipBody", "EquipHand", "EquipFoot", "EquipOrnament", "AttrAttackStrength",
        "AttrMagicStrength", "AttrDefense", "AttrDexterity", "AttrFleeRate",
        "ResistancePoison", "ResistanceWind", "ResistanceThunder", "ResistanceWater",
        "ResistanceFire", "ResistanceSoil", "CoveredBy", "_unknown2", "_unknown3", "_unknown4",
        "Magic1", "Magic2", "Magic3", "Magic4", "Magic5", "Magic6", "Magic7", "Magic8", "Magic9", "Magic10",
        "Magic11", "Magic12", "Magic13", "Magic14", "Magic15", "Magic16", "Magic17", "Magic18", "Magic19", "Magic20",
        "Magic21", "Magic22", "Magic23", "Magic24", "Magic25", "Magic26", "Magic27", "Magic28", "Magic29", "Magic30",
        "Magic31", "Magic32", "WalkFrames", "CooperativeMagic", "_unknown5", "_unknown6",
        "SoundDeath", "SoundAttack", "SoundWeapon", "SoundCritical", "SoundMagic", "SoundCover", "SoundDying"
    ];

    /// <summary>
    /// 获取 HeroAttribute 字符串
    /// </summary>
    /// <param name="attributeId">Hero 属性编号</param>
    /// <returns>HeroBody 字符串</returns>
    static string GetAttribute(int attributeId) => $"{_attribute[0]}.{_attribute[attributeId + 1]}";

    static readonly string[] _status = ["Status", "AttackFriends", "CannotAction", "Sleep", "CannotUseMagic", "DeceasedCanAttack", "MorePhysicalAttacks", "MoreDefense", "ActionsFaster", "DualAttack"];

    /// <summary>
    /// 获取 Status 字符串
    /// </summary>
    /// <param name="statusId">Status 编号</param>
    /// <returns>Status 字符串</returns>
    static string GetStatus(int statusId) => $"{_status[0]}.{_status[statusId + 1]}";

    static readonly string[] _battleResult = ["BattleResult", "逃离战斗", "脚本结束", "我方全灭", "战斗胜利"];

    /// <summary>
    /// 获取战斗结果字符串
    /// </summary>
    /// <param name="battleResultId">战斗结果编号</param>
    /// <returns>战斗结果字符串</returns>
    static string GetBattleResult(int battleResultId) => $"{_battleResult[0]}.{_battleResult[battleResultId + 2]}";

    static readonly string[][] _objectScript = [
        ["HeroScript", "FriendDeath", "Dying"],
        ["ItemScript", "Use", "Equip", "Throw", "Desc"],
        ["MagicScript", "Success", "Use", "Desc"],
        ["EnemyScript", "TurnStart", "BattleWon", "Action"],
        ["PoisonScript", "PlayerScript", "EnemyScript"],
    ];

    /// <summary>
    /// 获取对象脚本名称字符串
    /// </summary>
    /// <param name="objectTypeId">对象类型编号</param>
    /// <param name="scriptTypeId">对象脚本名称编号</param>
    /// <returns>对象脚本名称字符串</returns>
    static string GetObjectScript(int objectTypeId, int scriptTypeId) => $"{_objectScript[objectTypeId][0]}.{_objectScript[objectTypeId][scriptTypeId + 1]}";

    static List<string> _args { get; set; } = null!;

    /// <summary>
    /// 解档 Scirpt 条目
    /// </summary>
    public static void Process()
    {
        void AddAddr(CScriptArgs arg) => Config.AddAddress(arg.UShort);
        void Str(string text) => _args.Add(text);
        void TupleNumNum((short, short) texts) => _args.AddRange([texts.Item1.ToString(), texts.Item2.ToString()]);
        void TupleBoolNum((bool, short) texts) => _args.AddRange([GetBoolean(texts.Item1), texts.Item2.ToString()]);
        void Addr(CScriptArgs arg) => Str($"\"{arg.Addr}\"");
        void Num(CScriptArgs arg) => Str(arg.Short.ToString());
        void UNum(CScriptArgs arg) => Str(arg.UShort.ToString());
        void Bool(CScriptArgs arg) => Str(GetBoolean(arg.Bool));
        void BX(CScriptArgs arg) => Str(arg.BX.ToString());
        void BY(CScriptArgs arg) => Str(arg.BY.ToString());
        void BH(CScriptArgs arg) => Str(arg.BH.ToString());
        void X(CScriptArgs arg) => Str(arg.X.ToString());
        void Y(CScriptArgs arg) => Str(arg.Y.ToString());
        void SceneEvent(CScriptArgs arg) => TupleNumNum(arg.SceneEvent);
        void Scene(CScriptArgs arg) => Num(arg);
        void EventTrigger(CScriptArgs arg) => TupleBoolNum(arg.EventTrigger);
        void Direction(CScriptArgs arg) => Num(arg);
        void Party(CScriptArgs arg) => UNum(arg);
        void Role(CScriptArgs arg) => UNum(arg);
        void Hero(CScriptArgs arg) => UNum(arg);
        void HeroEntity(CScriptArgs arg) => Str(arg.HeroEntity.ToString());
        void ItemEntity(CScriptArgs arg) => Str(arg.ItemEntity.ToString());
        void MagicEntity(CScriptArgs arg) => Str(arg.MagicEntity.ToString());
        void EnemyEntity(CScriptArgs arg) => Str(arg.EnemyEntity.ToString());
        void PoisonEntity(CScriptArgs arg) => Str(arg.PoisonEntity.ToString());
        void Enemy(CScriptArgs arg) => Num(arg);
        void Body(CScriptArgs arg) => Str($"Body.{GetBody(arg.UShort - 0x000B + 1)}");
        void BodyUninstall(CScriptArgs arg) => Str($"UninstallEquip.{GetBody(arg.UShort)}");
        void EquipEffectType(CScriptArgs arg) => Str($"EquipEffectType.{GetBody(arg.UShort - 0x000B + 1)}");
        void Attr(CScriptArgs arg) => Str(GetAttribute(arg.UShort));
        void Status(CScriptArgs arg) => Str(GetStatus(arg.UShort));
        void Music(CScriptArgs arg) => Str(arg.Music);
        void CD(CScriptArgs arg) => Str(arg.CD);
        void PaletteTime(int paletteTimeId) => Str(paletteTimeId.ToString());
        void NpcChaseRange(int chaseRange) => Str(chaseRange.ToString());
        void Fbp(CScriptArgs arg) => Str(arg.Fbp);
        void BattleResult(CScriptArgs arg) => Str(GetBattleResult(arg.Short));
        void Palette(CScriptArgs arg) => Str(arg.Palette);
        void ObjectScript(int objectTypeId, CScriptArgs arg) => Str(GetObjectScript(objectTypeId, arg.UShort));

        string              pathOut, name;
        StreamWriter[]      fileScenes;
        StreamWriter        file;
        string?             scriptText;
        nint                pNative;
        CScript*            pScript, pThis;
        int                 i, count, progress, end, j;
        CScriptArgs*        pArgs;
        string[]?           names;
        bool                needSelectFile, isValidScript;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Scirpt>");

        //
        // 读取 Scirpt 数据
        //
        (pNative, count) = PalUtil.ReadMkfChunk(Config.FileCore, 4);
        pScript = (CScript*)pNative;
        count /= sizeof(CScript);

        //
        // 打开阉割语法后的 typescript 文件
        //
        pathOut = Config.ModWorkPath.Game.Data.Script;
        using StreamWriter filePublic = File.CreateText($@"{pathOut}\src\Public.ts");
        using StreamWriter fileHero = File.CreateText($@"{pathOut}\src\Hero.ts");
        using StreamWriter fileItem = File.CreateText($@"{pathOut}\src\Item.ts");
        using StreamWriter fileMagic = File.CreateText($@"{pathOut}\src\Magic.ts");
        using StreamWriter fileEnemy = File.CreateText($@"{pathOut}\src\Enemy.ts");
        using StreamWriter filePoison = File.CreateText($@"{pathOut}\src\Poison.ts");
        fileScenes = new StreamWriter[Records.Pal.Data.MaxScenes + 1];
        file = filePublic;

        //
        // 创建场景脚本目录
        //
        COS.Dir($@"{pathOut}\src\Scene");

        //
        // 处理 Scirpt
        //
        progress = (count / 10);
        end = count - 1;
        for (i = 1; i <= end; i++)
        {
            //
            // 输出处理进度
            //
            if (i % progress == 0 || i == end)
                Util.Log($"Unpack the game data. <Scirpt Addr: {((float)i / count * 100):f2}%>");

            //
            // 获取当前 Scirpt 条目
            //
            pThis = &pScript[i];
            pArgs = (CScriptArgs*)pThis->Args;
            names = _functionNames.First(x => x.Contains($"0x{pThis->Command:X4}"));
            name = names[0];

            switch (name)
            {
                case "ReplaceAndPauseWithNop":          // 0x0002
                case "GotoWithNop":                     // 0x0003
                case "Call":                            // 0x0004
                case "GotoWithSelect":                  // 0x000A
                case "CaptureTheEnemy":                 // 0x0033
                case "MakeElixir":                      // 0x0034
                case "SceneTeleport":                   // 0x0038
                case "RoleFleeBattle":                  // 0x003A
                case "JumpIfRoleNotPoisoned":           // 0x0061
                case "JumpIfEnemyTurn":                 // 0x0068
                case "JumpIfNotAllRolesFullHP":         // 0x0074
                case "JumpIfEnemyNotFirstOfKind":       // 0x0091
                    AddAddr(pArgs[0]);
                    break;

                case "GotoWithProbability":                 // 0x0006
                case "CashModify":                          // 0x001E
                case "EventSetAutoScript":                  // 0x0024
                case "EventSetTriggerScript":               // 0x0025
                case "JumpIfRoleNotPoisonedByKind":         // 0x005D
                case "JumpIfEnemyNotPoisonedByKind":        // 0x005E
                case "JumpIfEnemyHPMoreThanPercentage":     // 0x0064
                case "JumpIfHeroInParty":                   // 0x0079
                case "JumpIfCurrentSceneMatches":           // 0x0095
                case "EnemyClone":                          // 0x009C
                    AddAddr(pArgs[1]);
                    break;

                case "RemoveItem":                          // 0x0020
                case "JumpIfItemNotEquipped":               // 0x0086
                case "EnemySetStatus":                      // 0x002E
                case "JumpIfItemCountLessThan":             // 0x0058
                case "JumpIfPartyNotFacingEvent":           // 0x0081
                case "JumpIfEventNotInZone":                // 0x0083
                case "EventSetPosToPartyAndObstacle":       // 0x0084
                case "ObjectSetScript":                     // 0x0090
                case "JumpIfEventStateMatches":             // 0x0094
                case "EnemySummonMonster":                  // 0x009E
                    AddAddr(pArgs[2]);
                    break;

                case "BattleStart":         // 0x0007
                case "SceneSetScript":      // 0x006D
                    AddAddr(pArgs[1]);
                    AddAddr(pArgs[2]);
                    break;
            }
        }
        needSelectFile = true;
        isValidScript = true;
        for (i = 1; i <= end; i++)
        {
            //
            // 输出处理进度
            //
            if (i % progress == 0 || i == end)
                Util.Log($"Unpack the game data. <Scirpt: {((float)i / count * 100):f2}%>");

            //
            // 获取当前 Scirpt 条目
            //
            pThis = &pScript[i];
            pArgs = (CScriptArgs*)pThis->Args;

            //
            // 检查脚本地址是否被注册
            //
            if (Config.AddressDict.TryGetValue(i, out var address))
            {
                if (needSelectFile)
                {
                    needSelectFile = false;

                    file = address.Type switch
                    {
                        RAddress.AddrType.Public => filePublic,
                        RAddress.AddrType.Hero => fileHero,
                        RAddress.AddrType.Item => fileItem,
                        RAddress.AddrType.Magic => fileMagic,
                        RAddress.AddrType.Enemy => fileEnemy,
                        RAddress.AddrType.Poison => filePoison,
                        RAddress.AddrType.Scene => fileScenes[address.ObjectId] ??= new StreamWriter($@"{pathOut}\src\Scene\{address.ObjectId:D5}.ts"),
                        _ => throw new NotImplementedException(),
                    };
                }

                //
                // 写入脚本入口
                //
                file.WriteLine($"['{Config.AddAddress(i)}'];");

                //
                // 将后续指令标记为有效脚本
                //
                isValidScript = true;
            }

            scriptText = null;
            _args = [];
            names = _functionNames.First(x => x.Contains($"0x{pThis->Command:X4}"));
            name = names[0];
            switch (name)
            {
                case "Dialogue": // 0xFFFF
                    scriptText = $@"//{pArgs[0].Dialog}";
                    break;

                case "End": // 0x0000
                    scriptText = "";
                    needSelectFile = true;
                    break;

                case "ReplaceAndPause":             // 0x0001
                case "Replace":                     // 0x0008
                case "ShowSellItemMenu":            // 0x0027
                case "ScriptFailed":                // 0x0041
                case "NpcSetStillTime":             // 0x004B
                case "WaitForAnyKey":               // 0x004D
                case "LoadLastSave":                // 0x004E
                case "FadeToRed":                   // 0x004F
                case "RoleHalveHP":                 // 0x005A
                case "EnemyHalveHP":                // 0x005B
                case "KillRole":                    // 0x005F
                case "KillEnemy":                   // 0x0060
                case "BattleEnemyEscape":           // 0x0069
                case "BattleEnd":                   // 0x0078
                case "BattleEnableAuto":            // 0x008A
                case "VideoRestore":                // 0x008E
                case "CashHalve":                   // 0x008F
                case "ShowEndingAnimation":         // 0x0096
                case "QuitGame":                    // 0x00A0
                case "PartySetPosToFirstRole":      // 0x00A1
                case "ScreenBackup":                // 0x00A6
                    break;

                case "ReplaceAndPauseWithNop":      // 0x0002
                case "GotoWithNop":                 // 0x0003
                    Addr(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "Call": // 0x0004
                    Addr(pArgs[0]);
                    if (pArgs[1].Bool)
                        SceneEvent(pArgs[1]);
                    break;

                case "VideoUpdate": // 0x0005
                    UNum(pArgs[1]);
                    Bool(pArgs[2]);
                    break;

                case "GotoWithProbability":                 // 0x0006
                case "JumpIfEnemyHPMoreThanPercentage":     // 0x0064
                    UNum(pArgs[0]);
                    Addr(pArgs[1]);
                    break;

                case "BattleStart": // 0x0007
                    UNum(pArgs[0]);
                    Addr(pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "WaitEventAutoScriptRun": // 0x0009
                    UNum(pArgs[0]);
                    Bool(pArgs[1]);
                    Bool(pArgs[2]);
                    break;

                case "GotoWithSelect":              // 0x000A
                case "CaptureTheEnemy":             // 0x0033
                case "MakeElixir":                  // 0x0034
                case "SceneTeleport":               // 0x0038
                case "RoleFleeBattle":              // 0x003A
                case "JumpIfRoleNotPoisoned":       // 0x0061
                case "JumpIfEnemyTurn":             // 0x0068
                case "JumpIfNotAllRolesFullHP":     // 0x0074
                    Addr(pArgs[0]);
                    break;

                case "EventAnimate": // 0x000B, 0x000C, 0x000D, 0x000E, 0x0087
                    Direction(pArgs[0]);
                    break;

                case "NpcSetDirFrame": // 0x000F
                    Direction(pArgs[0]);
                    Num(pArgs[1]);
                    break;

                case "NpcMoveToBlock":              // 0x0010; 0x0082
                case "NpcMoveToBlockMutexLock":     // 0x0011; 0x007C
                case "RideNpcToPos":                // 0x003F; 0x0044; 0x0097
                case "PartySetPos":                 // 0x0046
                case "PartyWalkToBlock":            // 0x0070; 0x007A; 0x007B
                    BX(pArgs[0]);
                    BY(pArgs[1]);
                    BH(pArgs[2]);
                    switch (pThis->Command)
                    {
                        case 0x0010:
                            Str("3");
                            break;

                        case 0x0011:
                        case 0x003F:
                        case 0x0070:
                            Str("2");
                            break;

                        case 0x0044:
                        case 0x007A:
                        case 0x007C:
                            Str("4");
                            break;

                        case 0x007B:
                        case 0x0082:
                        case 0x0097:
                            Str("8");
                            break;
                    }
                    break;

                case "EventSetPosRelToParty":       // 0x0012
                case "EventSetPos":                 // 0x0013
                case "EventWalkOneStep":            // 0x006C
                case "EventModifyPos":              // 0x007D
                    SceneEvent(pArgs[0]);
                    X(pArgs[1]);
                    Y(pArgs[2]);
                    break;

                case "NpcSetFrame":                     // 0x0014
                case "ShowBuyItemMenu":                 // 0x0026
                case "RoleModifyBattleSpriteTemp":      // 0x0031
                case "SetRng":                          // 0x0036
                case "DrainHPFromEnemy":                // 0x0039
                case "SetDlgBox":                       // 0x003E
                case "PlaySound":                       // 0x0047
                case "FadeOut":                         // 0x0050
                case "NpcSetVanishTime":                // 0x0052
                case "SceneEnter":                      // 0x0059
                case "BattleRoleVanish":                // 0x005C
                case "BattleStealFromEnemy":            // 0x006A
                case "NpcChaseSetRange":                // 0x0062; 0x0063
                case "FadeToScene":                     // 0x0073; 0x009B
                case "MusicStop":                       // 0x0077
                case "Delay":                           // 0x0085
                case "RoleModifylevel":                 // 0x008D
                case "JumpToRandomInstruction":         // 0x00A2
                    switch (pThis->Command)
                    {
                        case 0x0062:
                            NpcChaseRange(0);
                            break;

                        case 0x0063:
                            NpcChaseRange(3);
                            break;
                    }
                    UNum(pArgs[0]);
                    switch (pThis->Command)
                    {
                        case 0x0073:
                            Str((-1).ToString());
                            break;

                        case 0x009B:
                            Num(pArgs[0]);
                            break;
                    }
                    break;

                case "RoleSetDirFrame": // 0x0015
                    Direction(pArgs[0]);
                    UNum(pArgs[1]);
                    Party(pArgs[2]);
                    break;

                case "EventSetDirFrame": // 0x0016
                    SceneEvent(pArgs[0]);
                    Direction(pArgs[1]);
                    UNum(pArgs[2]);
                    break;

                case "RoleSetEquipAttr": // 0x0017
                    EquipEffectType(pArgs[0]);
                    Attr(pArgs[1]);
                    Num(pArgs[2]);
                    break;

                case "RoleInstallEquip": // 0x0018
                    Body(pArgs[0]);
                    ItemEntity(pArgs[1]);
                    break;

                case "RoleModifyAttr":          // 0x0019
                case "RoleSetAttr":             // 0x001A
                case "RoleModifyAttrTemp":      // 0x0030
                    Attr(pArgs[0]);
                    Num(pArgs[1]);
                    Role(pArgs[2]);
                    break;

                case "RoleModifyHP":        // 0x001B
                case "RoleModifyMP":        // 0x001C
                case "RoleModifyHPMP":      // 0x001D
                case "EnemyModifyHP":       // 0x0021
                case "RoleRevive":          // 0x0022
                    Bool(pArgs[0]);
                    Num(pArgs[1]);
                    break;

                case "CashModify": // 0x001E
                    Num(pArgs[0]);
                    Addr(pArgs[1]);
                    break;

                case "AddItem":         // 0x001F
                    ItemEntity(pArgs[0]);
                    Num(pArgs[1]);
                    break;

                case "RemoveItem": // 0x0020
                case "JumpIfItemNotEquipped": // 0x0086
                    ItemEntity(pArgs[0]);
                    UNum(pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "RoleUninstallEquip": // 0x0023
                    Party(pArgs[0]);
                    BodyUninstall(pArgs[1]);
                    break;

                case "EventSetAutoScript":          // 0x0024
                case "EventSetTriggerScript":       // 0x0025
                    SceneEvent(pArgs[0]);
                    Addr(pArgs[1]);
                    break;

                case "EnemyApplyPoison":            // 0x0028
                case "RoleApplyPoison":             // 0x0029
                case "EnemyCurePoisonById":         // 0x002A
                case "RoleCurePoisonById":          // 0x002B
                    Bool(pArgs[0]);
                    PoisonEntity(pArgs[1]);
                    break;

                case "RoleCurePoisonBylevel":       // 0x002C
                    Bool(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "RoleSetStatus":       // 0x002D
                    Status(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "EnemySetStatus":      // 0x002E
                    Status(pArgs[0]);
                    UNum(pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "RoleRemoveStatus": // 0x002F
                    Status(pArgs[0]);
                    break;

                case "VideoShake":                  // 0x0035
                case "PartySetFollower":            // 0x0098
                    UNum(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "PlayRng":             // 0x0037
                case "PartySetRole":        // 0x0075
                    UNum(pArgs[0]);
                    UNum(pArgs[1]);
                    UNum(pArgs[2]);
                    break;

                case "SetDlgCenter": // 0x003B
                    UNum(pArgs[0]);
                    Bool(pArgs[2]);
                    break;

                case "SetDlgUpper":     // 0x003C
                case "SetDlgLower":     // 0x003D
                case "NpcChase":        // 0x004C
                    UNum(pArgs[0]);
                    UNum(pArgs[1]);
                    Bool(pArgs[2]);
                    break;

                case "EventSetTriggerMode": // 0x0040
                    SceneEvent(pArgs[0]);
                    EventTrigger(pArgs[1]);
                    break;

                case "SimulateRoleMagic": // 0x0042
                    MagicEntity(pArgs[0]);
                    Enemy(pArgs[1]);
                    Num(pArgs[2]);
                    break;

                case "MusicPlay": // 0x0043
                    Music(pArgs[0]);
                    Str(GetBoolean(pArgs[1].UShort != 1));
                    Str(GetBoolean(pArgs[1].UShort == 3));
                    break;

                case "SetBattleMusic": // 0x0045
                    Music(pArgs[0]);
                    break;

                case "EventSetState":       // 0x0049
                case "EventSyncState":      // 0x006F
                    SceneEvent(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "SetBattlefield": // 0x004A
                    Fbp(pArgs[0]);
                    break;

                case "FadeIn":                  // 0x0051
                case "BattleBlowAwayEnemy":     // 0x006B
                    Num(pArgs[0]);
                    break;

                case "SetPaletteTime": // 0x0053; 0x0054
                    switch (pThis->Command)
                    {
                        case 0x0053:
                            PaletteTime(0);
                            break;

                        case 0x0054:
                            PaletteTime(1);
                            break;
                    }
                    break;

                case "HeroAddMagic":        // 0x0055
                case "HeroRemoveMagic":     // 0x0056
                    MagicEntity(pArgs[0]);
                    Hero(pArgs[1]);
                    break;

                case "MagicSetBaseDamageByMP": // 0x0057
                    MagicEntity(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "JumpIfItemCountLessThan": // 0x0058
                    ItemEntity(pArgs[0]);
                    Num(pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "JumpIfRoleNotPoisonedByKind":         // 0x005D
                case "JumpIfEnemyNotPoisonedByKind":        // 0x005E
                    PoisonEntity(pArgs[0]);
                    Addr(pArgs[1]);
                    break;

                case "HeroSetSprite": // 0x0065
                    Hero(pArgs[0]);
                    UNum(pArgs[1]);
                    Bool(pArgs[2]);
                    break;

                case "RoleThrowWeapon": // 0x0066
                case "EnemySetMagic": // 0x0067
                    MagicEntity(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "SceneSetScript": // 0x006D
                    Scene(pArgs[0]);
                    Addr(pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "RoleMoveOneStep": // 0x006E
                    X(pArgs[0]);
                    Y(pArgs[1]);
                    UNum(pArgs[2]);
                    break;

                case "VideoWave": // 0x0071
                    UNum(pArgs[0]);
                    Num(pArgs[1]);
                    break;

                case "FadeFbp":         // 0x0076
                case "ScrollFbp":       // 0x00A4
                    Fbp(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "JumpIfHeroInParty": // 0x0079
                    HeroEntity(pArgs[0]);
                    Addr(pArgs[1]);
                    break;

                case "EventSetLayer": // 0x007E
                    SceneEvent(pArgs[0]);
                    Num(pArgs[1]);
                    break;

                case "ViewportMove": // 0x007F
                    X(pArgs[0]);
                    Y(pArgs[1]);
                    Num(pArgs[2]);
                    break;

                case "TogglePaletteTime": // 0x0080
                    Str(GetBoolean(!pArgs[0].Bool));
                    break;

                case "JumpIfPartyNotFacingEvent":           // 0x0081
                case "JumpIfEventNotInZone":                // 0x0083
                case "EventSetPosToPartyAndObstacle":       // 0x0084
                case "JumpIfEventStateMatches":             // 0x0094
                    SceneEvent(pArgs[0]);
                    UNum(pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "MagicSetBaseDamageByMoney": // 0x0088
                    MagicEntity(pArgs[0]);
                    break;

                case "BattleSetResult": // 0x0089
                    BattleResult(pArgs[0]);
                    break;

                case "SetPalette": // 0x008B
                    Palette(pArgs[0]);
                    break;

                case "FadeColor": // 0x008C
                    UNum(pArgs[0]);
                    UNum(pArgs[1]);
                    Bool(pArgs[2]);
                    break;

                case "ObjectSetScript": // 0x0090
                    if (pArgs[0].UShort < (ushort)BeginId.Hero)
                    {
                        //
                        // System
                        //
                        throw new Exception("The system object has no script.");
                    }
                    else if (pArgs[0].UShort < (ushort)BeginId.System2)
                    {
                        //
                        // Hero
                        //
                        name = "HeroSetScript";
                        j = 0;
                        HeroEntity(pArgs[0]);
                    }
                    else if (pArgs[0].UShort < (ushort)BeginId.Item)
                    {
                        //
                        // System 2
                        //
                        throw new Exception("The system object has no script.");
                    }
                    else if (pArgs[0].UShort < (ushort)BeginId.Magic)
                    {
                        //
                        // Item
                        //
                        name = "ItemSetScript";
                        j = 1;
                        ItemEntity(pArgs[0]);
                    }
                    else if (pArgs[0].UShort < (ushort)BeginId.Enemy)
                    {
                        //
                        // Magic
                        //
                        name = "MagicSetScript";
                        j = 2;
                        MagicEntity(pArgs[0]);
                    }
                    else if (pArgs[0].UShort < (ushort)BeginId.Poison)
                    {
                        //
                        // Enemy
                        //
                        name = "EnemySetScript";
                        j = 3;
                        EnemyEntity(pArgs[0]);
                    }
                    else
                    {
                        //
                        // Poison
                        //
                        name = "PoisonSetScript";
                        j = 4;
                        PoisonEntity(pArgs[0]);
                    }
                    ObjectScript(j, pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "JumpIfEnemyNotFirstOfKind": // 0x0091
                    Addr(pArgs[0]);
                    break;

                case "ShowRoleMagicAction": // 0x0092
                    Role(pArgs[0]);
                    break;

                case "VideoFadeAndUpdate": // 0x0093
                    Num(pArgs[0]);
                    break;

                case "JumpIfCurrentSceneMatches": // 0x0095
                    Scene(pArgs[0]);
                    Addr(pArgs[1]);
                    break;

                case "SceneSetMap": // 0x0099
                    Num(pArgs[0]);
                    UNum(pArgs[1]);
                    break;

                case "EventSetStateSequence": // 0x009A
                    SceneEvent(pArgs[0]);
                    SceneEvent(pArgs[1]);
                    UNum(pArgs[2]);
                    break;

                case "EnemyClone": // 0x009C
                    UNum(pArgs[0]);
                    Addr(pArgs[1]);
                    break;

                case "EnemySummonMonster": // 0x009E
                    Num(pArgs[0]);
                    UNum(pArgs[1]);
                    Addr(pArgs[2]);
                    break;

                case "EnemyTransform": // 0x009F
                    EnemyEntity(pArgs[0]);
                    break;

                case "PlayCDOrMusic": // 0x00A3
                    CD(pArgs[0]);
                    Music(pArgs[1]);
                    break;

                case "ShowFbpWithSprite": // 0x00A5
                    Fbp(pArgs[0]);
                    UNum(pArgs[1]);
                    UNum(pArgs[2]);
                    break;

                case "DlgItem": // 0x00A7
                    //
                    // 将后续指令标记为无效脚本
                    //
                    isValidScript = false;
                    break;
            }

            //
            // 跳过后续的无效脚本
            //
            if (!isValidScript)
                continue;

            //
            // 拼合脚本条目
            //
            scriptText ??= $"{name}({string.Join(", ", _args)});";

            //
            // 写入脚本条目
            //
            file.WriteLine(scriptText);
        }

        //
        // 关闭所有场景脚本文件
        //
        foreach (var fileScene in fileScenes)
            if (fileScene != null)
                fileScene.Dispose();

        //
        // 释放非托管内存
        //
        C.free(pNative);
    }
}
