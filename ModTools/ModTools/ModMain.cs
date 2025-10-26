using ModTools.Compile;
using ModTools.Unpack;

namespace ModTools;

public static unsafe class ModMain
{
    /// <summary>
    /// 解包游戏资源
    /// </summary>
    /// <param name="palPath">游戏资源目录</param>
    /// <param name="modPath">MOD 输出目录</param>
    public static void GoUnpack(string palPath, string modPath) =>
        UnpackMain.Process(palPath, modPath);

    /// <summary>
    /// 编译 MOD 资源
    /// </summary>
    /// <param name="modPath">MOD 资源目录</param>
    /// <param name="compiledPath">资源编译输出目录</param>
    /// <param name="isDosGame">欲编译的游戏版本</param>
    public static void GoCompile(string modPath, string compiledPath, bool? isDosGame) =>
        CompileMain.Process(modPath, compiledPath, isDosGame);
}
