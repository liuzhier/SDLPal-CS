using SDLPal;
using SimpleUtility;
using static ModTools.Record.Core;
using RGame = SDLPal.Record.RGame;

namespace ModTools.Unpack;

public static unsafe class Scene
{
    static ushort[] SceneEventIndexs { get; set; } = null!;

    /// <summary>
    /// 解档 Scene 实体对象。
    /// </summary>
    public static void Process()
    {
        string          pathScene;
        nint            pNative, pNative2;
        int             i, size, sceneCount, eventEnd, sceneId, j, k, eventId, progress;
        CEvent*         pEvent, pThisEvent;
        CScene*         pScene, pThisScene, pNextScene;
        string[]        sceneNames, eventNames;
        RGame.Scene     scene;
        RGame.Event     eventObject;
        ushort          status, mode, enumTouchNear;
        bool            isAutoTrigger;

        //
        // 输出处理进度
        //
        S.Log("Unpack the game data. <Scene>");

        //
        // 创建输出目录 Scene
        //
        pathScene = Global.WorkPath.Game.Data.Scene;
        COS.Dir(pathScene);

        //
        // 读取 Event 数据
        //
        (pNative, size) = Util.ReadMkfChunk(Config.FileCore, 0);
        pEvent = (CEvent*)pNative;

        //
        // 读取 Scene 数据
        //
        (pNative2, size) = Util.ReadMkfChunk(Config.FileCore, 1);
        pScene = (CScene*)pNative2;
        sceneCount = size / sizeof(CScene) - 1;
        sceneNames = new string[sceneCount];
        SceneEventIndexs = new ushort[sceneCount];

        //
        // 处理 Scene 实体对象
        //
        enumTouchNear = (ushort)EventTriggerMode.TouchNear;
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
            if (sceneId % progress == 0 || sceneId == sceneCount)
                S.Log($"Unpack the game data. <Scene: {((float)sceneId / sceneCount * 100):f2}%>");

            //
            // 获取当前 Scene
            //
            pThisScene = &pScene[i];
            pNextScene = &pScene[sceneId];
            SceneEventIndexs[i] = pThisScene->EventObjectIndex;

            //
            // 记录 Scene 名称
            //
            sceneNames[i] = Message.Scene[sceneId];

            scene = new RGame.Scene
            {
                Name = sceneNames[i],
                MapId = pThisScene->MapId,
                Script = new RGame.SceneScript
                {
                    EnterTag = Script.AddAddress(
                        pThisScene->ScriptOnEnter,
                        $"Scene_{sceneId:D5}_Enter",
                        Script.AddressType.Scene,
                        sceneId
                    ),
                    TeleportTag = Script.AddAddress(
                        pThisScene->ScriptOnTeleport,
                        $"Scene_{sceneId:D5}_Teleport",
                        Script.AddressType.Scene,
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
            eventEnd = pNextScene->EventObjectIndex;
            j = pThisScene->EventObjectIndex;
            eventNames = new string[eventEnd - j];
            for (k = 0; j < eventEnd; j++, k++)
            {
                //
                // 获取当前 Event
                //
                pThisEvent = &pEvent[j];
                status = pThisEvent->State;
                mode = pThisEvent->TriggerMode;
                isAutoTrigger = mode >= enumTouchNear;

                //
                // 记录 Scene 名称
                //
                eventNames[k] = Message.Sprite[pThisEvent->SpriteId];

                eventId = k + 1;
                eventObject = new(
                    Name: eventNames[k],
                    VanishTime: pThisEvent->VanishTime,
                    X: pThisEvent->X,
                    Y: pThisEvent->Y,
                    Layer: pThisEvent->Layer,
                    Trigger: new(
                        StateCode: status,
                        IsAutoTrigger: isAutoTrigger,
                        Range: (ushort)(isAutoTrigger ? mode - enumTouchNear : mode)
                    ),
                    Sprite: new RGame.EventSprite
                    {
                        SpriteId = pThisEvent->SpriteId,
                        SpriteFrames = pThisEvent->SpriteFrames,
                        Direction = pThisEvent->Direction,
                        CurrentFrameNum = pThisEvent->CurrentFrameNum,
                    },
                    Script: new RGame.EventScript
                    {
                        TriggerTag = Script.AddAddress(
                            pThisEvent->TriggerScript,
                            $"Event_{sceneId:D5}_{eventId:D5}_Trigger",
                            Script.AddressType.Scene,
                            sceneId
                        ),
                        AutoTag = Script.AddAddress(
                            pThisEvent->AutoScript,
                            $"Event_{sceneId:D5}_{eventId:D5}_Auto",
                            Script.AddressType.Scene,
                            sceneId
                        ),
                    }
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

    /// <summary>
    /// 将原游戏的硬 EventId 转换为软 SceneId 和软 EventId
    /// </summary>
    /// <param name="originEventId">原游戏的硬 EventId</param>
    /// <returns>软 SceneId 和软 EventId</returns>
    public static (short, short) GetSoftSceneEventId(short originEventId)
    {
        short      sceneId, eventId;

        if (originEventId == -1)
        {
            sceneId = eventId = -1;
        }
        else
        {
            for (sceneId = 0; sceneId < SceneEventIndexs.Length; sceneId++)
                if (originEventId < SceneEventIndexs[sceneId])
                    break;

            eventId = (short)(originEventId - SceneEventIndexs[--sceneId]);
        }

        return (sceneId, eventId);
    }
}
