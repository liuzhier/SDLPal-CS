using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DebugTools;
using System;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace PalDebugTools;

public unsafe partial class DataPageWindow : Window
{
    public DataPageWindow() =>
        //
        // 这又是一个设计器 bug。。。。必须有无参构造
        //
        InitializeComponent();

    public DataPageWindow(Window mainWindow)
    {
        InitializeComponent();

        Show();


        _mainWindow = mainWindow;
        //_BitmapMain = new Bitmap(@"C:\Users\22988\Desktop\pic.png");
        _BitmapMain = null!;
        _frameBuffer = CreateFrameBuffer();

        _updateImageTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(1),
        };
        _updateImageTimer.Tick += UpdateImageTimer_Tick;
        _updateImageTimer?.Start();

        Closed += (s, e) =>
        {
            _updateImageTimer?.Stop();
            _BitmapMain?.Dispose();
            _frameBuffer?.Dispose();
            _mainWindow?.Close();
        };
    }

    Window _mainWindow { get; init; } = null!;
    DispatcherTimer _updateImageTimer { get; init; } = null!;
    Bitmap _BitmapMain { get; init; } = null!;
    WriteableBitmap _frameBuffer { get; set; } = null!;

    WriteableBitmap CreateFrameBuffer() =>
        new WriteableBitmap(
            new(320, 200),
            new(96.0, 96.0),
            format: PixelFormat.Bgra8888,
            alphaFormat: AlphaFormat.Unpremul
        );

    void ClearVideo() =>
        Util.ClearPixel(_frameBuffer);

    void UpdateVideo(bool isHide = false) =>
        Util.UpdateUi(() =>
        {
            WriteableBitmap     newFrameBuffer;
            nint                buffer;
            PixelSize           size;
            int                 stride;

            if (isHide)
                Video_Image.Source = _BitmapMain;
            else if (_frameBuffer != null)
            {
                newFrameBuffer = CreateFrameBuffer();

                //
                // 获取 ILockedFramebuffer 对象
                //
                using (ILockedFramebuffer frameBuffer = _frameBuffer.Lock())
                {
                    //
                    // 从 frameBuffer 获取所需信息
                    //
                    buffer = frameBuffer.Address;
                    stride = frameBuffer.RowBytes;
                    size = frameBuffer.Size;

                    _frameBuffer.CopyPixels(new(size), buffer, stride * size.Height, stride);
                    Video_Image.Source = newFrameBuffer;
                }

                _frameBuffer?.Dispose();
                _frameBuffer = newFrameBuffer;
            }
        });

    public void DrawText(string text, int x, int y, uint argb = 0xFFFCFCFC, bool haveShadow = true) =>
        Util.DrawText(_frameBuffer, text, x, y, argb, haveShadow);

    void UpdateImageTimer_Tick(object? source, EventArgs e)
    {
        if (!Util.CheckDataPageExistsWithTruncatedInput())
        {
            //
            // 没有截断输入性页面被显示，检查非截断输入性页面
            //
            if (Util.CheckDataPageOpened(Util.HookPage.BattleData))
                //
                // 显示战斗额外信息
                //
                ShowBattleInfo();

            //
            // 对应按键
            //
            if (Util.PalKeyPressed(User32.VK.VK_1))
            {
                //
                // 按 1 显示/隐藏战场额外信息
                //
                Util.ToggleDataPage(Util.HookPage.BattleData);
            }
            else if (Util.PalKeyPressed(User32.VK.VK_2))
            {
                //
                // 按 2 打开敌方状态页面
                //
                Util.OpenDataPage(Util.HookPage.EnemyStatus);
            }
            else if (Util.PalKeyPressed(User32.VK.VK_3))
            {
                //
                // 按 3 打开可领悟的法术
                //
                Util.OpenDataPage(Util.HookPage.LearnableMagic);
            }
        }
        //
        // 检查截断输入性页面
        //
        else if (Util.CheckDataPageOpened(Util.HookPage.EnemyStatus))
            //
            // 显示敌方状态页面
            //
            ShowEnemyStatus();
        else if (Util.CheckDataPageOpened(Util.HookPage.LearnableMagic))
            //
            // 显示敌方状态页面
            //
            ShowLearnableMagic();

        //
        // 没有显示任何页面
        //
        UpdateVideo(!Util.CheckDataPageExists());

        //
        // 清理按键状态，这一轮的按键用不到了
        //
        Util.ClearPalKeyStates();
    }

    void ShowBattleInfo()
    {
        Util.UpdateUi(() =>
        {
            DrawText($"在战斗中：{(Config.DebugData.Battle->IsInBatttle ? '是' : '否')}", 0, 0);
        });
    }

    void ShowEnemyStatus()
    {
        if (Util.PalKeyPressed(User32.VK.VK_ESCAPE))
        {
            //
            // 按 ESC 退出页面
            //
            Util.CloseDataPage(Util.HookPage.EnemyStatus);
        }

        Util.UpdateUi(() =>
        {

            DrawText("我便是唯一的光", 0, 0);
        });
    }

    void ShowLearnableMagic()
    {
        if (Util.PalKeyPressed(User32.VK.VK_ESCAPE))
        {
            //
            // 按 ESC 退出页面
            //
            Util.CloseDataPage(Util.HookPage.LearnableMagic);
        }

        Util.UpdateUi(() =>
        {

        });
    }
}
