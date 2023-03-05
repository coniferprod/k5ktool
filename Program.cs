using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using CommandLine;
using Newtonsoft.Json;

using KSynthLib.Common;
using KSynthLib.K5000;

using SyxPack;


namespace K5KTool
{
    class Program
    {
        public const int SinglePatchCount = 128;  // banks A and D have 128 patches each
        public const int AdditiveWaveKitSize = 806;
        public const int SourceCountOffset = 51;

        static int Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<
                CreateOptions,
                ListOptions,
                DumpOptions,
                ReportOptions,
                InitOptions,
                EditOptions
            >(args);

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
            var header = new SystemExclusiveHeader();
            header.Function = SystemExclusiveFunction.OneBlockDump;
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

            var allData = new List<byte>();
            // SysEx initiator and basic header data
            allData.Add(SyxPack.Constants.Initiator);
            allData.AddRange(header.ToData());

            // Additional header data as required
            var patchNumber = opts.PatchNumber - 1;
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
                List<byte> singleData = single.GetSystemExclusiveData();
                Console.Error.WriteLine(string.Format("Generated single data size: {0} bytes", singleData.Count));
                Console.Error.WriteLine(single.ToString());
                allData.AddRange(singleData);

                allData.Add(SyxPack.Constants.Terminator);

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
            byte[] fileData = File.ReadAllBytes(fileName);
            string namePart = new DirectoryInfo(fileName).Name;
            DateTime timestamp = File.GetLastWriteTime(fileName);
            string timestampString = timestamp.ToString("yyyy-MM-dd hh:mm:ss");
            //Console.WriteLine($"System Exclusive file: '{namePart}' ({timestampString}, {fileData.Length} bytes)");

            ManufacturerSpecificMessage message = (ManufacturerSpecificMessage) Message.Create(fileData);

            var command = new ListCommand(message.Payload.ToArray());
            Console.Error.WriteLine($"Channel = {command.Header.Channel}, Cardinality = {command.Header.Cardinality}, Bank = {command.Header.Bank}, Kind = {command.Header.Kind}");

            if (opts.Type.Equals("sysex"))
            {
                /*
                if (command.Header.Cardinality != Cardinality.Block)
                {
                    Console.Error.WriteLine("Can only list blocks of patches");
                    return 0;
                }
                */

                if (command.Header.Kind != PatchKind.Single)
                {
                    Console.Error.WriteLine("Can only handle blocks of singles");
                    return 0;
                }

                command.ListPatches();

                return 0;
            }
            else if (opts.Type.Equals("bank"))
            {
                Console.WriteLine("Sorry, don't know how to handle native K5000 bank files yet!");
                return 0;
            }
            else
            {
                Console.WriteLine($"Unknown file type: {opts.Type}");
                return -1;
            }
        }

        private static string MakeTextList(byte[] data, string title)
        {
            var sb = new StringBuilder();

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
            var sb = new StringBuilder();

            return sb.ToString();
        }

        public static int RunDumpAndReturnExitCode(DumpOptions opts)
        {
            //Console.Error.WriteLine("Dump not implemented yet");

            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);

            var command = new DumpCommand(fileData);

            if (command.Header.Cardinality != Cardinality.Block)
            {
                Console.Error.WriteLine("Can only list blocks of patches");
                return 0;
            }

            if (command.Header.Kind != PatchKind.Single)
            {
                Console.Error.WriteLine("Can only handle blocks of singles");
                return 0;
            }

            command.DumpPatches(opts.Output);

            return 0;
        }

        public static int RunReportAndReturnExitCode(ReportOptions opts)
        {
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.WriteLine($"Report on System Exclusive file '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            Console.WriteLine($"Contains {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                ProcessMessage(message);
            }

            return 0;
        }

        private static DumpHeader Identify(byte[] fileData)
        {
            Console.WriteLine($"File data length = {fileData.Length} bytes");

            // Assume that we have at least one header's worth of data
            var header = new SystemExclusiveHeader(fileData);

            return new DumpHeader(1, Cardinality.Block, BankIdentifier.A, PatchKind.Single);
        }

        private static void ProcessMessage(byte[] message)
        {
            Console.WriteLine($"Message length = {message.Length} bytes");

            var header = new SystemExclusiveHeader(message);
            //Console.WriteLine("{0}", header);

            var function = (SystemExclusiveFunction)header.Function;
            string functionName = SystemExclusiveFunctionExtensions.Name(function);

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

            Console.WriteLine(functionName);

            if (function == SystemExclusiveFunction.OneBlockDump)
            {
                int toneNumber = message[8] + 1;
                Console.WriteLine($"Tone No = {toneNumber} ({message[8]})");
            }

            if (function == SystemExclusiveFunction.AllBlockDump)
            {
                var ToneMapData = new byte[ToneMap.Size];
                Array.Copy(message, SystemExclusiveHeader.DataSize, ToneMapData, 0, ToneMap.Size);

                var toneMap = new ToneMap(ToneMapData);

                Console.WriteLine("Patches included:");
                for (var i = 0; i < ToneMap.ToneCount; i++)
                {
                    if (toneMap[i])
                    {
                        Console.Write(i + 1);
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();

                var dataLength = message.Length - SystemExclusiveHeader.DataSize - ToneMap.Size;
                var data = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeader.DataSize + ToneMap.Size, data, 0, dataLength);
                //Console.WriteLine(Util.HexDump(data));

                var offset = 0;
                byte checksum = data[offset];
                Console.WriteLine($"checksum = {checksum:X2}");
                offset += 1;
            }

            // Single additive patch for bank A or D:
            if (header.Substatus1 == 0x00 && (header.Substatus2 == 0x00 || header.Substatus2 == 0x02))
            {
                var dataLength = message.Length - SystemExclusiveHeader.DataSize - ToneMap.Size;
                var data = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeader.DataSize + ToneMap.Size, data, 0, dataLength);
                //Console.WriteLine(Util.HexDump(data));

                // Chop the data into individual buffers based on the declared sizes
                var offset = 0;
                byte checksum = data[offset];
                Console.WriteLine($"checksum = {checksum:X2}");
                offset += 1;
                var commonData = new byte[SingleCommonSettings.DataSize];
                Array.Copy(data, offset, commonData, 0, SingleCommonSettings.DataSize);
                //Console.WriteLine(Util.HexDump(commonData));
                offset += SingleCommonSettings.DataSize;

                var patch = new SinglePatch(data);
                Console.WriteLine($"Name = {patch.SingleCommon.Name}");
            }
        }

        // Create an init patch. This is a single patch with basic settings.
        public static int RunInitAndReturnExitCode(InitOptions opts)
        {
            Console.WriteLine("Init");

            var patch = new SinglePatch();
            patch.SingleCommon.Name = new PatchName("DS Init");
            patch.SingleCommon.Volume.Value = 115;
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
            var lines = new List<string>();
            for (var i = 0; i < levels.Length; i++)
            {
                var cmd = new StringBuilder();
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

                var paramStrings = new List<string>(options.Params.Split(','));
                /*
                foreach (string s in paramStrings)
                {
                    Console.WriteLine(s);
                }
                */

                var paramValues = new List<double>();
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

                var parameters = new WaveformParameters
                {
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
            foreach (var line in group1Lines)
            {
                Console.WriteLine(line);
            }
            List<string> group2Lines = SendHarmonics(options.Device, 0, 1, levels, 2);
            foreach (var line in group2Lines)
            {
                Console.WriteLine(line);
            }

            return 0;
        }
    }
}
