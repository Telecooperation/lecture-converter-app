using ConverterCore.Processing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConverterCore.Recordings
{
    public class RecordingConverter
    {
        private readonly ILogger<RecordingConverter> _logger;
        private readonly MediaProcessor mediaProcessor;
        private BlockingCollection<MediaMetaData> processingQueue = new BlockingCollection<MediaMetaData>(new ConcurrentQueue<MediaMetaData>());
        private List<MediaMetaData> currentProcessing = new List<MediaMetaData>();

        public RecordingConverter(ILogger<RecordingConverter> logger,
            MediaProcessor mediaProcessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mediaProcessor = mediaProcessor;
        }

        public void QueueFile(MediaMetaData queuedFile)
        {
            _logger.LogInformation("New file to convert queued: {0}", queuedFile.FilePath);

            if (!processingQueue.Contains(queuedFile) && !currentProcessing.Contains(queuedFile))
                processingQueue.Add(queuedFile);
        }

        public void RunConversionQueue()
        {
            var th = new Thread(() =>
            {
                Action action = () =>
                {
                    while (true)
                    {
                        try
                        {
                            var queuedFile = processingQueue.Take();
                            ProcessFile(queuedFile);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message, ex);
                        }
                    }
                };


                Parallel.Invoke(action, action);
            });

            th.Start();
        }

        private bool ShouldConvert(MediaMetaData queuedFile)
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

        public void ProcessFile(MediaMetaData queuedFile)
        {
            // convert file?
            if (!ShouldConvert(queuedFile))
            {
                _logger.LogInformation("Ignore file: {0}", queuedFile.FilePath);
                return;
            }

            if (queuedFile.Course.Studio)
            {
                mediaProcessor.ProcessStudioMedia(queuedFile);
            }
            else
            {
                mediaProcessor.ProcessTrecMedia(queuedFile);
            }

            // delete from processing queue
            currentProcessing.Remove(queuedFile);
        }
    }
}
