using ModTools.Unpack;
using SDLPal;

namespace ModTools;

public static class ModMain
{
    public static void GameMain()
    {
        string      palPath, modPath;

        palPath = @"E:\liuzhier\origin\.NET\SDLPal.NET\Pal\Dos";
        modPath = @"E:\SDLPal-CS";

        //
        // 设置当前工作目录
        //
        //Environment.CurrentDirectory = palPath;

        //
        // 初始化全局配置
        //
        Config.Init(palPath);
        Global.Init(modPath);
        Script.Init();

        //
        // 开始解档
        //
        //Voice.Process();
        //Data.Process();
        Entity.Process();
        Scene.Process();
        Script.Process();

        //
        // 释放全局数据
        //
        Config.Free();
    }
}
