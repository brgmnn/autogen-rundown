using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutogenRundown.DataBlocks.Enemies
{
    public enum VanillaEnemyGroup : uint
    {
        BloodDoor_Easy = 30,
        BloodDoor_Medium = 76,
        BloodDoor_Bigs = 74,

        BloodDoor_Chargers_Easy = 32,
        BloodDoor_ChargersGiant_Easy = 72,

        BloodDoor_Hybrids_Easy = 31,
        BloodDoor_Hybrids_Medium = 33,

        BloodDoor_Shadows_Easy = 77,
        BloodDoor_Shadows_Medium = 35, // This seems to be the same as Shadows_Easy

        BloodDoor_BossMother = 36,
        BloodDoor_BossMotherSolo = 49,
        BloodDoor_BossTank = 46,

        BloodDoor_Pouncers = 75,
    }

    internal class EnemyGroup
    {
    }
}
