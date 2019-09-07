using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

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
                    Single single = new Single(commonData);
                    System.Console.WriteLine(single.Common.Name);
                }
                else if (command.Equals("dump"))   // show all patch information
                {
                    System.Console.WriteLine(String.Format("Original data (length = {0} bytes):\n{1}", data.Length, Util.HexDump(data)));
                    Single single = new Single(data);
                    System.Console.WriteLine(single.ToString());
                }
                else if (command.Equals("create"))
                {
                    Single single = new Single();

                    // TODO: Duplicate the "WizooIni" patch
                    single.Common.NumSources = 2;

                    byte[] singleData = single.ToData();
                    System.Console.WriteLine(String.Format("Generated single data size: {0} bytes", singleData.Length));
                    System.Console.WriteLine(Util.HexDump(singleData));
                }
            }

            // For debugging: dump the wave list
            //for (int i = 0; i < Wave.NumWaves; i++)
            //{
            //    System.Console.WriteLine(String.Format("{0,3} {1}", i + 1, Wave.Instance[i]));
            //}
/*
            string waveformName = "saw";
            int numHarmonics = 64;
            byte[] levels = LeiterEngine.GetHarmonicLevels(waveformName, numHarmonics, 127);
            System.Console.WriteLine(String.Format("{0}, {1} harmonics:", waveformName, numHarmonics));
            for (int i = 0; i < levels.Length; i++)
            {
                System.Console.WriteLine(String.Format("{0} = {1}", i + 1, levels[i]));
            }

            waveformName = "square";
            levels = LeiterEngine.GetHarmonicLevels(waveformName, numHarmonics, 127);
            System.Console.WriteLine(String.Format("{0}, {1} harmonics:", waveformName, numHarmonics));
            for (int i = 0; i < levels.Length; i++)
            {
                System.Console.WriteLine(String.Format("{0} = {1}", i + 1, levels[i]));
            }
 */

            return 0;
        }
    }
}
