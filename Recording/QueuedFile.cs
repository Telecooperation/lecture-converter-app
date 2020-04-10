using ConverterCore.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConverterCore.Recording
{
    public class QueuedFile
    {
        public string FilePath { get; set; }

        public Course Course { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(QueuedFile))
            {
                return (obj as QueuedFile).FilePath == this.FilePath;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return FilePath.GetHashCode();
        }
    }
}
