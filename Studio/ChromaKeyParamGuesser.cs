using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace TkRecordingConverter.util
{
    public class ChromaKeyParamGuesser
    {
        public static int NrOfSamples = 5;

        public string GuessChromaKeyParams(string videoFile)
        {
            List<Color> sampledColors = SampleColors(videoFile);

            float r = 0;
            float g = 0;
            float b = 0;

            foreach (var color in sampledColors)
            {
                r += color.R * color.R;
                g += color.G * color.G;
                b += color.B * color.B;
            }

            Color result = Color.FromArgb((int)Math.Sqrt(r / sampledColors.Count),
                (int)Math.Sqrt(g / sampledColors.Count), (int)Math.Sqrt(b / sampledColors.Count));

            string res = "0x" + result.R.ToString("X2") + result.G.ToString("X2") + result.B.ToString("X2");

            return res;
        }

        private List<Color> SampleColors(string videoFile)
        {
            List<Color> colors = new List<Color>();

            var outPath = Path.GetDirectoryName(videoFile);
            var len = GetMediaLength(videoFile);
            var stepSize = len.TotalSeconds / NrOfSamples;
            double currentTime = stepSize / 2.0;

            Console.WriteLine("Vidoe len: " + len.TotalSeconds);

            String args = "-image " + Path.Combine(outPath, "tmp.jpg") + " -clusters 2 -json";

            for (int i = 0; i < NrOfSamples - 1; i++)
            {
                Console.WriteLine("Sampeling at " + currentTime);
                currentTime += stepSize;

                ExportThumbnail((float)currentTime, videoFile, outPath, "tmp");

                Process p = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    p = FFmpegHelper.BuildProcess(Path.Combine("resources", "colorsummarizer", "bin", "colorsummarizer.exe"), args, true);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    p = FFmpegHelper.BuildProcess("perl", "-X " + Path.Combine("resources", "colorsummarizer", "bin", "colorsummarizer") + " " + args, true);

                p.Start();

                string content = "";
                while (!p.StandardOutput.EndOfStream)
                {
                    var line = p.StandardOutput.ReadLine();

                    if (!line.StartsWith("Use"))
                    {
                        content += line;
                        Console.WriteLine(line);
                    }
                }
                p.WaitForExit();

                dynamic jsonResult = JsonConvert.DeserializeObject(content);

                string color0 = jsonResult["data"]["color0"];
                var color = System.Drawing.ColorTranslator.FromHtml(color0);
                colors.Add(color);
            }

            File.Delete(Path.Combine(outPath, "tmp.jpg"));

            return colors;
        }

        private TimeSpan GetMediaLength(string path)
        {
            Process p = FFmpegHelper.FFmpeg("-i " + path);
            p.Start();
            p.WaitForExit();

            string stringDuration = "";

            while (!p.StandardError.EndOfStream)
            {
                string line = p.StandardError.ReadLine();
                if (line.Contains("Duration"))
                {
                    var durationSplit = line.Split(',')[0].Split(' ');
                    stringDuration = durationSplit[durationSplit.Length - 1];
                }
            }

            return TimeSpan.Parse(stringDuration);
        }

        private string ExportThumbnail(float timeInSeconds, string clip, string outPath, string id)
        {
            Console.WriteLine("Export Thumbnail at " + timeInSeconds + " from " + clip);
            String outFile = id + ".jpg";

            var args = "-y -ss " + timeInSeconds.ToString("0.00000", CultureInfo.InvariantCulture) +
                       " " +
                       "-i " + clip + " " +
                       "-vframes 1 " +
                       Path.Combine(outPath, outFile);

            Process p = FFmpegHelper.FFmpeg(args, false);
            p.Start();
            p.WaitForExit();

            return outFile;
        }

    }
}
