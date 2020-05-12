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
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;

namespace TkRecordingConverter.util
{
    public class ChromaKeyParamGuesser
    {
        public static int NrOfSamples = 10;

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
            var result = new List<Color>();

            var outPath = Path.GetDirectoryName(videoFile);
            var len = GetMediaLength(videoFile);
            var stepSize = len.TotalSeconds / NrOfSamples;
            double currentTime = stepSize / 2.0;

            for (int i = 0; i < NrOfSamples - 1; i++)
            {
                Console.WriteLine("Sampling at " + currentTime);
                currentTime += stepSize;

                // create thumbnail
                ExportThumbnail((float)currentTime, videoFile, outPath, "tmp");

                // pick colors
                using (var img = SixLabors.ImageSharp.Image.Load<Rgba32>(Path.Combine(outPath, "tmp.jpg")))
                {
                    var height = img.Height / 1080;
                    var width = img.Width / 1920;

                    var colors = new List<Color>();

                    var imgColor = img[400 * width, 400 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[350 * width, 350 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[200 * width, 800 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[180 * width, 820 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[1600 * width, 200 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[1620 * width, 150 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[1700 * width, 800 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[1720 * width, 820 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    // merge together
                    float r = 0, g = 0, b = 0;

                    foreach (var color in colors)
                    {
                        r += color.R * color.R;
                        g += color.G * color.G;
                        b += color.B * color.B;
                    }

                    result.Add(Color.FromArgb((int)Math.Sqrt(r / colors.Count),
                        (int)Math.Sqrt(g / colors.Count), (int)Math.Sqrt(b / colors.Count)));
                }
            }

            File.Delete(Path.Combine(outPath, "tmp.jpg"));

            return result;
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
