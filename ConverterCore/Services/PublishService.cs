using ConverterCore.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecordingProcessor.Model;
using System;
using System.IO;
using System.Threading;

namespace ConverterCore.Services
{
    public class PublishService
    {
        private readonly ILogger<PublishService> _logger;

        private Timer bgTask;

        public PublishService(ILogger<PublishService> logger)
        {
            this._logger = logger;
        }

        public void RunPublishQueue()
        {
            // do scanning the folder every now and then
            bgTask = new Timer(x => PublishCourses(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        public void PublishCourses()
        {
            var courses = Settings.Settings.LoadSettings();

            foreach (var course in courses.Courses)
            {
                PublishCourse(course);
            }
        }

        public void PublishCourse(Course course)
        {
            try
            {
                if (string.IsNullOrEmpty(course.ConvertFolder))
                    return;

                var directory = new DirectoryInfo(course.ConvertFolder);

                foreach (var videoDirectory in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if (!File.Exists(Path.Combine(videoDirectory.FullName, "meta.json")))
                        continue;

                    // read json
                    var recording = JsonConvert.DeserializeObject<Recording>(File.ReadAllText(Path.Combine(videoDirectory.FullName, "meta.json")));

                    if (recording.Date <= DateTime.Now)
                    {
                        // do publishing
                        _logger.LogInformation($"Publishing {videoDirectory.Name} of {course.Name}");

                        // move files
                        videoDirectory.MoveTo(Path.Combine(course.TargetFolder, "video", videoDirectory.Name));

                        // add lecture to json
                        var lecture = MetaDataService.LoadSettings(course);
                        lecture.Recordings.Add(recording);
                        MetaDataService.SaveSettings(lecture, course);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not publish {course.Name}: {ex.Message}");
            }
        }
    }
}
