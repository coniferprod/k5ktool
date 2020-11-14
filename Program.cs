﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

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
                    Console.WriteLine(String.Format("Unknown bank: '{0}'", opts.BankName));
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
                Console.WriteLine(String.Format("Generated single data size: {0} bytes", singleData.Length));
                Console.WriteLine(single.ToString());
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
                SystemExclusiveHeader header = new SystemExclusiveHeader(allData);
                // TODO: Check the SysEx file header for validity

                // Extract the patch bytes (discarding the SysEx header and terminator)
                dataLength = allData.Length - SystemExclusiveHeader.DataSize - 1;

                data = new byte[dataLength];
                Array.Copy(allData, SystemExclusiveHeader.DataSize, data, 0, dataLength);
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
                Console.WriteLine(String.Format($"Unknown output format: '{outputFormat}'"));
                return -1;
            }
        }

        private static string MakeTextList(byte[] data, string title)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SINGLE patches:\n");

/*
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
            Console.WriteLine("Dump not implemented yet");

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
            Console.WriteLine($"message length = {message.Length} bytes");

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
                    Console.WriteLine(String.Format("Unknown substatus1: {0:X2}", header.Substatus1));
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

            // Single additive patch for bank A or D:
            if (header.Substatus1 == 0x00 && (header.Substatus2 == 0x00 || header.Substatus2 == 0x02))
            {
                int dataLength = message.Length - SystemExclusiveHeader.DataSize;
                byte[] data = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeader.DataSize, data, 0, dataLength);
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
    }
}
