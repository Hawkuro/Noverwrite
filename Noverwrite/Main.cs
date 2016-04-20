using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using StardewModdingAPI;

namespace Noverwrite
{
    public class ConfigObject
    {
        public string SaveFolder { get; set; }
    }

    public static class Constants
    {
        public static readonly string MOD_NAME = "Noverwrite by Hawkuro";
        public static readonly string MOD_FOLDER = "Mods\\Noverwrite";
    }
    public class Main : Mod
    {
        private ConfigObject config;
        public override void Entry(params object[] objects)
        {
            Log.Info(Constants.MOD_NAME + " => Initializing");

            config = new JavaScriptSerializer().Deserialize<ConfigObject>(new StreamReader(Constants.MOD_FOLDER+"\\config.json").ReadToEnd());
            //Log.Info(config.SaveFolder);

            var saveFolder = new DirectoryInfo(Environment.ExpandEnvironmentVariables(config.SaveFolder));
            Log.Info(string.Join("\n", saveFolder.GetDirectories().Select(d=>d.Name)));
        }
    }
}
