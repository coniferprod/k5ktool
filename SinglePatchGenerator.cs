using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using KSynthLib.K5000;

namespace K5KTool
{
    public class HarmonicLevels
    {
        public List<int> Low;
        public List<int> High;
    }

    public class HarmonicLevelTemplate
    {
        public string Name;
        public HarmonicLevels Levels;
    }

    public class HarmonicEnvelopeTemplate
    {
        public string Name;
        public HarmonicEnvelope Envelope;
    }

    public class FormantFilterTemplate
    {
        public string Name;
        public List<int> Levels;
    }

    public class Templates
    {
        public List<HarmonicLevelTemplate> HarmonicLevels;
        public List<HarmonicEnvelopeTemplate> HarmonicEnvelopes;
        public List<FormantFilterTemplate> FormantFilters;
    }

    public class SinglePatchGenerator
    {
        private Templates templates;

        private Dictionary<string, HarmonicLevels> harmonicLevelTemplates;
        private Dictionary<string, HarmonicEnvelope> harmonicEnvelopeTemplates;
        private Dictionary<string, List<int>> formantFilterTemplates;

        public SinglePatchDescriptor Descriptor;

        public SinglePatchGenerator(SinglePatchDescriptor descriptor)
        {
            this.harmonicLevelTemplates = new Dictionary<string, HarmonicLevels>();
            this.harmonicEnvelopeTemplates = new Dictionary<string, HarmonicEnvelope>();
            this.formantFilterTemplates = new Dictionary<string, List<int>>();

            this.Descriptor = descriptor;

            this.templates = JsonConvert.DeserializeObject<Templates>(File.ReadAllText(@"templates.json"));
            Console.WriteLine($"Templates: {this.templates.HarmonicLevels.Count} harmonic level templates, {this.templates.HarmonicEnvelopes.Count} harmonic envelopes, {this.templates.FormantFilters.Count} formant filters");

            foreach (HarmonicLevelTemplate hlt in this.templates.HarmonicLevels)
            {
                this.harmonicLevelTemplates.Add(hlt.Name, hlt.Levels);
            }

            foreach (HarmonicEnvelopeTemplate het in this.templates.HarmonicEnvelopes)
            {
                this.harmonicEnvelopeTemplates.Add(het.Name, het.Envelope);
            }

            foreach (FormantFilterTemplate fft in this.templates.FormantFilters)
            {
                this.formantFilterTemplates.Add(fft.Name, fft.Levels);
            }

        }

        public SinglePatch Generate()
        {
            SinglePatch single = new SinglePatch();

            single.SingleCommon.Name = Descriptor.Name;
            single.SingleCommon.Volume = 115;
            single.SingleCommon.SourceCount = Descriptor.Sources.Count;
            single.SingleCommon.IsPortamentoEnabled = false;
            single.SingleCommon.PortamentoSpeed = 0;

            single.Sources = new Source[single.SingleCommon.SourceCount];

            for (int i = 0; i < single.SingleCommon.SourceCount; i++)
            {
                single.Sources[i] = GenerateSource(Descriptor.Sources[i]);
            }

            return single;
        }

        private Source GenerateSource(SourceDescriptor descriptor)
        {
            if (descriptor.WaveNumber == AdditiveKit.WaveNumber)
            {
                return GenerateAdditiveSource(
                    descriptor.WaveformTemplateName,
                    descriptor.HarmonicLevelTemplateName,
                    descriptor.HarmonicEnvelopeTemplateName,
                    descriptor.FormantFilterTemplateName);
            }
            else
            {
                return GeneratePCMSource(descriptor.WaveNumber);
            }
        }

