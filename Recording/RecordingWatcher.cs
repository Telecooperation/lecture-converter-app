using Converter.Model;
using Converter.Settings;
using LectureRecordingConverter.Converter;
using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Converter.Recording
{
    public class RecordingWatcher
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(RecordingWatcher));

        private Dictionary<string, FileSystemWatcher> fswDict = new Dictionary<string, FileSystemWatcher>();

        public void AddWatcher(string folderName)
        {
            if (fswDict.ContainsKey(folderName))
                return;

            // create a new file watcher
            var watcher = new FileSystemWatcher(folderName, "*.trec");
            watcher.Created += Watcher_Created;
            watcher.EnableRaisingEvents = true;

            fswDict.Add(folderName, watcher);

            // do a folder scan
            ScanFolder(folderName);
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            var filePath = e.FullPath;
            if (!filePath.EndsWith(".trec"))
                return;

            ProcessFile(filePath);
        }

        private void ScanFolder(string folderName)
        {
            var files = Directory.EnumerateFiles(folderName);
            foreach (var file in files)
            {
                if (file.EndsWith(".trec"))
                {
                    ProcessFile(file);
                }
            }
        }

        private void ProcessFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var folderName = Path.GetDirectoryName(filePath);

            // load processed files
            var processedFiles = ConvertedFiles.LoadFiles();

            if (!processedFiles.Files.Contains(filePath))
            {
                logger.InfoFormat("New file detected: {0}", filePath);

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
            var recording = new Model.Recording()
            {
                Name = Utils.GetCleanTitleFromFileName(fileName),
                Date = File.GetCreationTime(filePath),
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
