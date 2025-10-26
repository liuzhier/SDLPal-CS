using SimpleUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLPal.Record.RConfig
{
    public record class Game(
        Logger.Level LogLevel
    );
}
