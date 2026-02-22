using Records.Mod;
using Records.Mod.RGame;
using Records.Pal;
using SDL3;
using System.Collections.Generic;
using System.IO;

namespace SDLPal;

public static unsafe class PalUiGame
{
    public const int
        MenuExitVoice       = 510,
        MaxItemMenuRowCount = 7,
        ItemMenuScrollBarY  = 18;
    public const float StretchFactor = (PalVideo.Height / 480f) > 1 ? PalVideo.Height / 480f : 1;

    static Dictionary<string, nint[]> UiDict { get; set; } = [];

    public enum CommandAnimationType : sbyte
    {
        CenterButton        = 0,        // 中间按钮陷入
        OtherButton         = 1,        // 外围按钮向外打开
        NextGroupButton     = 2,        // 下一组按钮上升
    }

    /// <summary>
    /// 初始化 Ui 模块
    /// </summary>
    public static void Init()
    {
        //
        // 初始化所有 Ui
        //
        foreach (var directory in Directory.GetDirectories(S.ModUiPath))
        {
            var name = Path.GetFileName(directory);
            UiDict[name] = LoadMenuUi($@"{S.ModUiPath}\{name}");
        }

        //
        // 设置全局对话框边框厚度
        //
        PalDialog.Padding = GetUiSize("DialogBlack", 0, StretchFactor).W;
    }

    /// <summary>
    /// 销毁 Ui 模块
    /// </summary>
    public static void Free()
    {
        //
        // 销毁所有 Ui
        //
        foreach (var ui in UiDict) FreeMenuUi(ui.Value);

        //
        // 清空 Ui 池
        //
        UiDict.Clear();
    }

    /// <summary>
    /// 加载 UI
    /// </summary>
    /// <param name="path">UI 路径</param>
    /// <returns>贴图列表</returns>
    public static nint[] LoadMenuUi(string path)
    {
        //
        // 统计图像数
        //
        var i = 0;
        for (; ; i++) if (!File.Exists($@"{path}\{i:D5}.png")) break;

        //
        // 将所有图像加载为纹理
        //
        var surfaces = new nint[i];
        for (i = 0; i < surfaces.Length; i++)
        {
            var imgPath = $@"{path}\{i:D5}.png";
            if (!File.Exists(imgPath))
                //
                // 图像序列中断，结束读取
                //
                break;

            surfaces[i] = COS.Surface(imgPath);
        }

        return surfaces;
    }

    /// <summary>
    /// 销毁 UI
    /// </summary>
    /// <param name="surfaces">贴图列表</param>
    public static void FreeMenuUi(nint[] surfaces)
    {
        foreach (var texture in surfaces)
            FOS.Texture(texture);
    }

    /// <summary>
    /// 获取 UI
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <returns>贴图列表</returns>
    public static nint[] GetUi(string uiName)
    {
        S.Failed(
            "PalUiGame.GetUi",
            $"UI '{uiName}' is undefined",
            UiDict.TryGetValue(uiName, out var ui)
        );

        return ui!;
    }

    /// <summary>
    /// 获取 UI 原生的尺寸
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <param name="surfaceId">贴图编号</param>
    public static (int W, int H) GetUiOriginSize(nint[] ui, int surfaceId = 0)
    {
        S.GetSurfaceSize(ui[surfaceId], out var w, out var h);

        return (w, h);
    }

    /// <summary>
    /// 获取 UI 原生的尺寸
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <param name="surfaceId">贴图编号</param>
    public static (int W, int H) GetUiOriginSize(string uiName, int surfaceId = 0) => GetUiOriginSize(GetUi(uiName), surfaceId: surfaceId);

    /// <summary>
    /// 获取 UI 拉伸后的尺寸
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <param name="surfaceId">贴图编号</param>
    public static (int W, int H) GetUiSize(nint[] ui, int surfaceId = 0, float stretchFactor = StretchFactor)
    {
        var (w, h) = GetUiOriginSize(ui, surfaceId);

        return ((int)(w * stretchFactor), (int)(h * stretchFactor));
    }

    /// <summary>
    /// 获取 UI 拉伸后的尺寸
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <param name="surfaceId">贴图编号</param>
    public static (int W, int H) GetUiSize(string uiName, int surfaceId = 0, float stretchFactor = StretchFactor) =>
        GetUiSize(GetUi(uiName), surfaceId: surfaceId, stretchFactor: stretchFactor);

