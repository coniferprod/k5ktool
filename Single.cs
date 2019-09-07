using System;
using System.Text;
using System.Collections.Generic;

namespace K5KTool
{
    public class Single
    {
        public byte CheckSum;

        public CommonSettings Common;

        public Source[] Sources;

        // Initialize a single patch with default settings
        public Single()
        {
            Common = new CommonSettings();
            Common.NumSources = 1;
            Sources = new Source[Common.NumSources];
            for (int i = 0; i < Common.NumSources; i++)
            {
                Sources[i] = new Source();
            }
        }

        public Single(byte[] data)
        {
            int offset = 0;
            byte b = 0;
            (b, offset) = Util.GetNextByte(data, offset);
            CheckSum = b;
            System.Console.WriteLine(String.Format("{0:X8} check sum = {1:X2}", offset, CheckSum));

            byte[] commonData = new byte[CommonSettings.DataSize];
            Buffer.BlockCopy(data, offset, commonData, 0, CommonSettings.DataSize);
            Common = new CommonSettings(commonData);
            offset += CommonSettings.DataSize;
            System.Console.WriteLine(String.Format("{0:X8} parsed {1} ({1:X4}h) bytes of common data", offset, CommonSettings.DataSize));

            Sources = new Source[Common.NumSources];
            for (int i = 0; i < Common.NumSources; i++)
            {
                byte[] sourceData = new byte[Source.DataSize];

                // BlockCopy argument list: Array src, int srcOffset, Array dst, int dstOffset, int count
                Buffer.BlockCopy(data, offset, sourceData, 0, Source.DataSize);
                System.Console.WriteLine(String.Format("Source {0} data:\n{1}", i + 1, Util.HexDump(sourceData)));
                Source source = new Source(sourceData);
                Sources[i] = source;
                offset += Source.DataSize;
                System.Console.WriteLine(String.Format("{0:X6} parsed {1} bytes of source data", offset, Source.DataSize));
            }

            for (int i = 0; i < Common.NumSources; i++)
            {
                Source source = Sources[i];
                if (source.DCO.WaveNumber == AdditiveKit.WaveNumber)  // ADD source, so include wave kit size in calculation
                {
                    byte[] additiveData = new byte[AdditiveKit.DataSize];
                    System.Console.WriteLine(String.Format("About to copy from data at offset {0:X4} to start of new buffer", offset));
                    Buffer.BlockCopy(data, offset, additiveData, 0, AdditiveKit.DataSize);
                    System.Console.WriteLine(String.Format("{0:X6} Source {1} ADD data:\n{2}", offset, i + 1, Util.HexDump(additiveData)));
                    source.ADD = new AdditiveKit(additiveData);
                    offset += AdditiveKit.DataSize;
                    System.Console.WriteLine(String.Format("{0:X6} parsed {1} bytes of ADD data", offset, AdditiveKit.DataSize));
                }
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Common.ToString());
            builder.Append("SOURCES:\n");
            for (int i = 0; i < Common.NumSources; i++)
            {
                builder.Append(String.Format("S{0}:\n{1}\n", i + 1, Sources[i].ToString()));
            }
            builder.Append("\n");
            return builder.ToString();
        }

        // Convert this single patch into SysEx data.
        // "BANK A, D, E, F: (check sum) + (COMMON) + (SOURCE)*(2~8)" (probably should be ~2*6?)
        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            byte[] commonData = Common.ToData();
            foreach (byte b in commonData)
            {
                data.Add(b);
            }

            for (int i = 0; i < Common.NumSources; i++)
            {
                byte[] sourceData = Sources[i].ToData();
                foreach (byte b in sourceData)
                {
                    data.Add(b);
                }
            }

            // Compute check sum and add it as the first byte
            byte checkSum = ComputeCheckSum(data.ToArray());
            data.Insert(0, checkSum);

            return data.ToArray();
        }

        private byte ComputeCheckSum(byte[] data)
        {
            // BANK A, D, E, F: check sum = [(common sum) + (source1 sum) [ + (source2~8 sum)] + 0xa5) & 0x7f
            byte total = 0;

            // First, compute the sum of common data and add it to the total:
            byte[] commonData = Common.ToData();
            byte commonSum = 0;
            foreach (byte b in commonData)
            {
                commonSum += b;
            }
            total += commonSum;

            // For each source, compute the sum of source data and add it to the total:
            for (int i = 0; i < Common.NumSources; i++)
            {
                byte[] sourceData = Sources[i].ToData();
                byte sourceSum = 0;
                foreach (byte b in sourceData)
                {
                    sourceSum += b;
                }
                total += sourceSum;
            }

            total += 0xA5;

            return (byte)(total & 0x7f);
        }
    }
}