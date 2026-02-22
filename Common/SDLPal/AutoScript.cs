namespace SDLPal;

public static unsafe partial class PalScript
{
    /// <summary>
    /// 执行事件的自动脚本
    /// </summary>
    /// <param name="address">要执行的脚本条目地址</param>
    /// <param name="sceneId">调用脚本的事件所在的场景的编号</param>
    /// <param name="eventId">调用脚本的事件的编号</param>
    /// <returns></returns>
    public static int RunAuto(int address, int eventId)
    {
    begin:
        var script = Scripts[address];
        var command = script.Command;
        var args = script.Args;
        var @event = S.GetEvent(-1, eventId);

        //
        // 显示脚本注解
        //
        var sceneId = S.Save.SceneId;
        LogAsmMessage(address, $@"Auto[({sceneId}-{eventId}){@event.Name}]");

        //
        // 对于自动脚本程序，我们应当在每一帧中执行一条指令
        // （跳转指令除外），并保存下一条指令的地址
        //
        switch (command)
        {
            case 0x0000:
                /**
                * @note
                * -  停止执行
                * 
                * @param void 无
                */
                break;

            case 0x0001:
                /**
                * @note
                * -  暂停执行，将 调用地址 替换为 下一条命令
                * 
                * @param void 无
                */
                address++;
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
                if (args[1].Int == 0 || ++(@event.Script.AutoIdleFrame) < args[1].Int)
                    address = args[0].Address;
                else
                {
                    @event.Script.AutoIdleFrame = 0;
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
                if (args[1].Int == 0 || ++(@event.Script.AutoIdleFrame) < args[1].Int)
                {
                    address = args[0].Address;
                    goto begin;
                }
                else
                {
                    //
                    // 失败
                    //
                    @event.Script.AutoIdleFrame = 0;
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
                    if (args[1].Address != 0)
                    {
                        address = args[1].Address;
                        goto begin;
                    }
                }
                else address++;
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
                if (++(@event.Script.AutoIdleFrame) >= args[0].Int)
                {
                    //
                    // waiting ended; go further
                    //
                    @event.Script.AutoIdleFrame = 0;
                    address++;
                }
                break;

            default:
                address = RunPublic(address, sceneId, eventId);
                break;
        }

        return address;
    }
}
