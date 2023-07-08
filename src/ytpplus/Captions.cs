using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;

namespace YTPPlusPlusPlus
{
    // This is a wrapper for srt captions.
    public class CaptionText
    {
        public string text;
        public string startTime;
        public string endTime;
        public CaptionText(string text, string startTime, string endTime)
        {
            this.text = text;
            this.startTime = startTime;
            this.endTime = endTime;
        }
    }
    public class CaptionFile
    {
        public List<CaptionText> captions = new();
        // parse caption time as ms
        public static int ParseTime(string time)
        {
            // parse time as milliseconds (00:00:0,000 -> 0)
            int timeMs = int.Parse(time.Substring(0, 1)) * 3600000;
            timeMs += int.Parse(time.Substring(3, 2)) * 60000;
            timeMs += int.Parse(time.Substring(6, 2)) * 1000;
            timeMs += int.Parse(time.Substring(9, 3));
            return timeMs;
        }
        // encode ms as caption time
        public static string EncodeTime(int timeMs)
        {
            // encode time as milliseconds (0 -> 00:00:0,000)
            string time = "";
            int hours = timeMs / 3600000;
            timeMs -= hours * 3600000;
            int minutes = timeMs / 60000;
            timeMs -= minutes * 60000;
            int seconds = timeMs / 1000;
            timeMs -= seconds * 1000;
            int milliseconds = timeMs;
            time += hours.ToString("00");
            time += ":";
            time += minutes.ToString("00");
            time += ":";
            time += seconds.ToString("00");
            time += ",";
            time += milliseconds.ToString("000");
            return time;
        }
        // get caption at time
        public string GetCaption(int time)
        {
            // find caption at time
            for (int i = 0; i < captions.Count; i++)
            {
                CaptionText caption = captions[i];
                int startTime = ParseTime(caption.startTime);
                int endTime = ParseTime(caption.endTime);
                if (time >= startTime && time <= endTime)
                {
                    return caption.text;
                }
            }
            return "";
        }
        // set caption at start/end time
        public void SetCaption(string text, int startTime, int endTime)
        {
            // get parsed time
            string startTimeStr = EncodeTime(startTime);
            string endTimeStr = EncodeTime(endTime);
            for (int i = 0; i < captions.Count; i++)
            {
                // check if caption at time exists
                if (captions[i].startTime == startTimeStr && captions[i].endTime == endTimeStr)
                {
                    captions[i].text = text;
                    return;
                }
                // TODO: overlapping captions should be trimmed
            }
            // add new caption
            captions.Add(new CaptionText(text, startTimeStr, endTimeStr));
        }
        // save captions to file
        public void Save(string path)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < captions.Count; i++)
            {
                CaptionText caption = captions[i];
                lines.Add((i + 1).ToString());
                lines.Add($"{caption.startTime} --> {caption.endTime}");
                lines.Add(caption.text);
                lines.Add("");
            }
            File.WriteAllLines(path, lines);
        }
        // load captions from file
        public void Load(string path, bool append = false)
        {
            if (!append)
            {
                captions.Clear();
            }
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if(!lines[i].Contains(" --> "))
                    continue;
                string[] times = lines[i].Split(" --> ");
                CaptionText caption = new CaptionText(lines[i + 1], times[0], times[1]);
                captions.Add(caption);
            }
        }
    }
}