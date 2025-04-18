using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Items;

public record class ItemSpawn
{
    public static List<ItemSpawn> GlowSticks(double weight = 1.0)
        => new List<ItemSpawn>
        {
            new ItemSpawn
            {
                Item = Item.GlowStick_Christmas,
                Weight = weight / 4
            },
            new ItemSpawn
            {
                Item = Item.GlowStick_Halloween,
                Weight = weight / 4
            },
            new ItemSpawn
            {
                Item = Item.GlowStick_Yellow,
                Weight = weight / 4
            },
            new ItemSpawn
            {
                Item = Item.GlowStick,
                Weight = weight / 4
            },
        };

    /// <summary>
    /// Persistent Id of the item to spawn.
    /// </summary>
    [JsonProperty("itemID")]
    public Item Item { get; set; } = 0;

    public double Weight { get; set; } = 1.0;
}