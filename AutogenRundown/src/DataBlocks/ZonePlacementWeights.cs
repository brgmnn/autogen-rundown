using System.Diagnostics.CodeAnalysis;

namespace AutogenRundown.DataBlocks
{
    internal class ZonePlacementWeights
    {
        public double Start { get; set; } = 0.0;

        public double Middle { get; set; } = 0.0;

        public double End { get; set; } = 0.0;

        public static ZonePlacementWeights GenRandom()
            => Generator.Pick(new List<ZonePlacementWeights>
            {
                EvenlyDistributed,
                AtStart,
                AtMiddle,
                AtEnd,
                NotAtStart,
                NotAtMiddle,
                NotAtEnd
            })!;

        public static ZonePlacementWeights None
            = new ZonePlacementWeights { Start = 0.0, Middle = 0.0, End = 0.0 };

        public static ZonePlacementWeights EvenlyDistributed
            = new ZonePlacementWeights { Start = 1.0, Middle = 1.0, End = 1.0 };

        public static ZonePlacementWeights AtStart
            = new ZonePlacementWeights { Start = 1.0, Middle = 0.0, End = 0.0 };

        public static ZonePlacementWeights AtMiddle
            = new ZonePlacementWeights { Start = 0.0, Middle = 1.0, End = 0.0 };

        public static ZonePlacementWeights AtEnd
            = new ZonePlacementWeights { Start = 0.0, Middle = 0.0, End = 1.0 };

        public static ZonePlacementWeights NotAtStart
            = new ZonePlacementWeights { Start = 0.0, Middle = 1.0, End = 1.0 };

        public static ZonePlacementWeights NotAtMiddle
            = new ZonePlacementWeights { Start = 1.0, Middle = 0.0, End = 1.0 };

        public static ZonePlacementWeights NotAtEnd
            = new ZonePlacementWeights { Start = 1.0, Middle = 1.0, End = 0.0 };
    }
}
