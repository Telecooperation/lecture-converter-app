using Converter;
using Converter.Model;
using Converter.Settings;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Converter.Recording
{
    public class RecordingConverter
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(RecordingConverter));

        private BlockingCollection<String> processingQueue = new BlockingCollection<String>();

        public void AddFile(string fileName)
        {
            processingQueue.Add(fileName);

            logger.InfoFormat("New file to convert queued: {0}", fileName);
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

        public static void ConvertRecording(string inputFileName, string outputFileName)
        {
            var arguments = new StringBuilder();
            arguments.Append("-i");
            arguments.Append(" ");
            arguments.Append("\"" + inputFileName + "\"");
            arguments.Append(" -f mp4 -vcodec libx264 -tune stillimage -profile:v baseline -level 3.0 -pix_fmt yuv420p -acodec aac ");
            arguments.Append("\"" + outputFileName + "\"");

            // run ffmpeg
            var process = Process.Start("ffmpeg\\ffmpeg.exe", arguments.ToString());
            process.OutputDataReceived += Process_OutputDataReceived;
            process.WaitForExit();
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Debug(e.Data);
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
            var targetFileName = settings.TargetFolders[index]
                + "\\video\\" + fileName.Replace(".trec", ".mp4");

            // create folders
            Directory.CreateDirectory(settings.TargetFolders[index] + "\\video");
            Directory.CreateDirectory(settings.TargetFolders[index] + "\\assets");

            // add new lecture entry
            var lecture = Lecture.LoadSettings(settings.LectureNames[index], settings.TargetFolders[index] + "\\assets\\lecture.json");
            var recording = new Converter.Model.Recording()
            {
                Name = Utils.GetCleanTitleFromFileName(fileName),
                Date = File.GetLastWriteTime(filePath),
                FileName = "./video/" + fileName.Replace(".trec", ".mp4"),
                Processing = true
            };
            lecture.Recordings.Add(recording);
            Lecture.SaveSettings(lecture, settings.TargetFolders[index] + "\\assets\\lecture.json");

            // wait for file to finish copying
            logger.Info("Wait 120s for file to complete copy...");
            Thread.Sleep(120000);

            logger.InfoFormat("Begin converting file: {0}", fileName);
            RecordingConverter.ConvertRecording(filePath, targetFileName);
            logger.InfoFormat("Finished converting file: {0}", fileName);

            // update to finished
            recording.Processing = false;
            Lecture.SaveSettings(lecture, settings.TargetFolders[index] + "\\assets\\lecture.json");

            // mark file as processed
            var processedFiles = ConvertedFiles.LoadFiles();
            processedFiles.Files.Add(filePath);
            ConvertedFiles.SaveFiles(processedFiles);
        }
    }
}
