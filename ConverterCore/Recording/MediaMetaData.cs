using ConverterCore.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConverterCore.Recordings
{
    public class MediaMetaData
    {
        public string FilePath { get; set; }

        public Course Course { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(MediaMetaData))
            {
                return (obj as MediaMetaData).FilePath == this.FilePath;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return FilePath.GetHashCode();
        }
    }
}
