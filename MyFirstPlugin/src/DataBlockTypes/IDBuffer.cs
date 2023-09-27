namespace MyFirstPlugin.DataBlockTypes
{
    public class IDBuffer
    {
        public uint CurrentID { get; set; } = ushort.MaxValue;
        public IncrementMode IncrementMode { get; set; } = IncrementMode.Decrement;

        public uint GetNext()
        {
            if (IncrementMode == IncrementMode.Increment)
            {
                return CurrentID++;
            }
            else if (IncrementMode == IncrementMode.Decrement)
            {
                return CurrentID--;
            }
            else
            {
                return 0;
            }
        }
    }

    public enum IncrementMode
    {
        Decrement, Increment
    }
}