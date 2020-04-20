using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using ConverterCore.Model;
using Newtonsoft.Json;
using Tesseract;
using TkRecordingConverter.util;

namespace ConverterCore.Studio
{
    public class Converter
    {
        public void Convert(string folder, Dimension targetDimension)
        {
            var slidesFile = Directory.GetFiles(folder).FirstOrDefault(f => f.EndsWith("slides.mp4"));
            var thFile = Directory.GetFiles(folder).FirstOrDefault(f => f.EndsWith("talkinghead.mp4"));
            var metaFile = Directory.GetFiles(folder).FirstOrDefault(f => f.EndsWith("meta.json"));

            Convert(slidesFile, thFile, metaFile, targetDimension);
        }
        public Recording Convert(string slideVideoPath, string thVideoPath, string metaInfoPath, Dimension targetDimension)
        {
            dynamic projectJson = JsonConvert.DeserializeObject(File.ReadAllText(metaInfoPath));

            return Convert(new Configuration
            {
                slideVideoPath = slideVideoPath,
                thVideoPath = thVideoPath,
                slideInfoPath = metaInfoPath,
                outputDir = Path.GetDirectoryName(slideVideoPath) + "\\build",
                projectName = new FileInfo(slideVideoPath).Directory.Name,
                recordingStyle = RecordingStyle.TkStudioStyle(targetDimension),
                writeJson = true
            });
        }

        public Recording Convert(Configuration config)
        {
            Recording finalRecording = new Recording();
            Directory.CreateDirectory(config.outputDir);
            //ConvertVideoFiles(config);
            finalRecording.Name = config.projectName;
            finalRecording.Date = File.GetCreationTimeUtc(config.slideVideoPath);
            finalRecording.FileName = "slides.mp4";
            finalRecording.PresenterFileName = "talkinghead.mp4";
            finalRecording.StageVideo = "stage.mp4";
            finalRecording.Slides = BuildThumbnails(config, finalRecording);
            if (config.writeJson)
                WriteMetadata(finalRecording, config);
            return finalRecording;
        }

        private void WriteMetadata(Recording finalRecording, Configuration config)
        {
            string json = JsonConvert.SerializeObject(finalRecording, Formatting.Indented);

            //write string to file
            System.IO.File.WriteAllText(Path.Combine(config.outputDir, "lecture.json"), json);
        }

        private Slide[] BuildThumbnails(Configuration config, Recording finalRecording)
        {
            var thumbOutDir = Path.Combine(config.outputDir, "thumbs");
            Directory.CreateDirectory(thumbOutDir);

            var ocrEngine = new TesseractEngine(Path.Combine("resources", "tessdata"), "eng", EngineMode.Default);
            List<Slide> result = new List<Slide>();

            dynamic projectJson = JsonConvert.DeserializeObject(File.ReadAllText(config.slideInfoPath));
            int currentId = 0;

            var keyframes = new List<TimeSpan>();
            keyframes.Add(TimeSpan.Zero);
            foreach (string timestamp in projectJson["Slides"])
                keyframes.Add(TimeSpan.Parse(timestamp));

            var mediaLen = GetMediaLength(Path.Combine(config.outputDir, finalRecording.FileName));

            foreach (var keyframe in keyframes)
            {
                TimeSpan? nextKeyframe = null;

                if (keyframes.IndexOf(keyframe) != keyframes.Count - 1)
                {
                    nextKeyframe = keyframes[keyframes.IndexOf(keyframe) + 1];
                }
                else
                {
                    nextKeyframe = GetMediaLength(Path.Combine(config.outputDir, finalRecording.FileName));
                }

                string thumbName = ExportThumbnail((float)nextKeyframe.GetValueOrDefault().TotalSeconds - 2.0f, Path.Combine(config.outputDir, finalRecording.FileName), thumbOutDir,
                    (currentId++).ToString());

                var slide = new Slide
                {
                    StartPosition = (float)keyframe.TotalSeconds + 0.2f,
                    Thumbnail = "thumbs/" + thumbName,
                    Ocr = PerformOcr(Path.Combine(thumbOutDir, thumbName), ocrEngine)
                };

                if (keyframe.Equals(TimeSpan.Zero))
                    slide.StartPosition = 0.0f;
                result.Add(slide);
            }

            ocrEngine.Dispose();
            return result.ToArray();
        }

