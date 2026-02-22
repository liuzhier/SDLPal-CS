using Records.Mod;
using Records.Mod.RGame;
using SDL3;
using System;
using System.Collections.Generic;
using static SDL3.SDL;

namespace SDLPal;

public static class PalDialog
{
    public const int
        //DefaultOutputDelay      = 3,
        DefaultOutputDelay      = 6,
        LineSpacing             = 4,
        BorderWidth             = 4,
        StretchFactor           = 2;
    public const uint DefaultColor = 0xFFFFFF;

    static Pos TopPos => new(0, 0);
    static readonly Pos MiddlePos = new(0, -200);
    static Pos BottomPos => new(0, 0);

    static SDL.Rect Rect;
    static bool AutoSkipWait { get; set; }
    static bool UserSkip { get; set; }
    static int DelayTime { get; set; }
    static float MaxWaitingSeconds { get; set; }
    static bool IsName { get; set; }
    static string Name { get; set; } = null!;
    static int FaceId { get; set; }
    static int CurrentRowId { get; set; }
    static List<TextDrawInfo> TextDrawInfos { get; set; } = null!;
    static PalDialogPosition Position { get; set; }
    static SDL.Color CharColor { get; set; }
    static BackgroundColor Background { get; set; }
    public static int Padding { get; set; }
    public static bool ForceDrawOnMainScreen { get; set; } = false;
    static bool HaveName => (Name != null);
    static int MaxRowId => 3 + ((HaveName && (Position == PalDialogPosition.Top)) ? 1 : 0);

    public enum BackgroundColor
    {
        Black,      // NPC
        Blue,
        Green,
        Green2,
        Red,
        Red2,
    }

    /// <summary>
    /// 初始化对话框模块
    /// </summary>
    public static void Init() => ClearMesssge(false);

    /// <summary>
    /// 设置对话框信息
    /// </summary>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <param name="color">RGB 颜色值，缺省则为白色</param>
    /// <param name="isPlayingRng">是否正在播放 Rng 动画，缺省则为否</param>
    public static void InitMesssge(int x, int y, uint color = DefaultColor, int faceId = 0, bool isPlayingRng = false)
    {
        Rect.X = x;
        Rect.Y = y;

        CharColor = COS.Color((color << 8) | 0xFF);
        FaceId = faceId;

        if (isPlayingRng)
        {
            PalScreen.Backup(PalScreen.Main);
            PalGlobal.IsPlayingCurrentAnimationId = true;
        }
    }

    /// <summary>
    /// 设置对话框信息
    /// </summary>
    /// <param name="pos">对话框位置</param>
    /// <param name="color">RGB 颜色值，缺省则为白色</param>
    /// <param name="isPlayingRng">是否正在播放 Rng 动画，缺省则为否</param>
    public static void InitMesssge(Pos pos, uint color = DefaultColor, int faceId = 0, bool isPlayingRng = false) => InitMesssge(pos.X, pos.Y, color, faceId, isPlayingRng);

    /// <summary>
    /// 设置对话框信息
    /// </summary>
    /// <param name="pos">对话框位置</param>
    /// <param name="color">RGB 颜色值，缺省则为白色</param>
    /// <param name="isPlayingRng">是否正在播放 Rng 动画，缺省则为否</param>
    public static void InitMesssge(PalDialogPosition position, uint color = DefaultColor, int faceId = 0, bool isPlayingRng = false) =>
        InitMesssge((Position = position) switch
        {
            PalDialogPosition.Top => TopPos,
            PalDialogPosition.Middle => MiddlePos,
            PalDialogPosition.Bottom => BottomPos,
            _ => throw S.Failed("PalDialog.InitMesssge", $"Undefined dialog box position '{position}'"),
        }, color, faceId, isPlayingRng);

    /// <summary>
    /// 初始化对话框默认信息
    /// </summary>
    public static void InitDefaultMesssge() => InitMesssge(PalDialogPosition.Top);

