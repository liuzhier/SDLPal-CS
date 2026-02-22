using Records.Mod;
using Records.Mod.RGame;
using Records.Pal;
using SDL3;
using System;
using System.Collections.Generic;
using static SDL3.TTF;

namespace SDLPal;

public static class PalScene
{
    public const int
        Fps            = 15,
        FrameTime      = (1000 / Fps);
    public static List<MapSprite> Sprites { get; set; } = null!;

    public static void Clear()
    {
        Sprites = [];
        PalAtlas.Scene = new(PalAtlas.Scene);
    }

    /// <summary>
    /// 更新所有队员的位置和行走姿态。
    /// </summary>
    public static void UpdateHeroTeam()
    {
        var currDir = PalInput.State.Direction;

        //
        // 检查用户是否按下按键
        //
        if (currDir != PalDirection.Current)
        {
            var posTarget = S.HeroTeamLeaderTrail.Pos.Clone();

            var posOffset = new Pos(
                X: (currDir == PalDirection.West || currDir == PalDirection.South) ? -16 : 16,
                Y: (currDir == PalDirection.West || currDir == PalDirection.North) ? -8 : 8
            );

            posTarget.X += posOffset.X;
            posTarget.Y += posOffset.Y;

            //var heroTeams = S.Save.HeroTeams;
            //var followers = S.Save.Followers;

            //
            // 检查目标位置是否存在障碍物
            //
            if (!CheckObstacle(posTarget, true, 0, true))
            {
                //
                // 移动队伍并记录轨迹
                //
                //S.SetHeroTeamPos(posTarget);

                //
                // 更新所有队员的坐标
                //
                UpdateHeroTeamPos(posTarget, currDir);

                //
                // 更新姿势
                //
                UpdateHeroTeamGestures(true);

                return;     // 不要执行过头
            }

            S.SetHeroTeamDirection(currDir);
        }

        UpdateHeroTeamGestures(false);
    }

    /// <summary>
    /// 更新所有队员的行走姿态。
    /// </summary>
    /// <param name="isWalking">是否正在行走</param>
    public static void UpdateHeroTeamGestures(bool isWalking)
    {
        foreach (var member in S.Save.Members)
            UpdateCharacterGestures(isWalking, member.Trail, S.Entity.Heroes[member.HeroId].Sprite.FramesPerDirection);

        foreach (var follower in S.Save.Followers)
            UpdateCharacterGestures(isWalking, follower.Trail, follower.Trail.SpriteFramesAuto / 4);
    }

    /// <summary>
    /// 更新角色的行走姿态
    /// </summary>
    /// <param name="isWalking">是否正在行走</param>
    /// <param name="trail">步伐数据</param>
    /// <param name="framesPerDirection">每方向帧数</param>
    public static void UpdateCharacterGestures(bool isWalking, Trail trail, int framesPerDirection = 3)
    {
        if (isWalking)
        {
            //
            // 角色没有移动，切换为“站立”手势而非“行走”姿势。
            //
            if (framesPerDirection == 9)
            {
                //
                // 新版形象行走（索引为 0 的帧为原地站立帧）
                //
                if (trail.WalkCount == 0)
                    //
                    // 刚起步时随机先迈出左右脚
                    //
                    trail.WalkCount = (S.RandomLong(0, 1) != 0) ? 1 : 5;
                else
                    trail.WalkCount++;

                //
                // 行走动画如果播放到最后一帧，自动切回第一帧
                //
                trail.FrameId = trail.WalkCount % 10;

                //
                // 调整步伐，使其更自然
                //
                if (trail.FrameId == 5 && trail.WalkCount > 5) trail.FrameId = 0;
                else if (trail.FrameId > 5) trail.FrameId--;
            }
            else if (framesPerDirection == 3)
            {
                //
                // 旧版形象行走
                //
                if (trail.WalkCount == 0)
                    //
                    // 锁妖塔沉思鬼问题
                    //
                    // 沉思鬼：
                    //     我应该先踏出左脚．．
                    //     还是先踏出右脚呢？
                    //     你知道这问题的答案吗？
                    //
                    trail.WalkCount = (S.RandomLong(0, 1) != 0) ? 1 : 3;
                else
                    trail.WalkCount++;

                //
                // 行走动画如果播放到最后一帧，自动切回第一帧
                //
                trail.FrameId = trail.WalkCount % 4;

                //
                // 调整步伐，使其更自然
                //
                if (trail.FrameId == 2) trail.FrameId = 0;
                if (trail.FrameId == 3) trail.FrameId = 2;
            }
            else if (framesPerDirection > 0)
            {
                //
                // 其他每方向上帧数不标准的 NPC（十里坡蜜蜂、隐龙窟蜥蜴、黑水镇僵尸等）
                //
                trail.FrameId++;
                trail.FrameId %= framesPerDirection;
            }
            else
            {
                //
                // 非行走类动画播放（雪花、蝴蝶、水滴、卖艺者等）
                //
                trail.FrameId++;
                trail.FrameId %= trail.SpriteFramesAuto;
            }
        }
        else
        {
            //
            // 角色没有移动，切换为“站立”姿势。
            //
            trail.WalkCount = 0;
            trail.FrameId = 0;
        }
    }

