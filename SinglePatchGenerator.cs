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

            single.SingleCommon.Name = new PatchName(Descriptor.Name);
            single.SingleCommon.Volume.Value = 115;
            single.SingleCommon.SourceCount = Descriptor.Sources.Count;
            single.SingleCommon.IsPortamentoEnabled = false;
            single.SingleCommon.PortamentoSpeed.Value = 0;

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
            source.Zone = new Zone(0, 127);
            VelocitySwitchSettings vel = new VelocitySwitchSettings();
            vel.SwitchKind = VelocitySwitchKind.Off;
            vel.Threshold = 68;
            source.VelocitySwitch = vel;

            source.Volume.Value = 120;
            source.KeyOnDelay.Value = 0;
            source.EffectPath = EffectPath.Path1;
            source.BenderCutoff.Value = 12;
            source.BenderPitch.Value = 2;
            source.Pan = PanKind.Normal;
            source.PanValue.Value = 0;

            // DCO settings for additive source
            source.DCO.Wave =  new Wave(AdditiveKit.WaveNumber);
            source.DCO.Coarse.Value = 0;
            source.DCO.Fine.Value = 0;
            source.DCO.KSPitch = KeyScalingToPitch.ZeroCent;
            source.DCO.FixedKey = new FixedKey(); // OFF

            PitchEnvelope pitchEnv = new PitchEnvelope();
            pitchEnv.StartLevel.Value = 0;
            pitchEnv.AttackTime.Value = 4;
            pitchEnv.AttackLevel.Value = 0;
            pitchEnv.DecayTime.Value = 64;
            pitchEnv.LevelVelocitySensitivity.Value = 0;
            pitchEnv.TimeVelocitySensitivity.Value = 0;
            source.DCO.Envelope = pitchEnv;

            // DCF
            source.DCF.IsActive = true;
            source.DCF.Cutoff.Value = 55;
            source.DCF.Resonance.Value = 0;
            source.DCF.Level.Value = 7;
            source.DCF.Mode = FilterMode.LowPass;
            source.DCF.VelocityCurve = VelocityCurve.Curve5;

            // DCF Envelope
            FilterEnvelope filterEnv = new FilterEnvelope();
            source.DCF.EnvelopeDepth.Value = 25;
            filterEnv.AttackTime.Value = 0;
            filterEnv.Decay1Time.Value = 120;
            filterEnv.Decay1Level.Value = 63;
            filterEnv.Decay2Time.Value = 80;
            filterEnv.Decay2Level.Value = 63;
            filterEnv.ReleaseTime.Value = 20;
            source.DCF.Envelope = filterEnv;
            // DCF Modulation:
            source.DCF.KeyScalingToEnvelopeAttackTime.Value = 0;
            source.DCF.KeyScalingToEnvelopeDecay1Time.Value = 0;
            source.DCF.VelocityToEnvelopeDepth.Value = 30;
            source.DCF.VelocityToEnvelopeAttackTime.Value = 0;
            source.DCF.VelocityToEnvelopeDecay1Time.Value = 0;

            // DCA Envelope
            AmplifierEnvelope ampEnv = new AmplifierEnvelope();
            ampEnv.AttackTime.Value = 1;
            ampEnv.Decay1Time.Value = 94;
            ampEnv.Decay1Level.Value = 127;
            ampEnv.Decay2Time.Value = 80;
            ampEnv.Decay2Level.Value = 63;
            ampEnv.ReleaseTime.Value = 20;
            source.DCA.Envelope = ampEnv;

            // DCA Modulation
            source.DCA.KeyScaling.Level.Value = 0;
            source.DCA.KeyScaling.AttackTime.Value = 0;
            source.DCA.KeyScaling.Decay1Time.Value = 0;
            source.DCA.KeyScaling.ReleaseTime.Value = 0;

            source.DCA.VelocitySensitivity.Level.Value = 20;
            source.DCA.VelocitySensitivity.AttackTime.Value = 20;
            source.DCA.VelocitySensitivity.Decay1Time.Value = 20;
            source.DCA.VelocitySensitivity.ReleaseTime.Value = 20;

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
            vel.SwitchKind = VelocitySwitchKind.Off;
            vel.Threshold = 68;
            source.VelocitySwitch = vel;

            source.Volume.Value = 120;
            source.KeyOnDelay.Value = 0;
            source.EffectPath = EffectPath.Path1;
            source.BenderCutoff.Value = 12;
            source.BenderPitch.Value = 2;
            source.Pan = PanKind.Normal;
            source.PanValue.Value = 0;

            // DCO
            source.DCO.Wave = new Wave(waveNumber);
            source.DCO.Coarse.Value = 0;
            source.DCO.Fine.Value = 0;
            source.DCO.KSPitch = KeyScalingToPitch.ZeroCent;
            source.DCO.FixedKey = new FixedKey(); // OFF

            PitchEnvelope pitchEnv = new PitchEnvelope();
            pitchEnv.StartLevel.Value = 0;
            pitchEnv.AttackTime.Value = 4;
            pitchEnv.AttackLevel.Value = 0;
            pitchEnv.DecayTime.Value = 64;
            pitchEnv.LevelVelocitySensitivity.Value = 0;
            pitchEnv.TimeVelocitySensitivity.Value = 0;
            source.DCO.Envelope = pitchEnv;

            // DCF
            source.DCF.IsActive = true;
            source.DCF.Cutoff.Value = 55;
            source.DCF.Resonance.Value = 0;
            source.DCF.Level.Value = 7;
            source.DCF.Mode = FilterMode.LowPass;
            source.DCF.VelocityCurve = VelocityCurve.Curve5;

            // DCF Envelope
            FilterEnvelope filterEnv = new FilterEnvelope();
            source.DCF.EnvelopeDepth.Value = 25;
            filterEnv.AttackTime.Value = 0;
            filterEnv.Decay1Time.Value = 120;
            filterEnv.Decay1Level.Value = 63;
            filterEnv.Decay2Time.Value = 80;
            filterEnv.Decay2Level.Value = 63;
            filterEnv.ReleaseTime.Value = 20;
            source.DCF.Envelope = filterEnv;

            // DCF Modulation:
            source.DCF.KeyScalingToEnvelopeAttackTime.Value = 0;
            source.DCF.KeyScalingToEnvelopeDecay1Time.Value = 0;
            source.DCF.VelocityToEnvelopeDepth.Value = 30;
            source.DCF.VelocityToEnvelopeAttackTime.Value = 0;
            source.DCF.VelocityToEnvelopeDecay1Time.Value = 0;

            // DCA Envelope
            AmplifierEnvelope ampEnv = new AmplifierEnvelope();
            ampEnv.AttackTime.Value = 1;
            ampEnv.Decay1Time.Value = 94;
            ampEnv.Decay1Level.Value = 127;
            ampEnv.Decay2Time.Value = 80;
            ampEnv.Decay2Level.Value = 63;
            ampEnv.ReleaseTime.Value = 15;
            source.DCA.Envelope = ampEnv;

            // DCA Modulation
            source.DCA.KeyScaling.Level.Value = 0;
            source.DCA.KeyScaling.AttackTime.Value = 0;
            source.DCA.KeyScaling.Decay1Time.Value = 0;
            source.DCA.KeyScaling.ReleaseTime.Value = 0;

            source.DCA.VelocitySensitivity.Level.Value = 20;
            source.DCA.VelocitySensitivity.AttackTime.Value = 0;
            source.DCA.VelocitySensitivity.Decay1Time.Value = 0;
            source.DCA.VelocitySensitivity.ReleaseTime.Value = 0;

            return source;
        }
    }
}
