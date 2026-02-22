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
using Avalonia.Media;
using System;

namespace AvaloniaControl;

public partial class MessageBox : UserControl
{
    public MessageBox() => InitializeComponent();

    public new double FontSize
    {
        get => MsgBox_Label.FontSize;
        set
        {
            MsgBox_Label.FontSize = value;
            MsgBox_TextBlock.FontSize = value;
            MsgBox_OK_Button.FontSize = value;
        }
    }

    public new IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public string? Title
    {
        get => (string?)MsgBox_Label.Content;
        set => MsgBox_Label.Content = value;
    }

    public string? Text
    {
        get => MsgBox_TextBlock.Text;
        set => MsgBox_TextBlock.Text = value;
    }

    public string? ButtonTitle
    {
        get => (string?)MsgBox_OK_Button.Content;
        set => MsgBox_OK_Button.Content = value;
    }

    public event EventHandler<RoutedEventArgs>? Click
    {
        add => MsgBox_OK_Button.Click += value;
        remove => MsgBox_OK_Button.Click -= value;
    }
}
