using Lib.Mod;
using SimpleUtility;
using System.IO;

namespace Records.Mod;

public class Config
{
    public ConfigVideo Video { get; init; }
    public ConfigInput Input { get; init; }
    public ConfigLog Log { get; init; }

    public Config()
    {
        string      path;
        Config      jsonConfig;

        path = "Config.json";

        if (S.FileExist(path, isAssert: false))
            //
            // 文件存在，直接读取 json 文件
            //
            S.JsonLoad(out jsonConfig, path);

        Video ??= new ConfigVideo(
            Width: 1280,
            Height: 960,
            FullScreen: false,
            KeepAspectRatio: true
            //ScaleMode: SDL.ScaleMode.Nearest
        );

        Input ??= new ConfigInput(
            EnableKeyRepeat: true
        );

        Log ??= new ConfigLog
        {
#if DEBUG && TRUE
            LogLevel = ConfigLog.Level.All
#else
            LogLevel = ConfigLog.Level.Warning
#endif // DEBUG
        };
    }
}

public record class ConfigVideo(
    int Width,
    int Height,
    bool FullScreen,
    bool KeepAspectRatio
//SDL.ScaleMode ScaleMode
);

public record class ConfigInput(
    bool EnableKeyRepeat
);

public record class ConfigLog
{
    public enum Level
    {
        None    = 0,
        Error,
        Warning,
        Debug,
        Info,
        All,
    }

    public Level LogLevel { get; init; }
}

