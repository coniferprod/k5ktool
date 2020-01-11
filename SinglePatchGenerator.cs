using System;
using System.Collections.Generic;

using KSynthLib.K5000;

namespace K5KTool
{
    public class SinglePatchGenerator
    {
        static Dictionary<string, HarmonicEnvelope> HarmEnv = new Dictionary<string, HarmonicEnvelope>()
        {
            {
                "piano",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 125,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 92,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 49,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 39,
                        Level = 49
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "epiano",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 81,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 15,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "pluck",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 118,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 79,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "padFast",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 83,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 63,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 64,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 52,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "padSlow",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 67,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 63,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 64,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            }
        };

        public SinglePatchGenerator()
        {

        }

        public SinglePatch Generate(string patchName)
        {
            SinglePatch single = new SinglePatch();

            single.Common.Name = patchName;
            single.Common.Volume = 115;
            single.SingleCommon.NumSources = 2;
            single.SingleCommon.IsPortamentoEnabled = false;
            single.SingleCommon.PortamentoSpeed = 0;

            single.Sources = new Source[single.SingleCommon.NumSources];

            single.Sources[0] = GenerateAdditiveSource();
            single.Sources[1] = GeneratePCMSource();

            return single;
        }

        private Source GenerateAdditiveSource()
        {
            Source source = new Source();
            source.ZoneLow = 0;
            source.ZoneHigh = 127;
            VelocitySwitchSettings vel = new VelocitySwitchSettings();
            vel.Type = VelocitySwitchType.Off;
            vel.Velocity = 68;
            source.VelocitySwitch = vel;

            source.Volume = 120;
            source.KeyOnDelay = 0;
            source.EffectPath = 1;
            source.BenderCutoff = 12;
            source.BenderPitch = 2;
            source.Pan = PanType.Normal;
            source.NormalPanValue = 0;

            // DCO settings for additive source
            source.DCO.WaveNumber = AdditiveKit.WaveNumber;
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
            string waveformName = "saw";
            int numHarmonics = 64;
            byte[] levels = LeiterEngine.GetHarmonicLevels(waveformName, numHarmonics, 127);  // levels are 0...127
            Console.WriteLine(String.Format("{0}, {1} harmonics:", waveformName, numHarmonics));
            source.ADD.SoftHarmonics = levels;
            for (int i = 0; i < levels.Length; i++)
            {
                Console.WriteLine(String.Format("{0} = {1}", i + 1, levels[i]));
            }

            // Harmonic envelopes
            HarmonicEnvelope harmEnv = HarmEnv["pluck"];
            for (int i = 0; i < AdditiveKit.NumHarmonics; i++)
            {
                source.ADD.HarmonicEnvelopes[i] = harmEnv;
            }
            
            return source;
        }

        private Source GeneratePCMSource()
        {
            Source source = new Source();

            VelocitySwitchSettings vel = new VelocitySwitchSettings();
            vel.Type = VelocitySwitchType.Off;
            vel.Velocity = 68;
            source.VelocitySwitch = vel;

            source.Volume = 120;
            source.KeyOnDelay = 0;
            source.EffectPath = 1;
            source.BenderCutoff = 12;
            source.BenderPitch = 2;
            source.Pan = PanType.Normal;
            source.NormalPanValue = 0;

            // DCO
            source.DCO.WaveNumber = 412;
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