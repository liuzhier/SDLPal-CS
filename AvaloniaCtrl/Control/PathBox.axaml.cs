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
using System;

namespace AvaloniaControl;

public partial class PathBox : UserControl
{
    public PathBox() => InitializeComponent();

    public new double FontSize
    {
        get => pathBoxLabel.FontSize;
        set
        {
            pathBoxLabel.FontSize = value;
            pathBoxTextBox.FontSize = value;
            pathBoxButton.FontSize = value;
        }
    }

    public string? Title
    {
        get => (string?)pathBoxLabel.Content;
        set => pathBoxLabel.Content = value;
    }

    public string? Text
    {
        get => pathBoxTextBox.Text;
        set => pathBoxTextBox.Text = value;
    }

    public event EventHandler<RoutedEventArgs>? Click
    {
        add => pathBoxButton.Click += value;
        remove => pathBoxButton.Click -= value;
    }
}
