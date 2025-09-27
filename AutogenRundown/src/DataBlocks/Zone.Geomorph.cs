using AutogenRundown.DataBlocks.Enums;

namespace AutogenRundown.DataBlocks;

public partial record Zone
{
    /// <summary>
    /// Sets the zone to have a portal geomorph in it
    /// </summary>
    public void GenPortalGeomorph()
    {
        switch (level.Complex)
        {
            // This tile contains a possible path forward.
            case Complex.Mining:
                CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_portal_HA_01.prefab";
                break;

            // This tile is a dead end
            case Complex.Tech:
                CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_portal_HA_01.prefab";
                break;

            case Complex.Service:
            default:
                Plugin.Logger.LogError($"Attempted to build a portal geomorph for Service complex");
                break;
        }
    }
}
