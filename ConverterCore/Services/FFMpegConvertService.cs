using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecordingProcessor.Model;
using RecordingProcessor.Studio;
using RecordingProcessor.Utils;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ConverterCore.Services
{
    public class FFMpegConvertService
    {
        private readonly ILogger<FFMpegConvertService> _logger;
        private readonly ChromaKeyParamGuesser chromaKeyParamGuesser;
        private readonly MediaConverter converter;

        public FFMpegConvertService(
            ILogger<FFMpegConvertService> logger, 
            ChromaKeyParamGuesser chromaKeyParamGuesser,
            MediaConverter converter)
        {
            this._logger = logger;
            this.chromaKeyParamGuesser = chromaKeyParamGuesser;
            this.converter = converter;
        }

        public bool ConvertTrecRecording(string inputFileName, string outputFileName)
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
                    recordingStyle.ChromaKeyParams.color = chromaKeyParamGuesser.GuessChromaKeyParams(thVideoPath);
                }

                if (ckInfo["similarity"] != null)
                    recordingStyle.ChromaKeyParams.similarity = ckInfo["similarity"];
                if (ckInfo["blend"] != null)
                    recordingStyle.ChromaKeyParams.blend = ckInfo["blend"];
            }
            else
            {
               recordingStyle.ChromaKeyParams.color = chromaKeyParamGuesser.GuessChromaKeyParams(thVideoPath);
            }

            var config = new ConversionConfiguration()
            {
                SlideVideoPath = slideVideoPath,
                TalkingHeadVideoPath = thVideoPath,
                MetadataPath = inputFileName,
                OutputDirectory = outputFolder,
                ProjectName = targetFileName.Replace("_meta.json", ""),
                RecordingStyle = recordingStyle,
                ExportJson = false
            };

            var recording = converter.ConvertMedia(config);
            return recording;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.LogDebug(e.Data);
        }
    }
}
