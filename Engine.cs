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
        const int TONE_COMMON_DATA_SIZE = 82;
        const int NAME_OFFSET = 40;
        const int NAME_SIZE = 8;
        const int SRC_NMBR_OFFSET = 51;

        const int SOURCE_DATA_SIZE = 86;
        const int WAVE_KIT_MSB_OFFSET = 28;
        const int WAVE_KIT_ADD_MASK = 0x04;
        const int ADD_WAVE_KIT_SIZE = 806;

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

                    Array.Sort(bank.SortedTonePointer, delegate(Patch p1, Patch p2) {
                        return (int)p1.TonePointer - (int)p2.TonePointer;
                    });
                    bank.Base = bank.SortedTonePointer[0].TonePointer;
                    for (int p = 0; p < bank.PatchCount; p++)
                    {
                        var patch = bank.SortedTonePointer[p];
                        patch.TonePointer -= bank.Base;
                        for (int s = 0; s < SOURCES; s++)
                        {
                            var source = patch.Sources[s];
                            if (source.IsAdditive)
                            {
                                source.AdditiveKitPointer -= bank.Base;
                            }
                        }
                    }

                    bank.Patches[PATCHES].TonePointer -= bank.Base;

                    /* analyze number of sources, size and name: */
                    for (int p = 0; p < bank.PatchCount; p++)
                    {
                        var patch = bank.SortedTonePointer[p];
                        patch.SourceCount = bank.DataPool[patch.TonePointer + SRC_NMBR_OFFSET];
                        patch.Size = TONE_COMMON_DATA_SIZE + SOURCE_DATA_SIZE * patch.SourceCount + ADD_WAVE_KIT_SIZE * patch.AdditiveKitCount;
                        patch.Padding = bank.SortedTonePointer[p + 1].TonePointer - patch.TonePointer - patch.Size;

                        // Next up: patch name
                    }
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