using Avalonia.Controls;
using Avalonia.Interactivity;
using Lib.Ala;
using PatchPackageTool.Records;
using Records.Patch;
using SimpleUtility;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PatchPackageTool;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        //
        // 设置窗口标题
        //
        Title = $"Pal Patch Package Tool [{BuildInfo.CompileDateTime}]";

        //
        // 设计器有 bug，不能在设计时添加事件（除了 Click 事件）
        //
        PatchPath_PathBox.Click += PatchPath_PathBox_Click;
        Packing_Button.Click += Packing_Button_Click;
    }

    private async void PatchPath_PathBox_Click(object? sender, RoutedEventArgs e)
    {
        string?     path;

        //
        // 禁止与所有控件互动
        //
        SetOptionEnable(false);

        if ((path = await AlaUtil.PathSelector(this, "Select the game path")) != null)
            PatchName_PathBox.Text = path;

        //
        // 允许与所有控件互动
        //
        SetOptionEnable(true);
    }

    private async void Packing_Button_Click(object? sender, RoutedEventArgs e)
    {
        string              pathIn, pathOut;

        PatchConfig         config;
        FileInfo            fileInfo;
        PatchFileInfo       patchFile;
        long                totalSize;
        byte[]              hash;

        //
        // 禁止与所有控件互动
        //
        SetOptionEnable(false);

        await Task.Run(async () =>
        {
            pathIn = @".\";
            pathOut = @".\";
            AlaUtil.UpdateUi(() => {
                //
                // 获取用户输入的路径
                //
                pathIn = Path.GetFullPath($@"{PatchPath_PathBox.Text ?? pathIn}\");
                pathOut = Path.GetFullPath($@"{PackagePath_PathBox.Text ?? pathOut}\");
            });

            if (Path.GetFullPath($@"{Environment.CurrentDirectory}\").StartsWith(pathIn))
            {
                //
                // 输入路径不能是当前工作目录
                //
                AlaUtil.MsgBoxError(MsgBox, "Patch path cannot be the current working directory");
                return;
            }

            if (pathIn.Equals(pathOut))
            {
                //
                // 输出路径不能是输入路径
                //
                AlaUtil.MsgBoxError(MsgBox, "Patch path and package path cannot be the same");
                return;
            }

            if (!Directory.Exists(pathIn))
            {
                //
                // 目录不存在
                //
                AlaUtil.MsgBoxError(MsgBox, $@"The path '{pathIn}' does not exist!");
                return;
            }

            config = new();
            AlaUtil.UpdateUi(() =>
            {
                config.PatchInfo.Name = PatchName_PathBox.Text ?? string.Empty;
                config.PatchInfo.Version = PatchVersion_PathBox.Text ?? "1.0.0";
                config.PatchInfo.Author = Author_PathBox.Text ?? string.Empty;
                config.PatchInfo.Description = Description_PathBox.Text ?? string.Empty;
                config.PatchInfo.Compatibility = Compatibility_PathBox.Text ?? "仙剑98 v1.0";
                pathOut = $@"{pathOut}\{config.PatchInfo.Name}_output.zip";
            });
            config.Files.Clear();
            totalSize = 0;

            foreach (string file in Directory.EnumerateFiles(pathIn, "*.*"))
            {
                fileInfo = new(file);
                patchFile = new()
                {
                    FileName = Path.GetFileName(file),
                    OriginalSize = fileInfo.Length,
                    PatchSize = fileInfo.Length,
                    TargetPath = Path.GetRelativePath(pathIn, $@"{pathIn}\"),
                };

                try
                {
                    using var md5 = MD5.Create();
                    using var stream = File.OpenRead(file);

                    hash = md5.ComputeHash(stream);
                    patchFile.CheckSum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
                catch (Exception ex)
                {
                    //
                    // 计算文件校验和失败
                    //
                    AlaUtil.MsgBoxError(MsgBox, $@"An error occurred when calculating the checksum of the file '{file}': {ex.Message}");
                    return;
                }

                //
                // 将文件校验和放入配置表
                //
                config.Files.Add(patchFile);
                totalSize += fileInfo.Length;
            }

            //
            // 设置“检查完整性”时使用的字段
            //
            config.Integrity.FileCount = config.Files.Count;
            config.Integrity.TotalSize = totalSize;

            AlaUtil.UpdateUi(() =>
            {
                //
                // 保存 json 文件
                //
                S.JsonSave(config, $@"{pathIn}\patch.json");

                //
                // 打包到指定目录
                //
                CreateZip(pathIn, pathOut);
            });
        });

        //
        // 允许与所有控件互动
        //
        SetOptionEnable(true);
    }

    public static void AddDirectoryToZip(ZipArchive archive, string sourceDir, string entryPath, string filterPath = "")
    {
        string              entryName, dirName, subEntryPath;
        ZipArchiveEntry     entry;

        //
        // 添加当前目录下的所有文件
        //
        foreach (string filePath in Directory.GetFiles(sourceDir).Where(filePath => !string.Equals(filePath, filterPath, StringComparison.OrdinalIgnoreCase)))
        {
            entryName = Path.Combine(entryPath, Path.GetFileName(filePath));
            entry = archive.CreateEntry(entryName);

            using FileStream fileStream = new FileStream(filePath, FileMode.Open);
            using Stream entryStream = entry.Open();

            fileStream.CopyTo(entryStream);
        }

        //
        // 递归处理子目录
        //
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            dirName = Path.GetFileName(subDir);
            subEntryPath = Path.Combine(entryPath, dirName);

            AddDirectoryToZip(archive, subDir, subEntryPath);
        }
    }

    public static void CreateZip(string pathIn, string pathOut)
    {
        using FileStream zipToOpen = new FileStream(pathOut, FileMode.Create);
        using ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create);

        AddDirectoryToZip(archive, pathIn, "", pathOut);
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
            Options_Grid.IsEnabled = isEnabled;
        });
}
