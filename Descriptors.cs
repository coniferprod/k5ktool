using System;
using System.Collections.Generic;

namespace K5KTool
{
    public class SourceDescriptor
    {
        public int WaveNumber { get; set; }
        public string WaveformName { get; set; }
        public string HarmonicEnvelopeName { get; set; }   
    }

    public class SinglePatchDescriptor
    {
        public string Name { get; set; }
        public string Bank { get; set; }
        public int PatchNumber { get; set; }
        public List<SourceDescriptor> Sources { get; set; }
    }
}