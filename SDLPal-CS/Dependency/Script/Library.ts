type Probability =
0   | 1  | 2  | 3  | 4  | 5  | 6  | 7  | 8  | 9  |
10  | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 |
20  | 21 | 22 | 23 | 24 | 25 | 26 | 27 | 28 | 29 |
30  | 31 | 32 | 33 | 34 | 35 | 36 | 37 | 38 | 39 |
40  | 41 | 42 | 43 | 44 | 45 | 46 | 47 | 48 | 49 |
50  | 51 | 52 | 53 | 54 | 55 | 56 | 57 | 58 | 59 |
60  | 61 | 62 | 63 | 64 | 65 | 66 | 67 | 68 | 69 |
70  | 71 | 72 | 73 | 74 | 75 | 76 | 77 | 78 | 79 |
80  | 81 | 82 | 83 | 84 | 85 | 86 | 87 | 88 | 89 |
90  | 91 | 92 | 93 | 94 | 95 | 96 | 97 | 98 | 99 |
100;

type BlockHalf = 0 | 1;

enum Direction{
   Current = -1,
   West,
   North,
   East,
   South,
}

enum NpcMoveToBlockSpeed {
   _3 = 3,
   _8 = 8,
}

enum NpcMoveToBlockMutexLockSpeed {
   _2 = 2,
   _4 = 4,
}

enum Hero{
   AvatarId,
   SpriteIdInBattle,
   SpriteId,
   Name,
   AttackAll,
   Level,
   MaxHP,
   MaxMP,
   HP,
   MP,
   EquipHead,
   EquipCloak,
   EquipBody,
   EquipHand,
   EquipFoot,
   EquipOrnament,
   AttrAttackStrength,
   AttrMagicStrength,
   AttrDefense,
   AttrDexterity,
   AttrFleeRate,
   ResistancePoison,
   ResistanceWind,
   ResistanceThunder,
   ResistanceWater,
   ResistanceFire,
   ResistanceSoil,
   CoveredBy,
   Magic1,
   Magic2,
   Magic3,
   Magic4,
   Magic5,
   Magic6,
   Magic7,
   Magic8,
   Magic9,
   Magic10,
   Magic11,
   Magic12,
   Magic13,
   Magic14,
   Magic15,
   Magic16,
   Magic17,
   Magic18,
   Magic19,
   Magic20,
   Magic21,
   Magic22,
   Magic23,
   Magic24,
   Magic25,
   Magic26,
   Magic27,
   Magic28,
   Magic29,
   Magic30,
   Magic31,
   Magic32,
   WalkFrames,
   CooperativeMagic,
   SoundDeath,
   SoundAttack,
   SoundWeapon,
   SoundCritical,
   SoundMagic,
   SoundCover,
   SoundDying,
}

/**
 * @note
 * -  0x0000
 * 
 * @deprecated
 * -  停止执行
 * 
 * @param void 无
 */
function End() { }

/**
 * @note
 * -  0x0001
 * 
 * @deprecated
 * -  停止执行，将 调用地址 替换为 下一条命令
 * 
 * @param void 无
 */
function ReplaceAndPause() {}

/**
 * @note
 * -  0x0002
 * 
 * @deprecated
 * -  停止执行，将 调用地址 替换为 地址 {scrAddress}；
 *    累计触发 {count} 次后，将 调用地址 替换为 下一条命令
 * 
 * @param scrAddress 欲跳转到的地址
 * @param count 最大可触发的次数，若设置为 0 则为总是可触发
 */
function ReplaceAndPauseWithNop(scrAddress: string, count: number){}

/**
 * @note
 * -  0x0003
 * 
 * @deprecated
 * -  跳转到地址 {scrAddress}，完成跳转地址的执行后脚本结束；
 *    累计触发 {count} 次后，该指令将成为 NOP
 * 
 * @param scrAddress 欲跳转到的地址
 * @param count 最大可触发的次数，若设置为 0 则为总是可触发
 */
function GotoWithNop(scrAddress: string, count: number) {}

/**
 * @note
 * -  0x0004
 * 
 * @deprecated
 * -  调用地址 {scrAddress}，完成调用后返回并继续执行；
 *    参数 {SceneId} 和 {EventId} 只有 Event 触发脚本时才用到；
 *    注意：不是只有玩家才能触发脚本，Event 也是可以主动触发脚本的！
 * 
 * @param scrAddress 欲跳转到的地址
 * @param sceneId 当前事件所在的场景Id
 * @param EventId 当前事件Id
 */
function Call(scrAddress: string, SceneId? : number, EventId?: number) {}

/**
 * @note
 * -  0x0005
 * 
 * @deprecated
 * -  重绘屏幕并清理对话
 * 
 * @param Delay 画面更新后延迟的毫秒数，缺省则为 1，实际延迟 {Delay * 60} ms
 * @param UpdatePartyGestures 当 RNG 未播放 或 未进入战斗时，是否更新队伍步伐
 */
function VideoUpdate(Delay: number, UpdatePartyGestures: boolean) {}

/**
 * @note
 * -  0x0006
 * 
 * @deprecated
 * -  生成随机数1~100，若大于 {probability}，则跳转到地址 {scrAddress}；
 *    否则继续执行。
 * 
 * @param probability 0~100 的数值
 * @param scrAddress 欲跳转到的地址
 */
function GotoWithprobability(probability: Probability, scrAddress: string) {}

/**
 * @note
 * -  0x0007
 * 
 * @deprecated
 * -  进入战斗，将敌方队列 {enemyTeamId} 放入战场；
 *    若战斗失败，则调用参数二指向的地址；
 *    若战斗胜利，则调用到参数三指向的地址
 * 
 * @param enemyTeamId 敌方队列 Id 
 * @param scrDefeat 战斗失败脚本
 * @param scrFleed 战斗逃跑脚本（Boss 战这里为 0，无法逃跑）
 */
function BattleStart(enemyTeamId: number, scrDefeat: string, scrFleed: string) {}

/**
 * @note
 * -  0x0008
 * 
 * @deprecated
 * -  将 调用地址 替换为 下一条命令
 * 
 * @param void 无
 */
function Replace() {}

/**
 * @note
 * -  0x0009
 * 
 * @deprecated
 * -  所有事件的自动脚本循环 {frameNum} 帧；
 *    期间禁止用户一切操作（包括触发事件），仅场景中的所有事件在运转
 * 
 * @param frameNum 循环帧数
 * @param canTriggerEvent 是否允许触发事件
 * @param updatePartyGestures 是否更新队伍步伐
 */
