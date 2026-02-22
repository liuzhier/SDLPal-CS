using Records.Mod;
using SDL3;
using System;

namespace SDLPal;

public static class PalMain
{
    public static bool IsFreeing { get; private set; }

    public static void Init()
    {
        //
        // 设置程序崩溃时执行的回调
        //
        S.ExeFreeCallBack = new(() => Free());

        //
        // 初始化 SDL3 引擎
        //
        S.SDLFailed(
            "PalMain.Init",
            SDL.Init(SDL.InitFlags.Audio | SDL.InitFlags.Video)
        );

        //
        // 初始化全局游戏数据
        //
#if DEBUG
        PalConfig.Init(null!, @"E:\PAL98_new\palmod");
#else
        PalConfig.Init(null!, Environment.CurrentDirectory);
#endif // DEBUG

        //
        // 创建游戏缓存目录
        //
        InitOutputPath();

        //
        // 将新资源覆盖到游戏源数据
        //
        CopyDebugResources();

        //
        // 初始化游戏子系统
        //
        PalInput.Init();
        PalAudio.Init();
        PalVideo.Init();
        PalResource.Init();
    }

    public static void Free()
    {
        if (IsFreeing)
            //
            // 避免重复销毁引擎
            //
            return;

        //
        // 设置状态为正在销毁游戏引擎
        //
        IsFreeing = true;


        //
        // 绘制作者声明
        //
        using (var atlas = new Atlas(PalScreen.Text))
        {
            PalText.DrawText(atlas, new()
            {
                Text = "生活忙碌，人各有琐事，大侠欢迎再来！！！",
                FontSize = 55,
                Foreground = ColorYellow,
                HorizontalAlign = PalHorizontalAlign.Middle,
                VerticalAlign = PalVerticalAlign.Middle,
            });
            atlas.DrawGeometry();
        }
        PalScreen.Fade(1);

        //
        // 销毁游戏子系统
        //
        PalGlobal.MoviePlayer?.Dispose();
        PalAudio.Free();
        PalVideo.Free();
        PalResource.Free();

        //
        // 销毁 SDL3 引擎
        //
        SDL.Quit();

        //
        // 设置状态为已完成游戏引擎的销毁
        //
        IsFreeing = false;

        //
        // 强制结束程序
        //
        Environment.Exit(0);
    }

    /// <summary>
    /// 创建游戏缓存目录
    /// </summary>
    public static void InitOutputPath()
    {
        COS.Dir(S.ModPath.Temp);
        COS.Dir(S.ModPath.Log);
        COS.Dir(S.ModPath.Screenshot);
        COS.Dir(S.ModPath.Save);
    }

    /// <summary>
    /// 将新资源覆盖到游戏源数据
    /// </summary>
    public static void CopyDebugResources() => S.DirCopy($"{S.ModAssetsPath}New", "*", $"{S.ModAssetsPath}");

    /// <summary>
    /// 游戏主入口
    /// </summary>
    public static void Main()
    {
        //
        // 初始化游戏引擎
        //
        Init();

        //
        // 播放背景音乐
        //
        PalAudio.PlayMusic(4);

#if DEBUG
        PalScreen.Fade(0);
#else
        //
        // 播放开场动画
        //
        PalGlobal.MoviePlayer = new($@"{S.ModPath.Assets.Movie}\StartMenu.mp4");
        PalGlobal.MoviePlayer.Play();
        PalScreen.Fade(2);
#endif // DEBUG

        //
        // 进入游戏主循环
        //
        PalGame.GameMain();

        //
        // 理论上不会真的执行到这里
        //
        Free();
    }
}
