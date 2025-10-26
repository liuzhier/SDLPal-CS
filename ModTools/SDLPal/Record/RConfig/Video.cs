namespace SDLPal.Record.RConfig;

public record class Video(
    int Width,
    int Height,
    bool FullScreen,
    bool KeepAspectRatio
    //SDL.ScaleMode ScaleMode
);