function WaitEventAutoScriptRun(frameNum: number, canTriggerEvent: boolean, updatePartyGestures: boolean) {}

/**
 * @note
 * -  0x000A
 * 
 * @deprecated
 * -  显示选项【"是" "否"】；
 *    选【"是"】则继续执行；
 *    选【"否"】则跳转到参数一指向的地址
 * 
 * @param scrAddress 选 "否" 则跳转到的地址
 */
function GotoWithSelect(scrAddress: string) {}

/**
 * @note
 * -  0x000B；
 *    0x000C；
 *    0x000D；
 *    0x000E；
 *    0x0087
 * 
 * @deprecated
 * -  当前事件播放 {DirectionId} 方向行走动画（X += 2 * 速度，Y += 1 * 速度）；
 *    0x000B：向西（左下）行，速度 2（X += 4，Y += 1）；
 *    0x000C：向北（左上）行，速度 2；
 *    0x000D：向东（右上）行，速度 2；
 *    0x000E：向南（右下）行，速度 2；
 *    0x0087：向当前方向行，速度 0（X += 0，Y += 0，原地播放行走动画）；
 * 
 * @param direction 行走方向（移动速度是定值）
 */
function EventAnimate(direction: Direction) {}

/**
 * @note
 * -  0x000F
 * 
 * @deprecated
 * -  当前对象面向方向 {direction}，帧编号 {frameId}
 * 
 * @param direction 方向
 * @param frameId 帧编号，大多数第一帧为原地静止，后两帧为行走
 */
function NpcSetDirFrame(direction: Direction, frameId: number) {}

/**
 * @note
 * -  0x0010；
 *    0x0082
 * 
 * @deprecated
 * -  当前对象走到块坐标 {BX, BY, BH}，待对象到达终点才会继续执行下一条指令；
 *    0x0010：速度 3;
 *    0x0082：速度 8;
 * 
 * @param BX 欲走到块的 X 坐标
 * @param BY 欲走到块的 Y 坐标
 * @param BH 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
 * @param speed 移动速度【选项：3 8】
 */
function NpcMoveToBlock(BX: number, BY: number, BH: BlockHalf, speed: NpcMoveToBlockSpeed) {} 

/**
 * @note
 *    0x0011；
 *    0x007C；
 * 
 * @deprecated
 * -  当前对象走到块坐标 {BX, BY, BH}，奇偶（帧）互斥，待对象到达终点才会继续执行下一条指令；
 *    0x0011：速度 2，奇偶互斥;
 *    0x007C：速度 4，奇偶互斥;
 * 
 * @param BX 欲走到块的 X 坐标
 * @param BY 欲走到块的 Y 坐标
 * @param BH 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
 * @param Speed 移动速度【选项：2 4】
 */
function NpcMoveToBlockMutexLock(BX: number, BY: number, BH: number, Speed: NpcMoveToBlockMutexLockSpeed) {}

/**
 * @note
 * -  0x0012
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的坐标设置到相对于 [队伍领队] 的块 {BX, BY, BH}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param X 欲走到的 X 坐标
 * @param Y 欲走到的 Y 坐标
 */
function EventSetPosRelToParty(SceneId: number, EventId: number, X: number, Y: number) {}

/**
 * @note
 * -  0x0013
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的坐标设置到绝对块 {BX, BY, BH}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param X 欲设置到的 X 坐标
 * @param Y 欲设置到的 Y 坐标
 */
function EventSetPos(SceneId, EventId, X, Y) {}

/**
 * @note
 * -  0x0014
 * 
 * @deprecated
 * -  将当前事件的帧号设置为 {FrameId}
 * 
 * @param FrameId 帧编号，第一帧为原地，后两帧为走路
 */
function NpcSetFrame(FrameId) {}

/**
 * @note
 * -  0x0015
 * 
 * @deprecated
 * -  将 {PartyId} 的方向设置为 {Direction}，帧号设置为 {FrameId}
 * 
 * @param PartyId 队伍中 Role 的编号
 * @param DirectionId 方向（负数则方向不变） 【西-左下[0] 北-左上[1] 东-右上[2] 南-右下[3]】
 * @param FrameId 帧编号，第一帧为原地，后两帧为走路
 */
function RoleSetDirFrame(PartyId, DirectionId, FrameId) {}

/**
 * @note
 * -  0x0016
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的方向设置为 {Direction}，帧号设置为 {FrameId}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param DirectionId 方向（负数则方向不变） 【西-左下[0] 北-左上[1] 东-右上[2] 南-右下[3]】
 * @param FrameId 帧编号，第一帧为原地，后两帧为走路
 */
function EventSetDirFrame(SceneId, EventId, DirectionId, FrameId) {}

/**
 * @note
 * -  0x0017
 * 
 * @deprecated
 * -  将当前队员 {BodyId} 部位的装备附带属性 {AttrId} 设置为 {Value}
 * 
 * @param BodyId 装备对应身体部位的编号
 * @param AttrId 属性编号
 * @param Value 欲设置的值
 */
function RoleSetAttrExtra(BodyId, AttrId, Value) {}

/**
 * @note
 * -  0x0018
 * 
 * @deprecated
 * -  当前队员装备道具 {ItemId}，装备到部位 {BodyId}
 * 
 * @param ItemId 欲装备道具的编号
 * @param BodyId 装备对应身体部位的编号
 */
function RoleInstallEquip(ItemId, BodyId) {}

/**
 * @note
 * -  0x0019
 * 
 * @deprecated
 * -  将队员 {RoleId} 的基础属性 {AttrId} 变动 {Value}
 * 
 * @param RoleId 队员编号，缺省表示当前队员
 * @param AttrId 属性编号
 * @param Value 欲变动的值，正数增加，负数减少
 */
function RoleModifyAttr(RoleId, AttrId, Value) {}

/**
 * @note
 * -  0x001A
 * 
 * @deprecated
 * -  将队员 {RoleId} 的基础属性 {AttrId} 设置为 {Value}
 * 
 * @param RoleId 队员编号，缺省表示当前队员
 * @param AttrId 属性编号
 * @param Value 欲设置的值
 */
function RoleSetAttr(RoleId, AttrId, Value) {}

/**
 * @note
 * -  0x001B
 * 
 * @deprecated
 * -  我方 {ApplyToAll} HP 变动 {Value}
 * 
 * @param Value 欲变动的值，正数增加，负数减少
 * @param ApplyToAll 是否作用于我方全体，缺省则为当前队员
 */
function RoleModifyHP(Value, ApplyToAll) {}

