namespace AutogenRundown.DataBlocks.Custom.ZoneSensors;

/// <summary>
/// Density setting for runtime sensor count calculation.
/// When set on a ZoneSensorGroupDefinition, the Count property is ignored
/// and instead calculated at runtime based on zone area (VoxelCoverage).
/// </summary>
public enum SensorDensity
{
    /// <summary>
    /// Use explicit Count property instead of density calculation.
    /// </summary>
    None = 0,

    /// <summary>
    /// Low density: ~3 sensors per 100 units of voxel coverage.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium density: ~6 sensors per 100 units of voxel coverage.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High density: ~12 sensors per 100 units of voxel coverage.
    /// </summary>
    High = 3
}
