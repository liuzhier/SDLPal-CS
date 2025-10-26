using Lib.Mod;
using ModTools.Unpack;

namespace ModTools;

public static unsafe class ModMain
{
    public static void GoUnpack(string palPath, string modPath)
    {
        //
        // 初始化全局配置
        //
        Util.Log("Initialize the global data.");
        Config.Init(palPath, modPath);
        Util.Log("Initialize the map data.");
        Map.Init();
        Util.Log("Initialize the script data.");
        Script.Init();
        Util.Log("Initialize the Sprite data.");
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
        Message.Free();

        //
        // 解包完毕
        //
        Util.Log("The game has been unpacked!");
    }
}
