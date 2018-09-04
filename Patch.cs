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
            return $"Index={Index} IsUsed={IsUsed} TonePointer={TonePointer} SourceCount={SourceCount} AdditiveKitCount={AdditiveKitCount}";
        }
    }
}
