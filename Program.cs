using Converter.Recording;
using System;

namespace Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lecture Converter starting...");

            // load settings
            var settings = Settings.Settings.LoadSettings();

            var watcher = new RecordingWatcher();
            for(int i = 0; i < settings.LectureNames.Count; i++)
            {
                var sourceFolder = settings.SourceFolders[i];
                watcher.AddWatcher(sourceFolder);

                Console.WriteLine("Observing: " + sourceFolder);
            }

            Console.WriteLine("Converter is running, and waiting for new files...");
            Console.WriteLine("Press any key to close.");
            Console.ReadLine();
        }
    }
}
