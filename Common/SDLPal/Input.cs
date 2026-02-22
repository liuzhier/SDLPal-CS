using Records.Mod;
using SDL3;
using System;
using System.Collections.Generic;

namespace SDLPal;

public static unsafe class PalInput
{
    static readonly List<KeyDict> _keyDict = [
        new(SDL.Keycode.Up,         PalKey.Up),
        new(SDL.Keycode.Kp8,        PalKey.Up),
        new(SDL.Keycode.Down,       PalKey.Down),
        new(SDL.Keycode.Kp2,        PalKey.Down),
        new(SDL.Keycode.Left,       PalKey.Left),
        new(SDL.Keycode.Kp4,        PalKey.Left),
        new(SDL.Keycode.Right,      PalKey.Right),
        new(SDL.Keycode.Kp6,        PalKey.Right),
        new(SDL.Keycode.Escape,     PalKey.Menu),
        new(SDL.Keycode.Insert,     PalKey.Menu),
        new(SDL.Keycode.LAlt,       PalKey.Menu),
        new(SDL.Keycode.RAlt,       PalKey.Menu),
        new(SDL.Keycode.Kp0,        PalKey.Menu),
        new(SDL.Keycode.Return,     PalKey.Search),
        new(SDL.Keycode.Space,      PalKey.Search),
        new(SDL.Keycode.KpEnter,    PalKey.Search),
        new(SDL.Keycode.LCtrl,      PalKey.Search),
        new(SDL.Keycode.Pageup,     PalKey.PgUp),
        new(SDL.Keycode.Kp9,        PalKey.PgUp),
        new(SDL.Keycode.Pagedown,   PalKey.PgDn),
        new(SDL.Keycode.Kp3,        PalKey.PgDn),
        new(SDL.Keycode.Home,       PalKey.Home),
        new(SDL.Keycode.Kp7,        PalKey.Home),
        new(SDL.Keycode.End,        PalKey.End),
        new(SDL.Keycode.Kp1,        PalKey.End),
        new(SDL.Keycode.R,          PalKey.Repeat),
        new(SDL.Keycode.A,          PalKey.Auto),
        new(SDL.Keycode.D,          PalKey.Defend),
        new(SDL.Keycode.E,          PalKey.UseItem),
        new(SDL.Keycode.W,          PalKey.ThrowItem),
        new(SDL.Keycode.Q,          PalKey.Flee),
        new(SDL.Keycode.F,          PalKey.Force),
        new(SDL.Keycode.S,          PalKey.Status),
    ];

    static bool InputPaused { get; set; } = false;
    static ulong[] KeyLastTime { get; set; } = new ulong[_keyDict.Count];
    public static InputState State { get; set; } = new();

    /// <summary>
    /// 初始化输入子系统（禁用输入法候选词 UI）
    /// </summary>
    public static void Init() => SDL.SetHint(SDL.Hints.IMEImplementedUI, "0");

    /// <summary>
    /// 获取当前行走方向
    /// </summary>
    /// <returns>当前行走方向</returns>
    static PalDirection GetCurrDirection()
    {
        int               i;
        PalDirection      dirCurr = PalDirection.South;

        for (i = 1; i < State.KeyOrder.Length; i++)
            if (State.KeyOrder[(int)dirCurr] < State.KeyOrder[i])
                dirCurr = (PalDirection)i;

        if (State.KeyOrder[(int)dirCurr] == 0)
            dirCurr = PalDirection.Current;

        return dirCurr;
    }

    private static bool CheckDirection(PalKey key, PalKey check) => (key & check) != check;

    /// <summary>
    /// 当用户按下按键时触发此事件
    /// </summary>
    /// <param name="key">按键码</param>
    /// <param name="repeat">按键是否被重复按下</param>
    static void KeyDown(PalKey key, bool repeat)
    {
        PalDirection dirCurr;

        dirCurr = PalDirection.Current;

        if (!repeat)
        {
            if (key == PalKey.Down)
                dirCurr = PalDirection.South;
            else if (key == PalKey.Left)
                dirCurr = PalDirection.West;
            else if (key == PalKey.Up)
                dirCurr = PalDirection.North;
            else if (key == PalKey.Right)
                dirCurr = PalDirection.East;

            if (dirCurr != PalDirection.Current)
            {
                State.KeyMaxCount++;
                State.KeyOrder[(int)dirCurr] = State.KeyMaxCount;
                State.Direction = GetCurrDirection();
            }
        }

        State.KeyDown |= key;
    }

    /// <summary>
    /// 当用户松开按键时触发此事件
    /// </summary>
    /// <param name="key">按键码</param>
    static void KeyUp(PalKey key)
    {
        PalDirection      dirCurr;

        dirCurr = PalDirection.Current;

        if (key == PalKey.Down)
            dirCurr = PalDirection.South;
        else if (key == PalKey.Left)
            dirCurr = PalDirection.West;
        else if (key == PalKey.Up)
            dirCurr = PalDirection.North;
        else if (key == PalKey.Right)
            dirCurr = PalDirection.East;

        if (dirCurr != PalDirection.Current)
        {
            State.KeyOrder[(int)dirCurr] = 0;
            dirCurr = GetCurrDirection();
            State.KeyMaxCount = (dirCurr == PalDirection.Current) ? 0 : State.KeyOrder[(int)dirCurr];
            State.Direction = dirCurr;
        }

        State.KeyUp |= key;
    }