/**
 * @note
 * -  0x001C
 * 
 * @deprecated
 * -  我方 {ApplyToAll} MP 变动 {Value}
 * 
 * @param Value 欲变动的值，正数增加，负数减少
 * @param ApplyToAll 是否作用于我方全体，缺省则为当前队员
 */
function RoleModifyMP(Value, ApplyToAll) {}

/**
 * @note
 * -  0x001D
 * 
 * @deprecated
 * -  我方 {ApplyToAll} MP 变动 {Value}
 * 
 * @param Value 欲变动的值，正数增加，负数减少
 * @param ApplyToAll 是否作用于我方全体，缺省则为当前队员
 */
function RoleModifyHPMP(Value, ApplyToAll) {}

/**
 * @note
 * -  0x001E
 * 
 * @deprecated
 * -  金钱变动 {Value}
 * 
 * @param Value 欲变动的值，正数增加，负数减少，
 * @param scrAddress 金钱不足时跳转到的地址
 */
function CashModify(Value, scrAddress) {}

/**
 * @note
 * -  0x001F
 * 
 * @deprecated
 * -  将道具 {ItemId} 添加到背包中，数量 {Value}
 * 
 * @param ItemId 道具编号
 * @param Value 道具数量
 */
function AddItem(ItemId, Value) {}

/**
 * @note
 * -  0x0020
 * 
 * @deprecated
 * -  将道具 {ItemId} 在背包中删除，数量 {Value}，背包中道具不足，则会从角色身上卸下；
 *    若道具不足，则跳转到地址 {scrAddress}
 * 
 * @param ItemId 道具编号
 * @param Value 道具数量
 * @param scrAddress 道具不足时跳转到的地址
 */
function RemoveItem(ItemId, Value, scrAddress) {}

/**
 * @note
 * -  0x0021
 * 
 * @deprecated
 * -  敌方 {ApplyToAll} HP 变动 {Value}
 * 
 * @param Value 欲变动的值，正数增加，负数减少
 * @param ApplyToAll 是否作用于敌方全体，缺省则为当前敌人
 */
function EnemyModifyHP(Value, ApplyToAll) {}

/**
 * @note
 * -  0x0022
 * 
 * @deprecated
 * -  我方 {ApplyToAll} 复活，HP 恢复 {Value} %
 * 
 * @param Value 欲恢复 HP 的百分比
 * @param ApplyToAll 是否作用于我方全体，缺省则为当前队员
 */
function RoleRevive(Value, ApplyToAll) {}

/**
 * @note
 * -  0x0023
 * 
 * @deprecated
 * -  队员 {RoleId} 卸下部位 {BodyId} 的装备
 * 
 * @param RoleId 队员编号，缺省表示当前队员
 * @param BodyId 装备对应身体部位的编号
 */
function RoleUninstallEquip(RoleId, BodyId) {}

/**
 * @note
 * -  0x0024
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的自动脚本设置为 {scrAddress}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param scrAddress 欲设置的脚本地址
 */
function EventSetAutoScript(SceneId, EventId, scrAddress) {}

/**
 * @note
 * -  0x0025
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的触发脚本设置为 {scrAddress}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param scrAddress 欲设置的脚本地址
 */
function EventSetTriggerScript(SceneId, EventId, scrAddress) {}

/**
 * @note
 * -  0x0026
 * 
 * @deprecated
 * -  显示购买道具菜单 {StoreId}
 * 
 * @param StoreId 店铺编号
 */
function ShowBuyItemMenu(StoreId) {}

/**
 * @note
 * -  0x0027
 * 
 * @deprecated
 * -  显示出售道具菜单
 * 
 * @param void 无
 */
function ShowSellItemMenu() {}

/**
 * @note
 * -  0x0028
 * 
 * @deprecated
 * -  敌方 {ApplyToAll} 中毒 {PoisonId}
 * 
 * @param PoisonId 毒性编号
 * @param ApplyToAll 是否作用于敌方全体，缺省则为当前敌人
 */
function EnemyApplyPoison(PoisonId, ApplyToAll) {}

/**
 * @note
 * -  0x0029
 * 
 * @deprecated
 * -  我方 {ApplyToAll} 中毒 {PoisonId}
 * 
 * @param PoisonId 毒性编号
 * @param ApplyToAll 是否作用于我方全体，缺省则为当前队员
 */
function RoleApplyPoison(PoisonId, ApplyToAll) {}

/**
 * @note
 * -  0x002A
 * 
 * @deprecated
 * -  敌方 {ApplyToAll} 解毒 {PoisonId}
 * 
 * @param PoisonId 毒性编号
 * @param ApplyToAll 是否作用于敌方全体，缺省则为当前敌人
 */
function EnemyCurePoisonById(PoisonId, ApplyToAll) {}

/**
 * @note
 * -  0x002B
 * 
 * @deprecated
 * -  我方 {ApplyToAll} 解毒 {PoisonId}
 * 
 * @param PoisonId 毒性编号
 * @param ApplyToAll 是否作用于我方全体，缺省则为当前队员
 */
function RoleCurePoisonById(PoisonId, ApplyToAll) {}

/**
 * @note
 * -  0x002C
 * 
 * @deprecated
 * -  我方 {ApplyToAll} 解级别 {Level} 以内的毒
 * 
 * @param Level 毒性等级
 * @param ApplyToAll 是否作用于我方全体，缺省则为当前队员
 */
function RoleCurePoisonByLevel(Level, ApplyToAll) {}

/**
 * @note
 * -  0x002D
 * 
 * @deprecated
 * -  当前队员获得 {IsGood} 状态 {StatusId}，持续 {Value} 回合
 * 
 * @param IsGood 是否为增益状态（true=Good，false=Bad）
 * @param StatusId 状态编号
 * @param Value 状态持续的回合数
 */
function RoleSetStatus(IsGood, StatusId, Value) {}

/**
 * @note
 * -  0x002E
 * 
 * @deprecated
 * -  当前敌人获得 {IsGood} 状态 {StatusId}，持续 {Value} 回合
 * 
 * @param IsGood 是否为增益状态（true=Good，false=Bad）
 * @param StatusId 状态编号
 * @param Value 状态持续的回合数
 */
function EnemySetStatus(IsGood, StatusId, Value) {}

/**
 * @note
 * -  0x002F
 * 
 * @deprecated
 * -  当前队员清除 {IsGood} 状态 {StatusId}
 * 
 * @param IsGood 是否为增益状态（true=Good，false=Bad）
 * @param StatusId 状态编号
 */
function RoleRemoveStatus(IsGood, StatusId) {}

