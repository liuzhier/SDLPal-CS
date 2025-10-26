using SimpleUtility;
using System.Text;

using RLogger = Records.Mod.ConfigLog;

namespace SDLPal;

public static unsafe class Logger
{
    static string[] _logHeader => ["None", "Error", "Warning", "Debug", "Info", "All"];

    static StreamWriter _logWriter { get; set; } = null!;

    static Logger()
    {
        string      logPath;

        logPath = Global.WorkPath.Log;

        COS.Dir(logPath);

        try
        {
#if DEBUG
            logPath = $@"{logPath}\Debug.txt";
#else
            logPath = $@"{logPath}\{S.GetCurrDate()}";
            COS.Dir(logPath);
            logPath += $@"\{S.GetCurrTime()}.txt";
#endif // DEBUG

            _logWriter = new(logPath, false, Encoding.UTF8);
        }
        catch (Exception e)
        {
            S.Failed(
               "PalLog.Init",
               e.Message
            );
        }
    }

    public static void Free()
    {
        if (_logWriter != null)
            _logWriter.Dispose();
    }

    public static void Go(string log, RLogger.Level level = RLogger.Level.Info)
    {
        log = $"[{_logHeader[(int)level]}]  {log}";

        if (log.Last() != '.')
        {
            log += '.';
        }

        try
        {
            if (Global.Config!.Log.LogLevel >= level)
            {
                Console.WriteLine(log);

                _logWriter.WriteLine(log);
                _logWriter.Flush();
            }
        }
        catch (Exception e)
        {
            S.Failed(
               "PalLog.Go",
               e.Message
            );
        }
    }
}
