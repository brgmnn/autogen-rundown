using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelGeneration;
using Localization;

namespace AutogenRundown.Patches;

/// <summary>
/// Patches the Terminal Uplink code words to be longer, and for there to be
/// more code words for harder tiers
/// </summary>
[HarmonyPatch]
public static class TerminalUplink
{
    // A and B tier
    public static string[] FourLetterWords => new string[]
    {
        "able", "acid", "aged", "airy", "ally", "anti", "area", "army", "atom", "auto",
        "axis", "baby", "back", "bait", "bake", "bald", "ball", "band", "bank", "bark",
        "barn", "bath", "bead", "beak", "beam", "beef", "beer", "bell", "belt", "bend",
        "bent", "best", "beta", "bias", "biff", "bike", "bill", "bird", "bite", "blow",
        "blue", "blur", "boat", "body", "bold", "bolt", "bomb", "bone", "book", "boot",
        "born", "boss", "both", "bowl", "brag", "brow", "buck", "bull", "bump", "burn",
        "busy", "busy", "cage", "cake", "call", "camp", "card", "care", "case", "cash",
        "cast", "cave", "cell", "chef", "chew", "chip", "city", "clan", "clap", "claw",
        "clay", "clip", "club", "clue", "coal", "coat", "code", "coin", "cold", "coma",
        "comb", "cone", "cook", "cool", "copy", "core", "corn", "cost", "cozy", "crab",
        "crew", "crop", "crow", "cube", "cuff", "cure", "cute", "damp", "dare", "dark",
        "dart", "dash", "data", "date", "dawn", "dead", "deal", "deck", "deed", "deep",
        "dent", "deny", "desk", "dial", "diet", "dime", "dine", "dire", "dirt", "dish",
        "diva", "dive", "does", "doll", "done", "doom", "door", "dose", "down", "drag",
        "draw", "drip", "drop", "drug", "drum", "dual", "duck", "duel", "duet", "dull",
        "dump", "dune", "dunk", "dust", "duty", "earn", "ease", "east", "easy", "echo",
        "edge", "edit", "eery", "else", "envy", "epic", "etch", "ever", "evil", "exam",
        "exit", "expo", "face", "fact", "fade", "fail", "fake", "fall", "fame", "fang",
        "farm", "fast", "fate", "fear", "feed", "feel", "feet", "fill", "film", "find",
        "fine", "fire", "firm", "fish", "fist", "fits", "five", "flag", "flak", "flap",
        "flat", "flaw", "flew", "flex", "flip", "flop", "flow", "flux", "foam", "foil",
        "folk", "food", "fool", "foot", "fork", "form", "fort", "foul", "foxy", "frat",
        "free", "frog", "fuel", "full", "fund", "funk", "fury", "fuzz", "gain", "game",
        "gang", "gasp", "gate", "gaze", "gear", "germ", "girl", "give", "glow", "goal",
        "goat", "gold", "golf", "gone", "good", "grab", "grid", "grin", "grip", "grow",
        "gulf", "guru", "hack", "half", "hall", "halo", "hard", "hawk", "head", "hear",
        "heat", "hell", "help", "hemp", "herd", "here", "hero", "hide", "hill", "hint",
        "hire", "hiss", "hive", "hoax", "hold", "hole", "holy", "home", "hood", "hook",
        "hope", "horn", "host", "hour", "huge", "hulk", "hunt", "hurl", "hurt", "husk",
        "hymn", "hype", "idea", "idle", "idly", "idol", "iffy", "inch", "iris", "iron",
        "itch", "item", "jack", "jail", "jaws", "jazz", "jeep", "jive", "jobs", "join",
        "joke", "jolt", "jump", "junk", "jury", "just", "keep", "kick", "kill", "kilo",
        "kind", "king", "kiss", "kite", "kiwi", "knee", "knob", "know", "lack", "lady",
        "lair", "lake", "lama", "lamb", "lame", "lamp", "land", "lane", "lard", "lash",
        "last", "lawn", "lazy", "lead", "leaf", "lean", "leap", "left", "lend", "lens",
        "less", "levy", "liar", "life", "lift", "like", "lily", "limb", "lime", "line",
        "link", "lint", "lips", "list", "live", "load", "loaf", "loan", "lock", "logo",
        "long", "look", "loop", "loot", "lord", "lore", "lose", "loss", "lost", "love",
        "luck", "lump", "lush", "lynx", "made", "mail", "main", "make", "mark", "mask",
        "mass", "mate", "math", "maze", "meal", "mean", "menu", "mesh", "mess", "mild",
        "mile", "milk", "mini", "mint", "miss", "mist", "mojo", "monk", "mono", "mood",
        "moon", "more", "most", "moth", "move", "much", "mule", "muse", "must", "mute",
        "nail", "name", "navy", "near", "neat", "neck", "need", "neon", "nest", "news",
        "next", "nice", "nine", "none", "noon", "nose", "nosy", "note", "noun", "nuke",
        "numb", "oboe", "odds", "oily", "omen", "omit", "once", "only", "open", "oval",
        "oven", "over", "pack", "page", "paid", "park", "pass", "past", "path", "pawn",
        "peak", "peel", "pelt", "perk", "pest", "pick", "pipe", "plan", "play", "plot",
        "plug", "plum", "poem", "poke", "pole", "pony", "pool", "pope", "pork", "port",
        "pose", "post", "prop", "puck", "pull", "pulp", "puma", "pump", "punk", "pure",
        "push", "quit", "quiz", "race", "raft", "rage", "raid", "rail", "rain", "ramp",
        "rank", "rant", "rare", "rash", "rate", "rear", "redo", "rely", "rent", "rest",
        "rice", "rich", "ride", "riff", "rift", "ring", "riot", "ripe", "rise", "risk",
        "road", "roar", "robe", "rock", "roof", "room", "rope", "rose", "rosy", "rude",
        "ruin", "rule", "runt", "ruse", "rush", "rust", "safe", "salt", "same", "sand",
        "sane", "save", "scan", "scar", "seal", "seed", "seen", "sell", "semi", "send",
        "shoe", "shop", "shot", "show", "sick", "side", "sift", "sign", "silk", "sing",
        "sink", "site", "size", "skin", "skip", "skit", "slam", "slap", "slay", "slim",
        "slip", "slot", "slow", "slug", "slur", "snag", "snap", "snip", "snow", "snug",
        "soak", "soap", "sock", "soda", "sofa", "soft", "sold", "some", "song", "soon",
        "sort", "soul", "soup", "sour", "spam", "span", "spin", "spit", "spot", "spur",
        "stab", "star", "stay", "stem", "step", "stew", "stir", "stun", "such", "suit",
        "sure", "swan", "swim", "taco", "take", "tale", "talk", "tall", "tame", "tank",
        "task", "taxi", "tear", "teen", "tell", "tend", "tent", "term", "test", "text",
        "that", "them", "then", "they", "thin", "this", "tick", "tide", "tidy", "tier",
        "ties", "tile", "tilt", "time", "tire", "toad", "told", "toll", "tone", "took",
        "tool", "torn", "town", "trap", "tree", "trim", "trio", "trip", "true", "tsar",
        "tuba", "tube", "tuna", "tune", "turk", "turn", "twig", "twin", "type", "tzar",
        "ugly", "undo", "unit", "urge", "used", "user", "vase", "vast", "verb", "very",
        "vest", "vibe", "vice", "view", "vine", "visa", "void", "volt", "vote", "wage",
        "wait", "wake", "walk", "wall", "want", "warm", "warp", "wart", "wash", "wasp",
        "wave", "weak", "weed", "week", "were", "west", "when", "whip", "wife", "wild",
        "will", "wind", "wine", "wing", "wire", "wise", "wolf", "wood", "wool", "word",
        "work", "worm", "yank", "yard", "yarn", "yawn", "year", "yell", "yeti", "yoga",
        "your", "zone", "zoom"
    };

