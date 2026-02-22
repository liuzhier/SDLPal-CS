using SDL3;

namespace SimpleUtility;

/// <summary>
/// FOS 类名称是 Free objects safely 的缩写，
/// 其旨在为程序“安全地创建对象”。
/// </summary>
public class FOS
{
    public static void Surface(nint lpSrc) => SDL.DestroySurface(lpSrc);

    public static void Surface(ref nint pSurface)
    {
        Surface(pSurface);
        pSurface = 0;
    }

    public static void Texture(nint pTexture) => SDL.DestroyTexture(pTexture);

    public static void Texture(ref nint pSrc)
    {
        Texture(pSrc);
        pSrc = 0;
    }

    public static void Palette(nint pPalette) => SDL.DestroyPalette(pPalette);

    public static void Palette(ref nint pPalette)
    {
        Palette(pPalette);
        pPalette = 0;
    }
}
