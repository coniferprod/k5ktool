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

        public uint[] ReadPointers(string fileName)
        {
            uint[] pointers = new uint[PATCHES * 7 + 1];
            int index = 0;
            using (FileStream fs = File.OpenRead(fileName))
            {
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    for (int patchIndex = 0; patchIndex < PATCHES; patchIndex++)
                    {
                        var tonePointer = ReadOffset(binaryReader);
                        pointers[index] = tonePointer;
                        index++;

                        for (int sourceIndex = 0; sourceIndex < SOURCES; sourceIndex++)
                        {
                            var sourcePointer = ReadOffset(binaryReader);
                            pointers[index] = sourcePointer;
                            index++;
                        }
                    }
                    var highPointer = ReadOffset(binaryReader);
                    pointers[index] = highPointer;
                }
            }
            return pointers;
        }

        public Bank ReadBank(string fileName)
        {
            Bank bank = new Bank();
            bank.Patches = new Patch[PATCHES + 1];
            bank.SortedTonePointer = new Patch[PATCHES + 1];
            bank.SortedIndex = new Patch[PATCHES];
            bank.SortedName = new Patch[PATCHES];
            bank.SortedSourceCount = new Patch[PATCHES];
            bank.SortedSize = new Patch[PATCHES];
            bank.SortedCode = new Patch[PATCHES];

            using (FileStream fs = File.OpenRead(fileName))
            {
                Console.WriteLine($"Reading from '{fileName}'");
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    /* read the pointer table (128 * 7 pointers) and build list of used patches: */
                    var patchCount = 0;
                    for (int patchIndex = 0; patchIndex < PATCHES; patchIndex++)
                    {
                        Patch patch = new Patch();
                        patch.Index = patchIndex;

                        /* 1 pointer to tone data */
                        patch.TonePointer = ReadOffset(binaryReader);
                        patch.IsUsed = (patch.TonePointer != 0);

                        patch.AdditiveKitCount = 0;
                        patch.Sources = new Source[SOURCES];

                        Console.WriteLine(patch);

                        /* 6 pointers to ADD wave kits */
                        for (int sourceIndex = 0; sourceIndex < SOURCES; sourceIndex++)
                        {
                            var source = new Source();
                            source.AdditiveKitPointer = ReadOffset(binaryReader);
                            source.IsAdditive = (source.AdditiveKitPointer != 0);
                            if (source.IsAdditive)
                            {
                                patch.AdditiveKitCount += 1;
                            }

                            Console.WriteLine($"Source {sourceIndex}: {source}");
                            patch.Sources[sourceIndex] = source;
                        }

                        // The source processing is done earlier so that we get the 
                        // patch with its sources to all the various "Sorted..." arrays
                        // (but those will have to go).
                        if (patch.IsUsed)
                        {
                            bank.SortedIndex[patchCount] = patch;
                            bank.SortedTonePointer[patchCount] = patch;
                            bank.SortedName[patchCount] = patch;
                            bank.SortedSize[patchCount] = patch;
                            bank.SortedSourceCount[patchCount] = patch;
                            bank.SortedCode[patchCount] = patch;
                            patchCount +=1;
                        }

                        bank.Patches[patchCount] = patch;
                    }

                    var lastPatch = new Patch();
                    lastPatch.Index = PATCHES;
                    /* read the 'memory high water mark' pointer: */
                    lastPatch.TonePointer = ReadOffset(binaryReader);
                    bank.SortedTonePointer[patchCount] = lastPatch;

                    bank.PatchCount = patchCount;

                    bank.DataPool = ReadData(binaryReader);

                    Array.Sort(bank.SortedTonePointer, delegate(Patch p1, Patch p2) {
                        return (int)p1.TonePointer - (int)p2.TonePointer;
                    });
                    bank.Base = bank.SortedTonePointer[0].TonePointer;

                    Console.WriteLine($"Bank has {bank.PatchCount} patches");

                    for (int p = 0; p < bank.PatchCount; p++)
                    {
                        var patch = bank.SortedTonePointer[p];
                        patch.TonePointer -= bank.Base;
                        Console.WriteLine($"Patch {patch.Index}: TonePointer = {patch.TonePointer}");

                        for (int s = 0; s < SOURCES; s++)
                        {
                            var source = patch.Sources[s];
                            Console.WriteLine($"Source {s}");
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
                        //patch.Padding = bank.SortedTonePointer[p + 1].TonePointer - patch.TonePointer - patch.Size;

                        // Next up: patch name
                        var patchNameChars = new char[NAME_SIZE];
                        var namePointer = patch.TonePointer + NAME_OFFSET;
                        for (int i = 0; i < NAME_SIZE; i++)
                        {
                            patchNameChars[i] = (char) bank.DataPool[namePointer];
                            namePointer++;
                        }
                        // no need for trailing null char
                        patch.Name = new string(patchNameChars);
                    }

                    Array.Sort(bank.SortedSourceCount, delegate(Patch p1, Patch p2) {
                        return (int)p1.SourceCount - (int)p2.SourceCount;
                    });

                    Array.Sort(bank.SortedSize, delegate(Patch p1, Patch p2) {
                        return (int)p1.Size - (int)p2.Size;
                    });

                    Array.Sort(bank.SortedName, delegate(Patch p1, Patch p2) {
                        return string.Compare(p1.Name, p2.Name, StringComparison.OrdinalIgnoreCase);
                    });

                    /* analyze tone structure: */
                    for (int p = 0; p < bank.PatchCount; p++)
                    {
                        var patch = bank.SortedIndex[p];
                        var patchCodeChars = new char[SOURCES];
                        int s = 0;
                        for (s = 0; s < patch.SourceCount; s++)
                        {
                            var source = patch.Sources[s];
                            patchCodeChars[s] = source.IsAdditive ? 'A' : 'P';
                        }
                        while (s < SOURCES)
                        {
                            patchCodeChars[s] = '-';
                            s++;
                        }
                        patch.Code = new string(patchCodeChars);
                    }

                    Array.Sort(bank.SortedCode, delegate(Patch p1, Patch p2) {
                        return string.Compare(p1.Name, p2.Name, StringComparison.OrdinalIgnoreCase);
                    });
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