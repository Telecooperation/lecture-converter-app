using System.Collections.Generic;

namespace RecordingProcessor.Model
{
    public class Lecture
    {
        public string Name { get; set; }

        public string Semester { get; set; }

        public List<Recording> Recordings { get; set; }
    }
}
