using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModTools.Ui;

public static class Util
{
    public static Thread ThreadLog { get; set; } = null!;
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

    public static unsafe void StartLog()
    {
        ThreadLog ??= new(() =>
        {
            Span<char>      span;
            string          log;

            while (true)
            {
                span = new((void*)SimpleUtility.S.LogBuffer, SimpleUtility.S.LogBufferSize);
                log = span[..span.IndexOf('\0')].ToString();
                SimpleUtility.S.ClearLogBuffer();

                if (log.Length != 0)
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        LogBox_TextBox.Text += Environment.NewLine + log;
                        LogBox_TextBox.CaretIndex = LogBox_TextBox.Text.Length;
                        LogBox_TextBox.ScrollToLine(LogBox_TextBox.GetLineCount() - 1);
                    });

                if (log.StartsWith("[END] "))
                    break;

                Thread.Sleep(10);
            }
        })
        {
            IsBackground = true
        };

        ThreadLog.Start();
    }
}
