using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace ModTools;

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
