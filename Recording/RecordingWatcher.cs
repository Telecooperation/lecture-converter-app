using log4net;
using System;
using System.Collections.Generic;
using System.IO;

namespace Converter.Recording
{
    public class RecordingWatcher
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(RecordingWatcher));

        private Dictionary<string, FileSystemWatcher> fswDict = new Dictionary<string, FileSystemWatcher>();

        public event EventHandler<NewFileDetectedEventArgs> NewFileDetected;

        public void AddWatcher(string folderName)
        {
            if (fswDict.ContainsKey(folderName))
                return;

            // create a new file watcher
            var watcher = new FileSystemWatcher(folderName, "*.trec");
            watcher.Created += Watcher_Created;
            watcher.Error += Watcher_Error;
            watcher.EnableRaisingEvents = true;

            fswDict.Add(folderName, watcher);

            // do a folder scan
            ScanFolder(folderName);
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            logger.Error("File watcher raised an error.", e.GetException());

            try
            {
                NotAccessibleError((FileSystemWatcher)sender, e);
            }
            catch (Exception ex)
            {
                logger.Error("Could not restart file system watcher.", ex);
            }
        }

        private static void NotAccessibleError(FileSystemWatcher source, ErrorEventArgs e)
        {
            source.EnableRaisingEvents = false;
            int iMaxAttempts = 120;
            int iTimeOut = 30000;
            int i = 0;
            while (source.EnableRaisingEvents == false && i < iMaxAttempts)
            {
                i += 1;
                try
                {
                    source.EnableRaisingEvents = true;
                }
                catch
                {
                    source.EnableRaisingEvents = false;
                    System.Threading.Thread.Sleep(iTimeOut);
                }
            }

        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            var filePath = e.FullPath;
            if (!filePath.EndsWith(".trec"))
                return;

            OnNewFileDetected(new NewFileDetectedEventArgs() { FileName = filePath });
        }

        private void ScanFolder(string folderName)
        {
            var files = Directory.EnumerateFiles(folderName);
            foreach (var file in files)
            {
                if (file.EndsWith(".trec"))
                {
                    OnNewFileDetected(new NewFileDetectedEventArgs() { FileName = file });
                }
            }
        }

        public virtual void OnNewFileDetected(NewFileDetectedEventArgs e)
        {
            NewFileDetected?.Invoke(this, e);
        }
    }
}
