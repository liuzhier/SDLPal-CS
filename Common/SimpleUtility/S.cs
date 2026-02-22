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

using Records.Mod;
using Records.Mod.RGame;
using Records.Pal;
using Records.Ts;
using SDL3;
using SDLPal;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using Vanara.PInvoke;
using ModAssetsPath = Records.Mod.WorkPathAssets;
using ModDataPath = Records.Mod.WorkPathData;
using ModEntity = Records.Mod.Entity;
using ModPath = Records.Mod.WorkPath;
using ModSpritePath = Records.Mod.WorkPathSprite;

namespace SimpleUtility;

/// <summary>
/// S 类名称的全写为 Safely，
/// 其旨在为程序提供“安全的工具”。
/// </summary>
public static unsafe class S
{
    public static Setup Setup => PalConfig.Setup;
    public static ModPath ModPath => PalConfig.ModWorkPath;
    public static ModAssetsPath ModAssetsPath => ModPath.Assets;
    public static ModDataPath ModDataPath => ModAssetsPath.Data;
    public static ModSpritePath ModSpritePath => ModAssetsPath.Sprite;
    public static string ModUiPath => $"{ModSpritePath.Ui}";
    public static GameSave Save => PalGlobal.Save;
    public static Scene CurrScene => Save.Scenes[Save.SceneId];
    public static Inventory CurrentInventory => Save.Inventories[Save.ItemCursorId];
    public static Item CurrentItem => CurrentInventory.Item;
    public static Trail HeroTeamLeaderTrail => GetMemberTrail(0);
    public static ModEntity Entity => Save.Entity;
    public static bool NeedDrawSceneDebugData => PalGlobal.NeedDrawSceneDebugData;
    public static void SetDrawSceneDebugData(bool allowDraw) => PalGlobal.NeedDrawSceneDebugData = allowDraw;
    public static int GetAddr(string addressTag) => PalConfig.GetNewAddress(addressTag);
    public static void Log(string message) => PalGlobal.Logger.WriteLine(message);
    public static int GetHeroMagicCursorId(int heroId) => Save.HeroMagicCursorIds[heroId - 1];

    /// <summary>
    /// 获取类型的大小
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static int SizeOf<T>() => Marshal.SizeOf<T>();

    /// <summary>
    /// 标准化结构体指针
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static nint Ptr<T>(T* value) where T : struct => (nint)value;

    /// <summary>
    /// 标准化结构体指针
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static nint ArrayPtr<T>(T[] array) where T : struct
    {
        if (array.Length == 0) return 0;

        var size = SizeOf<T>();
        var ptr = C.malloc(size * array.Length);

        for (int i = 0; i < array.Length; i++)
            Marshal.StructureToPtr(array[i], nint.Add(ptr, size * i), fDeleteOld: false);

        return ptr;
    }

    /// <summary>
    /// 检查一维数组索引值是否合法，若为负数则转换为倒向取元素。
    /// </summary>
    /// <param name="length">数组长度</param>
    /// <param name="index">索引</param>
    public static void CheckoutArrayIndex(int length, ref int index) =>
        index = CheckoutArrayIndex(length, index);

    /// <summary>
    /// 检查一维数组索引值是否合法，若为负数则转换为倒向取元素。
    /// </summary>
    /// <param name="length">数组长度</param>
    /// <param name="index">索引</param>
    /// <returns>转换后的索引</returns>
    public static int CheckoutArrayIndex(int length, int index)
    {
        if (index < 0) index = length - index;

        return index;
    }

    /// <summary>
    /// 检查二维数组索引值是否合法，若为负数则转换为倒向取元素。
    /// </summary>
    /// <param name="length">数组总长度</param>
    /// <param name="index">索引</param>
    /// <param name="index2">索引2</param>
    public static void CheckoutArrayIndex2(int length, ref int index, int length2, ref int index2) =>
        (index, index2) = CheckoutArrayIndex2(length, index, length2, index2);

    /// <summary>
    /// 检查二维数组索引值是否合法，若为负数则转换为倒向取元素。
    /// </summary>
    /// <param name="length">数组总长度</param>
    /// <param name="index">索引</param>
    /// <param name="index2">索引2</param>
    /// <returns>转换后的两个索引</returns>
    public static (int, int) CheckoutArrayIndex2(int length, int index, int length2, int index2)
    {
        if (index < 0) index = (-index) % length;
        if (index2 < 0) index2 = (-index2) % length2;

        return (index, index2);
    }

