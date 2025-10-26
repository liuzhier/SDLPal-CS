using ModTools.Unpack;
using SDLPal;
using SimpleUtility;

namespace ModTools;

public static unsafe class ModMain
{
    public static void GoUnpack(string palPath, string modPath)
    {
        //
        // 初始化全局配置
        //
        Config.Init(palPath);
        Global.Init(modPath);
        Map.Init();
        Script.Init();
        Spirit.Init();

        //
        // 开始解档
        //
        Voice.Process();
        Map.Process();
        Data.Process();
        Entity.Process();
        Scene.Process();
        Script.Process();
        Spirit.Process();

        //
        // 释放全局数据
        //
        Config.Free();

        //
        // 解包完毕
        //
        S.Log("[END] The game has been unpacked!");
    }
}
