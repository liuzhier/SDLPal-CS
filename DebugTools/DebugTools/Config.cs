using System.Threading;
using static Vanara.PInvoke.Kernel32;

namespace DebugTools;

public static unsafe class Config
{
    public static nint PalHandle { get; set; } = 0;
    public static SafeHPROCESS PalProcess { get; set; } = null!;
    static Timer _updateDataTimer { get; set; } = null!;

    public static void Init()
    {
        //
        // 初始化内存数据缓冲区
        //
        PalData.Init();

        //
        // 开始计时器实时更新数据
        //
        _updateDataTimer = new(UpdateDataTimer_Tick, null, 0, 1);
    }

    public static void Free()
    {
        //
        // 释放资源
        //
        PalData.Free();

        //
        // 销毁计时器
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
            Util.ReadPal(PalData.Battle.Addr_IsInBatttle, out PalData.Battle.IsInBatttle);
            Util.ReadPal(PalData.Battle.Addr_IsInBatttle, out PalData.Battle.IsInBatttle);
        }
    }
}
