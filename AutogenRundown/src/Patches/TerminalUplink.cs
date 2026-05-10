using HarmonyLib;
using LevelGeneration;
using Localization;

namespace AutogenRundown.Patches;

/// <summary>
/// Patches the Terminal Uplink code words to be longer, and for there to be
/// more code words for harder tiers
///
/// Ideas:
///   * Make CorruptedUplink log files with codes harder to type. Add some
///     random characters to the filename.
/// </summary>
[HarmonyPatch]
public static class TerminalUplink
{
    private static string[] Ipv6Prefixes => new[] { "2001:db8", "fc00", "fd00", "fe80" };

    // These are the base game words
    public static string[] FourLetterWords => new[]
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

    public static string[] FiveLetterWords => new[]
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

    /// <summary>
    /// About 2000 words
    /// </summary>
    public static string[] SixLetterWords => new[]
    {
        "upkeep", "sewage", "lucida", "undyed", "faster", "bidder", "therms", "rosary",
        "pecked", "cramps", "stared", "stints", "prissy", "curlew", "smirky", "matter",
        "virile", "ghetto", "shroud", "rapper", "quiver", "sprays", "imbued", "booboo",
        "laurie", "vizier", "wombat", "adjoin", "yawned", "poppet", "allure", "slappy",
        "carols", "peeves", "linage", "statue", "wading", "blazon", "ribbon", "cloudy",
        "botany", "sorrow", "review", "kindle", "dabble", "modulo", "detach", "mouthy",
        "miking", "zygote", "copped", "chasms", "bracer", "stools", "rhymer", "parrot",
        "append", "orbits", "bobcat", "timers", "bovine", "extend", "babies", "repeat",
        "twelve", "horror", "admits", "subset", "choose", "donate", "snippy", "zealot",
        "nearby", "wiener", "janice", "pearly", "sudden", "venial", "rebels", "blonde",
        "masham", "biller", "graven", "staple", "pleads", "spinal", "earing", "sharps",
        "fondly", "treads", "sieves", "iceman", "wiggly", "zephyr", "starts", "aboard",
        "clerks", "friend", "tricks", "syrupy", "nutmeg", "league", "opened", "vining",
        "wicked", "adults", "groovy", "melody", "fungal", "corker", "ginkgo", "rotors",
        "crayon", "pandan", "temper", "laurus", "okayed", "boosts", "inched", "devise",
        "aisles", "juggle", "lapels", "silage", "mutant", "gagged", "avocet", "flamed",
        "admire", "wowing", "bugged", "unites", "scorer", "livery", "gaging", "lumped",
        "basket", "snacks", "extant", "alpine", "adding", "chintz", "smidge", "bryony",
        "carafe", "rhesus", "burden", "adroit", "tomato", "school", "crusts", "coping",
        "seaway", "mantel", "agrees", "jacked", "apiece", "belter", "debugs", "lulled",
        "sedges", "highly", "parlay", "rubber", "valets", "hombre", "rueful", "modest",
        "swathe", "foiled", "stares", "jokers", "daikon", "unlord", "jurors", "obtain",
        "earths", "amidst", "chided", "sundew", "played", "rescan", "flicks", "lyceum",
        "niacin", "wagers", "induct", "rhumba", "tildes", "radios", "riffle", "dearer",
        "fogged", "tiptoe", "embeds", "rained", "shaper", "freely", "machos", "groves",
        "yeoman", "dances", "stifle", "pseudo", "zigzag", "native", "zeroes", "knocks",
        "sculls", "driver", "equine", "jingle", "zeroth", "dilate", "darner", "abbess",
        "curing", "agenda", "revolt", "clowns", "origin", "scarfs", "bluest", "chords",
        "heresy", "burley", "merest", "darned", "babine", "clipse", "rhymes", "metals",
        "pastas", "spells", "future", "nutria", "coined", "basses", "nipper", "acidic",
        "heaves", "unused", "paltry", "wipers", "stripy", "plague", "acorns", "busker",
        "caning", "quinte", "mutton", "nudges", "urgent", "barman", "impels", "faults",
        "yahoos", "galler", "toasty", "goaded", "whammy", "nomads", "septic", "drapes",
        "dimmer", "vortex", "ruling", "spoons", "bolded", "pulser", "shrink", "pipped",
        "midair", "esprit", "optics", "lifter", "causer", "blazer", "flimsy", "chucks",
        "liming", "peapod", "goober", "dement", "dosage", "noises", "redial", "flight",
        "stomps", "toiled", "fruits", "phonic", "kitten", "dunker", "reverb", "avowed",
        "roared", "rewire", "myopic", "firmer", "twined", "earwax", "blades", "spiffy",
        "caller", "editor", "tended", "polkas", "ambers", "grieve", "owners", "macros",
        "global", "flayed", "dozier", "maxims", "smacks", "redden", "hipped", "simply",
        "bombed", "hyphen", "bevels", "steers", "unhook", "bidens", "albums", "backup",
        "suited", "pained", "royals", "answer", "stowed", "recast", "daters", "misery",
        "abroad", "raider", "steamy", "copter", "luxury", "lapdog", "adduce", "fenced",
        "tapper", "scalps", "leaner", "pistol", "anthem", "brinks", "flocks", "italic",
        "banyan", "bayous", "gourds", "buffer", "racing", "blasts", "offend", "shoppy",
        "termed", "oddity", "avenge", "spined", "cookie", "scorch", "trains", "badges",
        "office", "pelvis", "effete", "bruise", "tempts", "envoys", "anemic", "liners",
        "relate", "husker", "mussel", "baobab", "cuboid", "binder", "sugars", "orgies",
        "schist", "obsess", "obtuse", "unless", "morrow", "recoup", "prefer", "prover",
        "anemia", "plaint", "riprap", "busses", "critic", "howled", "sanity", "member",
        "ripper", "guilds", "satyrs", "piddle", "rookie", "corals", "speers", "leaves",
        "smudge", "payees", "voodoo", "musher", "ranged", "duster", "woolly", "canner",
        "enjoys", "recede", "rudder", "looses", "enrage", "clefts", "queues", "fronts",
        "window", "hooves", "unveil", "laymen", "allium", "hymnal", "sowing", "chisel",
        "invite", "uncool", "sweety", "peaker", "osprey", "danced", "sucker", "scouse",
        "reiner", "tilted", "lapper", "throat", "trials", "knifed", "rabbit", "filmed",
        "moiety", "echoes", "wetted", "regime", "nipple", "hummus", "vertex", "puzzle",
        "wretch", "bouton", "butter", "boasts", "elicit", "socked", "garden", "whiney",
        "chokes", "cortex", "sloppy", "diadem", "swears", "tapped", "evenly", "subsea",
        "bucked", "towels", "sprang", "petted", "banger", "allows", "bended", "donkey",
        "ranked", "scrubs", "apices", "acacia", "cuddly", "height", "looser", "maimed",
        "conics", "undone", "dimers", "suckle", "encore", "gamete", "unital", "jovian",
        "fleece", "anneal", "mapper", "amused", "lasing", "devoid", "fences", "derive",
        "mocked", "boater", "nickle", "spicey", "oxides", "chased", "decoys", "untrue",
        "rector", "racoon", "landed", "erupts", "detest", "plural", "posses", "halite",
        "insole", "tumult", "rooter", "fescue", "grainy", "umlaut", "retake", "inward",
        "payors", "headed", "skater", "dollar", "swivel", "flexor", "talked", "parole",
        "nobler", "dumped", "sipper", "throne", "jaunty", "legume", "ladder", "biased",
        "violet", "biceps", "ducker", "whoops", "valves", "cancan", "formal", "albeit",
        "purged", "squawk", "dialup", "muchly", "gaming", "sagged", "ordain", "dosing",
        "ketone", "cougar", "dumber", "partly", "gather", "inodes", "gunman", "inning",
        "revere", "empire", "felted", "lawyer", "smarmy", "gobble", "dimmed", "grebes",
        "mirage", "causes", "filing", "skyway", "robbed", "mobbed", "insane", "digest",
        "hamper", "saddle", "assort", "soothe", "subtly", "plated", "hushed", "slings",
        "lasted", "baking", "remain", "axioms", "oldest", "poured", "setups", "tsetse",
        "infant", "gnomes", "smocks", "shaggy", "surety", "karats", "defeat", "peeler",
        "dictum", "basque", "grates", "plates", "dunked", "fondue", "heeded", "closed",
        "retest", "dinner", "femmes", "landau", "eluent", "detect", "floret", "usable",
        "scorns", "cruise", "petals", "belted", "maroon", "linens", "sought", "minted",
        "shales", "fixing", "nonuse", "honeys", "aghast", "dampen", "brushy", "shorty",
        "speeds", "zoomed", "rufous", "coupon", "preset", "duffel", "daemon", "scrape",
        "herons", "petite", "modern", "museum", "tissue", "remits", "groats", "sender",
        "dually", "savant", "lender", "unlock", "scores", "laptop", "awakes", "fluffy",
        "stunts", "jigger", "debate", "puffin", "covert", "gibbet", "shader", "hereof",
        "reseal", "herder", "looped", "jigsaw", "accede", "midrib", "stinky", "rotten",
        "pseudo", "conned", "animas", "broods", "lackey", "broker", "sailer", "neared",
        "rabble", "shanty", "storks", "gauges", "cohort", "lesion", "eerily", "kiosks",
        "propyl", "errata", "subpar", "spurns", "chaise", "jiggly", "unspun", "earwig",
        "demons", "zenith", "specks", "barrel", "purple", "novena", "relent", "eluted",
        "horned", "caplet", "reheat", "celebs", "zombie", "methyl", "detail", "feeler",
        "quarry", "struck", "unread", "hubris", "hopper", "whiten", "scours", "notify",
        "resign", "wiggle", "engulf", "yogurt", "discus", "galore", "abused", "steels",
        "ornate", "bagger", "exotic", "scraps", "bedlam", "hanger", "phased", "easter",
        "alumna", "pylons", "soaked", "routes", "wigeon", "scrips", "fleets", "proofs",
        "heddle", "waited", "meeker", "veered", "elects", "barbed", "blight", "tanner",
        "demure", "tanked", "napper", "avatar", "ursine", "locate", "midden", "lowest",
        "jaunts", "creaky", "renege", "emcees", "lipper", "capote", "favors", "baited",
        "premix", "impair", "rubens", "abided", "gallet", "shrubs", "varies", "photos",
        "hoopoe", "refuel", "fathom", "depose", "coddle", "dredge", "impugn", "zinger",
        "motels", "hoodie", "hubbub", "decree", "whence", "cheats", "barked", "unlink",
        "swords", "isobar", "abated", "wintry", "smarty", "basted", "common", "holler",
        "grotty", "tangle", "humbug", "jenson", "merely", "spices", "sidled", "reiter",
        "sinewy", "humbly", "blooms", "ampere", "gander", "coking", "granma", "palate",
        "update", "embark", "bougie", "policy", "addict", "worded", "wolves", "strewn",
        "siphon", "revive", "givers", "resent", "teases", "broads", "hotels", "minors",
        "dogmas", "vandal", "secede", "charms", "chocks", "warder", "freeze", "ravine",
        "turbid", "mounds", "sanely", "righty", "cartel", "tuneup", "pastel", "dodges",
        "beamer", "garage", "vistas", "vestry", "urging", "greets", "bobbed", "artful",
        "gadfly", "hippie", "expire", "crated", "buffed", "rodder", "shunts", "parlor",
        "grants", "fender", "scouts", "tickle", "shrews", "journo", "knowns", "rented",
        "propel", "frowns", "garnet", "escrow", "asylum", "minced", "reckon", "swamps",
        "secret", "frosty", "micron", "corset", "attend", "indole", "gobies", "paging",
        "glared", "alpaca", "befall", "pulley", "square", "spores", "seeker", "abrupt",
        "eschew", "revues", "bowman", "rushed", "grovel", "baring", "abuser", "robles",
        "loathe", "koalas", "beings", "occult", "astron", "banned", "boxers", "revise",
        "nailed", "golder", "tufted", "renown", "cloned", "online", "target", "surged",
        "breeze", "esteem", "tenner", "warmly", "smugly", "result", "adored", "anomie",
        "sneath", "rouser", "choirs", "fiends", "simmer", "furrow", "cooper", "satiny",
        "unseat", "cupids", "slalom", "snuffy", "malted", "hermit", "deltas", "bogeys",
        "petrel", "ardent", "possum", "gables", "ducats", "biggie", "redraw", "orders",
        "ranker", "essays", "darker", "pupils", "loaned", "strove", "enters", "surfer",
        "tablet", "gammon", "tricky", "huddle", "whinge", "aspect", "kvetch", "cholla",
        "calmer", "kitbag", "picker", "revved", "unroll", "dozers", "bailer", "darted",
        "licked", "doting", "parley", "shapes", "rising", "exiles", "versed", "luring",
        "fogger", "grammy", "eddies", "pastor", "pistil", "pinger", "lasses", "hunker",
        "kaftan", "busboy", "manure", "mouton", "pumper", "bowled", "feline", "veiled",
        "scopes", "raffia", "rashly", "futile", "prised", "altern", "heated", "wrongs",
        "deists", "gouges", "discos", "tomcat", "blocky", "sitter", "shines", "sparse",
        "calles", "sicker", "harbor", "rigger", "islets", "merino", "valued", "mishap",
        "imager", "slough", "rebind", "broths", "eights", "skills", "pundit", "rectal",
        "smiley", "desire", "sitcom", "lessee", "bisect", "hurdle", "mended", "choker",
        "mallee", "voters", "mimics", "infest", "thrush", "output", "batted", "tawdry",
        "suplex", "shiner", "caucus", "tokens", "lazily", "soiled", "cloths", "collab",
        "ferrum", "navels", "norths", "excels", "blames", "moneys", "tilter", "maniac",
        "remove", "batten", "ravers", "herero", "typify", "stitch", "roused", "shades",
        "grouts", "parens", "harass", "bidden", "parish", "fiddle", "gunmen", "homers",
        "depths", "father", "chiles", "kimchi", "hangup", "raffle", "region", "haggis",
        "loader", "sundry", "strode", "scrawl", "sevens", "backer", "mounts", "evades",
        "crypto", "corked", "missal", "openly", "eighty", "baying", "exhale", "shoppe",
        "sprite", "hinted", "outrun", "racked", "behold", "lingua", "abbots", "odious",
        "expiry", "lowing", "exodus", "confit", "sprung", "mystic", "caveat", "vernal",
        "outers", "netted", "sherry", "fizzle", "zinnia", "eyelid", "shamed", "globes",
        "juntas", "swirls", "battle", "pillow", "whaler", "sunder", "format", "spying",
        "loaded", "hitter", "aspire", "safari", "mallow", "proven", "crowns", "coffee",
        "mangle", "massif", "alcove", "amoeba", "phones", "mayors", "remand", "looney",
        "troupe", "sallee", "curved", "agates", "custom", "feeble", "coning", "biomes",
        "keeper", "effort", "swales", "helped", "roader", "toffee", "graphs", "spaced",
        "coldly", "kitted", "candor", "moaned", "relied", "straws", "memory", "bypass",
        "medley", "spring", "winnow", "anodes", "capita", "singly", "wigwam", "parkas",
        "francs", "pillar", "blenny", "cozens", "marked", "plaids", "gassed", "scroll",
        "cental", "bosons", "budget", "throbs", "tennis", "portal", "playas", "lusted",
        "quinta", "crawly", "masons", "fixity", "oddest", "sorbed", "assure", "fixate",
        "menace", "bamboo", "liable", "banged", "charta", "forced", "flaked", "corner",
        "blamed", "louver", "tattle", "labile", "hearts", "parsed", "violin", "parent",
        "cyclic", "halted", "reined", "toners", "lucian", "bright", "shafts", "malaya",
        "audits", "cutest", "vireos", "scored", "refute", "faulty", "loving", "deepen",
        "napped", "forego", "demote", "tonics", "tuxedo", "carman", "dreamy", "cabins",
        "gummed", "rigged", "riding", "vowing", "parses", "floaty", "sweaty", "radian",
        "postal", "riches", "arcane", "turnip", "double", "otters", "driers", "hosing",
        "flinch", "inform", "corded", "single", "rioted", "coffer", "debits", "lurked",
        "outdid", "medals", "teensy", "sundog", "flaxen", "titers", "rammed", "mythos",
        "tented", "inputs", "polled", "ulcers", "sarong", "layman", "climes", "versus",
        "entail", "utopia", "verily", "unsung", "career", "tamper", "arcana", "limped",
        "rabies", "reefer", "swifty", "preyed", "puppet", "limpid", "inking", "beaver",
        "buffet", "sector", "wormed", "refers", "poplar", "kindly", "adorns", "enlace",
        "canons", "birder", "yippee", "dimple", "method", "nannie", "greyed", "killed",
        "hiatus", "funder", "oxygen", "atrial", "souped", "esters", "bionic", "sketch",
        "kittle", "otello", "adduct", "pagans", "indigo", "souled", "former", "cypher",
        "flings", "craves", "hominy", "mortar", "tiptop", "trader", "urbane", "stupid",
        "angles", "linker", "giving", "kicked", "ramjet", "specie", "tundra", "states",
        "opcode", "corral", "precis", "honest", "villas", "turbos", "paints", "roughs",
        "gambol", "lament", "pander", "sachem", "chichi", "begone", "frosts", "agency",
        "lodges", "tracks", "kismet", "entree", "zodiac", "depict", "syndic", "draper",
        "cateye", "reaper", "jabbed", "buyout", "stoked", "parody", "dyadic", "tacker",
        "oblate", "plexus", "woolen", "comber", "stater", "nagged", "donned"
    };

