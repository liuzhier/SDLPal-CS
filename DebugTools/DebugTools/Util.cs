using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SimpleUtility;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace DebugTools;

public unsafe static class Util
{
    public delegate void ProcessGame(params object?[] objects);
    static readonly PixelPoint _VanishPosition = new(-0x7FFF_FFFF, -0x7FFF_FFFF);
    public static void VanishAvaloniaWindow(Window avaloniaWindow) =>
        //
        // 将窗口藏到屏幕之外看不到的地方
        //
        avaloniaWindow.Position = _VanishPosition;

    public delegate void PointerPressed(object? sender = null, PointerEventArgs e = null!);

    /// <summary>
    /// 钩住 PAL.EXE 窗口
    /// </summary>
    /// <param name="hookData">遮罩窗口数据</param>
    /// <param name="processGame">遮罩窗口处理回调</param>
    /// <param name="objects">遮罩窗口处理回调的参数</param>
    public static void HookPalWindow(HookAvaloniaData hookData, ProcessGame processGame, params object?[] objects)
    {
        Process[]       processes;
        Process         process;

        if (hookData.IsInit || (hookData.NeedHook = ((Config.PalHandle = (nint)User32.FindWindow("ThunderRTMain")) == 0)))
        {
            //
            // 标记为不是首次初始化
            //
            hookData.IsInit = false;

            //
            // 需要进行 Hook 工作，先隐藏窗口
            //
            VanishAvaloniaWindow(hookData.Window);

            //
            // PAL.EXE 已退出/未打开，关闭已 Hook 状态
            //
            hookData.IsHooked = false;
        }
        else if (hookData.IsHooked)
            //
            // Hook 工作已经处理完毕了
            // 直接去更新 Avalonia 窗口状态
            //
            goto processGame;
        else if (!hookData.NeedHook)
            //
            // 已找到类名为 ThunderRTMain 的窗口，直接进行 Hook
            //
            goto beginHook;

        //
        // 标记 Hook 状态
        // 获取所有匹配的进程
        //
        if ((processes = Process.GetProcessesByName("PAL")).Length > 0)
        {
            //
            // 取第一个进程
            //
            process = processes[0];

            //
            // 使用 Kernel32 打开进程
            //
            Config.PalProcess = Kernel32.OpenProcess(
                new(
                    Kernel32.ProcessAccess.PROCESS_VM_OPERATION
                    | Kernel32.ProcessAccess.PROCESS_VM_WRITE
                    | Kernel32.ProcessAccess.PROCESS_VM_READ
                    | Kernel32.ProcessAccess.PROCESS_QUERY_INFORMATION
                ), false, (uint)process.Id
            );

            //
            // 获取 PAL.EXE 窗口句柄成功
            //
            if (!(hookData.NeedHook = Config.PalProcess.IsInvalid))
                Config.PalHandle = process.MainWindowHandle;
        }

        if (hookData.NeedHook)
        {
            //
            // 等待程序运行，进入下一轮 PAL.EXE 窗口查找
            //
            return;
        }

    beginHook:
        //
        // 获取 Avalonia 窗口句柄
        //
        if ((Config.PalHandle != 0) && (hookData.Handle != 0))
        {
            //
            // 安装 PAL.EXE 的按键输入钩子
            //
            HookPalKeyboard();

            //
            // 激活 PAL.EXE 窗口，并设置为 Avalonia 的父级窗口
            //
            User32.SetForegroundWindow(Config.PalHandle);
            User32.SetWindowLong(hookData.Handle, User32.WindowLongFlags.GWL_HWNDPARENT, Config.PalHandle);

            //
            // 标记 Hook 完毕状态
            //
            hookData.IsHooked = true;
        }

    processGame:
        processGame(objects);
    }

    /// <summary>
    /// 将数值转换为高 DPI 单位
    /// </summary>
    /// <param name="windowHandle">Windows32 窗口句柄</param>
    /// <param name="value">数值</param>
    /// <returns>转换后的数值</returns>
    public static double ToHighDpiUnit(nint windowHandle, int value) =>
        value / (User32.GetDpiForWindow(windowHandle) / 96.0);

