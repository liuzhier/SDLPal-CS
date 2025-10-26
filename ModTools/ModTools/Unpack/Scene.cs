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

using Lib.Mod;
using SimpleUtility;
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
        string                  pathScene;
        nint                    pNative, pNative2;
        int                     i, size, sceneCount, eventEnd, j, k, sceneId, eventId, progress;
        CEvent*                 pEvent, pThisEvent;
        CScene*                 pScene, pThisScene, pNextScene;
        string[]                sceneNames, eventNames;
        RGame.Scene             scene;
        RGame.Event             eventObject;
        ushort                  status;
        EventTriggerMode        mode;
        bool                    isAutoTrigger;

        //
        // 输出处理进度
        //
        Util.Log("Unpack the game data. <Scene>");

        //
        // 创建输出目录 Scene
        //
        pathScene = Config.ModWorkPath.Game.Data.Scene;
        COS.Dir(pathScene);

        //
        // 读取 Event 数据
        //
        (pNative, size) = Config.MkfCore.ReadChunk(0);
        pEvent = (CEvent*)pNative;

        //
        // 读取 Scene 数据
        //
        (pNative2, size) = Config.MkfCore.ReadChunk(1);
        pScene = (CScene*)pNative2;
        sceneCount = size / sizeof(CScene) - 1;
        sceneNames = new string[sceneCount + 1];
        Config.SceneEventIndexs = new ushort[sceneCount];

        //
        // 处理 Scene 实体对象
        //
        progress = (sceneCount / 10);
        for (i = 0; i < sceneCount; i++)
        {
            //
            // 创建输出目录 Scene
            //
            sceneId = i + 1;
            COS.Dir($@"{pathScene}\{sceneId:D5}");

            //
            // 输出处理进度
            //
            if (i % progress == 0 || i == sceneCount)
                Util.Log($"Unpack the game data. <Scene: {((float)i / sceneCount * 100):f2}%>");

            //
            // 获取当前 Scene
            //
            pThisScene = &pScene[i];
            pNextScene = &pScene[sceneId];
            Config.SceneEventIndexs[i] = pThisScene->EventObjectIndex;

            //
            // 记录 Scene 名称
            //
            sceneNames[sceneId] = Message.GetScene(sceneId);

            scene = new RGame.Scene
            {
                Name = sceneNames[sceneId],
                MapId = pThisScene->MapId,
                Script = new RGame.SceneScript(
                    Enter: Config.AddAddress(
                        pThisScene->ScriptOnEnter,
                        $"Scene_{sceneId:D5}_Enter",
                        RGame.Address.AddrType.Scene,
                        sceneId
                    ),
                    Teleport: Config.AddAddress(
                        pThisScene->ScriptOnTeleport,
                        $"Scene_{sceneId:D5}_Teleport",
                        RGame.Address.AddrType.Scene,
                        sceneId
                    )
                )
            };

            //
            // 导出 JSON 文件到输出目录
            //
            S.JsonSave(scene, $@"{pathScene}\{sceneId:D5}\Scene.json");

            //
            // 处理 Event 实体对象
            //
            eventEnd = pNextScene->EventObjectIndex;
            j = pThisScene->EventObjectIndex;

            if (eventEnd < j)
                //
                // 后面的是空场景
                //
                continue;

            eventNames = new string[eventEnd - j + 1];
            for (k = 0; j < eventEnd; j++, k++)
            {
                eventId = k + 1;

                //
                // 获取当前 Event
                //
                pThisEvent = &pEvent[j];
                status = pThisEvent->State;
                mode = pThisEvent->TriggerMode;
                isAutoTrigger = mode >= EventTriggerMode.TouchNear;

                //
                // 记录 Scene 名称
                //
                eventNames[eventId] = Message.Sprite[pThisEvent->SpriteId];

                eventObject = new(
                    Name: eventNames[eventId],
                    VanishTime: pThisEvent->VanishTime,
                    X: pThisEvent->X,
                    Y: pThisEvent->Y,
                    Layer: pThisEvent->Layer,
                    Trigger: new(
                        StateCode: status,
                        IsAutoTrigger: isAutoTrigger,
                        Range: (mode == EventTriggerMode.None) ? (short)-1 : (isAutoTrigger ? (short)(mode - EventTriggerMode.TouchNear) : (short)mode)
                    ),
                    Sprite: new RGame.EventSprite
                    {
                        SpriteId = pThisEvent->SpriteId,
                        SpriteFrames = pThisEvent->SpriteFrames,
                        Direction = pThisEvent->Direction,
                        CurrentFrameId = pThisEvent->CurrentFrameId,
                    },
                    Script: new RGame.EventScript(
                        Trigger: Config.AddAddress(
                            pThisEvent->TriggerScript,
                            $"Event_{sceneId:D5}_{eventId:D5}_Trigger",
                            RGame.Address.AddrType.Scene,
                            sceneId
                        ),
                        Auto: Config.AddAddress(
                            pThisEvent->AutoScript,
                            $"Event_{sceneId:D5}_{eventId:D5}_Auto",
                            RGame.Address.AddrType.Scene,
                            sceneId
                        ),
                        TriggerIdleFrame: pThisEvent->TriggerIdleFrame,
                        AutoIdleFrame: pThisEvent->AutoIdleFrame
                    )
                );

                //
                // 导出 JSON 文件到输出目录
                //
                S.JsonSave(eventObject, $@"{pathScene}\{sceneId:D5}\{eventId:D5}.json");
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
