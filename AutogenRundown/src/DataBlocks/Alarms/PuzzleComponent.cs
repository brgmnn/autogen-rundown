namespace AutogenRundown.DataBlocks.Alarms
{
    /// <summary>
    /// Scan types. Scan sizes go from:
    ///
    /// Small -> Big -> Large
    /// </summary>
    public enum PuzzleType : UInt32
    {
        // Single red circles (blue when non-alarm)
        #region Individual scans
        /// <summary>
        /// Single large circle: red when alarmed, blue (stealth) otherwise.
        /// </summary>
        Large = 2,

        /// <summary>
        /// Single small circle: red when alarmed, blue (stealth) otherwise.
        /// Usually not used much.
        /// </summary>
        Small = 3,
        #endregion

        #region Cluster: Multiple individual circles
        /// <summary>
        /// 8 small clusters, blue when non-alarm orange otherwise.
        /// </summary>
        Cluster = 9,

        /// <summary>
        /// 4 small circles, work like PuzzleType.Small
        /// </summary>
        ClusterSmall = 4,

        /// <summary>
        /// 2 large PuzzleType.Large circles
        /// </summary>
        ClusterLarge = 5,
        #endregion

        #region Full team scans (orange)
        // Require All teammates (orange)
        AllBig = 6,
        AllBig_GreenActive = 29, // Same as 27, 28

        /// <summary>
        /// Large orange team scan, blue when scanning with no alarm.
        /// </summary>
        AllLarge = 8,
        AllBig_BlueActive = 25, // Like AllBig, but blue when scanning. Name: SecurityScan_Big_RequireAll_R6A1_Narrative

        AllLarge_Slow = 20, // Name: SecurityScan_D10_RequireAll
        #endregion

        // Stealth scans
        StealthBig_Cluster = 15,

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

    public class PuzzleComponent
    {
        public PuzzleType PuzzleType { get; set; } = 0;

        public static PuzzleComponent BulkheadMain = new PuzzleComponent { PuzzleType = PuzzleType.BulkheadMain };
        public static PuzzleComponent BulkheadSecondary = new PuzzleComponent { PuzzleType = PuzzleType.BulkheadSecondary };
        public static PuzzleComponent BulkheadOverload = new PuzzleComponent { PuzzleType = PuzzleType.BulkheadOverload };

        public static PuzzleComponent ScanSmall = new PuzzleComponent { PuzzleType = PuzzleType.Small };
        public static PuzzleComponent ScanLarge = new PuzzleComponent { PuzzleType = PuzzleType.Large };

        public static PuzzleComponent ClusterSmall = new PuzzleComponent { PuzzleType = PuzzleType.ClusterSmall };
        public static PuzzleComponent ClusterLarge = new PuzzleComponent { PuzzleType = PuzzleType.ClusterLarge };
        public static PuzzleComponent Cluster = new PuzzleComponent { PuzzleType = PuzzleType.Cluster };

        public static PuzzleComponent AllBig = new PuzzleComponent { PuzzleType = PuzzleType.AllBig };
        public static PuzzleComponent AllBig_GreenActive = new PuzzleComponent { PuzzleType = PuzzleType.AllBig_GreenActive };
        public static PuzzleComponent AllLarge = new PuzzleComponent { PuzzleType = PuzzleType.AllLarge };
        public static PuzzleComponent AllLarge_Slow = new PuzzleComponent { PuzzleType = PuzzleType.AllLarge_Slow };

        public static PuzzleComponent StealthBig_Cluster = new PuzzleComponent { PuzzleType = PuzzleType.StealthBig_Cluster };

        public static PuzzleComponent Sustained = new PuzzleComponent { PuzzleType = PuzzleType.Sustained };
        public static PuzzleComponent SustainedSmall = new PuzzleComponent { PuzzleType = PuzzleType.SustainedSmall };
        public static PuzzleComponent SustainedMedium = new PuzzleComponent { PuzzleType = PuzzleType.SustainedMedium };
        public static PuzzleComponent SustainedHuge = new PuzzleComponent { PuzzleType = PuzzleType.SustainedHuge };
        public static PuzzleComponent SustainedMegaHuge = new PuzzleComponent { PuzzleType = PuzzleType.SustainedMegaHuge };
        public static PuzzleComponent SustainedBig_Cluster = new PuzzleComponent { PuzzleType = PuzzleType.SustainedBig_Cluster };
    }
}
