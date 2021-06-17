using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using CommandLine;
using Newtonsoft.Json;

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
            var parserResult = Parser.Default.ParseArguments<CreateOptions, ListOptions, DumpOptions, ReportOptions, InitOptions, EditOptions>(args);
            parserResult.MapResult(
                (CreateOptions opts) => RunCreateAndReturnExitCode(opts),
                (ListOptions opts) => RunListAndReturnExitCode(opts),
                (DumpOptions opts) => RunDumpAndReturnExitCode(opts),
                (ReportOptions opts) => RunReportAndReturnExitCode(opts),
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (EditOptions opts) => RunEditAndReturnExitCode(opts),
                errs => 1
            );

            return 0;
        }

        public static int RunCreateAndReturnExitCode(CreateOptions opts)
        {
            SystemExclusiveHeader header = new SystemExclusiveHeader();
            header.ManufacturerID = 0x40;  // Kawai
            header.Channel = 0x00;
            header.Function = (byte)SystemExclusiveFunction.OneBlockDump;
            header.Group = 0x00;
            header.MachineID = 0x0A;
            header.Substatus1 = 0x00;  // single

            // Get the right bank. Since it is a required parameter, I suppose we can trust that it exists.
            string bankName = opts.BankName.ToLower();
            char ch = bankName[0];
            switch (ch)
            {
                case 'a':
                    header.Substatus2 = 0x00;
                    break;

                case 'd':
                    header.Substatus2 = 0x02;
                    break;

                case 'e':
                    header.Substatus2 = 0x03;
                    break;

                case 'f':
                    header.Substatus2 = 0x04;
                    break;

                default:
                    Console.WriteLine(string.Format("Unknown bank: '{0}'", opts.BankName));
                    return -1;
            }

            List<byte> allData = new List<byte>();
            // SysEx initiator and basic header data
            allData.Add(SystemExclusiveHeader.Initiator);
            allData.AddRange(header.ToData());

            // Additional header data as required
            int patchNumber = opts.PatchNumber - 1;
            if (patchNumber < 0 || patchNumber > 127)
            {
                Console.WriteLine("Patch number must be 1...128");
                return -1;
            }
            allData.Add((byte)patchNumber);

            SinglePatchGenerator generator;
            SinglePatchDescriptor descriptor;

            if (opts.PatchType.Equals("single"))
            {
                if (!string.IsNullOrEmpty(opts.Descriptor))  // we have a JSON descriptor file, parse it
                {
                    var jsonText = File.ReadAllText(opts.Descriptor);
                    descriptor = JsonConvert.DeserializeObject<SinglePatchDescriptor>(jsonText);
                    generator = new SinglePatchGenerator(descriptor);
                }
                else
                {
                    descriptor = new SinglePatchDescriptor();
                    generator = new SinglePatchGenerator(descriptor);
                }

                SinglePatch single = generator.Generate();
                byte[] singleData = single.ToData();
                Console.Error.WriteLine(string.Format("Generated single data size: {0} bytes", singleData.Length));
                Console.Error.WriteLine(single.ToString());
                allData.AddRange(singleData);

                allData.Add(SystemExclusiveHeader.Terminator);

                File.WriteAllBytes(opts.OutputFileName, allData.ToArray());
            }
            else if (opts.PatchType.Equals("multi"))
            {
                Console.WriteLine("Don't know how to make a multi patch yet");
                return 1;
            }

            return 0;
        }

        public static int RunListAndReturnExitCode(ListOptions opts)
        {
            string fileName = opts.FileName;
            byte[] allData = File.ReadAllBytes(fileName);
            string namePart = new DirectoryInfo(fileName).Name;
            DateTime timestamp = File.GetLastWriteTime(fileName);
            string timestampString = timestamp.ToString("yyyy-MM-dd hh:mm:ss");
            Console.WriteLine($"System Exclusive file: '{namePart}' ({timestampString}, {allData.Length} bytes)");

            byte[] data;
            int dataLength;

            if (opts.Type.Equals("sysex"))
            {
                int offset = 0;
                byte[] headerData = new byte[27];  // max header length?
                Array.Copy(allData, offset, headerData, 0, 27);
                SystemExclusiveHeader header = new SystemExclusiveHeader(headerData);
                Console.Error.WriteLine($"SysEx: manufacturer = {header.ManufacturerID:X2}h, channel = {header.Channel + 1}");
                // TODO: Check the SysEx file header for validity

                DumpCommand command = new DumpCommand(headerData);
                Console.Error.WriteLine($"cardinality = {command.Card}, bank = {command.Bank}");

                offset += 8;

                PatchMap patchMap = new PatchMap();
                if (command.Card == DumpCommand.Cardinality.Block)
                {
                    // For a block data dump, need to parse the tone map
                    byte[] patchMapData = new byte[PatchMap.Size];
                    Array.Copy(allData, offset, patchMapData, 0, PatchMap.Size);
                    Console.Error.WriteLine($"Copied {PatchMap.Size} bytes from offset {offset} to patchMapData");

                    patchMap = new PatchMap(patchMapData);
                    Console.WriteLine("Patches included:");
                    int patchCount = 0;
                    for (int i = 0; i < PatchMap.PatchCount; i++)
                    {
                        if (patchMap[i])
                        {
                            patchCount += 1;
                            Console.Write(i + 1);
                            Console.Write(" ");
                        }
                    }
                    Console.WriteLine($"\nTotal = {patchCount} patches");

                    offset += PatchMap.Size;

                    Console.Error.WriteLine($"offset = {offset}");
                    data = new byte[allData.Length];
                    Array.Copy(allData, offset, data, 0, allData.Length - offset);

                    int totalPatchSize = 0;
                    int minimumPatchSize = SingleCommonSettings.DataSize + 2 * Source.DataSize;
                    Console.Error.WriteLine($"minimum patch size = {minimumPatchSize}");
                    for (int i = 0; i < patchCount; i++)
                    {
                        byte[] singleData = new byte[minimumPatchSize];
                        // first just use all of the patch data
                        Array.Copy(allData, offset, singleData, 0, minimumPatchSize);
                        Console.Error.WriteLine($"Copied {minimumPatchSize} bytes from offset {offset} to singleData");
                        Console.Error.Write(Util.HexDump(singleData));

                        SinglePatch patch = new SinglePatch(singleData);

                        // Find out how many PCM and ADD sources
                        int pcmCount = 0;
                        int addCount = 0;
                        foreach (Source source in patch.Sources)
                        {
                            if (source.IsAdditive)
                            {
                                addCount += 1;
                            }
                            else
                            {
                                pcmCount += 1;
                            }
                        }

                        // Figure out the total size of the single patch based on the counts
                        int patchSize = SingleCommonSettings.DataSize + pcmCount * Source.DataSize + addCount * Source.DataSize + addCount * AdditiveKit.DataSize;
                        Console.WriteLine($"{pcmCount}PCM {addCount}ADD size={patchSize} bytes");
                        Array.Copy(data, offset, singleData, 0, patchSize);
                        //Console.Error.Write(Util.HexDump(singleData));

                        offset += patchSize;
                        totalPatchSize = patchSize;
                    }

                    Console.WriteLine($"Total patch size = {totalPatchSize} bytes");
                }
                else
                {
                    offset += 3 + 5 + 1;

                    Console.WriteLine("Can't handle single patch files yet, sorry!");
                    return -1;
                }

            }
            else if (opts.Type.Equals("bank"))
            {
                dataLength = allData.Length;
                data = allData;
                Console.WriteLine("Sorry, don't know how to handle bank files yet!");
                return 1;
            }
            else
            {
                Console.WriteLine($"Unknown file type: {opts.Type}");
                return -1;
            }

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
                Console.WriteLine($"Unknown output format: '{outputFormat}'");
                return -1;
            }
        }

        private static string MakeTextList(byte[] data, string title)
        {
            StringBuilder sb = new StringBuilder();

            /*
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
            */

            return sb.ToString();
        }

        private static string MakeHtmlList(byte[] data, string title)
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }

        public static int RunDumpAndReturnExitCode(DumpOptions opts)
        {
            Console.Error.WriteLine("Dump not implemented yet");

            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);

            return 0;
        }

        public static int RunReportAndReturnExitCode(ReportOptions opts)
        {
            Console.WriteLine("Report");

            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.Error.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            Console.Error.WriteLine($"Got {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                ProcessMessage(message);
            }

            return 0;
        }

        private static void ProcessMessage(byte[] message)
        {
            Console.Error.WriteLine($"message length = {message.Length} bytes");

            SystemExclusiveHeader header = new SystemExclusiveHeader(message);

            Console.Error.WriteLine("{0}", header);
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
            string functionName;
            if (functionNames.TryGetValue(function, out functionName))
            {
                Console.Error.WriteLine("Function = {0}", functionName);
            }
            else
            {
                Console.Error.WriteLine("Unknown function: {0}", function);
            }

            switch (header.Substatus1)
            {
                case 0x00:
                    Console.WriteLine("Single");
                    break;

                case 0x20:
                    Console.WriteLine("Multi");
                    break;

                case 0x10:
                    Console.WriteLine("Drum Kit");  // K5000W only
                    break;

                case 0x11:
                    Console.WriteLine("Drum Inst");  // K5000W only
                    break;

                default:
                    Console.Error.WriteLine(string.Format("Unknown substatus1: {0:X2}", header.Substatus1));
                    break;
            }

            switch (header.Substatus2)
            {
                case 0x00:
                    Console.WriteLine("Add Bank A");
                    break;

                case 0x01:
                    Console.WriteLine("PCM Bank B");  // K5000W
                    break;

                case 0x02:
                    Console.WriteLine("Add Bank D");
                    break;

                case 0x03:
                    Console.WriteLine("Exp Bank E");
                    break;

                case 0x04:
                    Console.WriteLine("Exp Bank F");
                    break;

                default:
                    Console.WriteLine("Substatus2 is first data byte");
                    break;
            }

            switch (function)
            {
                case SystemExclusiveFunction.OneBlockDump:
                    Console.WriteLine("One Block Dump");

                    int toneNumber = message[8] + 1;
                    Console.WriteLine($"Tone No = {toneNumber} ({message[8]})");
                    break;

                case SystemExclusiveFunction.AllBlockDump:
                    Console.WriteLine("All Block Dump");
                    break;

                default:
                    Console.WriteLine($"Unknown function: {function}");
                    break;
            }

            if (function == SystemExclusiveFunction.AllBlockDump)
            {
                byte[] patchMapData = new byte[PatchMap.Size];
                Array.Copy(message, SystemExclusiveHeader.DataSize, patchMapData, 0, PatchMap.Size);

                PatchMap patchMap = new PatchMap(patchMapData);
                
                Console.WriteLine("Patches included:");
                for (int i = 0; i < PatchMap.PatchCount; i++)
                {
                    if (patchMap[i])
                    {
                        Console.Write(i + 1);
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();

                int dataLength = message.Length - SystemExclusiveHeader.DataSize - PatchMap.Size;
                byte[] data = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeader.DataSize + PatchMap.Size, data, 0, dataLength);
                Console.WriteLine(Util.HexDump(data));

                int offset = 0;
                byte checksum = data[offset];
                Console.WriteLine($"checksum = {checksum:X2}");
                offset += 1;
            }

            // Single additive patch for bank A or D:
            if (header.Substatus1 == 0x00 && (header.Substatus2 == 0x00 || header.Substatus2 == 0x02))
            {
                int dataLength = message.Length - SystemExclusiveHeader.DataSize - PatchMap.Size;
                byte[] data = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeader.DataSize + PatchMap.Size, data, 0, dataLength);
                //Console.WriteLine(Util.HexDump(data));

                // Chop the data into individual buffers based on the declared sizes
                int offset = 0;
                byte checksum = data[offset];
                Console.WriteLine($"checksum = {checksum:X2}");
                offset += 1;
                byte[] commonData = new byte[SingleCommonSettings.DataSize];
                Array.Copy(data, offset, commonData, 0, SingleCommonSettings.DataSize);
                Console.WriteLine(Util.HexDump(commonData));
                offset += SingleCommonSettings.DataSize;

                SinglePatch patch = new SinglePatch(data);
                Console.WriteLine($"Name = {patch.SingleCommon.Name}");
            }
        }

        // Create an init patch. This is a single patch with basic settings.
        public static int RunInitAndReturnExitCode(InitOptions opts)
        {
            Console.WriteLine("Init");

            SinglePatch patch = new SinglePatch();
            patch.SingleCommon.Name = "DS Init";
            patch.SingleCommon.Volume = 115;
            patch.SingleCommon.SourceCount = 2;

            patch.Sources = new Source[patch.SingleCommon.SourceCount];
            for (int i = 0; i < patch.SingleCommon.SourceCount; i++)
            {
                patch.Sources[i] = new Source();
            }


            Console.WriteLine(patch.ToString());

            return 0;
        }

        private static List<string> SendHarmonics(string deviceName, byte channel, int sourceNumber, byte[] levels, int groupNumber)
        {
            string commandName = "sendmidi";
            List<string> lines = new List<string>();
            for (int i = 0; i < levels.Length; i++)
            {
                StringBuilder cmd = new StringBuilder();
                cmd.Append($"{commandName} dev \"{deviceName}\" hex syx");

                List<byte> data = new List<byte>();
                data.Add(0x40); // Kawai ID
                data.Add(channel);
                data.Add((byte)SystemExclusiveFunction.ParameterSend);
                data.Add(0x00); // group number
                data.Add(0x0a); // machine number for K5000
                data.Add(0x02);
                data.Add((byte)(0x40 + groupNumber));
                data.Add((byte)sourceNumber);
                data.Add((byte)i);
                data.Add(0);
                data.Add(0);
                data.Add(levels[i]);

                foreach (byte b in data)
                {
                    cmd.Append(string.Format(" {0:X2}", b));
                }
                lines.Add(cmd.ToString());
            }

            return lines;
        }

        public static int RunEditAndReturnExitCode(EditOptions options)
        {
            byte[] levels;

            if (options.Waveform.Equals("custom"))
            {
                if (string.IsNullOrEmpty(options.Params))
                {
                    Console.WriteLine("Parameters required for Custom waveform");
                    return 1;
                }

                List<string> paramStrings = new List<string>(options.Params.Split(','));
                /*
                foreach (string s in paramStrings)
                {
                    Console.WriteLine(s);
                }
                */

                List<double> paramValues = new List<double>();
                foreach (string s in paramStrings)
                {
                    double value = double.Parse(s, CultureInfo.InvariantCulture);
                    paramValues.Add(value);
                }
                /*
                foreach (double v in paramValues)
                {
                    Console.WriteLine(v);
                }
                */

                WaveformParameters parameters = new WaveformParameters {
                    A = paramValues[0],
                    B = paramValues[1],
                    C = paramValues[2],
                    XP = paramValues[3],
                    D = paramValues[4],
                    E = paramValues[5],
                    YP = paramValues[6]
                };
                levels = WaveformEngine.GetCustomHarmonicLevels(parameters, 64, 127);
            }
            else
            {
                levels = WaveformEngine.GetHarmonicLevels(options.Waveform, 64, 127);
            }

            List<string> group1Lines = SendHarmonics(options.Device, 0, 1, levels, 1);
            foreach (string line in group1Lines)
            {
                Console.WriteLine(line);
            }
            List<string> group2Lines = SendHarmonics(options.Device, 0, 1, levels, 2);
            foreach (string line in group2Lines)
            {
                Console.WriteLine(line);
            }

            return 0;
        }
    }
}
