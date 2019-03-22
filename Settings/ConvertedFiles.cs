using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConverterCore.Settings
{
    public class ConvertedFiles
    {
        public List<String> Files { get; set; }

        public static ConvertedFiles LoadFiles()
        {
            // deserialize JSON directly from a file
            if (File.Exists(Path.Combine("config", "converted_files.json")))
            {
                return JsonConvert.DeserializeObject<ConvertedFiles>(File.ReadAllText(Path.Combine("config", "converted_files.json")));
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
            File.WriteAllText(Path.Combine("config", "converted_files.json"), jsonString);
        }
    }
}
