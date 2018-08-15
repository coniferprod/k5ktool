using System;
using System.IO;
using System.Collections.Generic;

namespace k5ktool
{
    public class Engine
    {
        const uint MaxPatchCount = 128;  // the max number of patches in a bank

        const uint MaxSourceCount = 6;  // the max number of sources in a patch

        const int DataSize = 0x20000;

        struct PatchOffsets
        {
            public uint index;
            public uint tone;
            public uint[] sources;
        }

        public Bank ReadBank(string fileName)
        {
            Bank bank = new Bank();

            using (FileStream fs = File.OpenRead(fileName))
            {
                Console.WriteLine($"Reading from '{fileName}'");
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    List<PatchOffsets> allOffsets = new List<PatchOffsets>();

                    var patchCount = 0;
                    for (uint patchIndex = 0; patchIndex < MaxPatchCount; patchIndex++)
                    {
                        PatchOffsets offsets = new PatchOffsets();
                        offsets.index = patchIndex;
                        offsets.tone = ReadOffset(binaryReader);

                        offsets.sources = new uint[MaxSourceCount];
                        for (int sourceIndex = 0; sourceIndex < MaxSourceCount; sourceIndex++)
                        {
                            offsets.sources[sourceIndex] = ReadOffset(binaryReader);
                        }

                        if (offsets.tone != 0)
                        {
                            allOffsets.Add(offsets);
                        }

                        var sourceOffsets = string.Join(", ", offsets.sources);
                        Console.WriteLine($"{patchIndex.ToString("000")}: tone = {offsets.tone}, sources = {sourceOffsets}");
                    }

                    var lastOffset = ReadOffset(binaryReader);
                    Console.WriteLine($"lastOffset = {lastOffset}");

                    var displacement = allOffsets[0].tone;
                    Console.WriteLine($"displacement = {displacement}");
                    
                    for (var i = 0; i < allOffsets.Count; i++)
                    {
                        PatchOffsets offsets = allOffsets[i];
                        offsets.tone -= displacement;
                        for (int sourceIndex = 0; sourceIndex < MaxSourceCount; sourceIndex++)
                        {
                            offsets.sources[sourceIndex] -= displacement;
                        }
                    }

                    Console.WriteLine("After applying displacement:");
                    foreach (var offsets in allOffsets)
                    {
                        var sourceOffsets = string.Join(", ", offsets.sources);
                        Console.WriteLine($"{offsets.index.ToString("000")}: tone = {offsets.tone}, sources = {sourceOffsets}");
                    }

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