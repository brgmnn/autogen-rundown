namespace MyFirstPlugin.DataBlocks.Alarms
{
    /// <summary>
    /// Scan types
    /// </summary>
    enum PuzzleType : UInt32
    {
        Large = 2,
        Small = 3,
        
        // Cluster
        ClusterSmall = 4,
        ClusterLarge = 5,
        ClusterSix = 9,

        // Require All teammates (orange)
        AllLarge = 8,
        AllLargeCluster = 15,

        // S-Class scans
        SustainedSmall = 14,
        Sustained = 13,
        SustainedMedium = 32,
        SustainedHuge = 18,
        SustainedMegaHuge = 17,
        SustainedBig_Cluster = 16,

        // Checkpoints
        Checkpoint = 19,

        // Expedition Exit
        ExpeditionExit = 7,

        // Bulkhead
        BulkheadMain = 10,
        BulkheadSecondary = 11,
        BulkheadOverload = 12,
    }

    internal class PuzzleComponent
    {
        public PuzzleType PuzzleType { get; set; } = 0;

        public static PuzzleComponent ScanSmall = new PuzzleComponent { PuzzleType = PuzzleType.Small };
        public static PuzzleComponent ScanLarge = new PuzzleComponent { PuzzleType = PuzzleType.Large };

        public static PuzzleComponent ClusterSmall = new PuzzleComponent { PuzzleType = PuzzleType.ClusterSmall };
        public static PuzzleComponent ClusterLarge = new PuzzleComponent { PuzzleType = PuzzleType.ClusterLarge };
        public static PuzzleComponent ClusterSix = new PuzzleComponent { PuzzleType = PuzzleType.ClusterSix };

        public static PuzzleComponent AllLarge = new PuzzleComponent { PuzzleType = PuzzleType.AllLarge };
        public static PuzzleComponent AllLargeCluster = new PuzzleComponent { PuzzleType = PuzzleType.AllLargeCluster };

        public static PuzzleComponent Sustained = new PuzzleComponent { PuzzleType = PuzzleType.Sustained };
        public static PuzzleComponent SustainedSmall = new PuzzleComponent { PuzzleType = PuzzleType.SustainedSmall };
        public static PuzzleComponent SustainedMedium = new PuzzleComponent { PuzzleType = PuzzleType.SustainedMedium };
        public static PuzzleComponent SustainedHuge = new PuzzleComponent { PuzzleType = PuzzleType.SustainedHuge };
        public static PuzzleComponent SustainedMegaHuge = new PuzzleComponent { PuzzleType = PuzzleType.SustainedMegaHuge };
        public static PuzzleComponent SustainedBig_Cluster = new PuzzleComponent { PuzzleType = PuzzleType.SustainedBig_Cluster };
    }
}
