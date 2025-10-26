using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace ModTools;

public static partial class Util
{
    public static TextBox LogBox_TextBox { get; set; } = null!;

    public static async Task<string?> PathSelector(Window window, string title)
    {
        var result = await window.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = title
            }
        );

        return (result.Count > 0) ? result[0].Path.LocalPath : null;
    }

    public static void UpdateUi(Action callback) =>
        Dispatcher.UIThread.Invoke(callback);

    public static void UpdateUiAsync(Action callback) =>
        Dispatcher.UIThread.InvokeAsync(callback);

    /// <summary>
    /// 在控制台输出日志信息。
    /// </summary>
    /// <param name="message">欲输出的信息</param>
    public static void Log(string message) =>
        UpdateUiAsync(() =>
        {
            LogBox_TextBox.Text += $"{Environment.NewLine}{message}";
            LogBox_TextBox.ScrollToLine(LogBox_TextBox.GetLineCount() - 1);
        });
}
