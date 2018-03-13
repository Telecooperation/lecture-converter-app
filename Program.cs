using Converter.Recording;
using log4net;
using log4net.Config;
using System;

namespace Converter
{
    class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            BasicConfigurator.Configure();

            logger.Info("Lecture Converter starting...");

            // load settings
            var settings = Settings.Settings.LoadSettings();

            var watcher = new RecordingWatcher();
            for (int i = 0; i < settings.LectureNames.Count; i++)
            {
                var sourceFolder = settings.SourceFolders[i];
                watcher.AddWatcher(sourceFolder);

                logger.InfoFormat("Observing: {0}", sourceFolder);
            }

            logger.Info("Converter is running, and waiting for new files...");
            logger.Info("Press any key to close.");
            Console.ReadLine();
        }
    }
}
