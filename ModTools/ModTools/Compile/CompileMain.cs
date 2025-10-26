using Lib.Mod;

namespace ModTools.Compile;

public static class CompileMain
{
    /// <summary>
    /// 编译 MOD 资源
    /// </summary>
    /// <param name="modPath">MOD 资源目录</param>
    /// <param name="compiledPath">资源编译输出目录</param>
    /// <param name="isDosGame">游戏资源是否是 Dos 版本</param>
    public static void Process(string modPath, string compiledPath, bool? isDosGame)
    {
        //
        // 初始化全局配置
        //
        Util.Log("Initialize the global data.");
        Config.Init(compiledPath, modPath, isDosGame);

        //
        // 开始编译
        //
        Map.Process();

        //
        // 编译完毕
        //
        Util.Log("The game resources have been compiled successfully!");
    }
}
