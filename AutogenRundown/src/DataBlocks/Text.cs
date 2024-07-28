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
    /// The actual text value that this text holds.
    /// </summary>
    [JsonProperty("English")]
    public string Value { get; set; } = "";

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
