using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ConverterCore.Settings
{
    public class Settings
    {
        public List<Course> Courses { get; set; }

        public static Settings LoadSettings()
        {
            // deserialize JSON directly from a file
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine("config", "settings.json")));
        }
    }
}
