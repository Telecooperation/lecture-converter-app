using System;
using System.Collections.Generic;
using System.Text;

namespace ConverterCore.Settings
{
    public class Course
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string SourceFolder { get; set; }

        public string TargetFolder { get; set; }

        public bool Studio { get; set; }
    }
}
