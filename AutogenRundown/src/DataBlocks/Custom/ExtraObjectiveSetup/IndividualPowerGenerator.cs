using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class IndividualPowerGenerator : Definition
{
    /// <summary>
    /// Mod notes: If set to true, this generator will be able to be plugged in power cell.
    /// </summary>
    public bool ForceAllowPowerCellInsertion { get; set; } = false;

    /// <summary>
    /// Execute warden events on inserting power cell.
    /// </summary>
    public List<WardenObjectiveEvent> EventsOnInsertCell { get; set; } = new();
}