    /// <summary>
    /// 复制文件。
    /// </summary>
    /// <param name="pathIn">文件所在目录</param>
    /// <param name="pathFileIn">文件名称</param>
    /// <param name="pathOut">欲复制到的目标目录</param>
    /// <param name="pathFileOut">另存为的文件名称</param>
    public static void FileCopy(string pathIn, string pathFileIn, string pathOut, string? pathFileOut = null)
    {
        if (pathFileOut == null || pathFileOut == "")
            pathFileOut = pathFileIn;

        File.Copy($@"{pathIn}\{pathFileIn}", $@"{pathOut}\{pathFileOut}", true);
    }

    /// <summary>
    /// 复制整个目录。
    /// </summary>
    /// <param name="sourceDirectory">欲复制的文件夹</param>
    /// <param name="fileFullName">欲复制的文件名</param>
    /// <param name="destDirectory">目标文件所在目录</param>
    public static void DirCopy(string sourceDirectory, string fileFullName, string destDirectory, bool checkPath = true)
    {
        if (checkPath) COS.Dir(destDirectory);

        foreach (string pngFile in Directory.EnumerateFiles(sourceDirectory, fileFullName))
            FileCopy(sourceDirectory, Path.GetFileName(pngFile), destDirectory);
    }

    /// <summary>
    /// 复制整个目录。
    /// </summary>
    /// <param name="sourceDirectory">欲复制</param>
    /// <param name="fileFullName">欲复制的文件名</param>
    /// <param name="destDirectory">目标文件所在目录</param>
    public static void DirCopy(string sourceDirectory, string[] fileFullNames, string destDirectory)
    {
        COS.Dir(destDirectory);

        foreach (string fileFullName in fileFullNames)
            DirCopy(sourceDirectory, $"*.{fileFullName}", destDirectory, checkPath: false);
    }

    /// <summary>
    /// 遇到错误后执行的回调函数
    /// </summary>
    public delegate void FreeCallBack();
    public static FreeCallBack ExeFreeCallBack { get; set; } = null!;
    public static nint ExeHwnd { get; set; }
    public static nint SDLWindow { get; set; }

    /// <summary>
    /// 弹出错误消息框。
    /// </summary>
    /// <param name="funcName">触发错误的函数名</param>
    /// <param name="error">错误信息内容</param>
    /// <param name="fIsCorrect">错误触发器</param>
    public static Exception Failed(string funcName, string error, bool fIsCorrect = false)
    {
        //
        // 如果没有触发错误，直接退出
        //
        if (fIsCorrect) return null!;

        var logFatal = $"{funcName} failed: {error}";

        if (error.Last() != '.') logFatal += '.';

        //
        // 提示错误
        //
        var title = "FATAL ERROR:";
        //throw new Exception($"{title} {logFatal}");
        if (SDLWindow == 0)
            MessageBox(ExeHwnd, logFatal, title);
        else
        {
            var buttons = ArrayPtr(new SDL.MessageBoxButtonData[]
            {
                new()
                {
                    Flags = SDL.MessageBoxButtonFlags.ReturnkeyDefault,
                    ButtonID = 0,
                    Text = "Yes",
                },
            });
            SDL.ShowMessageBox(new()
            {
                Flags = SDL.MessageBoxFlags.Error,
                Window = SDLWindow,
                Title = title,
                Message = logFatal,
                NumButtons = 1,
                Buttons = buttons,
                ColorScheme = 0,
            }, out _);
            C.free(buttons);
        }

        //
        // 遇到错误，退出游戏
        //
        //ExeFreeCallBack?.Invoke();
        Assert(false, logFatal, title);
        return null!;
    }

    public static Exception SDLFailed(string funcName, bool fIsCorrect)
    {
        if (fIsCorrect)
            //
            // 没有错误，取消崩溃
            //
            return null!;

        var error = SDL.GetError();
        SDL.LogError(SDL.LogCategory.Application, $"{funcName} failed: {error}.");

        return Failed(funcName, error);
    }

    public static Exception SDLFailed(string funcName, nint ptr) => SDLFailed(funcName, ptr != 0);