/**
 * @note
 * -  0x0030
 * 
 * @deprecated
 * -  队员 {RoleId} 修改临时属性 {AttrId} 变动 {Value} %
 * 
 * @param RoleId 队员编号，缺省表示当前队员
 * @param AttrId 属性编号
 * @param Value 欲变动的百分比，正数增加，负数减少
 */
function RoleModifyAttrTemp(RoleId, AttrId, Value) {}

/**
 * @note
 * -  0x0031
 * 
 * @deprecated
 * -  临时将当前队员的战斗图像设置为 {BattleSpriteId}
 * 
 * 参数：BattleSpriteId 队员战斗形象编号
 */
function RoleModifyBattleSpriteTemp(BattleSpriteId) {}

/**
 * @note
 * -  0x0033
 * 
 * @deprecated
 * -  将当前敌人收入紫金葫芦；
 *    若收妖失败（敌人不存在灵葫能量），则跳转到地址 {scrAddress}
 * 
 * @param scrAddress 收妖失败时跳转到的地址
 */
function CaptureTheEnemy(scrAddress) {}

/**
 * @note
 * -  0x0034
 * 
 * @deprecated
 * -  将紫金葫芦中的灵葫能量炼化为丹药；
 *    若炼丹失败（灵葫能量不足），则跳转到地址 {scrAddress}
 * 
 * @param scrAddress 炼丹失败时跳转到的地址
 */
function MakeElixir(scrAddress) {}

/**
 * @note
 * -  0x0035
 * 
 * @deprecated
 * -  画面震动 {FrameNum} 帧，振幅 {Level} 级
 * 
 * @param FrameNum 震动帧数
 * @param Level 震动等级
 */
function VideoShake(FrameNum, Level) {}

/**
 * @note
 * -  0x0036
 * 
 * @deprecated
 * -  将 RNG 动画设置为 {RNGId}
 * 
 * @param RNGId RNG 动画编号
 */
function SetRNG(RNGId) {}

/**
 * @note
 * -  0x0037
 * 
 * @deprecated
 * -  播放 RNG 动画，从第 {BeginFrameId} 帧开始播放，播放到 {EndFrameId} 帧终止，速度 {Speed}
 * 
 * @param BeginFrameId 起始帧号
 * @param EndFrameId 结束帧号，缺省则为播放到最后一帧
 * @param Speed 播放速度
 */
function PlayRNG(BeginFrameId, EndFrameId, Speed) {}

/**
 * @note
 * -  0x0038
 * 
 * @deprecated
 * -  土遁，脱离迷宫用。若土遁失败，则跳转到 {scrAddress}
 * 
 * @param scrAddress 土遁失败时跳转到的地址
 */
function SceneTeleport(scrAddress) {}

/**
 * @note
 * -  0x0039
 * 
 * @deprecated
 * -  吸取受害者 {Value} 点 HP 补充给施暴者，可我对敌，亦可敌对我
 * 
 * @param void 无
 */
function DrainHPFromEnemy(Value) {}

/**
 * @note
 * -  0x003A
 * 
 * @deprecated
 * -  非BOSS战逃跑。若逃跑失败，则跳转到 {scrAddress}
 * 
 * @param scrAddress 逃跑失败时跳转到的地址
 */
function RoleFleeBattle(scrAddress) {}

/**
 * @note
 * -  0x003B
 * 
 * @deprecated
 * -  将对话框位置设置在画面中间，头像为 {FaceId}，字体颜色为 {ColorHex}
 * 
 * @param FaceId 对话框上肖像的编号
 * @param ColorHex Hex 颜色值
 * @param RNGPlaying 是否正在播放 RNG 动画
 */
function SetDlgCenter(FaceId, ColorHex, RNGPlaying) {}

/**
 * @note
 * -  0x003C
 * 
 * @deprecated
 * -  将对话框位置设置在画面上部，头像为 {FaceId}，字体颜色为 {ColorHex}
 * 
 * @param FaceId 对话框上肖像的编号
 * @param ColorHex Hex 颜色值
 * @param RNGPlaying 是否正在播放 RNG 动画
 */
function SetDlgUpper(FaceId, ColorHex, RNGPlaying) {}

/**
 * @note
 * -  0x003D
 * 
 * @deprecated
 * -  将对话框位置设置在画面下部，头像为 {FaceId}，字体颜色为 {ColorHex}
 * 
 * @param FaceId 对话框上肖像的编号
 * @param ColorHex Hex 颜色值
 * @param RNGPlaying 是否正在播放 RNG 动画
 */
function SetDlgLower(FaceId, ColorHex, RNGPlaying) {}

/**
 * @note
 * -  0x003E
 * 
 * @deprecated
 * -  将对话框位置设置在画面中间的盒子里，字体颜色为 {ColorHex}
 * 
 * @param ColorHex Hex 颜色值
 */
function SetDlgBox(ColorHex) {}

/**
 * @note
 * -  0x003F；
 *    0x0044；
 *    0x0097
 * 
 * @deprecated
 * -  乘坐当前事件行至块坐标 {BX, BY, BH}；
 *    0x003F：速度 2;
 *    0x0044：速度 4;
 *    0x0097：速度 8;
 * 
 * @param BX 欲走到块的 X 坐标
 * @param BY 欲走到块的 Y 坐标
 * @param BH 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
 * @param Speed 行驶速度
 */
function RideNpcToPos(BX, BY, BH, Speed) {}

/**
 * @note
 * -  0x0040
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的触发方式设置为 {IsAutoTrigger}，触发范围为 {TriggerDistance}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param IsAutoTrigger 是否走到触发范围内自动触发
 * @param TriggerDistance 触发范围
 */
function EventSetTriggerMode(SceneId, EventId, IsAutoTrigger, TriggerDistance) {}

/**
 * @note
 * -  0x0041
 * 
 * @deprecated
 * -  将脚本标记为失败；
 *    通常用于仙术的 Use[条件检测] 脚本，标记为失败则不再执行 Success 脚本
 * 
 * @param void 无
 */
function ScriptFailed() {}

/**
 * @note
 * -  0x0042
 * 
 * @deprecated
 * -  模拟我方仙术 {MagicId}，攻击敌人 {EnemyId}，基础伤害为 {Value}；
 * 
 * @param MagicId 欲模拟的仙术编号
 * @param EnemyId 敌人编号，缺省则为当前敌人
 * @param Value 基础伤害
 */
function SimulateRoleMagic(MagicId, EnemyId, Value) {}

/**
 * @note
 * -  0x0043
 * 
 * @deprecated
 * -  设置场景音乐为 {MusicId}，循环播放 {Loop}，淡入淡出时间为 {FadeTime}
 * 
 * @param MusicId 欲播放的音乐编号
 * @param Loop 是否循环播放
 * @param FadeTime 淡入淡出时间；音乐 9 自带淡入淡出，无需淡入
 */
