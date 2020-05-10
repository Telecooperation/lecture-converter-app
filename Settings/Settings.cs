using Newtonsoft.Json;
using PushBulletSharp.Core;
using PushBulletSharp.Core.Models.Requests;
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

        public string PushbulletAccessToken { get; set; }

        public string PushbulletEmail { get; set; }

        public static Settings LoadSettings()
        {
            // deserialize JSON directly from a file
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine("config", "settings.json")));
        }

        public async static Task PushMessage(string title, string body)
        {
            var settings = LoadSettings();

            var pushbullet = new PushBulletClient(settings.PushbulletAccessToken);
            var request = new PushNoteRequest()
            {
                Email = settings.PushbulletEmail,
                Title = title,
                Body = body
            };

            await pushbullet.PushNote(request);
        }
    }
}
