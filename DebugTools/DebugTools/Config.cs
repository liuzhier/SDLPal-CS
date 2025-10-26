using Lib.Mod;
using Records.DebugMod;
using Records.Mod.RGame;
using Records.Pal;
using SimpleUtility;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Vanara.PInvoke;
using RModWorkPath = Records.Mod.WorkPath;

namespace DebugTools;

public static unsafe class Config
{
    public static nint PalHandle { get; set; } = 0;
    public static Kernel32.SafeHPROCESS PalProcess { get; set; } = null!;
    public static bool PalIsRunning => ((nint)User32.FindWindow("ThunderRTMain") != 0) || (Process.GetProcessesByName("PAL").Length > 0);
    public static bool PalIsHooked => PalIsRunning && (PalHandle != 0) && (PalProcess != null);
    public static RModWorkPath modWorkPath { get; set; } = null!;
    static Timer _updateDataTimer { get; set; } = new(UpdateDataTimer_Tick, null, 0, 1);
    static HashSet<string[]> _functionNames = null!;

    /// <summary>
    /// 初始化脚本
    /// </summary>
    static void InitScript()
    {

    }

    /// <summary>
    /// 初始化 mod 数据
    /// </summary>
    static void InitDebugGame()
    {
    }

    /// <summary>
    /// 初始化全局游戏数据
    /// </summary>
    public static void Init()
    {
        //
        // 初始化 MOD 工作区路径
        //
#if DEBUG
        modWorkPath = new(@"E:\PAL98_v0.89a\palmod");
#else
        modWorkPath = new(@".\palmod");
#endif // DEBUG

        //
        // 初始化脚本
        //
        InitScript();

        //
        // 将 mod 数据读入缓冲区
        //
        InitDebugGame();
    }

    public static void Free()
    {
        //
        // 销毁所有资源
        //
        _updateDataTimer.Dispose();
    }

    static void UpdateDataTimer_Tick(object? source)
    {
        if ((PalHandle != 0) && (PalProcess != null))
        {
            //
            // 开始更新数据
            //
            //Util.PalRead(PalAddr.Battle.IsInBatttle, &DebugData.Battle->IsInBatttle);
        }
    }
}