    /// <summary>
    /// 更新队伍坐标
    /// </summary>
    /// <param name="posTarget"></param>
    public static void UpdateHeroTeamPos(Pos posTarget, PalDirection direction = PalDirection.Current)
    {
        var members = S.Save.Members;
        var followers = S.Save.Followers;

        //
        // 备份最后一名队员的坐标和方向信息，
        // 因为第一个跟随者需要进行同步操作
        //
        var posLast = S.GetMemberPos(^1);
        var dirLast = S.GetMemberDirection(^1);

        //
        // 队员：从尾到头搬引用（^1 <- ^2 <- ... <- ^Length）
        //
        for (var i = 1; i < members.Length; i++)
        {
            S.SetMemberPos(^i, S.GetMemberPos(^(i + 1)));
            S.SetMemberDirection(^i, S.GetMemberDirection(^(i + 1)));
        }

        //
        // 随从：同样从尾到头搬引用（^1 <- ^2 <- ... <- ^Count）
        // 最后 follower[0] 接队尾旧引用
        //
        if (followers.Count > 0)
        {
            for (var i = 1; i < followers.Count; i++)
            {
                S.SetFollowerPos(^i, S.GetFollowerPos(^(i + 1)));
                S.SetFollowerDirection(^i, S.GetFollowerDirection(^(i + 1)));
            }

            //
            // 最后 follower[0] 接队尾旧引用
            //
            S.SetFollowerPos(0, posLast);
            S.SetFollowerDirection(0, dirLast);
        }

        //
        // 最后更新领队的位置
        //
        S.SetMemberPos(0, posTarget.Clone());
        if (direction != PalDirection.Current) S.SetMemberDirection(0, direction);
    }

    /// <summary>
    /// 将队伍整齐排为一列
    /// </summary>
    public static void UpdateHeroTeamLinearPos()
    {
        var save = S.Save;
        var pos = S.GetHeroTeamPos();
        var direction = S.GetHeroTeamDirection();

        var xOffset = (direction == PalDirection.West || direction == PalDirection.South) ? 16 : -16;
        var yOffset = (direction == PalDirection.West || direction == PalDirection.North) ? 8 : -8;

        var i = 0;
        for (; i < save.Members.Length; i++)
        {
            S.SetMemberPos(i, new(pos.X + xOffset * i, pos.Y + yOffset * i));
            S.SetMemberDirection(i, direction);
        }

        for (var j = 0; j < save.Followers.Count; j++, i++)
        {
            S.SetFollowerPos(j, new(pos.X + xOffset * i, pos.Y + yOffset * i));
            S.SetFollowerDirection(j, direction);
        }
    }

    /// <summary>
    /// 让队伍向指定块坐标位置移动
    /// </summary>
    /// <param name="bpos">块坐标</param>
    /// <param name="speed">移动速度</param>
    public static void HeroTeamWalkTo(BlockPos bpos, int speed)
    {
        var time = 0ul;
        var trail = S.HeroTeamLeaderTrail;
        var pos = trail.Pos.Clone();

        while (true)
        {
            var offsetX = bpos.X * 32 + bpos.H * 16 - pos.X;
            var offsetY = bpos.Y * 16 + bpos.H * 8 - pos.Y;

            if (offsetX == 0 && offsetY == 0) break;

            PalTimer.DelayUntil(time);

            time = SDL.GetTicks() + FrameTime;

            var direction = PalDirection.Current;
            if (offsetY < 0) direction = ((offsetX < 0) ? PalDirection.West : PalDirection.North);
            else direction = ((offsetX < 0) ? PalDirection.South : PalDirection.East);

            pos.X += (Math.Abs(offsetX) <= speed * 2) ? offsetX : (speed * (offsetX < 0 ? -2 : 2));
            pos.Y += (Math.Abs(offsetY) <= speed) ? offsetY : (speed * (offsetY < 0 ? -1 : 1));

            //
            // 更新队伍坐标
            //
            UpdateHeroTeamPos(pos, direction);

            //
            // 更新队伍步伐姿势
            //
            UpdateHeroTeamGestures(true);

            //
            // 执行所有事件的自动脚本
            //
            PalPlay.GameUpdate(false);
            Draw();
            PalScreen.Update();
        }

        //
        // 将队伍步伐姿势设置为原地站立
        //
        UpdateHeroTeamGestures(false);
    }