    /// <summary>
    /// 将数值转换为高 DPI 单位
    /// </summary>
    /// <param name="avaloniaWindow">Avalonia 窗口对象</param>
    /// <param name="value">数值</param>
    /// <returns>转换后的数值</returns>
    public static double ToHighDpiUnit(Window avaloniaWindow, int value)
    {
        nint        avaloniaHandle;

        if ((avaloniaHandle = avaloniaWindow.TryGetPlatformHandle()?.Handle ?? 0) == 0)
            //
            // 获取 Avalonia Window Handle 失败
            //
            return 0;

        return ToHighDpiUnit(avaloniaHandle, value);
    }

    /// <summary>
    /// 切换到 UI 线程，等待 UI 更新完毕
    /// </summary>
    /// <param name="callback">回调函数，里面是 UI 更新的过程</param>
    public static void UpdateUi(Action callback) =>
        Dispatcher.UIThread.Post(callback);

    public enum HookPage : uint
    {
        None                = 0,
        BattleData          = (1 << 0),
        NonTruncatedInput   = (1 << 15),
        EnemyStatus         = (1 << 16),
        LearnableMagic      = (1 << 17),
    }

    static User32.SafeHHOOK? _hookHandle { get; set; } = null;
    static readonly User32.HookProc _hookProc = HookCallback;
    static bool[] _palKey { get; set; } = new bool[0xFF];
    public static HookPage PalDataPage { get; set; } = HookPage.None;
    public static bool CheckDataPageOpened(HookPage dataPage) => (PalDataPage & dataPage) != 0;
    public static bool CheckDataPageExists() => PalDataPage != HookPage.None;
    public static bool CheckDataPageExistsWithTruncatedInput() => PalDataPage > HookPage.NonTruncatedInput;
    public static void OpenDataPage(HookPage dataPage) => PalDataPage |= dataPage;
    public static void ToggleDataPage(HookPage dataPage) => PalDataPage ^= dataPage;
    public static void CloseDataPage(HookPage dataPage) => PalDataPage &= ~dataPage;
    public static void ClearPalKeyStates() => _palKey.AsSpan().Clear();
    public static bool PalKeyPressed(User32.VK key) => _palKey[(int)key];

    /// <summary>
    /// 设置键盘钩子
    /// </summary>
    /// <returns>是否设置成功</returns>
    public static bool HookPalKeyboard()
    {
        if (Config.PalHandle == 0)
        {
            //
            // PAL.EXE 句柄无效，要先卸载钩子
            //
            FreeHookPalKeyboard();
            return false;
        }

        if (_hookHandle != null)
            //
            // 已经设置 Hook 了
            //
            return false;

        //
        // 使用 WH_KEYBOARD_LL 设置低级键盘钩子
        // 这种钩子不需要注入到目标进程，更简单安全
        //
        _hookHandle = User32.SetWindowsHookEx(
            User32.HookType.WH_KEYBOARD_LL,
            _hookProc,
            Kernel32.GetModuleHandle(null),
            dwThreadId: 0
        );

        if (_hookHandle?.IsInvalid ?? false)
        {
            //
            // Hook 无效
            //
            _hookHandle = null;

            //
            // Hook 失败
            //
            return false;
        }

        //
        // Hook 成功
        //
        return true;
    }

    /// <summary>
    /// 卸载键盘钩子
    /// </summary>
    public static void FreeHookPalKeyboard() => _hookHandle?.Dispose();

    /// <summary>
    /// 钩子回调函数
    /// </summary>
    /// <param name="nCode">钩子代码，如果 >= 0 则可以处理消息</param>
    /// <param name="wParam">消息类型 (如 WM_KEYDOWN, WM_KEYUP)</param>
    /// <param name="lParam">指向 KBDLLHOOKSTRUCT 结构体的指针</param>
    /// <returns>如果返回1，表示消息已被处理，不再传递；否则调用 CallNextHookEx</returns>
    private static nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        User32.KBDLLHOOKSTRUCT      kbStruct;

        //
        // 检查钩子代码是否有效，并且是按键按下事件
        //
        if (nCode >= 0 && (wParam == (nint)User32.WindowMessage.WM_KEYDOWN || wParam == (nint)User32.WindowMessage.WM_SYSKEYDOWN))
        {
            //
            // 核心：检查当前前台窗口是否是 PAL.EXE
            //
            if (User32.GetForegroundWindow() == Config.PalHandle)
            {
                //
                // 将指针转换为 KBDLLHOOKSTRUCT 结构体
                //
                kbStruct = Marshal.PtrToStructure<User32.KBDLLHOOKSTRUCT>(lParam);

                //
                // 转换为 PAL 按键状态映射
                //
                _palKey[kbStruct.vkCode] = true;
            }
        }

