using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GF.Log
{
    public static partial class Log
    {
        public static CustomLog Game = new CustomLog("Game", true, "#FF3333");
        public static CustomLog Net = new CustomLog("Net", true, "#FF3333");
    }
}
