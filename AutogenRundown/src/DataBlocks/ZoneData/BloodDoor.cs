using AutogenRundown.DataBlocks.Enemies;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.ZoneData;
/*
BloodDoor_Easy = 30,
BloodDoor_Medium = 76,
BloodDoor_Bigs = 74,

BloodDoor_Chargers_Easy = 32,
BloodDoor_ChargersGiant_Easy = 72,

BloodDoor_Hybrids_Easy = 31,
BloodDoor_Hybrids_Medium = 33,

BloodDoor_Shadows_Easy = 77,
BloodDoor_Shadows_Medium = 35,

BloodDoor_BossMother = 36,
BloodDoor_BossMotherSolo = 49,
BloodDoor_BossTank = 46,

BloodDoor_Pouncers = 75,
*/

public class BloodDoor
{
    #region Properties

    [JsonProperty("HasActiveEnemyWave")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Can pick between VanillaEnemyGroup or a custom one
    /// </summary>
    public EnemyGroup GroupBehindDoor { get; set; } = EnemyGroup.None;

    [JsonProperty("EnemyGroupInfrontOfDoor")]
    public uint EnemyGroupInfrontOfDoor => GroupBehindDoor.PersistentId;

    public EnemyGroup GroupInArea { get; set; } = EnemyGroup.None;

    [JsonProperty("EnemyGroupInArea")]
    public uint EnemyGroupInArea => GroupInArea.PersistentId;

    [JsonProperty("EnemyGroupsInArea")]
    public int AreaGroups { get; set; } = 0;

    #endregion

    public static readonly BloodDoor None = new() { Enabled = false };

    public static readonly BloodDoor Easy = new BloodDoor
    {
        Enabled = true,
        GroupBehindDoor = EnemyGroup.BloodDoor_Baseline_Easy,
        // AreaGroups = 1
    };

    public static readonly BloodDoor Medium = new BloodDoor
    {
        Enabled = true,
        GroupBehindDoor = EnemyGroup.BloodDoor_Baseline_Easy,
        // AreaGroups = 1
    };

    public static readonly BloodDoor HybridsEasy = new BloodDoor
    {
        Enabled = true,
        GroupBehindDoor = EnemyGroup.BloodDoor_Baseline_Easy,
        // AreaGroups = 1
    };

    public static readonly BloodDoor Easy_2x = new BloodDoor
    {
        Enabled = true,
        GroupBehindDoor = EnemyGroup.BloodDoor_Baseline_Easy,
        // AreaGroups = 1
    };
}
