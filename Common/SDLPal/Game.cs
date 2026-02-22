using SDL3;

namespace SDLPal;

public class PalGame
{
    /// <summary>
    /// 游戏正式开始的入口
    /// </summary>
    public static void GameMain()
    {
#if DEBUG
        PalGlobal.CurrentSaveId = 0;
#else
        //
        // 显示标题画面菜单
        //
        PalGlobal.CurrentSaveId = PalUiGame.TitleMenu();
#endif // DEBUG

        //
        // 将资源加载标志设置为加载游戏资源
        //
        PalGlobal.ReloadInNextTick(PalGlobal.CurrentSaveId);

        //
        // 进入游戏主循环
        //
        var time = SDL.GetTicks();
        while (true)
        {
            //
            // 按需加载游戏资源
            //
            PalResource.Load();

            //
            // 清除上一帧的输入状态
            //
            PalInput.ClearKeyState();

            //
            // 等待一帧的时间并接收输入
            //
            PalTimer.DelayUntil(time);
            time = SDL.GetTicks() + PalScene.FrameTime;

            //
            // 运行常规主框架
            //
            PalPlay.StartFrame();
        }
    }
}
