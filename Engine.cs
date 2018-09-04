using System;
using System.IO;
using System.Collections.Generic;

namespace k5ktool
{
    public class Engine
    {
        const int PATCHES = 128;  // the max number of patches in a bank

        const int SOURCES = 6;  // the max number of sources in a patch

        const int POOL_SIZE = 0x20000;

        public Bank ReadBank(string fileName)
        {
            Bank bank = new Bank();
            bank.Patches = new Patch[PATCHES + 1];
            bank.SortedTonePointer = new Patch[PATCHES + 1];
            bank.SortedIndex = new Patch[PATCHES];
            bank.SortedName = new Patch[PATCHES];
            bank.SortedSources = new Patch[PATCHES];
            bank.SortedSize = new Patch[PATCHES];
            bank.SortedCode = new Patch[PATCHES];

            using (FileStream fs = File.OpenRead(fileName))
            {
                Console.WriteLine($"Reading from '{fileName}'");
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    var count = 0;
                    for (int index = 0; index < PATCHES; index++)
                    {
                        Patch patch = new Patch();
                        patch.Index = index;
                        patch.TonePointer = ReadOffset(binaryReader);
                        patch.IsUsed = (patch.TonePointer != 0);
                        if (patch.IsUsed)
                        {
                            bank.SortedIndex[count] = patch;
                            bank.SortedTonePointer[count] = patch;
                            bank.SortedName[count] = patch;
                            bank.SortedSize[count] = patch;
                            bank.SortedSources[count] = patch;
                            bank.SortedCode[count] = patch;
                            count +=1;
                        }

                        patch.AdditiveKitCount = 0;
                        patch.Sources = new Source[SOURCES];

                        for (int sourceIndex = 0; sourceIndex < SOURCES; sourceIndex++)
                        {
                            var source = new Source();
                            source.AdditiveKitPointer = ReadOffset(binaryReader);
                            source.IsAdditive = (source.AdditiveKitPointer != 0);
                            if (source.IsAdditive)
                            {
                                patch.AdditiveKitCount += 1;
                            }
                            patch.Sources[sourceIndex] = source;
                        }

                        bank.Patches[count] = patch;
                    }

                    var lastPatch = new Patch();
                    lastPatch.Index = PATCHES;
                    lastPatch.TonePointer = ReadOffset(binaryReader);
                    bank.SortedTonePointer[count] = lastPatch;

                    bank.PatchCount = count;

                    bank.DataPool = ReadData(binaryReader);
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

        byte[] ReadData(BinaryReader br)
        {
            var data = br.ReadBytes(POOL_SIZE);
            return data;  // TODO: need to convert endiannness?
        }
    }
}