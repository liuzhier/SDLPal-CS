using SDL3;
using SDLPal;
using System.Collections.Generic;
using System.Linq;

namespace Records.Mod;

/*
public class Menu
{
    public int ActivityOptionId { get; set; } = 0;
    public SDL.Color FontColor { get; set; } = ColorWhite;
    public MenuOption[] Options { get; set; } = null!;
    public Menu(int optionCapacity) => Options = new MenuOption[optionCapacity];
    public Menu(TextDrawInfo[] textDrawInfo, int? rowOffset = null, int? columnOffset = null)
    {
        Options = new MenuOption[textDrawInfo.Length];

        for (var i = 0; i < Options.Length; i++)
        {
            if (textDrawInfo[i].PosOffset != null!)
            {
                if (rowOffset != null!)
                    pos.Y += (int)rowOffset;

                if (columnOffset != null!)
                    pos.X += (int)columnOffset;
            }

            Options[i] = new(texts[i], pos!);
        }
    }
}

public class MenuOption(TextDrawInfo textDrawInfo)
{
    public TextDrawInfo TextDrawInfo { get; set; } = textDrawInfo;
    public bool Enabled { get; set; } = true;
}
*/

public class UiMenu
{
    bool IsPressed { get; set; }
    public int ActiveOptionId { get; private set; } = 0;
    public int SubMenuId { get; private set; } = -1;
    public int CursorMoveVoice { get; set; } = 508;
    public UiMenuOption[] Options { get; set; } = null!;
    public SDL.Color Color { get; set; } = ColorWhite;
    public SDL.Color DisableColor { get; set; } = ColorRed;
    public SDL.Color ActiveColor { get; set; } = ColorGold;
    public SDL.Color ActiveDisableColor { get; set; } = ColorPurple;
    public int ColumnCount { get; set; } = 1;
    public bool CanVerticalMove { get; set; } = true; 
    public bool CanHorizontalMove { get; set; } = true;
    public bool RememberCursor { get; set; } = false;
    public UiMenuOption ActiveOption => Options[ActiveOptionId];
    public UiMenu SubMenu => (SubMenuId == -1) ? null! : Options[SubMenuId].SubMenu;
    public MenuCheckCursorMove DelegateCheckCursorMove { get; set; } = null!;
    public bool CheckOptionEnabled(int optionId) => Options[optionId].Enabled;
    public bool CheckOptionHidden(int optionId) => Options[optionId].Hidden;

    public delegate int MenuCheckCursorMove(in UiMenu menu);

    public UiMenu(int optionCapacity) => Options = new UiMenuOption[optionCapacity];

    public UiMenu(nint[][] surfaceGroups, AtlasPack atlasPack, string[] texts = null!, int rowOffset = 0, int columnCount = 1, int columnOffset = 0, bool rememberCursor = false, MenuCheckCursorMove @delegate = null!)
    {
        Options = new UiMenuOption[surfaceGroups.Length];
        ColumnCount = columnCount;
        RememberCursor = rememberCursor;
        DelegateCheckCursorMove = @delegate;

        var rect = atlasPack?.Rect ?? new();
        for (var i = 0; i < Options.Length; i++)
        {
            var pack = atlasPack?.Clone() ?? new();
            Options[i] = new(surfaceGroups[i], Text: texts?[i]!, AtlasPack: pack);

            pack.Rect.X = rect.X += columnOffset;
            pack.Rect.Y = rect.Y += rowOffset;

            CanVerticalMove = rowOffset > 0;
            CanHorizontalMove = ColumnCount > 1;
        }
    }

    public UiMenu(nint[][] surfaceGroups, AtlasPack atlasPack, SDL.Rect[] rects, string[] texts = null!, bool rememberCursor = false, MenuCheckCursorMove @delegate = null!) : this(surfaceGroups, atlasPack, texts: texts, rememberCursor: rememberCursor, @delegate: @delegate)
    {
        //
        // 设置每个选项的坐标偏移
        //
        for (var i = 0; i < Options.Length; i++) Options[i].AtlasPack.Rect = rects[i];

        CanVerticalMove = true;
        CanHorizontalMove = true;
    }

