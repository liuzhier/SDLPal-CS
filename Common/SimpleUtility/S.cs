using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SimpleUtility;

/// <summary>
/// S 类名称的全写为 Safely，
/// 其旨在为程序提供“安全的工具”。
/// </summary>
public static unsafe class S
{
    /// <summary>
    /// 检查一维数组索引值是否合法，若为负数则转换为倒向取元素。
    /// </summary>
    /// <param name="length">数组长度</param>
    /// <param name="index">索引</param>
    public static void CheckoutArrayIndex(int length, ref int index)
    {
        index = CheckoutArrayIndex(length, index);
    }

    /// <summary>
    /// 检查一维数组索引值是否合法，若为负数则转换为倒向取元素。
    /// </summary>
    /// <param name="length">数组长度</param>
    /// <param name="index">索引</param>
    /// <returns>转换后的索引</returns>
    public static int CheckoutArrayIndex(int length, int index)
    {
        if (index < 0) index = length - index;

        return index;
    }

    /// <summary>
    /// 检查二维数组索引值是否合法，若为负数则转换为倒向取元素。
    /// </summary>
    /// <param name="length">数组总长度</param>
    /// <param name="index">索引</param>
    /// <param name="index2">索引2</param>
    public static void CheckoutArrayIndex2(int length, ref int index, ref int index2) => (index, index2) = CheckoutArrayIndex2(length, index, index2);

    /// <summary>
    /// 检查二维数组索引值是否合法，若为负数则转换为倒向取元素。
    /// </summary>
    /// <param name="length">数组总长度</param>
    /// <param name="index">索引</param>
    /// <param name="index2">索引2</param>
    /// <returns>转换后的两个索引</returns>
    public static (int, int) CheckoutArrayIndex2(int length, int index, int index2)
    {
        if (index < 0) index = length - index;
        if (index2 < 0) index2 = length - index2;

        return (index, index2);
    }

    /// <summary>
    /// 复制文件。
    /// </summary>
    /// <param name="pathIn">文件所在目录</param>
    /// <param name="pathFileIn">文件名称</param>
    /// <param name="pathOut">欲复制到的目标目录</param>
    /// <param name="pathFileOut">另存为的文件名称</param>
    public static void FileCopy(string pathIn, string pathFileIn, string pathOut, string? pathFileOut = null)
    {
        if (pathFileOut == null || pathFileOut == "")
            pathFileOut = pathFileIn;

        File.Copy($@"{pathIn}\{pathFileIn}", $@"{pathOut}\{pathFileOut}", true);
    }

    /// <summary>
    /// 复制整个目录。
    /// </summary>
    /// <param name="sourceDirectory">欲复制</param>
    /// <param name="fileFullName">欲复制的文件名</param>
    /// <param name="destDirectory">目标文件所在目录</param>
    public static void DirCopy(string sourceDirectory, string fileFullName, string destDirectory, bool checkPath = true)
    {
        IEnumerable<string>     files;

        if (checkPath)
            COS.Dir(destDirectory);

        files = Directory.EnumerateFiles(sourceDirectory, fileFullName);

        foreach (string pngFile in files)
            FileCopy(sourceDirectory, Path.GetFileName(pngFile), destDirectory);
    }

    /// <summary>
    /// 复制整个目录。
    /// </summary>
    /// <param name="sourceDirectory">欲复制</param>
    /// <param name="fileFullName">欲复制的文件名</param>
    /// <param name="destDirectory">目标文件所在目录</param>
    public static void DirCopy(string sourceDirectory, string[] fileFullNames, string destDirectory)
    {
        COS.Dir(destDirectory);

        foreach (string fileFullName in fileFullNames)
            DirCopy(sourceDirectory, fileFullName, destDirectory, checkPath: false);
    }

    /// <summary>
    /// 弹出错误消息框。
    /// </summary>
    /// <param name="funcName">触发错误的函数名</param>
    /// <param name="error">错误信息内容</param>
    /// <param name="fIsCorrect">错误触发器</param>
    public static void Failed(string funcName, string error, bool fIsCorrect = false)
    {
        string      logFatal;

        //
        // 如果没有触发错误，直接退出
        //
        if (fIsCorrect) return;

        logFatal = $"{funcName} failed: {error}";

        if (error.Last() != '.')
        {
            logFatal += '.';
        }

        //
        // 提示错误
        //
        throw new Exception($"FATAL ERROR: {logFatal}");

        //
        // 遇到错误，退出游戏
        //
        //PalMain.Free();
    }

