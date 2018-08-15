using System;
using System.IO;
using System.Collections.Generic;

namespace k5ktool
{
    public class Engine
    {
        const int MaxPatchCount = 128;  // the max number of patches in a bank

        const int MaxSourceCount = 6;  // the max number of sources in a patch

        const int DataSize = 0x20000;

        struct PatchOffsets
        {
            public uint tone;
            public uint[] sources;
        }

        public Bank GetBank(string fileName)
        {
            Bank bank = new Bank();

            using (FileStream fs = File.OpenRead(fileName))
            {
                Console.WriteLine($"Reading from '{fileName}'");
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    List<PatchOffsets> allOffsets = new List<PatchOffsets>();

                    var patchCount = 0;
                    for (int patchIndex = 0; patchIndex < MaxPatchCount; patchIndex++)
                    {
                        PatchOffsets offsets = new PatchOffsets();
                        offsets.tone = ReadOffset(binaryReader);

                        offsets.sources = new uint[MaxSourceCount];
                        for (int sourceIndex = 0; sourceIndex < MaxSourceCount; sourceIndex++)
                        {
                            offsets.sources[sourceIndex] = ReadOffset(binaryReader);
                        }

                        allOffsets.Add(offsets);

                        var sourceOffsets = string.Join(", ", offsets.sources);
                        Console.WriteLine($"{patchIndex.ToString("000")}: tone = {offsets.tone}, sources = {sourceOffsets}");

                    }

                    var tonePointer = ReadOffset(binaryReader);

                    var data = binaryReader.ReadBytes(DataSize);
                }

            }

            return bank;
        }

        uint ReadOffset(BinaryReader br)
        {
            var data = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return BitConverter.ToUInt32(data, 0);
        }
    }
}