        private Source GenerateAdditiveSource(string waveformTemplateName, string harmonicLevelTemplateName, string harmonicEnvelopeTemplateName, string formantFilterTemplateName)
        {
            Source source = new Source();
            source.ZoneLow = 0;
            source.ZoneHigh = 127;
            VelocitySwitchSettings vel = new VelocitySwitchSettings();
            vel.SwitchType = VelocitySwitchType.Off;
            vel.Threshold = 68;
            source.VelocitySwitch = vel;

            source.Volume = 120;
            source.KeyOnDelay = 0;
            source.EffectPath = 1;
            source.BenderCutoff = 12;
            source.BenderPitch = 2;
            source.Pan = PanType.Normal;
            source.PanValue = 0;

            // DCO settings for additive source
            source.DCO.Wave =  new Wave(AdditiveKit.WaveNumber);
            source.DCO.Coarse = 0;
            source.DCO.Fine = 0;
            source.DCO.KSPitch = KeyScalingToPitch.ZeroCent;
            source.DCO.FixedKey = 0; // OFF

            PitchEnvelope pitchEnv = new PitchEnvelope();
            pitchEnv.StartLevel = 0;
            pitchEnv.AttackTime = 4;
            pitchEnv.AttackLevel = 0;
            pitchEnv.DecayTime = 64;
            pitchEnv.LevelVelocitySensitivity = 0;
            pitchEnv.TimeVelocitySensitivity = 0;
            source.DCO.Envelope = pitchEnv;

            // DCF
            source.DCF.IsActive = true;
            source.DCF.Cutoff = 55;
            source.DCF.Resonance = 0;
            source.DCF.Level = 7;
            source.DCF.Mode = FilterMode.LowPass;
            source.DCF.VelocityCurve = 5;

            // DCF Envelope
            FilterEnvelope filterEnv = new FilterEnvelope();
            source.DCF.EnvelopeDepth = 25;
            filterEnv.AttackTime = 0;
            filterEnv.Decay1Time = 120;
            filterEnv.Decay1Level = 63;
            filterEnv.Decay2Time = 80;
            filterEnv.Decay2Level = 63;
            filterEnv.ReleaseTime = 20;
            source.DCF.Envelope = filterEnv;
            // DCF Modulation:
            source.DCF.KSToEnvAttackTime = 0;
            source.DCF.KSToEnvDecay1Time = 0;
            source.DCF.VelocityToEnvDepth = 30;
            source.DCF.VelocityToEnvAttackTime = 0;
            source.DCF.VelocityToEnvDecay1Time = 0;

            // DCA Envelope
            AmplifierEnvelope ampEnv = new AmplifierEnvelope();
            ampEnv.AttackTime = 1;
            ampEnv.Decay1Time = 94;
            ampEnv.Decay1Level = 127;
            ampEnv.Decay2Time = 80;
            ampEnv.Decay2Level = 63;
            ampEnv.ReleaseTime = 20;
            source.DCA.Envelope = ampEnv;

            // DCA Modulation
            source.DCA.KeyScaling.Level = 0;
            source.DCA.KeyScaling.AttackTime = 0;
            source.DCA.KeyScaling.Decay1Time = 0;
            source.DCA.KeyScaling.ReleaseTime = 0;

            source.DCA.VelocitySensitivity.Level = 20;
            source.DCA.VelocitySensitivity.AttackTime = 20;
            source.DCA.VelocitySensitivity.Decay1Time = 20;
            source.DCA.VelocitySensitivity.ReleaseTime = 20;

            // Harmonic levels
            if (!string.IsNullOrEmpty(waveformTemplateName))
            {
                int numHarmonics = 64;
                byte[] levels = WaveformEngine.GetHarmonicLevels(waveformTemplateName, numHarmonics, 127);  // levels are 0...127
                source.ADD.SoftHarmonics = levels;

                Console.WriteLine(String.Format("waveform template = {0}", waveformTemplateName));

                /*
                Console.WriteLine(String.Format("{0}, {1} harmonics:", waveformName, numHarmonics));
                for (int i = 0; i < levels.Length; i++)
                {
                    Console.WriteLine(String.Format("{0} = {1}", i + 1, levels[i]));
                }
                */
            }
            else if (!string.IsNullOrEmpty(harmonicLevelTemplateName))
            {
                Console.WriteLine(String.Format("harmonic template = {0}", harmonicLevelTemplateName));

                int[] softLevels = this.harmonicLevelTemplates[harmonicLevelTemplateName].Low.ToArray();
                source.ADD.SoftHarmonics = softLevels.Select(i => (byte) i).ToArray();

                int[] loudLevels = this.harmonicLevelTemplates[harmonicLevelTemplateName].High.ToArray();
                source.ADD.LoudHarmonics = loudLevels.Select(i => (byte) i).ToArray();
            }
            else
            {
                Console.WriteLine("No template specified for waveform or harmonic levels, using defaults");

                int[] softLevels = this.harmonicLevelTemplates["Init"].Low.ToArray();
                source.ADD.SoftHarmonics = softLevels.Select(i => (byte) i).ToArray();

                int[] loudLevels = this.harmonicLevelTemplates["Init"].High.ToArray();
                source.ADD.LoudHarmonics = loudLevels.Select(i => (byte) i).ToArray();
            }

            // Harmonic envelopes. Initially assign the same envelope for each harmonic.
            string harmEnvName = harmonicEnvelopeTemplateName;
            if (string.IsNullOrEmpty(harmonicEnvelopeTemplateName))
            {
                harmEnvName = "Init";
            }
            Console.WriteLine(String.Format("harmonic envelope template = {0}", harmEnvName));
            HarmonicEnvelope harmEnv = this.harmonicEnvelopeTemplates[harmEnvName];
            for (int i = 0; i < AdditiveKit.HarmonicCount; i++)
            {
                source.ADD.HarmonicEnvelopes[i] = harmEnv;
            }

            // Formant filter
            string formantName = formantFilterTemplateName;
            if (string.IsNullOrEmpty(formantFilterTemplateName))
            {
                formantName = "Init";
            }
            Console.WriteLine(String.Format("formant filter template = {0}", formantName));
            int[] formantLevels = this.formantFilterTemplates[formantName].ToArray();
            source.ADD.FormantFilter = formantLevels.Select(i => (byte) i).ToArray();

            return source;
        }

