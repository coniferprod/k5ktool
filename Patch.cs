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
        int Size;
        int Padding;
        public string Name;
        string Code;
    }
}
