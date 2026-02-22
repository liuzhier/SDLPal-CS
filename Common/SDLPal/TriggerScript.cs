using ModTools.Unpack;
using Records.Mod.RGame;
using SDL3;
using System;
using System.Reflection.PortableExecutable;

namespace SDLPal;

public static unsafe partial class PalScript
{
    static int TriggerSceneIDLast { get; set; } = -1;
    static int TriggerEventIDLast { get; set; } = -1;

    /// <summary>
    /// 执行事件的触发脚本
    /// </summary>
    /// <param name="address">要执行的脚本条目地址</param>
    /// <param name="sceneId">调用脚本的事件所在的场景的编号</param>
    /// <param name="eventId">调用脚本的事件的编号</param>
    /// <param name="comment">调试描述信息</param>
    /// <returns>下一条要执行的脚本条目的地址</returns>
    public static int RunTrigger(int address, int sceneId, int eventId, string comment = null!)
    {
        var addressNext = address;
        var ended = false;
        //var updatedInBattle = false;
        var @event = (Event)null!;

        if (eventId == -1)
        {
            //
            // 获取当前场景编号和事件编号
            //
            sceneId = TriggerSceneIDLast;
            eventId = TriggerEventIDLast;
        }

        //
        // 备份当前场景编号和事件编号
        //
        TriggerSceneIDLast = sceneId;
        TriggerEventIDLast = eventId;

        if (sceneId != 0 && eventId != 0 && eventId != -1)
        {
            //
            // 获取是哪个事件触发的脚本
            //
            var scene = S.GetScene(sceneId);
            if (eventId < scene.Events.Count) @event = S.GetEvent(sceneId, eventId);
        }

        ScriptSuccess = true;

        //
        // 设置对话框默认输出速度
        //
        PalDialog.SetOutputDelay();

        var prefix = $"Trigger[{((comment != null || @event == null) ? comment : $"({((sceneId == -1) ? S.Save.SceneId : sceneId)}-{eventId}){@event!.Name}")}]";

        while (address != 0 && !ended)
        {
            var script = Scripts[address];
            var command = script.Command;
            var args = script.Args;

            //
            // 显示脚本注解
            //
            LogAsmMessage(address, prefix);

            switch (command)
            {
                case 0x0000:
                    /**
                    * @note
                    * -  停止执行
                    * 
                    * @param void 无
                    */
                    ended = true;
                    break;

                case 0x0001:
                    /**
                    * @note
                    * -  暂停执行，将 调用地址 替换为 下一条命令
                    * 
                    * @param void 无
                    */
                    ended = true;
                    addressNext = address + 1;
                    break;

                case 0x0002:
                    /**
                    * @note
                    * -  暂停执行，将 调用地址 替换为 地址 {scrAddress}；
                    *    累计触发 {count} 次后，将 调用地址 替换为 下一条命令
                    * 
                    * @param scrAddress 欲跳转到的地址
                    * @param count 最大可触发的次数，若设置为 0 则为总是可触发
                    */
                    if (args[1].Int == 0 || ++(@event!.Script.TriggerIdleFrame) < args[1].Int)
                    {
                        ended = true;
                        address = args[0].Address;
                    }
                    else
                    {
                        //
                        // 失败
                        //
                        @event.Script.TriggerIdleFrame = 0;
                        address++;
                    }
                    break;

                case 0x0003:
                    /**
                    * @note
                    * -  跳转到地址 {scrAddress}，完成跳转地址的执行后脚本结束；
                    *    累计触发 {count} 次后，该指令将成为 NOP
                    * 
                    * @param scrAddress 欲跳转到的地址
                    * @param count 最大可触发的次数，若设置为 0 则为总是可触发
                    */
                    if (args[1].Int == 0 || ++(@event!.Script.TriggerIdleFrame) < args[1].Int)
                        address = args[0].Address;
                    else
                    {
                        //
                        // 失败
                        //
                        @event.Script.TriggerIdleFrame = 0;
                        address++;
                    }
                    break;

                case 0x0004:
                    /**
                    * @note
                    * -  调用地址 {scrAddress}，完成调用后返回并继续执行；
                    *    参数 {sceneId} 和 {eventId} 只有 Event 触发脚本时才用到；
                    *    注意：不是只有玩家才能触发脚本，Event 也是可以主动触发脚本的！
                    * 
                    * @param scrAddress 欲跳转到的地址
                    * @param sceneId 当前事件所在的场景Id
                    * @param eventId 当前事件Id
                    */
                    RunTrigger(args[0].Address, args[1].Bool ? args[1].Int : sceneId, args[2].Bool ? args[2].Int : eventId);
                    address++;
                    break;

                case 0x0005:
                    /**
                    * @note
                    * -  若屏幕上有对话则会先等待按下任意键，之后再更新画面。
                    *    更新队伍步伐 {updatePartyGestures}，画面更新后延迟 {delay * 60} ms
                    * 
                    * @param delay 画面更新后延迟的毫秒数，缺省则为 1，实际延迟 {delay * 60} ms
                    * @param updatePartyGestures 当 RNG 未播放 或 未进入战斗时，是否更新队伍步伐
                    */
                    PalDialog.ClearMesssge(true);

                    if (PalGlobal.IsPlayingCurrentAnimationId)
                        PalScreen.Restore(PalScreen.Main);
                    else if (PalGlobal.InBattle)
                    {
                        //PAL_BattleMakeScene();
                        //Screen.Copy(g_Battle.lpSceneBuf, gpScreen);
                        PalScreen.Update();
                    }
                    else
                    {
                        if (args[1].Bool)
                            //
                            // 更新队伍步伐
                            //
                            PalScene.UpdateHeroTeamGestures(false);

                        PalScene.Draw();
                        PalScreen.Update();

                        PalTimer.Delay((args[0].Int == 0) ? 60 : (args[0].Int * 60));
                    }
                    address++;
                    break;

                case 0x0006:
                    /**
                    * @note
                    * -  生成随机数1~100，若大于 {probability}，则跳转到地址 {scrAddress}；
                    *    否则继续执行。
                    * 
                    * @param probability 0~100 的数值
                    * @param scrAddress 欲跳转到的地址
                    */
                    if (S.RandomLong(1, 100) >= args[0].Int)
                    {
                        address = args[1].Address;
                        continue;
                    }
                    else address++;
                    break;

                case 0x0007:
                    /**
                    * @note
                    * -  进入战斗，将敌方队列 {enemyTeamId} 放入战场；
                    *    若战斗失败，则调用参数二指向的地址；
                    *    若战斗胜利，则调用到参数三指向的地址
                    * 
                    * @param enemyTeamId 敌方队列 Id 
                    * @param scrDefeat 战斗失败脚本
                    * @param scrFleed 战斗逃跑脚本（Boss 战这里为 0，无法逃跑）
                    */
                    address++;
                    break;

                case 0x0008:
                    /**
                    * @note
                    * -  将 调用地址 替换为 下一条命令
                    * 
                    * @param void 无
                    */
                    addressNext = ++address;
                    break;

                case 0x0009:
                    /**
                    * @note
                    * -  所有事件的自动脚本循环 {frameNum} 帧；
                    *    期间禁止用户一切操作（包括触发事件），仅场景中的所有事件在运转
                    * 
                    * @param frameNum 循环帧数
                    * @param canTriggerEvent 是否允许触发事件
                    * @param updatePartyGestures 是否更新队伍步伐
                    */
                    {
                        PalDialog.ClearMesssge(true);

                        var time = SDL.GetTicks() + PalScene.FrameTime;

                        var frameNum = (args[0].Bool ? args[0].Int : 1);
                        for (var i = 0; i < frameNum; i++)
                        {
                            PalTimer.DelayUntil(time);

                            time = SDL.GetTicks() + PalScene.FrameTime;

                            if (args[2].Bool)
                                //
                                // 更新队伍步伐
                                //
                                PalScene.UpdateHeroTeamGestures(false);

                            PalPlay.GameUpdate(args[1].Bool);
                            PalScene.Draw();
                            PalScreen.Update();
                        }
                    }
                    address++;
                    break;

                case 0x000A:
                    /**
                    * @note
                    * -  显示选项【"是" "否"】；
                    *    选【"是"】则继续执行；
                    *    选【"否"】则跳转到参数一指向的地址
                    * 
                    * @param scrAddress 选 "否" 则跳转到的地址
                    */
                    PalDialog.ClearMesssge(false);
                    address = (!PalUiGame.ConfirmMenu(ref PalAtlas.Scene)) ? args[0].Address : ++address;
                    break;

                case 0x003B:
                    /**
                    * @note
                    * -  将对话框位置设置在画面中间，头像为 {faceId}，字体颜色为 {colorHex}
                    * 
                    * @param colorId 调色板颜色索引
                    * @param rngPlaying 是否正在播放 RNG 动画
                    */
                    PalDialog.ClearMesssge(true);
                    PalDialog.InitMesssge(DialogPosition.Middle, color: S.StrToUInt32(args[0].String), isPlayingRng: args[1].Bool);
                    address++;
                    break;

                case 0x003C:
                    /**
                    * @note
                    * -  将对话框位置设置在画面上部，头像为 {faceId}，字体颜色为 {colorId}，是否正在播放 Rng 动画 {rngIsPlaying}
                    * 
                    * @param faceId 对话框上肖像的编号
                    * @param colorId 调色板颜色索引
                    * @param rngIsPlaying 是否正在播放 Rng 动画
                    */
                    PalDialog.ClearMesssge(true);
                    PalDialog.InitMesssge(DialogPosition.Top, color: S.StrToUInt32(args[1].String), faceId: args[0].Int, isPlayingRng: args[2].Bool);
                    address++;
                    break;

                case 0x003D:
                    /**
                    * @note
                    * -  将对话框位置设置在画面下部，头像为 {faceId}，字体颜色为 {colorId}，是否正在播放 Rng 动画 {rngIsPlaying}
                    * 
                    * @param faceId 对话框上肖像的编号
                    * @param colorId 调色板颜色索引
                    * @param rngIsPlaying 是否正在播放 Rng 动画
                    */
                    PalDialog.ClearMesssge(true);
                    PalDialog.InitMesssge(DialogPosition.Bottom, color: S.StrToUInt32(args[1].String), faceId: args[0].Int, isPlayingRng: args[2].Bool);
                    address++;
                    break;

                case 0x003E:
                    /**
                    * @note
                    * -  将对话框位置设置在画面中间的条幅里，字体颜色为 {colorHex}
                    * 
                    * @param colorHex 调色板颜色字符串，支持 Dec Hex，示例："0xFF0000"（红色）
                    */
                    PalDialog.ClearMesssge(true);
                    PalDialog.InitMesssge(DialogPosition.Middle, color: S.StrToUInt32(args[0].String));
                    address++;
                    break;

                case 0x008E:
                    /**
                    * @note
                    * -  还原屏幕
                    * 
                    * @param void 无
                    */
                    PalDialog.NextPage();
                    address++;
                    break;

                case 0xFFFF:
                    /**
                    * @note
                    * -  显示对话
                    * 
                    * @param msg 对话内容
                    */
                    PalDialog.DrawTalkText(args[0].String);
                    address++;
                    break;

                default:
                    PalDialog.ClearMesssge(true);
                    address = RunPublic(address, sceneId, eventId);
                    break;
            }
        }

        PalDialog.EndMesssge();

        return addressNext;
    }
}
