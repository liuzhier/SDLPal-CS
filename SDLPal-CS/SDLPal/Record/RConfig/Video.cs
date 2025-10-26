using SDL3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLPal.Record.RConfig
{
    public record class Video(
        int Width,
        int Height,
        bool FullScreen,
        bool KeepAspectRatio,
        SDL.ScaleMode ScaleMode
    );
}