    public static bool MessageBox(HWND hwnd, string text, string? title, bool isError = true, bool onlyOk = true) =>
        User32.MessageBox(hwnd, text, title, (onlyOk ? User32.MB_FLAGS.MB_OK : User32.MB_FLAGS.MB_OKCANCEL) | (isError ? User32.MB_FLAGS.MB_ICONERROR : User32.MB_FLAGS.MB_ICONWARNING)) == User32.MB_RESULT.IDOK;

    /// <summary>
    /// 获取当前日期，格式为 YYYY-MM-DD。
    /// </summary>
    /// <returns>当前日期</returns>
    public static string GetCurrDate()
    {
        string          time;
        DateTime        now;

        //
        // 获取当前时间
        //
        time = "";
        now = DateTime.Now;

        //
        // 格式化日期字符串
        //
        try
        {
            time = string.Format(
               "{0}-{1}-{2}",
               now.Year, now.Month, now.Day
            );
        }
        catch (Exception e)
        {
            Failed(
               "S.GetCurrDate",
               e.Message
            );
        }

        return time;
    }

    /// <summary>
    /// 获取当前时间，格式为 HH-MM-SS_MMM。
    /// </summary>
    /// <returns>当前时间</returns>
    public static string GetCurrTime()
    {
        string      time;
        DateTime    now;

        //
        // 获取当前时间
        //
        time = "";
        now = DateTime.Now;

        //
        // 格式化时间字符串
        //
        try
        {
            time = string.Format(
               "{0}-{1}-{2}_{3}",
               now.Hour, now.Minute, now.Second, now.Millisecond
            );
        }
        catch (Exception e)
        {
            Failed(
               "S.GetCurrTime",
               e.Message
            );
        }

        return time;
    }

    /// <summary>
    /// 获取自定义的 JavaScriptEncoder 对象
    /// </summary>
    static readonly JsonAutoEncoder DefaultEncoder = new();

    /// <summary>
    /// 获取自定义的  JsonAuto 对象，用来序列化和反序列化
    /// </summary>
    static readonly JsonAuto JsonE = new(
        new JsonSerializerOptions
        {
            Encoder = DefaultEncoder,
            WriteIndented = true,
        }
    );

    /// <summary>
    /// 将记录对象序列化为 Json 格式
    /// </summary>
    /// <typeparam name="T">记录对象的类型</typeparam>
    /// <param name="obj">记录对象</param>
    /// <param name="path">Json 文件保存路径</param>
    public static void JsonSave<T>(T obj, string path)
    {
        using var file = File.Create(path);

        //JsonSerializer.Serialize(fs, obj, options: JsonE.Options);
        JsonSerializer.Serialize(file, obj, typeof(T), JsonE);
    }

    /// <summary>
    /// 将 Json 串反序列化为记录对象
    /// </summary>
    /// <typeparam name="T">记录对象的类型</typeparam>
    /// <param name="obj">记录对象</param>
    /// <param name="path">Json 文件所在路径</param>
    public static void JsonLoad<T>(out T obj, string path)
    {
        using var file = File.OpenRead(path);

        //obj = JsonSerializer.Deserialize<T>(file, options: JsonE.Options);
        obj = (T)JsonSerializer.Deserialize(file, typeof(T), JsonE)!;
    }

    /// <summary>
    /// 检查指定的条件，如果条件计算结果为 false，则触发跟踪断言失败。
    /// </summary>
    /// <param name="conditions">要计算的条件。如果为 false，则触发跟踪断言失败。</param>
    /// <param name="message">在断言失败时显示的可选消息，可为 null。</param>
    /// <param name="datailMessage">如果断言失败，将显示一条可选的详细消息，可为 null。</param>
    public static void Assert(bool conditions, string? message = null, string? datailMessage = null) =>
        Trace.Assert(conditions, message, datailMessage);

    /// <summary>
    /// 检查文件是否存在，可触发断言。
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="isAssert">是否触发断言</param>
    /// <returns>文件是否存在</returns>
    public static bool FileExist(string path, bool isAssert = true)
    {
        var result = File.Exists(path);

        if (isAssert)
            Assert(result, $@"文件 {path} 不存在！");

        return result;
    }

