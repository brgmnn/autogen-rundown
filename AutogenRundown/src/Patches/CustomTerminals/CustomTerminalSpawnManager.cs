using AutogenRundown.DataBlocks.Objectives;
using LevelGeneration;

namespace AutogenRundown.Patches.CustomTerminals;

public record CustomTerminalSpawnRequest
{
    public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

    public int LocalIndex { get; set; } = 0;

    public string GeomorphName { get; set; } = "";

    public DataBlocks.Vector3 LocalPosition { get; set; } = new();

    public DataBlocks.Vector3 LocalRotation { get; set; } = new();

    public float MarkerClearRadius { get; set; } = 2.0f;

    public LG_LayerType LayerType => Bulkhead switch
    {
        Bulkhead.Main => LG_LayerType.MainLayer,
        Bulkhead.Extreme => LG_LayerType.SecondaryLayer,
        Bulkhead.Overload => LG_LayerType.ThirdLayer,
        _ => LG_LayerType.MainLayer
    };

    public bool HasGeomorphName(string name)
        => name.StartsWith(Path.GetFileNameWithoutExtension(GeomorphName));
}

public static class CustomTerminalSpawnManager
{
    private static readonly Dictionary<uint, List<CustomTerminalSpawnRequest>> Requests = new();

    public static void AddSpawnRequest(uint layoutId, CustomTerminalSpawnRequest request)
    {
        if (!Requests.ContainsKey(layoutId))
            Requests[layoutId] = new List<CustomTerminalSpawnRequest>();

        Requests[layoutId].Add(request);

        Plugin.Logger.LogDebug(
            $"[CustomTerminal] Registered spawn request: layout={layoutId}, " +
            $"zone={request.LocalIndex}, geo={Path.GetFileName(request.GeomorphName)}");
    }

    public static List<CustomTerminalSpawnRequest> GetRequests(uint layoutId)
    {
        return Requests.TryGetValue(layoutId, out var requests)
            ? requests
            : new List<CustomTerminalSpawnRequest>();
    }

    public static void Clear()
    {
        Requests.Clear();
    }
}
