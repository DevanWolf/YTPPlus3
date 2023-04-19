using System;
using System.Collections.Generic;
using System.IO;

namespace YTPPlusPlusPlus
{    
    /// <summary>
    /// Handle static console input/output.
    /// </summary>
    public static class ConsoleOutput
    {
        public static readonly List<string> output = new List<string>();
        private static readonly int maxLines = 25;
        private static readonly int lineLength = 70;
        public static void WriteLine(string line)
        {
            // Wrap lines.
            int lineCount = line.Length / lineLength;
            if (line.Length % lineLength > 0)
                lineCount++;
            for (int i = 0; i < lineCount; i++)
            {
                int start = i * lineLength;
                int end = (i + 1) * lineLength;
                if (end > line.Length)
                    end = line.Length;
                output.Add(line.Substring(start, end - start));
            }
            // Remove old lines.
            while (output.Count > maxLines)
                output.RemoveAt(0);
            // DEBUG: Write to file.
            try
            {
                using (StreamWriter writer = new StreamWriter("console.txt", true))
                {
                    writer.WriteLine(line);
                }
            }
            catch
            {
            }
        }
        public static void Clear()
        {
            output.Clear();
            // DEBUG: Delete file.
            try
            {
                File.Delete("console.txt");
            }
            catch
            {
            }
        }
    }
}