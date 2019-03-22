using ConverterCore.Recording;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ConverterCore
{
    public class App
    {
        private readonly ILogger<App> _logger;

        private RecordingConverter _recordingConverter;

        private RecordingWatcher _recordingWatcher;

        public App(ILogger<App> logger, RecordingConverter recordingConverter, RecordingWatcher recordingWatcher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _recordingConverter = recordingConverter ?? throw new ArgumentNullException(nameof(recordingConverter));
            _recordingWatcher = recordingWatcher ?? throw new ArgumentNullException(nameof(recordingWatcher));
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

            for (int i = 0; i < settings.LectureNames.Count; i++)
            {
                var sourceFolder = settings.SourceFolders[i];
                _recordingWatcher.AddWatcher(sourceFolder);

                _logger.LogInformation("Observing: {0}", sourceFolder);
            }

            _logger.LogInformation("Converter is running, and waiting for new files...");
            await Task.CompletedTask;
        }

        private void Watcher_NewFileDetected(object sender, NewFileDetectedEventArgs e)
        {
            _recordingConverter.AddFile(e.FileName);
        }
    }
}
