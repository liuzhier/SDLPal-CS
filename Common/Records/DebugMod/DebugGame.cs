using SimpleUtility;
using System;
using System.Runtime.InteropServices;

namespace Records.DebugMod;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct DebugGame : IDisposable
{
    public  Battle*     Battle;
    public  PalFile     File;

    public void Dispose()
    {
        Battle->Dispose();
        File.Dispose();

        C.free(Battle);
    }
}
