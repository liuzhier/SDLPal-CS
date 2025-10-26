using Lib.Mod;
using Lib.Pal;
using SimpleUtility;
using System;
using System.IO;

namespace ModTools.Compile;

public static unsafe class Palette
{
    /// <summary>
    /// 编译调色板
    /// </summary>
    public static void Process()
    {
        const int       beginId = 0;

        string          palettePath, filePath, suffix;
        MkfWriter       mkf;
        FileReader      fileDay, fileNigth;
        int             i, count;

        //
        // 输出处理进度
        //
        Util.Log("Compiling Palette.");

        //
        // 获取 Palette 目录
        //
        palettePath = Config.ModWorkPath.Game.Palette.PathName;
        suffix = Config.ModWorkPath.Game.Palette.Suffix;

        //
        // 创建调色板文件
        //
        mkf = new(Config.PalWorkPath.DataBase.Palette);
        fileNigth = null!;

        //
        // 检查文件序列数量
        //
        count = ModUtil.GetFileSequenceCount(palettePath, beginId, digitNumber: 2, filePrefix: "A", fileSuffix: $".{suffix}");

        //
        // 创建文件头
        //
        mkf.SetLength(sizeof(int) * (beginId + count + 1));

        //
        // 开始处理音效序列
        //
        for (i = 0; i < count; i++)
        {
            //
            // 将 Chunk 地址写入 MKF 文件头
            //
            mkf.WriteHeader(i, (int)mkf.Length);

            if (i < beginId)
                //
                // 跳过不存在的文件
                //
                continue;

            //
            // 读取 ACT 颜色表数据 - 昼
            //
            if ((fileDay = new($@"{palettePath}\A{i:D2}.{suffix}")).Length == 0)
                //
                // 跳过空文件
                //
                goto loop_continue;

            //
            // 将 Chunk 写入 MKF 尾部
            //
            mkf.AppendPalette(fileDay.ReadAll(768));

            //
            // 读取 ACT 颜色表数据 - 夜
            //
            filePath = $@"{palettePath}\B{i:D2}.{suffix}";
            if (!S.FileExist(filePath, isAssert: false) || (fileNigth = new(filePath)).Length == 0)
                //
                // 跳过空文件
                //
                goto loop_continue;

            //
            // 将 Chunk 写入 MKF 尾部
            //
            mkf.AppendPalette(fileNigth.ReadAll(768));

        loop_continue:
            //
            // 关闭 MOD 数据文件
            //
            fileDay?.Dispose();
            fileNigth?.Dispose();
        }

        //
        // MKF 文件头最后的数据是文件长度
        //
        mkf.WriteHeader(i, (int)mkf.Length);

        //
        // 保存并关闭文件
        //
        mkf?.Dispose();
    }
}