    /// <summary>
    /// 绘制边框
    /// </summary>
    /// <param name="rect">区域，包括边框</param>
    public static AtlasPack DrawBox(Atlas atlas, DialogPack pack)
    {
        //
        // 获取对话框 UI
        //
        var ui = PalDialog.GetBoxUi(pack.BackgroundColor);

        //
        // 初始化对话框数据
        //
        var borderPack = new AtlasPack()
        {
            ColorMask = new(0.5f, 0.5f, 0.5f, 0.5f),
        };
        var content = borderPack.Clone();
        borderPack.ParentPack = content;
        var topLeft = borderPack.Clone();
        var topMiddle = borderPack.Clone();
        var topRight = borderPack.Clone();
        var middleLeft = borderPack.Clone();
        var middleRight = borderPack.Clone();
        var bottomLeft = borderPack.Clone();
        var bottomMiddle = borderPack.Clone();
        var bottomRight = borderPack.Clone();

        //
        // 绘制中心块
        //
        {
            var rect = pack.Rect;
            content.Rect = new()
            {
                X = rect.X + pack.HorizontalAlign switch
                {
                    PalHorizontalAlign.Left => GetUiSize(ui, 3, PalDialog.StretchFactor).W,
                    PalHorizontalAlign.Right => -GetUiSize(ui, 5, PalDialog.StretchFactor).W,
                    PalHorizontalAlign.Middle or _ => 0,
                },
                Y = rect.Y + pack.VerticalAlign switch
                {
                    PalVerticalAlign.Top => GetUiSize(ui, 1, PalDialog.StretchFactor).H,
                    PalVerticalAlign.Bottom => -GetUiSize(ui, 7, PalDialog.StretchFactor).H,
                    PalVerticalAlign.Middle or _ => 0,
                },
                W = rect.W,
                H = rect.H,
            };
            content.HorizontalAlign = pack.HorizontalAlign;
            content.VerticalAlign = pack.VerticalAlign;
            atlas.Add(new(ui[4]), content);
        }

        //
        // 绘制上边框
        //
        var contentW = content.Rect.W;
        var contentH = content.Rect.H;
        var w = 0;
        var h = 0;
        {
            (w, h) = GetUiSize(ui, 0, PalDialog.StretchFactor);
            topLeft.Rect = new()
            {
                W = w,
                H = h,
                X = -w,
                Y = -h,
            };

            (_, h) = GetUiSize(ui, 1, PalDialog.StretchFactor);
            topMiddle.Rect = new()
            {
                W = contentW,
                H = h,
                Y = -h,
            };
            topMiddle.HorizontalAlign = PalHorizontalAlign.Middle;

            (w, h) = GetUiSize(ui, 2, PalDialog.StretchFactor);
            topRight.Rect = new()
            {
                W = w,
                H = h,
                X = w,
                Y = -h,
            };
            topRight.HorizontalAlign = PalHorizontalAlign.Right;

            atlas.Add(new(ui[0]), topLeft);
            atlas.Add(new(ui[1]), topMiddle);
            atlas.Add(new(ui[2]), topRight);
        }

        //
        // 绘制左右边框
        //
        {
            (w, _) = GetUiSize(ui, 3, PalDialog.StretchFactor);
            middleLeft.Rect = new()
            {
                W = w,
                H = contentH,
                X = -w,
            };
            middleLeft.VerticalAlign = PalVerticalAlign.Middle;

            (w, _) = GetUiSize(ui, 5, PalDialog.StretchFactor);
            middleRight.Rect = new()
            {
                W = w,
                H = contentH,
                X = w,
            };
            middleRight.VerticalAlign = PalVerticalAlign.Middle;
            middleRight.HorizontalAlign = PalHorizontalAlign.Right;

            atlas.Add(new(ui[3]), middleLeft);
            atlas.Add(new(ui[5]), middleRight);
        }

        //
        // 绘制下边框
        //
        {
            (w, h) = GetUiSize(ui, 6, PalDialog.StretchFactor);
            bottomLeft.Rect = new()
            {
                W = w,
                H = h,
                X = -w,
                Y = h,
            };
            bottomLeft.VerticalAlign = PalVerticalAlign.Bottom;

            (_, h) = GetUiSize(ui, 7, PalDialog.StretchFactor);
            bottomMiddle.Rect = new()
            {
                W = contentW,
                H = h,
                Y = h,
            };
            bottomMiddle.VerticalAlign = PalVerticalAlign.Bottom;
            bottomMiddle.HorizontalAlign = PalHorizontalAlign.Middle;

            (w, h) = GetUiSize(ui, 8, PalDialog.StretchFactor);
            bottomRight.Rect = new()
            {
                W = w,
                H = h,
                X = w,
                Y = h,
            };
            bottomRight.VerticalAlign = PalVerticalAlign.Bottom;
            bottomRight.HorizontalAlign = PalHorizontalAlign.Right;

            atlas.Add(new(ui[6]), bottomLeft);
            atlas.Add(new(ui[7]), bottomMiddle);
            atlas.Add(new(ui[8]), bottomRight);
        }

        return content;
    }

