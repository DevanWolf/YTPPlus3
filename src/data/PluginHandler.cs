using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Linq;

namespace YTPPlusPlusPlus
{
    public enum PluginType
    {
        None,
        Python,
        NodeJS,
        Batch
    }
    public class Plugin
    {
        public string path { get; set; }
        public PluginType type { get; set; }
        public bool enabled { get; set; }
        public Plugin(string path, PluginType type, bool enabled = true)
        {
            this.path = path;
            this.type = type;
            this.enabled = enabled;
        }
        public bool Call(string video)
        {
            if (enabled == false)
            {
                return false;
            }
            ConsoleOutput.WriteLine($"Calling plugin {Path.GetFileName(path)}");
            switch (type)
            {
                case PluginType.NodeJS:
                    // node handler is .\plugins\plugininterface.js
                    Dictionary<string, string> args = new()
                    {
                        { "video", Path.GetFileName(video) },
                        { "plugin", Path.GetFileName(path) },
                        { "clips", SaveData.saveValues["MaxClipCount"] },
                        { "minstream", SaveData.saveValues["MinStreamDuration"] },
                        { "maxstream", SaveData.saveValues["MaxStreamDuration"] },
                        { "width", SaveData.saveValues["VideoWidth"] },
                        { "height", SaveData.saveValues["VideoHeight"] },
                        { "fps", "30" },
                        { "usetransitions", SaveData.saveValues["TransitionsEnabled"] == "true" ? "1" : "0" },
                    };
                    string argsString = string.Join(" ", args.Select(x => $"--{x.Key} {x.Value}"));
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = "node",
                        Arguments = $"plugininterface.js {argsString}",
                        WorkingDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugins"),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    Process process = new()
                    {
                        StartInfo = startInfo
                    };
                    process.Start();
                    process.WaitForExit();
                    return true;
                case PluginType.Batch:
                    // Batch plugins are the simplest.
                    List<string> batchArgs = new()
                    {
                        video,
                        SaveData.saveValues["VideoWidth"],
                        SaveData.saveValues["VideoHeight"],
                        @".\temp",
                        "ffmpeg",
                        "ffprobe",
                        "magick",
                        Path.Join("library", "resources"), // legacy resources folder
                        Path.Join("library", "audio", "sfx"),
                        Path.Join("library", "videos", "transitions"),
                        Path.Join("library", "audio", "music"),
                    };
                    string batchArgsString = string.Join(" ", batchArgs);
                    ProcessStartInfo batchStartInfo = new()
                    {
                        FileName = path,
                        Arguments = batchArgsString,
                        WorkingDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    Process batchProcess = new()
                    {
                        StartInfo = batchStartInfo
                    };
                    batchProcess.Start();
                    batchProcess.WaitForExit();
                    return true;
                case PluginType.Python:
                    // Python plugins take full advantage of YTP+++ features.
                    // The only argument is the video path.
                    // Temporarily add plugins/py/lib to path.
                    string pythonPath = Environment.GetEnvironmentVariable("PYTHONPATH");
                    Environment.SetEnvironmentVariable("PYTHONPATH", Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugins", "py", "lib"));
                    ProcessStartInfo pythonStartInfo = new()
                    {
                        FileName = "python3",
                        Arguments = $".\\py\\{Path.GetFileName(path)} generate \"{Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), video)}\"",
                        WorkingDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugins"),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    };
                    Process pythonProcess = new()
                    {
                        StartInfo = pythonStartInfo
                    };
                    pythonProcess.Start();
                    pythonProcess.WaitForExit();
                    // Remove plugins/py/lib from path.
                    Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath);
                    return true;
                default:
                    return false;
            }
        }
        public bool Query()
        {
            // Only Python plugins can query.
            if (type != PluginType.Python)
            {
                return true; // Don't error out if the plugin isn't Python.
            }
            // Temporarily add plugins/py/lib to path.
            string pythonPath = Environment.GetEnvironmentVariable("PYTHONPATH");
            Environment.SetEnvironmentVariable("PYTHONPATH", Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugins", "py", "lib"));
            // Capture output from plugin.
            ProcessStartInfo pythonStartInfo = new()
            {
                FileName = "python3",
                Arguments = $".\\py\\{Path.GetFileName(path)} query",
                WorkingDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugins"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            Process pythonProcess = new()
            {
                StartInfo = pythonStartInfo
            };
            pythonProcess.Start();
            string output = pythonProcess.StandardOutput.ReadToEnd();
            pythonProcess.WaitForExit();
            // Check for errors.
            if (pythonProcess.ExitCode != 0)
            {
                ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} returned an error code.");
                for (int i = 0; i < output.Split('\n').Length; i++)
                {
                    ConsoleOutput.WriteLine(output.Split('\n')[i]);
                }
                return false;
            }
            // Remove plugins/py/lib from path.
            Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath);
            // Parse output.
            output = output.Replace("\r", "").Replace("\n", "");
            string[] query = output.Split(';');
            int count = 0;
            for(int i = 0; i < query.Length; i++)
            {
                // First character is either a 0 or 1 for video/audio
                // Second character is a colon for separation
                // Rest is the pretty name, then a colon, then the base name.
                string[] split = query[i].Split(':');
                if (split.Length != 3)
                {
                    continue;
                }
                LibraryRootType rootType = split[0] == "0" ? LibraryRootType.Video : LibraryRootType.Audio;
                LibraryType dummyType = new(rootType, split[2]);
                string libPath = Path.Join("library", rootType == LibraryRootType.Video ? "video" : "audio", split[2]);
                string[] fileExts = LibraryData.libraryFileTypes[rootType == LibraryRootType.Video ? DefaultLibraryTypes.Video : DefaultLibraryTypes.Audio];
                DefaultLibraryTypes.AllTypes.Add(dummyType);
                LibraryData.libraryPaths.Add(dummyType, libPath);
                LibraryData.libraryFileTypes.Add(dummyType, fileExts);
                LibraryData.libraryNames.Add(dummyType, split[1].Replace("_", " "));
                // Print to console.
                ConsoleOutput.WriteLine($"Added {(rootType == LibraryRootType.Video ? "video" : "audio")} library {split[1]} from plugin {Path.GetFileName(path)}.");
                count++;
            }
            // Print count
            if (count > 0)
                ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} added {count} libraries.");
            return true;
        }
    }
    /// <summary>
    /// Python, Node.JS, and Batch plugin support.
    /// </summary>
    public static class PluginHandler
    {
        private readonly static List<Plugin> plugins = new();
        private static string pluginPath = @".\plugins";
        public static void LoadPlugin(string path, PluginType type)
        {
            Plugin plugin = new(path, type);
            if(!plugin.Query())
                throw new Exception($"Failed to query plugin {Path.GetFileName(path)}.");
            plugins.Add(plugin);
            ConsoleOutput.WriteLine($"Loaded plugin {Path.GetFileName(path)}.");
        }
        private static void LoadPluginsRecursive(string path, PluginType type)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                switch(type)
                {
                    case PluginType.Python:
                        // Python plugins can be .py or a subdir with __init__.py.
                        // Skip lib folder
                        if (path.Contains("/lib/") || path.Contains("\\lib\\"))
                        {
                            break;
                        }
                        else if(Path.GetFileNameWithoutExtension(file) == "__init__")
                        {
                            LoadPlugin(Path.GetDirectoryName(file), type);
                        }
                        else if (file.EndsWith(".py"))
                        {
                            LoadPlugin(file, type);
                        }
                        break;
                    case PluginType.NodeJS:
                        if (file.EndsWith(".js"))
                        {
                            LoadPlugin(file, type);
                        }
                        break;
                    case PluginType.Batch:
                        if (file.EndsWith(".bat"))
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
            // Create plugin directory if it doesn't exist.
            if(Directory.Exists(pluginPath) == false)
            {
                Directory.CreateDirectory(pluginPath);
            }
            ConsoleOutput.WriteLine($"Searching for plugins in {pluginPath}...");
            List<string> pluginDirs = new()
            {
                "py",
                "js",
                "bat"
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
                // Load from plugin path using subdirectories for each plugin type.
                foreach (string file in Directory.GetDirectories(pluginPath))
                {
                    string dirName = Path.GetFileName(file);
                    PluginType type = PluginType.None;
                    switch (dirName)
                    {
                        case "py":
                            type = PluginType.Python;
                            break;
                        case "js":
                            type = PluginType.NodeJS;
                            break;
                        case "bat":
                            type = PluginType.Batch;
                            break;
                    }
                    if (type == PluginType.None)
                        continue;
                    ConsoleOutput.WriteLine($"Loading {dirName} plugins...");
                    LoadPluginsRecursive(file, type);
                }
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine($"Error loading plugins: {e.Message}");
                return false;
            }
            return true;
        }
        public static bool PickRandom(Random rnd, string video)
        {
            // Pick a random plugin.
            Plugin plugin = plugins[rnd.Next(plugins.Count)];
            // Call the plugin.
            return plugin.Call(video);
        }
        public static bool PickNamed(string name, string video)
        {
            // Find the plugin.
            Plugin? plugin = plugins.Find(x => Path.GetFileName(x.path) == name);
            if (plugin == null)
            {
                return false;
            }
            // Call the plugin.
            return plugin.Call(video);
        }
        public static int GetPluginCount()
        {
            return plugins.Count;
        }
    }
}