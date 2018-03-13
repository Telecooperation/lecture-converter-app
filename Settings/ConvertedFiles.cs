using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

        public static void SaveFiles(ConvertedFiles files)
        {
            // deserialize JSON directly from a file
            var jsonString = JsonConvert.SerializeObject(files, Formatting.Indented);
            File.WriteAllText("converted_files.json", jsonString);
        }
    }
}
