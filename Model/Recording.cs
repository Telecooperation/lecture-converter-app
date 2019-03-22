using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterCore.Model
{
    public class Recording
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public string FileName { get; set; }

        public bool Processing { get; set; }
    }
}