    /// <summary>
    /// 检查路径是否存在，可触发断言。
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="isAssert">是否触发断言</param>
    /// <returns>文件是否存在</returns>
    public static bool DirExist(string path, bool isAssert = true)
    {
        var result = Directory.Exists(path);

        if (isAssert)
            Assert(result, $@"文件 {path} 不存在！");

        return result;
    }

    /// <summary>
    /// 将单词拼接为路径
    /// </summary>
    /// <param name="paths">各个单词</param>
    /// <returns>拼接后的路径</returns>
    public static string Paths(params string[] paths) => Path.Combine(paths);

    /// <summary>
    /// 将对象索引内容写入文件，若文件已存在则覆盖内容
    /// </summary>
    /// <param name="fileContent">文件内容</param>
    /// <param name="savePath">保存路径（不包括文件名，文件名固定为“#.txt”）</param>
    public static void IndexFileSave(EnumData enumData, string savePath)
    {
        using var file = File.CreateText($@"{savePath}\#.txt");

        file.WriteLine(enumData.Reverses.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}\t{kv.Value}\n"));
    }

    /// <summary>
    /// 将对象索引内容写入文件，若文件已存在则覆盖内容
    /// </summary>
    /// <param name="fileContent">文件内容</param>
    /// <param name="savePath">保存路径（不包括文件名，文件名固定为“#.txt”）</param>
    public static void IndexFileSave(string[] fileContent, string savePath)
    {
        using var file = File.CreateText($@"{savePath}\#.txt");

        for (var i = 0; i < fileContent.Length; i++)
            file.WriteLine($"{i}\t{fileContent[i]}");
    }

    private static FastChars CheckHex(FastChars chars, out NumberStyles numberStyles)
    {
        //
        // 使用 ReadOnlySpan 提升字符串处理性能，清除首尾空格
        //
        var span = chars.Trim();

        if (span.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            //
            // 若以 0x 开头，则说明是 Hex 数值
            // 需要去掉这个开头，并标记为 Hex 类型
            //
            span = span[2..];

            numberStyles = NumberStyles.HexNumber;
        }
        else
            //
            // 若不以 0x 开头，则说明不是 Hex 数值
            // 直接标记为 Dec 类型
            //
            numberStyles = NumberStyles.Integer;

        return span;
    }

    public static uint StrToUInt32(FastChars chars)
    {
        Failed(
           "S.StrToUInt",
            $"The string '{chars}' is not a number",
            uint.TryParse(CheckHex(chars, out var numberStyles), numberStyles, CultureInfo.InvariantCulture, out var val)
        );

        return val;
    }

    public static uint StrToUInt32(char strVal) => StrToUInt32(strVal.ToString());

    public static bool StrTryToInt32(FastChars chars, out int result) => int.TryParse(CheckHex(chars, out var numberStyles), numberStyles, CultureInfo.InvariantCulture, out result);

    public static int StrToInt32(FastChars chars)
    {
        Failed(
           "S.StrToInt",
            $"The string '{chars}' is not a number",
            StrTryToInt32(chars, out var result)
        );

        return result;
    }

    public static int StrToInt32(char strVal) => StrToInt32(strVal.ToString());

    public static ushort StrToUInt16(FastChars chars)
    {
        Failed(
           "S.StrToUInt16",
           $@"The string '{chars}' is not a number",
           ushort.TryParse(CheckHex(chars, out var numberStyles), numberStyles, CultureInfo.InvariantCulture, out var val)
        );

        return val;
    }

    public static ushort StrToUInt16(char character) => StrToUInt16(character.ToString());

    public static short StrToInt16(FastChars chars)
    {
        Failed(
           "S.StrToInt16",
           $@"The string '{chars}' is not a number",
           short.TryParse(CheckHex(chars, out var numberStyles), numberStyles, CultureInfo.InvariantCulture, out var val)
        );

        return val;
    }

    public static short StrToInt16(char character) => StrToInt16(character.ToString());

    public static bool StrToBool(string text) => text == "true";

    public static Span<TTo> Cast<TFrom, TTo>(Span<TFrom> span)
        where TFrom : struct
        where TTo : struct
        => MemoryMarshal.Cast<TFrom, TTo>(span);

    public static void GetTexSize(nint texture, out float w, out float h) =>
        SDLFailed(
           "GetTexSize",
           //"Failed to obtain texture size",
           SDL.GetTextureSize(texture, out w, out h)
        );

