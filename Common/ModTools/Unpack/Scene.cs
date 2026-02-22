#region License
/*
 * Copyright (c) 2025, liuzhier <lichunxiao_lcx@qq.com>.
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
#endregion License

using Records.Mod.RGame;
using SDLPal;
using static Records.Pal.Core;
using RGame = Records.Mod.RGame;

namespace ModTools.Unpack;

public static unsafe class Scene
{
    /// <summary>
    /// 解档 Scene 实体对象。
    /// </summary>
    public static void Process()
    {
        //
        // 输出处理进度
        //
        UiUtil.Log("Unpack the game data. <Scene>");

        //
        // 创建输出目录 Scene
        //
        var pathScene = PalConfig.ModWorkPath.Assets.Data.Scene;
        COS.Dir(pathScene);

        //
        // 读取 Event 数据
        //
        var (pNative, size) = PalConfig.MkfCore.ReadChunk(0);
        var pEvent = (CEvent*)pNative;

        //
        // 读取 Scene 数据
        //
        (var pNative2, size) = PalConfig.MkfCore.ReadChunk(1);
        var pScene = (CScene*)pNative2;
        var sceneCount = size / sizeof(CScene) - 1;
        var sceneNames = new string[sceneCount + 1];
        PalConfig.SceneEventIndexs = new ushort[sceneCount];

        //
        // 处理 Scene 实体对象
        //
        var progress = (sceneCount / 10);
        for (var i = 0; i < sceneCount; i++)
        {
            //
            // 创建输出目录 Scene
            //
            var sceneId = i + 1;
            COS.Dir($@"{pathScene}\{sceneId:D5}");

            //
            // 输出处理进度
            //
            if (i % progress == 0 || i == sceneCount)
                UiUtil.Log($"Unpack the game data. <Scene: {((float)i / sceneCount * 100):f2}%>");

            //
            // 获取当前 Scene
            //
            var pThisScene = &pScene[i];
            var pNextScene = &pScene[sceneId];
            PalConfig.SceneEventIndexs[i] = pThisScene->EventObjectIndex;

            //
            // 记录 Scene 名称
            //
            PalMessage.TryGetEnumEntry($"Scene{PalConfig.Version}", sceneId, out sceneNames[sceneId]);

            var scene = new RGame.Scene
            {
                Name = sceneNames[sceneId],
                MapId = pThisScene->MapId,
                Script = new()
                {
                    EnterTag = PalConfig.AddAddress(
                        pThisScene->ScriptOnEnter,
                        $"Scene_{sceneId:D5}_Enter",
                        RGame.Address.AddrType.Scene,
                        sceneId
                    ),
                    TeleportTag = PalConfig.AddAddress(
                        pThisScene->ScriptOnTeleport,
                        $"Scene_{sceneId:D5}_Teleport",
                        RGame.Address.AddrType.Scene,
                        sceneId
                    )
                },
            };

            //
            // 导出 JSON 文件到输出目录
            //
            S.JsonSave(scene, $@"{pathScene}\{sceneId:D5}\Scene.json");

            //
            // 处理 Event 实体对象
            //
            var eventEnd = pNextScene->EventObjectIndex;
            var j = pThisScene->EventObjectIndex;

            if (eventEnd < j)
                //
                // 后面的是空场景
                //
                continue;

            var eventNames = new string[eventEnd - j + 1];
            for (var k = 0; j < eventEnd; j++, k++)
            {
                var eventId = k + 1;

                //
                // 获取当前 Event
                //
                var pThisEvent = &pEvent[j];
                var status = pThisEvent->State;
                var mode = pThisEvent->TriggerMode;
                var isAutoTrigger = mode >= EventTriggerMode.TouchNear;

                //
                // 记录 Scene 名称
                //
                PalMessage.TryGetEnumEntry("Sprite", pThisEvent->SpriteId, out eventNames[eventId]);

                var @event = new Event()
                {
                    Name = eventNames[eventId],
                    Trigger = new()
                    {
                        StateCode = status,
                        IsAutoTrigger = isAutoTrigger,
                        Range = isAutoTrigger ? (short)(mode - EventTriggerMode.TouchNear + 1) : (short)mode,
                    },
                    Sprite = new()
                    {
                        SpriteId = pThisEvent->SpriteId,
                        Layer = pThisEvent->Layer,
                        VanishTime = pThisEvent->VanishTime,
                        FramesPerDirection = pThisEvent->FramesPerDirection,
                        Trail = new()
                        {
                            Pos = new(pThisEvent->X, pThisEvent->Y),
                            Direction = pThisEvent->Direction,
                            FrameId = pThisEvent->CurrentFrameId,
                        },
                    },
                    Script = new()
                    {
                        TriggerTag = PalConfig.AddAddress(
                            pThisEvent->TriggerScript,
                            $"Event_{sceneId:D5}_{eventId:D5}_Trigger",
                            RGame.Address.AddrType.Scene,
                            sceneId
                        ),
                        AutoTag = PalConfig.AddAddress(
                            pThisEvent->AutoScript,
                            $"Event_{sceneId:D5}_{eventId:D5}_Auto",
                            RGame.Address.AddrType.Scene,
                            sceneId
                        ),
                        TriggerIdleFrame = pThisEvent->TriggerIdleFrame,
                        AutoIdleFrame = pThisEvent->AutoIdleFrame,
                    }
                };

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(@event, $@"{pathScene}\{sceneId:D5}\{eventId:D5}.json");
            }

            //
            // 导出索引文件
            //
            S.IndexFileSave(eventNames, $@"{pathScene}\{sceneId:D5}");
        }

        //
        // 导出索引文件
        //
        S.IndexFileSave(sceneNames, pathScene);

        //
        // 释放非托管内存
        //
        C.free(pNative);
        C.free(pNative2);
    }
}