        private string PerformOcr(string file, TesseractEngine ocrEngine)
        {
            Console.WriteLine("Performing OCR for " + file);

            var img = Pix.LoadFromFile(file);
            var page = ocrEngine.Process(img);
            var result = page.GetText();
            page.Dispose();

            return result;
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

        private void ConvertVideoFiles(Configuration config)
        {
            var lenSlideVideo = GetMediaLength(config.slideVideoPath);
            var lenTHVideo = GetMediaLength(config.thVideoPath);

            var trimTHVideo = lenTHVideo - lenSlideVideo;

            string args = "-i " + config.slideVideoPath + " " +
                          "-i " + config.thVideoPath + " " +
                          "-i " + config.recordingStyle.targetDimension.background + " " +
                          "-filter_complex " +
                          "\"" +
                          "[1:v]trim=start=" + trimTHVideo.TotalSeconds.ToString("0.00000", CultureInfo.InvariantCulture) + ",setpts=PTS-STARTPTS[1v];" +
                          "[1:a]atrim=start=" + trimTHVideo.TotalSeconds.ToString("0.00000", CultureInfo.InvariantCulture) + ",asetpts=PTS-STARTPTS,asplit=2[1a1][1a2];" +
                          //"[0:v]scale=" + config.recordingStyle.targetDimension.width + ":" +
                          //  config.recordingStyle.targetDimension.height + ",split=2[slides1][slides2];" +
                          //"scale=" + targetDimension.width + ":-2, crop=" + targetDimension.width + ":" +
                          //targetDimension.height + "\"
                          "[0:v]scale=" + config.recordingStyle.targetDimension.width + ":-2, crop=" + config.recordingStyle.targetDimension.width + ":" +
                          config.recordingStyle.targetDimension.height + ",split=2[slides1][slides2];" +
                          "[1v]scale=" + config.recordingStyle.targetDimension.width + ":" +
                            config.recordingStyle.targetDimension.height + "[th];" +
                          "[th]format = rgba,chromakey=" + config.recordingStyle.ChromaKeyParams.color + ":" +
                          config.recordingStyle.ChromaKeyParams.similarity + ":" +
                          config.recordingStyle.ChromaKeyParams.blend + ",split=2[th_ck1][th_ck2];" +
                          "[2][th_ck1]overlay=0:0[th_ck_bg];" +
                          "[th_ck_bg]crop=" + config.recordingStyle.TalkingHeadConfig.Crop.width + ":" +
                                            config.recordingStyle.TalkingHeadConfig.Crop.height + ":" +
                                            config.recordingStyle.TalkingHeadConfig.Crop.x + ":" +
                                            config.recordingStyle.TalkingHeadConfig.Crop.y + "[th_ck_ct];" +
                          "[slides2]format = rgba,pad = iw + 4:ih + 4:2:2:black@0," +
                          "perspective=" + config.recordingStyle.StageConfig.slideTransformation.ToString() + "," +
                          "crop=" + config.recordingStyle.targetDimension.width + ":" +
                          config.recordingStyle.targetDimension.height + ":2:2" + "[slides_perspective];" +
                          "[th_ck2]pad = iw + 4:ih + 4:2:2:black@0," + "perspective=" + config.recordingStyle.StageConfig.speakerTransformation.ToString() + "[th_ck_tr];" +
                          "[2][slides_perspective]overlay=0:0[slides_with_background];" +
                          "[slides_with_background][th_ck_tr]overlay=0:0[stage]" +
                          "\" " +
                          "-map \"[slides1]\" -f mp4 -vcodec libx264 -crf 23 -preset veryfast -tune stillimage -profile:v baseline -level 3.0 -pix_fmt yuv420p " + Path.Combine(config.outputDir, "slides.mp4") + " " +
                          "-map \"[th_ck_ct]\" -map \"[1a1]\" -f mp4 -vcodec libx264 -crf 23 -preset veryfast -profile:v baseline -level 3.0 -pix_fmt yuv420p -acodec aac " + Path.Combine(config.outputDir, "talkinghead.mp4") + " " +
                          "-map \"[stage]\" -map \"[1a2\" -f mp4 -vcodec libx264 -crf 23 -preset veryfast -profile:v baseline -level 3.0 -pix_fmt yuv420p -acodec aac " + Path.Combine(config.outputDir, "stage.mp4") + " ";

            Console.WriteLine(args);

            Process p = FFmpegHelper.FFmpeg(args, false);
            p.Start();
            p.WaitForExit();
        }

        private void CreateVideoFiles(Configuration config)
        {
            var lenSlideVideo = GetMediaLength(config.slideVideoPath);
            var lenTHVideo = GetMediaLength(config.thVideoPath);

            var trimTHVideo = lenTHVideo - lenSlideVideo;

            //<
        }
    }
}
