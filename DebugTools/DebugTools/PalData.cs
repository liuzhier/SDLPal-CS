using Records.Pal;
using SimpleUtility;

namespace DebugTools;

public static unsafe class PalData
{
    public const nint Addr_PalOriginDll  = 0x10000000;

    public static class Battle
    {
        public const nint Addr_IsInBatttle = Addr_PalOriginDll + 0xE54D;
        public static bool IsInBatttle = false;

        public static class Enemy
        {
            public static nint[] BaseData = new nint[Data.MaxEnemysInTeam];
            public static nint[] CoreData = new nint[Data.MaxEnemysInTeam];

            public static void Init()
            {
                int     i;

                //
                // 为敌方基础数据申请缓冲区
                //
                for (i = 0; i < BaseData.Length; i++)
                    BaseData[i] = C.malloc(sizeof(Data.CEnemy));

                //
                // 为敌方核心数据申请缓冲区
                //
                for (i = 0; i < CoreData.Length; i++)
                    CoreData[i] = C.malloc(sizeof(Data.CEnemy));
            }

            public static void Free()
            {
                int     i;

                //
                // 释放敌方基础数据缓冲区
                //
                for (i = 0; i < BaseData.Length; i++)
                    C.free(BaseData[i]);

                //
                // 释放敌方核心数据缓冲区
                //
                for (i = 0; i < CoreData.Length; i++)
                    C.free(CoreData[i]);
            }
        }

        public static void Init()
        {
            Enemy.Init();
        }

        public static void Free()
        {
            Enemy.Free();
        }
    }

    public static void Init()
    {
        Battle.Init();
    }

    public static void Free()
    {
        Battle.Init();
    }
}
