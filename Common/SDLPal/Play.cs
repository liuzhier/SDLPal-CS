using Records.Mod.RGame;
using Records.Pal;
using System;
using System.Diagnostics;

namespace SDLPal;

public static class PalPlay
{
    /// <summary>
    /// 主要的游戏逻辑程序，更新所有元素的状态。
    /// </summary>
    /// <param name="allowTrigger">是否处理触发事件</param>
    public static void GameUpdate(bool allowTrigger)
    {
        //
        // 获取当前场景
        //
        var scene = S.CurrScene;

        if (allowTrigger)
        {
            //
            // 检查是否触发了事件
            //
            // 检查我们是否正在进入一个新的场景
            //
            if (PalGlobal.EnterScene)
            {
                PalGlobal.EnterScene = false;

                //
                // 执行进场脚本
                //
                var address = scene.Script.Enter;
                if (address != 0)
                {
                    S.Log($"\nEnterScene[Begin]: {scene.Name}");
                    scene.Script.Enter = PalScript.RunTrigger(address, -1, -1, $"SceneEnter<{PalGlobal.Save.SceneId}>");
                    S.Log($"EnterScene[End]: {scene.Name}\n");
                }

                if (PalGlobal.EnterScene)
                    //
                    //  别再往前走了，因为我们要切换到另一个场景了
                    //
                    return;

                PalInput.ClearKeyState();
                PalScene.Draw();
            }

            //
            // 对当前场景中的所有事件对象进行遍历
            //
            for (var i = 0; i < scene.Events.Count; i++)
            {
                var @event = scene.Events[i];

                if (@event == null)
                    //
                    // 索引为 0 的场景是空的，直接跳过
                    //
                    continue;

                if (@event.Sprite.VanishTime != 0)
                {
                    //
                    // 更新所有事件对象的消失时间/僵直（逃跑后罚站）时间
                    //
                    @event.Sprite.VanishTime += (@event.Sprite.VanishTime < 0 ) ? 1 : -1;
                    continue;
                }

                var trail = @event.Sprite.Trail;

                if (@event.IsVanish)
                {
                    //
                    // 将所有隐藏事件将恢复显示。
                    //
                    if (trail.Pos.X < PalViewport.Pos.X ||
                       trail.Pos.X > PalViewport.Pos.X + PalVideo.Width ||
                       trail.Pos.Y < PalViewport.Pos.Y ||
                       trail.Pos.Y > PalViewport.Pos.Y + PalVideo.Height)
                    {
                        @event.Trigger.StateCode = (Core.EventState)short.Abs((short)@event.Trigger.StateCode);
                        trail.FrameId = 0;
                    }
                }
                else if (@event.IsDisplay && @event.Trigger.IsAutoTrigger)
                {
                    //
                    // 此事件对象为靠近自动触发
                    //
                    var pos = S.GetHeroTeamPos();

                    var xOffset = pos.X - trail.Pos.X;
                    var yOffset = pos.Y - trail.Pos.Y;

                    if (Math.Abs(xOffset) + Math.Abs(yOffset) * 2 < (@event.Trigger.Range - 1) * 32 + 16)
                    {
                        //
                        // 领队处于可触发区域内
                        //
                        if (@event.Sprite.FramesPerDirection != 0)
                        {
                            //
                            // 这个形象每个方向有多个帧，尝试调整方向。
                            //
                            trail.FrameId = 0;

                            if (xOffset > 0)
                                trail.Direction = (yOffset > 0) ? PalDirection.East : PalDirection.North;
                            else
                                trail.Direction = (yOffset > 0) ? PalDirection.South : PalDirection.West;

                            //
                            // 更新队伍步伐并重绘场景
                            //
                            PalScene.UpdateHeroTeamGestures(false);

                            PalScene.Draw();
                            PalScreen.Update();
                        }

                        //
                        // 关闭额外场景信息显示
                        //
                        S.SetDrawSceneDebugData(false);
                        PalScene.Draw(onlyDraw: true);

                        //
                        // 执行触发脚本
                        //
                        var address = @event.Script.Trigger;
                        @event.Script.Trigger = PalScript.RunTrigger(address, -1, i);

                        PalInput.ClearKeyState();

                        if (PalGlobal.EnterScene)
                            //
                            // 在场景切换时不要接着执行
                            //
                            return;
                    }
                }
            }
        }

        //
        // 执行每个事件的自动脚本
        //
        for (var i = 1; i < scene.Events.Count; i++)
        {
            var @event = scene.Events[i];

            if (@event.IsDisplay && (@event.Sprite.VanishTime == 0))
            {
                var address = @event.Script.Auto;

                if (address != 0)
                {
                    @event.Script.Auto = PalScript.RunAuto(address, i);

                    if (PalGlobal.EnterScene)
                        //
                        // 在场景切换时不要接着执行
                        //
                        return;
                }
            }

            var trail = @event.Sprite.Trail;
            var pos = S.GetHeroTeamPos();

            //
            // 队员挡住了该 NPC 的去路
            //
            if (allowTrigger && @event.IsObstacle && @event.Sprite.FramesPerDirection != 0 &&
               Math.Abs(trail.Pos.X - pos.X) + Math.Abs(trail.Pos.Y - pos.Y) * 2 <= 12)
            {
                //
                // 队员挡住了该 NPC 的去路，试着向前走一步
                //
                var dir = (PalDirection)((int)(trail.Direction + 1) % 4);

                for (i = 0; i < 4; i++)
                {
                    pos = S.GetHeroTeamPos().Clone();

                    pos.X += (dir == PalDirection.West || dir == PalDirection.South) ? -16 : 16;
                    pos.Y += (dir == PalDirection.West || dir == PalDirection.North) ? -8 : 8;

                    if (!PalScene.CheckObstacle(pos, true, 0, true))
                    {
                        //
                        // 让队员移动到该位置，为该 NPC 让路
                        //
                        S.SetHeroTeamPos(pos);
                        break;
                    }
                }
            }
        }

        if (--S.Save.ChaseCycles == 0)
            //
            // 驱魔香、十里香时间结束
            // 敌人追击范围恢复默认状态
            //
            S.Save.ChaseRange = 1;

        PalGlobal.FrameNum++;
    }

