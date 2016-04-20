using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace Noverwrite
{
    public static class StaticInfo
    {
        public static readonly string MOD_NAME = "Noverwrite by Hawkuro";
    }
    public class Main : Mod
    {
        public override void Entry(params object[] objects)
        {
            Log.Info(StaticInfo.MOD_NAME + " => Initialized");
        }
    }
}
