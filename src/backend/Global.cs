using System.Diagnostics;
using System.Reflection;

namespace YTPPlusPlusPlus
{
    /// <summary>
    /// This class stores useful global variables and functions.
    /// </summary>
    public static class Global
    {
        public static Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        public static FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        public static string? productName = fileVersionInfo.ProductName;
        public static string? productVersion = fileVersionInfo.ProductVersion;
        public static Mask mask = new();
        public static bool justCompletedRender = false;
        public static bool exiting = false;
        public static bool shuffled = false;
        public static bool pluginsLoaded = false;
        // YTP+ variables.
        public static GeneratorFactory generatorFactory = new GeneratorFactory();
    }
}