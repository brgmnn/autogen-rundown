namespace AutogenRundown.DataBlocks.Enemies;

public record ModelData
{
    public string ModelFile { get; set; } = "";

    public string ModelCustomization { get; set; } = "";

    /// <summary>
    ///
    /// </summary>
    public Vector3 PositionOffset { get; set; } = Vector3.Zero();

    /// <summary>
    ///
    /// </summary>
    public Vector3 RotationOffset { get; set; } = Vector3.Zero();

    /// <summary>
    ///
    /// </summary>
    public Vector3 NeckScale { get; set; } = new();

    /// <summary>
    ///
    /// </summary>
    public Vector3 HeadScale { get; set; } = new();

    /// <summary>
    /// X = height direction
    /// </summary>
    public Vector3 ChestScale { get; set; } = new();

    /// <summary>
    ///
    /// </summary>
    public Vector3 ArmScale { get; set; } = new();

    /// <summary>
    ///
    /// </summary>
    public Vector3 LegScale { get; set; } = new();

    /// <summary>
    ///
    /// </summary>
    public Vector2 SizeRange { get; set; } = new();
}