    /// <summary>
    /// 处理手动调查触发事件
    /// </summary>
    public static void Search()
    {
        var poses = new Pos[13];

        //
        // 获取领队坐标
        //
        var pos = S.GetHeroTeamPos();
        var dir = S.GetHeroTeamDirection();
        var x = pos.X;
        var y = pos.Y;

        var xOffset = -16;
        var yOffset = -8;
        if (dir == PalDirection.North || dir == PalDirection.East) xOffset = -xOffset;
        if (dir == PalDirection.East || dir == PalDirection.South) yOffset = -yOffset;

        poses[0] = pos.Clone();

        for (var i = 0; i < 4; i++)
        {
            poses[i * 3 + 1] = new Pos(x + xOffset, y + yOffset);
            poses[i * 3 + 2] = new Pos(x, y + yOffset * 2);
            poses[i * 3 + 3] = new Pos(x + 2 * xOffset, y);
            x += xOffset;
            y += yOffset;
        }

        for (var i = 0; i < 13; i++)
        {
            //
            // 转换为地图位置
            //
            var dh = (poses[i].X % 32 != 0) ? 1 : 0;
            var dx = poses[i].X / 32;
            var dy = poses[i].Y / 16;

            var events = S.CurrScene.Events;

            //
            // 对所有事件对象进行循环遍历
            //
            for (var k = 1; k < events.Count; k++)
            {
                var @event = events[k];
                var trail = @event.Sprite.Trail;
                pos = trail.Pos;
                var ex = pos.X / 32;
                var ey = pos.Y / 16;
                var eh = (pos.X % 32 != 0) ? 1 : 0;

                if (!@event.IsDisplay
                   || @event.Trigger.IsAutoTrigger
                   || @event.Trigger.Range * 6 - 4 < i
                   || dx != ex
                   || dy != ey
                   || dh != eh)
                {
                    continue;
                }

                //
                // 调整队员和事件的方向/动作的相应参数
                //
                var sprite = @event.Sprite;
                if (sprite.FramesPerDirection * 4 > trail.FrameId)
                {
                    sprite.Trail.FrameId = 0;                                           // 当前站立姿势
                    sprite.Trail.Direction = (PalDirection)((int)(dir + 2) % 4);        // 面朝领队

                    for (var l = 0; l < S.Save.HeroTeamLength; l++)
                    {
                        //
                        // 所有队员都应面向事件
                        //
                        S.SetMemberDirection(l, dir);
                        S.GetMemberTrail(l).FrameId = 0;
                    }

                    //
                    // 重新绘制所有内容
                    //
                    PalScene.Draw();
                    PalScreen.Update();
                }

                //
                // 关闭额外场景信息显示
                //
                S.SetDrawSceneDebugData(false);
                PalScene.Draw(onlyDraw: true);

                //
                // 执行事件触发脚本
                //
                @event.Script.Trigger = PalScript.RunTrigger(@event.Script.Trigger, -1, k);

                //
                // 清除输入并短暂延迟
                //
                PalTimer.Delay(50);
                PalInput.ClearKeyState();

                return;     // 不要走过头了
            }
        }
    }

    /// <summary>
    /// 开始显示画面帧，每次播放一个画面帧时都会调用此函数
    /// </summary>
    public static void StartFrame()
    {
#if DEBUG
        //
        // 运行游戏一帧的逻辑
        //
        GameUpdate(true);
        if (PalGlobal.EnterScene) return;
#endif // !DEBUG

        //
        // 更新角色队伍的位置和动作状态
        //
        PalScene.UpdateHeroTeam();

        //
        // 更新屏幕
        //
        S.SetDrawSceneDebugData(true);
        PalScene.Draw();
        PalScreen.Update();

        if (PalInput.Pressed(PalKey.Menu))
        {
            //
            // 打开场景主菜单
            //
            PalUiGame.SceneMainMenu();
        }
        else if (PalInput.Pressed(PalKey.UseItem))
        {
            //
            // 打开背包
            //
            PalAtlas.Scene = new(PalAtlas.Scene);
            PalUiGame.CommandItemMenu(ref PalAtlas.Scene);
        }
        else if (PalInput.Pressed(PalKey.Search))
        {
            //
            // Process search events
            //
            Search();
        }

        var item = PalGlobal.SceneUseItem;
        if (item != null!)
        {
            //
            // 关闭额外场景信息显示
            //
            S.SetDrawSceneDebugData(false);
            PalScene.Draw(onlyDraw: true);

            //
            // 用户可能使用了剧情道具，执行它的脚本
            //
            item.Script.Use = PalScript.RunTrigger(item.Script.Use, -1, -1, $"UseItem<{item.Name}>");

            //
            // 道具自动消耗
            //
            S.AutoUseItem();

            //
            // 脚本执行完毕，置空
            //
            PalGlobal.SceneUseItem = null!;
        }
    }
}
