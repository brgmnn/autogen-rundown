namespace AutogenRundown.DataBlocks.Objectives;

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

    public static string LockedDoorMessage =
        "<color=red>://ERROR: Door in emergency lockdown, unable to operate.</color>";

    public static string TitlePrefix_LevelErrorAlarm = "<color=red>?!</color><color=#444444>-</color>";

    /// <summary>
    ///
    /// </summary>
    /// <param name="dimension"></param>
    /// <param name="bulkhead"></param>
    /// <param name="zoneNumber"></param>
    /// <param name="itemIndex"></param>
    /// <returns></returns>
    public static string TerminalSerial(string dimension, Bulkhead bulkhead, int zoneNumber, int itemIndex = 0)
    {
        var dimensionIndex = dimension switch
        {
            _ => 0
        };
        var layer = bulkhead switch
        {
            Bulkhead.Main => 0,
            Bulkhead.Extreme => 1,
            Bulkhead.Overload => 2,
            _ => 0
        };

        return $"[TERMINAL_{dimensionIndex}_{layer}_{zoneNumber}_{itemIndex}]";
    }

    /// <summary>
    /// Pulls a random variant of the string "Unknown Error"
    /// </summary>
    /// <returns></returns>
    public static string UnknownError() => Generator.Select(new List<(double, string)>
    {
        (1.2, "Unknown Error"),
        (0.5, "Unkn0wn Error"),
        (0.5, "Unknoẃn Error"),
        (1.0, "UNKN0wИ .3rr0R"),
        (0.7, "Uиκиowи .3rror"),
        (1.0, "Unkn0wИ Err0Я"),
        (1.0, "UnkИoшИ Eггoя"),
        (0.7, "Uηκηøwη Eггøг"),
        (1.0, "Ûnknõwn Ërrør"),
        (0.4, "UNK//N0WN::ERROR"),
        (0.7, @"Unkno\\n E//ror"),
        (0.7, "Un<kn>own Er|ror"),
        (0.7, "Unk.nwn Er_r0r"),
    })!;

    public static string UnknownError_Error() => Generator.Pick(new List<string>
    {
        $":://WARNING - {UnknownError()}: Error 0ccurred... 0xF@!#~",
        $":://WARNING - {UnknownError()}: Err0r оcçurr...",
        $":://WARNING - {UnknownError()}: Err0r occurr3d... sy$†3m??",
        $":://WARNING - {UnknownError()}: ErЯor o\u035fccurring. ΔΞρ†Ξd!!",
        $":://WARNING - {UnknownError()}: E\u0338rr\u0337o\u035fr dΞte\u00a2ted! @#~vØ1dΞ",
        $":://WARNING - {UnknownError()}: EЯЯ0Я iиρu† brΞaκ//liиe %!~",
        $":://WARNING - {UnknownError()}: F4iℓure dΞtected… ::ⱯB0Я†",
        $":://WARNING - {UnknownError()}: Data Ɐи0мaℓy \u2204//gΛтΞd!~",
        $":://WARNING - {UnknownError()}: Re§ponse ///corrup†Ξd??~",
        $":://WARNING - {UnknownError()}: UNK//ref==NULL…::EΞΞΞ>!!",
        $":://WARNING - {UnknownError()}: SystΣm vaɭue cяashΞd… ##"
    })!;

    public static string UnknownError_Biomass() => Generator.Pick(new List<string>
    {
        $":://WARNING - {UnknownError()}: Biomass s\u0337ig\u0336n\u0334al unl0c\u0336kΞd...",
        $":://WARNING - {UnknownError()}: Undetected bi0mass \u2192 mσνιиg",
        $":://WARNING - {UnknownError()}: Lαrge bi0mass b\u0360r\u0360e\u035fach dΞte\u00a2ted",
        $":://WARNING - {UnknownError()}: BIO-TRACE lΞvels surging...!!",
        $":://WARNING - {UnknownError()}: Proximity \u2191\u2191| biomass unιt? ~",
        $":://WARNING - {UnknownError()}: BΙΟ MΛSS escaping ~\u2206\u2206 RUN",
        $":://WARNING - {UnknownError()}: Bιo \u2206n0maly bɾeeding…! ?~",
        $":://WARNING - {UnknownError()}: \u2588\u2588 mass \u2588 unknown \u2588 groΨing",
        $":://WARNING - {UnknownError()}: Sιɢиαʟ lost - m\u0334a\u0338s\u0335s ιи flux ~",
        $":://WARNING - {UnknownError()}: Untracked organι\u00a2 foям\u2192detΣcted",
        $":://WARNING - {UnknownError()}: Proximity \u2191\u2191| biomass unιt? ~"
    })!;

    public static string UnknownError_Breach() => Generator.Pick(new List<string>
    {
        $":://WARNING - {UnknownError()}: Breach detected at ZN-4A...",
        $":://WARNING - {UnknownError()}: Breach detΣcted — c0rrιdor 3",
        $":://WARNING - {UnknownError()}: BRΞACH ∆LΣЯT… gates unsealed",
        $":://WARNING - {UnknownError()}: Breach dΞtectΞd @ ██entry",
        $":://WARNING - {UnknownError()}: BrΞΔch signal | VENT ΩPΞN",
        $":://WARNING - {UnknownError()}: ██ breach - coиtαιимΞит фailed",
        $":://WARNING - {UnknownError()}: ∄/BREACH/—motion on uиlogged path",
        $":://WARNING - {UnknownError()}: SΞCTOR wall compromised — trace∅",
        $":://WARNING - {UnknownError()}: Entry sΞal Ξrr0r — bяεach NOW",
        $":://WARNING - {UnknownError()}: ∷bяeacн active... σverride ∅lost",
        $":://WARNING - {UnknownError()}: Breach detected—link.sta//",
        $":://WARNING - {UnknownError()}: Breach DΞTECTED... sΞctor=∅∅",
        $":://WARNING - {UnknownError()}: BrΞach—bypass@αctivated...",
        $":://WARNING - {UnknownError()}: Signal loss ▒▒ breach λ0g...",
        $":://WARNING - {UnknownError()}: |BREACH| confirm: ██Δ▒█sys",
        $":://WARNING - {UnknownError()}: Breach?detected??route::bad",
        $":://WARNING - {UnknownError()}: BЯΞACH ƐПTRY@003—RECONRUN::",
        $":://WARNING - {UnknownError()}: Doorcycle fault → breach[1]",
        $":://WARNING - {UnknownError()}: Δccess node splintered:: ∄",
        $":://WARNING - {UnknownError()}: B\u0362r\u0362e\u0362a\u0362c\u0362h\u0362 logged. \u2237ξrrΔ_trΔcΞ"
    })!;

    public static string UnknownError_Any()
    {
        var text = "";

        Generator.SelectRun(new List<(double, Action)>
        {
            (1.0, () => text = UnknownError_Error()),
            (1.0, () => text = UnknownError_Biomass()),
            (1.0, () => text = UnknownError_Breach())
        });

        return text;
    }

    public static string Zone(int zoneNumber) => $"<color=orange>ZONE {zoneNumber}</color>";
}
