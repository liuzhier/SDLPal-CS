using Avalonia.Controls;
using System;
using Vanara.PInvoke;

namespace DebugTools;

public record class HookAvaloniaData
{
    public Window Window { get; init; }
    public nint Handle => Window?.TryGetPlatformHandle()?.Handle ?? 0;
    public bool IsInit { get; set; } = true;
    public bool IsHooked { get; set; } = false;
    public bool NeedHook { get; set; } = true;
    public bool MouseCanPassThrough { get; set; }

    public HookAvaloniaData(Window window, bool mouseCanPassThrough = false)
    {
        Window = window;
        MouseCanPassThrough = mouseCanPassThrough;
    }
}
