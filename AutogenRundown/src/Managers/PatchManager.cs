using AutogenRundown.Components;
using AutogenRundown.Patches;
using AutogenRundown.Patches.CustomTerminals;

namespace AutogenRundown.Managers;

public static class PatchManager
{
    public static void Setup()
    {
        Fix_NavMeshMarkerSubSeed.Setup();
        ZoneSeedManager.Setup();
        Fix_MissingGeneratorCluster.Setup();
        Fix_FactoryJobExceptionCatchAll.Setup();

        Patch_LG_ComputerTerminal_Setup.Setup();
        Patch_LG_SecurityDoor.Setup();
        Patch_SpawnCustomTerminals.Setup();
        Patch_UplinkWaveIsolation.Setup();

        RundownTierMarkerArchivist.PluginSetup();
    }
}
