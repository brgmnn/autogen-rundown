using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown.SecurityDoors;

public record SecurityDoor
{
    public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

    public int Layer => Bulkhead switch
    {
        Bulkhead.Main => 0,
        Bulkhead.Extreme => 1,
        Bulkhead.Overload => 2,
        _ => 0
    };

    public int ZoneNumber { get; set; } = 0;

    public string? InteractionMessage { get; set; }
}