    /// <summary>
    /// 标题画面菜单
    /// </summary>
    /// <returns>用户选择了哪个存档</returns>
    public static int TitleMenu()
    {
        var saveId = -1;

        //
        // 播放进入音效
        //
        PalAudio.PlayVoice(505);

        //
        // 获取标题画面 UI
        //
        var ui = GetUi("TitleScreen");

        //
        // 初始化菜单
        //
        var stretchFactor = StretchFactor / 1.5f;
        var (_, h) = GetUiSize(ui, 0, stretchFactor);
        var uiMenu = new UiMenu(
            surfaceGroups: [[ui[3], ui[2], ui[2]], [ui[5], ui[4], ui[4]], [ui[7], ui[6], ui[6]]],
            atlasPack: new()
            {
                Rect = new()
                {
                    X = 0,
                    Y = h / 2,
                },
                StretchFactor = stretchFactor,
                HorizontalAlign = PalHorizontalAlign.Middle,
                VerticalAlign = PalVerticalAlign.Middle,
            },
            texts: ["新的冒险", "旧的回忆", "离开游戏"],
            rowOffset: (int)(h * 1.5f),
            rememberCursor: true
        );
        uiMenu.Options[0].PressVoice = 506;
        uiMenu.Options[1].PressVoice = 509;
        uiMenu.Options[2].PressVoice = 509;
        uiMenu.Options[2].SubMenu = new(
            surfaceGroups: [[ui[0], ui[0], ui[0]], [ui[1], ui[1], ui[1]]],
            atlasPack: new()
            {
                Rect = uiMenu.Options[2].AtlasPack.Rect,
                StretchFactor = stretchFactor,
                HorizontalAlign = PalHorizontalAlign.Middle,
                VerticalAlign = PalVerticalAlign.Middle,
            },
            texts: ["　　　 否", "是 　　　"],
            columnCount: 2
        );

        //
        // 加载背景视频
        //
        PalGlobal.MoviePlayer = new($@"{S.ModPath.Assets.Movie}\MenuLoop.mp4");

        //
        // 进菜单前清理按键状态
        //
        PalInput.ClearKeyState();

        //
        // 进入菜单循环
        //
        var firstEntering = true;
        var movieDelay = 0ul;
        while (true)
        {
            //
            // 获取场景主 Atlas
            //
            ref var atlas = ref PalAtlas.Scene;
            atlas = new(atlas);

            //
            // 延迟相应的毫秒数，控制帧率
            //
            PalTimer.Delay(1);

            if (SDL.GetTicks() >= movieDelay)
            {
                //
                // 获取视频帧
                //
                do
                {
                    movieDelay = PalGlobal.MoviePlayer.DrawFrame();
                    if (movieDelay == MoviePlayer.ErrorCode)
                        PalGlobal.MoviePlayer.ReLoad();
                } while (movieDelay >= MoviePlayer.ErrorCode - 1);
            }

            //
            // 将视频帧绘制到屏幕
            //
            atlas.Add(new(PalGlobal.MoviePlayer.MovieSurface), new()
            {
                HorizontalAlign = PalHorizontalAlign.Stretch,
                VerticalAlign = PalVerticalAlign.Stretch,
            });

            //
            // 绘制画卷上镶边
            //
            atlas.Add(new(ui[8]), new()
            {
                StretchFactor = stretchFactor,
                HorizontalAlign = PalHorizontalAlign.Stretch,
            });

            //
            // 绘制画卷下镶边
            //
            atlas.Add(new(ui[8]), new()
            {
                StretchFactor = stretchFactor,
                HorizontalAlign = PalHorizontalAlign.Stretch,
                VerticalAlign = PalVerticalAlign.Bottom,
            });

            //
            // 绘制菜单选项
            //
            var results = uiMenu.Draw(atlas);

            //
            // 绘制游戏 LOGO
            //
            (var w, h) = GetUiOriginSize(ui, 10);
            atlas.Add(new(ui[10]), new()
            {
                Rect = new()
                {
                    X = 0,
                    Y = -200,
                },
                StretchFactor = stretchFactor,
                HorizontalAlign = PalHorizontalAlign.Middle,
                VerticalAlign = PalVerticalAlign.Middle,
            });

            //
            // 绘制作者声明
            //
            PalText.DrawTexts(atlas, [
                new()
                {
                    Text = "免费同人游戏，严禁用于商业用途",
                    FontSize = PalText.LargeSize,
                    Foreground = ColorGold,
                    HorizontalAlign = PalHorizontalAlign.Middle,
                },
                new()
                {
                    Text = "原作为 Bilibili UP 主　　　　　　　　　　的动画　　　　　　　",
                    FontSize = PalText.LargeSize,
                    PosOffset = new(0, PalText.DefaultTextOffset),
                    Foreground = ColorCyan,
                    HorizontalAlign = PalHorizontalAlign.Middle,
                },
                new()
                {
                    Text = "艾云的黄钻天空",
                    FontSize = PalText.LargestSize,
                    PosOffset = new(0, PalText.DefaultTextOffset),
                    Foreground = ColorRed,
                    HorizontalAlign = PalHorizontalAlign.Middle,
                },
                new()
                {
                    Text = "　　　　　　　　　　　　　　　　《青莲劫》",
                    FontSize = PalText.LargestSize,
                    PosOffset = new(0, PalText.DefaultTextOffset),
                    Foreground = ColorYellow,
                    HorizontalAlign = PalHorizontalAlign.Middle,
                },
                new()
                {
                    Text = "大量代码借自@SDLPAL@，作者：李狗蛋儿",
                    FontSize = PalText.LargeSize,
                    Foreground = ColorGreen,
                    HorizontalAlign = PalHorizontalAlign.Middle,
                    VerticalAlign = PalVerticalAlign.Bottom,
                },
            ]);

            //
            // 绘制 UI
            //
            atlas.DrawGeometry();

            //
            // 更新屏幕画面
            //
            if (firstEntering)
            {
                //
                // 首次进入菜单，淡出
                //
                firstEntering = false;
                PalScreen.Fade(2, false);
            }
            else
                PalScreen.Update();

            //
            // 检查选项路径
            //
            if (results != null && results.Count != 0)
                switch (results[^1])
                {
                    case 0:
                        //
                        // 用户选择重新开始游戏
                        //
                        saveId = 0;
                        goto EndOpeningMenu;

                    case 1:
                        //
                        // 用户选择继续游戏，这将读取游戏存档
                        //
                        saveId = 0;
                        goto EndOpeningMenu;

                    case 2:
                        //
                        // 用户选择退出了游戏
                        //
                        switch (results[^2])
                        {
                            case 0:
                                //
                                // 否
                                //
                                uiMenu.RecursiveSubMenu();
                                break;

                            case 1:
                                //
                                // 是
                                //
                                PalMain.Free();
                                break;
                        }
                        break;
                }
        }

    EndOpeningMenu:
        //
        // 淡出屏幕和音乐
        //
        PalAudio.StopMusic();
        PalScreen.Fade(1);

        return saveId;
    }

    /// <summary>
    /// 绘制菜单背景
    /// </summary>
    /// <param name="atlas">图集</param>
    public static void MainMenuBackground(Atlas atlas, bool needDrawShadow = false)
    {
        if (needDrawShadow)
        {
            //
            // 生成灰色半透明遮罩
            //
            var surface = COS.Surface(10, 10, PalVideo.DefaultFormat);
            SDL.FillSurfaceRect(surface, 0, 0x00_00_00_BF);

            //
            // 绘制遮罩
            //
            atlas.Add(new(surface, needFree: true), new()
            {
                HorizontalAlign = PalHorizontalAlign.Stretch,
                VerticalAlign = PalVerticalAlign.Stretch,
            });
        }

        //
        // 绘制主菜单背景
        //
        atlas.Add(new(GetUi("MainMenuBackground")[0]), new()
        {
            StretchFactor = StretchFactor,
            HorizontalAlign = PalHorizontalAlign.Middle,
            VerticalAlign = PalVerticalAlign.Middle,
        });

        //
        // 绘制现有金钱
        //
        MoneyBox(atlas);
    }

