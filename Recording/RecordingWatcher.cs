using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConverterCore.Recording
{
    public class RecordingWatcher
    {
        private readonly ILogger<RecordingWatcher> _logger;

        private Dictionary<string, FileSystemWatcher> fswDict = new Dictionary<string, FileSystemWatcher>();
        private Dictionary<string, Timer> tDict = new Dictionary<string, Timer>();

        public event EventHandler<NewFileDetectedEventArgs> NewFileDetected;

        public RecordingWatcher(ILogger<RecordingWatcher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddWatcher(string folderName)
        {
            if (fswDict.ContainsKey(folderName) || tDict.ContainsKey(folderName))
                return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // create a new file watcher
                var watcher = new FileSystemWatcher(folderName, "*.trec");
                watcher.Created += Watcher_Created;
                watcher.Changed += Watcher_Created;
                watcher.Error += Watcher_Error;
                watcher.EnableRaisingEvents = true;

                fswDict.Add(folderName, watcher);
            }
            else
            {
                // do scanning the folder every now and then
                var bgTask = new Timer(x => ScanFolder(folderName), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
                tDict.Add(folderName, bgTask);
            }

            // do a folder scan
            ScanFolder(folderName);
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "File watcher raised an error.");

            try
            {
                NotAccessibleError((FileSystemWatcher)sender, e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not restart file system watcher.");
            }
        }

        private void NotAccessibleError(FileSystemWatcher source, ErrorEventArgs e)
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

            // do a manual scan
            foreach (var folderName in fswDict.Keys)
            {
                ScanFolder(folderName);
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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not crawl observed folder: " + folderName);
            }
        }

        public virtual void OnNewFileDetected(NewFileDetectedEventArgs e)
        {
            NewFileDetected?.Invoke(this, e);
        }
    }
}
