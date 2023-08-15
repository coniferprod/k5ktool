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

            var payload = new List<byte>();
            payload.AddRange(header.ToData());

            // Additional header data as required
            var patchNumber = opts.PatchNumber - 1;
            if (patchNumber < 0 || patchNumber > 127)
            {
                Console.WriteLine("Patch number must be 1...128");
                return -1;
            }
            payload.Add((byte)patchNumber);

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
                List<byte> singleData = single.Data;
                Console.Error.WriteLine(string.Format("Generated single data size: {0} bytes", singleData.Count));
                Console.Error.WriteLine(single.ToString());
                payload.AddRange(singleData);

                var message = new ManufacturerSpecificMessage(
                    new ManufacturerDefinition(new byte[] { 0x40 }),
                    payload.ToArray()
                );

                File.WriteAllBytes(opts.OutputFileName, message.Data.ToArray());
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

            var message = Message.Create(fileData) as ManufacturerSpecificMessage;

            var command = new ListCommand(message.Payload);
            Console.Error.WriteLine($"Channel = {command.Header.Channel}, Cardinality = {command.Header.Cardinality}, Bank = {command.Header.Bank}, Kind = {command.Header.Kind}");

            if (command.Header.Kind != PatchKind.Single)
            {
                Console.Error.WriteLine("Can only handle blocks of singles");
                return 0;
            }

            command.ListPatches();

            return 0;
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
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            var message = Message.Create(fileData) as ManufacturerSpecificMessage;

            // Construct the command from the payload (no delimiter or manufacturer)
            var command = new DumpCommand(message.Payload);
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

            Console.WriteLine("Dump header:");
            Console.WriteLine($"Channel = {command.Header.Channel}");
            Console.WriteLine($"Cardinality = {command.Header.Cardinality}");
            Console.WriteLine($"Bank = {command.Header.Bank}");
            Console.WriteLine($"Kind = {command.Header.Kind}");

            Console.WriteLine($"Payload = {message.Payload.Count} bytes");

            command.DumpPatches(opts.Output);

            return 0;
        }

        public static int RunReportAndReturnExitCode(ReportOptions opts)
        {
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.WriteLine($"Report on System Exclusive file '{fileName}' ({fileData.Length} bytes)");

            var message = Message.Create(fileData) as ManufacturerSpecificMessage;

            // Construct the command from the payload (no delimiter or manufacturer)
            var header = new DumpHeader(message.Payload.ToArray());
            Console.WriteLine(header);

            if (header.Cardinality == Cardinality.One)
            {
                int toneNumber = header.SubBytes[0] + 1;  // TODO: is this right?
                Console.WriteLine($"Tone No = {toneNumber}");
            }

            if (header.Cardinality == Cardinality.Block)
            {
                if (header.Kind == PatchKind.Single)
                {
                    var toneMap = new ToneMap(header.SubBytes.ToArray());

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
                }
            }

            return 0;
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

                List<byte> payload = new List<byte>()
                {
                    0x40, // Kawai manufacturer ID
                    channel,
                    (byte)SystemExclusiveFunction.ParameterSend,
                    0x00,  // group number
                    0x0a,  // machine number for K5000
                    0x02,
                    (byte)(0x40 + groupNumber),
                    (byte)sourceNumber,
                    (byte)i,
                    0,
                    0,
                    levels[i]
                };

                foreach (byte b in payload)
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
