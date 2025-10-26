using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace ModTools;

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