    /// <summary>
    /// 事件向当前方向走一步
    /// </summary>
    /// <param name="sceneId">该事件所在的场景编号</param>
    /// <param name="eventId">要移动的事件对象</param>
    /// <param name="speed">移动速度</param>
    public static void NpcWalkOneStep(int sceneId, int eventId, int speed)
    {
        //
        // 检查无效参数
        //
        if (sceneId == 0 || sceneId > S.Save.Scenes.Count - 1) return;

        var @event = S.GetEvent(sceneId, eventId);
        var trail = @event.Trail;

        var posEvent = trail.Pos;
        var pos = posEvent.Clone();

        //
        // 根据指定的方向移动 Npc
        //
        pos.X += ((trail.Direction == PalDirection.West || trail.Direction == PalDirection.South) ? -2 : 2) * speed;
        pos.Y += ((trail.Direction == PalDirection.West || trail.Direction == PalDirection.North) ? -1 : 1) * speed;

        posEvent.X = pos.X;
        posEvent.Y = pos.Y;

        //
        // 更新步伐
        //
        UpdateCharacterGestures(true, trail, @event.Sprite.FramesPerDirection);

        /*
        trail.FrameId++;
        var count = @event.Sprite.FramesPerDirection;
        if (count == 9)
            trail.FrameId %= count;
        else if (count == 9)
        {
            trail.FrameId %= (count == 3) ? 4 : count;

            //if (count == 3 || count == 9)
            //{
            //   count++;
            //}

            //trail.FrameID %= count;

            //if (count == 3)
            //{
            //   if (trail.FrameID == 0)
            //   {
            //      trail.FrameID = S_RandomLong(1, 2);
            //   }
            //   else
            //   {
            //      trail.FrameID = 0;
            //   }
            //}
            //else
            //{
            //   trail.FrameID++;
            //   trail.FrameID %= count;
            //}
        }
        else if (trail.SpriteFramesAuto > 0)
        {
            trail.FrameId++;
            trail.FrameId %= trail.SpriteFramesAuto;
        }
        */
    }

