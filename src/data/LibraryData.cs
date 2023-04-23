using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace YTPPlusPlusPlus
{
    public enum LibraryRootType
    {
        /// <summary>
        /// All files.
        /// </summary>
        All = 0,
        /// <summary>
        /// Video files.
        /// </summary>
        Video,
        /// <summary>
        /// Audio files.
        /// </summary>
        Audio,
    }
    public enum LibraryFileType
    {
        /// <summary>
        /// All files.
        /// </summary>
        All = 0,
        /// <summary>
        /// Sound effects.
        /// </summary>
        SFX,
        /// <summary>
        /// Music.
        /// </summary>
        Music,
        /// <summary>
        /// Rendered videos.
        /// </summary>
        Render,
        /// <summary>
        /// Material videos.
        /// </summary>
        Material,
        /// <summary>
        /// Transition videos.
        /// </summary>
        Transition,
        /// <summary>
        /// Intro videos.
        /// </summary>
        Intro,
        /// <summary>
        /// Outro videos.
        /// </summary>
        Outro,
        /// <summary>
        /// Overlay videos.
        /// </summary>
        Overlay,
        /// <summary>
        /// Custom library type.
        /// </summary>
        Custom,
    }
    public class LibraryType
    {
        public LibraryRootType RootType { get; set; }
        public LibraryFileType FileType { get; set; }
        public bool Special { get; set; } = false;
        public string CustomName { get; set; } = "";
        public LibraryType(LibraryRootType rootType, LibraryFileType fileType, bool special = false)
        {
            RootType = rootType;
            FileType = fileType;
            Special = special;
        }
        public LibraryType(LibraryRootType rootType, string customName)
        {
            RootType = rootType;
            FileType = LibraryFileType.Custom;
            Special = false;
            CustomName = customName;
        }
    }
    public static class DefaultLibraryTypes
    {
        public static LibraryType All { get; } = new LibraryType(LibraryRootType.All, LibraryFileType.All, true);
        public static LibraryType Video { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.All, true);
        public static LibraryType Audio { get; } = new LibraryType(LibraryRootType.Audio, LibraryFileType.All, true);
        public static LibraryType SFX { get; } = new LibraryType(LibraryRootType.Audio, LibraryFileType.SFX);
        public static LibraryType Music { get; } = new LibraryType(LibraryRootType.Audio, LibraryFileType.Music);
        public static LibraryType Render { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Render);
        public static LibraryType Material { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Material);
        public static LibraryType Transition { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Transition);
        public static LibraryType Intro { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Intro);
        public static LibraryType Outro { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Outro);
        public static LibraryType Overlay { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Overlay);
        public static List<LibraryType> AllTypes { get; } = new List<LibraryType>()
        {
            All,
            Video,
            Audio,
            SFX,
            Music,
            Render,
            Material,
            Transition,
            Intro,
            Outro,
            Overlay,
        };
    }
    public class LibraryFile
    {
        public string? Nickname { get; set; }
        public string? Path { get; set; }
        public LibraryType? Type { get; set; }
        public bool Enabled { get; set; } = true;
        public LibraryFile(string? nickname, string? path, LibraryType? type, bool enabled = true)
        {
            Nickname = nickname;
            Path = path;
            Type = type;
            Enabled = enabled;
        }
    }
    public static class LibraryData
    {
        public static List<LibraryFile> libraryFiles { get; } = new List<LibraryFile>();
        public static string libraryRootPath { get; set; } = @".\library";
        public static Dictionary<LibraryType, string> libraryPaths { get; } = new Dictionary<LibraryType, string>()
        {
            { DefaultLibraryTypes.All, @"" },
            { DefaultLibraryTypes.Video, @"video" },
            { DefaultLibraryTypes.Audio, @"audio" },
            { DefaultLibraryTypes.SFX, @"audio\sfx" },
            { DefaultLibraryTypes.Music, @"audio\music" },
            { DefaultLibraryTypes.Render, @"video\renders" },
            { DefaultLibraryTypes.Material, @"video\materials" },
            { DefaultLibraryTypes.Transition, @"video\transitions" },
            { DefaultLibraryTypes.Intro, @"video\intros" },
            { DefaultLibraryTypes.Outro, @"video\outros" },
            { DefaultLibraryTypes.Overlay, @"video\overlays" },
        };
        public static Dictionary<LibraryType, string[]> libraryFileTypes { get; } = new Dictionary<LibraryType, string[]>()
        {
            { DefaultLibraryTypes.All, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv", ".wav", ".mp3", ".ogg", ".m4a" } },
            { DefaultLibraryTypes.Video, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Audio, new string[] { ".wav", ".mp3", ".ogg", ".m4a" } },
            { DefaultLibraryTypes.SFX, new string[] { ".wav", ".mp3", ".ogg", ".m4a" } },
            { DefaultLibraryTypes.Music, new string[] { ".wav", ".mp3", ".ogg", ".m4a" } },
            { DefaultLibraryTypes.Material, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Transition, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Intro, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Outro, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Overlay, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Render, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
        };
        public static Dictionary<LibraryType, string> libraryNames { get; } = new Dictionary<LibraryType, string>()
        {
            { DefaultLibraryTypes.All, "All" },
            { DefaultLibraryTypes.Video, "Video" },
            { DefaultLibraryTypes.Audio, "Audio" },
            { DefaultLibraryTypes.SFX, "Sound FX" },
            { DefaultLibraryTypes.Music, "Music" },
            { DefaultLibraryTypes.Render, "Renders" },
            { DefaultLibraryTypes.Material, "Materials" },
            { DefaultLibraryTypes.Transition, "Transitions" },
            { DefaultLibraryTypes.Intro, "Intros" },
            { DefaultLibraryTypes.Outro, "Outros" },
            { DefaultLibraryTypes.Overlay, "Overlays" },
        };
        private static void LoadRecursive(string path, LibraryType type)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                foreach (string filetype in libraryFileTypes[type])
                {
                    if (file.EndsWith(filetype))
                    {
                        libraryFiles.Add(new LibraryFile(Path.GetFileNameWithoutExtension(file), file, type));
                        break;
                    }
                }
            }
            foreach (string dir in Directory.GetDirectories(path))
                LoadRecursive(dir, type);
        }
        public static void Load()
        {
            libraryFiles.Clear();
            foreach (LibraryType type in libraryPaths.Keys)
            {
                if(type.Special)
                    continue; // Skip special types for now.
                string path = Path.Combine(libraryRootPath, libraryPaths[type]);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                LoadRecursive(path, type);
            }
        }
        public static LibraryFile? Load(LibraryFile file)
        {
            // Import the library file by copying it.
            if(file.Type == null)
            {
                ConsoleOutput.WriteLine("Cannot import library file: type is null.", Color.Red);
                return null;
            }
            string newpath = Path.Combine(libraryRootPath, libraryPaths[file.Type]);
            if (!Directory.Exists(newpath))
                Directory.CreateDirectory(newpath);
            string newfile = Path.Combine(newpath, file.Nickname + Path.GetExtension(file.Path));
            if(file.Path == null)
            {
                ConsoleOutput.WriteLine("Cannot import library file: path is null.", Color.Red);
                return null;
            }
            // Check to make sure the file isn't already in the library.
            foreach(LibraryFile libfile in libraryFiles)
            {
                if(libfile.Path == newfile)
                {
                    ConsoleOutput.WriteLine("Deleting existing library file", Color.Yellow);
                    File.Delete(libfile.Path);
                }
            }
            File.Copy(file.Path, newfile);
            file.Path = newfile;
            libraryFiles.Add(file);
            return file;
        }
        public static void Unload(LibraryFile file)
        {
            // Delete the library file.
            if(file.Path == null)
            {
                ConsoleOutput.WriteLine("Cannot delete library file: path is null.", Color.Red);
                return;
            }
            File.Delete(file.Path);
            libraryFiles.Remove(file);
        }
        public static string PickRandom(LibraryType type, Random rnd)
        {
            // Pick a random file from the library.
            List<LibraryFile> files = libraryFiles.FindAll(x => x.Type == type && x.Path != null && File.Exists(x.Path) && x.Enabled);
            if (files.Count == 0)
                return "";
            int index = rnd.Next(files.Count);
            string? path = files[index].Path;
            if(path != null)
                return path;
            return "";
        }
        public static List<LibraryFile> GetFiles(LibraryType type)
        {
            // Get all files of a certain type.
            return libraryFiles.FindAll(x => x.Type == type);
        }
        public static int GetFileCount(LibraryType type)
        {
            // Get the number of files of a certain type.
            return GetFiles(type).Count;
        }
        public static List<string> GetLibraryNames(LibraryRootType type)
        {
            List<string> names = new();
            foreach(KeyValuePair<LibraryType, string> pair in libraryNames)
            {
                if(pair.Key.Special)
                    continue;
                if(pair.Key.RootType == type || type == LibraryRootType.All)
                {
                    names.Add(pair.Value);
                }
            }
            return names;
        }
        public static LibraryFile Organize(LibraryFile source, LibraryType newType)
        {
            // Move the file to the new type's folder and re-import it.
            if(source.Type == null)
            {
                ConsoleOutput.WriteLine("Cannot organize library file: type is null.", Color.Red);
                return source;
            }
            if(source.Path == null)
            {
                ConsoleOutput.WriteLine("Cannot organize library file: path is null.", Color.Red);
                return source;
            }
            string newpath = Path.Combine(libraryRootPath, libraryPaths[newType]);
            string finalpath = Path.Combine(newpath, Path.GetFileName(source.Path));
            File.Move(source.Path, finalpath);
            // File is now in the new folder, so we can re-import it.
            bool removed = libraryFiles.Remove(source);
            source.Path = finalpath;
            source.Type = newType;
            if(removed)
                libraryFiles.Add(source);
            else
                ConsoleOutput.WriteLine("Organized file was not found, so it was not re-added.", Color.Yellow);
            return source;
        }
    }
}
