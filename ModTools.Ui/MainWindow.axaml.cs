using Avalonia.Controls;
using Avalonia.Threading;
using System.Threading;

namespace ModTools.Ui;

public partial class MainWindow : Window
{
    private Thread ThreadUnpack = null!;

    public MainWindow()
    {
        InitializeComponent();

        //
        // 初始化全局 LogBox
        //
        logBox_TextBox.Text = "The initialization of the Console Log is complete.";
        Util.LogBox_TextBox = logBox_TextBox;
        SimpleUtility.S.InitLogBuffer();
    }

    ~MainWindow() =>
        SimpleUtility.S.FreeLogBuffer();

    private async void gamePath_PathBox_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        gamePath_PathBox.Text = await Util.PathSelector(this, "Select the game path");

    private async void modPath_PathBox_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        modPath_PathBox.Text = await Util.PathSelector(this, "Select the unpacking path for the game mod");

    private async void compiledPath_PathBox_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        compiledPath_PathBox.Text = await Util.PathSelector(this, "Select the path of the game mod compilation result");

    private void unpakButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetOptionEnable(false);

        Util.StartLog();

        ThreadUnpack ??= new Thread(obj =>
        {
            string[]        paths;
            paths = (obj as string[])!;

            ModMain.GoUnpack(paths[0], paths[1]);

            SetOptionEnable(true);
        });

        ThreadUnpack.Start(new string[] {
            gamePath_PathBox.Text!,
            modPath_PathBox.Text!
        });
    }

    private void SetOptionEnable(bool isEnable)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            pathOption_Grid.IsEnabled = isEnable;
            functionOption_Grid.IsEnabled = isEnable;
        });
    }
}