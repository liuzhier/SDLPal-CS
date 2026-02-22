using Records.Mod;
using Records.Mod.RGame;
using Records.Pal;
using SDL3;
using System;

namespace SDLPal;

public static partial class PalScript
{
    public static bool ScriptSuccess { get; set; } = true;
    public static bool UpdatedInBattle { get; set; }

    /// <summary>
    /// 执行事件的触发脚本
    /// </summary>
    /// <param name="address">要执行的脚本条目地址</param>
    /// <param name="sceneId">调用脚本的事件所在的场景的编号</param>
    /// <param name="eventId">调用脚本的事件的编号</param>
    /// <returns>下一条要执行的脚本条目的地址</returns>
    public static int RunPublic(int address, int sceneId, int eventId)
    {
        var scene = (Scene)null!;
        var @event = (Event)null!;

        if (sceneId != 0)
        {
            scene = S.GetScene(sceneId);

            if (eventId != 0 && eventId != -1 && eventId < scene.Events.Count) @event = S.GetEvent(eventId);
        }

        var scr = Scripts[address];
        var cmd = scr.Command;
        var args = scr.Args;
        var save = S.Save;

        var heroTeamId = 0;
        var hero = (Hero)null!;
        if (args.Length >= 1)
        {
            //
            // 获取事件编号参数
            //
            var arg0 = args[0].Int;
            if (arg0 < 0 || arg0 >= Base.MaxHero) arg0 = 0;
            heroTeamId = S.GetMember(arg0).HeroId;
            hero = save.Entity.Heroes[heroTeamId];
        }

        var currentSceneId = -1;
        var currentEvent = (Event)null!;
        var currentEventId = 0;
        var eventTrail = (Trail)null!;
        if (args.Length >= 2)
        {
            //
            // 获取事件编号参数
            //
            currentEventId = args[1].Event;
            if (currentEventId <= 0)
            {
                currentEvent = @event;
                currentEventId = eventId;
            }
            else
            {
                //
                // 获取场景编号参数
                //
                currentSceneId = args[0].Scene;

                if ((currentSceneId == -1) || (currentSceneId > 0)) currentEvent = S.TryGetEvent(currentSceneId, currentEventId);
            }

            //
            // 获取步伐数据
            //
            eventTrail = currentEvent?.Trail;
        }

        var bposTmp = (args.Length >= 3) ? new BlockPos(args[0].BX, args[1].BY, args[2].BH) : null!;

        switch (cmd)
        {
            case 0x000B:
            case 0x000C:
            case 0x000D:
            case 0x000E:
            case 0x0087:
                /**
                * -  0x000B; 0x000C; 0x000D; 0x000E; 0x0087
                * 
                * -  当前事件播放 {DirectionId} 方向行走动画（X += 2 * 速度，Y += 1 * 速度）；
                *    0x000B：向西（左下）行，速度 2（X += 4，Y += 1）；
                *    0x000C：向北（左上）行，速度 2；
                *    0x000D：向东（右上）行，速度 2；
                *    0x000E：向南（右下）行，速度 2；
                *    0x0087：向当前方向行，速度 0（X += 0，Y += 0，原地播放行走动画）
                * 
                * @param direction 行走方向（移动速度是定值）
                */
                if (args[0].Direction != PalDirection.Current)
                    @event.Trail.Direction = args[0].Direction;
                PalScene.NpcWalkOneStep(sceneId, eventId, cmd switch
                {
                    0x0087 => 0,
                    _ => 2,
                });
                break;

            case 0x000F:
                /**
                * @note
                * -  当前对象面向方向 {direction}，帧编号 {frameId}
                * 
                * @param direction 方向
                * @param frameId 帧编号，大多数第一帧为原地静止，后两帧为行走
                */
                if (args[0].Direction != PalDirection.Current) @event.Trail.Direction = args[0].Direction;
                if (args[1].Int != -1) @event.Trail.FrameId = args[1].Int;
                break;

            case 0x0010:
            case 0x0082:
                /**
                * @note
                * -  当前对象走到块坐标 {bx, by, bh}，待对象到达终点才会继续执行下一条指令；
                *    0x0010：速度 3；
                *    0x0082：速度 8
                * 
                * @param bx 欲走到块的 X 坐标
                * @param by 欲走到块的 Y 坐标
                * @param bh 欲走到的块的 左右板块Id
                * @param speed 移动速度
                */
                if (!PalScene.NpcWalkTo(sceneId, eventId, bposTmp, args[3].Int)) address--;
                break;

            case 0x0011:
            case 0x007C:
                /**
                * @note
                * -  当前对象走到块坐标 {bx, by, bh}，奇偶（帧）互斥，待对象到达终点才会继续执行下一条指令；
                *    0x0011：速度 2，奇偶互斥；
                *    0x007C：速度 4，奇偶互斥
                * 
                * @param bx 欲走到块的 X 坐标
                * @param by 欲走到块的 Y 坐标
                * @param bh 欲走到的块的 左右板块Id
                * @param speed 移动速度
                */
                if (((eventId & 1) ^ (PalGlobal.FrameNum & 1)) != 0)
                {
                    if (!PalScene.NpcWalkTo(sceneId, eventId, bposTmp, args[3].Int)) address--;
                }
                else address--;
                break;

            case 0x0012:
                /**
                * @note
                * -  将事件 {sceneId eventId} 的坐标设置到相对于 [队伍领队] 的点 {x, y}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param x 欲走到的 X 坐标
                * @param y 欲走到的 Y 坐标
                */
                var pos = S.GetHeroTeamPos();
                eventTrail!.Pos.X = pos.X + args[2].X;
                eventTrail.Pos.Y = pos.Y + args[3].Y;
                break;

            case 0x0013:
                /**
                * @note
                * -  将事件 {sceneId eventId} 的坐标设置到绝对点 {x, y}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param x 欲设置到的 X 坐标
                * @param y 欲设置到的 Y 坐标
                */
                eventTrail!.Pos.X = args[2].X;
                eventTrail.Pos.Y = args[3].Y;
                break;

            case 0x0014:
                /**
                * @note
                * -  将当前事件的帧号设置为 {FrameId}，并把方向设置为 {Direction.Southwest}
                * 
                * @param frameId 帧编号，第一帧为原地，后两帧为走路
                */
                @event.Trail.FrameId = args[0].Int;
                @event.Trail.Direction = PalDirection.South;
                break;

            case 0x0015:
                /**
                * @note
                * -  将 {party} 的方向设置为 {direction}，帧号设置为 {frameId}
                * 
                * @param direction 方向（负数则方向不变）
                * @param frameId 帧编号，第一帧为原地，后两帧为走路
                * @param party 队伍中 Role 的编号
                */
                S.SetMemberDirection(args[2].Int, args[0].Direction);
                S.GetMemberTrail(args[2].Int).FrameId = args[1].Int;
                break;

            case 0x0016:
                /**
                * @note
                * -  将事件 {sceneId eventId} 的方向设置为 {direction}，帧号设置为 {frameId}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param direction 方向（负数则方向不变） 【西-左下[0] 北-左上[1] 东-右上[2] 南-右下[3]】
                * @param frameId 帧编号，第一帧为原地，后两帧为走路
                */
                if (args[0].Bool && args[1].Bool)
                {
                    eventTrail!.Direction = args[2].Direction;
                    eventTrail.FrameId = args[3].Int;
                }
                break;

            case 0x001D:
                /**
                * @note
                * -  我方 {applyToAll} MP 变动 {value}
                * 
                * @param applyToAll 是否作用于我方全体，缺省则为当前队员
                * @param value 欲变动的值，正数增加，负数减少
                */
                if (args[0].Bool)
                    //
                    // 作用于全体
                    //
                    for (var i = 0; i < save.HeroTeamLength; i++) S.GetMemberHero(i).ModifyHPMP(args[1].Int, args[1].Int);
                else
                    //
                    // 作用于单人
                    // 参数“eventId”代表 HeroId
                    //
                    if (!S.GetMemberHero(eventId).ModifyHPMP(args[1].Int, args[1].Int)) ScriptSuccess = false;
                break;

            case 0x001E:
                /**
                * @note
                * -  金钱变动 {value}
                * 
                * @param value 欲变动的值，正数增加，负数减少，
                * @param scrAddress 金钱不足时跳转到的地址
                */
                if (args[0].Int < 0 && save.Money < -args[0].Int)
                {
                    //
                    // 金钱不足
                    //
                    address = args[1].Address - 1;
                }
                else save.Money += args[0].Int;
                break;

            case 0x001F:
                /**
                * @note
                * -  将道具 {itemId} 添加到背包中，数量 {value}
                * 
                * @param itemId 道具编号
                * @param value 道具数量，缺省则为 1
                */
                save.AddInventory(args[0].Int, args[1].Bool ? args[1].Int : 1);
                break;

            case 0x0020:
                /**
                * @note
                * -  将道具 {itemId} 在背包中删除，数量 {value}。背包中道具不足，则会从角色身上卸下；
                *    若道具不足，则跳转到地址 {scrAddress}
                * 
                * @param itemId 道具编号
                * @param value 道具数量
                * @param scrAddress 道具不足时跳转到的地址
                */
                {
                    var itmeId = args[0].Int;
                    var item = args[0].Item;
                    var num = int.Max(args[1].Int, 1);

                    //
                    // 统计此物品的实际存量数量
                    //
                    if (num <= save.GetInventoryCount(itmeId) || (args[3].Address == 0))
                    {
                        if (!save.AddInventory(itmeId, -num).IsSuccess)
                            //
                            // 尝试删除已装备的物品
                            //
                            for (var i = 0; i < save.HeroTeamLength; i++)
                                if (S.GetMemberHero(i).RemoveEquipment(item) && (--num == 0))
                                    goto case_0x0020_end;
                    }
                    else
                        address = args[3].Address - 1;
                }
            case_0x0020_end:
                break;

            case 0x0022:
                /**
                * @note
                * -  我方 {applyToAll} 复活，HP 恢复 {value} %
                * 
                * @param applyToAll 是否作用于我方全体，缺省则为当前队员
                * @param value 欲恢复 HP 的百分比
                */
                if (args[0].Bool)
                {
                    //
                    // 作用于全体
                    //
                    ScriptSuccess = false;

                    for (var i = 0; i < save.HeroTeamLength; i++)
                    {
                        hero = S.GetMemberHero(i);
                        var power = hero.Power;

                        //
                        // 只作用于已经阵亡的 Hero
                        //
                        if (power.HP == 0)
                        {
                            power.HP = power.MaxHP * (args[1].Int / 10);

                            //
                            // 解 3 级以下的毒
                            //
                            hero.RemovePoisonStatusByLevel(3);

                            //
                            // 清理所有特殊状态
                            //
                            hero.RemoveAllStatus();

                            //
                            // 标记脚本状态为成功
                            //
                            ScriptSuccess = true;
                        }
                    }
                }
                else
                {
                    //
                    // 作用于单人
                    //
                    hero = S.GetMemberHero(eventId);
                    var power = hero.Power;

                    //
                    // 只作用于已经阵亡的 Hero
                    //
                    if (power.HP == 0)
                    {
                        power.HP = power.MaxHP * (args[1].Int / 10);

                        //
                        // 解 3 级烈度以下的毒
                        //
                        hero.RemovePoisonStatusByLevel(3);

                        //
                        // 清理所有特殊状态
                        //
                        hero.RemoveAllStatus();
                    }
                    else ScriptSuccess = false;
                }
                break;

            case 0x0024:
                /**
                * @note
                * -  将事件 {sceneId eventId} 的自动脚本设置为 {scrAddress}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param scrAddress 欲设置的脚本地址
                */
                if (args[1].Event != 0) currentEvent!.Script.Auto.Value = args[2].Address;
                break;

            case 0x0025:
                /**
                * @note
                * -  将事件 {sceneId eventId} 的触发脚本设置为 {scrAddress}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param scrAddress 欲设置的脚本地址
                */
                if (args[1].Event != 0) currentEvent!.Script.Trigger.Value = args[2].Address;
                break;

            case 0x0036:
                /**
                * @note
                * -  将 rng 动画设置为 {rngId}
                * 
                * @param rngId RNG 动画编号
                */
                PalGlobal.CurrentAnimationId = args[0].Int;
                break;

            case 0x0037:
                /**
                * @note
                * -  播放 RNG 动画，从第 {beginFrameId} 帧开始播放，播放到 {endFrameId} 帧终止，速度 {speed}
                * 
                * @param beginFrameId 起始帧号
                * @param endFrameId 结束帧号，缺省则为播放到最后一帧
                * @param speed 播放速度，缺省则为 16
                */
                {
                    var endFrameId = args[1].Int > 0 ? args[1].Int : -1;
                    PalAnimation.Play(PalGlobal.CurrentAnimationId, args[0].Int, endFrameId: endFrameId, speed: args[2].Int);
                }
                break;

            case 0x003F:
            case 0x0044:
            case 0x0097:
                /**
                * @note
                * -  乘坐当前事件行至块坐标 {bx, by, bh}；
                *    0x003F：速度 2；
                *    0x0044：速度 4；
                *    0x0097：速度 8
                * 
                * @param bx 欲走到块的 X 坐标
                * @param by 欲走到块的 Y 坐标
                * @param bh 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
                * @param speed 行驶速度
                */
                PalScene.HeroTeamRideEventObject(sceneId, eventId, bposTmp, args[3].Int);
                break;

            case 0x0040:
                /**
                * @note
                * -  将事件 {sceneId eventId} 的触发方式设置为 {isAutoTrigger}，触发范围为 {triggerDistance}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param isAutoTrigger 是否走到触发范围内自动触发
                * @param triggerDistance 触发范围（手动触发的范围限制在 1～3，自动触发范围不限）
                */
                if (args[1].Event != 0)
                {
                    currentEvent!.Trigger.IsAutoTrigger = args[2].Bool;
                    currentEvent.Trigger.Range = args[3].Int;
                }
                break;

            case 0x0043:
                /**
                * @note
                * -  设置场景音乐为 {music}，循环播放 {loop}，是否淡入淡出 {needFade}
                * 
                * @param music 欲播放的音乐编号
                * @param loop 是否循环播放
                * @param needFade 是否淡入淡出，时间 3 秒；但音乐 9 无法淡入淡出
                */
                save.MusicId = args[0].Int;
                PalAudio.PlayMusic(save.MusicId, args[1].Bool, args[2].Bool ? 3000 : 0);
                break;

            case 0x0045:
                /**
                * @note
                * -  设置战斗音乐为 {music}
                * 
                * @param music 欲播放的音乐编号
                */
                save.BattleMusicId = args[0].Int;
                break;

            case 0x0046:
                /**
                * @note
                * -  将队伍的块坐标设置为 {bx, by, bh}
                * 
                * @param bx 欲走到块的 X 坐标
                * @param by 欲走到块的 Y 坐标
                * @param bh 欲走到的块的 左右板块Id；【左板块[0] 右板块[1]】
                */
                S.SetHeroTeamPos(Pos.FromBlockPos(bposTmp));
                //PalScene.UpdateHeroTeamPos(Pos.FromBlockPos(bposTmp));
                PalScene.UpdateHeroTeamLinearPos();
                break;

            case 0x0047:
                /**
                * @note
                * -  播放音效 {soundId}
                * 
                * @param soundId 欲播放音效的编号
                */
                PalAudio.PlayVoice(args[0].Int);
                break;

            case 0x0049:
                /**
                * @note
                * -  设置事件 {sceneId eventId} 的状态码为 {stateCode}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param stateCode 显示状态
                */
                if (args[1].Event != 0) currentEvent!.Trigger.StateCode = args[2].StateCode;
                break;

            case 0x004A:
                /**
                * @note
                * -  设置战斗环境为 {fbp}
                * 
                * @param fbp 战斗环境编号
                */
                save.BattleFieldId = args[0].Int;
                break;

            case 0x004C:
                /**
                * @note
                * -  当前事件追逐队伍，警戒范围 {range}，追逐速度 {speed}
                * 
                * @param range 警戒范围，队伍走范围内开始被追逐，缺省则为 8
                * @param speed 追逐速度，缺省则为 4
                * @param canFly 能够穿墙追逐角色
                */
                {
                    var range = args[0].Int;
                    var speed = args[1].Int;

                    if (range == 0) range = 8;
                    if (speed == 0) speed = 4;

                    PalScene.MonsterChasePlayer(sceneId, eventId, range, speed, args[2].Bool);
                }
                break;

            case 0x0050:
                /**
                * @note
                * -  屏幕淡出
                * 
                * @param delay 每一步的延迟时间，缺省则为 1（实际延迟：{delay} * 600 ms）
                */
                PalScreen.Update();
                PalScreen.Fade(args[0].Bool ? args[0].Int : 1);
                PalGlobal.NeedToFadeIn = true;
                break;

            case 0x0051:
                /**
                * @note
                * -  屏幕淡入
                * 
                * @param delay 每一步的延迟时间，小于 0 则为 1（实际延迟：{delay} * 600 ms）
                */
                PalScreen.Update();
                PalScreen.Fade(args[0].Bool ? args[0].Int : 1, isFadeOut: false);
                PalGlobal.NeedToFadeIn = false;
                break;

            case 0x0052:
                /**
                * @note
                * -  当前对象隐藏 {frameNum} 帧
                * 
                * @param frameNum 欲隐藏的帧数，缺省则为 800 帧
                */
                {
                    var trigger = @event.Trigger;
                    trigger.StateCode = (Core.EventState)((short)trigger.StateCode * -1);
                    trigger.VanishTime = (args[0].Int != 0) ? args[0].Int : 800;
                }
                break;

            case 0x0053:
            case 0x0054:
                /**
                * @note
                * -  设置时间段滤镜
                * 
                * @param filter 时间段滤镜类型
                */
                save.TimeFilter = args[0].TimeFilter;
                break;

            case 0x0055:
                /**
                * @note
                * -  英雄 {hero} 练成仙术 {magicId}
                * 
                * @param magicId 欲习得仙术的编号
                * @param hero 英雄编号，缺省表示当前队员
                */
                S.Entity.Heroes[args[0].Int].AddMagic(args[1].Int);
                break;

            case 0x0056:
                /**
                * @note
                * -  英雄 {hero} 丧失仙术 {magicId}
                * 
                * @param magicId 欲丧失仙术的编号
                * @param hero 英雄编号，缺省表示当前队员
                */
                S.Entity.Heroes[args[0].Int].RemoveMagic(args[1].Int);
                break;

            case 0x0059:
                /**
                * @note
                * -  切换到场景 {sceneId}
                * 
                * @param sceneId 场景编号
                */
                if (args[0].Int > 0 && args[0].Int <= save.Scenes.Count && save.SceneId != args[0].Int)
                {
                    //
                    // 设置数据以便在下一帧加载场景
                    //
                    save.SceneId = args[0].Int;
                    PalResource.SetLoadFlags(PalResource.LoadFlag.Scene);
                    PalGlobal.EnterScene = true;
                    save.HeroTeamLayerOffset = 0;
                }
                break;

            case 0x0065:
                /**
                * @note
                * -  将英雄 {heroId} 的形象设置为 {spriteId}
                * 
                * @param heroId 英雄编号
                * @param spriteId 形象编号
                * @param applyImmediately 是否立即生效（在 Scene 时才有效）
                */
                S.Entity.Heroes[args[0].Int + 1].Sprite.SpriteId = args[1].Int;
                if (!PalGlobal.InBattle && args[2].Bool)
                {
                    PalResource.SetLoadFlags(PalResource.LoadFlag.HeroSprite);
                    PalResource.Load();
                }
                break;

            case 0x006C:
                /**
                * @note
                * -  事件 {sceneId eventId} 走一步，坐标移动 {x y}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param x X 坐标移动的量
                * @param y Y 坐标移动的量
                */
                eventTrail!.Pos.X += args[2].Short;
                eventTrail.Pos.Y += args[3].Short;
                PalScene.NpcWalkOneStep(currentSceneId, currentEventId, 0);
                break;

            case 0x006D:
                /**
                * @note
                * -  设置场景 {sceneId} 的脚本为 {scrEnter scrTeleport}（若二者皆为缺省时才都置 0）
                * 
                * @param sceneId 场景编号
                * @param scrEnter 进场脚本，缺省则不设置（单独缺省不会置 0）
                * @param scrTeleport 土遁脚本，缺省则不设置（单独缺省不会置 0）
                */
                if (args[0].Int != 0)
                {
                    scene = S.GetScene(args[0].Int);

                    if (args[1].Address != 0) scene.Script.Enter.Value = args[1].Address;
                    if (args[2].Address != 0) scene.Script.Teleport.Value = args[2].Address;
                }
                break;

            case 0x006E:
                /**
                * @note
                * -  队伍走一步，坐标移动 {x y}，图层变为 {Layer}
                * 
                * @param x X 坐标移动的量
                * @param y Y 坐标移动的量
                * @param layer 图层；实际图层数为 Layer * 8
                */
                pos = S.GetHeroTeamPos();
                pos.X += args[0].X;
                pos.Y += args[1].Y;
                PalScene.UpdateHeroTeamPos(pos);

                save.HeroTeamLayerOffset = args[2].Int * 8;

                if (args[0].X != 0 || args[1].Y != 0) PalScene.UpdateHeroTeamGestures(true);
                break;

            case 0x006F:
                /**
                * @note
                * -  当前事件与事件 {sceneId eventId} 同步为同一个状态
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param stateCode 显示状态
                */
                if (currentEvent!.Trigger.StateCode == args[2].StateCode)
                    @event.Trigger.StateCode = args[2].StateCode;
                break;

            case 0x0070:
            case 0x007A:
            case 0x007B:
                /**
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
                PalScene.HeroTeamWalkTo(bposTmp, args[3].Int);
                break;

            case 0x0073:
            case 0x009B:
                /**
                * @note
                * -  淡入到场景 {sceneId}，速度 {speed}；
                *    0x0073：当前场景
                * 
                * @param speed 淡入速度
                * @param sceneId 场景编号
                */
                PalScreen.Backup(PalScreen.Main);
                PalScene.Draw();
                PalScreen.FadeToScreen(args[0].Int);
                break;

            case 0x0075:
                /**
                * @note
                * -  设置队伍中的队员，并自动计算和设置队伍人数
                * 
                * @param roles Hero 编号序列串
                */
                save.HeroTeamLength = 0;

                var count = 0;
                foreach (var heroIdChar in args[0].String)
                {
                    var heroId = S.StrToInt32(heroIdChar);
                    if (heroId == 0)
                        //
                        // 跳过无效编号
                        //
                        continue;

                    var member = save.Members[count];
                    member.HeroId = heroId;
                    member.Trail.Direction = S.GetHeroTeamDirection();

                    save.HeroTeamLength++;
                    count++;
                }

                if (save.HeroTeamLength == 0)
                {
                    //
                    // 梦幻 2.11 拓展的用法
                    // 队伍中没有人，默认在队伍里加入 Hero 角色一
                    //
                    S.GetMember(0).HeroId = 1;
                    save.HeroTeamLength = 1;
                }

                //
                // 重新加载 Hero 贴图
                //
                PalResource.SetLoadFlags(PalResource.LoadFlag.HeroSprite);
                PalResource.Load();

                //
                // 更新所有队员所装备物品的效果
                //
                PalGlobal.UpdateEquipEffect();
                break;

            case 0x0076:
                /**
                * @note
                * -  淡入 Fbp 图像 {fbp}，速度 {speed}
                * 
                * @param fbp Fbp 图像编号
                * @param speed 淡入速度
                */
                S.CleanUpTex(PalScreen.Main, 0x000000FF);
                PalDialog.ForceDrawOnMainScreen = true;
                break;

            case 0x0077:
                /**
                * @note
                * -  停止播放音乐
                * 
                * @param fadeTime 淡入淡出时间（实际时间：{fadeTime} * 3），缺省则直接为 2 秒
                */
                PalAudio.StopMusic();
                save.MusicId = 0;
                break;

            case 0x0078:
                /**
                * @note
                * -  战后返回地图
                * 
                * @param void 无
                */
                break;

            case 0x007D:
                /**
                * @note
                * -  将事件 {sceneId eventId} 的坐标变动 {x y}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param x X 坐标移动的量
                * @param y Y 坐标移动的量
                */
                eventTrail!.Pos.X += args[2].X;
                eventTrail.Pos.Y += args[3].Y;
                break;

            case 0x007F:
                /**
                * @note
                * -  将视口相对移动 {x y}，以 {frameNum} 帧完成移动
                * 
                * @param x X 坐标移动的量
                * @param y Y 坐标移动的量
                * @param frameNum 图层；若为 -1 ，回到原点时不更新画面，移动时不更新画面
                */
                {
                    //pos = S.GetMemberPos(0);

                    var x = args[0].X;
                    var y = args[1].Y;

                    if (x == 0 && y == 0)
                    {
                        //
                        // 将视口恢复到正常状态
                        //
                        PalViewport.Offset.X = x;
                        PalViewport.Offset.Y = y;

                        if (args[2].Int != -1)
                        {
                            PalScene.Draw();
                            PalScreen.Update();
                        }
                    }
                    else
                    {
                        uint     time;

                        var i = 0;
                        time = (uint)(SDL.GetTicks() + PalScene.FrameTime);

                        do
                        {
                            if (args[2].Int == -1)
                            {
                                PalViewport.Offset.X = x * 32;
                                PalViewport.Offset.Y = y * 16;
                            }
                            else
                            {
                                PalViewport.Offset.X += x;
                                PalViewport.Offset.Y += y;
                            }

                            if (args[2].Int != -1)
                            {
                                PalPlay.GameUpdate(false);
                            }

                            PalScene.Draw();
                            PalScreen.Update();

                            //
                            // 延迟一帧
                            //
                            PalTimer.DelayUntil(time);
                            time = (uint)(SDL.GetTicks() + PalScene.FrameTime);
                        } while (++i < args[2].Int && args[2].Int != -1);
                    }
                }
                break;

            case 0x0080:
                /**
                * @note
                * -  昼夜调色板切换
                * 
                * @param applyImmediately 是否立即生效
                */
                {
                    var isSoon = (save.TimeFilter == PalFilter.Noon);

                    save.TimeFilter = PalFilter.Night;
                    PalScreen.FadeToFilter(PalFilter.Night, args[0].Bool, isFadeOut: isSoon);
                    save.TimeFilter = isSoon ? PalFilter.Night : PalFilter.Noon;
                }
                break;

            case 0x0081:
                /**
                * @note
                * -  若队伍领队面向事件 {sceneId eventId}，则将该事件的触发方式设置为 {isAutoTrigger}，触发范围为 {triggerDistance}；
                *    否则跳转到 {scrAddress}
                * 
                * @param sceneId 场景编号
                * @param eventId 事件编号
                * @param isAutoTrigger 是否走到触发范围内自动触发
                * @param triggerDistance 触发范围
                * @param scrAddress 队伍领队没有面向事件则跳转到的地址
                */
                {
                    if (args[0].Int != save.SceneId)
                    {
                        //
                        // 该事件对象不在当前场景中，直接标注脚本失败
                        //
                        address = args[4].Address - 1;
                        ScriptSuccess = false;
                        break;
                    }

                    pos = S.GetHeroTeamPos();
                    var x = eventTrail!.Pos.X - pos.X;
                    var y = eventTrail.Pos.Y - pos.Y;

                    var direction = S.GetHeroTeamDirection();
                    x += (direction == PalDirection.West || direction == PalDirection.South) ? 16 : -16;
                    y += (direction == PalDirection.West || direction == PalDirection.North) ? 8 : -8;

                    if ((args[3].Int > 0) && (Math.Abs(x) + Math.Abs(y * 2) < args[3].Int * 32 + 16) && S.GetEvent(args[0].Int, args[1].Int).IsDisplay)
                    {
                        //
                        // 更改触发模式，以便该对象能在下一帧被触发
                        //
                        currentEvent!.Trigger.IsAutoTrigger = true;
                        currentEvent.Trigger.Range = (Core.EventTriggerMode.TouchNormal - Core.EventTriggerMode.TouchNear + 1) + args[3].Int;
                    }
                    else
                    {
                        address = args[4].Address - 1;
                        ScriptSuccess = false;
                    }
                }
                break;

            case 0x0085:
                /**
                * @note
                * -  延迟 {delay * 80} ms
                * 
                * @param delay 延迟时间；实际延迟时间 {delay * 80} ms
                */
                PalTimer.Delay(args[0].Int * 80);
                break;

            case 0x0093:
                /**
                * @note
                * -  屏幕淡出，期间更新场景
                * 
                * @param step 淡入步长
                */
                PalScreen.FadeAndUpdate(args[0].Int, isFadeOut: (args[0].Int >= 0));
                PalGlobal.NeedToFadeIn = (args[0].Int < 0);
                break;

            case 0x0094:
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
                if (currentEvent!.Trigger.StateCode == args[2].StateCode) address = args[3].Address - 1;
                break;

            case 0x009A:
                /**
                * @note
                * -  将事件范围 {sceneId.eventBeginId} ～ {sceneId.eventEndId} 的状态码设置为 {stateCode}
                * 
                * @param sceneId 场景编号
                * @param eventBeginId 事件起始编号
                * @param eventEndId 事件末尾编号
                * @param stateCode 显示状态
                */
                scene = S.GetScene(args[0].Scene);
                var eventBeginId = args[1].Event;
                var eventEndId = int.Min(args[2].Event, scene.Events.Count - 1);
                for (var i = eventBeginId; i <= eventEndId; i++)
                    scene.Events[i].Trigger.StateCode = args[3].StateCode;
                break;

            /*
        case 0x0075:
            break;

        case 0x0075:
            break;

        case 0x0075:
            break;

        case 0x0075:
            break;

        case 0x0075:
            break;

        case 0x0075:
            break;

        case 0x0075:
            break;

        case 0x0075:
            break;

        case 0x0075:
            break;
            */

            default:
                var message = $@"Unknown script: {scr}";
                S.Log(message);
                S.Warning("PalScript.RunPublic", message);
                break;
        }

        return address + 1;
    }
}
