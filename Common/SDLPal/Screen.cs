using Records.Mod;
using SDL3;
using System.Collections.Generic;
using Vanara.PInvoke;

namespace SDLPal;

public static unsafe class PalScreen
{
    public static nint Actual { get; set; }                     // 物理主屏幕（实际屏幕）
    public static nint Main { get; private set; }               // 逻辑主屏幕
    public static nint MainBackup { get; private set; }         // 逻辑屏幕备份
    //public static nint DebugData { get; private set; }          // 屏幕额外数据层
    public static nint Text { get; private set; }               // 屏幕文本层
    public static nint ScreenFade { get; private set; }         // 屏幕淡入淡出层
    public static nint TimeFilter { get; private set; }         // 屏幕时间滤镜层
    public static nint DrawFRect { get; private set; }          // 屏幕实际绘制范围

    public enum BlendMode
    {
        Default,        // 默认 Blend
        Mul,            // 预乘
        Light,          // 去掉暗部
        Mask,           // 剪贴蒙版
    }

    static bool Rotated { get; set; } = false;
    static Dictionary<BlendMode, SDL.BlendMode> BlendModeDict { get; set; } = new()
    {
        [BlendMode.Default] = SDL.BlendMode.Blend,
        [BlendMode.Mul] = SDL.BlendMode.Mul,
        [BlendMode.Light] = SDL.ComposeCustomBlendMode(
            SDL.BlendFactor.One,
            SDL.BlendFactor.One,
            SDL.BlendOperation.Add,
            SDL.BlendFactor.One,
            SDL.BlendFactor.One,
            SDL.BlendOperation.Add
        ),
        [BlendMode.Mask] = SDL.ComposeCustomBlendMode(
            SDL.BlendFactor.SrcAlpha,
            SDL.BlendFactor.OneMinusSrcAlpha,
            SDL.BlendOperation.Add,
            SDL.BlendFactor.Zero,
            SDL.BlendFactor.One,
            SDL.BlendOperation.Add
        ),
    };
    static SDL.BlendMode CurrentBlendMode { get; set; } = BlendModeDict[BlendMode.Default];
    public static bool IsFade { get; set; } = false;
    public static bool IsFadeToScreen { get; set; } = false;
    //public static uint[] FilterColor { get; set; } = [0x00019723, 0x00000000, 0xCF543040, 0x00014270];
    public static Dictionary<PalFilter, uint> FilterColor { get; set; } = new()
    {
        [PalFilter.Morning] = 0x00019723,
        [PalFilter.Noon]    = 0x00000000,
        [PalFilter.Dusk]    = 0xCF543040,
        [PalFilter.Night]   = 0x00014270,
        //[PalFilter.Night]   = 0x001E8092,
    };
    public static void SetBlendMode(BlendMode blendMode) => CurrentBlendMode = BlendModeDict[blendMode];

    /// <summary>
    /// 初始化屏幕模块
    /// </summary>
    public static void Init()
    {
        var w = PalVideo.Width;
        var h = PalVideo.Height;
        var format = PalVideo.DefaultFormat;
        Main = COS.Texture(w, h, format, 2);
        MainBackup = COS.Texture(w, h, format, 2);
        //DebugData = COS.Texture(w, h, format, 2);
        Text = COS.Texture(w, h, format, 2);
        ScreenFade = COS.Texture(1, 1, format, 2);
        TimeFilter = COS.Texture(1, 1, format, 2);

        //
        // 制作淡入淡出遮罩，并先把它隐藏起来
        //
        DrawFRect = COS.FRect(new SDL.FRect
        {
            X = 0,
            Y = 0,
            W = PalVideo.Width,
            H = PalVideo.Height,
        });
        SDL.SetRenderTarget(PalVideo.Renderer, ScreenFade);
        SDL.RenderFillRect(PalVideo.Renderer, DrawFRect);
        SDL.RenderClear(PalVideo.Renderer);
        SDL.SetTextureAlphaMod(ScreenFade, 0x00);
    }

    /// <summary>
    /// 销毁屏幕模块
    /// </summary>
    public static void Free()
    {
        //
        // 销毁全局资源
        //
        C.free(DrawFRect);

        //
        // 销毁 SDL 纹理
        //
        FOS.Texture(TimeFilter);
        FOS.Texture(ScreenFade);
        FOS.Texture(Text);
        //FOS.Texture(DebugData);
        FOS.Texture(MainBackup);
        FOS.Texture(Main);
        FOS.Texture(Actual);
    }

