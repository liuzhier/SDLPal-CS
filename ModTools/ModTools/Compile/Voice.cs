using Lib.Mod;
using Lib.Pal;
using SimpleUtility;
using System;
using System.IO;

namespace ModTools.Compile;

public static unsafe class Voice
{
    /// <summary>
    /// 编译音效文件，处理音乐
    /// </summary>
    public static void Process()
    {
        const int       beginId = 1;

        string              pathVoice, suffix, pathMusic;
        MkfWriter           mkf;
        FileReader          fileIn;
        int                 i, endId;
        string[]            suffixs;

        //
        // 输出处理进度
        //
        Util.Log("Compiling Voice.");

        //
        // 获取 Voice 目录
        //
        pathVoice = Config.ModWorkPath.Game.Voice;
        suffix = Config.PalWorkPath.DataBase.Voice.Suffix;

        //
        // 创建音效文件（由于技术限制，强制打包为 SOUNDS.MKF，仅供 SDLPAL 使用）
        //
        mkf = new(Config.PalWorkPath.DataBase.Voice.PathName);

        //
        // 检查文件序列数量
        //
        endId = ModUtil.GetFileSequenceCount(pathVoice, beginId, fileSuffix: $".{suffix}");

        //
        // 创建文件头
        //
        mkf.SetLength(sizeof(int) * (beginId + endId + 1));

        //
        // 开始处理音效序列
        //
        for (i = 0; i <= endId; i++)
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
            // 打开 WAV 音效文件
            //
            if ((fileIn = new($@"{pathVoice}\{i:D5}.{suffix}")).Length == 0)
                //
                // 跳过空文件
                //
                goto loop_continue;

            //
            // 将 Chunk 写入 MKF 尾部
            //
            mkf.Append(fileIn.ReadAll());

        loop_continue:
            //
            // 关闭 MOD 数据文件
            //
            fileIn?.Dispose();
        }

        //
        // MKF 文件头最后的数据是文件长度
        //
        mkf.WriteHeader(i, (int)mkf.Length);

        //
        // 保存并关闭文件
        //
        mkf?.Dispose();

        //
        // 输出处理进度
        //
        Util.Log("Compiling Music or \"MUS.MKF\".");

        //
        // 获取音乐目录
        //
        pathMusic = Config.ModWorkPath.Game.Music;
        suffix = (suffixs = Config.PalWorkPath.DataBase.Music.Suffix)[0];

        if (S.DirExist(pathMusic, isAssert: false))
        {
            //
            // 开始处理音乐
            //
            if (Config.IsDosGame)
            {
                //
                // 将所有 RIX 音乐合并为一个文件
                //

                //
                // 创建音乐文件 MUS.MKF
                //
                mkf = new(Config.PalWorkPath.DataBase.Music.PathName);

                //
                // 检查文件序列数量
                //
                endId = ModUtil.GetFileSequenceCount(pathMusic, beginId, fileSuffix: $".{suffixs[0]}");

                //
                // 创建文件头（需要加上一块末尾）
                //
                mkf.SetLength(sizeof(int) * (beginId + endId + 1));

                //
                // 开始处理音乐序列
                //
                for (i = 0; i <= endId; i++)
                {
                    //
                    // 将 Chunk 地址写入 MKF 文件头
                    //
                    mkf.WriteHeader(i, (int)mkf.Length);

                    if (i < beginId)
                        //
                        // 跳过开头不存在的文件
                        //
                        continue;

                    //
                    // 读取 RIX 音乐数据
                    //
                    if ((fileIn = new($@"{pathMusic}\{i:D5}.{suffix}")).Length == 0)
                        //
                        // 跳过空文件
                        //
                        goto loop_continue;

                    //
                    // 将 Chunk 写入 MKF 尾部
                    //
                    mkf.Append(fileIn.ReadAll());

                loop_continue:
                    //
                    // 关闭 MOD 数据文件
                    //
                    fileIn?.Dispose();
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
            else
            {
                //
                // 复制整个音乐文件夹到编译目录
                //
                S.DirCopy(
                    Config.ModWorkPath.Game.Music,
                    suffixs,
                    Config.PalWorkPath.DataBase.Music.PathName
                );
            }
        }
    }
}