        //
        // 将消息传递给下一个钩子或目标窗口
        // 如果不传递，它们将无法接受按键输入
        //
        return CheckDataPageExistsWithTruncatedInput() && (User32.GetForegroundWindow() == Config.PalHandle) ? 1 : User32.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    static User32.CURSORINFO _cursorInfo;

    /// <summary>
    /// 强制显示鼠标光标
    /// </summary>
    public static void ShowCursor()
    {
        User32.GetCursorInfo(ref _cursorInfo);
        if (_cursorInfo.flags == User32.CursorState.CURSOR_HIDDEN)
            User32.ShowCursor(true);
    }

    /// <summary>
    /// 检查 PAL.EXE 是否是活动窗口
    /// </summary>
    /// <returns>PAL.EXE 是否是活动窗口</returns>
    public static bool PalWindowIsActive() =>
        User32.GetActiveWindow() == Config.PalHandle;

    /// <summary>
    /// 检查 PAL.EXE 是否是前景窗口
    /// </summary>
    /// <returns>PAL.EXE 是否是前景窗口</returns>
    public static bool PalWindowIsForeground() =>
        User32.GetForegroundWindow() == Config.PalHandle;

    static User32.WINDOWPLACEMENT _placement = new ()
    {
        length = (uint)Marshal.SizeOf<User32.WINDOWPLACEMENT>(),
    };


    public static bool PalWindowIsMinimized()
    {
        // 获取窗口状态信息
        if (User32.GetWindowPlacement(Config.PalHandle, ref _placement))
        {
            // 检查 showCmd 字段是否为 SW_SHOWMINIMIZED
            return _placement.showCmd == ShowWindowCommand.SW_SHOWMINIMIZED;
        }

        return true;
    }

    /// <summary>
    /// 读取内存
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="address">从哪里读</param>
    /// <param name="buffer">读到何处</param>
    /// <returns></returns>
    public static bool PalRead<T>(nint address, T* buffer) where T : unmanaged =>
        Config.PalIsHooked && Kernel32.ReadProcessMemory(Config.PalProcess, address, (nint)buffer, sizeof(T), out _);

    /// <summary>
    /// 将缓冲区内容写入 PAL.EXE 指定地址
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="address">欲写入的地址</param>
    /// <param name="buffer">缓冲区指针</param>
    /// <param name="size">欲写入的大小，缺省（-1）则为数据类型的大小</param>
    /// <returns>是否写入成功</returns>
    public static bool PalWrite<T>(nint address, T* buffer, nint size = -1) where T : unmanaged =>
        Config.PalIsHooked && Kernel32.WriteProcessMemory(Config.PalProcess, address, (nint)buffer, ((size == -1) ? sizeof(T) : size), out _);

    /// <summary>
    /// 为 PAL.EXE 释放指定地址的内存
    /// </summary>
    /// <param name="address">欲释放的内存地址</param>
    /// <returns>是否成功</returns>
    public static bool PalFree(nint address) =>
        Config.PalIsHooked && Kernel32.VirtualFreeEx(Config.PalProcess, address, 0, Kernel32.MEM_ALLOCATION_TYPE.MEM_FREE);

    /// <summary>
    /// 修改目标进程的内存：释放旧内存，分配新内存，并写入数据。
    /// </summary>
    /// <param name="address">要被替换的旧内存地址</param>
    /// <param name="buffer">要写入的新数据的缓冲区</param>
    /// <param name="bufferSize">缓冲区大小</param>
    /// <returns>新分配的内存地址，失败时返回 0</returns>
    public static nint PalRealloc<T>(nint address, T* buffer, nint bufferSize = -1) where T : unmanaged
    {
        nint        newAddress;

        if (!Kernel32.VirtualFreeEx(Config.PalProcess, address, 0, Kernel32.MEM_ALLOCATION_TYPE.MEM_FREE))
            //
            // 释放旧内存失败
            //
            return 0;

        if ((newAddress = Kernel32.VirtualAllocEx(Config.PalProcess, 0, ((bufferSize == -1) ? sizeof(T) : bufferSize), Kernel32.MEM_ALLOCATION_TYPE.MEM_COMMIT | Kernel32.MEM_ALLOCATION_TYPE.MEM_RESERVE, Kernel32.MEM_PROTECTION.PAGE_READWRITE)) == 0)
            //
            // 分配新内存失败
            //
            return 0;

        if (PalWrite(newAddress, buffer))
        {
            //
            // 写入新数据失败
            //
            Kernel32.VirtualFreeEx(Config.PalProcess, newAddress, UIntPtr.Zero, Kernel32.MEM_ALLOCATION_TYPE.MEM_FREE);
            return 0;
        }

        return newAddress;
    }