    /// <summary>
    /// 命令按钮菜单
    /// </summary>
    /// <param name="atlas">图集</param>
    /// <param name="frameId">动画帧编号</param>
    /// <param name="isReverse">是否倒放</param>
    /// <returns>动画是否播放完毕</returns>
    public static bool CommandMenu(Atlas atlas, CommandAnimationType animationType = CommandAnimationType.CenterButton, int frameId = 0, bool isReverse = false)
    {
        //
        // 播放结果，动画是否播放完成
        //
        var result = false;

        //
        // 获取命令按钮 UI
        //
        var ui = GetUi("CommandMenu");
        var background = GetUi("MainMenuBackground");

        //
        // 获取 UI 尺寸
        //
        var (boxWidth, boxHeight) = GetUiOriginSize(ui);
        var (borderWidth, borderHeight) = GetUiOriginSize(ui, 2);
        var (centerButtonWidth, centerButtonHeight) = GetUiOriginSize(ui, 3);
        var (otherButtonWidth, otherButtonHeight) = GetUiOriginSize(ui, 6);

        //
        // 加载图像
        //
        var box = COS.Texture(ui[0], lockedMod: 2, free: false);
        var nextGroupButton = COS.Texture(ui[1], free: false);
        var border = COS.Texture(ui[2], free: false);
        var center = COS.Texture(ui[3], free: false);
        var bottom = COS.Texture(ui[6], free: false);
        var left = COS.Texture(ui[9], free: false);
        var top = COS.Texture(ui[12], free: false);
        var right = COS.Texture(ui[15], free: false);

        //
        // 根据动画类型控制变量
        //
        var DefalutStretchFactor = 0f;
        var nextGroupButtonStretchFactor = 0f;
        var centerButtonStretchFactor = 0f;
        var offset = 3;
        if (isReverse)
        {
            DefalutStretchFactor = 0.7f;
            nextGroupButtonStretchFactor = DefalutStretchFactor;
            centerButtonStretchFactor = DefalutStretchFactor;
            offset = 3 + otherButtonHeight;
            switch (animationType)
            {
                case CommandAnimationType.CenterButton:
                    offset -= otherButtonHeight;
                    centerButtonStretchFactor = float.Min(1, (nextGroupButtonStretchFactor + frameId / 50f)); ;
                    result = (centerButtonStretchFactor == 1);
                    break;

                case CommandAnimationType.OtherButton:
                    frameId *= 2;
                    offset -= frameId;
                    result = (frameId >= otherButtonHeight);

                    FOS.Texture(center);
                    center = COS.Texture(ui[4], free: false);
                    break;

                case CommandAnimationType.NextGroupButton:
                    offset += otherButtonHeight;
                    nextGroupButtonStretchFactor = float.Max(DefalutStretchFactor, (1 - frameId / 50f));
                    result = ((centerButtonStretchFactor = nextGroupButtonStretchFactor) == DefalutStretchFactor);
                    centerButtonStretchFactor = float.Max(DefalutStretchFactor, (1 - frameId / 50f));
                    break;
            }
        }
        else
        {
            DefalutStretchFactor = 0.7f;
            nextGroupButtonStretchFactor = DefalutStretchFactor;
            centerButtonStretchFactor = DefalutStretchFactor;
            offset = 3;
            switch (animationType)
            {
                case CommandAnimationType.CenterButton:
                    centerButtonStretchFactor = float.Max(DefalutStretchFactor, (1 - frameId / 50f));
                    result = (centerButtonStretchFactor == DefalutStretchFactor);
                    break;

                case CommandAnimationType.OtherButton:
                    frameId *= 2;
                    offset += frameId;
                    result = (frameId >= otherButtonHeight);

                    FOS.Texture(center);
                    center = COS.Texture(ui[4], free: false);
                    break;

                case CommandAnimationType.NextGroupButton:
                    offset += otherButtonHeight;
                    nextGroupButtonStretchFactor = float.Min(1, (nextGroupButtonStretchFactor + frameId / 50f));
                    result = ((centerButtonStretchFactor = nextGroupButtonStretchFactor) == 1);
                    centerButtonStretchFactor = float.Min(1, centerButtonStretchFactor + frameId / 50f);
                    break;
            }
        }

        //
        // 合成图像
        //
        PalScreen.SetBlendMode(PalScreen.BlendMode.Mask);
        {
            var w = boxWidth * nextGroupButtonStretchFactor;
            var h = boxHeight * nextGroupButtonStretchFactor;
            PalScreen.Copy(nextGroupButton, box, new SDL.FRect()
            {
                X = (boxWidth - w) / 2,
                Y = (boxHeight - h) / 2,
                W = w,
                H = h,
            });
            FOS.Texture(nextGroupButton);
            w = otherButtonWidth;
            h = otherButtonHeight;
            PalScreen.Copy(bottom, box, new SDL.FRect()
            {
                X = (boxWidth - w) / 2,
                Y = (boxHeight + centerButtonHeight) / 2 + offset,
                W = w,
                H = h,
            });
            FOS.Texture(bottom);
            PalScreen.Copy(top, box, new SDL.FRect()
            {
                X = (boxWidth - w) / 2,
                Y = (boxHeight - centerButtonHeight) / 2 - h - offset + 1,
                W = w,
                H = h,
            });
            FOS.Texture(top);
            w = otherButtonHeight;
            h = otherButtonWidth;
            PalScreen.Copy(left, box, new SDL.FRect()
            {
                X = (boxWidth - centerButtonWidth) / 2 - w - offset + 1,
                Y = (boxHeight - h) / 2,
                W = w,
                H = h,
            });
            FOS.Texture(left);
            PalScreen.Copy(right, box, new SDL.FRect()
            {
                X = (boxWidth + centerButtonWidth) / 2 + offset - 1,
                Y = (boxHeight - h) / 2,
                W = w,
                H = h,
            });
            FOS.Texture(right);
            w = centerButtonWidth * centerButtonStretchFactor;
            h = centerButtonWidth * centerButtonStretchFactor;
            PalScreen.Copy(center, box, new SDL.FRect()
            {
                X = (boxWidth - w) / 2,
                Y = (boxHeight - h) / 2,
                W = w,
                H = h,
            });
            FOS.Texture(center);
            PalScreen.Copy(border, box, new SDL.FRect()
            {
                X = (boxWidth - borderWidth) / 2,
                Y = (boxHeight - borderHeight) / 2,
                W = borderWidth,
                H = borderHeight,
            });
            FOS.Texture(border);
        }
        PalScreen.SetBlendMode(PalScreen.BlendMode.Default);
        //PalScreen.SaveScreenshot(box, $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\123.png");

        //
        // 计算 UI 放大后的尺寸
        //
        var (backgroundWidth, backgroundHeight) = GetUiSize(background);
        (boxWidth, boxHeight) = GetUiSize(ui);
        //backgroundWidth = (int)(backgroundWidth * StretchFactor);
        //backgroundHeight = (int)(backgroundHeight * StretchFactor);
        //boxWidth = (int)(boxWidth * StretchFactor);
        //boxHeight = (int)(boxHeight * StretchFactor);

        //
        // 圆盘边框
        //
        atlas.Add(new(S.GetTexPixels(box), needFree: true), new()
        {
            Rect = new()
            {
                X = -((backgroundWidth - boxWidth) / 2),
                Y = (backgroundHeight - boxHeight) / 2,
            },
            StretchFactor = StretchFactor,
            HorizontalAlign = PalHorizontalAlign.Middle,
            VerticalAlign = PalVerticalAlign.Middle,
        });
        FOS.Texture(box);

        return result;
    }