    /// <summary>
    /// 检查并修复光标
    /// </summary>
    /// <returns>是否修复成功</returns>
    public bool CheckAndFixActiveOptionId()
    {
        if (!CheckOptionEnabled(ActiveOptionId))
        {
            for (var i = ActiveOptionId; i < Options.Length; i++)
                if (CheckOptionEnabled(i))
                {
                    //
                    // 查找成功
                    //
                    ActiveOptionId = i;
                    return true;
                }

            //
            // 查找失败
            //
            return false;
        }

        //
        // 查找失败
        //
        return true;
    }

    /// <summary>
    /// 检查并修复光标
    /// </summary>
    /// <returns>是否修复成功</returns>
    public bool CheckAndSkipHiddenOption()
    {
        if (CheckOptionHidden(ActiveOptionId))
        {
            for (var i = 0; i < Options.Length; i++)
                if (!CheckOptionHidden(i))
                {
                    //
                    // 查找成功
                    //
                    ActiveOptionId = i;
                    return true;
                }

            //
            // 查找失败
            //
            return false;
        }

        //
        // 查找失败
        //
        return true;
    }

    /// <summary>
    /// 绘制菜单
    /// </summary>
    /// <param name="atlas">要绘制到的图集</param>
    /// <param name="needClearKeyState">是否需要清理上一帧的输入状态</param>
    /// <returns>用户选择的选项序列</returns>
    public List<int> Draw(Atlas atlas)
    {
        //
        // 先绘制所有选项
        //
        for (var i = 0; i < Options.Length; i++)
        {
            var option = Options[i];

            atlas.Add(new(option.Surfaces[0]), option.AtlasPack);

            if (option.Text != null)
                PalText.DrawText(atlas, new()
                {
                    Text = option.Text,
                    ParentPack = option.AtlasPack,
                    Foreground = option.Enabled ? Color : DisableColor,
                    HorizontalAlign = PalHorizontalAlign.Middle,
                    VerticalAlign = PalVerticalAlign.Middle,
                });
        }

        //
        // 默认情况下，用户没有做出任何选择
        //
        var results = (List<int>)null!;

        //
        // 检查子菜单是否被打开
        //
        if (SubMenu != null)
        {
            //
            // 显示子菜单
            //
            if ((results = SubMenu.Draw(atlas)) != null)
            {
                //
                // 检查子菜单返回的结果路径
                //
                if (results.Count != 0)
                    //
                    // 用户选择了最终选项，将当前菜单的选择也加进去，组成选项路径
                    //
                    results.Add(ActiveOptionId);
                else if (results.Count == 0)
                    //
                    // 退出了子菜单
                    //
                    ExitSubMenu();
            }
        }
        else
        {
            //
            // 接受一帧输入
            //
            PalInput.ProcessEvent();

            //
            // 检查用户输入
            //
            if (PalInput.Pressed(PalKey.Search))
            {
                //
                // 开始按钮按下状态
                //
                IsPressed = true;

                //
                // 用户按下了选项，播放反馈音效
                //
                PalAudio.PlayVoice(ActiveOption.PressVoice);
            }

            if (PalInput.Pressed(PalKey.Menu))
            {
                //
                // 关闭按钮按下状态
                //
                IsPressed = false;

                //
                // 用户退出菜单，播放反馈音效
                //
                PalAudio.PlayVoice(PalUiGame.MenuExitVoice);

                //
                // 退出子菜单
                //
                ExitSubMenu();

                results = [];
            }
            else if (PalInput.Released(PalKey.Search))
            {
                //
                // 关闭按钮按下状态
                //
                IsPressed = false;

                if (!ActiveOption.Enabled)
                {
                    //
                    // 用户选择了被禁用的选项，播放反馈音效
                    //
                    //PalAudio.PlayVoice(ActiveOption.PressVoice);
                }
                else
                {
                    if (ActiveOption.SubMenu != null)
                    {
                        //
                        // 有子菜单，打开子菜单
                        //
                        SubMenuId = ActiveOptionId;

                        //
                        // 打开子菜单
                        //
                    }
                    else
                    {
                        //
                        // 返回最终结果
                        //
                        results = [ActiveOptionId];
                    }
                }
            }
            else
                //
                // 检查光标是否移动
                //
                CheckCursorMove();

            //
            // 检查光标有效性（自动避开隐藏的选项）
            //
            if (CheckAndSkipHiddenOption())
            {
                //
                // 绘制活动选项（光标）
                //
                atlas.Add(new(ActiveOption.Surfaces[IsPressed ? 2 : 1]), ActiveOption.AtlasPack);

                //
                // 绘制活动选项的文字（光标）
                //
                if (ActiveOption.Text != null)
                    PalText.DrawText(atlas, new()
                    {
                        Text = ActiveOption.Text,
                        ParentPack = ActiveOption.AtlasPack,
                        Foreground = (CheckOptionEnabled(ActiveOptionId)) ? ActiveColor : ActiveDisableColor,
                        HorizontalAlign = PalHorizontalAlign.Middle,
                        VerticalAlign = PalVerticalAlign.Middle,
                    });
            }
        }

        //
        // 清理按键输入状态
        //
        PalInput.ClearKeyState();

        //
        // 用户没有做出任何选择
        //
        return results!;
    }

