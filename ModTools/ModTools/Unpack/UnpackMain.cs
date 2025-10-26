using Lib.Mod;

namespace ModTools.Unpack;

public static unsafe class UnpackMain
{
    /// <summary>
    /// 解包游戏资源
    /// </summary>
    /// <param name="palPath">游戏资源目录</param>
    /// <param name="modPath">MOD 输出目录</param>
    public static void Process(string palPath, string modPath)
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
        Util.Log("The game resources have been unpacked successfully!");
    }

    public static void GoCompile()
    {


        //
        // 编译完毕
        //
        Util.Log("The game resources have been compiled successfully!");
    }
}
