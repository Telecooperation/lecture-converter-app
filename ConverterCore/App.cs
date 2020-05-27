using ConverterCore.Recordings;
using ConverterCore.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConverterCore
{
    public class App
    {
        private readonly ILogger<App> _logger;

        private RecordingConverter _recordingConverter;

        private CourseWatcher _recordingWatcher;

        private PublishService _publishService;

        public App(ILogger<App> logger, 
            RecordingConverter recordingConverter, 
            CourseWatcher recordingWatcher,
            PublishService publishService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _recordingConverter = recordingConverter ?? throw new ArgumentNullException(nameof(recordingConverter));
            _recordingWatcher = recordingWatcher ?? throw new ArgumentNullException(nameof(recordingWatcher));
            _publishService = publishService ?? throw new ArgumentNullException(nameof(publishService));
        }

        public async Task Run()
        {
            _logger.LogInformation("Lecture Converter starting...");

            // load settings
            var settings = Settings.Settings.LoadSettings();

            // run recording queue
            _recordingConverter.RunConversionQueue();

            // load watcher
            _recordingWatcher.NewFileDetected += Watcher_NewFileDetected;

            foreach (var course in settings.Courses)
            {
                // create folders
                Directory.CreateDirectory(Path.Combine(course.TargetFolder, "video"));
                Directory.CreateDirectory(Path.Combine(course.TargetFolder, "assets"));

                _recordingWatcher.AddWatcher(course);

                _logger.LogInformation("Observing: {0}", course.SourceFolder);
            }

            // start publish service
            _publishService.RunPublishQueue();

            _logger.LogInformation("Converter is running, and waiting for new files...");
            await Task.CompletedTask;
        }

        private void Watcher_NewFileDetected(object sender, NewFileDetectedEventArgs e)
        {
            _recordingConverter.QueueFile(new MediaMetaData() { FilePath = e.FileName, Course = e.Course });
        }
    }
}