    /// <summary>
    /// 播放命令按钮菜单动画
    /// </summary>
    /// <param name="atlas">图集</param>
    /// <param name="isReverse">是否倒放</param>
    /// <returns>动画是否播放完毕</returns>
    public static bool CommandMenuAnimation(ref Atlas atlas, ref CommandAnimationFramePack pack, bool isReverse = false)
    {
        var result = false;
        var time = SDL.GetTicks();

        //
        // 帧延迟 10ms
        //
        if (time < pack.LastTick + 10) goto Return;

        pack.LastTick = time;

        if (!pack.IsPlaying)
        {
            pack.IsPlaying = true;

            //
            // 关闭/开启一层圆盘界面
            //
            pack.AnimationType = isReverse ? CommandAnimationType.NextGroupButton : CommandAnimationType.CenterButton;

            //
            // 播放关闭/开启音效
            //
            PalAudio.PlayVoice(isReverse ? 512 : 511);
        }

        //
        // 播放命令按钮打开动画
        //
        if (CommandMenu(atlas, pack.AnimationType, pack.FrameId++, isReverse))
        {
            //
            // 播放完毕，切换到下一个动画
            //
            if (isReverse)
            {
                pack.AnimationType--;

                if (pack.AnimationType < CommandAnimationType.CenterButton)
                    //
                    // 所有动画全部播放完毕
                    //
                    result = true;
            }
            else
            {
                pack.AnimationType++;

                if (pack.AnimationType > CommandAnimationType.NextGroupButton)
                    //
                    // 所有动画全部播放完毕
                    //
                    result = true;
            }

            if (result)
                //
                // 重新初始化包状态，以便复用
                //
                pack = new();
            else
                //
                // 帧编号回到第一帧
                //
                pack.FrameId = 0;
        }

        //
        // 绘制最终画面
        //
        atlas.DrawGeometry();
        atlas = new(atlas);
        PalScreen.Update();

    Return:
        //
        // 正常播放完成，返回是否播放完成
        //
        return result;
    }

    /// <summary>
    /// 一键生成轮盘菜单
    /// </summary>
    /// <returns></returns>
    public static UiMenu GenerateCommandMenu(GenerateCommandMenuPack[] packs)
    {
        const int optionCount = 5;

        //
        // 获取场景主菜单 UI
        //
        var ui = GetUi("CommandMenu");

        //
        // 获取 UI 尺寸
        //
        var (backgroundWidth, backgroundHeight) = GetUiSize(GetUi("MainMenuBackground"));
        var (boxWidth, boxHeight) = GetUiSize(ui);
        var (borderWidth, borderHeight) = GetUiSize(ui, 2);
        var (centerButtonWidth, centerButtonHeight) = GetUiSize(ui, 3);
        var (otherButtonWidth, otherButtonHeight) = GetUiSize(ui, 6);

        var pos = new Pos(
            X: -((backgroundWidth - boxWidth) / 2),
            Y: (backgroundHeight - boxHeight) / 2
        );

        var surfaces = (nint[][])[[ui[12], ui[14], ui[13]], [ui[9], ui[11], ui[10]], [ui[3], ui[5], ui[4]], [ui[15], ui[17], ui[16]], [ui[6], ui[8], ui[7]]];
        var rects = (SDL.Rect[])[
            new()       // 上
            {
                X = pos.X,
                Y = pos.Y - centerButtonHeight,
            },
            new()       // 左
            {
                X = pos.X - centerButtonWidth,
                Y = pos.Y,
            },
            new()       // 中
            {
                X = pos.X,
                Y = pos.Y,
            },
            new()       // 右
            {
                X = pos.X + centerButtonWidth,
                Y = pos.Y,
            },
            new()       // 下
            {
                X = pos.X,
                Y = pos.Y + centerButtonHeight,
            },
        ];

        //
        // 检查实际需要使用的贴图和对应 rect
        //
        var actualRects = new SDL.Rect[optionCount];
        var actualSurfaces = new nint[optionCount][];
        var actualTexts = new string[optionCount];
        for (int i = 0; i < optionCount; i++)
        {
            actualRects[i] = rects[i];
            actualSurfaces[i] = surfaces[i];
            actualTexts[i] = packs[i].Text;
        }

        //
        // 创建菜单
        //
        var menu = new UiMenu(
            surfaceGroups: actualSurfaces,
            atlasPack: new()
            {
                StretchFactor = StretchFactor,
                HorizontalAlign = PalHorizontalAlign.Middle,
                VerticalAlign = PalVerticalAlign.Middle,
            },
            rects: actualRects,
            texts: actualTexts,
            rememberCursor: true,
            @delegate: CommandMenuCursorMove
        )
        {
            //
            // 设置光标移动音效
            //
            CursorMoveVoice = 513,
        };

        //
        // 设置选项按下音效、是否被禁用、隐藏
        //
        for (int i = 0; i < optionCount; i++)
        {
            menu.Options[i].PressVoice = 514;
            menu.Options[i].Enabled = packs[i].Enabled;
            menu.Options[i].Hidden = packs[i].Hidden;
        }

        return menu;
    }

    /// <summary>
    /// 检查命令按钮菜单光标是否移动
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    public static int CommandMenuCursorMove(in UiMenu menu)
    {
        var activityOptionId = menu.ActiveOptionId;
        var isMoveFailed = false;
        var minOptionId = 0;
        var maxOptionId = menu.Options.Length - 1;

        if (menu.CanVerticalMove && PalInput.Pressed(PalKey.Down))
            //
            // 选项光标向下移动
            //
            isMoveFailed = ((activityOptionId = int.Min(((activityOptionId + 2) / 3 + 1) * 2, maxOptionId)) == menu.ActiveOptionId);
        else if (menu.CanHorizontalMove && PalInput.Pressed(PalKey.Left))
        {
            if (activityOptionId > 3) activityOptionId = minOptionId;

            //
            // 选项光标向左移动
            //
            isMoveFailed = ((activityOptionId = int.Max(--activityOptionId, 1)) == menu.ActiveOptionId);
        }
        else if (menu.CanVerticalMove && PalInput.Pressed(PalKey.Up))
            //
            // 选项光标向上移动
            //
            isMoveFailed = ((activityOptionId = int.Max(((activityOptionId - 2) / 3 - 1) * 2, minOptionId)) == menu.ActiveOptionId);
        else if (menu.CanHorizontalMove && PalInput.Pressed(PalKey.Right))
        {
            if (activityOptionId < 1) activityOptionId = maxOptionId;

            //
            // 选项光标向右移动
            //
            isMoveFailed = ((activityOptionId = int.Min(++activityOptionId, 3)) == menu.ActiveOptionId);
        }

        if (isMoveFailed)
        {
            //
            // 播放光标越界音效
            //

        }

        return activityOptionId;
    }

