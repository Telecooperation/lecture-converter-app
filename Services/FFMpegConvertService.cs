using ConverterCore.Model;
using ConverterCore.Studio;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TkRecordingConverter.util;

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
            var process = FFmpegHelper.FFmpeg(arguments.ToString(), false);
            process.OutputDataReceived += Process_OutputDataReceived;
            process.WaitForExit();

            return process.ExitCode == 0;
        }

        public Recording ConvertStudioRecording(string inputFileName, string targetFileName, string outputFolder)
        {
            var slideVideoPath = inputFileName.Replace("_meta.json", "_slides.mp4");
            var thVideoPath = inputFileName.Replace("_meta.json", "_talkinghead.mp4");

            var targetDimension = Dimension.Dim720p;

            var converter = new Converter();
            var recording = converter.Convert(new TkRecordingConverter.util.Configuration()
            {
                slideVideoPath = slideVideoPath,
                thVideoPath = thVideoPath,
                slideInfoPath = inputFileName,
                outputDir = outputFolder,
                projectName = targetFileName.Replace("_meta.json", ""),
                recordingStyle = RecordingStyle.TkStudioStyle(targetDimension),
                writeJson = false
            });

            return recording;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.LogDebug(e.Data);
        }
    }
}
