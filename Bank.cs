using System;
using System.Collections.Generic;
using System.Linq;
using System.Buffers.Binary;

namespace K5KTool
{
    public class Patch
    {
        public string Name;
        public int Index;
        public int SourceCount;
        public int Size;
        public int Padding;
        public int ToneOffset;
        public List<int> SourceOffsets;
        public List<byte> Data;
    }

    public class OffsetTableEntry
    {
        public int Index;
        public int ToneOffset;
        public List<int> SourceOffsets;

        public OffsetTableEntry(int index, int toneOffset, List<int> sourceOffsets)
        {
            this.Index = index;
            this.ToneOffset = toneOffset;
            this.SourceOffsets = sourceOffsets;
        }
    }

    public class Bank
    {
        public static int MAX_PATCH_COUNT = 128;
        public static int POOL_SIZE = 0x20000;
        public static int SOURCE_COUNT_OFFSET = 51;
        public static int SOURCE_DATA_SIZE = 86;
        public static int TONE_COMMON_DATA_SIZE = 82;
        public static int ADD_KIT_SIZE = 806;
        public static int NAME_OFFSET = 40;
        public static int NAME_LENGTH = 8;

        public List<byte> DataPool;
        public List<Patch> Patches;
        public int BaseOffset;

        public Bank(List<byte> data)
        {
            var offsetTable = GetOffsetTable(data);
            var sortedOffsetTable = offsetTable.OrderBy(e => e.ToneOffset).ToList();
            PrintOffsetTable(sortedOffsetTable);

            int highOffset = GetHighOffset(data);

            int baseOffset = sortedOffsetTable[0].ToneOffset;

            foreach (var entry in sortedOffsetTable)
            {
                entry.ToneOffset -= baseOffset;
                for (var sourceIndex = 0; sourceIndex < 6; sourceIndex++)
                {
                    if (entry.SourceOffsets[sourceIndex] != 0)
                    {
                        entry.SourceOffsets[sourceIndex] -= baseOffset;
                    }
                }
            }
            highOffset -= baseOffset;

            var offset = MAX_PATCH_COUNT * 7 * 4 + 4;
            var patchData = data.GetRange(offset, POOL_SIZE);
            this.DataPool = patchData;

            var patches = new List<Patch>();
            var index = 0;
            foreach (var p in sortedOffsetTable)
            {
                var toneOffset = p.ToneOffset;
                var sourceCount = patchData[toneOffset + SOURCE_COUNT_OFFSET];

                var addCount = 0;
                foreach (var src in p.SourceOffsets)
                {
                    if (src != 0)
                    {
                        addCount++;
                    }
                }

                var size = TONE_COMMON_DATA_SIZE
                    + SOURCE_DATA_SIZE * sourceCount
                    + ADD_KIT_SIZE * addCount;

                var nextOffset = highOffset;
                if (index < sortedOffsetTable.Count - 1)
                {
                    nextOffset = sortedOffsetTable[index + 1].ToneOffset;
                }
                var padding = nextOffset - toneOffset - size;

                var nameOffset = toneOffset + NAME_OFFSET;
                var name = System.Text.Encoding.UTF8.GetString(
                    patchData.GetRange(nameOffset, NAME_LENGTH).ToArray()
                ).TrimEnd(new char[] { ' ', '\x7f' });

                patches.Add(new Patch
                {
                    Name = name, Index = p.Index, SourceCount = sourceCount,
                    Size = size, Padding = padding, ToneOffset = toneOffset,
                    SourceOffsets = p.SourceOffsets
                });

                index++;
            }

            this.Patches = patches;
            this.BaseOffset = baseOffset;
        }

        private List<OffsetTableEntry> GetOffsetTable(List<byte> data)
        {
            var offset = 0;
            var offsetTable = new List<OffsetTableEntry>();

            for (var i = 0; i < MAX_PATCH_COUNT; i++)
            {
                List<int> entries = new List<int>();
                for (var e = 0; e < 7; e++)
                {
                    var entry = BinaryPrimitives.ReadInt32BigEndian(new ReadOnlySpan<byte>(data.ToArray(), offset, 4));
                    entries.Add(entry);
                    offset += 4;
                }

                var toneOffset = entries[0];
                if (toneOffset != 0)
                {
                    var sources = entries.GetRange(1, 6);
                    offsetTable.Add(new OffsetTableEntry(i, toneOffset, sources));
                }
            }

            return offsetTable;
        }

        private void PrintOffsetTable(List<OffsetTableEntry> pt)
        {
            foreach (var entry in pt)
            {
                Console.WriteLine($"index: {entry.Index}");
                Console.WriteLine($"tone: {entry.ToneOffset}");
                Console.WriteLine("sources:");
                foreach (var src in entry.SourceOffsets)
                {
                    Console.WriteLine($"{src:X8}");
                }
                Console.WriteLine();
            }
        }

        private int GetHighOffset(List<byte> data)
        {
            var offset = MAX_PATCH_COUNT * 7 * 4;  // 128 patch locations with seven pointers of four bytes each
            var entry =  BinaryPrimitives.ReadInt32BigEndian(new ReadOnlySpan<byte>(data.ToArray(), offset, 4));
            return entry;
        }
    }
}
