using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public record Rundown : DataBlock
{
    public static readonly uint R_Daily = 1;
    public static readonly uint R_Weekly = 2;
    public static readonly uint R_Monthly = 3;

    public List<Level> TierA { get; set; } = new();
    public List<Level> TierB { get; set; } = new();
    public List<Level> TierC { get; set; } = new();
    public List<Level> TierD { get; set; } = new();
    public List<Level> TierE { get; set; } = new();

    [JsonIgnore]
    public int TierA_Count { get; set; } = Generator.Between(1, 2);
    [JsonIgnore]
    public int TierB_Count { get; set; } = Generator.Between(3, 4);
    [JsonIgnore]
    public int TierC_Count { get; set; } = Generator.Between(2, 4);
    [JsonIgnore]
    public int TierD_Count { get; set; } = Generator.Between(1, 2);
    [JsonIgnore]
    public int TierE_Count { get; set; } = Generator.Between(1, 2);

    /// <summary>
    /// If there are build seeds in this pool they will be used to set the build seed of the level being built.
    /// </summary>
    [JsonIgnore]
    public List<int> BuildSeedPool { get; set; } = new();

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
    public int VanityItemLayerDropDataBlock = 0;

    #region Unlock requirements for levels
    public bool UseTierUnlockRequirements { get; set; } = false;

    // No restrictions on which levels can be accessed, so these are all defaults
    public ReqToReachTier ReqToReachTierB = new();
    public ReqToReachTier ReqToReachTierC = new();
    public ReqToReachTier ReqToReachTierD = new();
    public ReqToReachTier ReqToReachTierE = new();
    #endregion

    private double ScaleTierWidth(int levelCount)
        => levelCount switch
        {
            1 => 0.01,
            2 => 0.50,
            3 => 0.65,
            4 => 0.75,
            5 => 0.85,

            _ => 1.0
        };

    [JsonIgnore]
    public Color VisualsETier { get; set; } = Color.MenuVisuals;

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
                    Scale = ScaleTierWidth(TierA.Count),
                    ScaleYModifier = 0.3
                },
                TierBVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = ScaleTierWidth(TierB.Count),
                    ScaleYModifier = 0.3
                },
                TierCVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = ScaleTierWidth(TierC.Count),
                    ScaleYModifier = 0.3
                },
                TierDVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = ScaleTierWidth(TierD.Count),
                    ScaleYModifier = 0.3
                },
                TierEVisuals = new
                {
                    Color = VisualsETier,
                    Scale = ScaleTierWidth(TierE.Count),
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

    [JsonIgnore]
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
        rundown.Name = $"RndRundownSeed{Generator.Seed.Replace("_", "")}";
        rundown.DisplaySeed = Generator.DisplaySeed;

        return rundown;
    }

    [JsonProperty("name")]
    public new string BlockName
    {
        get => Name;
        private set { }
    }
}