using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class Target
{
    /// <summary>
    /// Converts the Mode enum to the output string for JSON
    /// </summary>
    [JsonProperty("Mode")]
    public string JsonMode
    {
        get => Mode switch
        {
            Mode.PersistentId => "PersistentID",
            Mode.NameEquals => "NameEquals",
            Mode.NameContains => "NameContains",
            Mode.Everything => "Everything",
            Mode.CategoryAny => "CategoryAny",
            Mode.CategoryAll => "CategoryAll",
        };
        private set { }
    }

    /// <summary>
    /// Possible Values for Mode:
    /// * PersistentID - uses persistentIDs field, enemy with matching id will be selected
    /// * NameEquals - uses nameParam field, If enemy's datablock name is equal to this
    /// * NameContains - uses nameParam field, If enemy's datablock name contains this substring
    /// * Everything - this applies to every enemies!
    /// * CategoryAny - uses categories field, If enemy is matching with any category that listed (SEE ALSO: Category.json file!)
    /// * CategoryAll - uses categories field, If enemy is matching with all category that listed
    /// </summary>
    [JsonIgnore]
    public Mode Mode { get; set; } = Mode.PersistentId;

    /// <summary>
    /// PersistentIDs for PersistentID Mode
    ///
    /// Default = []
    /// </summary>
    [JsonProperty("persistentIDs")]
    public List<uint> PersistentIds { get; set; } = new();

    /// <summary>
    /// NameParameter for NameEquals and NameContains Mode
    ///
    /// Default = ""
    /// </summary>
    [JsonProperty("nameParam")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Should we ignore case for NameContains and NameEquals?
    ///
    /// Default = false
    /// </summary>
    [JsonProperty("nameIgnoreCase")]
    public bool IgnoreCase { get; set; }

    /// <summary>
    /// Category list for CategoryAny/CategoryAll Setting
    /// </summary>
    [JsonProperty("categories")]
    public ICollection<string> Categories { get; set; } = new List<string>();
}
