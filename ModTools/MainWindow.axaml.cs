using Avalonia.Controls;
using System.Threading.Tasks;

namespace ModTools.Ui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        //
        // 初始化全局 LogBox
        //
        logBox_TextBox.Text = "The initialization of the Console Log is complete.";
        Util.LogBox_TextBox = logBox_TextBox;
    }

    private async void gamePath_PathBox_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        gamePath_PathBox.Text = await Util.PathSelector(this, "Select the game path");

    private async void modPath_PathBox_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        modPath_PathBox.Text = await Util.PathSelector(this, "Select the unpacking path for the game mod");

    private async void compiledPath_PathBox_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        compiledPath_PathBox.Text = await Util.PathSelector(this, "Select the path of the game mod compilation result");

    static string gamePath { get; set; } = null!;
    static string modPath { get; set; } = null!;

    private void unpakButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetOptionEnable(false);

        gamePath = gamePath_PathBox.Text!;
        modPath = modPath_PathBox.Text!;

        Task.Run(() =>
        {
            ModMain.GoUnpack(gamePath, modPath);

            SetOptionEnable(true);
        });
    }

    private void SetOptionEnable(bool isEnable) =>
        Util.UpdateUiAsync(() =>
        {
            pathOption_Grid.IsEnabled = isEnable;
            functionOption_Grid.IsEnabled = isEnable;
        });
}