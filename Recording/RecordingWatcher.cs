using Converter.Model;
using Converter.Settings;
using LectureRecordingConverter.Converter;
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
        private Dictionary<string, FileSystemWatcher> fswDict = new Dictionary<string, FileSystemWatcher>();

        public void AddWatcher(string folderName)
        {
            if (fswDict.ContainsKey(folderName))
                return;

            var watcher = new FileSystemWatcher(folderName, "*.trec");
            watcher.Created += Watcher_Created;
            watcher.EnableRaisingEvents = true;

            fswDict.Add(folderName, watcher);
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            var filePath = e.FullPath;
            if (!filePath.EndsWith(".trec"))
                return;

            Console.WriteLine("New file detected: " + filePath);

            var fileName = Path.GetFileName(filePath);
            var folderName = Path.GetDirectoryName(filePath);

            // load processed files
            var processedFiles = ConvertedFiles.LoadFiles();
            
            if(!processedFiles.Files.Contains(filePath.ToLower()))
            {
                // original folder
                var settings = Settings.Settings.LoadSettings();
                var index = settings.SourceFolders.IndexOf(folderName);

                // process file
                var targetFileName = settings.TargetFolders[index]
                    + "\\video\\" + fileName.Replace(".trec", ".mp4");

                Console.WriteLine("Wait 60s for file to complete copy...");
                Thread.Sleep(60000);

                Console.WriteLine("Begin converting file: " + fileName);
                RecordingConverter.ConvertRecording(filePath, targetFileName);
                Console.WriteLine("Finished converting file:" + fileName);

                // add new lecture entry
                var lecture = Lecture.LoadSettings(settings.LectureNames[index], settings.TargetFolders[index] + "\\lecture.json");
                lecture.Recordings.Add(new Model.Recording()
                {
                    Name = Utils.GetCleanTitleFromFileName(fileName),
                    Date = File.GetCreationTime(filePath).ToShortDateString(),
                    FileName = "./video/" + fileName.Replace(".trec", ".mp4")
                });

                Lecture.SaveSettings(lecture, settings.TargetFolders[index] + "\\lecture.json");
            }
        }
    }
}
