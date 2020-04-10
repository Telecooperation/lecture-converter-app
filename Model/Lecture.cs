using ConverterCore.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterCore.Model
{
    public class Lecture
    {
        public string Name { get; set; }

        public string Semester { get; set; }

        public List<Recording> Recordings { get; set; }


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
                    Semester = Utils.GetSemester(DateTime.Now),
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
