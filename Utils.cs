using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter
{
    public class Utils
    {
        public static string GetSemester(DateTime date)
        {
            if (date.Month >= 4 && date.Month < 10)
            {
                return "Summer term " + date.Year;
            }
            else
            {
                if (date.Month < 10)
                {
                    return "Winter term " + (date.Year - 1) + " / " + date.Year;
                }
                else
                {
                    return "Winter term " + date.Year + " / " + (date.Year + 1);
                }
            }
        }

        public static string GetCleanTitleFromFileName(string fileName)
        {
            string name = fileName.Replace(".mp4", "").Replace(".trec", "");
            name = name.Replace("-", " ").Replace("_", " ");

            string[] splitName = name.Split(' ');

            string result = "";
            for (int i = 0; i < splitName.Length; i++)
            {
                var part = splitName[i];

                if (part.ToLower().StartsWith("ws") || part.ToLower().StartsWith("ss"))
                {
                    // ignore
                }
                else
                {
                    result += part + " ";
                }
            }

            return result.Trim();
        }
    }
}
