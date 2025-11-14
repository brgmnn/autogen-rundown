using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public record LanguageData
{
    public string Translation { get; set; } = "";
    public bool ShouldTranslate => true;
}

public record GameDataText : Text
{
    [JsonIgnore]
    public static Dictionary<uint, Text> Blocks { get; } = new();

    [JsonIgnore]
    public static Dictionary<string, List<uint>> BlocksLookup { get; } = new();

    public GameDataText() : base(PidOffsets.None)
    { }
}

public record Text : DataBlock<Text>
{
    /// <summary>
    /// Instantiates a new text field. This will also persist that text string to the data blocks array. So ensure you
    /// use it.
    /// </summary>
    /// <param name="text"></param>
    public Text(string text)
    {
        Value = () => text;
        Bins.Texts.AddBlock(this);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="lazyText"></param>
    public Text(Func<string> lazyText)
    {
        Value = lazyText;
        Bins.Texts.AddBlock(this);
    }

    public Text(PidOffsets offsets = PidOffsets.WavePopulation)
        : base(Generator.GetPersistentId(offsets))
    { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Text FromFile(string path)
    {
        var text = new Text();

        return text;
    }

    public static void Setup()
    {
        Setup<GameDataText>(Bins.Texts, "Text", text =>
        {
            GameDataText.Blocks.Add(text.PersistentId, text);

            if (GameDataText.BlocksLookup.ContainsKey(text.English))
                GameDataText.BlocksLookup[text.English].Add(text.PersistentId);
            else
                GameDataText.BlocksLookup[text.English] = new List<uint> { text.PersistentId };
        });

        Plugin.Logger.LogWarning($"How many text data blocks from base game 111? {GameDataText.Blocks.Count}");

        // var dupes = GameDataText.BlocksLookup
        //     .Where(pair => pair.Value.Count > 1)
        //     .OrderBy(pair => pair.Value.Count)
        //     .Select(pair => $"--> {pair.Key}: [{string.Join(", ", pair.Value)}]");
        //
        // Plugin.Logger.LogDebug("Duplicate TextIDs");
        //
        // foreach (var dupe in dupes)
        //     Plugin.Logger.LogDebug(dupe);
    }

    /// <summary>
    /// Local progression text for completing a level with no boosters
    /// </summary>
    private static GameDataText LocalProgression = new()
    {
        English = "Unaugmented",
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
        English = "RAND-GEN AUTOMATIC PROCESSOR ACTIVATED",
        ExportVersion = 2,
        ImportVersion = 2,
        Name = "MainMenu.RundownPage.R1_alt_RoleplayHeader",
        PersistentId = 1616762565
    };

    public static Text None = new Text { PersistentId = 0 };

    public new static void SaveStatic()
    {
        Bins.Texts.AddBlock(LocalProgression);

        Bins.Texts.ReplaceBlock(RundownSelectionTitle);
    }

    /// <summary>
    /// The actual text value that this text holds.
    /// </summary>
    [JsonProperty("English")]
    public string English
    {
        get => Value.Invoke();
        set => Value = () => value;
    }

    [JsonIgnore]
    public Func<string> Value { get; set; } = () => "";

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