    /// <summary>
    /// 获取当前日期，格式为 YYYY-MM-DD。
    /// </summary>
    /// <returns>当前日期</returns>
    public static string GetCurrDate()
    {
        string          time;
        DateTime        now;

        //
        // 获取当前时间
        //
        time = "";
        now = DateTime.Now;

        //
        // 格式化日期字符串
        //
        try
        {
            time = string.Format(
               "{0}-{1}-{2}",
               now.Year, now.Month, now.Day
            );
        }
        catch (Exception e)
        {
            Failed(
               "S.GetCurrDate",
               e.Message
            );
        }

        return time;
    }

    /// <summary>
    /// 获取当前时间，格式为 HH-MM-SS_MMM。
    /// </summary>
    /// <returns>当前时间</returns>
    public static string GetCurrTime()
    {
        string      time;
        DateTime    now;

        //
        // 获取当前时间
        //
        time = "";
        now = DateTime.Now;

        //
        // 格式化时间字符串
        //
        try
        {
            time = string.Format(
               "{0}-{1}-{2}_{3}",
               now.Hour, now.Minute, now.Second, now.Millisecond
            );
        }
        catch (Exception e)
        {
            Failed(
               "S.GetCurrTime",
               e.Message
            );
        }

        return time;
    }

    /// <summary>
    /// 获取自定义的 JavaScriptEncoder 对象
    /// </summary>
    static readonly JsonAutoEncoder DefaultEncoder = new();

    /// <summary>
    /// 获取自定义的  JsonAuto 对象，用来序列化和反序列化
    /// </summary>
    static readonly JsonAuto JsonE = new(
        new JsonSerializerOptions
        {
            Encoder = DefaultEncoder,
            WriteIndented = true,
        }
    );

    /// <summary>
    /// 将记录对象序列化为 Json 格式
    /// </summary>
    /// <typeparam name="T">记录对象的类型</typeparam>
    /// <param name="obj">记录对象</param>
    /// <param name="path">Json 文件保存路径</param>
    public static void JsonSave<T>(T obj, string path)
    {
        using FileStream fs = File.Create(path);

        //JsonSerializer.Serialize(fs, obj, options: JsonE.Options);
        JsonSerializer.Serialize(fs, obj, typeof(T), JsonE);
    }

    /// <summary>
    /// 将 Json 串反序列化为记录对象
    /// </summary>
    /// <typeparam name="T">记录对象的类型</typeparam>
    /// <param name="obj">记录对象</param>
    /// <param name="path">Json 文件所在路径</param>
    public static void JsonLoad<T>(out T obj, string path)
    {
        using FileStream fs = File.OpenRead(path);

        //obj = JsonSerializer.Deserialize<T>(fs, options: JsonE.Options);
        obj = (T)JsonSerializer.Deserialize(fs, typeof(T), JsonE)!;
    }

    /// <summary>
    /// 检查指定的条件，如果条件计算结果为 false，则触发跟踪断言失败。
    /// </summary>
    /// <param name="conditions">要计算的条件。如果为 false，则触发跟踪断言失败。</param>
    /// <param name="message">在断言失败时显示的可选消息，可为 null。</param>
    /// <param name="datailMessage">如果断言失败，将显示一条可选的详细消息，可为 null。</param>
    public static void Assert(bool conditions, string? message = null, string? datailMessage = null) => Trace.Assert(conditions, message, datailMessage);

    /// <summary>
    /// 检查文件是否存在，可触发断言。
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="isAssert">是否触发断言</param>
    /// <returns>文件是否存在</returns>
    public static bool FileExist(string path, bool isAssert = true)
    {
        bool        result;

        result = File.Exists(path);

        if (isAssert)
            Assert(result, $@"文件 {path} 不存在！");

        return result;
    }

    /// <summary>
    /// 检查路径是否存在，可触发断言。
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="isAssert">是否触发断言</param>
    /// <returns>文件是否存在</returns>
    public static bool DirExist(string path, bool isAssert = true)
    {
        bool        result;

        result = Directory.Exists(path);

        if (isAssert)
            Assert(result, $@"文件 {path} 不存在！");

        return result;
    }

    /// <summary>
    /// 将单词拼接为路径
    /// </summary>
    /// <param name="paths">各个单词</param>
    /// <returns>拼接后的路径</returns>
    public static string Paths(params string[] paths) => Path.Combine(paths);

    /// <summary>
    /// 将对象索引内容写入文件，若文件已存在则覆盖内容
    /// </summary>
    /// <param name="fileContent">文件内容</param>
    /// <param name="savePath">保存路径（不包括文件名，文件名固定为“#.txt”）</param>
    public static void IndexFileSave(string[] fileContent, string savePath)
    {
        int     i;

        using StreamWriter file = File.CreateText($@"{savePath}\#.txt");

        for (i = 0; i < fileContent.Length; i++)
            file.WriteLine($"{i + 1}\t{fileContent[i]}");
    }
}