    /// <summary>
    /// 清理对话框，如果需要则先等待任意键按下
    /// </summary>
    /// <param name="waitForKey">是否等待按下按键</param>
    /// <param name="needDeleteName">是否删除已缓存的发言者名称</param>
    public static void ClearMesssge(bool waitForKey, bool needDeleteName = true)
    {
        if (AutoSkipWait)
            //
            // 等待指定的秒数
            //
            DialogWaitForKeyWithMaximumSeconds(MaxWaitingSeconds);
        else if ((CurrentRowId > 0) && waitForKey)
            //
            // 等待对话框确认
            //
            DialogWaitForKey();

        CurrentRowId = 0;
        if (needDeleteName)
        {
            Name = null!;
            FaceId = 0;
        }
        TextDrawInfos = [];
        AutoSkipWait = false;
        MaxWaitingSeconds = 0;
        Background = (BackgroundColor)S.RandomLong((int)BackgroundColor.Black, (int)BackgroundColor.Red2);
        Rect = new();

        if ((Position != PalDialogPosition.Top) && (Position != PalDialogPosition.Bottom))
            //
            // 初始化对话框默认信息
            //
            InitDefaultMesssge();

        //
        // 启用逐字延迟功能
        //
        UserSkip = false;
    }

    /// <summary>
    /// 结束对话框，将下个对话框默认位置恢复到上部
    /// </summary>
    public static void EndMesssge()
    {
        ClearMesssge(true, true);
        InitDefaultMesssge();
    }

    /// <summary>
    /// 设置对话显示速度
    /// </summary>
    /// <param name="delayTime">每个字间的延迟，实际每字延迟为 {delayTime * 8}</param>
    public static void SetOutputDelay(int delayTime = DefaultOutputDelay) => DelayTime = delayTime;

    /// <summary>
    /// 等待指定的秒数，期间接收输入，超时退出
    /// </summary>
    /// <param name="maxWaitingSeconds">要等待的秒数</param>
    static void DialogWaitForKeyWithMaximumSeconds(float maxWaitingSeconds)
    {
        var beginningTicks = SDL.GetTicks();
        var TimeOutMs = (ulong)(maxWaitingSeconds * 1000);

        PalInput.ClearKeyState();

        if (AutoSkipWait)
            while (PalTimer.CheakTimeOut(beginningTicks, TimeOutMs)) PalTimer.Delay(100);
        else
            while ((PalInput.State.KeyDown == 0) && PalTimer.CheakTimeOut(beginningTicks, TimeOutMs)) PalTimer.Delay(100);
    }

    /// <summary>
    /// 等待指定的秒数，期间接收输入
    /// </summary>
    /// <param name="MaxWaitingSeconds">要等待的秒数</param>
    static void DialogWaitForKey() => DialogWaitForKeyWithMaximumSeconds(0);

    /// <summary>
    /// 切换到下一页对话
    /// </summary>
    public static void NextPage()
    {
        //
        // 清理对话框
        //
        ClearMesssge(true, needDeleteName: false);
        PalScreen.Restore(PalScreen.Main);

        //
        // 把发言者名称先绘制一遍
        //
        if (HaveName) DrawTalkText(Name);
    }