    public static void MoneyBox(Atlas atlas)
    {
        //
        // 获取 UI
        //
        var ui = GetUi("MoneyBox");
        var background = GetUi("MainMenuBackground");

        //
        // 获取 UI 尺寸
        //
        var (uiWidth, uiHeight) = GetUiSize(ui);
        var (backgroundWidth, backgroundHeight) = GetUiSize(background);

        //
        // 显示铜钱图像
        //
        var padding = (int)(15 * StretchFactor);
        var pack = new AtlasPack()
        {
            Rect = new()
            {
                X = -((backgroundWidth - uiWidth) / 2) + padding,
                Y = -((backgroundHeight - uiHeight) / 2) + padding,
            },
            StretchFactor = StretchFactor,
            HorizontalAlign = PalHorizontalAlign.Middle,
            VerticalAlign = PalVerticalAlign.Middle,
        };
        atlas.Add(new(ui[0]), pack);

        //
        // 显示现有金钱
        //
        PalText.DrawText(atlas, new()
        {
            Text = $"{S.Save.Money:N0}",
            PosOffset = new(
                X: uiWidth + padding,
                Y: 0
            ),
            VerticalAlign = PalVerticalAlign.Middle,
            ParentPack = pack,
        });
    }

    /// <summary>
    /// 场景主菜单
    /// </summary>
    public static void SceneMainMenu()
    {
        //
        // 清理场景贴图缓存并显示
        //
        ref var atlas = ref PalAtlas.Scene;
        atlas = new(atlas);

        //
        // 绘制菜单背景
        //
        MainMenuBackground(atlas, true);

        //
        // 播放轮盘打开新界面动画
        //
        var pack = new CommandAnimationFramePack();
        while (!CommandMenuAnimation(ref atlas, ref pack, isReverse: false)) ;

        //
        // 生成菜单
        //
        var uiMenu = GenerateCommandMenu([
            new()
            {
                Text = "状态",
            },
            new()
            {
                Text = "仙术",
            },
            new()
            {
                Text = "特殊",
            },
            new()
            {
                Text = "道具",
            },
            new()
            {
                Text = "系统",
            },
        ]);

        //
        // 清理上一帧的输入
        //
        PalInput.ClearKeyState();

        var results = (List<int>)null!;
        while (true)
        {
            //
            // 延迟 10ms
            //
            PalTimer.Delay(10);

            //
            // 接受一帧输入
            //
            PalInput.ProcessEvent();
            if (PalInput.Pressed(PalKey.Menu))
            {
                //
                // 退出菜单，播放轮盘关闭动画
                //
                while (!CommandMenuAnimation(ref atlas, ref pack, isReverse: true)) ;
                return;
            }
            else if (results != null && results.Count != 0)
            {
                //
                // 检查用户选择了哪个选项
                //
                switch (results[^1])
                {
                    case 0:
                        //
                        // 状态
                        //
                        break;

                    case 1:
                        //
                        // 仙术
                        //
                        break;

                    case 2:
                        //
                        // 特殊
                        //
                        break;

                    case 3:
                        //
                        // 道具
                        //
                        if (CommandItemMenu(ref atlas)) return;
                        break;

                    case 4:
                        //
                        // 系统
                        //
                        break;
                }

                //
                // 绘制菜单背景
                //
                MainMenuBackground(atlas);

                //
                // 清理上一帧的输入
                //
                PalInput.ClearKeyState();
            }

            //
            // 显示命令按钮
            //
            CommandMenu(atlas);

            //
            // 显示选项
            //
            results = uiMenu.Draw(atlas);

            //
            // 绘制最终画面
            //
            atlas.DrawGeometry();
            atlas = new(atlas);
            PalScreen.Update();
        }
    }

    /// <summary>
    /// 圆盘命令道具菜单
    /// </summary>
    /// <param name="atlas">图集</param>
    /// <param name="needBackupVideo">需要备份画面</param>
    public static bool CommandItemMenu(ref Atlas atlas, bool needBackupVideo = true)
    {
        //
        // 播放轮盘打开新界面动画
        //
        var pack = new CommandAnimationFramePack();
        while (!CommandMenuAnimation(ref atlas, ref pack, isReverse: false)) ;

        if (needBackupVideo)
            //
            // 备份原来的画面
            //
            PalScreen.Backup(PalScreen.Main);

        //
        // 生成菜单
        //
        var enable = S.Save.Inventories.Count > 0;
        var uiMenu = GenerateCommandMenu([
            new(Hidden: true),
            new(Hidden: true),
            new()
            {
                Text = "使用",
                Enabled = enable,
            },
            new()
            {
                Text = "装备",
                Enabled = enable,
            },
            new(Hidden: true),
        ]);

        //
        // 清理上一帧的输入
        //
        PalInput.ClearKeyState();

        var results = (List<int>)null!;
        var needExit = false;
        while (!needExit)
        {
            //
            // 延迟 10ms
            //
            PalTimer.Delay(10);

            //
            // 接受一帧输入
            //
            PalInput.ProcessEvent();
            if (PalInput.Pressed(PalKey.Menu))
                //
                // 用户退出菜单
                //
                break;
            else if (results != null && results.Count != 0)
            {
                //
                // 检查用户选择了哪个选项
                //
                switch (results[^1])
                {
                    case 2:
                        //
                        // 使用
                        //
                        if (needExit = UseItemMenu(ref atlas)) goto Exit;
                        break;

                    case 3:
                        //
                        // 装备
                        //
                        break;
                }

                if (needBackupVideo)
                    //
                    // 恢复被污染的背景
                    //
                    PalScreen.Restore(PalScreen.Main);

                //
                // 清理上一帧的输入
                //
                PalInput.ClearKeyState();
            }

            //
            // 显示命令按钮
            //
            CommandMenu(atlas);

            //
            // 显示选项
            //
            results = uiMenu.Draw(atlas);

            //
            // 绘制最终画面
            //
            atlas.DrawGeometry();
            atlas = new(atlas);
            PalScreen.Update();
        }

    Exit:
        //
        // 退出菜单，播放轮盘关闭动画
        //
        while (!CommandMenuAnimation(ref atlas, ref pack, isReverse: true)) ;

        return needExit;
    }

    public static int GetItemMenuRowCount(int column = 3) => (int)float.Ceiling(1f * S.Save.Inventories.Count / column);

    public static int GetMagicMenuRowCount(int heroId, int column = 3)
    {
        var magics = S.Save.Entity.Heroes[heroId].Raw->Magics;

        //
        // 统计有效仙术数量
        //
        var actualMagicsCount = 0;
        for (var i = 0; i < Base.MaxHeroMagic; i++) if (magics[i] > 0) actualMagicsCount++;

        return (int)float.Ceiling(1f * actualMagicsCount / column);
    }

