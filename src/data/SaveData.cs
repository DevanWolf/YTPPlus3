using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// This class handles data saving and loading.
    /// All data stored here are default values.
    /// </summary>
    [Serializable]
    public static class SaveData
    {
        public static Dictionary<string, string> saveValues = new Dictionary<string, string>()
        {
            {"ScreenWidth", "320"},
            {"ScreenHeight", "240"},
            {"ScreenScale", "2"}, // 1 - 4 ONLY!
            {"BackgroundSaturation", "0"},
            {"ProjectTitle", "Result"},
            {"ProjectAuthor", "Unknown"},
            {"PluginTest", ""},
            {"RandomSeed", "0"},
            {"MinStreamDuration", "0.2"},
            {"MaxStreamDuration", "0.4"},
            {"MinTransitionDuration", "0.2"},
            {"MaxTransitionDuration", "0.4"},
            {"MaxClipCount", "20"},
            {"VideoWidth", "640"},
            {"VideoHeight", "480"},
            {"PluginTestEnabled", "false"},
            {"TransitionsEnabled", "false"},
            {"IntrosEnabled", "false"},
            {"OutrosEnabled", "false"},
            {"OverlaysEnabled", "false"},
            {"AddToLibrary", "true"},
            {"AprilFoolsFlappyBirdScore", "0"},
            {"MusicVolume", "100"},
            {"SoundEffectVolume", "100"},
            {"ActiveMusic", "1"},
            {"ShuffleMusic", "false"},
            {"TransitionChance", "20"},
            {"OverlayChance", "20"},
            {"EffectChance", "60"},
            {"FirstBoot", "true"},
            {"FirstBootVersion", Global.productVersion}
        };
        private static string _saveFileName = "Options.json";
        public static bool Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(saveValues, Formatting.Indented);
                File.WriteAllText(_saveFileName, json);
                return true;
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine(e.Message);
                return false;
            }
        }
        public static bool Load()
        {
            try
            {
                if (!File.Exists(_saveFileName))
                {
                    ConsoleOutput.WriteLine("Save file not found. Creating new one.");
                    Save();
                }
                string json = File.ReadAllText(_saveFileName);
                Dictionary<string, string>? loadedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (loadedValues == null)
                {
                    ConsoleOutput.WriteLine("Save file is corrupted.");
                    loadedValues = new Dictionary<string, string>();
                }
                // Merge loaded values into save values.
                foreach (KeyValuePair<string, string> pair in saveValues)
                {
                    if (loadedValues.ContainsKey(pair.Key))
                    {
                        if (loadedValues[pair.Key] != pair.Value)
                        {
                            saveValues[pair.Key] = loadedValues[pair.Key];
                        }
                    }
                    else
                    {
                        saveValues[pair.Key] = pair.Value;
                    }
                }
                // Save the new values.
                Save();
                return true;
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine(e.Message);
                return false;
            }
        }
    }
}
