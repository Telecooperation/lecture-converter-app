﻿using ConverterCore.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConverterCore.Recordings
{
    public class CourseWatcher
    {
        private readonly ILogger<CourseWatcher> _logger;

        private Dictionary<string, Timer> tDict = new Dictionary<string, Timer>();

        private List<string> detectedFiles = new List<string>();

        public event EventHandler<NewFileDetectedEventArgs> NewFileDetected;

        public CourseWatcher(ILogger<CourseWatcher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddWatcher(Course course)
        {
            if (tDict.ContainsKey(course.Id))
                return;

            // do scanning the folder every now and then
            var bgTask = new Timer(x => ScanCourse(course), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            tDict.Add(course.Id, bgTask);
        }

        private void ScanCourse(Course course)
        {
            try
            {
                var files = Directory.EnumerateFiles(course.SourceFolder, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if ((file.EndsWith("_meta.json")) && !detectedFiles.Contains(file)) // file.EndsWith(".trec") || 
                    {
                        OnNewFileDetected(new NewFileDetectedEventArgs() { FileName = file, Course = course });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not crawl observed folder: " + course.SourceFolder);
            }
        }

        public virtual void OnNewFileDetected(NewFileDetectedEventArgs e)
        {
            if (e.FileName.EndsWith(".json"))
            {
                var slides = e.FileName.Replace("_meta.json", "_slides.mp4");
                var th = e.FileName.Replace("_meta.json", "_talkinghead.mp4");

                if (!File.Exists(slides) || !File.Exists(th))
                {
                    return;
                }
            }

            detectedFiles.Add(e.FileName);

            NewFileDetected?.Invoke(this, e);
        }
    }
}
