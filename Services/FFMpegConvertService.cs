using ConverterCore.Model;
using ConverterCore.Studio;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        private readonly Converter converter;

        public FFMpegConvertService(ILogger<FFMpegConvertService> logger, Converter converter)
        {
            this._logger = logger;
            this.converter = converter;
        }

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
            // identify file paths
            var slideVideoPath = inputFileName.Replace("_meta.json", "_slides.mp4");
            var thVideoPath = inputFileName.Replace("_meta.json", "_talkinghead.mp4");
            var ckFile = inputFileName.Replace("_meta.json", ".ckparams");

            // setup of recording
            var targetDimension = Dimension.Dim720p;
            var recordingStyle = RecordingStyle.TkStudioStyle(targetDimension);

            if (File.Exists(ckFile))
            {
                dynamic ckInfo = JsonConvert.DeserializeObject(File.ReadAllText(ckFile));

                if (ckInfo["color"] != null)
                    recordingStyle.ChromaKeyParams.color = ckInfo["color"];
                else
                {
                    ChromaKeyParamGuesser g = new ChromaKeyParamGuesser();
                    recordingStyle.ChromaKeyParams.color = g.GuessChromaKeyParams(thVideoPath);
                }

                if (ckInfo["similarity"] != null)
                    recordingStyle.ChromaKeyParams.similarity = ckInfo["similarity"];
                if (ckInfo["blend"] != null)
                    recordingStyle.ChromaKeyParams.blend = ckInfo["blend"];
            }
            else
            {
                var chromaKeyParamGuesser = new ChromaKeyParamGuesser();
                recordingStyle.ChromaKeyParams.color = chromaKeyParamGuesser.GuessChromaKeyParams(thVideoPath);
            }

            var config = new Configuration()
            {
                slideVideoPath = slideVideoPath,
                thVideoPath = thVideoPath,
                slideInfoPath = inputFileName,
                outputDir = outputFolder,
                projectName = targetFileName.Replace("_meta.json", ""),
                recordingStyle = recordingStyle,
                writeJson = false
            };

            var recording = converter.Convert(config);
            return recording;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.LogDebug(e.Data);
        }
    }
}
