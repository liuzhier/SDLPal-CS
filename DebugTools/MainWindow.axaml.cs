using Avalonia.Controls;
using Avalonia.Threading;
using DebugTools;
using System;
using System.Threading;
using Vanara.PInvoke;

namespace PalDebugTools;

public partial class MainWindow : Window
{
    /// <summary>
    /// 初始化 Avalonia 窗口
    /// </summary>
    public MainWindow()
    {
        //
        // 初始化窗口组件
        //
        InitializeComponent();

        //
        // 初始化全局数据
        //
        Config.Init();

        //
        // 初始化数据页窗口
        //
        _hookDataMain = new(this);
        _hookDataPage = new(new DataPageWindow(this), mouseCanPassThrough: true);

        //
        // 初始化计时器
        // 设置间隔为 5 ms
        //
        _showCursorTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(5),
        };
        _showCursorTimer.Tick += ShowCursorTimer_Tick;
        _showCursorTimer?.Start();
        _mainTimer = new(MainTimer_Tick, null, 0, 1);
        _pageTimer = new(PageTimer_Tick, null, 0, 1);

        //
        // 手动为控件订阅事件（不要在 axaml 页面直接订阅事件，否则触发设计器未知 bug 报错）
        //
        Closed += (s, e) =>
        {
            if (!isClosing)
            {
                isClosing = true;
                Util.FreeHookPalKeyboard();
                _mainTimer?.Dispose();
                _pageTimer?.Dispose();
                _showCursorTimer?.Stop();

                //
                // 释放全局数据
                //
                CodePage.Free();
                Config.Init();
            }
        };
        Exit_Button.Click += (s, e) => Close();
    }

    bool isClosing { get; set; } = false;
    DispatcherTimer _showCursorTimer { get; init; } = null!;
    Timer _mainTimer { get; init; }
    Timer _pageTimer { get; init; }
    HookAvaloniaData _hookDataMain { get; init; }
    HookAvaloniaData _hookDataPage { get; init; }

    void ShowCursorTimer_Tick(object? source, EventArgs e)
    {
        Util.ShowCursor();

        Hide();
    }

    void MainTimer_Tick(object? source) =>
        Util.UpdateUi(() => Util.HookPalWindow(_hookDataMain, _mainUpdatePosition, (object?)_hookDataMain));


    void PageTimer_Tick(object? source) =>
        Util.UpdateUi(() => Util.HookPalWindow(_hookDataPage, _pageUpdatePosition, (object?)_hookDataPage));


    readonly Util.ProcessGame _mainUpdatePosition = (objects) =>
    {
        HookAvaloniaData?       hookData;
        RECT                    rect, rectClient;
        POINT                   point;

        //
        // 获取 hook data 参数
        //
        hookData = objects[0] as HookAvaloniaData;

        //
        // 更新 Avalonia 高度和坐标
        // 获取窗口区域和用户区域
        // 窗口的高度减去用户区域的高度得到标题栏和边框的高度之和
        //
        if ((hookData != null)
            && User32.GetWindowRect(Config.PalHandle, out rect)
            && User32.GetClientRect(Config.PalHandle, out rectClient)
            && (hookData.Handle != 0))
        {
            //
            // 同步 Avalonia 高度与 PAL.EXE 窗口一致
            //
            hookData.Window.Height = Util.ToHighDpiUnit(hookData.Handle, rectClient.Height);

            //
            // 将范围转化为“左上角”的点
            //
            point = new(rectClient.left, rectClient.top);

            if (User32.ClientToScreen(Config.PalHandle, ref point))
                //
                // 使 Avalonia 窗口紧贴 PAL.EXE 窗口右侧
                //
                hookData.Window.Position = new(rect.right, point.Y);
        }
    };

    readonly Util.ProcessGame _pageUpdatePosition = (objects) =>
    {
        HookAvaloniaData?       hookData;
        RECT                    rect, rectClient;
        POINT                   point;

        //
        // 获取 hook data 参数
        //
        hookData = objects[0] as HookAvaloniaData;

        //
        // 更新 Avalonia 高度和坐标
        // 获取窗口区域和用户区域
        // 窗口的高度减去用户区域的高度得到标题栏和边框的高度之和
        //
        if ((hookData != null) && (hookData.Handle != 0) && (Config.PalHandle != 0)
            && User32.GetWindowRect(Config.PalHandle, out rect)
            && User32.GetClientRect(Config.PalHandle, out rectClient))
        {
            //
            // 同步 Window 大小与 PAL.EXE 窗口一致
            //
            hookData.Window.Width = Util.ToHighDpiUnit(hookData.Handle, rectClient.Width);
            hookData.Window.Height = Util.ToHighDpiUnit(hookData.Handle, rectClient.Height);

            //
            // 将范围转化为“左上角”的点
            //
            point = new(rectClient.left, rectClient.top);

            if (User32.ClientToScreen(Config.PalHandle, ref point))
                //
                // 使 window 窗口跟随 PAL.EXE 窗口用户区移动
                //
                hookData.Window.Position = new(point.X, point.Y);

            //
            // 当 PAL.EXE 活动的、最前的窗口时，保持遮罩层置顶
            //
            hookData.Window.Topmost = (Util.PalWindowIsActive() && Util.PalWindowIsForeground() && !Util.PalWindowIsMinimized());

            //
            // 保持遮罩层鼠标穿透
            //
            User32.SetWindowLong(
                hookData.Handle,
                User32.WindowLongFlags.GWL_EXSTYLE,
                User32.GetWindowLong(
                    hookData.Handle,
                    User32.WindowLongFlags.GWL_EXSTYLE
                )
                | (int)User32.WindowStylesEx.WS_EX_LAYERED
                | (int)User32.WindowStylesEx.WS_EX_TRANSPARENT
                | (int)User32.WindowStylesEx.WS_EX_NOACTIVATE
            );
        }
    };
}
