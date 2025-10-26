using System;
using System.Runtime.InteropServices;

namespace Records.DebugMod;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Battle : IDisposable
{
    public  bool        IsInBatttle;        // 在战斗中

    public void Dispose()
    {

    }
}
