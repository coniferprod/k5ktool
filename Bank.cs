using System.Collections.Generic;

namespace k5ktool
{
    public struct Bank
    {
        public byte[] DataPool;
        public Patch[] Patches;
        public int PatchCount;
        uint Base;

        public Patch[] SortedTonePointer;  // remember +1
        public Patch[] SortedIndex;
        public Patch[] SortedName;
        public Patch[] SortedSources;
        public Patch[] SortedSize;
        public Patch[] SortedCode;
    }
}
