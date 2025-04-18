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
    [JsonProperty("HasActiveEnemyWave")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Can pick between VanillaEnemyGroup or a custom one
    /// </summary>
    public uint EnemyGroupInfrontOfDoor { get; set; } = 0;

    public uint EnemyGroupInArea { get; set; } = 0;

    public int EnemyGroupsInArea { get; set; } = 0;

    static public BloodDoor None = new BloodDoor { Enabled = false };

    static public BloodDoor Easy = new BloodDoor
    {
        Enabled = true,
        EnemyGroupInfrontOfDoor = (uint)VanillaEnemyGroup.BloodDoor_Easy,
        EnemyGroupInArea = (uint)VanillaEnemyGroup.BloodDoor_Easy,
        EnemyGroupsInArea = 1
    };

    static public BloodDoor Medium = new BloodDoor
    {
        Enabled = true,
        EnemyGroupInfrontOfDoor = (uint)VanillaEnemyGroup.BloodDoor_Medium,
        EnemyGroupInArea = (uint)VanillaEnemyGroup.BloodDoor_Medium,
        EnemyGroupsInArea = 1
    };

    static public BloodDoor HybridsEasy = new BloodDoor
    {
        Enabled = true,
        EnemyGroupInfrontOfDoor = (uint)VanillaEnemyGroup.BloodDoor_Hybrids_Easy,
        EnemyGroupInArea = 0,
        EnemyGroupsInArea = 1
    };

    static public BloodDoor Easy_2x = new BloodDoor
    {
        Enabled = true,
        EnemyGroupInfrontOfDoor = (uint)VanillaEnemyGroup.BloodDoor_Easy,
        EnemyGroupInArea = (uint)VanillaEnemyGroup.BloodDoor_Easy,
        EnemyGroupsInArea = 1
    };
}