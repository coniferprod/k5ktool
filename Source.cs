namespace k5ktool
{
    public struct Source
    {
        public bool IsAdditive;
        public uint AdditiveKitPointer;

        public override string ToString()
        {
            return $"IsAdditive={IsAdditive} AdditiveKitPointer={AdditiveKitPointer}";
        }
    }
}
