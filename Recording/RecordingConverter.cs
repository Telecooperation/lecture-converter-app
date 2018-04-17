using log4net;
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
        private static readonly ILog logger = LogManager.GetLogger(typeof(RecordingConverter));

        public static void ConvertRecording(string inputFileName, string outputFileName)
        {
            var arguments = new StringBuilder();
            arguments.Append("-i");
            arguments.Append(" ");
            arguments.Append("\"" + inputFileName + "\"");
            arguments.Append(" -f mp4 -vcodec libx264 -tune stillimage -profile:v baseline -level 3.0 -pix_fmt yuv420p -acodec aac ");
            arguments.Append("\"" + outputFileName + "\"");

            // run ffmpeg
            var process = Process.Start("ffmpeg\\ffmpeg.exe", arguments.ToString());
            process.OutputDataReceived += Process_OutputDataReceived;
            process.WaitForExit();
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Debug(e.Data);
        }
    }
}
