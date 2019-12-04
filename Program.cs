using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using CommandLine;

using KSynthLib.Common;
using KSynthLib.K5000;

namespace K5KTool
{

    class Program
    {
        public const int SinglePatchCount = 128;  // banks A and D have 128 patches each
        public const int AdditiveWaveKitSize = 806;
        public const int SourceCountOffset = 51;

        static int Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<CreateOptions, ListOptions, DumpOptions, ReportOptions, InitOptions>(args);
            parserResult.MapResult(
                (CreateOptions opts) => RunCreateAndReturnExitCode(opts),
                (ListOptions opts) => RunListAndReturnExitCode(opts),
                (DumpOptions opts) => RunDumpAndReturnExitCode(opts),
                (ReportOptions opts) => RunReportAndReturnExitCode(opts),
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                errs => 1
            );

            return 0;
        }

        public static int RunCreateAndReturnExitCode(CreateOptions opts)
        {
            SinglePatch single = NewSinglePatch("NewSound");
            byte[] singleData = single.ToData();
            Console.WriteLine(String.Format("Generated single data size: {0} bytes", singleData.Length));
            Console.WriteLine(Util.HexDump(singleData));
            return 0;
        }

        static SinglePatch NewSinglePatch(string patchName)
        {
            SinglePatch single = new SinglePatch();

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

        public static int RunListAndReturnExitCode(ListOptions opts)
        {
            Console.WriteLine("List");

            string fileName = opts.FileName;
            byte[] message = File.ReadAllBytes(fileName);
            string namePart = new DirectoryInfo(fileName).Name;
            DateTime timestamp = File.GetLastWriteTime(fileName);
            string timestampString = timestamp.ToString("yyyy-MM-dd hh:mm:ss");
            Console.WriteLine($"System Exclusive file: '{namePart}' ({timestampString}, {message.Length} bytes)");

            SystemExclusiveHeader header = new SystemExclusiveHeader(message);
            // TODO: Check the SysEx file header for validity

            // Extract the patch bytes (discarding the SysEx header and terminator)
            int dataLength = message.Length - SystemExclusiveHeader.DataSize - 1;
            byte[] data = new byte[dataLength];
            Array.Copy(message, SystemExclusiveHeader.DataSize, data, 0, dataLength);

            string outputFormat = opts.Output;
            if (outputFormat.Equals("text"))
            {
                Console.WriteLine(MakeTextList(data, namePart));
                return 0;
            }
            else if (outputFormat.Equals("html"))
            {
                Console.WriteLine(MakeHtmlList(data, namePart));
                return 0;
            }
            else 
            {
                Console.WriteLine(String.Format($"Unknown output format: '{outputFormat}'"));
                return -1;
            }
        }

        private static string MakeTextList(byte[] data, string title)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SINGLE patches:\n");

            int offset = 0;
            for (int i = 0; i < SinglePatchCount; i++)
            {
                byte[] singleData = new byte[SinglePatch.DataSize];
                Buffer.BlockCopy(data, offset, singleData, 0, SinglePatch.DataSize);
                SinglePatch single = new SinglePatch(singleData);
                string name = PatchUtil.GetPatchName(i);
                sb.Append($"S{name}  {single.Name}\n");
                if ((i + 1) % 16 == 0) {
                    sb.Append("\n");
                }
                offset += SinglePatch.DataSize;
            }
            sb.Append("\n");

            return sb.ToString();
        }

        private static string MakeHtmlList(byte[] data, string title)
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }

        public static int RunDumpAndReturnExitCode(DumpOptions opts)
        {
            Console.WriteLine("Dump");

            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);

            return 0;
        }

        public static int RunReportAndReturnExitCode(ReportOptions opts)
        {
            Console.WriteLine("Report");

            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            Console.WriteLine($"Got {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                ProcessMessage(message);
            }

            return 0;
        }

        private static void ProcessMessage(byte[] message)
        {
            SystemExclusiveHeader header = new SystemExclusiveHeader(message);

            Console.WriteLine("{0}", header);
            Dictionary<SystemExclusiveFunction, string> functionNames = new Dictionary<SystemExclusiveFunction, string>()
            {
                { SystemExclusiveFunction.OneBlockDumpRequest, "One Block Dump Request" },
                { SystemExclusiveFunction.AllBlockDumpRequest, "All Block Dump Request" },
                { SystemExclusiveFunction.ParameterSend, "Parameter Send" },
                { SystemExclusiveFunction.TrackControl, "Track Control" },
                { SystemExclusiveFunction.OneBlockDump, "One Block Dump" },
                { SystemExclusiveFunction.AllBlockDump, "All Block Dump" },
                { SystemExclusiveFunction.ModeChange, "Mode Change" },
                { SystemExclusiveFunction.Remote, "Remote" },
                { SystemExclusiveFunction.WriteComplete, "Write Complete" },
                { SystemExclusiveFunction.WriteError, "Write Error" },
                { SystemExclusiveFunction.WriteErrorByProtect, "Write Error (Protect)" },
                { SystemExclusiveFunction.WriteErrorByMemoryFull, "Write Error (Memory Full)" },
                { SystemExclusiveFunction.WriteErrorByNoExpandMemory, "Write Error (No Expansion Memory)" }
            };

            SystemExclusiveFunction function = (SystemExclusiveFunction)header.Function;
            string functionName = "";
            if (functionNames.TryGetValue(function, out functionName))
            {
                Console.WriteLine("Function = {0}", functionName);
            }
            else
            {
                Console.WriteLine("Unknown function: {0}", function);
            }
        }

        public static int RunInitAndReturnExitCode(InitOptions opts)
        {
            Console.WriteLine("Init");
            return 0;
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