    public static void GetSurfaceSize(nint surface, out int w, out int h)
    {
        var pSurface = (SDL.Surface*)surface;

        w = pSurface->Width;
        h = pSurface->Height;
    }

    public static void CleanUpTex(nint texture, uint colorHex = 0x00000000)
    {
        //SDL.SetTextureAlphaModFloat(texture, 1);
        SDL.SetTextureBlendMode(PalVideo.Renderer, SDL.BlendMode.None);
        {
            SDL.SetRenderDrawColor(
               PalVideo.Renderer,
               (byte)((colorHex >> (8 * 3)) & 0xFF),
               (byte)((colorHex >> (8 * 2)) & 0xFF),
               (byte)((colorHex >> (8 * 1)) & 0xFF),
               (byte)((colorHex >> (8 * 0)) & 0xFF)
            );
            SDL.SetRenderTarget(PalVideo.Renderer, texture);
            SDL.RenderClear(PalVideo.Renderer);
        }
        SDL.SetTextureBlendMode(PalVideo.Renderer, SDL.BlendMode.Blend);
    }

    /// <summary>
    /// 计算拉伸后的数值
    /// </summary>
    /// <param name="val">原数值</param>
    /// <returns>拉伸后的数值</returns>
    public static int Ratio(int val) => val * PalMap.Ratio;

    /// <summary>
    /// 计算拉伸前的数值
    /// </summary>
    /// <param name="val">原数值</param>
    /// <returns>拉伸前的数值</returns>
    public static int UnRatio(int val) => val / PalMap.Ratio;

    /// <summary>
    /// 获取实际文本长度（去掉特殊字符后的实际长度）
    /// </summary>
    /// <param name="text"></param>
    /// <returns>实际文本长度</returns>
    public static int GetTextActualLength(string text)
    {
        int      i, charCount;

        for (i = 0, charCount = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '_':
                case '@':
                case '\'':
                case '\"':
                    break;

                case '$':
                case '~':
                    i += 2;
                    break;

                default:
                    charCount++;
                    break;
            }
        }

