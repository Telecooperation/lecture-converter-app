using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TkRecordingConverter.util
{
    public class FFmpegHelper
    {
        public static Process FFmpeg(string args)
        {
            return FFmpeg(args, true);
        }

        public static Process FFmpeg(string args, bool redirectOutput)
        {
            return BuildProcess(CommandForPlatform("ffmpeg"), args, redirectOutput);
        }

        public static String CommandForPlatform(String command)
        {
            string os = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                os = "win";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                os = "unix";

            string architecture = null;
            if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                architecture = "x86";
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                architecture = "x64";

            return Path.Combine("resources", "ffmpeg", os, architecture, command);
        }

        public static Process BuildProcess(String command, String args, bool redirectOutput)
        {
            Console.WriteLine("Executing " + command + " " + args);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = redirectOutput,
                    RedirectStandardError = redirectOutput
                    //CreateNoWindow = true
                }
            };

            return proc;
        }
    }
}
