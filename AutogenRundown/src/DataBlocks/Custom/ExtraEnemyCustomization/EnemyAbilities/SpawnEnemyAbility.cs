using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.EnemyAbilities;

public class SpawnEnemyAbility
{
    public bool StopAgent { get; set; } = false;

    public int Delay { get; set; } = 0;

    [JsonProperty("EnemyID")]
    public uint EnemyId { get; set; } = 0;

    public string AgentMode { get; set; } = "Agressive";

    public int TotalCount { get; set; } = 0;

    public int CountPerSpawn { get; set; } = 0;

    public int DelayPerSpawn { get; set; } = 0;

    public string Name { get; set; } = "SpawnEnemy";
}
