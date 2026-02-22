using SDLPal;

namespace Records.Mod;

public class CommandAnimationFramePack
{
    public bool IsPlaying { get; set; }
    public PalUiGame.CommandAnimationType AnimationType { get; set; }
    public int FrameId { get; set; }
    public ulong LastTick { get; set; }
}