    /// <summary>
    /// 清空图像的所有像素
    /// </summary>
    /// <param name="bitmap">欲绘制的图像</param>
    public static void ClearPixel(WriteableBitmap bitmap)
    {
        nint                buffer;
        int                 stride;
        PixelSize           size;

        //
        // 获取 ILockedFramebuffer 对象
        //
        using (ILockedFramebuffer frameBuffer = bitmap.Lock())
        {
            //
            // 从 frameBuffer 获取所需信息
            //
            buffer = frameBuffer.Address;
            stride = frameBuffer.RowBytes;
            size = frameBuffer.Size;

            //
            // 清空像素
            //
            C.memset(buffer, 0, stride * size.Height);
        }
    }

    /// <summary>
    /// 在图像上绘制带阴影的字符
    /// </summary>
    /// <param name="bitmap">欲绘制的图像</param>
    /// <param name="unicodeId">字符的 unicode 码</param>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <param name="argb">颜色值，缺省为 0xFFFC_FCFC</param>
    public static void DrawCharWithShadow(WriteableBitmap bitmap, ushort unicodeId, int x, int y, uint argb = 0xFFFC_FCFC)
    {
        //
        // 先绘制一遍阴影
        //
        DrawChar(bitmap, unicodeId, x + 1, y + 1, 0xFF00_0000);

        if ((argb & 0xFF00_0000) != 0)
        {
            //
            // 只有非透明时才绘制（不支持半透明）
            //
            argb |= 0xFF00_0000;
            DrawChar(bitmap, unicodeId, x, y, argb);
        }
    }

    /// <summary>
    /// 在图像上绘制字符
    /// </summary>
    /// <param name="bitmap">欲绘制的图像</param>
    /// <param name="unicodeId">字符的 unicode 码</param>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <param name="argb">颜色值，缺省为 0xFFFC_FCFC</param>
    public static void DrawChar(WriteableBitmap bitmap, ushort unicodeId, int x, int y, uint argb = 0xFFFC_FCFC)
    {
        nint                        buffer;
        PixelSize                   size;
        int                         stride, charWidth, row, rowChar, column, columnChar;
        uint*                       pUInt32;
        ReadOnlySpan<ushort>        thisChar;

        //
        // 获取 ILockedFramebuffer 对象
        //
        using ILockedFramebuffer frameBuffer = bitmap.Lock();

        //
        // 从 frameBuffer 获取所需信息
        //
        buffer = frameBuffer.Address;
        stride = frameBuffer.RowBytes;
        size = frameBuffer.Size;

        //
        // 绘制文字
        //
        thisChar = CodePage.GetCharMask(unicodeId);
        charWidth = CodePage.GetCharWidth(unicodeId);
        for (row = y, rowChar = 0; row < size.Height && rowChar < CodePage.CharHeight; row++, rowChar++)
        {
            pUInt32 = (uint*)(buffer + row * stride);
            for (column = x, columnChar = 0; column < size.Width && columnChar < charWidth; column++, columnChar++)
                if ((thisChar[rowChar] & (1 << columnChar)) != 0)
                    pUInt32[column] = argb;
        }
    }

    /// <summary>
    /// 在图像上绘制文本
    /// </summary>
    /// <param name="bitmap">欲绘制的图像</param>
    /// <param name="text">欲绘制的文本</param>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <param name="argb">颜色值，缺省为 0xFFFC_FCFC</param>
    /// <param name="haveShadow">是否绘制阴影，缺省为是</param>
    public static void DrawText(WriteableBitmap bitmap, string text, int x, int y, uint argb = 0xFFFCFCFC, bool haveShadow = true)
    {
        int         i;
        ushort      unicodeId;

        for (i = 0; i < text.Length; i++)
        {
            //
            // 绘制单个字符
            //
            unicodeId = text[i];
            DrawCharWithShadow(bitmap, unicodeId, x, y);
            x += CodePage.GetCharWidth(unicodeId);
        }
    }
}
