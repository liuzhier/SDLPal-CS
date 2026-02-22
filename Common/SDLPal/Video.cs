using SDL3;

namespace SDLPal;

public static unsafe class PalVideo
{
    public const SDL.PixelFormat DefaultFormat = SDL.PixelFormat.RGBA8888;

    static nint Cursor { get; set; }
    public static nint Window { get; private set; }
    public static nint Renderer { get; private set; }
    public static int Width => S.Setup.Window.ViewportWidth;
    public static int Height => S.Setup.Window.ViewportHeight;

    /// <summary>
    /// 初始化画面子系统
    /// </summary>
    public static void Init()
    {
        //
        // 初始化窗口和渲染器
        //
        S.SDLFailed(
           "PalVideo.Init",
           SDL.CreateWindowAndRenderer(
              $@"SDLPal-CS [{BuildInfo.CompileDateTime}]",
              S.Setup.Window.Width, S.Setup.Window.Height,
              SDL.WindowFlags.Transparent, out var window, out var renderer
           )
        );
        S.SDLWindow = Window = window;
        Renderer = renderer;
        S.Log($"Renderer backend: {Renderer}.");

        //
        // 尝试启用可变刷新率
        //
        S.Log("Trying to enable VRR.");
        if (!SDL.SetRenderVSync(Renderer, SDL.RendererVSyncAdaptive))
        {
            //
            // 启动失败，回退到垂直同步
            //
            S.Log("Failed, fallback to vsync");
            SDL.SetRenderVSync(Renderer, 1);
        }

        //
        // 允许调整窗口大小
        //
        S.SDLFailed(
           "SDL.SetWindowResizable",
           SDL.SetWindowResizable(Window, true)
        );

        //
        // 根据配置文件决定要不要全屏，默认为否
        //
        SDL.SetWindowFullscreen(Window, S.Setup.Window.FullScreen);

#if true
        //
        // 为了便于调试，全屏图像将投射到外部屏幕上
        // To facilitate debugging, the full-screen image will be projected onto an external screen
        //
        SDL.GetDisplayBounds(SDL.GetDisplays(out var _)![^1], out var rect);
        SDL.SetWindowPosition(Window, rect.X, rect.Y);
        SDL.SetWindowFullscreen(Window, true);
#elif false
        var properties = SDL.GetWindowProperties(window);
        var hwnd = SDL.GetPointerProperty(properties, SDL.Props.WindowWin32HWNDPointer, 0);
        User32.SetWindowLong(hwnd, User32.WindowLongFlags.GWL_EXSTYLE, User32.GetWindowLong(hwnd, User32.WindowLongFlags.GWL_EXSTYLE) | (int)User32.WindowStylesEx.WS_EX_LAYERED);
        User32.SetLayeredWindowAttributes(hwnd, new COLORREF(0, 0, 0), 0xFF, User32.LayeredWindowAttributes.LWA_COLORKEY);
        //SDL.GetWindowPosition(Window, out var x, out _);
        //SDL.SetWindowPosition(Window, x, 0);
        //SDL.SetWindowFullscreen(Window, true);
        SDL.SetWindowBordered(Window, false);
#else
        SDL.SetWindowBordered(Window, !S.Setup.Window.FullScreen);
#endif // DEBUG

        //
        // 根据配置文件决定纹理拉伸模式，默认为临近值
        //
        SDL.SetDefaultTextureScaleMode(Renderer, S.Setup.Window.ScaleMode);

        //
        // 设置鼠标光标
        //
        var tempSurface = COS.Surface($@"{S.ModUiPath}\Cursor\{0:D5}.png");
        var cursorSurface = SDL.ScaleSurface(tempSurface, 329 / 4, 734 / 4, S.Setup.Window.ScaleMode);
        Cursor = SDL.CreateColorCursor(cursorSurface, 0, 0);
        SDL.SetCursor(Cursor);
        FOS.Surface(cursorSurface);
        FOS.Surface(tempSurface);

        //
        // 初始化屏幕模块
        //
        PalScreen.Init();

        //
        // 设置窗口大小
        //
        SDL.GetRenderOutputSize(Renderer, out var w, out var h);
        Resize(w, h);
    }

    /// <summary>
    /// 销毁画面子系统
    /// </summary>
    public static void Free()
    {
        //
        // 销毁屏幕模块
        //
        PalScreen.Free();

        //
        // 销毁光标
        //
        SDL.DestroyCursor(Cursor);

        //
        // 销毁 SDL 渲染器和窗口
        //
        S.SDLWindow = 0;
        SDL.DestroyRenderer(Renderer);
        SDL.DestroyWindow(Window);

        //
        // 销毁 SDL 引擎
        //
        SDL.Quit();
    }

    /// <summary>
    /// 调整屏幕内容（调整窗口大小时调用）
    /// </summary>
    /// <param name="width">窗口内容宽度</param>
    /// <param name="height">窗口内容高度</param>
    public static void Resize(int width, int height)
    {
        int            w, h;
        double         ratio;
        SDL.FRect*     pFRect;

        w = Width;
        h = Height;

        pFRect = (SDL.FRect*)PalScreen.DrawFRect;
        pFRect->X = 0;
        pFRect->Y = 0;
        pFRect->W = width;
        pFRect->H = height;

        //
        // 检查是否要保持纵横比
        //
        if (S.Setup.Window.KeepAspectRatio)
        {
            if (((float)width / height) > ((float)Width / Height))
            {
                ratio = (double)w / h;

                pFRect->W = (int)(height * ratio);

                pFRect->X = (width - pFRect->W) / 2;
            }
            else if (((float)height / width) > ((float)Height / Width))
            {
                ratio = (double)h / w;

                pFRect->H = (int)(width * ratio);

                pFRect->Y = (height - pFRect->H) / 2;
            }
        }

        //
        // 重新创建实际屏幕纹理
        //
        FOS.Texture(PalScreen.Actual);
        PalScreen.Actual = COS.Texture((int)pFRect->W, (int)pFRect->H, DefaultFormat, 2);

        //
        // 更新画面
        //
        PalScreen.Update();
    }

}