    // C tier
    public static string[] FiveLetterWords => new string[]
    {
        "about", "actor", "adapt", "adieu", "adobe", "aeons", "after", "again", "agape",
        "agate", "aging", "aglow", "agree", "ahead", "aimed", "aired", "alarm", "alive",
        "alley", "allow", "aloft", "aloud", "alpha", "altar", "alter", "altos", "amiss",
        "amity", "among", "amour", "ample", "amply", "angle", "angry", "aping", "appal",
        "apply", "aptly", "arena", "array", "arson", "ashen", "aside", "asked", "asset",
        "atlas", "atone", "attic", "audio", "audit", "augur", "avail", "awake", "award",
        "awoke", "axles", "babel", "baits", "baize", "baker", "balmy", "banjo", "banns",
        "basal", "based", "baste", "batch", "bathe", "baths", "beads", "beams", "bears",
        "beast", "beaux", "begun", "belie", "bench", "berry", "bible", "bides", "bight",
        "biped", "birch", "black", "blade", "blast", "blend", "bless", "blink", "bliss",
        "blots", "blown", "blues", "blush", "boars", "boast", "bodes", "boggy", "boils",
        "boles", "books", "boons", "boots", "booze", "borne", "bough", "braid", "brake",
        "brats", "bread", "break", "brief", "brier", "brine", "brink", "briny", "brood",
        "brown", "budge", "bugle", "build", "built", "bulks", "bulls", "bunks", "burnt",
        "burrs", "burst", "buyer", "cabin", "cable", "caked", "cakes", "calms", "camps",
        "caper", "casks", "casts", "cater", "cedar", "chafe", "chaff", "chain", "chaos",
        "charm", "chart", "chary", "chasm", "chats", "cheat", "check", "cheek", "cheer",
        "chess", "chide", "chill", "choir", "chops", "chord", "chump", "chums", "chunk",
        "churl", "chute", "cinch", "cites", "clash", "clasp", "class", "claws", "clear",
        "climb", "clips", "clods", "clogs", "close", "clout", "clown", "coast", "cobra",
        "comic", "cooed", "cools", "cores", "corns", "count", "court", "cover", "covey",
        "crabs", "craft", "crash", "crass", "crate", "craze", "creak", "creek", "creep",
        "crews", "cribs", "crick", "cries", "crime", "croak", "croup", "crows", "crude",
        "cruel", "crumb", "cubes", "curls", "curly", "curve", "cycle", "cynic", "dairy",
        "dames", "damps", "dance", "dared", "dares", "dates", "daubs", "dears", "death",
        "debit", "debts", "decoy", "decry", "deeds", "deeps", "delay", "dells", "delta",
        "demon", "depot", "deuce", "dimes", "dimly", "diner", "ditch", "ditto", "ditty",
        "dizzy", "docks", "dogma", "doled", "dolls", "dosed", "doses", "doves", "downs",
        "drink", "drone", "ducks", "dukes", "dummy", "dumps", "dunce", "dupes", "dusty",
        "dwell", "dwelt", "eager", "earls", "early", "earth", "eaten", "eater", "edged",
        "egged", "eight", "elate", "elder", "elude", "email", "empty", "ended", "enemy",
        "ennui", "epics", "equal", "equip", "error", "evils", "evoke", "exalt", "excel",
        "expel", "extra", "faces", "false", "fares", "farms", "fasts", "fault", "fears",
        "feign", "fells", "ferry", "feted", "fever", "fiche", "fiefs", "fifth", "fifty",
        "fight", "files", "filet", "fines", "fires", "fishy", "fives", "fjord", "flaky",
        "flame", "flank", "flaps", "flats", "flaws", "fleck", "flees", "flesh", "flier",
        "fling", "flirt", "float", "flood", "floor", "floss", "flows", "flues", "fluke",
        "flume", "folio", "folks", "foods", "foray", "force", "fords", "forks", "forte",
        "forts", "forum", "fours", "foxes", "frame", "fraud", "freer", "frees", "fruit",
        "fugue", "fully", "fumed", "fumes", "furze", "fuses", "gains", "games", "gamin",
        "gangs", "gapes", "gases", "gazes", "geese", "gents", "germs", "ghost", "giant",
        "gilds", "gills", "girth", "given", "gives", "glade", "glass", "glean", "glint",
        "glove", "glows", "gnaws", "goads", "godly", "golly", "gongs", "gonna", "goody",
        "gored", "gourd", "gowns", "grain", "grant", "graph", "grate", "grave", "graze",
        "greed", "greys", "grime", "gross", "grown", "grows", "guard", "guild", "guile",
        "guilt", "gully", "hails", "hairs", "hands", "hangs", "harms", "harsh", "haste",
        "haven", "heady", "heart", "hedge", "heirs", "helix", "helps", "hence", "hides",
        "hills", "hilts", "hinds", "hinge", "hives", "hoist", "holds", "holly", "homes",
        "horde", "hours", "house", "howls", "hulks", "hunch", "hunts", "hurts", "icily",
        "ideas", "idiom", "idled", "idler", "idols", "image", "impel", "imply", "incur",
        "inept", "infer", "inter", "irony", "jacks", "jaunt", "jeans", "jeers", "jelly",
        "jests", "jetty", "jiffy", "jokes", "junks", "karma", "khaki", "kicks", "kinda",
        "knack", "knelt", "knits", "knobs", "knots", "known", "label", "ladle", "lager",
        "lambs", "lamed", "lance", "lanes", "lapel", "larch", "largo", "larva", "latch",
        "later", "lawns", "leafy", "leaky", "leans", "leapt", "lease", "ledge", "level",
        "liege", "liens", "light", "liked", "liken", "limbo", "limbs", "limit", "lines",
        "lions", "lithe", "lived", "livid", "llama", "loads", "loans", "loath", "lobby",
        "lobes", "looms", "loops", "loser", "lousy", "loved", "lower", "lowly", "lucid",
        "lucky", "lulls", "lunar", "lurid", "lusty", "lymph", "lyric", "mains", "maize",
        "major", "maker", "males", "mamma", "mange", "mango", "mania", "manor", "manse",
        "march", "mated", "mates", "mayor", "mazes", "meats", "medal", "media", "meets",
        "melon", "memes", "merit", "metal", "might", "milch", "miles", "milky", "mimes",
        "mince", "mined", "mines", "mints", "minus", "mirth", "mites", "mixed", "moats",
        "modem", "modes", "moles", "moody", "moral", "mores", "mouse", "mover", "mowed",
        "mower", "multi", "munch", "musky", "musty", "myths", "nabob", "naked", "names",
        "nasal", "nasty", "natty", "naval", "nerve", "nests", "niche", "nooks", "north",
        "nosed", "oases", "oaten", "odium", "offer", "oiled", "olden", "older", "omits",
        "onion", "opals", "opens", "opera", "ounce", "ovals", "owner", "oxide", "paddy",
        "paler", "pales", "palsy", "panel", "panic", "pansy", "pared", "party", "paste",
        "patio", "paved", "payed", "payer", "peach", "peaks", "pears", "pecks", "pelts",
        "pence", "penne", "peril", "pesky", "pesos", "pests", "petal", "phase", "piled",
        "pinch", "pines", "pinto", "pints", "pitch", "pivot", "place", "plaid", "plant",
        "plaza", "plush", "poems", "poets", "point", "pokes", "pools", "popes", "poppy",
        "ports", "pound", "pours", "press", "prick", "pries", "prime", "privy", "prize",
        "props", "prosy", "prove", "prows", "proxy", "psalm", "pudgy", "puffy", "pumps",
        "puree", "purge", "putty", "quart", "quays", "queen", "queue", "quips", "quite",
        "raged", "raise", "rated", "raven", "react", "reaps", "rebus", "reeds", "reeve",
        "refer", "relic", "remit", "renew", "reply", "rests", "rhyme", "rides", "right",
        "riled", "rills", "rimes", "rinse", "ripen", "risen", "risks", "risky", "roads",
        "roams", "roast", "roles", "rolls", "roomy", "roost", "roots", "rosin", "rouge",
        "rough", "rouse", "rover", "rowdy", "rowed", "ruder", "ruins", "ruler", "runes",
        "ruses", "sabre", "sagas", "sails", "salsa", "salvo", "scalp", "scarf", "scent",
        "scion", "scold", "scoop", "scope", "scour", "scowl", "seals", "sects", "seeds",
        "seers", "seize", "sense", "serfs", "serum", "sever", "sewed", "shack", "shaft",
        "shaky", "shale", "shall", "shams", "shape", "share", "shawl", "shear", "sheds",
        "shift", "ships", "shirk", "shoal", "shone", "shout", "showy", "shred", "shrub",
        "siege", "sieve", "signs", "silky", "silly", "siren", "sites", "skate", "skies",
        "skiff", "skips", "skirt", "skull", "slack", "slang", "slave", "sleds", "sleek",
        "sleet", "slept", "slime", "slink", "slips", "sloop", "slope", "slunk", "slush",
        "smash", "smelt", "smite", "smoke", "snags", "snake", "snaky", "snaps", "snarl",
        "snipe", "snuff", "soars", "sober", "socks", "soggy", "songs", "sonny", "sores",
        "sough", "souls", "sowed", "space", "spans", "spark", "spelt", "spicy", "spike",
        "spins", "spire", "split", "spoor", "sprig", "spurn", "squad", "stage", "stags",
        "stale", "stamp", "stank", "stare", "start", "steal", "steep", "stews", "stiff",
        "stoic", "store", "storm", "stove", "strew", "stump", "sucks", "suite", "suits",
        "sully", "surer", "surly", "sweat", "swell", "swept", "swims", "swing", "sworn",
        "synod", "taboo", "tacks", "taint", "taken", "tamed", "tapes", "tardy", "tarry",
        "tarts", "tasks", "taunt", "taxed", "teems", "teeth", "tempo", "temps", "tenet",
        "tents", "tepid", "texts", "these", "thick", "third", "those", "thumb", "tibia",
        "tiers", "tight", "tiles", "tires", "toads", "toils", "tolls", "tonic", "tooth",
        "topaz", "topic", "torso", "torts", "tough", "tours", "towed", "toxic", "toyed",
        "trade", "trail", "trash", "treed", "trice", "trick", "tries", "trill", "tripe",
        "trite", "truly", "trump", "trunk", "trust", "tubes", "tunes", "tunic", "tusks",
        "tweed", "twice", "types", "udder", "uncle", "uncut", "under", "unfit", "unsay",
        "until", "urban", "users", "utter", "vales", "vases", "venom", "verbs", "vials",
        "views", "villa", "viola", "visit", "vista", "vogue", "voice", "volts", "voter",
        "vouch", "vowel", "vying", "wakes", "walks", "warns", "waxed", "wears", "weave",
        "weeks", "weigh", "whale", "wheel", "whelp", "which", "whine", "whirl", "whist",
        "whoop", "whose", "wield", "wilds", "wiles", "wills", "winds", "wines", "winks",
        "wiped", "wipes", "wired", "wires", "women", "works", "worst", "worth", "wound",
        "wrath", "wreak", "wrong", "years", "yield", "yoked", "yolks", "young", "yours"
    };

