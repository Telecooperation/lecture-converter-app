using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter.Settings
{
    public class ConvertedFiles
    {
        public List<String> Files { get; set; }

        public static ConvertedFiles LoadFiles()
        {
            // deserialize JSON directly from a file
            if (File.Exists("converted_files.json"))
            {
                return JsonConvert.DeserializeObject<ConvertedFiles>(File.ReadAllText("converted_files.json"));
            }

            return new ConvertedFiles()
            {
                Files = new List<string>()
            };
        }
    }
}
