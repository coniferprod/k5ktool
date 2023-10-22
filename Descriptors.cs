using System.Collections.Generic;

namespace K5KTool
{
    public class SourceDescriptor
    {
        public ushort WaveNumber { get; set; }
        public string WaveformTemplateName { get; set; }
        public string HarmonicLevelTemplateName { get; set; }
        public string HarmonicEnvelopeTemplateName { get; set; }
        public string FormantFilterTemplateName { get; set; }
    }

    public class SinglePatchDescriptor
    {
        public string Name { get; set; }
        public List<SourceDescriptor> Sources { get; set; }

        public SinglePatchDescriptor()
        {
            Name = "NewSound";
            Sources = new List<SourceDescriptor>();

            SourceDescriptor s1d = new SourceDescriptor();
            s1d.HarmonicLevelTemplateName = "Saw soft";
            s1d.HarmonicEnvelopeTemplateName = "E-Piano";
            Sources.Add(s1d);

            SourceDescriptor s2d = new SourceDescriptor();
            s1d.WaveNumber = 411;  // "Syn Saw1 Cyc"
            s1d.FormantFilterTemplateName = "Init";
            Sources.Add(s2d);
        }
    }
}