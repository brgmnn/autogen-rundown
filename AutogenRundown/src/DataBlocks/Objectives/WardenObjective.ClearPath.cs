using AutogenRundown.DataBlocks.Enemies;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: ClearPath
 *
 *
 * Fairly straight forward objective, get to the end zone. Some additional enemies
 * at the end make this a more interesting experience.
 *
 * This objective can only be for Main given it ends the level on completion
 */
public partial record class WardenObjective : DataBlock
{
    public void Build_ClearPath(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        // TODO: For some reason "[EXTRACTION_ZONE]" is not registering the exit zone correctly.
        // For now we manually find the exit zone number.
        var exitZone = layout.Zones.Find(z => z.CustomGeomorph != null && z.CustomGeomorph.Contains("exit_01"));
        var exitIndex = layout.ZoneAliasStart + exitZone?.LocalIndex;
        var exitZoneString = $"<color=orange>ZONE {exitIndex}</color>";

        MainObjective = $"Clear a path to the exit point in {exitZoneString}";
        GoToWinCondition_Elevator = "";
        GoToWinCondition_CustomGeo = $"Go to the forward exit point in {exitZoneString}";

        // Ensure there's a nice spicy hoard at the end
        exitZone?.EnemySpawningInZone.Add(
            // These will be predominately strikers / shooters
            new EnemySpawningData()
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)EnemyRoleDifficulty.Easy,
                Points = 75, // 25pts is 1.0 distribution, this is quite a lot
            });
    }
}
