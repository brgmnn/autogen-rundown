namespace AutogenRundown.DataBlocks.Enums;

/// <summary>
/// Used for determining what type of security door to use between zones. Most will just use
/// "Security", and "Apex" marks doors to be red and extra secure looking.
///
/// See:
///  * https://gtfo-modding.gitbook.io/wiki/reference/nested-types/expeditionzonedata#securitygatetoenter-esecuritygatetype-enum
///  * https://gtfo-modding.gitbook.io/wiki/reference/enum-types#esecuritygatetype
/// </summary>
public enum SecurityGate
{
    Security = 0,
    Apex = 1
}
