using ConverterCore.Model;
using ConverterCore.Settings;
using Microsoft.Extensions.Logging;
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

namespace ConverterCore.Recording
{
    public class RecordingConverter
    {
        private readonly ILogger<RecordingConverter> _logger;

        private BlockingCollection<string> processingQueue = new BlockingCollection<string>();

        public RecordingConverter(ILogger<RecordingConverter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddFile(string fileName)
        {
            // load processed files
            var processedFiles = ConvertedFiles.LoadFiles();

            if (!processedFiles.Files.Contains(fileName) && !processingQueue.Contains(fileName))
            {
                _logger.LogInformation("New file to convert queued: {0}", fileName);

                processingQueue.Add(fileName);
            }
        }

        public void RunConversionQueue()
        {
            var th = new Thread(() =>
            {
                foreach (var fileName in processingQueue.GetConsumingEnumerable())
                {
                    ProcessFile(fileName);
                }
            });
            th.Start();
        }

        public void ConvertRecording(string inputFileName, string outputFileName)
        {
            var arguments = new StringBuilder();
            arguments.Append("-i");
            arguments.Append(" ");
            arguments.Append("\"" + inputFileName + "\"");
            arguments.Append(" -f mp4 -vcodec libx264 -tune stillimage -profile:v baseline -level 3.0 -pix_fmt yuv420p -acodec aac ");
            arguments.Append("\"" + outputFileName + "\"");

            // run ffmpeg
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var process = Process.Start(Path.Combine("ffmpeg", "win", "ffmpeg"), arguments.ToString());
                process.OutputDataReceived += Process_OutputDataReceived;
                process.WaitForExit();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var process = Process.Start(Path.Combine("ffmpeg", "unix", "ffmpeg"), arguments.ToString());
                process.OutputDataReceived += Process_OutputDataReceived;
                process.WaitForExit();
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.LogDebug(e.Data);
        }

        private void ProcessFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var folderName = Path.GetDirectoryName(filePath);

            // load processed files
            var processedFiles = ConvertedFiles.LoadFiles();

            if (!processedFiles.Files.Contains(filePath))
            {
                ProcessFile(fileName, folderName, filePath);
            }
        }

        private void ProcessFile(string fileName, string folderName, string filePath)
        {
            // original folder
            var settings = Settings.Settings.LoadSettings();
            var index = settings.SourceFolders.IndexOf(folderName);

            // process file
            var targetFileName = Path.Combine(settings.TargetFolders[index], "video", fileName.Replace(".trec", ".mp4"));

            // create folders
            Directory.CreateDirectory(Path.Combine(settings.TargetFolders[index], "video"));
            Directory.CreateDirectory(Path.Combine(settings.TargetFolders[index], "assets"));

            // add new lecture entry
            var lecture = Lecture.LoadSettings(settings.LectureNames[index], Path.Combine(settings.TargetFolders[index], "assets", "lecture.json"));
            var recording = new Model.Recording()
            {
                Name = Utils.GetCleanTitleFromFileName(fileName),
                Date = File.GetLastWriteTime(filePath),
                FileName = "./video/" + fileName.Replace(".trec", ".mp4"),
                Processing = true
            };
            lecture.Recordings.Add(recording);
            Lecture.SaveSettings(lecture, Path.Combine(settings.TargetFolders[index], "assets", "lecture.json"));

            // wait for file to finish copying
            _logger.LogInformation("Wait 120s for file to complete copy...");
            Thread.Sleep(120000);

            _logger.LogInformation("Begin converting file: {0}", fileName);
            ConvertRecording(filePath, targetFileName);
            _logger.LogInformation("Finished converting file: {0}", fileName);

            // update to finished
            recording.Processing = false;
            recording.Date = File.GetLastWriteTime(filePath);
            Lecture.SaveSettings(lecture, Path.Combine(settings.TargetFolders[index], "assets", "lecture.json"));

            // mark file as processed
            var processedFiles = ConvertedFiles.LoadFiles();
            processedFiles.Files.Add(filePath);
            ConvertedFiles.SaveFiles(processedFiles);
        }
    }
}
