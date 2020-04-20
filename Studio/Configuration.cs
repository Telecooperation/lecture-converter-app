using System;
using System.Collections.Generic;
using System.Text;

namespace TkRecordingConverter.util
{
    public struct Configuration
    {
        public string slideVideoPath;
        public string thVideoPath;
        public string slideInfoPath;
        public string outputDir;
        public string projectName;
        public RecordingStyle recordingStyle;
        public bool writeJson;
    }
}