    /// <summary>
    /// 检查光标是否移动
    /// </summary>
    public void CheckCursorMove()
    {
        var activityOptionId = ActiveOptionId;

        if (DelegateCheckCursorMove == null)
        {
            if (CanVerticalMove && PalInput.Pressed(PalKey.Down))
            {
                //
                // 选项光标向下移动
                //
                activityOptionId++;

                if (activityOptionId > Options.Length - 1)
                    activityOptionId = 0;
            }
            else if (CanHorizontalMove && PalInput.Pressed(PalKey.Left))
            {
                //
                // 选项光标向左移动
                //
                activityOptionId--;

                if (activityOptionId < 0)
                    activityOptionId = Options.Length - 1;
            }
            else if (CanVerticalMove && PalInput.Pressed(PalKey.Up))
            {
                //
                // 选项光标向上移动
                //
                activityOptionId--;

                if (activityOptionId < 0)
                    activityOptionId = Options.Length - 1;
            }
            else if (CanHorizontalMove && PalInput.Pressed(PalKey.Right))
            {
                //
                // 选项光标向下移动
                //
                activityOptionId++;

                if (activityOptionId > Options.Length - 1)
                    activityOptionId = 0;
            }
        }
        else
            //
            // 检查自定义键盘事件
            //
            activityOptionId = DelegateCheckCursorMove(this);

        if (activityOptionId != ActiveOptionId)
        {
            if (!CheckOptionHidden(activityOptionId))
            {
                //
                // 光标移动了，播放反馈音效
                //
                PalAudio.PlayVoice(CursorMoveVoice);

                ActiveOptionId = activityOptionId;
            }
            else
            {
                //
                // 播放光标移动失败失败反馈
                //

            }
        }
    }

    /// <summary>
    /// 关闭菜单路径
    /// </summary>
    public void ExitSubMenu()
    {
        SubMenuId = -1;

        if (!RememberCursor)
            //
            // 重置光标位置为默认
            //
            ActiveOptionId = 0;
    }

    /// <summary>
    /// 递归关闭菜单路径
    /// </summary>
    public void RecursiveSubMenu()
    {
        SubMenu?.RecursiveSubMenu();
        ExitSubMenu();
    }
}

public class UiMenuOption(nint[] Surfaces, string Text = null!, AtlasPack AtlasPack = null!)
{
    public nint[] Surfaces { get; set; } = Surfaces;
    public string Text { get; set; } = Text;
    public AtlasPack AtlasPack { get; set; } = AtlasPack;
    public int PressVoice { get; set; } = 507;
    public bool Enabled { get; set; } = true;
    public bool Hidden { get; set; } = false;
    public UiMenu SubMenu { get; set; } = null!;
}
