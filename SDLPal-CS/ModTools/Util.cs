using SimpleUtility;
using RWorkPath = ModTools.Record.RWorkPath;

namespace ModTools;

public static unsafe partial class Util
{
    public enum UnitSize
    {
        MKF = sizeof(int),
        YJ_1 = sizeof(int),
        WinPack = sizeof(int),
        SMKF = sizeof(short),
        DialogIndex = sizeof(int),
    }

    /// <summary>
    /// 以只读权限打开二进制文件。
    /// </summary>
    /// <param name="filePath">文件所在路径</param>
    /// <returns>返回 BinaryReader 对象</returns>
    public static BinaryReader BinaryRead(string filePath) => new(File.OpenRead(filePath));

    /// <summary>
    /// 以读写权限打开二进制文件。
    /// </summary>
    /// <param name="filePath">文件所在路径</param>
    /// <returns>返回 BinaryReader 对象</returns>
    public static BinaryWriter BinaryWrite(string filePath) => new(File.OpenWrite(filePath));

    /// <summary>
    /// 打开一组二进制文件。
    /// </summary>
    /// <param name="filePath">各个文件所在路径</param>
    /// <returns>返回 BinaryReader[] 对象</returns>
    public static BinaryReader[] OpenBinaryGroup(params string[] filePath)
    {
        List<BinaryReader>      gameFileList;

        gameFileList = [];

        foreach (var path in filePath)
            gameFileList.Add(BinaryRead(path));

        return [.. gameFileList];
    }

    /// <summary>
    /// 关闭二进制文件。
    /// </summary>
    /// <param name="file">欲关闭的文件</param>
    public static void CloseBinary(BinaryReader file) => file.Dispose();
    public static void CloseBinary(BinaryWriter file) => file.Dispose();

    /// <summary>
    /// 关闭一组二进制文件。
    /// </summary>
    /// <param name="fileList">欲关闭的那一组文件</param>
    public static void CloseBinaryGroup(BinaryReader[] fileList)
    {
        foreach (var file in fileList)
            CloseBinary(file);
    }

    /// <summary>
    /// 检查 PAL 资源版本。
    /// </summary>
    /// <param name="gamePath">游戏所在路径</param>
    /// <returns>若为 DOS 版游戏资源，返回 true，否则返回 false</returns>
    public static bool CheckVersion(RWorkPath.Root workPath)
    {
        bool                    isDosGame;
        RWorkPath.Bitmap        bitmap;
        BinaryReader[]          gameFileList;
        int                     count, i, dosFlagCount, dataSize;
        BinaryReader            coreFile;

        //
        // 打开部分资源文件，检查版本
        //
        bitmap = workPath.Bitmap;
        gameFileList = OpenBinaryGroup(
            bitmap.Enemy,
            workPath.DataBase.Map,
            bitmap.HeroFight,
            bitmap.FightBackPicture,
            bitmap.FightEffect,
            bitmap.Character
        );

        //
        // 检查有个 DOS 游戏资源文件
        //
        isDosGame = false;
        dosFlagCount = 0;
        foreach (var file in gameFileList)
        {
            //
            // 获取文件中块的数量
            //
            count = GetMkfChunkCount(file);

            //
            // 查找文件中首块非空二进制流
            //
            for (i = 0; i < count && GetMkfChunkSize(file, i) < 4; i++) ;

            if (i >= count)
                goto EndCheckVersion;

            if (i < count)
            {
                //
                // 将文件光标定位到块
                //
                SeekMkfChunk(file, i);

                //
                // 检查块是否以 YJ_1 开头
                //
                if (file.ReadInt32() != 0x315f4a59)
                    goto EndCheckVersion;

                dosFlagCount++;
            }
        }

        //
        // 计算核心文件中对象数据区块的大小
        //
        coreFile = BinaryRead(workPath.DataBase.Core);
        dataSize = GetMkfChunkSize(coreFile, 2);
        CloseBinary(coreFile);

        //
        // 检查核心文件中第二个块是否能和 DOS 版资源结构对齐
        //
        if (dosFlagCount == gameFileList.Length && dataSize % 12 == 0)
            isDosGame = true;

        EndCheckVersion:
        //
        // 关闭所有二进制文件
        //
        CloseBinaryGroup(gameFileList);

        return isDosGame;
    }

    /// <summary>
    /// 将文件光标定位到 MKF 文件中指定块的索引位置。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    public static void SeekMkfHeader(BinaryReader binaryReader, int chunkId) => binaryReader.BaseStream.Seek(chunkId * (int)UnitSize.MKF, SeekOrigin.Begin);

    /// <summary>
    /// 将文件光标定位到 MKF 文件中的指定块。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    public static void SeekMkfChunk(BinaryReader binaryReader, int chunkId)
    {
        //
        // 将文件光标等位到文件头指定块索引处
        //
        SeekMkfHeader(binaryReader, chunkId);

        //
        // 将光标定位到块
        //
        binaryReader.BaseStream.Seek(binaryReader.ReadInt32(), SeekOrigin.Begin);
    }

    /// <summary>
    /// 获取 MKF 文件中的块数量。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <returns>块总数</returns>
    public static int GetMkfChunkCount(BinaryReader binaryReader)
    {
        //
        // 将文件光标等位到文件开头
        //
        SeekMkfHeader(binaryReader, 0);

        //
        // 读取文件头部第一个整数，计算块数量
        //
        return binaryReader.ReadInt32() / (int)UnitSize.MKF - 1;
    }

    /// <summary>
    /// 检查块编号是否超出最大块，超出则抛出异常。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    public static void SafetyCheckMkfChunk(BinaryReader binaryReader, int chunkId)
    {
        if (chunkId >= GetMkfChunkCount(binaryReader))
            throw new Exception("Utility.GetMkfChunkSize: 块编号超出最大块。");
    }

    /// <summary>
    /// 获取 MKF 中指定块的大小。
    /// </summary>
    /// <param name="binaryReader">目标文件</param>
    /// <param name="chunkId">块编号</param>
    /// <returns>块大小</returns>
    public static int GetMkfChunkSize(BinaryReader binaryReader, int chunkId)
    {
        //
        // 检查块
        //
        SafetyCheckMkfChunk(binaryReader, chunkId);

        //
        // 将文件光标等位到文件头部中的指定块索引
        //
        binaryReader.BaseStream.Seek(chunkId * (int)UnitSize.MKF, SeekOrigin.Begin);

        //
        // 计算块大小
        //
        return -(binaryReader.ReadInt32() - binaryReader.ReadInt32());
    }

    public static (nint, int) ReadMkfChunk(BinaryReader binaryReader, int chunkId)
    {
        int             chunkLen;
        nint            pDest;
        Span<byte>      span;

        if (binaryReader == null)
            throw new Exception("Unpak.ReadMKFChunk: The file pointer is empty");

        pDest = 0;

        //
        // Get the length of the chunk.
        //
        chunkLen = GetMkfChunkSize(binaryReader, chunkId);

        if (chunkLen != 0)
        {
            pDest = C.malloc(chunkLen);
            span = new Span<byte>((void*)pDest, chunkLen);

            SeekMkfChunk(binaryReader, chunkId);

            binaryReader.Read(span);
        }

        return (pDest, chunkLen);
    }

    /// <summary>
    /// 根据游戏资源版本来选择解码方法
    /// </summary>
    /// <param name="source">源二进制流</param>
    /// <returns>解码后的二进制流和流长度</returns>
    public static (nint, int) Unpack(nint source, bool isDosGame) => isDosGame ? UnpackDos(source) : UnpackWin(source);
}
