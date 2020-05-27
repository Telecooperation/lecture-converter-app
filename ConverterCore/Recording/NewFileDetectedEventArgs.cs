using ConverterCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterCore.Recordings
{
    public class NewFileDetectedEventArgs : EventArgs
    {
        public string FileName { get; set; }

        public Course Course { get; set; }
    }
}
