using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ConverterCore.Services
{
    public class FFMpegConvertService
    {
        private readonly ILogger<FFMpegConvertService> _logger;

        public bool ConvertSingleFile(string inputFileName, string outputFileName)
        {
            var arguments = new StringBuilder();
            arguments.Append("-i");
            arguments.Append(" ");
            arguments.Append("\"" + inputFileName + "\"");
            arguments.Append(" -f mp4 -vcodec libx264 -tune stillimage -profile:v baseline -level 3.0 -pix_fmt yuv420p -acodec aac ");
            arguments.Append("\"" + outputFileName + "\"");

            // run ffmpeg
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var process = Process.Start(Path.Combine("ffmpeg", "win", "ffmpeg"), arguments.ToString());
                process.OutputDataReceived += Process_OutputDataReceived;
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var process = Process.Start(Path.Combine("ffmpeg", "unix", "ffmpeg"), arguments.ToString());
                process.OutputDataReceived += Process_OutputDataReceived;
                process.WaitForExit();

                return process.ExitCode == 0;
            }

            return false;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.LogDebug(e.Data);
        }
    }
}
