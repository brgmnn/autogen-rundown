using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public record Dimension : DataBlock
{
    #region Properties

    /// <summary>
    /// For some reason the devs decided to put all the properties under this data key
    /// </summary>
    [JsonProperty("DimensionData")]
    public Dimensions.DimensionData Data { get; init; } = new();

    #endregion

    public static readonly Dimension None = new() { PersistentId = 0, Name = "None" };

    public bool RecordEqual(Dimension? other)
    {
        if (other is null || GetType() != other.GetType())
            return false;

        return Data == other.Data;
    }

    public void Persist(BlocksBin<Dimension>? bin = null)
    {
        bin ??= Bins.Dimensions;
        bin.AddBlock(this);
    }

    public static Dimension FindOrPersist(Dimension dimension)
    {
        // We specifically don't want to persist none, as we want to set the PersistentID to 0
        if (dimension == None)
            return None;

        var existing = Bins.Dimensions.GetBlock(dimension.RecordEqual);

        if (existing != null)
            return existing;

        if (dimension.PersistentId == 0)
            dimension.PersistentId = Generator.GetPersistentId(PidOffsets.WaveSettings);

        dimension.Persist();

        return dimension;
    }

    /// <summary>
    /// Instance version of static method
    /// </summary>
    /// <returns></returns>
    public Dimension FindOrPersist() => FindOrPersist(this);
}
