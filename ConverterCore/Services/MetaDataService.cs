using ConverterCore.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RecordingProcessor.Model;
using RecordingProcessor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConverterCore.Services
{
    public class MetaDataService
    {
        public static Lecture LoadSettings(Course course)
        {
            // deserialize JSON directly from a file
            var courseJsonFilePath = Path.Combine(course.TargetFolder, "assets", "lecture.json");

            if (File.Exists(courseJsonFilePath))
            {
                return JsonConvert.DeserializeObject<Lecture>(File.ReadAllText(courseJsonFilePath));
            }
            else
            {
                return new Lecture()
                {
                    Name = course.Name,
                    Semester = StringUtils.GetSemester(DateTime.Now),
                    Recordings = new List<Recording>()
                };
            }
        }

        public static void SaveSettings(Lecture obj, Course course)
        {
            var courseJsonFilePath = Path.Combine(course.TargetFolder, "assets", "lecture.json");

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // sort recordings
            obj.Recordings = obj.Recordings.OrderByDescending(x => x.Date).ToList();

            // deserialize JSON directly from a file
            var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            File.WriteAllText(courseJsonFilePath, jsonString);
        }
    }
}