    // D tier
    public static string[] SevenLetterWords => new[]
    {
        "hello"
    };

    // E tier
    public static string[] NineLetterWords => new[]
    {
        "hello"
    };

	public static LocalizedText StringToLocalized(string stringConvert)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		return new LocalizedText
		{
			Id = 0u,
			UntranslatedText = stringConvert
		};
	}

    #region SerialGenerator
    /// <summary>
    /// Sets up the code words and code word prefixes pool for choosing words from
    /// </summary>
	[HarmonyPatch(typeof(SerialGenerator), "Setup")]
	[HarmonyPrefix]
    public static void SerialGenerator_Setup()
	{
        Plugin.Logger.LogWarning($"Expedition tier is: ${RundownManager.GetActiveExpeditionData().tier}");

        var tier = RundownManager.GetActiveExpeditionData().tier;

        SerialGenerator.m_codeWordPrefixes = tier switch
        {
            // Vanilla prefixes
            <= eRundownTier.TierB => new string[27]
            {
                "X01", "X02", "X03", "X04", "X05", "X06", "X07", "X08", "X09",
                "Y01", "Y02", "Y03", "Y04", "Y05", "Y06", "Y07", "Y08", "Y09",
                "Z01", "Z02", "Z03", "Z04", "Z05", "Z06", "Z07", "Z08", "Z09"
            },
            >= eRundownTier.TierC => new string[84]
            {
                "A01", "A02", "A03", "A04", "A05", "A06", "A07", "A08", "A09", "A10", "A11", "A12",
                "B01", "B02", "B03", "B04", "B05", "B06", "B07", "B08", "B09", "B10", "B11", "B12",
                "C01", "C02", "C03", "C04", "C05", "C06", "C07", "C08", "C09", "C10", "C11", "C12",
                "W01", "W02", "W03", "W04", "W05", "W06", "W07", "W08", "W09", "W10", "W11", "W12",
                "X01", "X02", "X03", "X04", "X05", "X06", "X07", "X08", "X09", "X10", "X11", "X12",
                "Y01", "Y02", "Y03", "Y04", "Y05", "Y06", "Y07", "Y08", "Y09", "Y10", "Y11", "Y12",
                "Z01", "Z02", "Z03", "Z04", "Z05", "Z06", "Z07", "Z08", "Z09", "Z10", "Z11", "Z12"
            }
        };
        SerialGenerator.m_codeWords = tier switch
        {
            <= eRundownTier.TierB => FourLetterWords,
            >= eRundownTier.TierC => FiveLetterWords
        };
	}
    #endregion

    #region TerminalUplinkPuzzle
    /// <summary>
    /// Patches the terminal uplink puzzle to have a configurable number of
    /// code words per round. Currently, this is just set by tier, with harder
    /// tiers having more code words per round.
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(TerminalUplinkPuzzle), nameof(TerminalUplinkPuzzle.Setup))]
    [HarmonyPostfix]
    public static void TerminalUplinkPuzzle_Setup(TerminalUplinkPuzzle __instance)
    {
        var tier = RundownManager.GetActiveExpeditionData().tier;
        var codesCount = tier switch
        {
            <= eRundownTier.TierB => 6,
            eRundownTier.TierC => 8,
            eRundownTier.TierD => 12,
            eRundownTier.TierE => 15,
            _ => 6,
        };

        foreach (var round in __instance.m_rounds)
        {
            round.CorrectIndex = Builder.SessionSeedRandom.Range(0, codesCount);
            round.Prefixes = new string[codesCount];
            round.Codes = new string[codesCount];

            for (var i = 0; i < codesCount; i++)
            {
                round.Codes[i] = SerialGenerator.GetCodeWord();
                round.Prefixes[i] = SerialGenerator.GetCodeWordPrefix();
            }
        }
    }

    /// <summary>
    /// Reworks the display string both on screen and in logs to wrap for longer length codes
    /// </summary>
    /// <param name="__result"></param>
    /// <param name="round"></param>
    /// <param name="newLine"></param>
    /// <returns></returns>
    [HarmonyPatch(typeof(TerminalUplinkPuzzle), nameof(TerminalUplinkPuzzle.GetCodesString))]
    [HarmonyPrefix]
    public static bool TerminalUplinkPuzzle_GetCodesString(ref string __result, TerminalUplinkPuzzleRound round, bool newLine = false)
    {
        var tier = RundownManager.GetActiveExpeditionData().tier;
        var perLine = tier switch
        {
            eRundownTier.TierC => 4,
            eRundownTier.TierD => 4,
            eRundownTier.TierE => 5,
            _ => 3,
        };

        var text = "\n";
        for (var index = 0; index < round.Codes.Length; index++)
        {
            text += $"<color=orange>{round.Prefixes[index]}</color>:{round.Codes[index]}";

            if (index < round.Codes.Length - 1)
                text += (index + 1) % perLine != 0 ? " - " : "\n";
        }

        __result = text;
        return false;
    }
    #endregion

	// [HarmonyPatch(typeof(SerialGenerator), "Setup")]
	// [HarmonyPostfix]
 //    public static void IPV6Uplinks()
	// {
	// 	SerialGenerator.m_ips = new string[99];
	// 	for (int i = 0; i < ((Il2CppArrayBase<string>)(object)SerialGenerator.m_ips).Length; i++)
	// 	{
	// 		int num = 2001;
	// 		string text = ReturnRandomChar().ToString() + ReturnRandomChar() + Builder.SessionSeedRandom.Range(1, 9, "NO_TAG");
	// 		string text2 = Builder.SessionSeedRandom.Range(10, 99, "NO_TAG").ToString() + ReturnRandomChar() + Builder.SessionSeedRandom.Range(1, 9, "NO_TAG");
	// 		string text3 = Builder.SessionSeedRandom.Range(10, 99, "NO_TAG").ToString() + ReturnRandomChar() + ReturnRandomChar();
	// 		string text4 = ReturnRandomChar().ToString() + Builder.SessionSeedRandom.Range(10, 99, "NO_TAG");
	// 		((Il2CppArrayBase<string>)(object)SerialGenerator.m_ips)[i] = num + ":0:" + text + ":" + text2 + ":" + text3 + ":" + text4;
	// 	}
 //
 //        Plugin.Logger.LogError($"We got SerialGenerator.Setup()!");
	// }

	[HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter),  nameof(LG_ComputerTerminalCommandInterpreter.TerminalCorruptedUplinkConnect))]
	[HarmonyPrefix]
    public static bool NoUpperCase(LG_ComputerTerminalCommandInterpreter __instance, string param1, string param2)
	{
		if ((object)__instance.m_terminal.CorruptedUplinkReceiver == null)
		{
			Plugin.Logger.LogDebug("TerminalCorruptedUplinkConnect() critical failure because terminal does not have a CorruptedUplinkReceiver.");
			return false;
		}
		if (LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId != 0 && LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId != __instance.m_terminal.SyncID)
		{
			__instance.AddOngoingUplinkOutput();
			return false;
		}
		LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId = __instance.m_terminal.SyncID;
		// Plugin.Logger.LogDebug(Object.op_Implicit("TerminalCorruptedUplinkConnect, param1: " + param1 + " TerminalUplink: " + ((Object)__instance.m_terminal.UplinkPuzzle).ToString()));
		if (param1 == __instance.m_terminal.UplinkPuzzle.TerminalUplinkIP)
		{
			if (__instance.m_terminal.CorruptedUplinkReceiver.m_command.HasRegisteredCommand((TERM_Command)27))
			{
				__instance.AddUplinkCorruptedOutput();
			}
			else
			{
				__instance.AddUplinkCorruptedOutput();
				__instance.AddOutput("", true);
				__instance.AddOutput((TerminalLineType)4, "Sending connection request to " + __instance.m_terminal.CorruptedUplinkReceiver.PublicName, 3f, (TerminalSoundType)0, (TerminalSoundType)0);
				__instance.AddOutput((TerminalLineType)0, "Connection request sent. Waiting for confirmation.", 0.6f, (TerminalSoundType)0, (TerminalSoundType)0);
				__instance.AddOutput("", true);
				__instance.AddOutput((TerminalLineType)0, "Please <color=green>'CONFIRM'</color> connection on " + __instance.m_terminal.CorruptedUplinkReceiver.PublicName, 0.8f, (TerminalSoundType)0, (TerminalSoundType)0);
				__instance.m_terminal.CorruptedUplinkReceiver.m_command.AddCommand((TERM_Command)27, "CONFIRM", StringToLocalized("Confirm an established Uplink connection with this terminal and another."), (TERM_CommandRule)2);
				__instance.m_terminal.CorruptedUplinkReceiver.m_command.AddOutput((TerminalLineType)0, "Connection request from " + __instance.m_terminal.PublicName + ". Please type <color=green>CONFIRM</color> to continue.", 0f, (TerminalSoundType)0, (TerminalSoundType)0);
			}
		}
		else
		{
			__instance.AddUplinkWrongAddressError(param1);
		}
		return false;
	}

    public static char ReturnRandomChar()
	{
		return System.Convert.ToChar(Builder.SessionSeedRandom.Range(97, 122, "NO_TAG"));
	}
}
