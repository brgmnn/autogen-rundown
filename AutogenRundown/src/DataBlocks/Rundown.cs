using AutogenRundown.GeneratorData;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    public record class Rundown : DataBlock
    {
        public static uint R1 = 32;
        public static uint R2 = 33;
        public static uint R3 = 34;
        public static uint R4 = 37;
        public static uint R5 = 38;
        public static uint R6 = 41;
        public static uint R7 = 31;

        public List<Level> TierA { get; set; } = new List<Level>();
        public List<Level> TierB { get; set; } = new List<Level>();
        public List<Level> TierC { get; set; } = new List<Level>();
        public List<Level> TierD { get; set; } = new List<Level>();
        public List<Level> TierE { get; set; } = new List<Level>();

        // Values we just leave as is
        public bool NeverShowRundownTree = false;
        public bool UseTierUnlockRequirements = false;
        public int VanityItemLayerDropDataBlock = 10;

        // No restrictions on which levels can be accessed, so these are all defaults
        public ReqToReachTier ReqToReachTierB = new ReqToReachTier();
        public ReqToReachTier ReqToReachTierC = new ReqToReachTier();
        public ReqToReachTier ReqToReachTierD = new ReqToReachTier();
        public ReqToReachTier ReqToReachTierE = new ReqToReachTier();

        public JObject StorytellingData = JObject.FromObject(new
        {
            Title = "Alt Rundown",
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
        static public Rundown Build(Rundown rundown)
        {
            var name = $"{Generator.Pick(Words.Adjectives)} {Generator.Pick(Words.NounsRundown)}";

            rundown.Name = $"RND Rundown {name}";
            rundown.DisplaySeed = Generator.DisplaySeed;

            var rundownNumber = $"{Generator.DisplaySeed}";
            rundown.StorytellingData["Title"] = $"<color=green>RND://</color>RUNDOWN {rundownNumber}\r\nTITLE: {name.ToUpper()}";

            return rundown;
        }
    }
}
