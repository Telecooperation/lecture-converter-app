using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter.Model
{
    public class Lecture
    {
        public string Name { get; set; }

        public string Semester { get; set; }

        public List<Recording> Recordings { get; set; }


        public static Lecture LoadSettings(string lectureName, string inputFile)
        {
            // deserialize JSON directly from a file
            if (File.Exists(inputFile))
            {
                return JsonConvert.DeserializeObject<Lecture>(File.ReadAllText(inputFile));
            }
            else
            {
                return new Lecture()
                {
                    Name = lectureName,
                    Semester = Utils.GetSemester(DateTime.Now),
                    Recordings = new List<Recording>()
                };
            }
        }

        public static void SaveSettings(Lecture obj, string outputFile)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // sort recordings
            obj.Recordings = obj.Recordings.OrderBy(x => DateTime.Parse(x.Date)).ToList();

            // deserialize JSON directly from a file
            var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            File.WriteAllText(outputFile, jsonString);
        }
    }
}