    /// <summary>
    /// 显示使用道具/仙术框 UI
    /// </summary>
    /// <param name="atlas">图集</param>
    /// <param name="heroId">Hero 编号，缺省则为道具菜单</param>
    public static void UseItemBackground(Atlas atlas, int heroId = 0)
    {
        //
        // 获取背景 UI
        //
        var ui = GetUi("UseItemMenu");

        //
        // 计算背景放大后的尺寸
        //
        var (_, msgBoxHeight) = GetUiSize(ui);
        var (_, listBoxHeight) = GetUiSize(ui, 1);
        var (_, listBoxOriginHeight) = GetUiOriginSize(ui, 1);
        var padding = (int)(5 * StretchFactor);
        var uiH = msgBoxHeight + listBoxHeight + padding;

        //
        // 绘制道具/仙术描述框
        //
        var msgBoxPack = new AtlasPack()
        {
            Rect = new()
            {
                Y = -(uiH - msgBoxHeight + padding) / 2,
            },
            StretchFactor = StretchFactor,
            HorizontalAlign = PalHorizontalAlign.Middle,
            VerticalAlign = PalVerticalAlign.Middle,
        };
        atlas.Add(new(ui[0]), msgBoxPack);

        //
        // 绘制道具/仙术列表框
        //
        var listBoxPack = new AtlasPack()
        {
            Rect = new()
            {
                Y = msgBoxHeight + padding,
            },
            StretchFactor = StretchFactor,
            HorizontalAlign = PalHorizontalAlign.Middle,
            ParentPack = msgBoxPack,
        };
        atlas.Add(new(ui[1]), listBoxPack);

        //
        // 计算滚动条物理高度
        //
        var rowCount = 0;
        var cursorId = 0;
        var infos = (TextDrawInfo[])null!;
        var desc = (string[])null!;
        var (surface, stretchFactor) = (nint.Zero, StretchFactor);
        var count = 0;
        if (heroId <= 0)
        {
            //
            // 是道具菜单
            //
            var save = S.Save;
            rowCount = int.Max(MaxItemMenuRowCount, GetItemMenuRowCount());
            cursorId = save.ItemCursorId;

            //
            // 获取道具描述
            //
            var entityId = save.Inventories[cursorId].ItemId;
            var item = S.Entity.Items[entityId];
            desc = item.Description;

            //
            // 获取道具图像
            //
            if (!PalSprites.GetItem(item.BitmapId, out surface)) stretchFactor *= 1.3f;
            count = save.Inventories[cursorId].Amount;
        }
        else
        {
            //
            // 是仙术菜单
            //
            rowCount = int.Max(MaxItemMenuRowCount, GetMagicMenuRowCount(heroId));
            cursorId = S.GetHeroMagicCursorId(heroId);

            //
            // 获取仙术描述
            //
            var entityId = S.Entity.Heroes[heroId].Raw->Magics[cursorId];
            desc = S.Entity.Magics[entityId].Description;

            //
            // 获取仙术系属图像
            //
            //surface = COS.Surface($@"{S.ModSpritePath.Item}\{item.BitmapId:D5}.png");
            surface = COS.Surface($@"{S.ModSpritePath.Item}\{185:D5}.png");
        }
        if (desc != null) infos = new TextDrawInfo[desc.Length];
        var currentRowId = (int)float.Ceiling(1f * (cursorId + 1) / MaxItemMenuRowCount) - 1;
        var scrollBarRatio = 1f * MaxItemMenuRowCount / rowCount;
        var scrollBarMaxH = (listBoxOriginHeight - ItemMenuScrollBarY - 18);

        //
        // 绘制滚动条
        //
        atlas.Add(new(ui[2]), new()
        {
            Rect = new()
            {
                X = -(int)(8 * StretchFactor),
                Y = (int)((ItemMenuScrollBarY + scrollBarMaxH * (1f * currentRowId / rowCount)) * StretchFactor),
                H = (int)(scrollBarMaxH * scrollBarRatio),
            },
            StretchFactor = StretchFactor,
            HorizontalAlign = PalHorizontalAlign.Right,
            ParentPack = listBoxPack,
        });

        if (desc != null)
        {
            //
            // 显示道具/仙术描述
            //
            var offsetY = 0;

            for (var i = 0; i < infos.Length; i++)
            {
                infos[i] = new()
                {
                    Text = desc[i],
                    PosOffset = new(
                        X: (int)(132 * StretchFactor),
                        Y: (int)((10 + offsetY) * StretchFactor)
                    ),
                    Foreground = ColorGold,
                    ParentPack = msgBoxPack,
                };

                offsetY += 11 + PalText.ShadowOffset;
            }

            PalText.DrawTexts(atlas, infos);
        }

        if (desc != null)
        {
            //
            // 显示道具/仙术实际效果说明
            //
            var offsetY = (infos.Length > 1) ? -7 : 0;

            for (var i = 0; i < infos.Length; i++)
            {
                infos[i] = new()
                {
                    Text = desc[i],
                    PosOffset = new(
                        X: (int)(132 * StretchFactor),
                        Y: (int)((94 + offsetY) * StretchFactor)
                    ),
                    Foreground = ColorGold,
                    ParentPack = msgBoxPack,
                };

                offsetY += 8 + PalText.ShadowOffset;
            }

            PalText.DrawTexts(atlas, infos);
        }

        //
        // 显示道具图像/仙术标识
        //
        S.GetSurfaceSize(surface, out var w, out var h);
        var spritePack = new AtlasPack()
        {
            Rect = new()
            {
                X = (int)((124 * msgBoxPack.StretchFactor - w * stretchFactor) / 2),
                Y = 0,
            },
            StretchFactor = stretchFactor,
            ParentPack = msgBoxPack,
            VerticalAlign = PalVerticalAlign.Middle,
        };
        atlas.Add(new(surface, needFree: true), spritePack);

        //
        // 显示数量
        //
        if (heroId <= 0)
            //
            // 绘制道具数量
            //
            PalText.DrawText(atlas, new()
            {
                Text = $"{count}",
                HorizontalAlign = PalHorizontalAlign.Right,
                VerticalAlign = PalVerticalAlign.Bottom,
                ParentPack = spritePack,
            });
    }

