using SDLPal;
using SimpleUtility;
using EntityDos = ModTools.Record.Entity.Dos;
using EntityWin = ModTools.Record.Entity.Win;

namespace ModTools.Unpack
{
    /// <summary>
    /// 解档基础游戏数据。
    /// </summary>
    public static unsafe class Entity
    {
        public static nint BinData { get; set; }
        public static int CoreDataCount { get; set; }
        public static EntityDos* CoreDos { get; set; }
        public static EntityWin* CoreWin { get; set; }

        /// <summary>
        /// 解档游戏实体对象。
        /// </summary>
        public static void Process()
        {
            string      path;
            int         binDataLength;

            //
            // 输出处理进度
            //
            S.Log("Unpack the game data. <Entity>");

            //
            // 创建输出目录
            //
            path = Global.WorkPath.Game.Data.Entity.PathName;
            COS.Dir(path);
             
            //
            // 将数据读入非托管内存
            //
            (BinData, binDataLength) = Util.ReadMkfChunk(Config.FileCore, 2);
            if (Config.IsDosGame)
            {
                CoreDataCount = binDataLength / sizeof(EntityDos);
                CoreDos = (EntityDos*)BinData;
            }
            else
            {
                CoreDataCount = binDataLength / sizeof(EntityWin);
                CoreWin = (EntityWin*)BinData;
            }

            //
            // 处理实体对象数据
            //
            System.Process();
            Hero.Process();
            Item.Process();
            Magic.Process();
            Enemy.Process();
            Poison.Process();

            //
            // 释放非托管内存
            //
            C.free(BinData);
        }
    }
}
