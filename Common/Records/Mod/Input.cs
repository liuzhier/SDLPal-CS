using SDL3;

namespace Records.Mod;

public class InputState
{
    public PalDirection Direction { get; set; } = PalDirection.Current;
    public PalKey KeyDown { get; set; } = PalKey.None;
    public PalKey KeyUp { get; set; } = PalKey.None;
    public ulong[] KeyOrder { get; set; } = new ulong[4];
    public ulong KeyMaxCount { get; set; } = 0;
}

public class KeyDict(SDL.Keycode sdl, PalKey pal)
{
    public SDL.Keycode KeySDL { get; init; } = sdl;
    public PalKey KeyPAL { get; init; } = pal;
}
