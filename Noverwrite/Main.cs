using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Events;

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
        public static void LogInfo(string info)
        {
            Log.Info(Constants.MOD_NAME + " => " + info);
        }

        public static long IdFromString(string saveName)
        {
            return long.Parse(saveName.Split('_').Last());
        }

        public class SdVSave
        {
            public DirectoryInfo SaveDir;
            public long UniqueId;
            public static List<long> ExistingIds;
            public string SaveName { get { return SaveDir.Name; } }

            public SdVSave()
            {
                SaveDir = new DirectoryInfo(StardewModdingAPI.Constants.CurrentSavePath);
                UniqueId = IdFromString(SaveName);
            }

            public void StoreOldSave()
            {
                // Initialize
                var newUniqueId = ExistingIds.Max() + 1;
                LogInfo("New save's Id: "+newUniqueId);
                var NUILongform = newUniqueId.ToString("D9");
                var UILongform = UniqueId.ToString("D9");
                var splitSaveName = SaveName.Split('_');
                splitSaveName[splitSaveName.Length-1] = NUILongform;
                var newSaveName = string.Join("_", splitSaveName);
                LogInfo("New save's name: "+newSaveName);

                // Create new save directory
                var newSaveDir = Directory.CreateDirectory(SaveFolder.FullName+ "\\" + newSaveName);
                LogInfo("New save directory created: "+newSaveDir.FullName);

                // Copy save file
                File.Copy(SaveDir.FullName+"\\"+SaveName+"_old", newSaveDir.FullName+"\\"+newSaveName);
                LogInfo("Copied previous save file to new one");

                // Edit save file
                File.WriteAllText(newSaveDir.FullName + "\\" + newSaveName,
                    File.ReadAllText(newSaveDir.FullName + "\\" + newSaveName)
                        .Replace("<uniqueIDForThisGame>" + UILongform + "</uniqueIDForThisGame>",
                            "<uniqueIDForThisGame>" + NUILongform + "</uniqueIDForThisGame>"));

                // Copy save game info file
                File.Copy(SaveDir.FullName+"\\SaveGameInfo_old",newSaveDir.FullName+"\\SaveGameInfo");
                LogInfo("Copied previous save game info file to new one");
            }
        }

        public static ConfigObject config;
        public static DirectoryInfo SaveFolder;
        public SdVSave currentSave;
        public bool justLoaded;

        public override void Entry(params object[] objects)
        {
            LogInfo("Initializing");

            config = new JavaScriptSerializer().Deserialize<ConfigObject>(new StreamReader(Constants.MOD_FOLDER+"\\config.json").ReadToEnd());
            //Log.Info(config.SaveFolder);

            SaveFolder = new DirectoryInfo(Environment.ExpandEnvironmentVariables(config.SaveFolder));
            var saveDirs = SaveFolder.GetDirectories().Select(d=>d.Name).ToList();
            LogInfo("Found the following save files: "+string.Join(", ", saveDirs));
            SdVSave.ExistingIds = saveDirs.Select(IdFromString).ToList();
            LogInfo("Found the following save file Ids: "+string.Join(", ", SdVSave.ExistingIds));

            PlayerEvents.LoadedGame += GameLoaded;
            TimeEvents.TimeOfDayChanged += StoreOldSave;
        }

        private void StoreOldSave(object sender, EventArgsIntChanged eventArgs)
        {
            if (StardewValley.Game1.timeOfDay != 610 || currentSave == null)
            {
                //LogInfo(StardewValley.Game1.timeOfDay.ToString());
                return;
            }
            //if (justLoaded)
            //{
            //    justLoaded = false;
            //    return;
            //}
            currentSave.StoreOldSave();
        }

        private void GameLoaded(object sender, EventArgsLoadedGameChanged eventArgs)
        {
            currentSave = new SdVSave();
            LogInfo("Save "+ currentSave.SaveName + " loaded");
            justLoaded = true;
        }
    }
}