    /// <summary>
    /// About 1,570 words
    /// </summary>
    public static string[] SevenLetterWords => new[]
    {
        "abating", "abiding", "abolish", "abyssal", "acacias", "academy", "accents",
        "account", "acquire", "acreage", "actions", "acutely", "address", "adjunct",
        "adjusts", "adviser", "affixed", "affront", "against", "aground", "aircrew",
        "airflow", "airmail", "allelic", "allured", "almanac", "almonds", "alpacas",
        "already", "amateur", "ambling", "amended", "ammonia", "amorous", "amyloid",
        "anarchy", "anguish", "annexes", "annulus", "another", "answers", "antacid",
        "antenna", "anthems", "antonym", "anymore", "apostle", "apparel", "applies",
        "aqueous", "archery", "archive", "armband", "armorer", "arouses", "arrival",
        "arsenic", "article", "artiest", "artless", "arugula", "asylums", "attract",
        "aunties", "auroras", "austere", "authors", "autocar", "avenger", "avenues",
        "average", "awakens", "awaking", "babbled", "babysit", "backing", "backlog",
        "badgers", "baggage", "ballads", "ballots", "baneful", "banging", "bangles",
        "banking", "banquet", "baptize", "barking", "baronet", "baroque", "barring",
        "bashful", "basques", "bathtub", "battles", "baubles", "beading", "bearish",
        "beavers", "because", "bedrock", "beepers", "beeping", "befalls", "belcher",
        "beliefs", "believe", "belongs", "belting", "bending", "benefit", "berated",
        "betrays", "betting", "bettors", "between", "bicolor", "bicycle", "bifocal",
        "biggies", "binning", "biology", "biotics", "birding", "bishops", "bitumen",
        "blacked", "blacken", "blaming", "blankly", "bleeder", "blessed", "blinded",
        "blister", "blitzed", "blocker", "bloomed", "blooper", "blouses", "blowers",
        "blunted", "blurred", "blurted", "blusher", "boarded", "boatmen", "boffins",
        "boggles", "bolding", "bollard", "bombard", "bonuses", "bookend", "bookies",
        "booklet", "boolean", "boomers", "booster", "boredom", "borings", "borough",
        "bossier", "bothers", "bottles", "bouncer", "bounces", "bowhead", "boycott",
        "bracing", "brahmin", "brasher", "braving", "breaded", "breathy", "breezes",
        "bribery", "bridals", "briefed", "brights", "brining", "briskly", "bristle",
        "bristly", "broadly", "bromide", "bronzes", "brother", "brownie", "browsed",
        "bruises", "brutish", "buddies", "budgets", "budgies", "buffers", "buffets",
        "bugbear", "bulbous", "bulging", "bulking", "bulldog", "bullies", "bullish",
        "bullpen", "bumpers", "bunched", "bungled", "bunting", "buoyant", "bureaus",
        "burgers", "burrito", "butters", "buttery", "buyouts", "bylined", "bylines",
        "cabanas", "cabinet", "caching", "cadmium", "calcium", "cantons", "capping",
        "caprice", "captain", "captors", "carafes", "caramel", "caravan", "carcass",
        "cargoes", "carpets", "carrion", "carrots", "cartoon", "castors", "catarrh",
        "catcher", "catches", "catered", "causing", "caveman", "ceasing", "cements",
        "censors", "censure", "centers", "central", "ceviche", "chafing", "chalice",
        "challah", "changes", "channel", "chanted", "chaotic", "chapped", "charger",
        "charted", "chasers", "chasing", "checker", "cherubs", "chevron", "chicory",
        "chiming", "choline", "chopped", "chorale", "chorizo", "chucked", "clapped",
        "clashed", "classic", "cleanse", "clicked", "climbed", "clinger", "clipped",
        "clipper", "clogged", "closely", "closeup", "clotted", "clovers", "clubbed",
        "coaches", "coasted", "coerced", "cognate", "collars", "college", "combust",
        "comedic", "comment", "commies", "commune", "company", "compare", "compels",
        "compere", "comport", "compose", "conceal", "concert", "concise", "condemn",
        "condors", "conjure", "console", "consume", "contact", "contend", "content",
        "contest", "control", "convert", "cookout", "coolest", "coopers", "cooties",
        "coppers", "copycat", "cornets", "coronet", "corsage", "cougars", "coulomb",
        "council", "country", "courage", "courant", "coursed", "courses", "covered",
        "cowbird", "coyotes", "cracked", "crackle", "crashed", "cravers", "creaked",
        "credits", "crimper", "crimson", "cronies", "cropper", "crowing", "crucify",
        "crudely", "cruelly", "crusher", "crushes", "cubbies", "cubicle", "culture",
        "culvert", "cumulus", "cunning", "currant", "current", "curried", "cushion",
        "cuticle", "cutlery", "cutover", "cyclops", "cypress", "dabbled", "damping",
        "damsels", "dancing", "dawning", "daytime", "debater", "debtors", "decease",
        "decided", "decimal", "deckers", "decoder", "decodes", "decreed", "deduced",
        "deeming", "defiant", "defraud", "defunct", "defused", "deicing", "deliver",
        "deltoid", "demands", "demonic", "demoted", "deniers", "densest", "depicts",
        "deposed", "depress", "dequeue", "derails", "derived", "derives", "designs",
        "desired", "desktop", "details", "detects", "detente", "devises", "devours",
        "diabolo", "dialing", "dictate", "diesels", "digests", "diggers", "digging",
        "digital", "dilated", "dilator", "dinette", "diorama", "dirtier", "disable",
        "disease", "dislike", "display", "dispute", "diurnal", "divider", "divides",
        "divisor", "doctors", "dodgers", "doilies", "dollars", "dominos", "donning",
        "dossier", "drafted", "dragoon", "dreaded", "drifter", "drinker", "drizzly",
        "droning", "dubious", "dumbing", "durable", "dusting", "dyeable", "eagerly",
        "earlier", "earlobe", "earners", "earshot", "echelon", "echoing", "edibles",
        "edifice", "edition", "ejected", "elected", "elector", "embassy", "embolic",
        "eminent", "emirate", "empower", "emptied", "emptive", "emulate", "encased",
        "encores", "endgame", "endings", "endowed", "endures", "enjoins", "enjoyed",
        "enlaces", "enraged", "enteric", "envious", "enzymes", "erosive", "erratum",
        "estates", "evacuee", "evaders", "evasion", "evoking", "evolved", "examine",
        "example", "excises", "exciter", "excites", "exempts", "existed", "explain",
        "exploit", "exports", "expunge", "faculty", "zeroing", "faintly", "fairies",
        "falcons", "fangled", "farmers", "farming", "farther", "fascias", "fastest",
        "fatally", "fateful", "fathers", "favored", "fawning", "federal", "feeders",
        "feelers", "females", "fetched", "fielder", "fifteen", "figures", "filling",
        "finance", "fishery", "fission", "fixedly", "flaming", "flatbed", "flecked",
        "flicked", "floated", "floored", "flopped", "flowery", "fluidic", "flutter",
        "foaming", "focuser", "folders", "fondled", "fooling", "footers", "footway",
        "forages", "forceps", "foresaw", "forests", "forgave", "forging", "forgive",
        "forsake", "framers", "fraying", "freeing", "freezer", "frescos", "freshly",
        "friends", "frigate", "fringes", "fuchsia", "fuelled", "fulcrum", "fumbles",
        "funnies", "further", "futures", "gallery", "galling", "gallops", "gambler",
        "gangway", "gapping", "garners", "garters", "gasping", "gazelle", "gazette",
        "gelatin", "general", "genetic", "genital", "gestalt", "gigabit", "giraffe",
        "glacial", "glamour", "glanced", "glassed", "glazing", "gleeful", "glimmer",
        "glossed", "glutton", "goalies", "gobbler", "godhead", "godward", "goofing",
        "gosling", "goulash", "gracing", "grained", "granary", "grander", "grannie",
        "granola", "granted", "grantee", "granule", "grassed", "gravest", "graying",
        "greases", "greater", "greener", "gremlin", "grenade", "griddle", "gripper",
        "grizzle", "grooved", "grosses", "grouchy", "grouped", "growths", "grudges",
        "grumble", "guessed", "gumtree", "guzzles", "gymnast", "habitat", "hacksaw",
        "halcyon", "hallows", "hallway", "halting", "hammers", "hamster", "handing",
        "handoff", "hankies", "hapless", "haploid", "harmony", "harping", "harpoon",
        "hastens", "hatches", "haulage", "hawkish", "hayseed", "hazards", "healers",
        "heaping", "heaters", "heavier", "heights", "helmets", "hickory", "hipster",
        "history", "hitched", "hobnail", "hoedown", "hoisted", "hollers", "hollows",
        "hooting", "hopeful", "hoppers", "hopping", "hormone", "horrors", "hospice",
        "hostels", "hostile", "hosting", "hotdogs", "hotline", "however", "hulking",
        "humanly", "humbled", "humerus", "humidor", "humming", "hunters", "huskers",
        "huskies", "idyllic", "ignited", "impaler", "imperil", "impiety", "impious",
        "implies", "imposed", "impound", "imprint", "improve", "imputed", "include",
        "indents", "indexed", "indexer", "indexes", "indicts", "indulge", "infects",
        "inflict", "infuser", "ingress", "injects", "injured", "innings", "inquest",
        "inroads", "insider", "insides", "insight", "insists", "inspect", "instate",
        "insured", "insures", "invents", "inverse", "ionized", "ionizer", "islands",
        "isomers", "isotope", "issuers", "jackals", "janitor", "january", "jarring",
        "jazzman", "jeepers", "jellies", "jerking", "jigsaws", "jobbers", "jobsite",
        "joiners", "joining", "jolting", "journal", "judging", "jungles", "karaoke",
        "kernels", "ketchup", "ketones", "ketosis", "keylock", "keypads", "kicking",
        "kimonos", "kindred", "kinetic", "kissing", "kitting", "kneeled", "knowhow",
        "labored", "lacking", "lagging", "lagoons", "lancets", "largely", "latches",
        "lateral", "launder", "lawless", "layaway", "leaders", "leaflet", "leaking",
        "lecture", "leeward", "legumes", "leisure", "lemming", "leopard", "lessees",
        "liberty", "library", "licking", "lifting", "likable", "lilting", "limited",
        "limiter", "lintels", "listens", "listing", "lithium", "liturgy", "livable",
        "lobbies", "located", "locater", "locates", "lockets", "locknut", "lodging",
        "logbook", "looking", "lookout", "lookups", "looming", "lotions", "louvers",
        "loyalty", "lurkers", "machine", "magical", "magnify", "mahjong", "mailbox",
        "maiming", "majored", "malting", "manatee", "mangers", "mangoes", "mankind",
        "mansion", "mariner", "markets", "marries", "marshal", "martial", "martyrs",
        "massing", "mastiff", "mastoid", "matinee", "maudlin", "mauling", "maxilla",
        "meaning", "medical", "mediums", "meeting", "melanin", "members", "mermaid",
        "message", "meteors", "microbe", "middles", "midline", "midterm", "million",
        "millman", "mimosas", "mincing", "minding", "mingled", "mingles", "minimum",
        "minnows", "minuses", "minutes", "miserly", "mislead", "missile", "mistake",
        "modeled", "modular", "moments", "monarch", "montage", "monthly", "moorhen",
        "moorish", "mosaics", "mothers", "mountie", "mulling", "mullion", "mumbles",
        "mummies", "musings", "muskets", "muskrat", "mutable", "mutants", "mutates",
        "mutters", "mystics", "nagging", "napkins", "natural", "nebulae", "nesting",
        "netball", "netting", "network", "nibbles", "nightly", "nipping", "noisily",
        "nonbank", "noncash", "nonstop", "notable", "notably", "notched", "notches",
        "nothing", "noticed", "novelty", "nozzles", "numbers", "numeric", "nurture",
        "nutcase", "oakmoss", "obviate", "ocarina", "octaves", "october", "offends",
        "offeror", "offside", "ominous", "omnibus", "onboard", "oneself", "onshore",
        "openers", "opossum", "optical", "options", "oracles", "oranges", "orbital",
        "ordeals", "outcome", "outfits", "outlets", "outlier", "outrage", "overdue",
        "overrun", "oversaw", "oxfords", "oysters", "pageant", "paisley", "pampers",
        "panacea", "pancake", "paneled", "panning", "pansies", "papayas", "parable",
        "paraded", "parapet", "parfait", "parlors", "paroled", "parsley", "parsons",
        "parties", "passage", "passing", "passive", "pastors", "patella", "patrons",
        "pattern", "pausing", "payable", "payment", "payroll", "pecking", "penalty",
        "pending", "peonies", "percept", "perches", "perfume", "perfumy", "pickets",
        "picture", "piercer", "pigtail", "pincher", "pinkish", "pinless", "pirated",
        "pitched", "pitfall", "pivoted", "plainer", "plainly", "planets", "planted",
        "planter", "plateau", "platoon", "pleases", "pliable", "plotter", "plunges",
        "plywood", "poetess", "pointed", "pointer", "pontiff", "pontoon", "pooling",
        "poppies", "popular", "porcine", "possums", "postbag", "postfix", "posting",
        "postman", "postwar", "potions", "powdery", "powered", "preachy", "preamps",
        "precise", "pregame", "preheat", "premise", "prepare", "present", "preteen",
        "preying", "primacy", "priming", "prisons", "privacy", "private", "problem",
        "process", "proctor", "prodded", "produce", "product", "profess", "profile",
        "profits", "program", "project", "prolong", "provide", "proving", "proxied",
        "proxies", "prudent", "psyched", "publish", "puffing", "pulsars", "punched",
        "punches", "pungent", "puritan", "pursuer", "pushing", "pushrod", "putters",
        "puzzled", "pygmies", "pyramid", "quality", "quantal", "quarrel", "quickly",
        "quintet", "quizzes", "radiate", "raiders", "raiding", "railway", "raining",
        "rallied", "ramadan", "rambled", "rancher", "ranchos", "rangers", "rapidly",
        "rappers", "rascals", "rattled", "ravioli", "rawhide", "reached", "reacher",
        "readout", "rebuked", "recalls", "recipes", "reckons", "recline", "records",
        "rectors", "rectory", "recurse", "recusal", "recused", "redbird", "reddish",
        "redfish", "reeling", "reentry", "referee", "reflect", "refocus", "reforms",
        "refuted", "regaled", "regency", "regimen", "regress", "regrets", "regroup",
        "reissue", "rejoice", "relapse", "related", "relaxed", "release", "reliant",
        "remakes", "remarks", "remarry", "remixed", "remnant", "remodel", "removes",
        "renames", "rentals", "renters", "reorder", "repeals", "repents", "replays",
        "replete", "reports", "reproof", "repulse", "request", "rescale", "resists",
        "resound", "restate", "results", "retails", "reticle", "retract", "retread",
        "returns", "reunion", "reveals", "reveled", "revered", "reverse", "reviews",
        "reviser", "revisor", "revived", "rewinds", "rewrote", "ribbons", "ricotta",
        "ridding", "riddled", "rioting", "ripcord", "ripoffs", "riposte", "rippers",
        "riveted", "riveter", "roaring", "roasted", "roebuck", "rollout", "romaine",
        "rompers", "roofing", "rooster", "rotator", "rounded", "rounder", "rousing",
        "royalty", "ruffles", "rumbled", "rumored", "rundown", "runtime", "rustled",
        "saguaro", "saintly", "salmons", "salting", "salvage", "samhain", "sampled",
        "sandman", "saucers", "savages", "savings", "savored", "sayings", "scalper",
        "scamper", "scanner", "scarcer", "scarier", "scarred", "scatter", "schools",
        "science", "scissor", "scooped", "scooter", "scourge", "scouted", "scraped",
        "scrapes", "scratch", "scribes", "scrubby", "sealine", "seasons", "seaters",
        "seconds", "secrete", "section", "seeders", "seethed", "seismic", "selfish",
        "sellout", "servant", "service", "settles", "several", "shabbat", "shadowy",
        "shakeup", "shapely", "sharing", "sheared", "sheaths", "sheeted", "shellac",
        "shifted", "shifter", "shingle", "shipper", "shocker", "shoeing", "shovels",
        "showers", "shrieks", "shrinks", "shudder", "shuffle", "sickens", "sidecar",
        "sigmoid", "signers", "silents", "similar", "similes", "sirloin", "sizzler",
        "skelter", "skewing", "skidded", "skillet", "skimmed", "skipper", "skylark",
        "slander", "slapped", "sleeper", "slicker", "slivers", "sloping", "smacked",
        "smarter", "smiling", "smitten", "smudges", "smuggle", "sneaked", "sneezed",
        "snicker", "snorkel", "snuffed", "soaking", "soberly", "society", "softest",
        "softies", "sonnets", "soothed", "sorcery", "sorrows", "sorters", "sorting",
        "sources", "spatial", "spawner", "spaying", "special", "species", "specify",
        "specter", "spiders", "spiller", "spindle", "spinner", "splints", "spotter",
        "squalls", "squeals", "squishy", "stabler", "stainer", "stamped", "stapled",
        "starker", "statute", "stereos", "sterner", "stipend", "stopgap", "stopped",
        "stopper", "stories", "storing", "streets", "strolls", "stubble", "student",
        "studies", "stuffed", "stunned", "subdued", "subject", "sublime", "subsume",
        "subtype", "suggest", "summery", "summons", "sunrise", "sunsets", "sunspot",
        "suppers", "support", "surface", "surfers", "surgeon", "surgery", "surreal",
        "swapper", "swarmed", "swarthy", "sweeper", "swifter", "swisher", "systems",
        "tablets", "tabstop", "tacitly", "tacking", "tactics", "tailers", "tailors",
        "talking", "tamping", "tangent", "tangles", "tanners", "tannery", "tannins",
        "tantrum", "tapioca", "tassels", "tatting", "tattoos", "taxiing", "teaches",
        "teacups", "teasers", "tedious", "tempted", "tempter", "tenable", "tenders",
        "tending", "tenuate", "tenures", "termini", "testbed", "testers", "tetanus",
        "textual", "theater", "thereon", "thicken", "thimble", "thirdly", "thirsty",
        "thither", "threads", "through", "thumbed", "thumper", "tickets", "tickled",
        "tidings", "tidying", "tilings", "tinting", "tippers", "tissues", "tithing",
        "toaster", "toggled", "toggles", "toiling", "tonight", "tooting", "topiary",
        "topside", "torrent", "tossing", "touched", "touches", "touting", "towards",
        "toyland", "traffic", "tragedy", "trapped", "treason", "treated", "trefoil",
        "tresses", "tricked", "trigram", "trimmed", "triplet", "tripods", "tritone",
        "trolled", "troller", "trotted", "trustee", "tubular", "tumbles", "tutored",
        "twaddle", "tweedle", "tweeter", "twiddle", "twining", "twinkle", "twisted",
        "ugliest", "unarmed", "unasked", "unblock", "uncivil", "unfired", "unicorn",
        "unifies", "unknown", "unlearn", "unspent", "untruth", "untying", "untyped",
        "unwired", "updated", "upholds", "uploads", "upwards", "utilize", "utopian",
        "vacancy", "vaccine", "vagrant", "valiant", "vassals", "vectors", "velvety",
        "vendors", "verbose", "verdant", "version", "vertigo", "vestige", "vetting",
        "vibrant", "violent", "virtues", "viruses", "viscous", "visions", "visuals",
        "voicing", "wafting", "wallaby", "walling", "wardens", "warming", "warning",
        "warring", "warrior", "warthog", "wettest", "whaling", "wheelie", "whereby",
        "wherein", "whether", "whitish", "whizzes", "widower", "winches", "windows",
        "windrow", "winging", "winking", "winless", "winsome", "wiretap", "wisdoms",
        "wishful", "wishing", "without", "wizards", "wobbler", "working", "workmen",
        "wrapper", "wreaths", "wriggle", "written", "xylitol"
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
	[HarmonyPatch(typeof(SerialGenerator), nameof(SerialGenerator.Setup))]
	[HarmonyPrefix]
    public static void SerialGenerator_Setup()
	{
        Plugin.Logger.LogWarning($"Expedition tier is: ${RundownManager.GetActiveExpeditionData().tier}");

        var tier = RundownManager.GetActiveExpeditionData().tier;

        SerialGenerator.m_codeWordPrefixes = tier switch
        {
            // Vanilla prefixes
            <= eRundownTier.TierB => new[]
            {
                "X01", "X02", "X03", "X04", "X05", "X06", "X07", "X08", "X09",
                "Y01", "Y02", "Y03", "Y04", "Y05", "Y06", "Y07", "Y08", "Y09",
                "Z01", "Z02", "Z03", "Z04", "Z05", "Z06", "Z07", "Z08", "Z09"
            },
            >= eRundownTier.TierC => new[]
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
            eRundownTier.TierA => FourLetterWords,
            eRundownTier.TierB => FourLetterWords,
            eRundownTier.TierC => FiveLetterWords,
            eRundownTier.TierD => SixLetterWords,
            eRundownTier.TierE => SevenLetterWords,

            _ => FourLetterWords,
        };
	}

    /// <summary>
    /// Replaces the serial generator IPv4 address pool with "realistic"
    /// looking IPv6 addresses. This only occurs on D-tier and below.
    /// </summary>
    [HarmonyPatch(typeof(SerialGenerator), nameof(SerialGenerator.Setup))]
    [HarmonyPostfix]
    public static void UplinkAddresses()
    {
        var tier = RundownManager.GetActiveExpeditionData().tier;

        // Keep IPv4 addresses for A/B/C tier levels
        if (tier < eRundownTier.TierD)
            return;

        SerialGenerator.m_ips = new string[99];

        for (var ip = 0; ip < SerialGenerator.m_ips.Length; ip++)
        {
            var prefix = Ipv6Prefixes[Builder.SessionSeedRandom.Range(0, Ipv6Prefixes.Length)];
            var segments = prefix.Split(':').Select(part => part.ToLowerInvariant()).ToList();

            while (segments.Count < 8)
            {
                // Add zero option
                if (segments.Count is >= 3 and <= 5)
                {
                    segments.Add("0");
                    continue;
                }

                var (min, max) = (segments.Count, Builder.SessionSeedRandom.Range(0, 10)) switch
                {
                    // Always full for the last 2
                    (>= 6, _) => (0x01000, 0x10000),

                    (_, <= 1) => (      0, 0x00000),
                    (_, <= 3) => (      0, 0x00100),
                    (_, <= 5) => (      0, 0x00200),
                    (_, <= 7) => (      0, 0x01000),
                    (_,    _) => (      0, 0x10000),
                };

                segments.Add(max == 0 ? "0" : Builder.SessionSeedRandom.Range(min, max).ToString("X"));
            }

            // Identify the longest zero run for compression
            var zeroRanges = new List<(int start, int length)>();
            var start = -1;
            for (var i = 0; i <= segments.Count; i++)
            {
                if (i < segments.Count && segments[i] == "0")
                {
                    if (start == -1)
                        start = i;
                }
                else
                {
                    if (start != -1)
                    {
                        zeroRanges.Add((start, i - start));
                        start = -1;
                    }
                }
            }

            var bestZeroRun = zeroRanges.OrderByDescending(r => r.length).FirstOrDefault();
            if (bestZeroRun.length < 2) bestZeroRun = default; // compress only if at least 2

            // Rebuild with compression
            var parts = new List<string>();
            for (var i = 0; i < segments.Count;)
            {
                if (bestZeroRun.length >= 2 && i == bestZeroRun.start)
                {
                    parts.Add(""); // :: compression
                    i += bestZeroRun.length;
                    continue;
                }
                parts.Add(segments[i]);
                i++;
            }

            // Force all these as lower case. It seems the string checking
            // happens as lower case even though the display on the terminal
            // is all upper case. Forcing this to upper case makes users unable
            // to input the IP address.
            var address = parts.Join(part => part.ToLowerInvariant(), ":")
                                     .Replace(":::", "::");

            SerialGenerator.m_ips[ip] = address;
        }

        Plugin.Logger.LogInfo($"Replaced SerialGenerator m_ips with IPv6 addresses");
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
        // Guards against rerolls with uninitialized terminals
        if (__instance == null || __instance.m_terminal == null)
            return;

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
    ///
    /// For log files, the text will be automaticall wrapped at 43 chars.
    /// So make sure we wrap the logs appropriately to fit how we want
    /// for the terminal. Hence why we have separate logic for when it's
    /// on a log file. Below is the divider surrounding the codes:
    ///     -------------------------------------------
    ///     A01:four  A01:four  A01:four  A01:four
    ///     A02:fives  A02:fives  A02:fives  A02:fives
    ///     A03:sixsix  A03:sixsix  A03:sixsix
    ///     A04:sevenis  A04:sevenis  A04:sevenis
    ///
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
        var perLineDisplay = tier switch
        {
            eRundownTier.TierC => 4,
            eRundownTier.TierD => 4,
            eRundownTier.TierE => 5,
            _ => 3,
        };
        var perLineLogFile = tier switch
        {
            eRundownTier.TierA => 4,
            eRundownTier.TierB => 4,
            eRundownTier.TierC => 4,
            _ => 3,
        };
        var isLogfile = newLine;

        var text = !isLogfile ? "\n" : "";
        for (var index = 0; index < round.Codes.Length; index++)
        {
            text += $"<color=orange>{round.Prefixes[index]}</color>:{round.Codes[index]}";

            if (index >= round.Codes.Length - 1)
                continue;

            if (isLogfile)
                text += (index + 1) % perLineLogFile != 0 ? "  " : "\n";
            else
                text += (index + 1) % perLineDisplay != 0 ? " - " : "\n";
        }

        __result = text;
        return false;
    }

    // [HarmonyPatch(typeof(TerminalUplinkPuzzle), nameof(TerminalUplinkPuzzle.SendLogsToCorruptedUplinkReceiver))]
    // [HarmonyPrefix]
    // public static bool TerminalUplinkPuzzle_SendLogsToCorruptedUplinkReceiver(TerminalUplinkPuzzle __instance, int roundIndex)
    // {
    //     if (__instance.m_terminal.CorruptedUplinkReceiver != (UnityEngine.Object)null)
    //     {
    //         string codesString = TerminalUplinkPuzzle.GetCodesString(__instance.m_rounds[roundIndex], true);
    //         __instance.m_terminal.CorruptedUplinkReceiver.AddLocalLog(new TerminalLogFileData()
    //         {
    //             FileName = "UplinkCodes_" + (roundIndex + 1).ToString("D2") + ".LOG",
    //             FileContent = new LocalizedText
    //             {
    //                 Id = 0u,
    //                 UntranslatedText = "-------------------------------------------\n" +
    //                                    codesString +
    //                                    "\n-------------------------------------------"
    //             }
    //         });
    //         __instance.m_terminal.AddLine(TerminalLineType.ProgressWait, "SENDNING UPLINK VERIFICATION CODES TO " + __instance.m_terminal.CorruptedUplinkReceiver.PublicName.ToUpper() + " AS LOG FILES", 2f);
    //         __instance.m_terminal.CorruptedUplinkReceiver.AddLine(TerminalLineType.ProgressWait, "RECEIVING UPLINK VERIFICATION CODES FROM " + __instance.m_terminal.PublicName.ToUpper() + " AS LOG FILES", 2f);
    //     }
    //     else
    //         Plugin.Logger.LogError($"{__instance.m_terminal.PublicName} tried to send log file to its CorruptedUplinkReceiver terminal, but it was null!");
    //
    //     return false;
    // }
    #endregion

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

		if (param1 == __instance.m_terminal.UplinkPuzzle.TerminalUplinkIP)
		{
			if (__instance.m_terminal.CorruptedUplinkReceiver.m_command.HasRegisteredCommand((TERM_Command)27))
				__instance.AddUplinkCorruptedOutput();
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
			__instance.AddUplinkWrongAddressError(param1);

		return false;
	}
}
