using System;

using KSynthLib.Common;
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

            single.Sources[0] = new Source();
            single.Sources[0].ZoneLow = 0;
            single.Sources[0].ZoneHigh = 127;
            VelocitySwitchSettings vel1 = new VelocitySwitchSettings();
            vel1.Type = VelocitySwitchType.Off;
            vel1.Velocity = 68;
            single.Sources[0].VelocitySwitch = vel1;

            single.Sources[0].Volume = 120;
            single.Sources[0].KeyOnDelay = 0;
            single.Sources[0].EffectPath = 1;
            single.Sources[0].BenderCutoff = 12;
            single.Sources[0].BenderPitch = 2;
            single.Sources[0].Pan = PanType.Normal;
            single.Sources[0].NormalPanValue = 0;

            // DCO settings for additive source
            single.Sources[0].DCO.WaveNumber = AdditiveKit.WaveNumber;
            single.Sources[0].DCO.Coarse = 0;
            single.Sources[0].DCO.Fine = 0;
            single.Sources[0].DCO.KSPitch = KeyScalingToPitch.ZeroCent;
            single.Sources[0].DCO.FixedKey = 0; // OFF

            PitchEnvelope pitchEnv = new PitchEnvelope();
            pitchEnv.StartLevel = 0;
            pitchEnv.AttackTime = 4;
            pitchEnv.AttackLevel = 0;
            pitchEnv.DecayTime = 64;
            pitchEnv.LevelVelocitySensitivity = 0;
            pitchEnv.TimeVelocitySensitivity = 0;
            single.Sources[0].DCO.Envelope = pitchEnv;

            // DCF
            single.Sources[0].DCF.IsActive = true;
            single.Sources[0].DCF.Cutoff = 55;
            single.Sources[0].DCF.Resonance = 0;
            single.Sources[0].DCF.Level = 7;
            single.Sources[0].DCF.Mode = FilterMode.LowPass;
            single.Sources[0].DCF.VelocityCurve = 5;

            // DCF Envelope
            FilterEnvelope filterEnv = new FilterEnvelope();
            single.Sources[0].DCF.EnvelopeDepth = 25;
            filterEnv.AttackTime = 0;
            filterEnv.Decay1Time = 120;
            filterEnv.Decay1Level = 63;
            filterEnv.Decay2Time = 80;
            filterEnv.Decay2Level = 63;
            filterEnv.ReleaseTime = 20;
            single.Sources[0].DCF.Envelope = filterEnv;
            // DCF Modulation:
            single.Sources[0].DCF.KSToEnvAttackTime = 0;
            single.Sources[0].DCF.KSToEnvDecay1Time = 0;
            single.Sources[0].DCF.VelocityToEnvDepth = 30;
            single.Sources[0].DCF.VelocityToEnvAttackTime = 0;
            single.Sources[0].DCF.VelocityToEnvDecay1Time = 0;

            // DCA Envelope
            AmplifierEnvelope ampEnv = new AmplifierEnvelope();
            ampEnv.AttackTime = 1;
            ampEnv.Decay1Time = 94;
            ampEnv.Decay1Level = 127;
            ampEnv.Decay2Time = 80;
            ampEnv.Decay2Level = 63;
            ampEnv.ReleaseTime = 20;
            single.Sources[0].DCA.Envelope = ampEnv;

            // DCA Modulation
            single.Sources[0].DCA.KeyScaling.Level = 0;
            single.Sources[0].DCA.KeyScaling.AttackTime = 0;
            single.Sources[0].DCA.KeyScaling.Decay1Time = 0;
            single.Sources[0].DCA.KeyScaling.ReleaseTime = 0;

            single.Sources[0].DCA.VelocitySensitivity.Level = 20;
            single.Sources[0].DCA.VelocitySensitivity.AttackTime = 20;
            single.Sources[0].DCA.VelocitySensitivity.Decay1Time = 20;
            single.Sources[0].DCA.VelocitySensitivity.ReleaseTime = 20;

            // Harmonic levels
            string waveformName = "pluckedString";
            int numHarmonics = 64;
            byte[] levels = LeiterEngine.GetHarmonicLevels(waveformName, numHarmonics, 127);  // levels are 0...127
            Console.WriteLine(String.Format("{0}, {1} harmonics:", waveformName, numHarmonics));
            single.Sources[0].ADD.SoftHarmonics = levels;
            for (int i = 0; i < levels.Length; i++)
            {
                Console.WriteLine(String.Format("{0} = {1}", i + 1, levels[i]));
            }

            // Harmonic envelopes
            HarmonicEnvelope harmEnv = HarmEnv["pluck"];
            for (int i = 0; i < AdditiveKit.NumHarmonics; i++)
            {
                single.Sources[0].ADD.HarmonicEnvelopes[i] = harmEnv;
            }
            
            single.Sources[1] = new Source();
            VelocitySwitchSettings vel2 = new VelocitySwitchSettings();
            vel2.Type = VelocitySwitchType.Off;
            vel2.Velocity = 68;
            single.Sources[1].VelocitySwitch = vel2;

            single.Sources[1].Volume = 120;
            single.Sources[1].KeyOnDelay = 0;
            single.Sources[1].EffectPath = 1;
            single.Sources[1].BenderCutoff = 12;
            single.Sources[1].BenderPitch = 2;
            single.Sources[1].Pan = PanType.Normal;
            single.Sources[1].NormalPanValue = 0;

            // DCO
            single.Sources[1].DCO.WaveNumber = 412;
            single.Sources[1].DCO.Coarse = 0;
            single.Sources[1].DCO.Fine = 0;
            single.Sources[1].DCO.KSPitch = KeyScalingToPitch.ZeroCent;
            single.Sources[1].DCO.FixedKey = 0; // OFF

            PitchEnvelope pitchEnv2 = new PitchEnvelope();
            pitchEnv2.StartLevel = 0;
            pitchEnv2.AttackTime = 4;
            pitchEnv2.AttackLevel = 0;
            pitchEnv2.DecayTime = 64;
            pitchEnv2.LevelVelocitySensitivity = 0;
            pitchEnv2.TimeVelocitySensitivity = 0;
            single.Sources[1].DCO.Envelope = pitchEnv2;

            // DCF
            single.Sources[1].DCF.IsActive = true;
            single.Sources[1].DCF.Cutoff = 55;
            single.Sources[1].DCF.Resonance = 0;
            single.Sources[1].DCF.Level = 7;
            single.Sources[1].DCF.Mode = FilterMode.LowPass;
            single.Sources[1].DCF.VelocityCurve = 5;

            // DCF Envelope
            FilterEnvelope filterEnv2 = new FilterEnvelope();
            single.Sources[1].DCF.EnvelopeDepth = 25;
            filterEnv2.AttackTime = 0;
            filterEnv2.Decay1Time = 120;
            filterEnv2.Decay1Level = 63;
            filterEnv2.Decay2Time = 80;
            filterEnv2.Decay2Level = 63;
            filterEnv2.ReleaseTime = 20;
            single.Sources[1].DCF.Envelope = filterEnv2;
            // DCF Modulation:
            single.Sources[1].DCF.KSToEnvAttackTime = 0;
            single.Sources[1].DCF.KSToEnvDecay1Time = 0;
            single.Sources[1].DCF.VelocityToEnvDepth = 30;
            single.Sources[1].DCF.VelocityToEnvAttackTime = 0;
            single.Sources[1].DCF.VelocityToEnvDecay1Time = 0;

            // DCA Envelope
            AmplifierEnvelope ampEnv2 = new AmplifierEnvelope();
            ampEnv2.AttackTime = 1;
            ampEnv2.Decay1Time = 94;
            ampEnv2.Decay1Level = 127;
            ampEnv2.Decay2Time = 80;
            ampEnv2.Decay2Level = 63;
            ampEnv2.ReleaseTime = 15;
            single.Sources[1].DCA.Envelope = ampEnv2;

            // DCA Modulation
            single.Sources[1].DCA.KeyScaling.Level = 0;
            single.Sources[1].DCA.KeyScaling.AttackTime = 0;
            single.Sources[1].DCA.KeyScaling.Decay1Time = 0;
            single.Sources[1].DCA.KeyScaling.ReleaseTime = 0;

            single.Sources[1].DCA.VelocitySensitivity.Level = 20;
            single.Sources[1].DCA.VelocitySensitivity.AttackTime = 0;
            single.Sources[1].DCA.VelocitySensitivity.Decay1Time = 0;
            single.Sources[1].DCA.VelocitySensitivity.ReleaseTime = 0;

            return single;
        }
    }
}