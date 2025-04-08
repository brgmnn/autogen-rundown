namespace AutogenRundown.PeerMods;

public class Mod
{
    public int ManifestVersion { get; set; }
    public string Name { get; set; }
    public string AuthorName { get; set; }
    public string WebsiteUrl { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string GameVersion { get; set; }
    public string NetworkMode { get; set; }
    public string PackageType { get; set; }
    public string InstallMode { get; set; }
    public long InstalledAtTime { get; set; }
    public List<string> Loaders { get; set; }
    public List<string> Dependencies { get; set; }
    public List<string> Incompatibilities { get; set; }
    public List<string> OptionalDependencies { get; set; }
    public VersionNumber VersionNumber { get; set; }
    public bool Enabled { get; set; }
    public string Icon { get; set; }
}
