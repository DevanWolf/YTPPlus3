using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;

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
                return s;

            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while getting length of video.", Color.Red);
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
            SnipVideo(video, startTime.ToString(CultureInfo.InvariantCulture), endTime.ToString(CultureInfo.InvariantCulture), output);
        }
        public static void SnipVideo(string video, string startTime, string endTime, string output)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = "-i \"" + video
                        + "\" -ss " + startTime
                        + " -to " + endTime
                        + " -ac 1"
                        + " -ar 44100"
                        + " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30"
                        + " -y"
                        + " \"" + output + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.HasExited && process.ExitCode == 1)
                {
                    //ConsoleOutput.WriteLine("ERROR");
                }
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while snipping video.", Color.Red);
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
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while copying video.", Color.Red);
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
        public static void ConcatenateVideo(int count, string ou, CaptionFile captions = null)
        {
            string captionsFile = Path.Combine(Path.GetDirectoryName(ou), Path.GetFileNameWithoutExtension(ou) + ".srt");
            bool stopped = false;
            try
            {
                if (File.Exists(ou))
                    File.Delete(ou);
                // delete existing srt file
                if (File.Exists(captionsFile))
                    File.Delete(captionsFile);
                if (captions != null)
                {
                    captions.Save(captionsFile);
                }
            }
            catch (Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
            }
            // try to continue anyways
            try
            {
                string command1 = "";

                for (int i = 0; i < count; i++)
                {
                    if (File.Exists(Path.Combine(temporaryDirectory, "video" + i + ".mp4")))
                    {
                        command1 += " -i " + Path.Combine(temporaryDirectory, "video" + i + ".mp4");
                    }
                }

                // import captions if they exist
                if (captions != null)
                {
                    command1 += " -sub_charenc UTF-8 -i \"" + captionsFile + "\" -c:s mov_text -metadata:s:s:0 language=eng";
                }

                command1 += " -filter_complex \"";

                int realCount = 0;
                for (int i = 0; i < count; i++)
                {
                    if (File.Exists(Path.Combine(temporaryDirectory, "video" + i + ".mp4")))
                    {
                        command1 += "[" + i + ":v:0][" + i + ":a:0]";
                        realCount += 1;
                    }
                }

                string captionMap = "";
                if (captions != null)
                {
                    captionMap = " -map " + realCount + ":s:0";
                }

                //realcount +=1;
                command1 += "concat=n=" + realCount + ":v=1:a=1[outv][outa]\" -map [outv] -map [outa]" + captionMap + " -fps_mode vfr -shortest -y \"" + ou + "\"";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = command1;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    if(e.Data != null)
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                        // Conversion failed?
                        if (e.Data.Contains("Conversion failed!"))
                        {
                            // We don't want to try to concatenate again
                            ConsoleOutput.WriteLine("Fatal error while concatenating videos.");
                            Global.generatorFactory.failureReason = "Fatal error while concatenating videos.";
                            Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                            Global.generatorFactory.CancelGeneration();
                        }
                    }
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();
                if(process.HasExited && process.ExitCode == 1)
                {
                    throw new Exception("Concatenation failed.");
                }
            }
            catch(Exception ex)
            {
                if(!stopped)
                {
                    ConsoleOutput.WriteLine(ex.Message, Color.Red);
                    ConsoleOutput.WriteLine("Trying a different method of concatenation.", Color.Yellow);
                    Global.generatorFactory.progressText = "Trying a different method of concatenation.";
                    ConcatenateVideo2(count, ou, captions);
                }
            }
        }
        public static void ConcatenateVideo2(int count, string ou, CaptionFile captions = null)
        {
            string captionsFile = Path.Combine(Path.GetDirectoryName(ou), Path.GetFileNameWithoutExtension(ou) + ".srt");
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
                string captionAppend1 = "";
                string captionAppend2 = "";
                // import captions if they exist
                if (captions != null)
                {
                    captionAppend1 = " -sub_charenc UTF-8 -i \"" + captionsFile + "\"";
                    captionAppend2 = " -c:s mov_text -metadata:s:s:0 language=eng";
                }
                startInfo.Arguments = "-f concat -safe 0 -i \"" + Path.Combine(temporaryDirectory, "concat.txt")
                        + "\"" + captionAppend1 + " -c copy -shortest -y" + captionAppend2
                        + " \"" + ou + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
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
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = "-i \"" + video
                        + "\" -i \"" + overlay
                        + "\" -filter_complex \"[1:v]colorkey=0x00FF00:0.3:0.2,scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30[outv];[0:v][outv]overlay=shortest=1[finalv];[0:a][1:a]amix=inputs=2:duration=shortest[outa]\" -map \"[finalv]\" -map \"[outa]\" -fps_mode vfr -y \"" + overlayed_video + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();

                // Rename the temporary file to the original file
                File.Delete(video);
                File.Move(overlayed_video, video);
                //File.Delete(overlay);
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Skipping overlaying video.", Color.Yellow);
                Global.generatorFactory.progressText = "Skipping overlaying video.";
            }
        }
        /// <summary>
        /// Extract an srt subtitle file from a video.
        /// </summary>
        /// <param name="video">The video to extract the srt file from.</param>
        /// <returns>The path to the srt file.</returns>
        public static void ExtractSrt(string video, string captionsFile)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = "-i \"" + video + "\" -map 0:s:0 -fps_mode vfr -y \"" + captionsFile + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Skipping extracting srt file.", Color.Yellow);
                Global.generatorFactory.progressText = "Skipping extracting srt file.";
            }
        }
        /// <summary>
        /// Turns an image file into a video zooming into a random point with music.
        /// </summary>
        /// <param name="image">The image to zoom into.</param>
        /// <param name="output">The output video.</param>
        public static float ImageVideo(string image, string output)
        {
            try
            {
                // Calculate desired aspect ratio to simplified fraction
                int gcd = (int)GCD(int.Parse(SaveData.saveValues["VideoWidth"]), int.Parse(SaveData.saveValues["VideoHeight"]));
                string aspectratio = (int.Parse(SaveData.saveValues["VideoWidth"]) / gcd) + "/" + (int.Parse(SaveData.saveValues["VideoHeight"]) / gcd);
                string music = LibraryData.PickRandom(DefaultLibraryTypes.Music, Global.generatorFactory.globalRandom);
                if(!image.EndsWith(".gif"))
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = ffmpegLocation;
                    Vector2 randomPoint = new Vector2(Global.generatorFactory.globalRandom.Next(0, int.Parse(SaveData.saveValues["VideoWidth"])), Global.generatorFactory.globalRandom.Next(0, int.Parse(SaveData.saveValues["VideoHeight"])));
                    Vector2 randomPointNormalized = new Vector2(randomPoint.X / float.Parse(SaveData.saveValues["VideoWidth"]), randomPoint.Y / float.Parse(SaveData.saveValues["VideoHeight"]));
                    int minframes = (int)(float.Parse(SaveData.saveValues["MinStreamDuration"]) * 30);
                    int maxframes = (int)(float.Parse(SaveData.saveValues["MaxStreamDuration"]) * 30);
                    int frames = Global.generatorFactory.globalRandom.Next(minframes, maxframes);
                    startInfo.Arguments = "-i \"" + image + "\"";
                    // Seek audio 1-5 seconds ahead to avoid silence at the beginning
                    string seek = " -ss 00:00:0" + Global.generatorFactory.globalRandom.Next(1, 5);
                    if(music != "")
                        startInfo.Arguments += seek+" -i \"" + music + "\"";
                    else
                        startInfo.Arguments += " -f lavfi"+seek+" -t " + (frames / 30f) + " -i anullsrc";
                    startInfo.Arguments += " -filter_complex \"[0:v]scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"]
                            + ",setsar=1:1"
                            + ",zoompan=z='min(zoom+0.0015,5)':x='if(gte(zoom,5),x,x" + (randomPointNormalized.X < 0 ? "-" : "+") + randomPointNormalized.X + "*zoom)':y='if(gte(zoom,5),y,y" + (randomPointNormalized.Y < 0 ? "-" : "+") + randomPointNormalized.Y + "*zoom)':d="+frames
                            + ",fps=fps=30[toutv]"
                            + ";[toutv]scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setdar=" + aspectratio + "[outv]\""
                            + " -map \"[outv]\""
                            + " -map 1:a"
                            + " -c:v libx264 -preset veryfast -crf 18 -c:a aac -shortest -y \"" + output + "\"";
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardError = true;
                    startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    startInfo.CreateNoWindow = true;
                    process.StartInfo = startInfo;
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                    };
                    process.Start();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
                else
                {
                    // Play the gif at its original framerate
                    // First, get the framerate
                    string framerate = "";
                    float giflength = 0;
                    try
                    {
                        using (var gif = System.Drawing.Image.FromFile(image))
                        {
                            PropertyItem item = gif.GetPropertyItem(20736);
                            int delay = (item.Value[0] + item.Value[1] * 256) * 10;
                            framerate = (1000f / delay).ToString();
                            giflength = gif.GetFrameCount(FrameDimension.Time) * delay / 1000f;
                        }
                    }
                    catch
                    {
                        framerate = "30";
                    }
                    float minlength = float.Parse(SaveData.saveValues["MinStreamDuration"]);
                    float maxlength = float.Parse(SaveData.saveValues["MaxStreamDuration"]);
                    float length = Global.generatorFactory.globalRandom.Next((int)(minlength * 30), (int)(maxlength * 30)) / 30f;
                    float gifstart = Global.generatorFactory.globalRandom.Next(0, (int)(giflength - length));
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = ffmpegLocation;
                    startInfo.Arguments = "-ignore_loop 0 -ss " + gifstart + " -t " + length + " -i \"" + image + "\"";
                    // Seek audio 1-5 seconds ahead to avoid silence at the beginning
                    string seek = " -ss 00:00:0" + Global.generatorFactory.globalRandom.Next(1, 5);
                    if (music != "")
                        startInfo.Arguments += seek + " -i \"" + music + "\"";
                    else
                        startInfo.Arguments += " -f lavfi" + seek + " -t " + (int.Parse(SaveData.saveValues["MaxStreamDuration"]) + 5) + " -i anullsrc";
                    startInfo.Arguments += " -filter_complex \"[0:v]scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"]
                            + ",setsar=1:1"
                            + ",fps=" + framerate + "[toutv]"
                            + ";[toutv]scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setdar=" + aspectratio + "[outv]\""
                            + " -map \"[outv]\""
                            + " -map 1:a"
                            + " -c:v libx264 -preset veryfast -crf 18 -c:a aac -shortest -y \"" + output + "\"";
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardError = true;
                    startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    startInfo.CreateNoWindow = true;
                    process.StartInfo = startInfo;
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                    };
                    process.Start();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Skipping image video.", Color.Yellow);
                Global.generatorFactory.progressText = "Skipping image video.";
                return -1;
            }
            return float.Parse(GetLength(output));
        }
        private static void Simplify(int[] numbers)
        {
            int gcd = GCD(numbers);
            for (int i = 0; i < numbers.Length; i++)
                numbers[i] /= gcd;
        }
        private static int GCD(int a, int b)
        {
            while (b > 0)
            {
                int rem = a % b;
                a = b;
                b = rem;
            }
            return a;
        }
        private static int GCD(int[] args)
        {
            // using LINQ:
            return args.Aggregate((gcd, arg) => GCD(gcd, arg));
        }
        // Speed up or slow down a video (if needed) to match a given length
        public static void MatchTimeVideo(string video, float duration, string output)
        {
            try
            {
                string length = GetLength(video);
                float videolength = float.Parse(length);
                float speed = videolength / duration;
                if(videolength == duration)
                {
                    File.Copy(video, output, true);
                    return;
                }
                // Change the video speed
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = ffmpegLocation;
                startInfo.Arguments = "-i \"" + video + "\" -filter:v \"setpts=" + speed + "*PTS\" -filter:a \"atempo=" + speed + "\" -y \"" + output + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Skipping video.", Color.Yellow);
                Global.generatorFactory.progressText = "Skipping video.";
            }
        }
    }
}