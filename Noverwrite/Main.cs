using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Noverwrite
{
    public class ConfigObject
    {
        public string SaveFolder { get; set; } = "%APPDATA%\\StardewValley\\Saves";
        public int TimeToStore { get; set; } = 610;
        public int NoverwriteEveryXDays { get; set; } = 1;
        public int NoverwriteDayOffset { get; set; } = 0;
    }

    public static class Constants
    {
        public static readonly string MOD_NAME = "Noverwrite by Hawkuro";
        public static readonly string MOD_FOLDER = "Mods\\Noverwrite";
    }
    public class Main : Mod
    {
        // Helpers
        public static void LogInfo(string info)
        {
            Log.Info(Constants.MOD_NAME + " => " + info);
        }

        public static void LogError(string error, Exception ex)
        {
            Log.Error(Constants.MOD_NAME + " => " + error + ": " + ex.Message);
        }

        public static void LogError(string error)
        {
            Log.Error(Constants.MOD_NAME + " => " + error);
        }

        public static long IdFromString(string saveName)
        {
            return long.Parse(saveName.Split('_').Last());
        }

        // Static data
        public static ConfigObject config;
        public static HashSet<long> ExistingIds;
        public static DirectoryInfo SaveFolder;
        public static bool justLoaded;

        // Store the previous save, saving it from being discarded later on
        public static void StoreOldSave()
        {
            // Initialize
            DirectoryInfo SaveDir;
            try
            {
                SaveDir = new DirectoryInfo(StardewModdingAPI.Constants.CurrentSavePath);
            }
            catch (Exception e)
            {
                LogError("Current save directory not found, aborting", e);
                return;
            }
            // Add the current Id to the set in case it's not there already (i.e. new game)
            ExistingIds.Add(IdFromString(SaveDir.Name));

            // Create a new, unused unique Id
            var newUniqueId = ExistingIds.Max() + 1; // There are better ways to do this, but meh
            LogInfo("New save's Id: " + newUniqueId);

            // Create a new save name using the previous save's character name and new save's Id
            var splitSaveName = SaveDir.Name.Split('_');
            splitSaveName[splitSaveName.Length - 1] = newUniqueId.ToString();
            var newSaveName = string.Join("_", splitSaveName);
            LogInfo("New save's name: " + newSaveName);

            // Create new save directory
            DirectoryInfo newSaveDir;
            try
            {
                newSaveDir = Directory.CreateDirectory(SaveFolder.FullName + "\\" + newSaveName);
            }
            catch (Exception e)
            {
                LogError("Failed to create new save's directory", e);
                return;
            }
            LogInfo("New save directory created: " + newSaveDir.FullName);
            ExistingIds.Add(newUniqueId);

            // Copy and edit save file with new Id
            try
            {
                File.WriteAllText(newSaveDir.FullName + "\\" + newSaveName,
                    new Regex("<uniqueIDForThisGame>\\d+</uniqueIDForThisGame>")
                        .Replace(File.ReadAllText(SaveDir.FullName + "\\" + SaveDir.Name + "_old"),
                            "<uniqueIDForThisGame>" + newUniqueId + "</uniqueIDForThisGame>"));
            }
            catch (Exception e)
            {
                LogError("Failed to copy and edit save file", e);
                return;
            }
            LogInfo("Copied previous save file to new one");

            // Copy save game info file
            try
            {
                File.Copy(SaveDir.FullName + "\\SaveGameInfo_old", newSaveDir.FullName + "\\SaveGameInfo");
            }
            catch (Exception e)
            {
                LogError("Failed to copy save game info file", e);
                return;
            }
            LogInfo("Copied previous save game info file to new one");
        }

        public override void Entry(params object[] objects)
        {
            LogInfo("Initializing");

            // Load config and verify
            try
            {
                config = new JavaScriptSerializer().Deserialize<ConfigObject>(new StreamReader(Constants.MOD_FOLDER + "\\config.json").ReadToEnd());
            }
            catch (Exception e)
            {
                config = new ConfigObject();
                LogError("Failed to read Noverwrite config, loaded config with defaults", e);
            }

            if (config.NoverwriteDayOffset >= config.NoverwriteEveryXDays)
            {
                LogError("NoverwriteDayOffset may not be bigger or equal to NoverwriteEveryXDays");
                config.NoverwriteDayOffset = 0;
            }

            // Find the game's save folder and check existing save Ids
            List<string> saveDirs;
            try
            {
                SaveFolder = new DirectoryInfo(Environment.ExpandEnvironmentVariables(config.SaveFolder));
                saveDirs = SaveFolder.GetDirectories().Select(d=>d.Name).ToList();
            }
            catch (Exception e)
            {
                LogError("Failed to find save folder, assuming no existing saves", e);
                saveDirs = new List<string>();
            }
            LogInfo("Found the following save files: "+string.Join(", ", saveDirs));
            ExistingIds = new HashSet<long>(saveDirs.Select(IdFromString).ToList());
            LogInfo("Found the following save file Ids: "+string.Join(", ", ExistingIds));

            // Add EventListeners
            PlayerEvents.LoadedGame += GameLoaded;
            TimeEvents.TimeOfDayChanged += OnTimeChange;
        }

        public static readonly Dictionary<string, int> SeasonInts = new Dictionary<string, int>
        {
            {"spring", 0 },
            {"summer", 1 },
            {"fall", 2 },
            {"winter", 3 },
        };
        public static int TimeOfDay { get { return StardewValley.Game1.timeOfDay; } }
        public static int GameDay { get
        {
            return 28*4*(StardewValley.Game1.year-1) + 28*SeasonInts[StardewValley.Game1.currentSeason] +
                   StardewValley.Game1.dayOfMonth;
        } }
        private void OnTimeChange(object sender, EventArgsIntChanged eventArgs)
        {
            if ( TimeOfDay != config.TimeToStore)
            {
                // Do nothing, unless time of day is TimeToStore
                return;
            }
            if (justLoaded)
            {
                // Don't back up old save if this is the first day since loading
                justLoaded = false;
                return;
            }
            if (GameDay%config.NoverwriteEveryXDays != config.NoverwriteDayOffset)
            {
                // Only back up every X days
                return;
            }
            // All criteria have been met, store the save
            StoreOldSave();
        }

        private void GameLoaded(object sender, EventArgsLoadedGameChanged eventArgs)
        {
            LogInfo("Game loaded");
            justLoaded = true;
        }
    }
}