        private Source GeneratePCMSource(ushort waveNumber)
        {
            Source source = new Source();

            VelocitySwitchSettings vel = new VelocitySwitchSettings();
            vel.SwitchType = VelocitySwitchType.Off;
            vel.Threshold = 68;
            source.VelocitySwitch = vel;

            source.Volume = 120;
            source.KeyOnDelay = 0;
            source.EffectPath = 1;
            source.BenderCutoff = 12;
            source.BenderPitch = 2;
            source.Pan = PanType.Normal;
            source.PanValue = 0;

            // DCO
            source.DCO.Wave = new Wave(waveNumber);
            source.DCO.Coarse = 0;
            source.DCO.Fine = 0;
            source.DCO.KSPitch = KeyScalingToPitch.ZeroCent;
            source.DCO.FixedKey = 0; // OFF

            PitchEnvelope pitchEnv = new PitchEnvelope();
            pitchEnv.StartLevel = 0;
            pitchEnv.AttackTime = 4;
            pitchEnv.AttackLevel = 0;
            pitchEnv.DecayTime = 64;
            pitchEnv.LevelVelocitySensitivity = 0;
            pitchEnv.TimeVelocitySensitivity = 0;
            source.DCO.Envelope = pitchEnv;

            // DCF
            source.DCF.IsActive = true;
            source.DCF.Cutoff = 55;
            source.DCF.Resonance = 0;
            source.DCF.Level = 7;
            source.DCF.Mode = FilterMode.LowPass;
            source.DCF.VelocityCurve = 5;

            // DCF Envelope
            FilterEnvelope filterEnv = new FilterEnvelope();
            source.DCF.EnvelopeDepth = 25;
            filterEnv.AttackTime = 0;
            filterEnv.Decay1Time = 120;
            filterEnv.Decay1Level = 63;
            filterEnv.Decay2Time = 80;
            filterEnv.Decay2Level = 63;
            filterEnv.ReleaseTime = 20;
            source.DCF.Envelope = filterEnv;

            // DCF Modulation:
            source.DCF.KSToEnvAttackTime = 0;
            source.DCF.KSToEnvDecay1Time = 0;
            source.DCF.VelocityToEnvDepth = 30;
            source.DCF.VelocityToEnvAttackTime = 0;
            source.DCF.VelocityToEnvDecay1Time = 0;

            // DCA Envelope
            AmplifierEnvelope ampEnv = new AmplifierEnvelope();
            ampEnv.AttackTime = 1;
            ampEnv.Decay1Time = 94;
            ampEnv.Decay1Level = 127;
            ampEnv.Decay2Time = 80;
            ampEnv.Decay2Level = 63;
            ampEnv.ReleaseTime = 15;
            source.DCA.Envelope = ampEnv;

            // DCA Modulation
            source.DCA.KeyScaling.Level = 0;
            source.DCA.KeyScaling.AttackTime = 0;
            source.DCA.KeyScaling.Decay1Time = 0;
            source.DCA.KeyScaling.ReleaseTime = 0;

            source.DCA.VelocitySensitivity.Level = 20;
            source.DCA.VelocitySensitivity.AttackTime = 0;
            source.DCA.VelocitySensitivity.Decay1Time = 0;
            source.DCA.VelocitySensitivity.ReleaseTime = 0;

            return source;
        }
    }
}