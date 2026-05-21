namespace AutogenRundown.DataBlocks.Enums;

/// <summary>
/// Used for determining what type of security door to use between zones. Most will just use
/// "Security", and "Apex" marks doors to be red and extra secure looking.
///
/// FreePassage is an autogen-only extension: the game's eSecurityGateType only defines
/// Security (0) and Apex (1). Value 2 is recognized only by Patch_LG_BuildGateJob_OpenPath,
/// which intercepts LG_BuildGateJob.Build and replaces the zone-source gate with a
/// LG_GateType.FreePassage flow (no security door, traversable from start).
///
/// See:
///  * https://gtfo-modding.gitbook.io/wiki/reference/nested-types/expeditionzonedata#securitygatetoenter-esecuritygatetype-enum
///  * https://gtfo-modding.gitbook.io/wiki/reference/enum-types#esecuritygatetype
/// </summary>
public enum SecurityGate
{
    Security = 0,
    Apex = 1,
    FreePassage = 2
}
