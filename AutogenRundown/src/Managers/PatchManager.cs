using AutogenRundown.Patches;

namespace AutogenRundown.Managers;

public static class PatchManager
{
    public static void Setup()
    {
        Fix_NavMeshMarkerSubSeed.Setup();

        Patch_LG_ComputerTerminal_Setup.Setup();
        Patch_LG_SecurityDoor.Setup();
    }
}
