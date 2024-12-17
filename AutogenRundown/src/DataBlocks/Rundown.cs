using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    public record class Rundown : DataBlock
    {
        public static uint Tutorial = 39;
        public static uint Geomorph = 27;

        public static uint R1 = 32;
        public static uint R2 = 33;
        public static uint R3 = 34;
        public static uint R4 = 37;
        public static uint R5 = 38;
        public static uint R6 = 41;
        public static uint R7 = 31;
        public static uint R8 = 35;

        public static uint R_Daily = 1;
        public static uint R_Weekly = 2;
        public static uint R_Monthly = 3;

        public List<Level> TierA { get; set; } = new List<Level>();
        public List<Level> TierB { get; set; } = new List<Level>();
        public List<Level> TierC { get; set; } = new List<Level>();
        public List<Level> TierD { get; set; } = new List<Level>();
        public List<Level> TierE { get; set; } = new List<Level>();

        /// <summary>
        /// Name of the rundown to be used
        /// </summary>
        [JsonIgnore]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Name of the rundown to be used
        /// </summary>
        [JsonIgnore]
        public string StoryTitle { get; set; } = string.Empty;


        // Values we just leave as is
        public bool NeverShowRundownTree = false;
        public bool UseTierUnlockRequirements = false;
        public int VanityItemLayerDropDataBlock = 10;

        // No restrictions on which levels can be accessed, so these are all defaults
        public ReqToReachTier ReqToReachTierB = new();
        public ReqToReachTier ReqToReachTierC = new();
        public ReqToReachTier ReqToReachTierD = new();
        public ReqToReachTier ReqToReachTierE = new();

        public JObject StorytellingData {
            get => JObject.FromObject(new
            {
                Title = StoryTitle,
                TextLog = 1268,
                TextLogPos = new
                {
                    x = 0.0,
                    y = 0.0,
                    magnitude = 0.0,
                    sqrMagnitude = 0.0
                },
                Visuals = new
                {
                    ColorBackground = new Color { Alpha = 1.0f },
                    TierAVisuals = new
                    {
                        Color = Color.MenuVisuals,
                        Scale = 0.75,
                        ScaleYModifier = 0.3
                    },
                    TierBVisuals = new
                    {
                        Color = Color.MenuVisuals,
                        Scale = 0.75,
                        ScaleYModifier = 0.3
                    },
                    TierCVisuals = new
                    {
                        Color = Color.MenuVisuals,
                        Scale = 0.75,
                        ScaleYModifier = 0.3
                    },
                    TierDVisuals = new
                    {
                        Color = Color.MenuVisuals,
                        Scale = 0.75,
                        ScaleYModifier = 0.3
                    },
                    TierEVisuals = new
                    {
                        Color = Color.MenuVisuals,
                        Scale = 0.75,
                        ScaleYModifier = 0.3
                    },
                },
                SurfaceIconPosition = new
                {
                    x = 0.0,
                    y = 0.0,
                    magnitude = 0.0,
                    sqrMagnitude = 0.0
                },
                SurfaceDescription = 1267,
                ExternalExpTitle = 3901084012
            });
            private set { }
        }

        public string DisplaySeed { get; set; } = "";

        public void AddLevel(Level level)
        {
            switch (level.Tier)
            {
                case "A":
                    TierA.Add(level);
                    break;
                case "B":
                    TierB.Add(level);
                    break;
                case "C":
                    TierC.Add(level);
                    break;
                case "D":
                    TierD.Add(level);
                    break;
                case "E":
                    TierE.Add(level);
                    break;
            }
        }

        /// <summary>
        /// Builds a new Rundown
        /// </summary>
        /// <returns></returns>
        public static Rundown Build(Rundown rundown)
        {
            // Rundown.Name is used by LocalProgression for storing the progression data. Ensure
            // this is unique to guarantee we store progression between runs.
            rundown.Name = $"RND_Rundown__Seed={Generator.Seed}";
            rundown.DisplaySeed = Generator.DisplaySeed;

            return rundown;
        }
    }
}
