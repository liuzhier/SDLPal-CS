#region License
/*
 * Copyright (c) 2025, liuzhier <lichunxiao_lcx@qq.com>.
 * 
 * This file is part of SDLPAL-CS.
 * 
 * SDLPAL-CS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 3
 * as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */
#endregion License

using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControl;
using System.Threading.Tasks;

namespace ModTools.Ui;

public partial class MainWindow : Window
{
    string GamePath { get; set; } = null!;
    string ModPath { get; set; } = null!;

    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        GamePath_PathBox.Text = @"E:\PAL98_new";
        ModPath_PathBox.Text = @"E:\PAL98_new\palmod";
#endif // DEBUG

        //
        // 设计器有 bug，不能在设计时添加事件（除了 Click 事件）
        //
        GamePath_PathBox.Click += GamePath_PathBox_Click;
        ModPath_PathBox.Click += ModPath_PathBox_Click;
        UnpackButton.Click += UnpackButton_ClickAsync;
        MsgBox.Click += MsgBox_Click;

        //
        // 初始化全局控件
        //
        LogBox_TextBox.Text = "The initialization of the Console Log is complete.";
        UiDelegateUtil.LogBox_TextBox = LogBox_TextBox;
        UiDelegateUtil.Init();

        //
        // 绑定崩溃回调
        //
        S.ExeHwnd = TryGetPlatformHandle()?.Handle ?? 0;
        S.ExeFreeCallBack = new(() => AlaUtil.UpdateUi(() =>
        {
            Close();
        }));
    }

    /// <summary>
    /// 选择游戏目录
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GamePath_PathBox_Click(object? sender, RoutedEventArgs e)
    {
        string?     path;

        //
        // 禁止与所有控件互动
        //
        SetOptionEnable(false);

        if ((path = await AlaUtil.PathSelector(this, "Select the game path")) != null)
            GamePath_PathBox.Text = path;

        //
        // 允许与所有控件互动
        //
        SetOptionEnable(true);
    }

    /// <summary>
    /// 选择 MOD 目录
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ModPath_PathBox_Click(object? sender, RoutedEventArgs e)
    {
        string?     path;

        //
        // 禁止与所有控件互动
        //
        SetOptionEnable(false);

        if ((path = await AlaUtil.PathSelector(this, "Select the unpacking path for the game mod")) != null)
            ModPath_PathBox.Text = path;

        //
        // 允许与所有控件互动
        //
        SetOptionEnable(true);
    }

    /// <summary>
    /// 点击解包按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void UnpackButton_ClickAsync(object? sender, RoutedEventArgs e)
    {
        //
        // 禁止与所有控件互动
        //
        SetOptionEnable(false);

        GamePath = GamePath_PathBox.Text!;
        ModPath = ModPath_PathBox.Text!;

        //
        // 开始解包
        //
        await Task.Run(async () => await ModToolsMain.GoUnpack(GamePath, ModPath));

        //
        // 允许与所有控件互动
        //
        SetOptionEnable(true);
    }

    /// <summary>
    /// 消息框的按钮被点击
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MsgBox_Click(object? sender, RoutedEventArgs e) =>
        MsgBox.IsVisible = false;

    /// <summary>
    /// 允许/禁止点击所有按钮a
    /// </summary>
    /// <param name="isEnabled">是否允许</param>
    /// <returns></returns>
    private void SetOptionEnable(bool isEnabled) =>
        AlaUtil.UpdateUi(() =>
        {
            pathOption_Grid.IsEnabled = isEnabled;
            functionOption_Grid.IsEnabled = isEnabled;
        });
}
