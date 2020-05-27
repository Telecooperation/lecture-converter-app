using ConverterCore.Recordings;
using ConverterCore.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecordingProcessor.Model;
using RecordingProcessor.Utils;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConverterCore.Processing
{
    public class MediaProcessor
    {
        private readonly ILogger<MediaProcessor> _logger;
        private readonly FFMpegConvertService ffMpegConvertService;

        public MediaProcessor(
            ILogger<MediaProcessor> logger,
            FFMpegConvertService ffMpegConvertService)
        {
            this._logger = logger;
            this.ffMpegConvertService = ffMpegConvertService;
        }

        public void ProcessStudioMedia(MediaMetaData mediaMetaData)
        {
            var inputFilePath = mediaMetaData.FilePath;

            var targetFileName = Path.GetFileName(mediaMetaData.FilePath);
            var targetFilePath = Path.Combine(mediaMetaData.Course.ConvertFolder, targetFileName.Replace("_meta.json", ""));

            // wait for file to finish copying
            _logger.LogInformation($"Begin transcoding {targetFileName}...");
            Thread.Sleep(1200);

            var recording = ffMpegConvertService.ConvertStudioRecording(inputFilePath, targetFileName, targetFilePath);
            var targetFolderName = Path.Combine("video", targetFileName.Replace("_meta.json", ""));

            // read metadata
            var metadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(inputFilePath));

            if (metadata != null)
            {
                // add new lecture entry
                var recordingItem = new Recording()
                {
                    Name = metadata.Description != null ? metadata.Description : recording.Name,
                    Description = metadata.Description,
                    Date = metadata.LectureDate != null ? metadata.LectureDate : File.GetLastWriteTime(inputFilePath),
                    FileName = targetFolderName + "/" + recording.FileName,
                    StageVideo = targetFolderName + "/" + recording.StageVideo,
                    PresenterFileName = targetFolderName + "/" + recording.PresenterFileName,
                    Processing = false,
                    Slides = recording.Slides,
                    Duration = recording.Duration
                };

                foreach (var slide in recordingItem.Slides)
                {
                    slide.Thumbnail = targetFolderName + "/" + slide.Thumbnail;
                }

                File.WriteAllText(Path.Combine(targetFilePath, "meta.json"), JsonConvert.SerializeObject(recordingItem, Formatting.Indented));
            }

            _logger.LogInformation($"Finished transcoding {targetFileName}");
        }

        public void ProcessTrecMedia(MediaMetaData mediaMetaData)
        {
            // convert
            var inputFilePath = mediaMetaData.FilePath;

            var targetFileName = Path.GetFileName(mediaMetaData.FilePath).Replace(".trec", ".mp4");
            var targetFilePath = Path.Combine(mediaMetaData.Course.TargetFolder, "video", targetFileName);

            // add new lecture entry
            var lecture = MetaDataService.LoadSettings(mediaMetaData.Course);
            var recording = lecture.Recordings.Where(x => x.Name == StringUtils.GetCleanTitleFromFileName(targetFileName)).SingleOrDefault();

            if (recording == null)
            {
                recording = new Recording()
                {
                    Name = StringUtils.GetCleanTitleFromFileName(targetFileName),
                    Date = File.GetLastWriteTime(inputFilePath),
                    FileName = "./video/" + targetFileName,
                    Processing = true
                };
                lecture.Recordings.Add(recording);
                MetaDataService.SaveSettings(lecture, mediaMetaData.Course);
            }

            // wait for file to finish copying
            _logger.LogInformation("Wait 120s for file to complete copy...");
            Thread.Sleep(120000);

            _logger.LogInformation("Begin converting file: {0}", inputFilePath);
            ffMpegConvertService.ConvertTrecRecording(inputFilePath, targetFilePath);
            _logger.LogInformation("Finished converting file: {0}", inputFilePath);

            // update to finished
            recording.Processing = false;
            recording.Date = File.GetLastWriteTime(inputFilePath);
            MetaDataService.SaveSettings(lecture, mediaMetaData.Course);
        }
    }
}
