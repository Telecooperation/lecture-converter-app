using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterCore.Recording
{
    public class NewFileDetectedEventArgs : EventArgs
    {
        public string FileName { get; set; }
    }
}
