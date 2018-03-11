using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter.Settings
{
    public class Settings
    {
        public List<String> SourceFolders { get; set; }

        public List<String> TargetFolders { get; set; }

        public List<String> LectureNames { get; set; }

        public static Settings LoadSettings()
        {
            // deserialize JSON directly from a file
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
        }
    }
}
