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
using Lib.Ala;
using System.Threading.Tasks;

namespace ModTools;

public partial class MainWindow : Window
{
    bool? _compiledVersion { get; set; } = null!;
    string _gamePath { get; set; } = null!;
    string _modPath { get; set; } = null!;
    string _compiledPath { get; set; } = null!;
    CheckBox[] _checkBoxs { get; init; } = null!;

    public MainWindow()
    {
        InitializeComponent();

        //
        // 统计所有有效复选框
        //
        _checkBoxs = [
            Palette_CheckBox,
            CoreData_CheckBox,
            MapAndTile_CheckBox,
            MusicAndVoice_CheckBox,
            Item_CheckBox,
            Hero_CheckBox,
            Avatar_CheckBox,
            Animation_CheckBox,
            Background_CheckBox,
        ];

        //
        // 设计器有 bug，不能在设计时添加事件（除了 Click 事件）
        //
        GamePath_PathBox.Click += GamePath_PathBox_Click;
        ModPath_PathBox.Click += ModPath_PathBox_Click;
        CompiledPath_PathBox.Click += CompiledPath_PathBox_Click;
        UnpackButton.Click += UnpackButton_ClickAsync;
        BaseData_CheckBox.IsCheckedChanged += BaseData_CheckBox_IsCheckedChanged;
        CoreData_CheckBox.IsCheckedChanged += CoreData_CheckBox_IsCheckedChanged;
        Compile_Button.Click += Compile_Button_Click;
        SelectAll_Button.Click += SelectAll_Button_Click;
        MsgBox.Click += MsgBox_Click;

        //
        // 初始化全局控件
        //
        LogBox_TextBox.Text = "The initialization of the Console Log is complete.";
        Util.LogBox_TextBox = LogBox_TextBox;
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
    /// 选择编译输出目录
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CompiledPath_PathBox_Click(object? sender, RoutedEventArgs e)
    {
        string?     path;

        //
        // 禁止与所有控件互动
        //
        SetOptionEnable(false);

        if ((path = await AlaUtil.PathSelector(this, "Select the path of the game mod compilation result")) != null)
            CompiledPath_PathBox.Text = path;

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

        _gamePath = GamePath_PathBox.Text!;
        _modPath = ModPath_PathBox.Text!;

        await Task.Run(async () =>
        {
            await ModMain.GoUnpack(_gamePath, _modPath);
        });

        //
        // 允许与所有控件互动
        //
        SetOptionEnable(true);
    }

    /// <summary>
    /// 检查 Base Data 复选框是否改变
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BaseData_CheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        CheckBox?       checkBox;
        CheckBox[]      checkBoxs;

        checkBoxs = [
            Enemy_CheckBox,
            Magic_CheckBox,
            UiAndAction_CheckBox,
        ];

        //
        // 一键勾选/取消所有选项
        //
        if ((checkBox = sender as CheckBox) is not null)
            foreach (var item in checkBoxs)
                item.IsChecked = checkBox.IsChecked;
    }

    /// <summary>
    /// 检查 Core Data 复选框是否改变
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CoreData_CheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        CheckBox?       checkBox;

        if ((checkBox = sender as CheckBox) is not null)
            BaseData_CheckBox.IsChecked = checkBox.IsChecked;
    }

    /// <summary>
    /// 检查全选复选框是否改变
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SelectAll_Button_Click(object? sender, RoutedEventArgs e)
    {
        bool            isSelectedAll, isChecked;

        //
        // 检查是否有选项未被选中
        //
        isSelectedAll = true;
        foreach (var item in _checkBoxs)
            if (item.IsChecked == false)
            {
                isSelectedAll = false;
                break;
            }

        //
        // 选择/取消选择所有选项
        //
        isChecked = !isSelectedAll;
        foreach (var item in _checkBoxs)
            item.IsChecked = isChecked;
    }

    /// <summary>
    /// 点击编译按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Compile_Button_Click(object? sender, RoutedEventArgs e)
    {
        int     isCheckedCount;

        //
        // 检查是否有选项被勾选
        //
        isCheckedCount = 0;
        foreach (var item in _checkBoxs)
            if (item.IsChecked == true)
                isCheckedCount++;

        if (isCheckedCount == 0)
            //
            // 提示用户先勾选编译选项
            //
            AlaUtil.MsgBox(MsgBox, "Please select the item you want to compile first!");
        else
        {
            //
            // 禁止与所有控件互动
            //
            SetOptionEnable(false);

            _modPath = ModPath_PathBox.Text!;
            _compiledPath = CompiledPath_PathBox.Text!;
            _compiledVersion = Version_RadioButton.IsChecked;

            await Task.Run(async () =>
            {
                await ModMain.GoCompile(_modPath, _compiledPath, _compiledVersion);
            });

            //
            // 允许与所有控件互动
            //
            SetOptionEnable(true);
        }
    }

    /// <summary>
    /// 消息框的按钮被点击
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MsgBox_Click(object? sender, RoutedEventArgs e) =>
        MsgBox.IsVisible = false;

    /// <summary>
    /// 允许/禁止点击所有按钮
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