        return charCount;
    }

    /// <summary>
    /// 获取实际文本内容（去掉特殊字符后的实际内容）
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>实际文本内容</returns>
    public static string GetTextActualContent(string text)
    {
        var charCount = GetTextActualLength(text);
        var span = stackalloc char[charCount];
        charCount = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var charWord = text[i];

            switch (charWord)
            {
                case '_':
                case '@':
                case '\'':
                case '\"':
                    break;

                case '$':
                case '~':
                    i += 2;
                    break;

                default:
                    span[charCount++] = charWord;
                    break;
            }
        }

        return new string(span, 0, charCount);
    }

    /// <summary>
    /// 获取文本像素尺寸（去掉特殊字符后的尺寸）
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>文本像素尺寸</returns>
    public static (int W, int H) GetTextActualSize(string text, int fontSize)
    {
        var ttfText = TTF.CreateText(PalText.TextEngine, PalText.FontDict[fontSize], GetTextActualContent(text), 0);

        TTF.GetTextSize(ttfText, out var w, out var h);
        TTF.DestroyText(ttfText);

        Failed("S.GetTextActualSize", $"Failed to obtain the final size occupied by the string '{text}' when its font size is '{fontSize}'!", (w != 0 && h != 0));

        return (w, h);
    }

    /// <summary>
    /// 获取文本像素尺寸（去掉特殊字符后的尺寸）
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>文本像素尺寸</returns>
    public static (int W, int H) GetCharActualSize(char charWord, int fontSize) =>
        GetTextActualSize($"{charWord}", fontSize);

    /// <summary>
    /// 将纹理转换为 Surface
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static nint GetTexPixels(nint texture)
    {
        var props = SDL.GetTextureProperties(texture);

        //
        // 判断纹理权限是否是 Target，不是就创建一个新的，复制像素
        //
        var accessIsNotTarget = (SDL.TextureAccess)SDL.GetNumberProperty(props, SDL.Props.TextureAccessNumber, -1) != SDL.TextureAccess.Target;
        if (accessIsNotTarget)
        {
            //
            // 创建一个新的、空的“目标纹理”来作为我们的临时画布
            // 注意：访问权限必须是 Target，并且格式带 Alpha
            //
            GetTexSize(texture, out var w, out var h);
            nint targetTexture = COS.Texture((int)w, (int)h, PalVideo.DefaultFormat, 2);

            //
            // 将源纹理的内容复制到临时纹理
            //
            PalScreen.Copy(texture, texture = targetTexture);
        }

        //
        // 将渲染目标切换到待处理纹理
        //
        SDL.SetRenderTarget(PalVideo.Renderer, texture);

        //
        // 读取临时纹理上的像素
        //
        var surface = SDL.RenderReadPixels(PalVideo.Renderer, null);

        if (accessIsNotTarget)
            //
            // 销毁临时纹理
            //
            FOS.Texture(texture);

        return surface;
    }


    public static HeroTeam GetMember(Index memberId) => Save.HeroTeams[memberId];

    public static Trail GetMemberTrail(Index memberId) => GetMember(memberId).Trail;

    public static Pos GetMemberPos(Index memberId) => GetMemberTrail(memberId).Pos;

    public static void SetMemberPos(Index memberId, Pos pos) => GetMemberTrail(memberId).Pos = pos;

    public static PalDirection GetMemberDirection(Index memberId) => GetMemberTrail(memberId).Direction;

    public static void SetMemberDirection(Index memberId, PalDirection direction) => GetMemberTrail(memberId).Direction = direction;

    public static PalDirection GetHeroTeamDirection() => HeroTeamLeaderTrail.Direction;

    public static void SetHeroTeamDirection(PalDirection direction) => HeroTeamLeaderTrail.Direction = direction;

    public static Pos GetHeroTeamPos() => HeroTeamLeaderTrail.Pos;

    public static void SetHeroTeamPos(Pos pos) => HeroTeamLeaderTrail.Pos = pos;

    public static void SetHeroTeamPos(int x, int y) => SetHeroTeamPos(new(x, y));

    public static Trail GetFollowerTrail(Index memberId) => GetMember(memberId).Trail;

    public static Pos GetFollowerPos(Index followerId) => GetFollowerTrail(followerId).Pos;

    public static void SetFollowerPos(Index followerId, Pos pos) => GetFollowerTrail(followerId).Pos = pos;

    public static PalDirection GetFollowerDirection(Index memberId) => GetFollowerTrail(memberId).Direction;

    public static PalDirection SetFollowerDirection(Index memberId, PalDirection direction) => GetFollowerTrail(memberId).Direction = direction;

    public static Scene GetScene(int sceneId)
    {
        if (sceneId == -1)
            //
            // 返回当前场景
            //
            return CurrScene;

        var scenes = Save.Scenes;

        Failed(
           "S.GetScene",
           $"Scene {sceneId} does not exist! The maximum scene number is {scenes.Count}",
           sceneId < scenes.Count
        );

        return scenes[sceneId];
    }

    public static Event TryGetEvent(int sceneId, int eventId)
    {
        var scene = GetScene(sceneId);

        if (scene == null || eventId >= scene.Events.Count)
            return null!;

        return scene.Events[eventId]!;
    }

    public static Event GetEvent(int sceneId, int eventId) => GetScene(sceneId)?.Events[eventId]!;

    public static Event GetEvent(int eventId) => GetEvent(-1, eventId);

    public static Trail GetEventTrail(int sceneId, int eventId) => GetEvent(sceneId, eventId).Sprite.Trail;

    public static Pos GetEventPos(int sceneId, int eventId) => GetEventTrail(sceneId, eventId).Pos;

    /// <summary>
    /// 生成指定范围的随机数（包括 from 和 to 这两个值）
    /// </summary>
    /// <param name="from">极小值</param>
    /// <param name="to">极大值</param>
    /// <returns>范围的随机数</returns>
    public static long RandomLong(int from, int to) => PalRandom.NextInt64(from, to + 1);

    public static uint MapFromRGBA(nint surface, SDL.Color color) =>
        SDL.MapRGBA(SDL.GetPixelFormatDetails(((SDL.Surface*)surface)->Format), 0, color.R, color.G, color.B, color.A);

    /// <summary>
    ///   可以向库存中添加或移除道具
    /// </summary>
    /// <param name="itemId">道具实体编号</param>
    /// <param name="num">需要添加数量（正值）或需要删除的数量（负值）</param>
    /// 
    /// <returns>IsSuccess: 操作是否成功<br/>Count: 当需要删除的数量不足时，返回不足的数量</returns>
    public static (bool IsSuccess, int Count) InventoryAddItem(int itemId, int num)
    {
        var isSuccess = true;
        var count = 0;

        if (itemId == 0 || num == 0)
            //
            // 无需进行任何操作
            //
            goto Return;

        var inventories = Save.Inventories;
        var item = inventories.Find(x => x.ItemId == itemId);

        if (num > 0)
        {
            //
            // 数量为正数，直接增加
            //
            if (item == null) inventories.Add(new(itemId, num));
            else item.Amount += num;

            isSuccess = true;
        }
        else
        {
            //
            // 剩下的情况只有负数了，现有数量减少
            //
            if (item != null)
            {
                isSuccess = item.Amount >= num;
                item.Amount += num;

                if (item.Amount <= 0)
                {
                    //
                    // 库存物品的数量可能不足，
                    // 所缺失的数量需要进行记录。
                    //
                    count = Math.Abs(item.Amount);
                    isSuccess = (count == 0);
                    inventories.Remove(item);
                }
            }
        }

    Return:
        return (isSuccess, count);
    }

    /// <summary>
    /// 自动使用一个道具
    /// </summary>
    /// <param name="item"></param>
    public static void AutoUseItem()
    {
        if (CurrentItem.Scope.Consuming && PalScript.ScriptSuccess)
        {
            //
            // 使用成功了，消耗一个道具
            //
            InventoryAddItem(CurrentInventory.ItemId, -1);
            Save.Inventories.Remove(CurrentInventory);
        }
    }

    /// <summary>统计库存中以及玩家装备中指定的道具数量</summary>
    /// <param name="itemId">道具实体编号</param>
    /// <returns>道具数量</returns>
    public static int InventoryCountItem(int itemId)
    {
        //
        // 在库存中查找指定的道具
        //
        var inventory = Save.Inventories.Find(x => x.ItemId == itemId);
        var count = 0;
        if (inventory != null)
            //
            // 此物品存在于库存中
            //
            count += inventory.Amount;

        //
        // 在团队成员身上的装备中查找指定的道具
        //
        for (var i = 0; i <= Save.HeroTeamLength; i++)
        {
            var hero = Entity.Heroes[GetMember(i).HeroId];

            var pEquip = (int*)&hero.Raw->Equipment;
            for (var j = 0; j < Base.MaxHeroEquipments; j++) if (pEquip[j] == itemId) count++;
        }

        return count;
    }

    public static void RemoveEquipmentEffect(int memberId, EquipmentPart equipPart)
    {
        var effect = GetMember(memberId).Hero.EquipmentEffect;

        switch (equipPart)
        {
            case EquipmentPart.Head:
                effect.Head = new();
                break;

            case EquipmentPart.Body:
                effect.Body = new();
                break;

            case EquipmentPart.Armour:
                effect.Armour = new();
                break;

            case EquipmentPart.Backside:
                effect.Backside = new();
                break;

            case EquipmentPart.Hand:
                effect.Hand = new();
                break;

            case EquipmentPart.Foot:
                effect.Foot = new();
                break;
        }
    }

    public static bool HeroModifyHPMP(int heroId, int hp, int mp)
    {
        var success = false;
        var hero = GetMember(heroId).Hero.Raw;
        var originHP = hero->HP;
        var originMP = hero->MP;

        //
        // 只作用于存活的玩家
        //
        if (hero->HP > 0)
        {
            //
            // HP 变化
            //
            hero->HP += hp;
            hero->HP = int.Max(hero->HP, 0);
            hero->HP = int.Min(hero->HP, hero->MaxHP);

            //
            // MP 变化
            //
            hero->MP += mp;
            hero->MP = int.Max(hero->MP, 0);
            hero->MP = int.Min(hero->MP, hero->MaxMP);

            if (originHP != hero->HP || originMP != hero->MP)
                //
                // HP 或 MP 有变化，返回成功结果
                //
                success = true;
        }

        return success;
    }

    public static void RoleAddMagic(int roleId, int magicId)
    {
        var magics = Entity.Heroes[roleId].Raw->Magics;
        for (var i = 0; i < Base.MaxHeroMagic; i++) if (magics[i] == 0) magics[i] = magicId;
    }
}