function MusicPlay(MusicId, Loop, FadeTime) {}

/**
 * @note
 * -  0x0045
 * 
 * @deprecated
 * -  设置战斗音乐为 {MusicId}
 * 
 * @param MusicId 欲播放的音乐编号
 */
function SetBattleMusic(MusicId) {}

/**
 * @note
 * -  0x0046
 * 
 * @deprecated
 * -  队伍走到块坐标 {BX, BY, BH}
 * 
 * @param BX 欲走到块的 X 坐标
 * @param BY 欲走到块的 Y 坐标
 * @param BH 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
 */
function PartySetPos(BX, BY, BH) {}

/**
 * @note
 * -  0x0047
 * 
 * @deprecated
 * -  播放音效 {SoundId}
 * 
 * @param SoundId 欲播放音效的编号
 */
function PlaySound(SoundId) {}

/**
 * @note
 * -  0x0049
 * 
 * @deprecated
 * -  设置事件 {SceneId EventId} 的显示状态为 {Display}，阻碍队伍通行 {IsObstacle}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param Display 显示状态
 * @param IsObstacle 阻碍队伍通行
 */
function EventSetState(SceneId, EventId, Display, IsObstacle) {}

/**
 * @note
 * -  0x004A
 * 
 * @deprecated
 * -  设置战斗环境为 {BattlefieldId}
 * 
 * @param BattlefieldId 战斗环境编号
 */
function SetBattlefield(BattlefieldId) {}

/**
 * @note
 * -  0x004B
 * 
 * @deprecated
 * -  当前事件静止不动一段时间，期间无法互动
 * 
 * @param void 无
 */
function NpcSetStillTime() {}

/**
 * @note
 * -  0x004C
 * 
 * @deprecated
 * -  当前事件追逐队伍，追逐速度 {Speed}，警戒范围 {Range}
 * 
 * @param Speed 追逐速度，缺省则为 4
 * @param Range 警戒范围，队伍走范围内开始被追逐，缺省则为 8
 * @param CanFly 能够穿墙追逐角色
 */
function NpcChase(Speed, Range, CanFly) {}

/**
 * @note
 * -  0x004D
 * 
 * @deprecated
 * -  等待按下任意键
 * 
 * @param void 无
 */
function WaitForAnyKey() {}

/**
 * @note
 * -  0x004E
 * 
 * @deprecated
 * -  读取最近的一次存档
 * 
 * @param void 无
 */
function LoadLastSave() {}

/**
 * @note
 * -  0x004F
 * 
 * @deprecated
 * -  屏幕泛红（我方全灭时用）
 * 
 * @param void 无
 */
function FadeToRed() {}

/**
 * @note
 * -  0x0050
 * 
 * @deprecated
 * -  屏幕淡出
 * 
 * @param Delay 每一步的延迟时间，默认 600 ms
 */
function FadeOut(Delay) {}

/**
 * @note
 * -  0x0051
 * 
 * @deprecated
 * -  屏幕淡入
 * 
 * @param Delay 每一步的延迟时间，默认 600 ms
 */
function FadeIn(Delay) {}

/**
 * @note
 * -  0x0052
 * 
 * @deprecated
 * -  当前对象隐藏 {FrameNum} 帧
 * 
 * @param FrameNum 欲隐藏的帧数，缺省则为 800 帧
 */
function NpcSetVanishTime(FrameNum) {}

/**
 * @note
 * -  0x0053
 *    0x0054
 * 
 * @deprecated
 * -  设置时间滤镜；
 *    0x0053：白天；
 *    0x0054：晚上
 * 
 * @param TimeId 时间编号（0=白天 1=黄昏 2=晚上）
 */
function SetTimeFilter(TimeId) {}

/**
 * @note
 * -  0x0055
 * 
 * @deprecated
 * -  队员 {RoleId} 练成仙术 {MagicId}
 * 
 * @param RoleId 队员编号，缺省表示当前队员
 * @param MagicId 欲习得仙术的编号
 */
function RoleAddMagic(RoleId, MagicId) {}

/**
 * @note
 * -  0x0056
 * 
 * @deprecated
 * -  队员 {RoleId} 丧失仙术 {MagicId}
 * 
 * @param RoleId 队员编号，缺省表示当前队员
 * @param MagicId 欲丧失仙术的编号
 */
function RoleRemoveMagic(RoleId, MagicId) {}

/**
 * @note
 * -  0x0057
 * 
 * @deprecated
 * -  将仙术 {MagicId} 的基础伤害设置为 MP 的 {Multiple} 倍
 * 
 * @param MagicId 欲设置仙术的编号
 * @param Multiple 倍数
 */
function MagicSetBaseDamageByMP(MagicId, Multiple) {}

/**
 * @note
 * -  0x0058
 * 
 * @deprecated
 * -  如果库存中的道具 {ItemId} 少于 {Value} 个，则跳转到 {scrAddress}
 * 
 * @param ItemId 道具编号
 * @param Value 道具数量
 * @param scrAddress 道具不足时跳转到的地址
 */
function JumpIfItemCountLessThan(ItemId, Value, scrAddress) {}

/**
 * @note
 * -  0x0059
 * 
 * @deprecated
 * -  切换到场景 {SceneId}
 * 
 * @param SceneId 场景编号
 */
function SceneEnter(SceneId) {}

/**
 * @note
 * -  0x005A
 * 
 * @deprecated
 * -  当前队员 HP 减半
 * 
 * @param void 无
 */
function RoleHalveHP() {}

/**
 * @note
 * -  0x005B
 * 
 * @deprecated
 * -  当前敌人 HP 减半
 * 
 * @param void 无
 */
function EnemyHalveHP() {}

/**
 * @note
 * -  0x005C
 * 
 * @deprecated
 * -  我方在战斗中隐身，持续 {Value} 回合
 * 
 * @param Value 持续的回合数
 */
function BattleRoleVanish(Value) {}

/**
 * @note
 * -  0x005D
 * 
 * @deprecated
 * -  如果当前队员未中毒 {PoisonId}，则跳转到 {scrAddress}
 * 
 * @param PoisonId 毒性编号
 * @param scrAddress 未中毒则跳转到的地址
 */
function JumpIfRoleNotPoisoned(PoisonId, scrAddress) {}

/**
 * @note
 * -  0x005E
 * 
 * @deprecated
 * -  如果当前敌人未中毒 {PoisonId}，则跳转到 {scrAddress}
 * 
 * @param PoisonId 毒性编号
 * @param scrAddress 未中毒则跳转到的地址
 */
