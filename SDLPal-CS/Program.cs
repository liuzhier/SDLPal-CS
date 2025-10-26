using ModTools;

namespace SDLPal;

public static class Program
{
    delegate void MainDelegate();

    static void Main(string[] args)
    {
        bool                isDebugModTools;

        isDebugModTools = true;

        if (isDebugModTools)
            new MainDelegate(ModMain.GameMain)();
        else
            new MainDelegate(PalMain.GameMain)();
    }
}
