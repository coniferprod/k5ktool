namespace k5ktool
{
    public struct Patch
    {
        public int Index;
        public bool IsUsed;
        public uint TonePointer;
        public int SourceCount;
        public int AdditiveKitCount;
        Source[] Sources;
        int Size;
        public string Name;
        string Code;
    }
}
