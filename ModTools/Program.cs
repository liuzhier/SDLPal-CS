using ModTools;

namespace SDLPal;

public static class Program
{
    delegate void MainDelegate(string[] args);

    static void Main(string[] args)
    {
        bool                isDebugModTools;

        isDebugModTools = true;

        if (isDebugModTools)
            new MainDelegate(ModMain.GameMain)(args);
        else
            new MainDelegate(PalMain.GameMain)(args);
    }
}
