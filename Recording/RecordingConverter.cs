using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LectureRecordingConverter.Converter
{
    public class RecordingConverter
    {
        public static void ConvertRecording(string inputFileName, string outputFileName)
        {
            var arguments = new StringBuilder();
            arguments.Append("-i");
            arguments.Append(" ");
            arguments.Append("\"" + inputFileName + "\"");
            arguments.Append(" -f mp4 -vcodec libx264 -preset ultrafast -profile:v main -pix_fmt yuv420p -acodec aac ");
            arguments.Append("\"" + outputFileName + "\"");

            // run ffmpeg
            var process = Process.Start("ffmpeg\\ffmpeg.exe", arguments.ToString());
            process.WaitForExit();

        }
    }
}
