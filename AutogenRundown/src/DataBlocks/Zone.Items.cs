namespace AutogenRundown.DataBlocks;

public partial record Zone
{
    public bool ForceBigPickupsAllocation { get; set; } = false;

    /// <summary>
    /// The Big pickup distribution data block to use
    /// </summary>
    public uint BigPickupDistributionInZone { get; set; } = 0;
}
