using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaControl;
using System;
using System.Threading.Tasks;

namespace Lib.Ala;

public static class AlaUtil
{

    /// <summary>
    /// 打开文件夹选择器
    /// </summary>
    /// <param name="window">父级窗口</param>
    /// <param name="title">文件夹选择器的标题</param>
    /// <returns>用户选择的文件夹的路径</returns>
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

    /// <summary>
    /// 切换到 UI 线程，等待 UI 更新完毕
    /// </summary>
    /// <param name="callback">回调函数，里面是 UI 更新的过程</param>
    public static void UpdateUi(Action callback) =>
        Dispatcher.UIThread.Invoke(callback);

    /// <summary>
    /// 切换到 UI 线程，异步更新 UI
    /// </summary>
    /// <param name="callback">回调函数，里面是 UI 更新的过程</param>
    public static async Task UpdateUiAsync(Action callback) =>
        await Dispatcher.UIThread.InvokeAsync(callback);

    /// <summary>
    /// 显示消息框
    /// </summary>
    /// <param name="text">消息框内容</param>
    /// <param name="title">消息框标题，默认为 Warning</param>
    /// <param name="buttonTitle">消息框按钮标题，默认为 OK</param>
    public static void MsgBox(MessageBox messageBox, string text, string title = "Warning", string buttonTitle = "OK") =>
        UpdateUi(() =>
        {
            messageBox.Title = title;
            messageBox.Text = text;
            messageBox.ButtonTitle = buttonTitle;
            messageBox.IsVisible = true;
        });

    /// <summary>
    /// 显示错误消息框
    /// </summary>
    /// <param name="text">错误消息框内容</param>
    public static void MsgBoxError(MessageBox messageBox, string text) =>
        MsgBox(messageBox, text, "Error", "OK");
}
