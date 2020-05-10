using ConverterCore.Model;
using ConverterCore.Services;
using ConverterCore.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConverterCore.Recordings
{
    public class RecordingConverter
    {
        private readonly ILogger<RecordingConverter> _logger;

        private BlockingCollection<QueuedFile> processingQueue = new BlockingCollection<QueuedFile>();
        private List<QueuedFile> currentProcessing = new List<QueuedFile>();

        public FFMpegConvertService ConvertService { get; }

        public RecordingConverter(ILogger<RecordingConverter> logger,
            FFMpegConvertService convertService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ConvertService = convertService;
        }

        public void QueueFile(QueuedFile queuedFile)
        {
            _logger.LogInformation("New file to convert queued: {0}", queuedFile.FilePath);

            if (!processingQueue.Contains(queuedFile) && !currentProcessing.Contains(queuedFile))
                processingQueue.Add(queuedFile);
        }

        public void RunConversionQueue()
        {
            var th = new Thread(() =>
            {
                foreach (var queuedFile in processingQueue.GetConsumingEnumerable())
                {
                    currentProcessing.Add(queuedFile);

                    ProcessFile(queuedFile);
                }
            });
            th.Start();
        }

        private bool ShouldConvert(QueuedFile queuedFile)
        {
            // process file?
            if (queuedFile.FilePath.EndsWith(".trec"))
            {
                var targetFileName = Path.GetFileName(queuedFile.FilePath).Replace(".trec", ".mp4");
                var targetFilePath = Path.Combine(queuedFile.Course.TargetFolder, targetFileName);

                return !File.Exists(targetFilePath);
            }
            else if (queuedFile.FilePath.EndsWith("_meta.json"))
            {
                var targetFileName = Path.GetFileName(queuedFile.FilePath.Replace("_meta.json", ""));
                var convertFilePath = Path.Combine(queuedFile.Course.ConvertFolder, targetFileName);
                var targetFilePath = Path.Combine(queuedFile.Course.TargetFolder, "video", targetFileName);

                return !Directory.Exists(targetFilePath) && !Directory.Exists(convertFilePath);
            }

            return false;
        }

        private void ProcessFile(QueuedFile queuedFile)
        {
            // convert file?
            if (!ShouldConvert(queuedFile))
            {
                _logger.LogInformation("Ignore file: {0}", queuedFile.FilePath);
                return;
            }

            if (queuedFile.Course.Studio)
            {
                var inputFilePath = queuedFile.FilePath;

                var targetFileName = Path.GetFileName(queuedFile.FilePath);
                var targetFilePath = Path.Combine(queuedFile.Course.ConvertFolder, targetFileName.Replace("_meta.json", ""));

                // wait for file to finish copying
                _logger.LogInformation($"Begin transcoding {targetFileName}...");
                Thread.Sleep(1200);

                var recording = ConvertService.ConvertStudioRecording(inputFilePath, targetFileName, targetFilePath);
                var targetFolderName = Path.Combine("video", targetFileName.Replace("_meta.json", ""));

                // read metadata
                var metadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(inputFilePath));

                if (metadata != null)
                {
                    // add new lecture entry
                    var recordingItem = new Model.Recording()
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
            else
            {
                // convert
                var inputFilePath = queuedFile.FilePath;

                var targetFileName = Path.GetFileName(queuedFile.FilePath).Replace(".trec", ".mp4");
                var targetFilePath = Path.Combine(queuedFile.Course.TargetFolder, "video", targetFileName);

                // add new lecture entry
                var lecture = Lecture.LoadSettings(queuedFile.Course);
                var recording = lecture.Recordings.Where(x => x.Name == Utils.GetCleanTitleFromFileName(targetFileName)).SingleOrDefault();

                if (recording == null)
                {
                    recording = new Model.Recording()
                    {
                        Name = Utils.GetCleanTitleFromFileName(targetFileName),
                        Date = File.GetLastWriteTime(inputFilePath),
                        FileName = "./video/" + targetFileName,
                        Processing = true
                    };
                    lecture.Recordings.Add(recording);
                    Lecture.SaveSettings(lecture, queuedFile.Course);
                }

                // wait for file to finish copying
                _logger.LogInformation("Wait 120s for file to complete copy...");
                Thread.Sleep(120000);

                _logger.LogInformation("Begin converting file: {0}", inputFilePath);
                ConvertService.ConvertSingleFile(inputFilePath, targetFilePath);
                _logger.LogInformation("Finished converting file: {0}", inputFilePath);

                // update to finished
                recording.Processing = false;
                recording.Date = File.GetLastWriteTime(inputFilePath);
                Lecture.SaveSettings(lecture, queuedFile.Course);
            }

            // delete from processing queue
            currentProcessing.Remove(queuedFile);
        }
    }
}
