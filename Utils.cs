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
                    return "Winter term " + date.Year + " / " + (date.Year + 1);
                }
                else
                {
                    return "Winter term " + (date.Year - 1) + " / " + date.Year;
                }
            }
        }

        public static string GetCleanTitleFromFileName(string fileName)
        {
            string name = fileName.Replace(".mp4", "").Replace(".trec", "");
            name = name.Replace("-", " ");

            string[] splitName = name.Split(' ');
            if(splitName[splitName.Length - 1].ToLower().Contains("ws") ||
                splitName[splitName.Length - 1].ToLower().Contains("ss"))
            {
                name = "";

                for(int i = 0; i < splitName.Length - 1; i++)
                {
                    name += splitName[i] + " ";
                }

                name = name.Trim();
            }

            return name;
        }
    }
}