    /// <summary>
    /// 让事件向指定块坐标位置移动
    /// </summary>
    /// <param name="sceneId">该事件所在的场景编号</param>
    /// <param name="eventId">要移动的事件对象</param>
    /// <param name="bpos">块坐标</param>
    /// <param name="speed">移动速度</param>
    /// <returns>是否已经到达终点</returns>
    public static bool NpcWalkTo(int sceneId, int eventId, BlockPos bpos, int speed)
    {
        var @event = S.GetEvent(sceneId, eventId);
        var trail = @event.Trail;

        var offsetX = (bpos.X * 32 + bpos.H * 16) - trail.Pos.X;
        var offsetY = (bpos.Y * 16 + bpos.H * 8) - trail.Pos.Y;

        if (offsetY < 0)
            trail.Direction = ((offsetX < 0) ? PalDirection.West : PalDirection.North);
        else
            trail.Direction = ((offsetX < 0) ? PalDirection.South : PalDirection.East);

        if (Math.Abs(offsetX) < speed * 2 || Math.Abs(offsetY) < speed * 2)
        {
            trail.Pos.X = bpos.X * 32 + bpos.H * 16;
            trail.Pos.Y = bpos.Y * 16 + bpos.H * 8;
        }
        else NpcWalkOneStep(sceneId, eventId, speed);

        if (trail.Pos.X == bpos.X * 32 + bpos.H * 16 && trail.Pos.Y == bpos.Y * 16 + bpos.H * 8)
        {
            trail.FrameId = 0;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 队伍驾驶指定事件行驶至指定位置
    /// </summary>
    /// <param name="sceneId">所驾驶的事件在哪个场景</param>
    /// <param name="eventId">所驾驶的事件在场景的编号</param>
    /// <param name="bpos">欲行驶到的位置</param>
    /// <param name="speed">行驶速度</param>
    public static void HeroTeamRideEventObject(int sceneId, int eventId, BlockPos bpos, int speed)
    {
        var pos = S.GetHeroTeamPos().Clone();
        var posEvt = S.GetEventTrail(sceneId, eventId).Pos;

        var t = 0u;

        while (true)
        {
            var xOffset = bpos.X * 32 + bpos.H * 16 - pos.X;
            var yOffset = bpos.Y * 16 + bpos.H * 8 - pos.Y;

            if (xOffset == 0 && yOffset == 0) return;

            PalTimer.DelayUntil(t);

            t = (uint)(SDL.GetTicks() + FrameTime);

            var direction = PalDirection.Current;
            if (yOffset < 0) direction = (xOffset < 0) ? PalDirection.West : PalDirection.North;
            else direction = (xOffset < 0) ? PalDirection.South : PalDirection.East;

            var dx = (int.Abs(xOffset) > speed * 2) ? (speed * (xOffset < 0 ? -2 : 2)) : xOffset;
            var dy = (int.Abs(yOffset) > speed) ? speed * (yOffset < 0 ? -1 : 1) : yOffset;

            //
            // 计算队伍下一步行走的坐标位置
            //
            pos.X += dx;
            pos.Y += dy;

            //
            // 更新队伍成员坐标位置
            //
            UpdateHeroTeamPos(pos, direction);

            //
            // 设置事件的坐标位置
            //
            posEvt.X += dx;
            posEvt.Y += dy;

            //
            // 绘制一帧游戏场景
            //
            PalPlay.GameUpdate(false);
            Draw();
            PalScreen.Update();
        }
    }

    /// <summary>
    /// 让指定的事件对象去追逐玩家
    /// </summary>
    /// <param name="sceneId">该事件所在的场景编号</param>
    /// <param name="eventId">该事件的编号</param>
    /// <param name="chaseRange">事件的攻击范围</param>
    /// <param name="speed">追逐速度</param>
    /// <param name="canFly"></param>
    public static void MonsterChasePlayer(int sceneId, int eventId, int chaseRange, int speed, bool canFly)
    {
        var monsterSpeed = 0;
        var trail = S.GetEventTrail(sceneId, eventId);
        var pos = S.GetHeroTeamPos();
        var posTarget = new Pos();
        var save = S.Save;

        if (save.ChaseRange != 0)
        {
            var x = pos.X - trail.Pos.X;
            var y = pos.Y - trail.Pos.Y;

            if (x == 0) x = (S.RandomLong(0, 1) != 0) ? -1 : 1;
            if (y == 0) y = (S.RandomLong(0, 1) != 0) ? -1 : 1;

            var xPrev = trail.Pos.X;
            var yPrev = trail.Pos.Y;

            var i = xPrev % 32;
            var j = yPrev % 16;

            xPrev /= 32;
            yPrev /= 16;
            var l = 0;

            if (i + j * 2 >= 16)
            {
                if (i + j * 2 >= 48)
                {
                    xPrev++;
                    yPrev++;
                }
                else if (32 - i + j * 2 < 16)
                {
                    xPrev++;
                }
                else if (32 - i + j * 2 < 48)
                {
                    l = 1;
                }
                else
                {
                    yPrev++;
                }
            }

            xPrev = xPrev * 32 + l * 16;
            yPrev = yPrev * 16 + l * 8;

            //
            // 领队是否靠近了该事件对象
            //
            if ((int.Abs(x) + int.Abs(y) * 2) < (chaseRange * 32 * save.ChaseRange))
            {
                if (x < 0) trail.Direction = (y < 0) ? PalDirection.West : PalDirection.South;
                else trail.Direction = (y < 0) ? PalDirection.North : PalDirection.East;

                x = (x != 0) ? (trail.Pos.X + x / int.Abs(x) * 16) : trail.Pos.X;
                y = (y != 0) ? (trail.Pos.Y + y / int.Abs(y) * 8) : trail.Pos.Y;

                if (canFly) monsterSpeed = speed;
                else
                {
                    if (!CheckObstacle(new Pos(x, y), true, eventId)) monsterSpeed = speed;
                    else
                    {
                        trail.Pos.X = xPrev;
                        trail.Pos.Y = yPrev;
                    }

                    for (l = 0; l < 4; l++)
                    {
                        switch (l)
                        {
                            case 0:
                                trail.Pos.X -= 4;
                                trail.Pos.Y += 2;
                                break;

                            case 1:
                                trail.Pos.X -= 4;
                                trail.Pos.Y -= 2;
                                break;

                            case 2:
                                trail.Pos.X += 4;
                                trail.Pos.Y -= 2;
                                break;

                            case 3:
                                trail.Pos.X += 4;
                                trail.Pos.Y += 2;
                                break;
                        }

                        if (CheckObstacle(trail.Pos, false, 0))
                        {
                            trail.Pos.X = xPrev;
                            trail.Pos.Y = yPrev;
                        }
                    }
                }
            }
        }
        else
        {
            //
            // 驱魔香效果：使这种事件对象暂时不追逐领队，只是原地打转
            // 该事件对象每隔两帧改变一次方向
            //
            if ((PalGlobal.FrameNum & 1) != 0)
            {
                trail.Direction++;

                if (trail.Direction != PalDirection.Current) trail.Direction = PalDirection.South;
            }
        }

        NpcWalkOneStep(sceneId, eventId, monsterSpeed);
    }

    public static bool CanFly = false;

    /// <summary>
    /// 检查指定位置是否有障碍物
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="checkEvent">True 则检查事件对象，否则仅检查地图</param>
    /// <param name="eventSelfId">将要跳过的事件对象</param>
    /// <param name="checkRange">是否需要检查范围</param>
    /// <returns>是否有障碍物</returns>
    public static bool CheckObstacle(Pos pos, bool checkEvent, int eventSelfId, bool checkRange = false)
    {
        //
        // 避免超出范围，远离地图边缘
        //
        var x = pos.X;
        var y = pos.Y;
        if (checkRange)
            if (x <= -16 || x > PalMap.Width - 32 || y <= -8 || y > PalMap.Height - 8)
                return true;

        if (CanFly && checkRange)
            //
            // Debugging: 允许玩家穿过墙壁
            //
            return false;

        //
        // 检查指定位置的地图图块是否为障碍物
        //
        x /= 32;
        y /= 16;
        var h = 0;

        var xr = pos.X % 32;
        var yr = pos.Y % 16;

        if (xr + yr * 2 >= 16)
        {
            if (xr + yr * 2 >= 48)
            {
                x++;
                y++;
            }
            else if (32 - xr + yr * 2 < 16)
            {
                x++;
            }
            else if (32 - xr + yr * 2 < 48)
            {
                h = 1;
            }
            else
            {
                y++;
            }
        }

        if (PalMap.TileIsObstacle(new BlockPos((byte)x, (byte)y, (byte)h))) return true;

        if (checkEvent)
        {
            var events = S.CurrScene.Events;

            //
            // 对当前场景中的所有事件对象进行遍历
            //
            for (var i = 1; i < events.Count; i++)
            {
                var @event = events[i];

                if (i == eventSelfId)
                    //
                    // 跳过检查自己
                    //
                    continue;

                //
                // 检查事件是否为障碍物
                //
                if (@event.Trigger.StateCode == Core.EventState.Obstacle)
                {
                    var evtX = S.GetEventPos(-1, i).X;
                    var evtY = S.GetEventPos(-1, i).Y;

                    //
                    // 检查是否发生碰撞（是否会被空气墙挡住去路）
                    //
                    if (Math.Abs(evtX - pos.X) + Math.Abs(evtY - pos.Y) * 2 < 16) return true;
                }
            }
        }

        //
        // 如果执行到这里，说明没有撞到空气墙
        //
        return false;
    }

    /// <summary>
    /// 计算出能够覆盖指定精灵的所有瓦片，并将这些瓦片也添加到我们的列表中
    /// </summary>
    /// <param name="mapSprite"></param>
    static void CalcCoverTiles(MapSprite mapSprite)
    {
        var sx = PalViewport.Pos.X + mapSprite.Pos.X - mapSprite.LayerHeight / 2;
        var sy = PalViewport.Pos.Y + mapSprite.Pos.Y - mapSprite.LayerHeight;
        var sh = (sx % 32 != 0) ? 1 : 0;

        var dx = 0;
        var dy = 0;
        var dh = 0;

        //
        // 获取当前瓦片尺寸
        //
        S.GetSurfaceSize(mapSprite.Sprite, out var width, out var height);

        //
        // 遍历形象所在的区域内的所有图块
        //
        for (var y = (sy - height - 15) / 16; y <= sy / 16; y++)
        {
            for (var x = (sx - width / 2) / 32; x <= (sx + width / 2) / 32; x++)
            {
                for (var i = ((x == (sx - width / 2) / 32) ? 0 : 3); i < 5; i++)
                {
                    //
                    // 按以下形式扫描图块（* 表示需要扫描）：
                    //
                    // . . . * * * . . .
                    //  . . . * * . . . .
                    //
                    switch (i)
                    {
                        case 0:
                            dx = x;
                            dy = y;
                            dh = sh;
                            break;

                        case 1:
                            dx = x - 1;
                            break;

                        case 2:
                            dx = ((sh != 0) ? x : (x - 1));
                            dy = ((sh != 0) ? (y + 1) : y);
                            dh = 1 - sh;
                            break;

                        case 3:
                            dx = x + 1;
                            dy = y;
                            dh = sh;
                            break;

                        case 4:
                            dx = ((sh != 0) ? (x + 1) : x);
                            dy = ((sh != 0) ? (y + 1) : y);
                            dh = 1 - sh;
                            break;
                    }

                    for (var l = 0; l < 2; l++)
                    {
                        var pTile = PalMap.GetTileSurface(new BlockPos((byte)dx, (byte)dy, (byte)dh), l);
                        var iTileHeight = (sbyte)PalMap.GetTileLayer(new BlockPos((byte)dx, (byte)dy, (byte)dh), l);

                        //
                        // 检查此图块是否能够覆盖该形象
                        //
                        if (pTile != 0 && iTileHeight > 0 && (dy + iTileHeight) * 16 + dh * 8 >= sy)
                        {
                            var zx = dx * 32 + dh * 16 - 16 - PalViewport.Pos.X;
                            var layer = iTileHeight * 8 + l;
                            var zy = dy * 16 + dh * 8 + 7 + layer - PalViewport.Pos.Y;

                            //
                            // 该瓷砖可能会覆盖到那个角色模型
                            //
                            Sprites.Add(new()
                            {
                                Sprite = pTile,
                                Pos = new Pos(zx, zy),
                                LayerHeight = layer,
                                ColorMask = new()
                                {
                                    R = 1,
                                    G = 1,
                                    B = 1,
                                    A = 0.8333f,
                                },
                            });
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 将所有的形象绘制到场景
    /// </summary>
    static void DrawSprite(Atlas atlas)
    {
        var save = S.Save;
        var members = save.Members;

        //
        // 将所有要绘制的精灵放入列表
        //

        //
        // Hero 队伍
        //
        var len = save.HeroTeamLength;
        //len = arrParty.Length + save.listFollower.Count;
        for (var i = 0; i < len; i++)
        {
            var trail = S.GetMemberTrail(i);
            var direction = (int)trail.Direction;
            var hero = S.Entity.Heroes[members[i].HeroId];
            var walkFrames = hero.Sprite.FramesPerDirection;
            var frameId = direction * walkFrames + trail.FrameId;
            var bitmap = PalResource.GetSpriteFrame(PalResource.SpriteType.Hero, i, frameId);

            if (bitmap == 0)
                //
                // 跳过空形象
                //
                continue;

            //
            // 计算坐标
            //
            S.GetSurfaceSize(bitmap, out var w, out var h);
            var needStretch = PalResource.CheckStretch(hero.Sprite.SpriteId);
            if (!needStretch)
            {
                //
                // 不需要缩放的贴图，应使用其逻辑高度
                //
                w = S.UnRatio(w);
                h = S.UnRatio(h);
            }
            var x = trail.Pos.X - PalViewport.Pos.X - w / 2;
            var layer = save.HeroTeamLayerOffset + 6;
            var y = trail.Pos.Y - PalViewport.Pos.Y + layer + 4;

            //
            // 将其添加到形象列表
            //
            Sprites.Add(new()
            {
                Sprite = bitmap,
                Pos = new(x, y),
                LayerHeight = layer,
                NeedStretch = needStretch,
#if DEBUG
                TextDrawInfo = [
                    new()
                    {
                        Text = hero.Name,
                        PosOffset = new(w / 2, int.MinValue),
                        Foreground = ColorGold,
                    },
                ],
#endif // !DEBUG
            });

            //
            // 在地图上添加覆盖的瓷砖
            //
            CalcCoverTiles(Sprites[^1]);
        }

        //
        // 事件对象（怪物/非玩家角色/其他）
        //
        len = S.CurrScene.Events.Count;
        for (var i = 1; i < len; i++)
        {
            var @event = S.GetEvent(i);
            var sprite = @event.Sprite;

            if (!@event.IsDisplay || @event.InVanishTime || !S.NeedDrawSceneDebugData && sprite.SpriteId == 0)
            {
                continue;
            }

            var trail = @event.Trail;
            var layer = sprite.Layer * 8 + 2;
            var x = trail.Pos.X - PalViewport.Pos.X;
            var y = trail.Pos.Y - PalViewport.Pos.Y + layer + 7;
            var w = 0;
            var h = 0;
            var bitmap = nint.Zero;
            var needStretch = false;
            var textDrawInfo = (TextDrawInfo[])null!;
            if (@event.Sprite.SpriteId == 0)
            {
                //
                // 该事件没有形象
                //
                if (S.NeedDrawSceneDebugData)
                    //
                    // 直接处理场景调试信息
                    //
                    goto AddEventSprites;

                //
                // 跳过该事件
                //
                continue;
            }

            //
            // 获取精灵图
            //
            var direction = (int)trail.Direction;
            var frameId = trail.FrameId;

            //
            // 获取角色形象
            //
            frameId = direction * sprite.FramesPerDirection + frameId;
            bitmap = PalResource.GetSpriteFrame(PalResource.SpriteType.Event, i, frameId);
            if (bitmap == 0)
                //
                // 形象为空，直接跳过
                //
                continue;

            //
            // 计算坐标并检查是否超出屏幕范围
            //
            S.GetSurfaceSize(bitmap, out w, out h);
            needStretch = PalResource.CheckStretch(sprite.SpriteId);
            if (!needStretch)
            {
                //
                // 不需要缩放的贴图，应使用其逻辑高度
                //
                w = S.UnRatio(w);
                h = S.UnRatio(h);
            }
            x -= w / 2;
            if (x >= PalViewport.Rect.W || x < -w)
                //
                // 在屏幕之外，直接跳过
                //
                continue;
            var vy = y - h - layer;
            if (vy >= PalViewport.Rect.H || vy < -h)
                //
                // 在屏幕之外，直接跳过
                //
                continue;

        AddEventSprites:
            if (S.NeedDrawSceneDebugData)
            {
                //
                // 需要添加场景调试信息
                //
                textDrawInfo = [
                    new()
                    {
                        Text = @event.Name,
                        PosOffset = new(w / 2, int.MinValue),
                        Foreground = ColorGold,
                    },
                    new()
                    {
                        Text = $"{i}",
                        PosOffset = new Pos(w / 2, S.Ratio(h)),
                    },
                ];
            }

            //
            // 将其添加到列表
            //
            Sprites.Add(new()
            {
                Sprite = bitmap,
                Pos = new(x, y),
                LayerHeight = layer,
                NeedStretch = needStretch,
                TextDrawInfo = textDrawInfo,
            });

            if (bitmap != 0)
                //
                // 如果形象不为空，则计算并添加覆盖在其上的瓦片
                //
                CalcCoverTiles(Sprites[^1]);
        }

        //
        // 现在所有的形象都已存入我们的数组中
        // 按照它们的垂直位置对它们进行排序
        //
        Sprites.Sort((curr, next) => curr.Pos.Y.CompareTo(next.Pos.Y));

        //
        // 将所有形象绘制到屏幕上的动作录制到图集
        //
        foreach (var sprite in Sprites)
        {
            //
            // 计算最终绘制位置
            //
            var pos = sprite.Pos;
            var h = 0;
            if (sprite.Sprite != 0)
            {
                //
                // 获取贴图高度
                //
                S.GetSurfaceSize(sprite.Sprite, out _, out h);
                if (!sprite.NeedStretch)
                    //
                    // 不需要缩放的贴图，应使用其逻辑高度
                    //
                    h = S.UnRatio(h);
            }
            pos.X = S.Ratio(pos.X);
            pos.Y = S.Ratio(pos.Y - h - sprite.LayerHeight);
            atlas.Add(new(sprite.Sprite), new()
            {
                Rect = new()
                {
                    X = pos.X,
                    Y = pos.Y,
                },
                StretchFactor = sprite.NeedStretch ? 2 : 1,
                ColorMask = sprite.ColorMask,
            });
        }

        if (S.NeedDrawSceneDebugData)
            foreach (var sprite in Sprites)
            {
                var infos = sprite.TextDrawInfo;
                if (infos != null)
                    foreach (var info in infos)
                    {
                        //
                        // 文本居中显示，名称信息应显示在头顶
                        //
                        var (w, h) = S.GetTextActualSize(info.Text, info.FontSize);
                        var pos = sprite.Pos;
                        var textPos = info.PosOffset ??= Pos.Zero;
                        if (textPos.Y == int.MinValue)
                            textPos.Y = -(S.UnRatio(h) + PalText.ShadowOffset);

                        textPos.X += pos.X - w / 2;
                        textPos.Y += pos.Y;
                    }

                //
                // 将绘制场景调试数据（事件的名称和编号）的动作录制到图集
                //
                PalText.DrawTexts(atlas, sprite.TextDrawInfo);
            }
    }

    /// <summary>
    /// 显示场景调试信息
    /// </summary>
    public static void DrawDebugInfo(Atlas atlas)
    {
        var pos = S.GetHeroTeamPos();
        var x = -PalDialog.Padding;
        //var y = -PalDialog.Padding;
        var y = 0;
        //var y = 0;
        var infos = new TextDrawInfo[]
        {
            new()
            {
                Text = $"{S.Save.SceneId}{S.CurrScene}",
                PosOffset = new(x, y),
                HorizontalAlign = PalHorizontalAlign.Right,
                VerticalAlign = PalVerticalAlign.Middle,
            },
            new()
            {
                Text = $"{pos}",
                PosOffset = new(x, y),
                HorizontalAlign = PalHorizontalAlign.Right,
                VerticalAlign = PalVerticalAlign.Middle,
            },
            new()
            {
                Text = $"{BlockPos.FromPos(pos)}",
                PosOffset = new(x, y),
                HorizontalAlign = PalHorizontalAlign.Right,
                VerticalAlign = PalVerticalAlign.Middle,
            },
        };

        //
        // 计算信息框大小
        //
        var width = 0;
        var height = 0;
        var h = 0;
        var boxH = 0;
        foreach (var info in infos)
        {
            //
            // 计算文本总尺寸
            //
            (var w, h) = S.GetTextActualSize(info.Text, info.FontSize);
            width = int.Max(width, w);
            height += h;

            //
            // 每行文本坐标偏移
            //
            info.PosOffset.Y = boxH;
            boxH += h;
        }
        foreach (var info in infos)
            info.PosOffset.Y -= (height - h) / 2;
        width += PalDialog.Padding * 2;
        height += PalDialog.Padding * 2;

        //
        // 绘制信息框
        //
        PalShape.DrawBox(new()
        {
            Rect = new()
            {
                W = width,
                H = height,
            },
            HorizontalAlign = PalHorizontalAlign.Right,
            VerticalAlign = PalVerticalAlign.Middle,
        });

        //
        // 绘制信息文本
        //
        PalText.DrawTexts(atlas, infos);
    }

    /// <summary>
    /// 将当前帧的画面绘制到屏幕上。这里会处理地图和精灵形象。
    /// </summary>
    /// <param name="needBuildScreen">是否构建最终画面</param>
    public static void Draw(bool needBuildScreen = true, bool onlyDraw = false)
    {
        //
        // 第 1 步：清理场景画面
        //
        Clear();

        //
        // 第 2 步：绘制完整的地图，包括两个层的内容
        //
        PalMap.Draw(PalAtlas.Scene, 0);
        PalMap.Draw(PalAtlas.Scene, 1);

        //
        // 第 3 步：应用屏幕波纹效果
        //
        //Screen.ApplyWave(g_pScreen);

        //
        // 第 4 步：绘制所有形象
        //
        DrawSprite(PalAtlas.Scene);

        //
        // 第 5 步：绘制场景调试信息
        //
        DrawDebugInfo(PalAtlas.Scene);

        //
        // 绘制所有图形
        //
        if (needBuildScreen) PalAtlas.Scene.DrawGeometry();

        //
        // 检查是否需要进行淡入效果
        //
        if (PalGlobal.NeedToFadeIn && !onlyDraw)
        {
            PalScreen.Update();
#if DEBUG
            PalScreen.Fade(0, false);
#else
            PalScreen.Fade(1, false);
#endif // DEBUG
            PalGlobal.NeedToFadeIn = false;
        }
    }
}
