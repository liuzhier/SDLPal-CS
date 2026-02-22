using System;
using System.Collections.Generic;
using System.Text;

namespace ModTools;

public static class UiUtil
{
    public delegate void UiLogDelegate(string message);
    public static UiLogDelegate UiLog { get; set; } = null!;

    /// <summary>
    /// 在控制台输出日志信息
    /// </summary>
    /// <param name="message">欲输出的信息</param>
    public static void Log(string message) => UiLog(message);
}