    /// <summary>
    /// 将纹理 src 的某区域绘制到 dest 的某区域
    /// </summary>
    /// <param name="src">源纹理</param>
    /// <param name="dest">目标纹理</param>
    /// <param name="srcFRect">源纹理的区域</param>
    /// <param name="destFRect">目标纹理的区域</param>
    public static void Copy(nint src, nint dest, nint srcFRect, nint destFRect)
    {
        //
        // 将纹理 dest 设置为渲染目标
        //
        SDL.SetRenderTarget(PalVideo.Renderer, dest);

        if (dest == 0)
            //
            // 如果 dest 是屏幕，则先清理屏幕
            //
            SDL.RenderClear(PalVideo.Renderer);

        SDL.SetTextureBlendMode(src, CurrentBlendMode);

        if (Rotated)
            //
            // 水平翻转绘制
            //
            SDL.RenderTextureRotated(PalVideo.Renderer, src, 0, destFRect, 0.0, 0, SDL.FlipMode.Horizontal);
        else
            //
            //正常绘制
            //
            SDL.RenderTexture(PalVideo.Renderer, src, srcFRect, destFRect);
    }

    public static void Copy(nint src, nint dest, nint destFRect) => Copy(src, dest, 0, destFRect);

    public static void Copy(nint src, nint dest, SDL.FRect destFRect) => Copy(src, dest, S.Ptr(&destFRect));

    /// <summary>
    /// 将纹理 src 拉伸绘制到 dest
    /// </summary>
    /// <param name="src">源纹理</param>
    /// <param name="dest">目标纹理</param>
    /// <param name="stretch">是否拉伸</param>
    /// <param name="x">目标纹理的 X 坐标</param>
    /// <param name="y">目标纹理的 Y 坐标</param>
    public static void Copy(nint src, nint dest, bool stretch = true, int x = 0, int y = 0)
    {
        if (stretch)
            //
            // 拉伸至 dest 相同大小
            //
            Copy(src, dest, 0);
        else
        {
            //
            // 按 src 原始大小绘制
            //
            S.GetTexSize(src, out var w, out var h);
            Copy(src, dest, new SDL.FRect
            {
                X = x,
                Y = y,
                W = w,
                H = h,
            });
        }
    }

    public static void CopyMain(nint src, SDL.FRect rectFDest) => Copy(src, Main, rectFDest);

    public static void CopyMain(nint src, bool stretch = true, int x = 0, int y = 0) =>
        Copy(src, Main, stretch, x, y);

    public static void DrawGeometry(Atlas atlas, nint texture = 0)
    {
        if (texture == 0) texture = Main;

        SDL.SetRenderTarget(PalVideo.Renderer, texture);
        SDL.RenderGeometry(PalVideo.Renderer, atlas.Texture, atlas.Vertexs, atlas.Vertexs.Length, atlas.Indices, atlas.Indices.Length);
    }

    /// <summary>
    /// 备份指定屏幕
    /// </summary>
    /// <param name="src">欲备份的屏幕</param>
    public static void Backup(nint src) => Copy(src, MainBackup, false);

    /// <summary>
    /// 将屏幕备份的内容还原到指定屏幕
    /// </summary>
    /// <param name="dest">目标屏幕</param>
    public static void Restore(nint dest) => Copy(MainBackup, dest, false);

    /// <summary>
    /// 将屏幕最终合成结果绘制到窗口
    /// </summary>
    public static void Update()
    {
        SDL.SetRenderTarget(PalVideo.Renderer, Actual);
        SDL.SetRenderDrawColor(PalVideo.Renderer, 0, 0, 0, 0xFF);
        SDL.RenderClear(PalVideo.Renderer);

        if (IsFade)
        {
            if (PalDialog.ForceDrawOnMainScreen)
                //
                // 后续取消将对话强制画到主屏幕
                //
                PalDialog.ForceDrawOnMainScreen = false;

            //
            // 将渐变图层绘制到屏幕上
            //
            Copy(ScreenFade, Actual);

            //
            // 将文本层绘制到屏幕上
            //
            Copy(Text, Actual);
        }

        //
        // 将主视频层绘制到屏幕上
        //
        Copy(Main, Actual);

        //if (S.NeedDrawSceneDebugData)
        //    //
        //    // 将场景调试数据层绘制到屏幕上
        //    //
        //    Copy(DebugData, Actual);

        if (IsFadeToScreen)
            //
            // 将屏幕备份层绘制到屏幕上
            //
            Copy(MainBackup, Actual);

        //
        // 应用时间滤镜层
        //
        SDL.GetTextureAlphaModFloat(TimeFilter, out var alpha);
        if (alpha > 0 || (S.Save.TimeFilter == PalFilter.Night))
        {
            if (S.Save.TimeFilter != PalFilter.Night)
            {
                alpha = float.Max(alpha - 1.5625f / 100.0f / 3f, 0);
                SDL.SetTextureAlphaModFloat(TimeFilter, alpha);
            }

            SDL.SetTextureBlendMode(Actual, SDL.BlendMode.Mul);
            {
                Copy(TimeFilter, Actual);
            }
            SDL.SetTextureBlendMode(Actual, SDL.BlendMode.Blend);
        }

        //
        // 合成最终的画面
        //
        Copy(Actual, 0, DrawFRect);

        //
        // 绘制最终画面到窗口
        //
        SDL.RenderPresent(PalVideo.Renderer);
    }

