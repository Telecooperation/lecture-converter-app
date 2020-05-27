using System;

namespace RecordingProcessor.Model
{
    public class Recording
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public string FileName { get; set; }

        public string PresenterFileName { get; set; }

        public string StageVideo { get; set; }

        public bool Processing { get; set; }

        public Slide[] Slides { get; set; }

        public double Duration { get; set; }
    }
}
