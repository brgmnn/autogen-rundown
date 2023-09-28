namespace MyFirstPlugin.DataBlocks
{
    internal class ZonePlacementWeights
    {
        public double Start { get; set; } = 0.0;

        public double Middle { get; set; } = 0.0;

        public double End { get; set; } = 0.0;

        public static ZonePlacementWeights EvenlyDistributed
            = new ZonePlacementWeights { Start = 1.0, Middle = 1.0, End = 1.0 };
    }
}
