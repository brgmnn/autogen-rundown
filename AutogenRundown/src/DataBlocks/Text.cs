using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public record LanguageData
{
    public string Translation { get; set; } = "";
    public bool ShouldTranslate => true;
}

public record Text : DataBlock
{
    record GameDataText : Text
    {
        public GameDataText() : base(PidOffsets.None)
        { }
    }

    /// <summary>
    /// Instantiates a new text field. This will also persist that text string to the data blocks array. So ensure you
    /// use it.
    /// </summary>
    /// <param name="text"></param>
    public Text(string text)
    {
        Value = text;
        Bins.Texts.AddBlock(this);
    }

    public Text(PidOffsets offsets = PidOffsets.WavePopulation)
        : base(Generator.GetPersistentId(offsets))
    { }

    public static void Setup()
        => Setup<GameDataText, Text>(Bins.Texts, "Text");

    /// <summary>
    /// Local progression text for completing a level with no boosters
    /// </summary>
    private static GameDataText LocalProgression = new()
    {
        Value = "Unaugmented",
        CharacterMetaData = 5,
        ExportVersion = 1,
        ImportVersion = 1,
        Name = "InGame.ExpeditionSuccessPage.AllClearWithNoBoosterUsed",
        IsLocalProgression = true,
        PersistentId = Generator.GetPersistentId()
    };

    /// <summary>
    /// Replaces the big red text on the rundown title
    /// </summary>
    private static Text RundownSelectionTitle = new()
    {
        // CLCTR\u00A0multithread processor activated\r\n\r\n",
        Value = "RAND-GEN AUTOMATIC PROCESSOR ACTIVATED",
        ExportVersion = 2,
        ImportVersion = 2,
        Name = "MainMenu.RundownPage.R1_alt_RoleplayHeader",
        PersistentId = 1616762565
    };

    public new static void SaveStatic()
    {
        Bins.Texts.AddBlock(LocalProgression);

        Bins.Texts.ReplaceBlock(RundownSelectionTitle);
    }

    /// <summary>
    /// The actual text value that this text holds.
    /// </summary>
    [JsonProperty("English")]
    public string Value { get; set; } = "";

    [JsonIgnore]
    private bool IsLocalProgression { get; set; } = false;

    [JsonProperty("name")]
    public new string BlockName
    {
        get => IsLocalProgression ? "InGame.ExpeditionSuccessPage.AllClearWithNoBoosterUsed" : $"{PersistentId}_{Name}";
        private set { }
    }

    #region Fixed properties
    public string Description { get; set; } = "";
    public int CharacterMetaData  { get; set; } = 2;

    public readonly LanguageData French = new();
    public readonly LanguageData Italian = new();
    public readonly LanguageData German = new();
    public readonly LanguageData Spanish = new();
    public readonly LanguageData Russian = new();
    [JsonProperty("Portuguese_Brazil")]
    public readonly LanguageData PortugueseBrazil = new();
    public readonly LanguageData Polish = new();
    public readonly LanguageData Japanese = new();
    public readonly LanguageData Korean = new();
    [JsonProperty("Chinese_Traditional")]
    public readonly LanguageData ChineseTraditional = new();
    [JsonProperty("Chinese_Simplified")]
    public readonly LanguageData ChineseSimplified = new();

    public bool SkipLocalization { get; set; } = true;
    public bool MachineTranslation { get; set; } = false;
    public int ExportVersion { get; set; } = 2;
    public int ImportVersion { get; set; } = 0;
    #endregion
}