function JumpIfEnemyNotPoisoned(PoisonId, scrAddress) {}

/**
 * @note
 * -  0x005F
 * 
 * @deprecated
 * -  当前队员直接阵亡
 * 
 * @param void 无
 */
function KillRole() {}

/**
 * @note
 * -  0x0060
 * 
 * @deprecated
 * -  当前敌人直接阵亡
 * 
 * @param void 无
 */
function KillEnemy() {}

/**
 * @note
 * -  0x0061
 * 
 * @deprecated
 * -  如果当前队员未中毒，则跳转到 {scrAddress}
 * 
 * @param Level 毒性等级
 * @param scrAddress 未中指定等级以下的毒则跳转到的地址
 */
function JumpIfRoleNotPoisonedByLevel(Level, scrAddress) {}

/**
 * @note
 * -  0x0062；0x0063
 * 
 * @deprecated
 * -  事件暂停追逐队伍，持续 {FrameNum} 帧
 *    0x0062：警戒距离级别 0（停止追逐）
 *    0x0063：警戒距离级别 3
 * 
 * @param FrameNum 持续的帧数
 */
function NpcChaseSetRange(Level, FrameNum) {}

/**
 * @note
 * -  0x0064
 * 
 * @deprecated
 * -  如果当前敌人的 HP 大于 {Value} %，则跳转到 {scrAddress}
 * 
 * @param Value 欲恢复 HP 的百分比
 * @param scrAddress HP 大于指定百分比则跳转到的地址
 */
function JumpIfEnemyHPMoreThanPercentage(Value, scrAddress) {}

/**
 * @note
 * -  0x0065
 * 
 * @deprecated
 * -  将英雄 {HeroId} 的形象设置为 {SpriteId}
 * 
 * @param HeroId 英雄编号
 * @param SpriteId 形象编号
 * @param Update 是否立即更新
 */
function HeroSetSprite(HeroId, SpriteId, Update) {}

/**
 * @note
 * -  0x0066
 * 
 * @deprecated
 * -  当前队员投掷当前敌人，模拟法术 {MagicId}，基础伤害 {Value}
 * 
 * @param MagicId 欲模拟仙术的编号
 * @param Value 基础伤害
 */
function RoleThrowWeapon(MagicId, Value) {}

/**
 * @note
 * -  0x0067
 * 
 * @deprecated
 * -  设置当前敌人的法术为 {MagicId}，施法概率 {Value} %
 * 
 * @param MagicId 仙术编号
 * @param Value 施法概率百分比
 */
function EnemySetMagic(MagicId, Value) {}

/**
 * @note
 * -  0x0068
 * 
 * @deprecated
 * -  如果是敌方正在行动则跳转到 {scrAddress}
 * 
 * @param scrAddress 敌方正在行动则跳转到的地址
 */
function JumpIfEnemyTurn(scrAddress) {}

/**
 * @note
 * -  0x0069
 * 
 * @deprecated
 * -  敌人从战斗逃跑
 * 
 * @param void 无
 */
function BattleEnemyEscape() {}

/**
 * @note
 * -  0x006A
 * 
 * @deprecated
 * -  当前队员偷窃当前敌人，成功率 {Value} %
 * 
 * @param Value 偷窃成功的概率百分比
 */
function BattleStealFromEnemy() {}

/**
 * @note
 * -  0x006B
 * 
 * @deprecated
 * -  吹动敌方全体，后撤 {FrameNum} 帧
 * 
 * @param FrameNum 后撤的帧计数
 */
function BattleBlowAwayEnemy(FrameNum) {}

/**
 * @note
 * -  0x006C
 * 
 * @deprecated
 * -  事件 {SceneId EventId} 走一步，坐标移动 {X Y}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param X X 坐标移动的量
 * @param Y Y 坐标移动的量
 */
function EventWalkOneStep(SceneId, EventId, X, Y) {}

/**
 * @note
 * -  0x006D
 * 
 * @deprecated
 * -  设置场景 {SceneId} 的脚本为 {ScrEnter ScrTeleport}
 * 
 * @param SceneId 场景编号
 * @param ScrEnter 进场脚本，缺省则不设置
 * @param ScrTeleport 土遁脚本，缺省则不设置
 */
function SceneSetScripts(SceneId, ScrEnter, ScrTeleport) {}

/**
 * @note
 * -  0x006E
 * 
 * @deprecated
 * -  队伍走一步，坐标移动 {X Y}，图层变为 {Layer}
 * 
 * @param X X 坐标移动的量
 * @param Y Y 坐标移动的量
 * @param Layer 图层；实际图层数为 Layer * 8
 */
function RoleMoveOneStep(X, Y, Layer) {}

/**
 * @note
 * -  0x006F
 * 
 * @deprecated
 * -  当前事件与事件 {SceneId EventId} 同步为同一个状态
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param Display 显示状态
 * @param IsObstacle 阻碍队伍通行
 */
function EventSyncState(SceneId, EventId, Display, IsObstacle) {}

/**
 * @note
 * -  0x0070；
 *    0x007A；
 *    0x007B；
 * 
 * @deprecated
 * -  队伍走到块坐标 {BX, BY, BH}；
 *    0x0070：速度 2；
 *    0x007A：速度 4；
 *    0x007B：速度 8
 * 
 * @param BX 欲走到块的 X 坐标
 * @param BY 欲走到块的 Y 坐标
 * @param BH 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
 * @param Speed 移动速度
 */
function PartyWalkToBlock(BX, BY, BH, Speed) {}

/**
 * @note
 * -  0x0071
 * 
 * @deprecated
 * -  波动/扭曲屏幕，层次 {Level}，进度 {Progression}
 * 
 * @param Level 波动/扭曲层次
 * @param Progression 波动/扭曲进度
 */
function VideoWave(Level, Progression) {}

/**
 * @note
 * -  0x0073；
 *    0x009B
 * 
 * @deprecated
 * -  淡入到场景 {SceneId}，速度 {Speed}；
 *    0x0073：当前场景；
 * 
 * @param SceneId 场景编号
 * @param Speed 淡入速度
 */
function FadeToScene(SceneId, Speed) {}

/**
 * @note
 * -  0x0074
 * 
 * @deprecated
 * -  如果队伍中有 HP 不满的的队员，则跳转到 {scrAddress}
 * 
 * @param scrAddress 欲跳转到的地址
 */
function JumpIfNotAllRolesFullHP(scrAddress) {}

