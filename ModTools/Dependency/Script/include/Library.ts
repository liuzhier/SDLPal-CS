/*
 * Copyright (c) 2025, liuzhier.
 * All rights reserved.
 * 
 * This file is part of SDLPAL-CS.
 * 
 * SDLPAL-CS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 3
 * as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

declare global {

    /**
    * -  0x0000
    * 
    * @note
    * -  停止执行
    * 
    * @param void 无
    */
    function End(): void;

    /**
    * -  0x0001
    * 
    * @note
    * -  停止执行，将 调用地址 替换为 下一条命令
    * 
    * @param void 无
    */
    function ReplaceAndPause(): void;

    /**
    * -  0x0002
    * 
    * @note
    * -  停止执行，将 调用地址 替换为 地址 {scrAddress}；
    *    累计触发 {count} 次后，将 调用地址 替换为 下一条命令
    * 
    * @param scrAddress 欲跳转到的地址
    * @param count 最大可触发的次数，若设置为 0 则为总是可触发
    */
    function ReplaceAndPauseWithNop(scrAddress: string, count: ushort): void;

    /**
    * -  0x0003
    * 
    * @note
    * -  跳转到地址 {scrAddress}，完成跳转地址的执行后脚本结束；
    *    累计触发 {count} 次后，该指令将成为 NOP
    * 
    * @param scrAddress 欲跳转到的地址
    * @param count 最大可触发的次数，若设置为 0 则为总是可触发
    */
    function GotoWithNop(scrAddress: string, count: ushort): void;

    /**
    * -  0x0004
    * 
    * @note
    * -  调用地址 {scrAddress}，完成调用后返回并继续执行；
    *    参数 {sceneId} 和 {eventId} 只有 Event 触发脚本时才用到；
    *    注意：不是只有玩家才能触发脚本，Event 也是可以主动触发脚本的！
    * 
    * @param scrAddress 欲跳转到的地址
    * @param sceneId 当前事件所在的场景Id
    * @param eventId 当前事件Id
    */
    function Call(scrAddress: string, sceneId?: short, eventId?: short): void;

    /**
    * -  0x0005
    * 
    * @note
    * -  重绘屏幕并清理对话
    * 
    * @param Delay 画面更新后延迟的毫秒数，缺省则为 1，实际延迟 {Delay * 60} ms
    * @param UpdatePartyGestures 当 RNG 未播放 或 未进入战斗时，是否更新队伍步伐
    */
    function VideoUpdate(Delay: ushort, UpdatePartyGestures: boolean): void;

    /**
    * -  0x0006
    * 
    * @note
    * -  生成随机数1~100，若大于 {probability}，则跳转到地址 {scrAddress}；
    *    否则继续执行。
    * 
    * @param probability 0~100 的数值
    * @param scrAddress 欲跳转到的地址
    */
    function GotoWithProbability(probability: Rate100, scrAddress: string): void;

    /**
    * -  0x0007
    * 
    * @note
    * -  进入战斗，将敌方队列 {enemyTeamId} 放入战场；
    *    若战斗失败，则调用参数二指向的地址；
    *    若战斗胜利，则调用到参数三指向的地址
    * 
    * @param enemyTeamId 敌方队列 Id 
    * @param scrDefeat 战斗失败脚本
    * @param scrFleed 战斗逃跑脚本（Boss 战这里为 0，无法逃跑）
    */
    function BattleStart(enemyTeamId: ushort, scrDefeat: string, scrFleed: string): void;

    /**
    * -  0x0008
    * 
    * @note
    * -  将 调用地址 替换为 下一条命令
    * 
    * @param void 无
    */
    function Replace(): void;

    /**
    * -  0x0009
    * 
    * @note
    * -  所有事件的自动脚本循环 {frameNum} 帧；
    *    期间禁止用户一切操作（包括触发事件），仅场景中的所有事件在运转
    * 
    * @param frameNum 循环帧数
    * @param canTriggerEvent 是否允许触发事件
    * @param updatePartyGestures 是否更新队伍步伐
    */
    function WaitEventAutoScriptRun(frameNum: ushort, canTriggerEvent: boolean, updatePartyGestures: boolean): void;

    /**
    * -  0x000A
    * 
    * @note
    * -  显示选项【"是" "否"】；
    *    选【"是"】则继续执行；
    *    选【"否"】则跳转到参数一指向的地址
    * 
    * @param scrAddress 选 "否" 则跳转到的地址
    */
    function GotoWithSelect(scrAddress: string): void;

    /**
    * -  0x000B；
    *    0x000C；
    *    0x000D；
    *    0x000E；
    *    0x0087
    * 
    * -  当前事件播放 {DirectionId} 方向行走动画（X += 2 * 速度，Y += 1 * 速度）；
    *    0x000B：向西（左下）行，速度 2（X += 4，Y += 1）；
    *    0x000C：向北（左上）行，速度 2；
    *    0x000D：向东（右上）行，速度 2；
    *    0x000E：向南（右下）行，速度 2；
    *    0x0087：向当前方向行，速度 0（X += 0，Y += 0，原地播放行走动画）；
    * 
    * @param direction 行走方向（移动速度是定值）
    */
    function EventAnimate(direction: Direction): void;

    /**
    * -  0x000F
    * 
    * @note
    * -  当前对象面向方向 {direction}，帧编号 {frameId}
    * 
    * @param direction 方向
    * @param frameId 帧编号，大多数第一帧为原地静止，后两帧为行走
    */
    function NpcSetDirFrame(direction: Direction, frameId: ushort): void;

    /**
    * -  0x0010；
    *    0x0082
    * 
    * @note
    * -  当前对象走到块坐标 {bx, by, bh}，待对象到达终点才会继续执行下一条指令；
    *    0x0010：速度 3;
    *    0x0082：速度 8;
    * 
    * @param bx 欲走到块的 X 坐标
    * @param by 欲走到块的 Y 坐标
    * @param bh 欲走到的块的 左右板块Id
    * @param speed 移动速度
    */
    function NpcMoveToBlock(bx: byte, by: byte, bh: BlockHalf, speed: NpcMoveToBlockSpeed): void;

    /**
    *    0x0011；
    *    0x007C；
    * 
    * @note
    * -  当前对象走到块坐标 {bx, by, bh}，奇偶（帧）互斥，待对象到达终点才会继续执行下一条指令；
    *    0x0011：速度 2，奇偶互斥;
    *    0x007C：速度 4，奇偶互斥;
    * 
    * @param bx 欲走到块的 X 坐标
    * @param by 欲走到块的 Y 坐标
    * @param bh 欲走到的块的 左右板块Id
    * @param speed 移动速度
    */
    function NpcMoveToBlockMutexLock(bx: byte, by: byte, bh: BlockHalf, speed: NpcMoveToBlockMutexLockSpeed): void;

    /**
    * -  0x0012
    * 
    * @note
    * -  将事件 {sceneId eventId} 的坐标设置到相对于 [队伍领队] 的点 {x, y}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param x 欲走到的 X 坐标
    * @param y 欲走到的 Y 坐标
    */
    function EventSetPosRelToParty(sceneId: short_1, eventId: short_1, x: short, y: short): void;

    /**
    * -  0x0013
    * 
    * @note
    * -  将事件 {sceneId eventId} 的坐标设置到绝对点 {x, y}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param x 欲设置到的 X 坐标
    * @param y 欲设置到的 Y 坐标
    */
    function EventSetPos(sceneId: short_1, eventId: short_1, x: ushort, y: ushort): void;

    /**
    * -  0x0014
    * 
    * @note
    * -  将当前事件的帧号设置为 {FrameId}，并把方向设置为 Direction.Southwest
    * 
    * @param frameId 帧编号，第一帧为原地，后两帧为走路
    */
    function NpcSetFrame(FrameId: ushort): void;

    /**
    * -  0x0015
    * 
    * @note
    * -  将 {party} 的方向设置为 {Direction}，帧号设置为 {frameId}
    * 
    * @param direction 方向（负数则方向不变）
    * @param frameId 帧编号，第一帧为原地，后两帧为走路
    * @param party 队伍中 Role 的编号
    */
    function RoleSetDirFrame(direction: Direction, frameId: ushort, partyId: Party): void;

    /**
    * -  0x0016
    * 
    * @note
    * -  将事件 {sceneId eventId} 的方向设置为 {direction}，帧号设置为 {frameId}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param direction 方向（负数则方向不变） 【西-左下[0] 北-左上[1] 东-右上[2] 南-右下[3]】
    * @param frameId 帧编号，第一帧为原地，后两帧为走路
    */
    function EventSetDirFrame(sceneId: short_1, eventId: short_1, direction: Direction, frameId: ushort): void;

    /**
    * -  0x0017
    * 
    * @note
    * -  将当前队员 {body} 部位的装备附带属性 {attribute} 设置为 {value}
    * 
    * @param body 装备对应身体部位的编号
    * @param attribute 属性编号
    * @param value 欲设置的值
    */
    function RoleSetEquipAttr(body: EquipEffectType, attribute: Attribute, value: short): void;

    /**
    * -  0x0018
    * 
    * @note
    * -  当前队员装备道具 {itemId}，装备到部位 {bodyId}
    * 
    * @param itemId 欲装备道具的编号
    * @param body 装备对应身体部位的编号
    */
    function RoleInstallEquip(body: Body, itemId: ushort): void;

    /**
    * -  0x0019
    * 
    * @note
    * -  将队员 {role} 的基础属性 {attribute} 变动 {value}
    * 
    * @param attribute 属性编号
    * @param value 欲变动的值，正数增加，负数减少
    * @param role 队员编号，缺省表示当前队员
    */
    function RoleModifyAttr(attribute: Attribute, value: short, role: Role): void;

    /**
    * -  0x001A
    * 
    * @note
    * -  将队员 {role} 的基础属性 {attribute} 设置为 {value}
    * 
    * @param role 队员编号，缺省表示当前队员
    * @param attribute 属性编号
    * @param value 欲设置的值
    */
    function RoleSetAttr(attribute: Attribute, value: short, role: Role): void;

    /**
    * -  0x001B
    * 
    * @note
    * -  我方 {applyToAll} HP 变动 {value}
    * 
    * @param applyToAll 是否作用于我方全体，缺省则为当前队员
    * @param value 欲变动的值，正数增加，负数减少
    */
    function RoleModifyHP(applyToAll: boolean, value: short): void;

    /**
    * -  0x001C
    * 
    * @note
    * -  我方 {applyToAll} MP 变动 {value}
    * 
    * @param applyToAll 是否作用于我方全体，缺省则为当前队员
    * @param value 欲变动的值，正数增加，负数减少
    */
    function RoleModifyMP(applyToAll: boolean, value: short): void;

    /**
    * -  0x001D
    * 
    * @note
    * -  我方 {applyToAll} MP 变动 {value}
    * 
    * @param applyToAll 是否作用于我方全体，缺省则为当前队员
    * @param value 欲变动的值，正数增加，负数减少
    */
    function RoleModifyHPMP(applyToAll: boolean, value: short): void;

    /**
    * -  0x001E
    * 
    * @note
    * -  金钱变动 {value}
    * 
    * @param value 欲变动的值，正数增加，负数减少，
    * @param scrAddress 金钱不足时跳转到的地址
    */
    function CashModify(value: short, scrAddress: string): void;

    /**
    * -  0x001F
    * 
    * @note
    * -  将道具 {itemId} 添加到背包中，数量 {value}
    * 
    * @param itemId 道具编号
    * @param value 道具数量，缺省则为 1
    */
    function AddItem(itemId: ushort, value: short): void;

    /**
    * -  0x0020
    * 
    * @note
    * -  将道具 {itemId} 在背包中删除，数量 {value}，背包中道具不足，则会从角色身上卸下；
    *    若道具不足，则跳转到地址 {scrAddress}
    * 
    * @param itemId 道具编号
    * @param value 道具数量
    * @param scrAddress 道具不足时跳转到的地址
    */
    function RemoveItem(itemId: ushort, value: ushort, scrAddress: string): void;

    /**
    * -  0x0021
    * 
    * @note
    * -  敌方 {applyToAll} HP 减少 {value}
    * 
    * @param applyToAll 是否作用于敌方全体，缺省则为当前敌人
    * @param value 欲变动的值
    */
    function EnemyModifyHP(applyToAll: boolean, value: ushort): void;

    /**
    * -  0x0022
    * 
    * @note
    * -  我方 {applyToAll} 复活，HP 恢复 {value} %
    * 
    * @param applyToAll 是否作用于我方全体，缺省则为当前队员
    * @param value 欲恢复 HP 的十分比
    */
    function RoleRevive(applyToAll: boolean, value: ushort): void;

    /**
    * -  0x0023
    * 
    * @note
    * -  队员 {role} 卸下部位 {body} 的装备
    * 
    * @param role 队员编号，缺省表示当前队员
    * @param body 装备对应身体部位的编号
    */
    function RoleUninstallEquip(role: Role, body: UninstallEquip): void;

    /**
    * -  0x0024
    * 
    * @note
    * -  将事件 {sceneId eventId} 的自动脚本设置为 {scrAddress}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param scrAddress 欲设置的脚本地址
    */
    function EventSetAutoScript(sceneId: short_1, eventId: short_1, scrAddress: string): void;

    /**
    * -  0x0025
    * 
    * @note
    * -  将事件 {sceneId eventId} 的触发脚本设置为 {scrAddress}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param scrAddress 欲设置的脚本地址
    */
    function EventSetTriggerScript(sceneId: short_1, eventId: short_1, scrAddress: string): void;

    /**
    * -  0x0026
    * 
    * @note
    * -  显示购买道具菜单 {storeId}
    * 
    * @param storeId 店铺编号
    */
    function ShowBuyItemMenu(storeId: ushort): void;

    /**
    * -  0x0027
    * 
    * @note
    * -  显示出售道具菜单
    * 
    * @param void 无
    */
    function ShowSellItemMenu(): void;

    /**
    * -  0x0028
    * 
    * @note
    * -  敌方 {applyToAll} 中毒 {poisonId}
    * 
    * @param applyToAll 是否作用于敌方全体，缺省则为当前敌人
    * @param poisonId 毒性编号
    */
    function EnemyApplyPoison(applyToAll: boolean, poisonId: ushort): void;

    /**
    * -  0x0029
    * 
    * @note
    * -  我方 {applyToAll} 中毒 {poisonId}
    * 
    * @param applyToAll 是否作用于我方全体，缺省则为当前队员
    * @param poisonId 毒性编号
    */
    function RoleApplyPoison(applyToAll: boolean, poisonId: ushort): void;

    /**
    * -  0x002A
    * 
    * @note
    * -  敌方 {applyToAll} 解毒 {poisonId}
    * 
    * @param applyToAll 是否作用于敌方全体，缺省则为当前敌人
    * @param poisonId 毒性编号
    */
    function EnemyCurePoisonById(applyToAll: boolean, poisonId: ushort): void;

    /**
    * -  0x002B
    * 
    * @note
    * -  我方 {applyToAll} 解毒 {poisonId}
    * 
    * @param applyToAll 是否作用于我方全体，缺省则为当前队员
    * @param poisonId 毒性编号
    */
    function RoleCurePoisonById(applyToAll: boolean, poisonId: ushort): void;

    /**
    * -  0x002C
    * 
    * @note
    * -  我方 {applyToAll} 解级别 {level} 以内的毒
    * 
    * @param applyToAll 是否作用于我方全体，缺省则为当前队员
    * @param level 毒性等级
    */
    function RoleCurePoisonBylevel(applyToAll: boolean, level: ushort): void;

    /**
    * -  0x002D 
    * 
    * @note
    * -  当前队员获得 {status} 状态，持续 {value} 回合
    * 
    * @param status 状态编号
    * @param value 状态持续的回合数
    */
    function RoleSetStatus(StatusId: Status, value: ushort): void;

    /**
    * -  0x002E
    * 
    * @note
    * -  当前敌人获得 {status} 状态，持续 {value} 回合。巫抗判断，失败则跳转到 {scrAddress}
    * 
    * @param status 状态编号
    * @param value 状态持续的回合数
    * @param scrAddress 欲设置的脚本地址
    */
    function EnemySetStatus(status: Status, value: ushort, scrAddress: string): void;

    /**
    * -  0x002F
    * 
    * @note
    * -  当前队员清除 {status} 状态
    * 
    * @param status 状态编号
    */
    function RoleRemoveStatus(status: Status): void;

    /**
    * -  0x0030
    * 
    * @note
    * -  队员 {role} 修改临时属性 {attribute} 变动 {value} %
    * 
    * @param attribute 属性编号
    * @param value 欲变动的百分比，正数增加，负数减少
    * @param role 队员编号，缺省表示当前队员
    */
    function RoleModifyAttrTemp(attribute: Attribute, value: short, role: Role): void;

    /**
    * -  0x0031
    * 
    * @note
    * -  临时将当前队员的战斗图像设置为 {battleSpriteId}
    * 
    * 参数：BattleSpriteId 队员战斗形象编号
    */
    function RoleModifyBattleSpriteTemp(battleSpriteId: ushort): void;

    /**
    * -  0x0033
    * 
    * @note
    * -  将当前敌人收入紫金葫芦；
    *    若收妖失败（敌人不存在灵葫能量），则跳转到地址 {scrAddress}
    * 
    * @param scrAddress 收妖失败时跳转到的地址
    */
    function CaptureTheEnemy(scrAddress: string): void;

    /**
    * -  0x0034
    * 
    * @note
    * -  将紫金葫芦中的灵葫能量炼化为丹药；
    *    若炼丹失败（灵葫能量不足），则跳转到地址 {scrAddress}
    * 
    * @param scrAddress 炼丹失败时跳转到的地址
    */
    function MakeElixir(scrAddress: string): void;

    /**
    * -  0x0035
    * 
    * @note
    * -  画面震动 {frameNum} 帧，振幅 {level} 级
    * 
    * @param frameNum 震动帧数
    * @param level 震动等级
    */
    function VideoShake(frameNum: ushort, level: ushort): void;

    /**
    * -  0x0036
    * 
    * @note
    * -  将 rng 动画设置为 {rngId}
    * 
    * @param rngId RNG 动画编号
    */
    function SetRng(rngId: ushort): void;

    /**
    * -  0x0037
    * 
    * @note
    * -  播放 RNG 动画，从第 {beginFrameId} 帧开始播放，播放到 {endFrameId} 帧终止，速度 {speed}
    * 
    * @param beginFrameId 起始帧号
    * @param endFrameId 结束帧号，缺省则为播放到最后一帧
    * @param speed 播放速度，缺省则为 16
    */
    function PlayRng(beginFrameId: ushort, endFrameId: ushort, speed: ushort): void;

    /**
    * -  0x0038
    * 
    * @note
    * -  土遁，脱离迷宫用。若土遁失败，则跳转到 {scrAddress}
    * 
    * @param scrAddress 土遁失败时跳转到的地址
    */
    function SceneTeleport(scrAddress: string): void;

    /**
    * -  0x0039
    * 
    * @note
    * -  吸取受害者 {value} 点 HP 补充给施暴者，可我对敌，亦可敌对我
    * 
    * @param void 无
    */
    function DrainHPFromEnemy(value: ushort): void;

    /**
    * -  0x003A
    * 
    * @note
    * -  非 Boss 战逃跑。若逃跑失败，则跳转到 {scrAddress}
    * 
    * @param scrAddress 逃跑失败时跳转到的地址
    */
    function RoleFleeBattle(scrAddress: string): void;

    /**
    * -  0x003B
    * 
    * @note
    * -  将对话框位置设置在画面中间，头像为 {faceId}，字体颜色为 {colorHex}
    * 
    * @param colorId 调色板颜色索引
    * @param rngPlaying 是否正在播放 RNG 动画
    */
    function SetDlgCenter(colorId: ushort, rngPlaying: boolean): void;

    /**
    * -  0x003C
    * 
    * @note
    * -  将对话框位置设置在画面上部，头像为 {faceId}，字体颜色为 {colorId}，是否正在播放 Rng 动画 {rngIsPlaying}
    * 
    * @param faceId 对话框上肖像的编号
    * @param colorId 调色板颜色索引
    * @param rngIsPlaying 是否正在播放 Rng 动画
    */
    function SetDlgUpper(faceId: ushort, colorId: ushort, rngIsPlaying: boolean): void;

    /**
    * -  0x003D
    * 
    * @note
    * -  将对话框位置设置在画面上部，头像为 {faceId}，字体颜色为 {colorId}，是否正在播放 Rng 动画 {rngIsPlaying}
    * 
    * @param FaceId 对话框上肖像的编号
    * @param colorId 调色板颜色索引
    * @param rngIsPlaying 是否正在播放 Rng 动画
    */
    function SetDlgLower(faceId: ushort, colorId: ushort, rngIsPlaying: boolean): void;

    /**
    * -  0x003E
    * 
    * @note
    * -  将对话框位置设置在画面中间的盒子里，字体颜色为 {ColorHex}
    * 
    * @param colorId 调色板颜色索引
    */
    function SetDlgBox(colorId: byte): void;

    /**
    * -  0x003F；
    *    0x0044；
    *    0x0097
    * 
    * @note
    * -  乘坐当前事件行至块坐标 {bx, by, bh}；
    *    0x003F：速度 2;
    *    0x0044：速度 4;
    *    0x0097：速度 8;
    * 
    * @param bx 欲走到块的 X 坐标
    * @param by 欲走到块的 Y 坐标
    * @param bh 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
    * @param speed 行驶速度
    */
    function RideNpcToPos(bx: byte, by: byte, bh: BlockHalf, speed: RideNpcToPosSpeed): void;

    /**
    * -  0x0040
    * 
    * @note
    * -  将事件 {sceneId eventId} 的触发方式设置为 {isAutoTrigger}，触发范围为 {triggerDistance}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param isAutoTrigger 是否走到触发范围内自动触发
    * @param triggerDistance 触发范围（手动触发的范围限制在 0～2，自动触发范围不限）
    */
    function EventSetTriggerMode(sceneId: short_1, eventId: short_1, isAutoTrigger: boolean, triggerDistance: short_1): void;

    /**
    * -  0x0041
    * 
    * @note
    * -  将脚本标记为失败；
    *    通常用于仙术的 Use[条件检测] 脚本，标记为失败则不再执行 Success 脚本
    * 
    * @param void 无
    */
    function ScriptFailed(): void;

    /**
    * -  0x0042
    * 
    * @note
    * -  模拟我方仙术 {magicId}，攻击敌人 {enemyId}，基础伤害为 {value}；
    * 
    * @param magicId 欲模拟的仙术编号
    * @param enemy 敌人编号，缺省则为当前敌人
    * @param value 基础伤害
    */
    function SimulateRoleMagic(magicId: ushort, enemyId: Enemy, value: short): void;

    /**
    * -  0x0043
    * 
    * @note
    * -  设置场景音乐为 {music}，循环播放 {loop}，是否淡入淡出 {needFade}
    * 
    * @param music 欲播放的音乐编号
    * @param loop 是否循环播放
    * @param needFade 是否淡入淡出，时间 3 秒；但音乐 9 无法淡入淡出
    */
    function MusicPlay(music: Music, loop: boolean, needFade: boolean): void;

    /**
    * -  0x0045
    * 
    * @note
    * -  设置战斗音乐为 {music}
    * 
    * @param music 欲播放的音乐编号
    */
    function SetBattleMusic(music: Music): void;

    /**
    * -  0x0046
    * 
    * @note
    * -  队伍走到块坐标 {bx, by, bh}
    * 
    * @param bx 欲走到块的 X 坐标
    * @param by 欲走到块的 Y 坐标
    * @param bh 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
    */
    function PartySetPos(bx: byte, by: byte, bh: BlockHalf): void;

    /**
    * -  0x0047
    * 
    * @note
    * -  播放音效 {soundId}
    * 
    * @param soundId 欲播放音效的编号
    */
    function PlaySound(soundId: ushort): void;

    /**
    * -  0x0049
    * 
    * @note
    * -  设置事件 {sceneId eventId} 的状态码为 {stateCode}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param stateCode 显示状态
    */
    function EventSetState(sceneId: short_1, eventId: short_1, stateCode: EventStateCode): void;

    /**
    * -  0x004A
    * 
    * @note
    * -  设置战斗环境为 {fbp}
    * 
    * @param fbp 战斗环境编号
    */
    function SetBattlefield(fbp: FbpDos): void;
    function SetBattlefield(fbp: FbpWin): void;

    /**
    * -  0x004B
    * 
    * @note
    * -  当前事件静止不动一段时间，期间无法互动（其实就是逃跑后敌人在 Scene 原地罚站）
    * 
    * @param void 无
    */
    function NpcSetStillTime(): void;

    /**
    * -  0x004C
    * 
    * @note
    * -  当前事件追逐队伍，警戒范围 {range}，追逐速度 {speed}
    * 
    * @param range 警戒范围，队伍走范围内开始被追逐，缺省则为 8
    * @param speed 追逐速度，缺省则为 4
    * @param canFly 能够穿墙追逐角色
    */
    function NpcChase(range: ushort, speed: ushort, canFly: boolean): void;

    /**
    * -  0x004D
    * 
    * @note
    * -  等待按下任意键
    * 
    * @param void 无
    */
    function WaitForAnyKey(): void;

    /**
    * -  0x004E
    * 
    * @note
    * -  读取最近的一次存档
    * 
    * @param void 无
    */
    function LoadLastSave(): void;

    /**
    * -  0x004F
    * 
    * @note
    * -  屏幕泛红（我方全灭时用）
    * 
    * @param void 无
    */
    function FadeToRed(): void;

    /**
    * -  0x0050
    * 
    * @note
    * -  屏幕淡出
    * 
    * @param delay 每一步的延迟时间，缺省则为 1（实际延迟：{delay} * 600 ms）
    */
    function FadeOut(delay: ushort): void;

    /**
    * -  0x0051
    * 
    * @note
    * -  屏幕淡入
    * 
    * @param delay 每一步的延迟时间，小于 0 则为 1（实际延迟：{delay} * 600 ms）
    */
    function FadeIn(delay: short): void;

    /**
    * -  0x0052
    * 
    * @note
    * -  当前对象隐藏 {frameNum} 帧
    * 
    * @param frameNum 欲隐藏的帧数，缺省则为 800 帧（）
    */
    function NpcSetVanishTime(frameNum: ushort): void;

    /**
    * -  0x0053
    *    0x0054
    * 
    * @note
    * -  设置调色板时间段
    * 
    * @param paletteTime 调色板时间段
    */
    function SetPaletteTime(paletteTime: PaletteTime): void;

    /**
    * -  0x0055
    * 
    * @note
    * -  英雄 {hero} 练成仙术 {magicId}
    * 
    * @param magicId 欲习得仙术的编号
    * @param hero 英雄编号，缺省表示当前队员
    */
    function HeroAddMagic(magicId: ushort, hero: Hero_1): void;

    /**
    * -  0x0056
    * 
    * @note
    * -  英雄 {hero} 丧失仙术 {magicId}
    * 
    * @param magicId 欲丧失仙术的编号
    * @param hero 英雄编号，缺省表示当前队员
    */
    function HeroRemoveMagic(magicId: ushort, hero: Hero_1): void;

    /**
    * -  0x0057
    * 
    * @note
    * -  将仙术 {magicId} 的基础伤害设置为 MP 的 {multiple} 倍
    * 
    * @param magicId 欲设置仙术的编号
    * @param multiple 倍数，缺省则为 8
    */
    function MagicSetBaseDamageByMP(magicId: ushort, multiple: ushort): void;

    /**
    * -  0x0058
    * 
    * @note
    * -  如果库存中的道具 {itemId} 少于 {value} 个，则跳转到 {scrAddress}
    * 
    * @param itemId 道具编号
    * @param value 道具数量
    * @param scrAddress 道具不足时跳转到的地址
    */
    function JumpIfItemCountLessThan(itemId: ushort, value: short, scrAddress: string): void;

    /**
    * -  0x0059
    * 
    * @note
    * -  切换到场景 {sceneId}
    * 
    * @param sceneId 场景编号
    */
    function SceneEnter(sceneId: short_1): void;

    /**
    * -  0x005A
    * 
    * @note
    * -  当前队员 HP 减半
    * 
    * @param void 无
    */
    function RoleHalveHP(): void;

    /**
    * -  0x005B
    * 
    * @note
    * -  当前敌人 HP 减半
    * 
    * @param void 无
    */
    function EnemyHalveHP(): void;

    /**
    * -  0x005C
    * 
    * @note
    * -  我方在战斗中隐身，持续 {value} 回合
    * 
    * @param value 持续的回合数
    */
    function BattleRoleVanish(value: ushort): void;

    /**
    * -  0x005D
    * 
    * @note
    * -  如果当前队员未中毒 {poisonId}，则跳转到 {scrAddress}
    * 
    * @param poisonId 毒性编号
    * @param scrAddress 未中毒则跳转到的地址
    */
    function JumpIfRoleNotPoisonedByKind(poisonId: ushort, scrAddress: string): void;

    /**
    * -  0x005E
    * 
    * @note
    * -  如果当前敌人未中毒 {PoisonId}，则跳转到 {scrAddress}
    * 
    * @param PoisonId 毒性编号
    * @param scrAddress 未中毒则跳转到的地址
    */
    function JumpIfEnemyNotPoisonedByKind(poisonId: ushort, scrAddress: string): void;

    /**
    * -  0x005F
    * 
    * @note
    * -  当前队员直接阵亡
    * 
    * @param void 无
    */
    function KillRole(): void;

    /**
    * -  0x0060
    * 
    * @note
    * -  当前敌人直接阵亡
    * 
    * @param void 无
    */
    function KillEnemy(): void;

    /**
    * -  0x0061
    * 
    * @note
    * -  如果当前队员未中毒，则跳转到 {scrAddress}
    * 
    * @param scrAddress 未中毒则跳转到的地址
    */
    function JumpIfRoleNotPoisoned(scrAddress: string): void;

    /**
    * -  0x0062；0x0063
    * 
    * @note
    * -  事件暂停追逐队伍，持续 {frameNum} 帧
    *    0x0062：警戒距离级别 0（停止追逐）
    *    0x0063：警戒距离级别 3
    * 
    * @param range 警戒范围
    * @param frameNum 持续的帧数（Scene 每秒更新 10 帧，实际生效秒数：{frameNum} / 10）
    */
    function NpcChaseSetRange(range: NpcChaseRange, frameNum: ushort): void;

    /**
    * -  0x0064
    * 
    * @note
    * -  如果当前敌人的 HP 大于 {value} %，则跳转到 {scrAddress}
    * 
    * @param value 欲恢复 HP 的百分比
    * @param scrAddress HP 大于指定百分比则跳转到的地址
    */
    function JumpIfEnemyHPMoreThanPercentage(value: Rate100, scrAddress: string): void;

    /**
    * -  0x0065
    * 
    * @note
    * -  将英雄 {heroId} 的形象设置为 {spriteId}
    * 
    * @param heroId 英雄编号
    * @param spriteId 形象编号
    * @param applyImmediately 是否立即生效（在 Scene 时才有效）
    */
    function HeroSetSprite(heroId: Hero, spriteId: ushort, applyImmediately: boolean): void;

    /**
    * -  0x0066
    * 
    * @note
    * -  当前队员投掷当前敌人，模拟法术 {magicId}，基础伤害 {value}
    * 
    * @param magicId 欲模拟仙术的编号
    * @param value 基础伤害
    */
    function RoleThrowWeapon(magicId: ushort, value: ushort): void;

    /**
    * -  0x0067
    * 
    * @note
    * -  设置当前敌人的法术为 {magicId}，施法概率十分之 {rate}
    * 
    * @param magicId 仙术编号（0 = 只能普攻，-1 = 只能普攻，有十分之 {rate} 概率本回合不行动）
    * @param rate 施法概率十分比，缺省则为 10
    */
    function EnemySetMagic(magicId: short, rate: Rate10): void;

    /**
    * -  0x0068
    * 
    * @note
    * -  如果是敌方正在行动则跳转到 {scrAddress}
    * 
    * @param scrAddress 敌方正在行动则跳转到的地址
    */
    function JumpIfEnemyTurn(scrAddress: string): void;

    /**
    * -  0x0069
    * 
    * @note
    * -  敌人从战斗逃跑
    * 
    * @param void 无
    */
    function BattleEnemyEscape(): void;

    /**
    * -  0x006A
    * 
    * @note
    * -  当前队员偷窃当前敌人，成功率十分之 {rate}
    * 
    * @param rate 偷窃成功的概率十分比
    */
    function BattleStealFromEnemy(rate: Rate10): void;

    /**
    * -  0x006B
    * 
    * @note
    * -  吹动敌方全体，后撤 {frameNum} 帧
    * 
    * @param frameNum 后撤的帧计数
    */
    function BattleBlowAwayEnemy(frameNum: short): void;

    /**
    * -  0x006C
    * 
    * @note
    * -  事件 {sceneId eventId} 走一步，坐标移动 {x y}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param x X 坐标移动的量
    * @param y Y 坐标移动的量
    */
    function EventWalkOneStep(sceneId: short_1, eventId: short_1, x: short, y: short): void;

    /**
    * -  0x006D
    * 
    * @note
    * -  设置场景 {sceneId} 的脚本为 {scrEnter scrTeleport}（若二者皆为缺省时才都置 0）
    * 
    * @param sceneId 场景编号
    * @param scrEnter 进场脚本，缺省则不设置（单独缺省不会置 0）
    * @param scrTeleport 土遁脚本，缺省则不设置（单独缺省不会置 0）
    */
    function SceneSetScript(sceneId: short_1, scrEnter: string, scrTeleport: string): void;

    /**
    * -  0x006E
    * 
    * @note
    * -  队伍走一步，坐标移动 {x y}，图层变为 {Layer}
    * 
    * @param x X 坐标移动的量
    * @param y Y 坐标移动的量
    * @param layer 图层；实际图层数为 Layer * 8
    */
    function RoleMoveOneStep(x: short, y: short, layer: ushort): void;

    /**
    * -  0x006F
    * 
    * @note
    * -  当前事件与事件 {sceneId eventId} 同步为同一个状态
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param stateCode 显示状态
    */
    function EventSyncState(sceneId: short_1, eventId: short_1, stateCode: EventStateCode): void;

    /**
    * -  0x0070；
    *    0x007A；
    *    0x007B；
    * 
    * @note
    * -  队伍走到块坐标 {bx, by, bh}；
    *    0x0070：速度 2；
    *    0x007A：速度 4；
    *    0x007B：速度 8
    * 
    * @param bx 欲走到块的 X 坐标
    * @param by 欲走到块的 Y 坐标
    * @param bh 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
    * @param speed 移动速度
    */
    function PartyWalkToBlock(bx: byte, by: byte, bh: BlockHalf, speed: PartyWalkToBlockSpeed): void;

    /**
    * -  0x0071
    * 
    * @note
    * -  波动/扭曲屏幕，层次 {level}，进度 {progression}
    * 
    * @param level 波动/扭曲层次
    * @param progression 波动/扭曲进度
    */
    function VideoWave(level: ushort, progression: short): void;

    /**
    * -  0x0073；
    *    0x009B
    * 
    * @note
    * -  淡入到场景 {sceneId}，速度 {speed}；
    *    0x0073：当前场景；
    * 
    * @param sceneId 场景编号
    * @param speed 淡入速度
    */
    function FadeToScene(speed: short_1, sceneId: short_1): void;

    /**
    * -  0x0074
    * 
    * @note
    * -  如果队伍中有 HP 不满的的队员，则跳转到 {scrAddress}
    * 
    * @param scrAddress 欲跳转到的地址
    */
    function JumpIfNotAllRolesFullHP(scrAddress: string): void;

    /**
    * -  0x0075
    * 
    * @note
    * -  设置队伍中的队员，并自动计算和设置队伍人数
    * 
    * @param role1 Hero 编号
    * @param role2 Hero 编号
    * @param role3 Hero 编号
    */
    function PartySetRole(role1: Hero_1, role2: Hero_1, role3: Hero_1): void;

    /**
    * -  0x0076
    * 
    * @note
    * -  淡入 Fbp 图像 {fbp}，速度 {speed}
    * 
    * @param fbp Fbp 图像编号
    * @param speed 淡入速度
    */
    function FadeFbp(fbp: FbpDos, speed: ushort): void;
    function FadeFbp(fbp: FbpWin, speed: ushort): void;

    /**
    * -  0x0077
    * 
    * @note
    * -  停止播放音乐
    * 
    * @param fadeTime 淡入淡出时间（实际时间：{fadeTime} * 3），缺省则直接为 2 秒
    */
    function MusicStop(fadeTime: ushort): void;

    /**
    * -  0x0078
    * 
    * @note
    * -  战后返回地图
    * 
    * @param void 无
    */
    function BattleEnd(): void;

    /**
    * -  0x0079
    * 
    * @note
    * -  如果英雄 {hero} 在队伍中，则跳转到地址 {scrAddress}；
    * 
    * @param hero 英雄编号
    * @param scrAddress 英雄在队伍中则跳转到的地址
    */
    function JumpIfHeroInParty(hero: HeroEntity, scrAddress: string): void;

    /**
    * -  0x007D
    * 
    * @note
    * -  将事件 {sceneId eventId} 的坐标变动 {x y}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param x X 坐标移动的量
    * @param y Y 坐标移动的量
    */
    function EventModifyPos(sceneId: short, eventId: short, x: short, y: short): void;

    /**
    * -  0x007E
    * 
    * @note
    * -  将事件 {sceneId eventId} 的图层设置为 {layer}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param layer 图层
    */
    function EventSetLayer(sceneId: short, eventId: short, layer: short): void;

    /**
    * -  0x007F
    * 
    * @note
    * -  将视口相对移动 {x y}，以 {frameNum} 帧完成移动
    * 
    * @param x X 坐标移动的量
    * @param y Y 坐标移动的量
    * @param frameNum 图层；若为 -1 ，回到原点时不更新画面，移动时不更新画面
    */
    function ViewportMove(x: short, y: short, frameNum: short_1): void;

    /**
    * -  0x0080
    * 
    * @note
    * -  昼夜调色板切换
    * 
    * @param applyImmediately 是否立即生效
    */
    function TogglePaletteTime(applyImmediately: boolean): void;

    /**
    * -  0x0081
    * 
    * @note
    * -  若队伍领队没有面向事件 {sceneId eventId}，则跳转到 {scrAddress}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param triggerDistance 触发范围
    * @param scrAddress 队伍领队没有面向事件则跳转到的地址
    */
    function JumpIfPartyNotFacingEvent(sceneId: short_1, eventId: short_1, triggerDistance: short_1, scrAddress: string): void;

    /**
    * -  0x0083
    * 
    * @note
    * -  若事件 {sceneId eventId}，没有在当前事件的范围 {range} 内，则跳转到 {scrAddress}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param range 范围
    * @param scrAddress 队伍领队没有面向事件则跳转到的地址
    */
    function JumpIfEventNotInZone(sceneId: short_1, eventId: short_1, range: ushort, scrAddress: string): void;

    /**
    * -  0x0084
    * 
    * @note
    * -  将事件 {sceneId eventId} 的坐标设置到领队面前；
    *    设置显示状态为 {Display}，阻碍队伍通行 {IsObstacle}，放置失败则跳转到 {scrAddress}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param stateCode 显示状态
    * @param scrAddress 队伍领队没有面向事件则跳转到的地址
    */
    function EventSetPosToPartyAndObstacle(sceneId: short_1, eventId: short_1, stateCode: EventStateCode, scrAddress: string): void;

    /**
    * -  0x0085
    * 
    * @note
    * -  延迟 {delay * 80} ms
    * 
    * @param delay 延迟时间；实际延迟时间 {delay * 80} ms
    */
    function Delay(delay: ushort): void;

    /**
    * -  0x0086
    * 
    * @note
    * -  若所有队员中身上装备的 {itemId} 数量不足 {value}，则跳转到 {scrAddress}
    * 
    * @param itemId 道具编号
    * @param value 道具数量
    * @param scrAddress 装备数不足则跳转到的脚本
    */
    function JumpIfItemNotEquipped(itemId: ushort, value: ushort, scrAddress: string): void;

    /**
    * -  0x0088
    * 
    * @note
    * -  失去 5000 文钱（不够则全部失去），将仙术 {magicId} 的基础伤害设置为实际失去钱数的 0.4 倍
    * 
    * @param magicId 仙术编号
    */
    function MagicSetBaseDamageByMoney(magicId: ushort): void;

    /**
    * -  0x0089
    * 
    * @note
    * -  设置战斗结果为 {battleResult}
    * 
    * @param battleResult 战斗结果
    */
    function BattleSetResult(battleResult: BattleResult): void;

    /**
    * -  0x008A
    * 
    * @note
    * -  将下次战斗设置为自动战斗
    * 
    * @param void 无
    */
    function BattleEnableAuto(): void;

    /**
    * -  0x008B
    * 
    * @note
    * -  更改当前调色板；废弃的指令
    * 
    * @param palette 调色板编号
    */
    function SetPalette(palette: Palette): void;

    /**
    * -  0x008C
    * 
    * @note
    * -  淡出到颜色 {colorId} /从颜色 {colorId} 淡出
    * 
    * @param delay 每一步的延迟，实际延迟 {delay * 60} ms
    * @param colorId 当前调色板上的 colorId
    * @param isFrom 是否从 colorId 淡出，否则淡出到 colorId
    */
    function FadeColor(delay: ushort, colorId: byte, isFrom: boolean): void;

    /**
    * -  0x008D
    * 
    * @note
    * -  当前队员修行变动 {value}
    * 
    * @param value 欲变动的值，正数增加，负数减少
    */
    function RoleModifylevel(value: ushort): void;

    /**
    * -  0x008E
    * 
    * @note
    * -  还原屏幕
    * 
    * @param void 无
    */
    function VideoRestore(): void;

    /**
    * -  0x008F
    * 
    * @note
    * -  金钱减半
    * 
    * @param void 无
    */
    function CashHalve(): void;

    /**
    * -  0x0090
    * 
    * @note
    * -  将对象 {ObjectId} 脚本 {scrType} 的地址设置为 {scrAddress}
    * 
    * @param objectId 对象编号
    * @param scrType 脚本类型
    * @param scrAddress 欲设置的脚本地址
    */
    function HeroSetScript(heroId: ushort, scrType: HeroScript, scrAddress: string): void;
    function ItemSetScript(itemId: ushort, scrType: ItemScript, scrAddress: string): void;
    function MagicSetScript(magicId: ushort, scrType: MagicScript, scrAddress: string): void;
    function EnemySetScript(enemyId: ushort, scrType: EnemyScript, scrAddress: string): void;
    function PoisonSetScript(poisonId: ushort, scrType: PoisonScript, scrAddress: string): void;

    /**
    * -  0x0091
    * 
    * @note
    * -  如果战场上有多个敌人与当前敌人编号一样，
    *    且当前敌人是其中的第一个，则跳转到 {scrAddress}
    * 
    * @param scrAddress 则跳转到的脚本地址
    */
    function JumpIfEnemyNotFirstOfKind(scrAddress: string): void;

    /**
    * -  0x0092
    * 
    * @note
    * -  播放队员 {role} 的施法动作，之后我方全体高亮（颜色偏移）
    * 
    * @param role 队员编号，缺省表示当前队员（缺省则没有人做出施法动作，直接全体高亮）
    */
    function ShowRoleMagicAction(role: Role): void;

    /**
    * -  0x0093
    * 
    * @note
    * -  屏幕淡出，期间更新场景
    * 
    * @param step 淡入步长
    */
    function VideoFadeAndUpdate(step: short): void;

    /**
    * -  0x0094
    * 
    * @note
    * -  如果事件 {sceneId eventId} 的状态为 {state}，则跳转到 {scrAddress}
    * 
    * @param sceneId 场景编号
    * @param eventId 事件编号
    * @param stateCode 显示状态
    * @param scrAddress 事件状态匹配则跳转到的地址
    */
    function JumpIfEventStateMatches(sceneId: short, eventId: short, stateCode: EventStateCode, scrAddress: string): void;

    /**
    * -  0x0095
    * 
    * @note
    * -  如果当前场景为 {sceneId}，则跳转到 {scrAddress}
    * 
    * @param sceneId 场景编号
    * @param scrAddress 场景匹配则跳转到的地址
    */
    function JumpIfCurrentSceneMatches(sceneId: ushort, scrAddress: string): void;

    /**
    * -  0x0096
    * 
    * @note
    * -  显示游戏通关后的动画
    * 
    * @param void 无
    */
    function ShowEndingAnimation(): void;

    /**
    * -  0x0098
    * 
    * @note
    * -  设置队伍随从，人数不限
    * 
    * @param sprite1Id 随从 1 形象编号
    * @param sprite2Id 随从 2 形象编号
    */
    function PartySetFollower(sprite1Id: ushort, sprite2Id: ushort): void;

    /**
    * -  0x0099
    * 
    * @note
    * -  将场景 {sceneId} 的地图设置为 {mapId}
    * 
    * @param sceneId 场景编号
    * @param mapId 地图编号
    */
    function SceneSetMap(sceneId: short_1, mapId: ushort): void;

    /**
    * -  0x009A
    * 
    * @note
    * -  将事件范围 {sceneBeginId.eventBeginId} ～ {sceneEndId.eventEndId} 的状态码设置为 {stateCode}
    * 
    * @param sceneBeginId 场景起始编号
    * @param eventBeginId 事件起始编号
    * @param sceneEndId 场景末尾编号
    * @param eventEndId 事件末尾编号
    * @param stateCode 显示状态
    */
    function EventSetStateSequence(sceneBeginId: ushort, eventBeginId: ushort, sceneEndId: ushort, eventEndId: ushort, stateCode: EventStateCode): void;

    /**
    * -  0x009C
    * 
    * @note
    * -  敌人分身出 {value} 个替身，数据完全克隆，分身失败则跳转到 {scrAddress}
    * 
    * @param value 分身的数量，缺省则为 1
    * @param scrAddress 分身失败则跳转到的脚本地址
    */
    function EnemyClone(value: ushort, scrAddress: string): void;

    /**
    * -  0x009E
    * 
    * @note
    * -  敌人召唤 {enemyId}，数量 {value}，若召唤失败则跳转到 {scrAddress}
    * 
    * @param enemyId 欲召唤的敌人编号（若为 -1 或 0 则克隆当前敌人，但数据为全新而非克隆）
    * @param value 欲召唤的数量
    * @param scrAddress 召唤失败则跳转到的脚本地址
    */
    function EnemySummonMonster(enemyId: short_1, value: ushort, scrAddress: string): void;

    /**
    * -  0x009F
    * 
    * @note
    * -  敌方变身为 {enemyId}
    * 
    * @param enemyId 敌人编号
    */
    function EnemyTransform(enemyId: ushort): void;

    /**
    * -  0x00A0
    * 
    * @note
    * -  退出游戏
    * 
    * @param void 无
    */
    function QuitGame(): void;

    /**
    * -  0x00A1
    * 
    * @note
    * -  将队伍的坐标设置为和领队重合
    * 
    * @param void 无
    */
    function PartySetPosToFirstRole(): void;

    /**
    * -  0x00A2
    * 
    * @note
    * -  随机跳转到后面指令 0 ～ {range} 中的任意一条指令
    * 
    * @param range 范围
    */
    function JumpToRandomInstruction(range: ushort): void;

    /**
    * -  0x00A3
    * 
    * @note
    * -  播放 CD {cd}，若 CD 不存在则播放 RIX {music}
    * 
    * @param cd 欲播放的音乐编号，若为 -1 则相当于 Nop 指令
    * @param music 欲播放的音乐编号
    */
    function PlayCDOrMusic(cd: CD, music: Music): void;

    /**
    * -  0x00A4
    * 
    * @note
    * -  将 Fbp {fbp} 滚动到屏幕，每帧滚动延迟 {800 / speed} 毫秒
    * 
    * @param fbp Fbp 图像编号
    * @param speed 滚动速度，缺省则为 800 毫秒
    */
    function ScrollFbp(fbp: FbpDos, speed: ushort): void;
    function ScrollFbp(fbp: FbpWin, speed: ushort): void;

    /**
    * -  0x00A5
    * 
    * @note
    * -  淡入被 320*200 大小的 Npc {spriteId} 覆盖的 Fbp 图像 {fbp}，速度 {speed}
    * 
    * @param fbp Fbp 图像编号
    * @param spriteId Npc 形象编号
    * @param speed 淡入速度，每步延迟 (speed + 1) * 10 毫秒，共 16 * 6 步，即 (speed + 1) * 960 毫秒
    */
    function ShowFbpWithSprite(fbp: FbpDos, spriteId: ushort, speed: ushort): void;
    function ShowFbpWithSprite(fbp: FbpWin, spriteId: ushort, speed: ushort): void;

    /**
    * -  0x00A6
    * 
    * @note
    * -  备份当前屏幕
    * 
    * @param void 无
    */
    function ScreenBackup(): void;

    /**
    * -  0x00A7
    * 
    * @note
    * -  将对话框设置到对象描述的位置，仙术/道具
    * 
    * @param void 无
    */
    function DlgItem(): void;

    /**
    * -  0xFFFF
    * 
    * @note
    * -  显示对话
    * 
    * @param Msg 对话内容
    */
    function Dlg(Msg: string): void;

} export { };