    /// <summary>
    /// 淡入淡出屏幕
    /// </summary>
    /// <param name="delay">每一帧之间的延迟时间</param>
    /// <param name="isFadeOut">是则淡出，否则淡入</param>
    /// <param name="stepCount">总步长</param>
    public static void Fade(int delay, bool isFadeOut = true, byte stepCount = 2)
    {
        var addNum = stepCount / 255.0f;

        if (isFadeOut)
        {
            IsFade = true;

            for (var alpha = 1f; ; alpha -= addNum)
            {
                SDL.SetTextureAlphaModFloat(Main, alpha);

                Update();

                PalTimer.Delay(delay);

                if (alpha <= 0) break;
            }

            S.CleanUpTex(Text);
        }
        else
        {
            //
            // 检查是否需要过渡时间滤镜
            //
            SDL.GetTextureAlphaMod(TimeFilter, out var filterAlpha);

            for (var alpha = 0f; ; alpha += addNum)
            {
                SDL.SetTextureAlphaModFloat(Main, alpha);

                Update();

                PalTimer.Delay(delay);

                if (alpha >= 1) break;
            }

            IsFade = false;
        }
    }

    /// <summary>
    /// 从屏幕备份淡入到当前屏幕（在此之前将地图备份到屏幕备份，先绘制地图，再淡入屏幕备份，最后绘制角色）
    /// </summary>
    /// <param name="delay">每一帧之间的延迟时间</param>
    /// <param name="isFadeOut">是则淡出，否则淡入</param>
    /// <param name="stepCount">总步长</param>
    public static void FadeAndUpdate(int delay, bool isFadeOut = true, byte stepCount = 2)
    {
        PalGlobal.NeedToFadeIn = false;
        var addNum = stepCount * 1.65f / 255.0f;

        if (isFadeOut)
        {
            for (var alpha = 0f; ; alpha += addNum)
            {
                var time = SDL.GetTicks() + 100;

                SDL.SetTextureAlphaModFloat(Main, alpha);

                PalInput.ClearKeyState();
                PalInput.State.Direction = PalDirection.Current;
                PalPlay.GameUpdate(false);
                PalScene.Draw();
                Update();

                if (alpha >= 1) break;

                while (!PalTimer.TicksPass(SDL.GetTicks(), time))
                {
                    PalInput.ProcessEvent();
                    SDL.Delay(5);
                }
            }

            IsFade = false;
        }
        else
        {
            IsFade = true;

            S.CleanUpTex(Text);

            for (var alpha = 1f; ; alpha -= addNum)
            {
                var time = SDL.GetTicks() + 100;

                SDL.SetTextureAlphaModFloat(Main, alpha);

                PalInput.ClearKeyState();
                PalInput.State.Direction = PalDirection.Current;
                PalPlay.GameUpdate(false);
                PalScene.Draw();
                Update();

                if (alpha <= 0) break;

                while (!PalTimer.TicksPass(SDL.GetTicks(), time))
                {
                    PalInput.ProcessEvent();
                    SDL.Delay(5);
                }
            }
        }
    }

    /// <summary>
    /// 淡出到屏幕备份
    /// </summary>
    /// <param name="delay">每帧延迟</param>
    /// <param name="stepCount">步长</param>
    public static void FadeToScreen(int delay, byte stepCount = 2)
    {
        var addNum = stepCount / 255.0f;

        IsFadeToScreen = true;
        {
            for (var alpha = 1.0f; ; alpha -= addNum)
            {
                SDL.SetTextureAlphaModFloat(MainBackup, alpha);

                Update();

                PalTimer.Delay(delay);

                if (alpha <= 0) break;
            }
        }
        IsFadeToScreen = false;

        SDL.SetTextureAlphaModFloat(MainBackup, 1.0f);
    }

