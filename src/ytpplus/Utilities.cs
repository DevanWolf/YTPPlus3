using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace YTPPlusPlusPlus
{
    public static class Utilities
    {
        public static string ffmpegLocation = "ffmpeg";
        public static string ffprobeLocation = "ffprobe";
        public static string temporaryDirectory = @".\temp";
        public static string GetLength(string file)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffprobeLocation;
                startInfo.Arguments = "-i \"" + file
                        + "\" -show_entries format=duration"
                        + " -v quiet"
                        + " -of csv=\"p=0\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();
                string s = "";
                // Read stdout synchronously (on this thread)

                while (true)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (line == null)
                        break;
                    //ConsoleOutput.WriteLine(line);
                    s = line;
                }

                process.WaitForExit();
                //ConsoleOutput.WriteLine(s);
                return s;

            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while getting length of video.");
                Global.generatorFactory.failureReason = "Fatal error while getting length of video.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
                return "0";
            }
        }

        /**
         * Snip a video file between the start and end time, and save it to an output file.
         *
         * @param video input video filename to work with
         * @param startTime start time (in TimeStamp format, e.g. new TimeStamp(seconds);)
         * @param endTime start time (in TimeStamp format, e.g. new TimeStamp(seconds);)
         * @param output output video filename to save the snipped clip to
         */
        public static void SnipVideo(string video, double startTime, double endTime, string output)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = "-i \"" + video
                        + "\" -ss " + startTime.ToString("0.#########################", new CultureInfo("en-US"))
                        + " -to " + endTime.ToString("0.#########################", new CultureInfo("en-US"))
                        + " -ac 1"
                        + " -ar 44100"
                        + " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30"
                        + " -y"
                        + " \"" + output + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                if (process.HasExited && process.ExitCode == 1)
                {
                    //ConsoleOutput.WriteLine("ERROR");
                }
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while snipping video.");
                Global.generatorFactory.failureReason = "Fatal error while snipping video.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
            }
        }

        /**
         * Copies a video and encodes it in the proper format without changes.
         *
         * @param video input video filename to work with
         * @param output output video filename to save the snipped clip to
         */
        public static void CopyVideo(string video, string output)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = "-i \"" + video
                        + "\" -ar 44100"
                        + " -ac 1"
                        //+ " -filter:v fps=fps=30,setsar=1:1"
                        + " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30"
                        + " -y"
                        + " \"" + output + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while copying video.");
                Global.generatorFactory.failureReason = "Fatal error while copying video.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
            }
        }

        /**
         * Concatenate videos by count
         *
         * @param count number of input videos to concatenate
         * @param out output video filename
         */
        public static void ConcatenateVideo(int count, string ou)
        {
            try
            {
                if (File.Exists(ou))
                    File.Delete(ou);

                string command1 = "";

                for (int i = 0; i < count; i++)
                {
                    if (File.Exists(Path.Combine(temporaryDirectory, "video" + i + ".mp4")))
                    {
                        command1 += (" -i " + Path.Combine(temporaryDirectory, "video" + i + ".mp4"));
                    }
                }
                command1 += (" -filter_complex \"");

                int realcount = 0;
                for (int i = 0; i < count; i++)
                {
                    if (File.Exists(Path.Combine(temporaryDirectory, "video" + i + ".mp4")))
                    {
                        realcount += 1;
                    }
                }
                for (int i = 0; i < realcount; i++)
                {
                    command1 += ("[" + i + ":v:0][" + i + ":a:0]");
                }

                //realcount +=1;
                command1 += ("concat=n=" + realcount + ":v=1:a=1[outv][outa]\" -map \"[outv]\" -map \"[outa]\" -y " + "\"" + ou + "\"");

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = command1;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                //cmdLine = CommandLine.parse(command2);
                //executor = new DefaultExecutor();
                //exitValue = executor.execute(cmdLine);

                //temp.delete();

            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Trying a different method of concatenation.");
                Global.generatorFactory.progressText = "Trying a different method of concatenation.";
                try
                {
                    // Use concat filter instead
                    string list = "";
                    for (int i = 0; i < count; i++)
                    {
                        if (File.Exists(Path.Combine(temporaryDirectory, "video" + i + ".mp4")))
                        {
                            list += "file 'video" + i + ".mp4'\r\n";
                        }
                    }
                    // Write to concat.txt
                    File.WriteAllText(Path.Combine(temporaryDirectory, "concat.txt"), list);
                    // Run ffmpeg
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = ffmpegLocation;
                    startInfo.Arguments = "-f concat -safe 0 -i \"" + Path.Combine(temporaryDirectory, "concat.txt")
                            + "\" -c copy -y"
                            + " \"" + ou + "\"";
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    startInfo.CreateNoWindow = true;
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
                catch(Exception ex2)
                {
                    ConsoleOutput.WriteLine(ex2.Message);
                    ConsoleOutput.WriteLine("Fatal error while concatenating videos.");
                    Global.generatorFactory.failureReason = "Fatal error while concatenating videos.";
                    Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                    Global.generatorFactory.CancelGeneration();
                }
            }
        }
        /// <summary>
        /// Overlay a video on top of another video, using chroma key.
        /// </summary>
        /// <param name="video">The video to overlay.</param>
        /// <param name="overlay">The video to overlay on top of the first video.</param>
        public static void OverlayVideo(string video, string overlay)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                string overlayed_video = video.Replace(".mp4", "_chromakey.mp4");
                ConsoleOutput.WriteLine("Overlaying video " + video + " with " + overlay);
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = "-i \"" + video
                        + "\" -i \"" + overlay
                        + "\" -filter_complex \"[1:v]colorkey=0x00FF00:0.3:0.2,scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30[outv];[0:v][outv]overlay=shortest=1[finalv];[0:a][1:a]amerge[finala]\" -map \"[finalv]\" -map \"[finala]\" -y \"" + overlayed_video + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                // Rename the temporary file to the original file
                File.Delete(video);
                File.Move(overlayed_video, video);
                File.Delete(overlay);
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while overlaying video.");
                Global.generatorFactory.failureReason = "Fatal error while overlaying video.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
            }
        }
    }
}