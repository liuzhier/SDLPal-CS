using System.Runtime.InteropServices;

namespace ModLoader;

public static unsafe class Util
{
    [UnmanagedCallersOnly(EntryPoint = "Add")]
    public static int Add(int a, int b) => a+ b;
}
