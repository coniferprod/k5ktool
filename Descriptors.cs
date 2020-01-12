using System;
using System.Collections.Generic;

namespace K5KTool
{
    public class SourceDescriptor
    {
        public int WaveNumber { get; set; }
        public string WaveformTemplateName { get; set; }
        public string HarmonicLevelTemplateName { get; set; }
        public string HarmonicEnvelopeTemplateName { get; set; }
        public string FormantFilterTemplateName { get; set; }
    }

    public class SinglePatchDescriptor
    {
        public string Name { get; set; }
        public List<SourceDescriptor> Sources { get; set; }
    }
}