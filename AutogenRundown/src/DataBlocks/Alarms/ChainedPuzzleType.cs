namespace AutogenRundown.DataBlocks.Alarms;

public record ChainedPuzzleType : DataBlock<ChainedPuzzleType>
{
    public string Prefab { get; set; } = "";

    public ChainedPuzzleType(PidOffsets offsets = PidOffsets.None)
        : base(Generator.GetPersistentId(offsets))
    { }

    public static void Setup()
        => Setup<GameDataChainedPuzzleType>(Bins.ChainedPuzzleTypes, "ChainedPuzzleType");

    public new static void SaveStatic() { }
}

public record GameDataChainedPuzzleType : ChainedPuzzleType
{
    public GameDataChainedPuzzleType() : base(PidOffsets.None)
    { }
}
