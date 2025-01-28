namespace AutogenRundown.DataBlocks.Objectives
{
    // Find strings with: \[\w+_\w+\]
    public class Lore
    {
        public static string AllItems = "[ALL_ITEMS]";

        public static string CountCurrent = "[COUNT_CURRENT]";
        public static string CountRequired = "[COUNT_REQUIRED]";

        /// <summary>
        /// Fills in with the extraction zone. Note that this seems to not function well with Clear Path.
        /// </summary>
        public static string ExtractionZone = "[EXTRACTION_ZONE]";

        public static string ItemSerial = "<color=orange>[ITEM_SERIAL]</color>";

        public static string ItemZone = "[ITEM_ZONE]";

        public static string KeycardZone = "[KEYCARD_ZONE]";

        public static string Zone(int zoneNumber) => $"<color=orange>ZONE {zoneNumber}</color>";
    }
}
