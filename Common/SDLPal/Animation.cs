using SDL3;

namespace SDLPal;

public static class PalAnimation
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="animationId">动画编号</param>
    /// <param name="beginFrame">动画起始帧</param>
    /// <param name="endFrame">动画终止帧（区间左闭右开）</param>
    /// <param name="speed">播放速度</param>
    public static void Play(int animationId, int beginFrameId = 0, int endFrameId = -1, int speed = 16)
    {
        var delay = (double)SDL.GetPerformanceFrequency() / (speed == 0 ? 16 : speed);
        var time = (double)SDL.GetPerformanceCounter();

        for (var frameId = beginFrameId; frameId != endFrameId; frameId++)
        {
            time += delay;

            var path = $@"{S.ModSpritePath.Animation}\{animationId:D5}\{frameId:D5}.png";
            if (!S.FileExist(path, isAssert: false))
                //
                // 该文件不存在，结束播放
                //
                break;

            var texture = COS.Texture(path);

            PalScreen.Copy(texture, PalScreen.Main, true);
            PalScreen.Update();

            //
            // 如有需要，让屏幕淡入显示
            //
            if (PalGlobal.NeedToFadeIn)
            {
                PalScreen.Fade(1, false);
                PalGlobal.NeedToFadeIn = false;
            }

            FOS.Texture(ref texture);

            PalTimer.DelayUntilPC(time);
        }
    }
}
