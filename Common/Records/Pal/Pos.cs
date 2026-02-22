using System.Runtime.InteropServices;

namespace Records.Pal;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CPos
{
    public  short       X;
    public  short       Y;

    public readonly byte BX => (byte)(X / 32);
    public readonly byte BY => (byte)(Y / 16);
    public readonly byte BH => (byte)(X % 32);
}