    /// <summary>
    /// 淡入到指定颜色
    /// </summary>
    /// <param name="delay">每帧之间的延迟时间</param>
    /// <param name="color">RGB 颜色值</param>
    /// <param name="isFadeOut">是则从指定颜色淡出，否则淡出到指定颜色</param>
    /// <param name="updateScene">是否正更新场景</param>
    /// <param name="stepCount">总步长</param>
    public static void FadeToColor(int delay, uint color = 0x000000, bool isFadeOut = true, bool updateScene = false, byte stepCount = 2)
    {
        var alpha = 0.0f;
        var addNum = stepCount / 255.0f;

        //
        // Convert to RGBA format
        // Set alpha to 255
        //
        color <<= 8;
        color |= 0xFF;

        S.CleanUpTex(ScreenFade, color);

        SDL.SetTextureAlphaModFloat(Main, alpha);

        if (isFadeOut)
        {
            IsFade = true;

            S.CleanUpTex(Text);

            for (alpha = 0; ; alpha += addNum)
            {
                SDL.SetTextureAlphaModFloat(ScreenFade, alpha);

                if (updateScene)
                {
                    PalInput.ClearKeyState();
                    PalInput.State.Direction = PalDirection.Current;
                    PalPlay.GameUpdate(false);
                    PalScene.Draw();
                }

                Update();

                PalTimer.Delay(delay);

                if (alpha >= 1) break;
            }
        }
        else
        {
            for (alpha = 1; ; alpha -= addNum)
            {
                SDL.SetTextureAlphaModFloat(ScreenFade, alpha);

                if (updateScene)
                {
                    PalInput.ClearKeyState();
                    PalInput.State.Direction = PalDirection.Current;
                    PalPlay.GameUpdate(false);
                    PalScene.Draw();
                }

                Update();

                PalTimer.Delay(delay);

                if (alpha <= 0) break;
            }

            IsFade = false;
        }
    }

    /// <summary>
    /// 从当前的滤镜切换至指定的滤镜
    /// </summary>
    /// <param name="filter">要设置的时间滤镜</param>
    /// <param name="updateScene">是否正更新场景</param>
    /// <param name="isFadeOut">是则淡出，否则淡入</param>
    public static void FadeToFilter(PalFilter filter, bool updateScene, bool isFadeOut = true)
    {
        var alpha = 0.0f;
        var addNum = 1.5625f / 100.0f / 3f;
        var time = (updateScene ? PalScene.FrameTime : PalScene.FrameTime / 4);

        S.CleanUpTex(TimeFilter, FilterColor[filter]);

        SDL.SetTextureAlphaModFloat(TimeFilter, alpha);

        if (isFadeOut)
        {
            for (alpha = 0; ; alpha += addNum)
            {
                SDL.SetTextureAlphaModFloat(TimeFilter, alpha);

                if (updateScene)
                {
                    PalInput.ClearKeyState();
                    PalInput.State.Direction = PalDirection.Current;
                    PalPlay.GameUpdate(false);
                    PalScene.Draw();
                }

                Update();

                PalTimer.Delay(time, 5);

                if (alpha >= 1) break;
            }
        }
        else
        {
            for (alpha = 1; ; alpha -= addNum)
            {
                SDL.SetTextureAlphaModFloat(TimeFilter, alpha);

                if (updateScene)
                {
                    PalInput.ClearKeyState();
                    PalInput.State.Direction = PalDirection.Current;
                    PalPlay.GameUpdate(false);
                    PalScene.Draw();
                }

                Update();

                PalTimer.Delay(time, 5);

                if (alpha <= 0) break;
            }
        }
    }

    /// <summary>
    /// 将纹理保存为 PNG
    /// </summary>
    /// <param name="texture">纹理</param>
    /// <param name="path">保存路径</param>
    public static void SaveScreenshot(nint texture = 0, string path = null!)
    {
        if (texture == 0) texture = Main;

        //
        // 获取纹理像素
        //
        nint surface = S.GetTexPixels(texture);

        //
        // 保存 Surface
        //
        path ??= $@"{S.ModPath.Screenshot}\{S.GetCurrTime()}.png";
        SDL.SavePNG(surface, path);

        //
        // 销毁临时资源
        //
        FOS.Surface(surface);
    }

    /// <summary>
    /// 如果有需要则为纹理应用波浪效果
    /// </summary>
    /// <param name="texture">纹理</param>
    public static void ApplyWave(nint texture)
    {

    }
}