    /// <summary>
    /// 使用道具菜单
    /// </summary>
    public static bool UseItemMenu(ref Atlas atlas)
    {
        //
        // 播放界面打开音效
        //
        PalAudio.PlayVoice(509);

        //
        // 清理上一帧的输入
        //
        PalInput.ClearKeyState();

        var save = S.Save;
        var min = 0;
        var max = save.Inventories.Count - 1;
        var needExit = false;
        while (!needExit)
        {
            var cursorId = save.ItemCursorId;

            //
            // 延迟 10ms
            //
            PalTimer.Delay(10);

            //
            // 接受一帧输入
            //
            PalInput.ProcessEvent();
            if (PalInput.Pressed(PalKey.Menu))
                //
                // 退出菜单
                //
                break;
            else if (PalInput.Pressed(PalKey.Down))
                //
                // 光标向下
                //
                save.ItemCursorId = int.Min(save.ItemCursorId + MaxItemMenuRowCount, max);
            else if (PalInput.Pressed(PalKey.Left))
                //
                // 光标向左
                //
                save.ItemCursorId = int.Max(save.ItemCursorId - 1, min);
            else if (PalInput.Pressed(PalKey.Up))
                //
                // 光标向上
                //
                save.ItemCursorId = int.Max(save.ItemCursorId - MaxItemMenuRowCount, min);
            else if (PalInput.Pressed(PalKey.Right))
                //
                // 光标向右
                //
                save.ItemCursorId = int.Min(save.ItemCursorId + 1, max);
            else if (PalInput.Pressed(PalKey.Search))
            {
                //
                // 执行使用脚本
                //
                var item = S.CurrentItem;
                if (needExit = !item.Scope.NeedSelectTarget)
                {
                    //
                    // 无需选择对象，可能是剧情道具
                    // 退出菜单再执行脚本
                    //
                    PalGlobal.SceneUseItem = item;
                }
                else
                {
                    //
                    // 关闭额外场景信息显示
                    //
                    S.SetDrawSceneDebugData(false);
                    PalScene.Draw(onlyDraw: true);

                    item.Script.Use = PalScript.RunTrigger(item.Script.Use, -1, -1, $"UseItem<{item.Name}>");

                    //
                    // 道具自动消耗
                    //
                    S.AutoUseItem();
                }

                //atlas = new(atlas);
                //if (needExit) goto Exit;
            }

            //
            // 绘制道具框 UI
            //
            UseItemBackground(atlas, 0);

            //
            // 绘制菜单
            //
            PalInput.ClearKeyState();

            //
            // 显示最终画面
            //
            atlas.DrawGeometry();
            atlas = new(atlas);
            PalScreen.Update();
        }

    //Exit:
        return needExit;
    }

    /// <summary>
    /// 选择框“是”和“否”
    /// </summary>
    /// <returns>用户是否选择了“是”</returns>
    public static bool ConfirmMenu(ref Atlas atlas)
    {
        //
        // 清理图集
        //
        atlas = new(atlas);

        //
        // 获取 UI
        //
        var ui = GetUi("ConfirmMenu");

        //
        // 将背景加入图集
        //
        var backgroundPack = new AtlasPack()
        {
            Rect = new()
            {

            },
            StretchFactor = StretchFactor,
            HorizontalAlign = PalHorizontalAlign.Middle,
            VerticalAlign = PalVerticalAlign.Middle,
        };
        atlas.Add(new(ui[0]), backgroundPack);

        //
        // 将选择框标题加入图集
        //
        PalText.DrawTexts(atlas, [
            new()
            {
                Text = "我一开水果摊的，能买你生瓜蛋子儿？",
                PosOffset = new()
                {
                    Y = -(int)(30 * backgroundPack.StretchFactor),
                },
                Foreground = ColorGold,
                HorizontalAlign = PalHorizontalAlign.Middle,
                ParentPack = backgroundPack,
            },
            new()
            {
                Text = "你™故意找茬是不是？",
                PosOffset = new()
                {
                    Y = 0,
                },
                Foreground = ColorGold,
                HorizontalAlign = PalHorizontalAlign.Middle,
                ParentPack = backgroundPack,
            },
            new()
            {
                Text = "你要不要吧！要不要！",
                PosOffset = new()
                {
                    Y = (int)(30 * backgroundPack.StretchFactor),
                },
                Foreground = ColorRed,
                HorizontalAlign = PalHorizontalAlign.Middle,
                ParentPack = backgroundPack,
            },
        ]);

        //
        // 创建菜单
        //
        var menu = new UiMenu(
            surfaceGroups: [[ui[4], ui[5], ui[6]], [ui[1], ui[2], ui[3]]],
            atlasPack: new()
            {
                Rect = new()
                {
                    Y = -(int)(15 * StretchFactor),
                },
                StretchFactor = backgroundPack.StretchFactor,
                HorizontalAlign = PalHorizontalAlign.Middle,
                VerticalAlign = PalVerticalAlign.Bottom,
                ParentPack = backgroundPack,
            },
            texts: ["否", "是"],
            columnCount: 2
        );
        var pack = menu.Options[0].AtlasPack;
        var (w, _) = GetUiSize(ui, 1);
        menu.Options[0].AtlasPack.Rect.X = (int)((w + 21 * backgroundPack.StretchFactor) / 2);
        menu.Options[1].AtlasPack.Rect.X = -pack.Rect.X;

        //
        // 延迟 500ms，避免按键弹起状态未被正常重置
        //
        PalTimer.Delay(200);

        //
        // 清理上一帧的输入
        //
        PalInput.ClearKeyState();

        PalAudio.PlayVoice((S.RandomLong(0, 1) != 0) ? 1000 : 1001);

        while (true)
        {
            //
            // 延迟 10ms
            //
            PalTimer.Delay(10);

            //
            // 显示选项
            //
            var results = menu.Draw(atlas);

            //
            // 绘制最终画面
            //
            atlas.DrawGeometry();
            atlas = new(atlas);
            PalScreen.Update();

            if (results != null && results.Count != 0)
                //
                // 检查用户选择了哪个选项
                //
                return results[^1] == 1;
        }
    }

    /*
    public static UiMenu GetSaveSlotMenu()
    {
        //
        // 获取标题画面 UI
        //
        var ui = GetUi("SaveSlotMenu");

        //
        // 初始化菜单
        //
        S.GetTexSize(ui[0], out var w, out var h);
        var uiMenu = new UiMenu(
            textureGroups: [[ui[3], ui[2]], [ui[5], ui[4]], [ui[7], ui[6]]],
            pos: new Pos((Width - (int)w) / 2, Height / 2 + 25),
            rowOffset: (int)(h * 1.5f)
        );

        return uiMenu;
    }
    */
}
