using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YTPPlusPlusPlus
{
    public enum PluginType
    {
        None,
        Batch,
        PowerShell,
    }
    public class PluginReturnValue
    {
        public bool success;
        public string pluginName;
        public PluginReturnValue(bool success = true, string pluginName = "")
        {
            this.success = success;
            this.pluginName = pluginName;
        }
    }
    public class Plugin
    {
        public string path { get; set; }
        public PluginType type { get; set; }
        public bool enabled { get; set; }
        public List<LibraryType> libraryTypes = new List<LibraryType>();
        public Dictionary<string, object> settings = new();
        public Dictionary<string, string> settingTooltips = new();
        public Plugin(string path, PluginType type, bool enabled = true)
        {
            this.path = path;
            this.type = type;
            this.enabled = enabled;
        }
        public PluginReturnValue Call(string video)
        {
            if (enabled == false)
            {
                return new PluginReturnValue(false, Path.GetFileName(path));
            }
            ConsoleOutput.WriteLine($"Calling plugin {Path.GetFileName(path)}", Color.LightBlue);
            switch (type)
            {
                case PluginType.Batch:
                    // Batch plugins are the simplest.
                    List<string> batchArgs = new()
                    {
                        video,
                        SaveData.saveValues["VideoWidth"],
                        SaveData.saveValues["VideoHeight"],
                        @".\temp\",
                        @".\ffmpeg.exe",
                        @".\ffprobe.exe",
                        "magick",
                        @".\library\", // legacy resources folder
                        @".\" +Path.Join("library", "audio", "sfx") + @"\",
                        @".\" +Path.Join("library", "videos", "transitions") + @"\",
                        @".\" +Path.Join("library", "audio", "music") + @"\",
                        @".\library\",
                    };
                    string batchArgsString = string.Join(" ", batchArgs);
                    ProcessStartInfo batchStartInfo = new()
                    {
                        FileName = path,
                        Arguments = batchArgsString,
                        WorkingDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    Process batchProcess = new()
                    {
                        StartInfo = batchStartInfo
                    };
                    batchProcess.OutputDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.LightBlue);
                    };
                    batchProcess.ErrorDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                    };
                    batchProcess.Start();
                    batchProcess.BeginOutputReadLine();
                    batchProcess.BeginErrorReadLine();
                    batchProcess.WaitForExit();
                    if (batchProcess.ExitCode == 0)
                    {
                        return new PluginReturnValue(true, Path.GetFileName(path));
                    }
                    return new PluginReturnValue(false, Path.GetFileName(path));
                case PluginType.PowerShell:
                    // ps1 plugins use the same format as batch with Set-ExecutionPolicy Bypass -Scope Process; path
                    List<string> psArgs = new()
                    {
                        "Set-ExecutionPolicy", "Bypass",
                        "-Scope", "Process;",
                        path,
                        video,
                        SaveData.saveValues["VideoWidth"],
                        SaveData.saveValues["VideoHeight"],
                        @".\temp\",
                        @".\ffmpeg.exe",
                        @".\ffprobe.exe",
                        "magick",
                        @".\library\", // legacy resources folder
                        @".\" +Path.Join("library", "audio", "sfx") + @"\",
                        @".\" +Path.Join("library", "videos", "transitions") + @"\",
                        @".\" +Path.Join("library", "audio", "music") + @"\",
                        @".\library\",
                        SaveData.saveFileName, // powershell plugins can access the JSON save file
                        settings.Count.ToString(), // number of settings
                    };
                    // Add settings to args
                    foreach (KeyValuePair<string, object> setting in settings)
                    {
                        psArgs.Add(setting.Key);
                        psArgs.Add(setting.Value.ToString());
                    }
                    string psArgsString = string.Join(" ", psArgs);
                    ProcessStartInfo psStartInfo = new()
                    {
                        FileName = "powershell",
                        Arguments = psArgsString,
                        WorkingDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    Process psProcess = new()
                    {
                        StartInfo = psStartInfo
                    };
                    psProcess.OutputDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.LightBlue);
                    };
                    psProcess.ErrorDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                    };
                    psProcess.Start();
                    psProcess.BeginOutputReadLine();
                    psProcess.BeginErrorReadLine();
                    psProcess.WaitForExit();
                    if (psProcess.ExitCode == 0)
                    {
                        return new PluginReturnValue(true, Path.GetFileName(path));
                    }
                    return new PluginReturnValue(false, Path.GetFileName(path));
                default:
                    return new PluginReturnValue(false, Path.GetFileName(path));
            }
        }
        public bool Query()
        {
            // Query for batch plugins is deprecated.
            if (type == PluginType.Batch)
            {
                return true;
            }
            // Call plugin with query argument.
            string fileName = path;
            List<string> batchArgs = new();
            if(type == PluginType.PowerShell)
            {
                fileName = "powershell";
                batchArgs.Add("Set-ExecutionPolicy");
                batchArgs.Add("Bypass");
                batchArgs.Add("-Scope");
                batchArgs.Add("Process;");
                batchArgs.Add(path);
            }
            batchArgs.Add("query");
            string batchArgsString = string.Join(" ", batchArgs);
            ProcessStartInfo batchStartInfo = new()
            {
                FileName = fileName,
                Arguments = batchArgsString,
                WorkingDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            Process batchProcess = new()
            {
                StartInfo = batchStartInfo
            };
            string output = "";
            batchProcess.OutputDataReceived += (sender, e) =>
            {
                output += e.Data;
            };
            batchProcess.ErrorDataReceived += (sender, e) =>
            {
                ConsoleOutput.WriteLine(e.Data, Color.Transparent);
            };
            batchProcess.Start();
            batchProcess.BeginOutputReadLine();
            batchProcess.BeginErrorReadLine();
            batchProcess.WaitForExit();
            if (batchProcess.ExitCode != 0)
            {
                ConsoleOutput.WriteLine(output, Color.Red);
                return false;
            }
            // Parse output.
            string[] outputarray = output.Replace("\r", "").Replace("\n", "").Split("|");
            if(outputarray.Length < 1)
            {
                return true; // no query results
            }
            string[] query = outputarray[0].Split(';');
            int count = 0;
            for(int i = 0; i < query.Length; i++)
            {
                // First character is either a 0 or 1 for video/audio
                // Second character is a colon for separation
                // Rest is the pretty name, then a colon, then the base name.
                string[] split = query[i].Split(':');
                if (split.Length < 3)
                {
                    continue;
                }
                LibraryRootType rootType = split[0] == "0" ? LibraryRootType.Video : LibraryRootType.Audio;
                // split[3] is description
                string description = split.Length >= 4 ? split[3].Replace("_", " ") : "";
                LibraryType dummyType = new(rootType, split[2].Replace("_", ""), description);
                string libPath = Path.Join(rootType == LibraryRootType.Video ? "video" : "audio", split[2]);
                string[] fileExts = LibraryData.libraryFileTypes[rootType == LibraryRootType.Video ? DefaultLibraryTypes.Video : DefaultLibraryTypes.Audio];
                DefaultLibraryTypes.AllTypes.Add(dummyType);
                LibraryData.libraryPaths.Add(dummyType, libPath);
                LibraryData.libraryFileTypes.Add(dummyType, fileExts);
                LibraryData.libraryNames.Add(dummyType, split[1].Replace("_", " "));
                libraryTypes.Add(dummyType);
                Global.justCompletedRender = true; // demand a refresh
                // Print to console.
                ConsoleOutput.WriteLine($"Added {(rootType == LibraryRootType.Video ? "video" : "audio")} library {split[1]} from plugin {Path.GetFileName(path)}.", Color.LightBlue);
                count++;
            }
            // Print count
            if (count > 0)
                ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} added {count} libraries.", Color.LightBlue);
            // Library query successful, check for settings query.
            if (outputarray.Length < 2)
            {
                return true; // no settings query results
            }
            // Format: setting_name:default_setting_value:tooltip;setting_name:default_setting_value:tooltip
            string[] settingsQuery = outputarray[1].Split(';');
            count = 0;
            for(int i = 0; i < settingsQuery.Length; i++)
            {
                string[] split = settingsQuery[i].Split(':');
                if (split.Length < 2)
                {
                    continue;
                }
                string settingName = split[0];
                string settingValue = split[1];
                string settingTooltip = split.Length >= 3 ? split[2] : "";
                settings.Add(settingName, settingValue);
                settingTooltips.Add(settingName, settingTooltip);
                count++;
            }
            // Print count
            if (count > 0)
                ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} added {count} settings.", Color.LightBlue);
            return true;
        }
    }
    /// <summary>
    /// Plugin support.
    /// </summary>
    public static class PluginHandler
    {
        public static List<Plugin> plugins = new();
        private static string pluginPath = @".\plugins";
        private static string pluginSettingsPath = @".\PluginSettings.json";
        public static void LoadPluginSettings()
        {
            if (!File.Exists(pluginSettingsPath))
            {
                // Create empty settings file.
                File.WriteAllText(pluginSettingsPath, "{}");
            }
            // {"pluginname.ps1": {"settings": {"settingname": "settingvalue"}, "disabled": false}}
            Dictionary<string, Dictionary<string, object>>? pluginSettings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(pluginSettingsPath));
            if (pluginSettings == null)
            {
                // Delete invalid settings file.
                File.Delete(pluginSettingsPath);
                LoadPluginSettings();
                return;
            }
            foreach(KeyValuePair<string, Dictionary<string, object>> pluginSetting in pluginSettings)
            {
                string pluginName = pluginSetting.Key;
                int index = plugins.FindIndex(plugin => Path.GetFileName(plugin.path) == pluginName);
                if (index == -1)
                    continue;
                Plugin plugin = plugins[index];
                Dictionary<string, object>? settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(pluginSetting.Value["settings"].ToString());
                foreach(KeyValuePair<string, object> setting in settings)
                {
                    ConsoleOutput.WriteLine($"Setting {setting.Key} to {setting.Value} for plugin {pluginName}...", Color.LightBlue);
                    if(!plugin.settings.ContainsKey(setting.Key))
                    {
                        ConsoleOutput.WriteLine($"Setting {setting.Key} not found for plugin {pluginName}.", Color.Red);
                        continue;
                    }
                    plugin.settings[setting.Key] = setting.Value;
                }
                plugin.enabled = !(pluginSetting.Value["disabled"] as bool? ?? false);
            }
            SavePluginSettings();
        }
        public static void SavePluginSettings()
        {
            Dictionary<string, Dictionary<string, object>> pluginSettings = new();
            foreach(Plugin plugin in plugins)
            {
                Dictionary<string, object> pluginSetting = new();
                pluginSetting.Add("settings", plugin.settings);
                pluginSetting.Add("disabled", !plugin.enabled);
                pluginSettings.Add(Path.GetFileName(plugin.path), pluginSetting);
            }
            File.WriteAllText(pluginSettingsPath, JsonConvert.SerializeObject(pluginSettings, Formatting.Indented));
        }
        public static void LoadPlugin(string path, PluginType type)
        {
            Plugin plugin = new(path, type);
            Global.generatorFactory.progressText = $"Loading plugin {Path.GetFileName(path)}...";
            if(!plugin.Query())
                throw new Exception($"Failed to query plugin {Path.GetFileName(path)}.");
            plugins.Add(plugin);
            // Add entry to pluginsettings if it doesn't exist.
            Dictionary<string, object> pluginSettings = new();
            pluginSettings.Add("settings", plugin.settings);
            pluginSettings.Add("disabled", !plugin.enabled);
            if (!File.Exists(pluginSettingsPath))
            {
                // Create empty settings file.
                File.WriteAllText(pluginSettingsPath, "{}");
            }
            Dictionary<string, Dictionary<string, object>>? existingPluginSettings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(pluginSettingsPath));
            if (existingPluginSettings != null)
            {
                if (!existingPluginSettings.ContainsKey(Path.GetFileName(path)))
                {
                    existingPluginSettings.Add(Path.GetFileName(path), pluginSettings);
                    File.WriteAllText(pluginSettingsPath, JsonConvert.SerializeObject(existingPluginSettings, Formatting.Indented));
                }
            }
            ConsoleOutput.WriteLine($"Loaded plugin {Path.GetFileName(path)}.", Color.LightBlue);
        }
        private static void LoadPluginsRecursive(string path, PluginType type)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                switch(type)
                {
                    case PluginType.Batch:
                        // Delete *arta*emix.bat if it exists.
                        Regex regex = new(@"arta.*emix.bat");
                        if (regex.IsMatch(file))
                        {
                            File.Delete(file);
                            continue;
                        }
                        if (file.EndsWith(".bat"))
                        {
                            LoadPlugin(file, type);
                        }
                        break;
                    case PluginType.PowerShell:
                        if (file.EndsWith(".ps1"))
                        {
                            LoadPlugin(file, type);
                        }
                        break;
                }
            }
            // Recurse into subdirectories.
            foreach(string file in Directory.GetDirectories(path))
            {
                LoadPluginsRecursive(file, type);
            }
        }
        public static bool LoadPlugins()
        {
            // Clear plugins.
            plugins.Clear();
            // Create plugin directory if it doesn't exist.
            if(Directory.Exists(pluginPath) == false)
            {
                Directory.CreateDirectory(pluginPath);
            }
            ConsoleOutput.WriteLine($"Searching for plugins in {pluginPath}...", Color.LightBlue);
            List<string> pluginDirs = new()
            {
                "bat",
                "ps1"
            };
            // Create plugin subdirectories if they don't exist.
            foreach(string subdir in pluginDirs)
            {
                if(Directory.Exists(Path.Combine(pluginPath, subdir)) == false)
                {
                    Directory.CreateDirectory(Path.Combine(pluginPath, subdir));
                }
            }
            try
            {
                int count = 0;
                // Load from plugin path using subdirectories for each plugin type.
                foreach (string file in Directory.GetDirectories(pluginPath))
                {
                    string dirName = Path.GetFileName(file);
                    PluginType type = PluginType.None;
                    switch (dirName)
                    {
                        case "bat":
                            type = PluginType.Batch;
                            break;
                        case "ps1":
                            type = PluginType.PowerShell;
                            break;
                    }
                    if (type == PluginType.None)
                        continue;
                    ConsoleOutput.WriteLine($"Loading {dirName} plugins...", Color.LightBlue);
                    LoadPluginsRecursive(file, type);
                    count += plugins.Count;
                }
                LoadPluginSettings();
                Global.generatorFactory.progressText = $"{count} plugins, check console for details.";
                Global.canRender = true;
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine($"Error loading plugins: {e.Message}", Color.Red);
                return false;
            }
            return true;
        }
        public static PluginReturnValue PickRandom(Random rnd, string video)
        {
            if(plugins.Count == 0)
            {
                return new PluginReturnValue()
                {
                    success = false,
                    pluginName = "",
                };
            }
            // Pick a random plugin that isn't disabled.
            List<Plugin> enabledPlugins = plugins.FindAll(plugin => plugin.enabled);
            if(enabledPlugins.Count == 0)
            {
                return new PluginReturnValue()
                {
                    success = false,
                    pluginName = "",
                };
            }
            Plugin plugin = enabledPlugins[rnd.Next(enabledPlugins.Count)];
            // Call the plugin.
            return plugin.Call(video);
        }
        public static int GetPluginCount()
        {
            return plugins.Count;
        }
    }
}