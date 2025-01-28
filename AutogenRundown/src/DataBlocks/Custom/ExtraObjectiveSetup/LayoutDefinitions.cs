using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class LayoutDefinitions
{
    [JsonIgnore]
    public ExtraObjectiveSetupType Type { get; set; }

    [JsonIgnore]
    public string Name { get; set; } = "";

    public uint MainLevelLayout { get; set; } = 0u;

    public List<Definition> Definitions { get; set; } = new();

    /// <summary>
    /// Primarily cleans the directories
    /// </summary>
    public static void Setup()
    {
        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        Directory.Delete(
            Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}", "Custom", "ExtraObjectiveSetup"),
            recursive: true);
    }

    public void Save()
    {
        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();
        var folder = Type switch
        {
            ExtraObjectiveSetupType.ActivateSmallHSU => "ActivateSmallHSU",
            ExtraObjectiveSetupType.EventsOnBossDeath => "EventsOnBossDeath",
            ExtraObjectiveSetupType.EventsOnScoutScream => "EventsOnScoutScream",
            ExtraObjectiveSetupType.ExtraExpeditionSettings => "ExtraExpeditionSettings",
            ExtraObjectiveSetupType.GeneratorCluster => "GeneratorCluster",
            ExtraObjectiveSetupType.IndividualGenerator => "IndividualGenerator",
            ExtraObjectiveSetupType.ObjectiveCounter => "ObjectiveCounter",
            ExtraObjectiveSetupType.ReactorShutdown => "ReactorShutdown",
            ExtraObjectiveSetupType.ReactorStartup => "ReactorStartup",
            ExtraObjectiveSetupType.TerminalPosition => "TerminalPosition",
            ExtraObjectiveSetupType.TerminalUplink => "TerminalUplink"
        };

        var dir = Path.Combine(
            Paths.BepInExRootPath,
            "GameData",
            $"{revision}",
            "Custom",
            "ExtraObjectiveSetup",
            folder);
        var path = Path.Combine(dir, $"{MainLevelLayout}_{Name}.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}
