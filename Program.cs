using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using K5KLib;

namespace K5KTool
{
    public struct SystemExclusiveHeader
    {
        public byte ManufacturerID;
	    public byte Channel;
	    public byte Function;
	    public byte Group;
	    public byte MachineID;
	    public byte Substatus1;
	    public byte Substatus2;

        public override string ToString()
        {
            return String.Format("ManufacturerID = {0,2:X2}H, Channel = {1}, Function = {2,2:X2}H, Group = {3,2:X2}H, MachineID = {4,2:X2}H, Substatus1 = {5,2:X2}H, Substatus2 = {6,2:X2}H", ManufacturerID, Channel, Function, Group, MachineID, Substatus1, Substatus2);
        }
    }

    public enum SystemExclusiveFunction
    {
        OneBlockDumpRequest = 0x00,
        AllBlockDumpRequest = 0x01,
        ParameterSend = 0x10,
        TrackControl = 0x11,
        OneBlockDump = 0x20,
        AllBlockDump = 0x21,
        ModeChange = 0x31,
        Remote = 0x32,
        WriteComplete = 0x40,
        WriteError = 0x41,
        WriteErrorByProtect = 0x42,
        WriteErrorByMemoryFull = 0x44,
        WriteErrorByNoExpandMemory = 0x45
    }

    public class PatchMap
    {
        const int Size = 19;  // bytes

        public const int NumPatches = 128;

        private bool[] include;

        public PatchMap()
        {
            include = new bool[NumPatches];  // initialized to false by C#, right?
        }

        public PatchMap(byte[] data)
        {
            // TODO: Check that the data length matches

            include = new bool[NumPatches];

            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    buf.Append(data[i].IsBitSet(j) ? "1" : "0");
                }
            }

            string bitString = buf.ToString().Reversed();
            for (int i = 0; i < bitString.Length; i++)
            {
                include[i] = bitString[i] == '1' ? true : false;
            }
        }

        public PatchMap(bool[] incl)
        {
            include = new bool[NumPatches];
            // TODO: Check that lengths match
            for (int i = 0; i < incl.Length; i++)
            {
                include[i] = incl[i];
            }
        }

        public bool this[int i]
        {
            get { return include[i]; }
        }

        public byte[] ToData()
        {
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < include.Length; i++)
            {
                buf.Append(include[i] ? "1" : "0");
                // each byte maps seven patches, and every 8th bit must be a zero
                if (i % 8 == 0)
                {
                    buf.Append("0");
                }
            }
            // The patches are enumerated starting from the low bits, so reverse the string.
            string bitString = buf.ToString().Reversed();
            // Now we have a long bit string. Slice it into chunks of eight bits to convert to bytes.
            string[] parts = bitString.Split(8);
            List<byte> data = new List<byte>();
            foreach (string s in parts)
            {
                data.Add(Convert.ToByte(s, 2));
            }
            return data.ToArray();
        }
    }

    class Program
    {
        // N.B. The length of the SysEx header varies by command!
        private static int GetSystemExclusiveHeaderLength(SystemExclusiveFunction func)
        {
            if (func == SystemExclusiveFunction.OneBlockDumpRequest)
            {
                return 9;
            }

            // TODO: Add lengths for other commands

            return 8;
        }

        private const int SystemExclusiveHeaderLength = 8;
        private const byte SystemExclusiveTerminator = 0xF7;

        static SystemExclusiveHeader GetSystemExclusiveHeader(byte[] data)
        {
            SystemExclusiveHeader header;
            // data[0] is the SysEx identifier F0H
            header.ManufacturerID = data[1];
            header.Channel = data[2];
		    header.Function = data[3];
		    header.Group = data[4];
		    header.MachineID = data[5];
		    header.Substatus1 = data[6];
		    header.Substatus2 = data[7];
            return header;
        }

        public const int SinglePatchCount = 128;  // banks A and D have 128 patches each
        public const int AdditiveWaveKitSize = 806;
        public const int SourceCountOffset = 51;

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("Usage: K5KTool cmd filename.syx");
                return 1;
            }

            string command = args[0];
            string fileName = args[1];
            string patchName = "";
            if (args.Length > 2)
            {
                patchName = args[2];
            }

            byte[] fileData = File.ReadAllBytes(fileName);
            System.Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, SystemExclusiveTerminator);
            System.Console.WriteLine($"Got {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                SystemExclusiveHeader header = GetSystemExclusiveHeader(message);
                // TODO: Check the SysEx file header for validity

                int headerLength = GetSystemExclusiveHeaderLength(SystemExclusiveFunction.OneBlockDumpRequest);

                // Extract the patch bytes (discarding the SysEx header)
                int dataLength = message.Length - headerLength;
                //System.Console.WriteLine($"data length = {dataLength}");
                byte[] data = new byte[dataLength];
                Array.Copy(message, headerLength, data, 0, dataLength);

                SystemExclusiveFunction function = (SystemExclusiveFunction) header.Function;
                if (function != SystemExclusiveFunction.OneBlockDump)
                {
                    System.Console.WriteLine($"This is not a block single dump: {header.ToString()}");
                    break;
                }

                if (command.Equals("list"))  // just list patch name(s)
                {
                    // At first use only common data so that we can take a look without parsing everything:
                    byte[] commonData = new byte[CommonSettings.DataSize + Source.DataSize];
                    Buffer.BlockCopy(data, 0, commonData, 0, CommonSettings.DataSize + Source.DataSize);
                    //System.Console.WriteLine(Util.HexDump(commonData));
                    K5KLib.Single single = new K5KLib.Single(commonData);
                    System.Console.WriteLine(single.Common.Name);
                }
                else if (command.Equals("dump"))   // show all patch information
                {
                    System.Console.WriteLine(String.Format("Original data (length = {0} bytes):\n{1}", data.Length, Util.HexDump(data)));
                    K5KLib.Single single = new K5KLib.Single(data);
                    System.Console.WriteLine(single.ToString());
                }
                else if (command.Equals("create"))
                {
                    K5KLib.Single single = NewSinglePatch("NewSound");
                    byte[] singleData = single.ToData();
                    System.Console.WriteLine(String.Format("Generated single data size: {0} bytes", singleData.Length));
                    System.Console.WriteLine(Util.HexDump(singleData));
                }
            }

            return 0;
        }

        static K5KLib.Single NewSinglePatch(string patchName)
        {
            K5KLib.Single single = new K5KLib.Single();

            single.Common.Name = patchName;
            single.Common.Volume = 115;
            single.Common.NumSources = 2;
            single.Common.IsPortamentoEnabled = false;
            single.Common.PortamentoSpeed = 0;

            single.Sources = new Source[single.Common.NumSources];

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
            System.Console.WriteLine(String.Format("{0}, {1} harmonics:", waveformName, numHarmonics));
            single.Sources[0].ADD.SoftHarmonics = levels;
            for (int i = 0; i < levels.Length; i++)
            {
                System.Console.WriteLine(String.Format("{0} = {1}", i + 1, levels[i]));
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
    }
}