    /// <summary>
    /// 检查并更新键盘状态
    /// </summary>
    public static void UpdateKeyboardState()
    {
        var keyStates = SDL.GetKeyboardState(out _);
        var currentTime = SDL.GetTicks();

        for (var i = 0; i < _keyDict.Count; i++)
        {
            if (keyStates[(int)SDL.GetScancodeFromKey(_keyDict[i].KeySDL, out var _)])
            {
                if (currentTime > KeyLastTime[i])
                {
                    KeyDown(_keyDict[i].KeyPAL, (KeyLastTime[i] != 0));
                    if (S.Setup.Input.EnableKeyRepeat)
                        KeyLastTime[i] = currentTime + (ulong)((KeyLastTime[i] == 0) ? 200 : 75);
                    else
                        KeyLastTime[i] = 0xFFFFFFFF;
                }
            }
            else
            {
                if (KeyLastTime[i] != 0)
                {
                    KeyUp(_keyDict[i].KeyPAL);
                    KeyLastTime[i] = 0;
                }
            }
        }
    }

    /// <summary>
    /// 清除已按下按键的记录
    /// </summary>
    public static void ClearKeyState() => State.KeyDown = State.KeyUp = PalKey.None;

    /// <summary>
    /// 判断按键是否按下
    /// </summary>
    /// <param name="keycode">按键码</param>
    /// <returns>按键是否按下</returns>
    public static bool Pressed(PalKey keycode) => (State.KeyDown & keycode) != 0;

    /// <summary>
    /// 判断按键是否弹起
    /// </summary>
    /// <param name="keycode">按键码</param>
    /// <returns>按键是否弹起</returns>
    public static bool Released(PalKey keycode) => (State.KeyUp & keycode) != 0;

    /// <summary>
    /// 处理键盘事件
    /// </summary>
    /// <param name="pEvent">事件</param>
    static void KeyboardEventFilter(SDL.Event* pEvent)
    {
        if ((SDL.EventType)pEvent->Type == SDL.EventType.KeyDown)
            //
            // 自定义快捷键事件
            //
            if ((pEvent->Key.Mod & SDL.Keymod.Alt) != 0)
            {
                //
                // 组合快捷键
                //
                if (pEvent->Key.Key == SDL.Keycode.Return)
                {
                    //
                    // 按下“Alt + Enter”（切换全屏模式）
                    //
                    S.Setup.Window.FullScreen = !S.Setup.Window.FullScreen;

                    SDL.SetWindowFullscreen(PalVideo.Window, S.Setup.Window.FullScreen);
                    return;
                }
                else if (pEvent->Key.Key == SDL.Keycode.F4)
                    //
                    // 按下“Alt + F4”（退出程序）
                    //
                    PalMain.Free();
            }
            else if (pEvent->Key.Key == SDL.Keycode.P)
                //
                // 将游戏画面保存为截图
                //
                PalScreen.SaveScreenshot();
            else if (pEvent->Key.Key == SDL.Keycode.Return)
                //
                // 停止播放视频
                //
                PalGlobal.MoviePlayer?.Stop();
    }

    /// <summary>
    /// SDL 事件过滤函数，一个用于处理所有事件的过滤器
    /// </summary>
    /// <param name="pEvent">事件</param>
    static void EventFilter(SDL.Event* pEvent)
    {
        switch ((SDL.EventType)pEvent->Type)
        {
            case SDL.EventType.WindowResized:
                //
                // 调整窗口大小
                //
                PalVideo.Resize(pEvent->Window.Data1, pEvent->Window.Data2);
                break;

            case SDL.EventType.Quit:
                //
                // 退出游戏
                //
                PalMain.Free();
                break;

            default:
                //
                // 默认行为
                //
                break;
        }

        //
        // 窗口快捷键
        // 所有事件均在此处处理，切勿将任何内容放入内部队列中
        //
        KeyboardEventFilter(pEvent);
    }

    /// <summary>
    /// 抛出并处理一项事件
    /// </summary>
    /// <param name="pEvent">事件</param>
    /// <returns></returns>
    static bool PollEvent(SDL.Event* pEvent)
    {
        var ret = SDL.PollEvent(out var evt);

        if (ret && true)
            EventFilter(&evt);

        if (pEvent != null)
            *pEvent = evt;

        return ret;
    }

    /// <summary>
    /// 处理所有事件
    /// </summary>
    public static void ProcessEvent()
    {
        while (PollEvent(null)) ;

        if (InputPaused)
        {
            InputPaused = !InputPaused;
            return;
        }

        UpdateKeyboardState();
    }
}