/**
 * @note
 * -  0x0075
 * 
 * @deprecated
 * -  设置队伍中的队员，并自动计算和设置队伍人数
 * 
 * @param StrDec 十进制位字符串（1李 2赵 3林 4巫 5奴 6盖，示例：巫李（30））
 */
function PartySetRole(StrDec) {}

/**
 * @note
 * -  0x0076
 * 
 * @deprecated
 * -  淡入 FBP 图像，速度 {Speed}
 * 
 * @param FBPId FBP 图像编号
 * @param Speed 淡入速度
 */
function FadeFBP(FBPId, Speed) {}

/**
 * @note
 * -  0x0077
 * 
 * @deprecated
 * -  停止播放音乐
 * 
 * @param void 无
 */
function MusicStop() {}

/**
 * @note
 * -  0x0078
 * 
 * @deprecated
 * -  战后返回地图
 * 
 * @param void 无
 */
function BattleEnd() {}

/**
 * @note
 * -  0x0079
 * 
 * @deprecated
 * -  如果英雄 {HeroId} 在队伍中，则跳转到地址 {scrAddress}；
 * 
 * @param HeroId 英雄编号
 * @param scrAddress 英雄在队伍中则跳转到的地址
 */
function JumpIfHeroInParty(HeroId, scrAddress) {}

/**
 * @note
 * -  0x007D
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的坐标变动 {X Y}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param X X 坐标移动的量
 * @param Y Y 坐标移动的量
 */
function EventModifyPos(SceneId, EventId, X, Y) {}

/**
 * @note
 * -  0x007E
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的图层设置为 {Layer}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param Layer 图层；
 */
function EventSetLayer(SceneId, EventId, Layer) {}

/**
 * @note
 * -  0x007F
 * 
 * @deprecated
 * -  将视口相对移动 {X Y}，以 {FrameNum} 帧完成移动
 * 
 * @param X X 坐标移动的量
 * @param Y Y 坐标移动的量
 * @param FrameNum 图层；若为 -1 ，回到原点时不更新画面，移动时不更新画面
 */
function ViewportMove(X, Y, FrameNum) {}

/**
 * @note
 * -  0x0080
 * 
 * @deprecated
 * -  昼夜时间滤镜切换
 * 
 * @param UpdateScene 是否更新画面
 */
function ToggleDayNight(UpdateScene) {}

/**
 * @note
 * -  0x0081
 * 
 * @deprecated
 * -  若队伍领队没有面向事件 {SceneId EventId}，则跳转到 {scrAddress}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param TriggerDistance 触发范围
 * @param scrAddress 队伍领队没有面向事件则跳转到的地址
 */
function JumpIfPartyNotFacingEvent(SceneId, EventId, TriggerDistance, scrAddress) {}

/**
 * @note
 * -  0x0083
 * 
 * @deprecated
 * -  若事件 {SceneId EventId}，没有在当前事件的范围 {Range} 内，则跳转到 {scrAddress}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param Range 范围
 * @param scrAddress 队伍领队没有面向事件则跳转到的地址
 */
function JumpIfEventNotInZone(SceneId, EventId, Range, scrAddress) {}

/**
 * @note
 * -  0x0084
 * 
 * @deprecated
 * -  将事件 {SceneId EventId} 的坐标设置到领队面前；
 *    设置显示状态为 {Display}，阻碍队伍通行 {IsObstacle}，放置失败则跳转到 {scrAddress}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param Display 显示状态
 * @param IsObstacle 阻碍队伍通行
 * @param scrAddress 队伍领队没有面向事件则跳转到的地址
 */
function EventSetPosToPartyAndObstacle(SceneId, EventId, Display, IsObstacle, scrAddress) {}

/**
 * @note
 * -  0x0085
 * 
 * @deprecated
 * -  延迟 {Delay * 80} ms
 * 
 * @param Delay 延迟时间；实际延迟时间 {Delay * 80} ms
 */
function Delay(Delay) {}

/**
 * @note
 * -  0x0086
 * 
 * @deprecated
 * -  若所有队员中身上装备的 {ItemId} 数量不足 {Value}，则跳转到 {scrAddress}
 * 
 * @param ItemId 道具编号
 * @param Value 道具数量
 * @param scrAddress 装备数不足则跳转到的脚本
 */
function JumpIfItemNotEquipped(Delay, Value, scrAddress) {}

/**
 * @note
 * -  0x0088
 * 
 * @deprecated
 * -  将仙术 {MagicId} 的基础伤害设置为金钱的 0.4 倍
 * 
 * @param MagicId 仙术编号
 * @param Multiple 倍数
 */
function MagicSetBaseDamageByMoney(MagicId, Multiple) {}

/**
 * @note
 * -  0x0089
 * 
 * @deprecated
 * -  设置战斗结果为 {BattleResult}
 * 
 * @param BattleResult 战斗结果（-1=逃离战斗 0=脚本结束 1=我方全灭 2=战斗胜利）
 */
function BattleSetResult(BattleResult) {}

/**
 * @note
 * -  0x008A
 * 
 * @deprecated
 * -  将下次战斗设置为自动战斗
 * 
 * @param void 无
 */
function BattleEnableAuto() {}

/**
 * @note
 * -  0x008B
 * 
 * @deprecated
 * -  更改当前调色板；废弃的指令
 * 
 * @param PaletteId 调色板编号
 */
function SetPalette(PaletteId) {}

/**
 * @note
 * -  0x008C
 * 
 * @deprecated
 * -  淡出到颜色 {ColorHex} /从颜色 {ColorHex} 淡出
 * 
 * @param Delay 每一步的延迟，实际延迟 {Delay * 60} ms
 * @param ColorHex Hex 颜色值
 * @param IsFrom Hex 是否从 ColorHex 淡出，否则淡出到 ColorHex
 */
function FadeColor(Delay, ColorHex, IsFrom) {}

/**
 * @note
 * -  0x008D
 * 
 * @deprecated
 * -  当前队员修行变动 {Value}
 * 
 * @param Value 欲变动的值，正数增加，负数减少
 */
function RoleModifyLevel(Value) {}

/**
 * @note
 * -  0x008E
 * 
 * @deprecated
 * -  还原屏幕
 * 
 * @param void 无
 */
function VideoRestore() {}

/**
 * @note
 * -  0x008F
 * 
 * @deprecated
 * -  金钱减半
 * 
 * @param void 无
 */
function CashHalve() {}

/**
 * @note
 * -  0x0090
 * 
 * @deprecated
 * -  将 {ObjectType} 对象 {ObjectId} 脚本 {ScrType} 的地址设置为 {scrAddress}
 * 
 * @param ObjectType 对象类型（0=Hero 1=Item 2=Magic 3=Enemy 4=Poison）
 * @param ObjectId 对象编号
 * @param ScrType 脚本类型
 * @param scrAddress 欲设置的脚本地址
 */
