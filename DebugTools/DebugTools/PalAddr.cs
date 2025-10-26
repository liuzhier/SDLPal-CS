using Records.Pal;
using SimpleUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DebugTools;

public static class PalAddr
{
    public const nint PalOriginDll  = 0x10000000;

    public static class Battle
    {
        public const nint IsInBatttle = PalOriginDll + 0xE54D;

        public static class Enemy
        {

        }
    }
}