    public static nint[] GetBoxUi(BackgroundColor color) => PalUiGame.GetUi($"Dialog{(color switch
    {
        BackgroundColor.Blue => "Blue",
        BackgroundColor.Green => "Green",
        BackgroundColor.Green2 => "Green2",
        BackgroundColor.Red => "Red",
        BackgroundColor.Red2 => "Red2",
        BackgroundColor.Black or _ => "Black",
    })}");

    /// <summary>
    /// 显示对话框跳出动画
    /// </summary>
    static AtlasPack DrawDialogAnimation(Atlas atlas)
    {
        var pack = PalUiGame.DrawBox(atlas, new()
        {
            Rect = Rect,
            BackgroundColor = Background,
            HorizontalAlign = PalHorizontalAlign.Middle,
            VerticalAlign = Position switch
            {
                PalDialogPosition.Top => PalVerticalAlign.Top,
                PalDialogPosition.Bottom => PalVerticalAlign.Bottom,
                _ => PalVerticalAlign.Middle,
            },
        });

        if (FaceId > 0)
        {
            //
            // 绘制肖像
            //
            var stretchFactor = 4;
            if (PalSprites.GetAvatar(FaceId, out var surface)) stretchFactor = 2;
            S.GetSurfaceSize(surface, out var w, out _);
            w *= stretchFactor;

            atlas.Add(new(surface, needFree: true), new()
            {
                Rect = new()
                {
                    X = Position switch
                    {
                        PalDialogPosition.Bottom => w,
                        PalDialogPosition.Top or _ => -w,
                    },
                    Y = Position switch
                    {
                        PalDialogPosition.Top => -pack.Rect.Y,
                        PalDialogPosition.Bottom => -pack.Rect.Y,
                        _ => 0,
                    },
                },
                StretchFactor = stretchFactor,
                HorizontalAlign = Position switch
                {
                    PalDialogPosition.Bottom => PalHorizontalAlign.Right,
                    PalDialogPosition.Top or _ => PalHorizontalAlign.Left,
                },
                VerticalAlign = Position switch
                {
                    PalDialogPosition.Bottom => PalVerticalAlign.Bottom,
                    PalDialogPosition.Top or _ => PalVerticalAlign.Top,
                },
                ParentPack = pack,
            });
        }

        //
        // 绘制对话框
        //
        return pack;
    }

    /// <summary>
    /// 判断需要画在哪个离屏的图集上
    /// </summary>
    /// <returns></returns>
    public static ref Atlas GetScreen() => ref ((PalScreen.IsFade && !ForceDrawOnMainScreen) ? ref PalAtlas.Text : ref PalAtlas.Scene);

    /// <summary>
    /// 将绘制对话文本的动作录制到图集
    /// </summary>
    /// <param name="text">要绘制的对话</param>
    public static void DrawTalkText(string text)
    {
        //
        // 检查当前是否是第一行
        //
        var isFirstLine = (CurrentRowId == 0);

        if (isFirstLine)
            //
            // 备份显示对话框前的画面
            //
            PalScreen.Backup(PalScreen.Main);
        else if (CurrentRowId > MaxRowId)
            //
            // 超过五行自动换行
            //
            NextPage();

        //
        // 设置当前行文本绘制参数
        //
        var actualText = S.GetTextActualContent(text);

        //
        // 设置当前行对话绘制信息
        //
        var currentRowInfo = new TextDrawInfo()
        {
            FontSize = PalText.LargeSize,
            PosOffset = new(0, Rect.H),
            Foreground = CharColor,
        };

        //
        // 设置对话框大小
        //
        var offsetX = currentRowInfo.FontSize + PalText.ShadowOffset;
        var (w, h) = S.GetTextActualSize(actualText, currentRowInfo.FontSize);
        Rect.W = int.Max(Rect.W, w + (HaveName ? offsetX : 0));
        Rect.H += h;

        if (CurrentRowId == 0)
            //
            // 偏移 Y 轴，避免对话框过于靠上
            //
            Rect.Y += Position switch
            {
                PalDialogPosition.Top => h,
                PalDialogPosition.Bottom => -h,
                _ => 0,
            };

        //
        // 检查字符串内的特殊参数
        //
        var textSpan = text.AsSpan();
        if ((IsName = (textSpan.EndsWith('：') || textSpan.EndsWith(':'))) && isFirstLine)
        {
            //
            // 该文本为名称，备份名称
            // 显示为金色，不需要后移一段距离或逐字显示
            //
            Name = text;
            currentRowInfo.Foreground = ColorGold;
        }
        else if (textSpan.Length > 2)
        {
            if (textSpan.Length > 2)
            {
                //
                // 非名称文本，检查文本输出速度、显示时间
                //
                var startsNum = (FastChars)null!;

                if ((textSpan.Length >= 3) && textSpan.StartsWith('$') && S.StrTryToInt32(startsNum = textSpan[1..3], out _))
                {
                    //
                    // 设置文本显示的延迟时间（文字显示速度）
                    //
                    SetOutputDelay(S.StrToInt32(startsNum) * 10 / 7);
                    textSpan = textSpan[3..];
                }
            }

            if (textSpan.Length > 2)
            {
                var EndsNum = textSpan[^2..];
                if ((textSpan.Length >= 3) && textSpan[^3].Equals('~') && S.StrTryToInt32(EndsNum, out _))
                {
                    //
                    // 设置对话自动中断等待时间（不须按空格）
                    //
                    MaxWaitingSeconds = S.StrToInt32(EndsNum) / 40f;
                    textSpan = textSpan[..^3];

                    //
                    // 禁止手动确认，只能等到超时后才跳过对话
                    //
                    AutoSkipWait = true;
                }
            }
        }

        if ((CurrentRowId == 0) && !HaveName && (Position == PalDialogPosition.Bottom))
            //
            // 防止没有名称时行数过多
            //
            CurrentRowId++;

        //
        // 有名称的话，正文需要全部后移两个字
        //
        currentRowInfo.PosOffset.X = (!HaveName || (HaveName && IsName) || (Position == PalDialogPosition.Middle)) ? 0 : offsetX;

        //
        // 获取场景图集
        //
        ref Atlas atlas = ref GetScreen();
        atlas = new(atlas);

        //
        // 逐字绘制
        //
        var i = 0;
        var j = 0;
        text = $"{textSpan}";
        for (; i < text.Length; i++)
        {
        BeginDrawChar:
            if (!(UserSkip || PalGlobal.NeedToFadeIn))
            {
                //
                // 清理按键输入
                // 
                PalInput.ClearKeyState();

                //
                // 逐字延迟，并接收按键输入
                //
                PalTimer.Delay(DelayTime * 8);

                if (!AutoSkipWait && PalInput.Pressed(PalKey.Search | PalKey.Menu))
                {
                    //
                    // 用户按下按键以跳过对话框
                    // 此段对话取消逐字延迟效果
                    //
                    UserSkip = true;
                    goto BeginDrawChar;
                }
            }

            //
            // 获取当前文字的绘制信息
            //
            if (!IsName)
            {
                //
                // 设置字体颜色为上一帧配置的颜色
                //
                currentRowInfo.Foreground = CharColor;

                //
                // 检查非名称行文本颜色
                //
                var charColor = currentRowInfo.Foreground;
                switch (text[i])
                {
                    case '_':
                        //
                        // 青色文本
                        //
                        if (charColor.Equals(ColorCyan)) goto DefaultColor;
                        CharColor = ColorCyan;
                        continue;

                    case '@':
                    case '\'':
                        //
                        // 红色文本
                        //
                        if (charColor.Equals(ColorRed)) goto DefaultColor;
                        CharColor = ColorRed;
                        continue;

                    case '\"':
                        //
                        // 黄色文本
                        //
                        if (charColor.Equals(ColorYellow)) goto DefaultColor;
                        CharColor = ColorYellow;
                        continue;

                    DefaultColor:
                        //
                        // 白色文本
                        //
                        CharColor = ColorWhite;
                        continue;

                    default:
                        break;
                }
            }

            if (((Position == PalDialogPosition.Top) || (Position == PalDialogPosition.Bottom)))
            {
                var offsetBeginRowId = Position switch
                {
                    PalDialogPosition.Bottom => MaxRowId,
                    PalDialogPosition.Top or _ => MaxRowId,
                };
                if (CurrentRowId >= offsetBeginRowId)
                    //
                    // 下方对话框有名称且行数达到限制时，使其紧贴屏幕下方
                    //
                    Rect.Y = 0;
            }

            //
            // 绘制对话框动画
            //
            var dialogPack = DrawDialogAnimation(atlas);

            //
            // 设置当前文字的信息
            //
            currentRowInfo.Text = $"{actualText[j++]}";
            TextDrawInfos.Add(currentRowInfo.Clone());
            foreach (var info in TextDrawInfos) info.ParentPack = dialogPack;
            currentRowInfo.PosOffset.X += S.GetTextActualSize(currentRowInfo.Text, currentRowInfo.FontSize).W;

            //
            // 绘制全部文本
            //
            PalText.DrawTexts(atlas, [.. TextDrawInfos]);

            if (!ForceDrawOnMainScreen)
                //
                // 恢复画面
                //
                PalScreen.Restore(PalScreen.Main);

            //
            // 更新画面
            //
            atlas.DrawGeometry();
            atlas = new(atlas);
            if (!ForceDrawOnMainScreen) PalScreen.Update();
        }

        //
        // 换行，恢复字体颜色为默认的白色
        //
        CurrentRowId++;
        //CharColor = ColorWhite;
    }
}
