using ModTools.Unpack;
using SDLPal;

namespace ModTools;

public static class ModMain
{
    public static void GameMain(string[] args)
    {
        string      palPath, modPath;

#if DEBUG
        palPath = @"E:\liuzhier\origin\.NET\SDLPal.NET\Pal\Win";
        modPath = @"E:\SDLPal-CS";
#else
        if (args.Length == 0)
        {
            palPath = @".\";
            modPath = @".\";
        }
        else
        {
            palPath = args[0];
            modPath = args[1];
        }
#endif // DEBUG

        //
        // 设置当前工作目录
        //
        //Environment.CurrentDirectory = palPath;

        //
        // 初始化全局配置
        //
        Config.Init(palPath);
        Global.Init(modPath);
        Map.Init();
        Script.Init();

        //
        // 开始解档
        //
        Voice.Process();
        Map.Process();
        Data.Process();
        Entity.Process();
        Scene.Process();
        Script.Process();

        //
        // 释放全局数据
        //
        Config.Free();
    }
}
