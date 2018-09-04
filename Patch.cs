namespace k5ktool
{
    public struct Patch
    {
        public int Index;
        public bool IsUsed;
        public uint TonePointer;
        public int SourceCount;
        public int AdditiveKitCount;
        public Source[] Sources;
        public int Size;
        public uint Padding;
        public string Name;
        public string Code;

        public override string ToString()
        {
            return string.Format(
                "Index={0} IsUsed={1} TonePointer={2:X6} SourceCount={3} AdditiveKitCount={4}",
                Index, IsUsed, TonePointer, SourceCount, AdditiveKitCount
            );
        }
    }
}