function ObjectSetScript(ObjectType, ObjectId, ScrType, scrAddress) {}

/**
 * @note
 * -  0x0091
 * 
 * @deprecated
 * -  如果战场上有多个敌人与当前敌人编号一样，
 *    且当前敌人是其中的第一个，则跳转到 {scrAddress}
 * 
 * @param scrAddress 则跳转到的脚本地址
 */
function JumpIfEnemyNotFirstOfKind(scrAddress) {}

/**
 * @note
 * -  0x0092
 * 
 * @deprecated
 * -  播放队员 {RoleId} 的施法动作，之后我方全体高亮
 * 
 * @param RoleId 队员编号，缺省表示当前队员
 */
function ShowRoleMagicAction(RoleId) {}

/**
 * @note
 * -  0x0093
 * 
 * @deprecated
 * -  屏幕淡出，期间更新场景
 * 
 * @param Step 淡入步长
 */
function VideoFadeAndUpdate(Step) {}

/**
 * @note
 * -  0x0094
 * 
 * @deprecated
 * -  如果事件 {SceneId EventId} 的状态为 {State}，则跳转到 {scrAddress}
 * 
 * @param SceneId 场景编号
 * @param EventId 事件编号
 * @param Display 显示状态
 * @param IsObstacle 阻碍队伍通行
 * @param scrAddress 事件状态匹配则跳转到的地址
 */
function JumpIfEventStateMatches(SceneId, EventId, Display, IsObstacle, scrAddress) {}

/**
 * @note
 * -  0x0095
 * 
 * @deprecated
 * -  如果当前场景为 {SceneId}，则跳转到 {scrAddress}
 * 
 * @param SceneId 场景编号
 * @param scrAddress 场景匹配则跳转到的地址
 */
function JumpIfCurrentSceneMatches(SceneId, scrAddress) {}

/**
 * @note
 * -  0x0096
 * 
 * @deprecated
 * -  显示游戏通关后的动画
 * 
 * @param void 无
 */
function ShowEndingAnimation() {}

/**
 * @note
 * -  0x0098
 * 
 * @deprecated
 * -  设置队伍随从，人数不限
 * 
 * @param SpriteId 形象编号
 */
function PartySetFollower(SpriteId) {}

/**
 * @note
 * -  0x0099
 * 
 * @deprecated
 * -  将场景 {SceneId} 的地图设置为 {MapId}
 * 
 * @param SceneId 场景编号
 * @param MapId 地图编号
 */
function SceneSetMap(SceneId, MapId) {}

/**
 * @note
 * -  0x009A
 * 
 * @deprecated
 * -  将事件 {EventBeginId} ~ 事件 {EventEndId} 的地图设置为 {MapId}
 * 
 * @param SceneBeginId 场景编号
 * @param EventBeginId 起始事件编号
 * @param SceneEndId 场景编号
 * @param EventEndId 终末事件编号
 * @param MapId 地图编号
 * @param Display 显示状态
 * @param IsObstacle 阻碍队伍通行
 */
function EventSetStateSequence(BeginSceneId, BeginEventId, EndSceneId, EndEventId, Display, IsObstacle) {}

/**
 * @note
 * -  0x009C
 * 
 * @deprecated
 * -  敌人分身，数据完全复制，分身失败则跳转到 {scrAddress}
 * 
 * @param Value 分身的数量
 * @param scrAddress 分身失败则跳转到的脚本地址
 */
function EnemyClone(Value, scrAddress) {}

/**
 * @note
 * -  0x009E
 * 
 * @deprecated
 * -  敌人召唤 {EnemyId}，数量 {Value}，若召唤失败则跳转到 {scrAddress}
 * 
 * @param EnemyId 欲召唤的敌人编号
 * @param Value 欲召唤的数量
 * @param scrAddress 召唤失败则跳转到的脚本地址
 */
function EnemySummonMonster(EnemyId, Value, scrAddress) {}

/**
 * @note
 * -  0x009F
 * 
 * @deprecated
 * -  敌方变身为 {EnemyId}
 * 
 * @param EnemyId 敌人编号
 */
function EnemyTransform(EnemyId) {}

/**
 * @note
 * -  0x00A0
 * 
 * @deprecated
 * -  退出游戏
 * 
 * @param void 无
 */
function QuitGame() {}

/**
 * @note
 * -  0x00A1
 * 
 * @deprecated
 * -  将队伍的坐标设置为和领队重合
 * 
 * @param void 无
 */
function PartySetPosToFirstRole() {}

/**
 * @note
 * -  0x00A2
 * 
 * @deprecated
 * -  随机跳转到后面指令 0 ～ {Range} 中的任意一条指令
 * 
 * @param Range 范围
 */
function JumpToRandomInstruction(Range: number) {}

/**
 * @note
 * -  0x00A3
 * 
 * @deprecated
 * -  播放 CD {CDId}，若 CD 不存在则播放 RIX {MusicId}
 * 
 * @param CDId 欲播放的音乐编号
 * @param MusicId 欲播放的音乐编号
 */
function PlayCDOrMusic(CDId, MusicId) {}

/**
 * @note
 * -  0x00A4
 * 
 * @deprecated
 * -  将 FBP {FBPId} 滚动到屏幕，每帧滚动延迟 {800 / Speed} 毫秒
 * 
 * @param FBPId FBP 图像编号
 * @param Speed 滚动速度
 */
function ScrollFBP(FBPId, Speed) {}

/**
 * @note
 * -  0x00A5
 * 
 * @deprecated
 * -  淡入被 320*200 大小的 Npc {SpriteId} 覆盖的 FBP 图像，速度 {Speed}
 * 
 * @param FBPId FBP 图像编号
 * @param SpriteId Npc 形象编号
 * @param Speed 淡入速度
 */
function ShowFBPWithSprite(FBPId, Speed) {}

/**
 * @note
 * -  0x00A6
 * 
 * @deprecated
 * -  备份当前屏幕
 * 
 * @param void 无
 */
function ScreenBackup() {}

/**
 * @note
 * -  0x00A7
 * 
 * @deprecated
 * -  将对话框设置到对象描述的位置，仙术/道具
 * 
 * @param void 无
 */
function DlgItem() {}

/**
 * @note
 * -  0xFFFF
 * 
 * @deprecated
 * -  显示对话
 * 
 * @param Msg 对话内容
 */
function Dlg(Msg) {}