using HarmonyLib;
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
    private static string[] Ipv6Prefixes => new[] { "2001:db8", "fc00", "fd00", "fe80" };

    // These are the base game words
    private static string[] FourLetterWords => new[]
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

    private static string[] FiveLetterWords => new[]
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

    // About 2000 words
    private static string[] SixLetterWords => new[]
    {
        "bourns", "upkeep", "sewage", "lucida", "undyed", "faster", "bidder", "salada",
        "therms", "rosary", "pecked", "cramps", "terran", "stared", "tammie", "linsey",
        "cheung", "stints", "rajesh", "prissy", "curlew", "achill", "medica", "smirky",
        "matter", "virile", "ghetto", "lysate", "chiasm", "shroud", "audion", "antica",
        "quaker", "rapper", "buckie", "kidder", "quiver", "sprays", "imbued", "moline",
        "booboo", "laurie", "vizier", "wombat", "lemuel", "adjoin", "caspar", "culver",
        "yawned", "poppet", "ruffin", "allure", "slappy", "busine", "keloid", "carols",
        "peeves", "linage", "statue", "wading", "blazon", "ribbon", "cloudy", "botany",
        "apnoea", "sorrow", "review", "kindle", "dabble", "garvey", "modulo", "detach",
        "thymic", "mouthy", "miking", "zygote", "copped", "amarth", "chasms", "bracer",
        "anubis", "papist", "stools", "talbot", "rhymer", "parrot", "lipase", "append",
        "orbits", "stanno", "rupiah", "bobcat", "timers", "bovine", "extend", "babies",
        "repeat", "twelve", "horror", "admits", "subset", "elaine", "choose", "mulder",
        "kronur", "donate", "bertin", "smiths", "snippy", "zealot", "nearby", "wiener",
        "pieter", "lowrie", "janice", "zydeco", "pearly", "sudden", "venial", "rebels",
        "blonde", "masham", "biller", "contex", "graven", "staple", "pleads", "spinal",
        "earing", "sharps", "titian", "plough", "bedell", "sadist", "fondly", "treads",
        "sieves", "judith", "sander", "iceman", "wiggly", "zephyr", "starts", "aboard",
        "clerks", "friend", "gudrun", "tricks", "shirky", "syrupy", "nutmeg", "bonita",
        "league", "opened", "vining", "wicked", "harlot", "adults", "groovy", "pinder",
        "melody", "fungal", "corker", "tupelo", "ginkgo", "rotors", "crayon", "pandan",
        "yukata", "temper", "somers", "untill", "rogero", "laurus", "lignes", "okayed",
        "boosts", "inched", "cesium", "tigris", "devise", "aisles", "juggle", "lapels",
        "silage", "dotlet", "giulio", "mutant", "gagged", "avocet", "flamed", "admire",
        "wowing", "bugged", "aliyah", "humpty", "bushie", "gerald", "unites", "morgan",
        "scorer", "livery", "capias", "gaging", "lumped", "basket", "snacks", "extant",
        "alpine", "adding", "chintz", "billon", "smidge", "bryony", "carafe", "rhesus",
        "iseult", "tatler", "burden", "adroit", "sheikh", "tomato", "peplum", "school",
        "crusts", "coping", "seaway", "mantel", "titres", "agrees", "verona", "jacked",
        "apiece", "belter", "eccles", "debugs", "lulled", "sedges", "amides", "highly",
        "parlay", "apache", "rubber", "valets", "siletz", "hombre", "rueful", "modest",
        "swathe", "foiled", "stares", "jokers", "daikon", "dobson", "unlord", "jurors",
        "obtain", "gilles", "earths", "amidst", "chided", "sundew", "played", "rescan",
        "flicks", "lyceum", "niacin", "wagers", "induct", "regula", "rhumba", "dhamma",
        "tildes", "radios", "riffle", "oswego", "dearer", "fogged", "tiptoe", "ludwig",
        "emeril", "embeds", "rained", "shaper", "bhakta", "freely", "machos", "groves",
        "yeoman", "dances", "stifle", "pseudo", "zigzag", "native", "zeroes", "knocks",
        "sculls", "driver", "morbus", "equine", "jingle", "eudora", "zeroth", "dilate",
        "darner", "rouges", "abbess", "curing", "agenda", "runkle", "revolt", "clowns",
        "origin", "kurgan", "scarfs", "medusa", "bluest", "paiute", "chords", "quincy",
        "heresy", "suresh", "burley", "merest", "darned", "babine", "clipse", "rhymes",
        "metals", "kieran", "pastas", "initio", "spells", "future", "nutria", "coined",
        "basses", "toxoid", "nipper", "crimea", "patria", "moosey", "acidic", "heaves",
        "unused", "paltry", "wipers", "stripy", "plague", "carpel", "ridder", "acorns",
        "busker", "caning", "quinte", "mutton", "nudges", "valine", "urgent", "barman",
        "impels", "faults", "yahoos", "galler", "propio", "mutism", "toasty", "cajuns",
        "goaded", "latigo", "whammy", "nomads", "suerte", "septic", "drapes", "dimmer",
        "agatha", "vortex", "backus", "ruling", "flugel", "spoons", "goodby", "bolded",
        "pulser", "shrink", "pipped", "ibidem", "midair", "esprit", "optics", "lifter",
        "wahine", "cesses", "pollan", "edison", "africa", "quidam", "causer", "blazer",
        "coling", "chicot", "flimsy", "chucks", "liming", "peapod", "smyrna", "goober",
        "dement", "dosage", "noises", "redial", "amadou", "flight", "valent", "stomps",
        "igitur", "toiled", "markka", "fruits", "phonic", "shanti", "kitten", "gaddis",
        "simeon", "premia", "dunker", "reverb", "avowed", "roared", "rewire", "myopic",
        "termes", "firmer", "granth", "twined", "earwax", "koller", "blades", "spiffy",
        "caller", "wapiti", "editor", "tended", "munity", "polkas", "cassie", "aryans",
        "ambers", "baltic", "grieve", "owners", "macros", "global", "flayed", "dozier",
        "maxims", "smacks", "redden", "hipped", "bunyip", "simply", "bombed", "hyphen",
        "lowder", "bevels", "steers", "unhook", "bidens", "armpit", "succes", "albums",
        "backup", "suited", "pained", "royals", "answer", "stowed", "recast", "daters",
        "misery", "abroad", "shewed", "callan", "schola", "raider", "steamy", "copter",
        "luxury", "lapdog", "adduce", "fenced", "chiave", "tapper", "scalps", "ridley",
        "leaner", "pistol", "anthem", "brinks", "amines", "flocks", "italic", "banyan",
        "bayous", "relais", "crosby", "gourds", "buffer", "racing", "blasts", "shelly",
        "offend", "samoan", "shoppy", "termed", "oddity", "avenge", "spined", "adonai",
        "cookie", "scorch", "trains", "badges", "enface", "office", "messin", "pelvis",
        "ananda", "effete", "bruise", "tempts", "envoys", "fingal", "anemic", "lyndon",
        "liners", "relate", "husker", "mussel", "baobab", "cuboid", "binder", "sugars",
        "orgies", "pavane", "schist", "obsess", "obtuse", "unless", "cayman", "morrow",
        "geryon", "pratap", "brandi", "recoup", "prefer", "prover", "vitric", "sicily",
        "anemia", "plaint", "riprap", "busses", "critic", "meader", "velcro", "howled",
        "sanity", "cayuse", "member", "ripper", "guilds", "strich", "christ", "satyrs",
        "ludlow", "marcos", "filtre", "mikael", "poppin", "piddle", "rookie", "corals",
        "speers", "vallis", "riyals", "gordon", "leaves", "smudge", "payees", "voodoo",
        "brooke", "musher", "ranged", "duster", "woolly", "canner", "enjoys", "samiti",
        "recede", "virial", "rudder", "looses", "enrage", "clefts", "queues", "fronts",
        "window", "hooves", "unveil", "marion", "orgone", "portia", "casita", "laymen",
        "allium", "hymnal", "sowing", "annuli", "diquat", "darien", "chisel", "invite",
        "maitre", "uncool", "callas", "sweety", "myxoma", "peaker", "dalles", "refrig",
        "persue", "harmer", "osprey", "montes", "danced", "sucker", "scouse", "reiner",
        "tilted", "lapper", "throat", "rogers", "trials", "knifed", "megara", "rabbit",
        "filmed", "moiety", "echoes", "buenas", "wetted", "bhakti", "regime", "nipple",
        "hummus", "higdon", "vertex", "puzzle", "wretch", "bouton", "butter", "boasts",
        "elicit", "socked", "garden", "whiney", "chokes", "cortex", "sloppy", "diadem",
        "spiers", "swears", "tapped", "evenly", "subsea", "siskin", "petits", "bucked",
        "towels", "sprang", "fiscus", "chiron", "debbie", "petted", "tetrad", "banger",
        "allows", "stevan", "jasmin", "shorea", "balaam", "bended", "mendel", "donkey",
        "ranked", "scrubs", "manchu", "apices", "acacia", "japans", "cuddly", "height",
        "looser", "maimed", "conics", "undone", "dimers", "brigid", "domina", "suckle",
        "encore", "brothe", "gamete", "unital", "jovian", "scarpa", "fleece", "sabine",
        "anneal", "mapper", "amused", "allene", "lasing", "devoid", "marita", "nellie",
        "fences", "derive", "labrum", "mocked", "boater", "nickle", "costal", "spicey",
        "oxides", "chased", "decoys", "chalon", "mandar", "brulee", "ternal", "untrue",
        "rector", "racoon", "crappy", "landed", "willey", "erupts", "detest", "plural",
        "posses", "brique", "hooper", "labour", "darius", "harmon", "streit", "halite",
        "insole", "tumult", "datura", "rooter", "umpqua", "fescue", "grainy", "yvonne",
        "umlaut", "alkene", "retake", "inward", "payors", "headed", "emboli", "tilden",
        "skater", "dollar", "swivel", "flexor", "collin", "talked", "parole", "nobler",
        "dumped", "trypan", "sipper", "throne", "jaunty", "legume", "ladder", "biased",
        "violet", "valgus", "darren", "biceps", "ducker", "whoops", "palais", "valves",
        "cancan", "formal", "albeit", "purged", "bunton", "squawk", "dialup", "muchly",
        "gaming", "sagged", "guelph", "ordain", "psycho", "fijian", "dosing", "ketone",
        "cougar", "dumber", "partly", "gather", "inodes", "ribald", "cygnus", "skelly",
        "gunman", "inning", "revere", "charon", "empire", "avanti", "felted", "lawyer",
        "gandhi", "smarmy", "gobble", "dimmed", "grebes", "efford", "mirage", "ashram",
        "causes", "filing", "skyway", "robbed", "mobbed", "purana", "insane", "digest",
        "hamper", "winkel", "saddle", "assort", "soothe", "subtly", "plated", "hushed",
        "quapaw", "slings", "lasted", "baking", "remain", "lenora", "axioms", "oldest",
        "ravish", "poured", "setups", "tsetse", "infant", "gnomes", "smocks", "shaggy",
        "surety", "karats", "defeat", "peeler", "dictum", "basque", "sayers", "beckie",
        "grates", "plates", "dunked", "fondue", "heeded", "closed", "retest", "rhonda",
        "dinner", "femmes", "laster", "myrica", "pannel", "landau", "eluent", "detect",
        "rafale", "corban", "floret", "usable", "scorns", "cruise", "barolo", "petals",
        "belted", "mandat", "maroon", "linens", "sought", "minted", "zambia", "shales",
        "hamzah", "fixing", "dallas", "nonuse", "honeys", "gaelic", "aghast", "dampen",
        "brushy", "shorty", "speeds", "zoomed", "stanly", "kinase", "rufous", "coupon",
        "preset", "duffel", "humour", "daemon", "scrape", "herons", "greffe", "coleen",
        "petite", "modern", "museum", "tissue", "remits", "groats", "pierre", "mudras",
        "sender", "ribble", "dually", "savant", "lender", "intown", "europa", "unlock",
        "scores", "schone", "laptop", "awakes", "fluffy", "stunts", "jigger", "strate",
        "debate", "murthy", "puffin", "gnosis", "covert", "chabot", "gibbet", "shader",
        "hereof", "reseal", "herder", "looped", "partis", "giller", "jigsaw", "accede",
        "colter", "midrib", "stinky", "rotten", "pseudo", "conned", "animas", "broods",
        "lackey", "broker", "sailer", "neared", "rabble", "shanty", "tucano", "storks",
        "gauges", "cohort", "sunway", "lesion", "eerily", "kiosks", "propyl", "bashaw",
        "errata", "haslet", "subpar", "spurns", "chaise", "jiggly", "pestis", "unspun",
        "troppo", "earwig", "demons", "virion", "askari", "zenith", "specks", "barrel",
        "stalag", "purple", "novena", "relent", "fianna", "folles", "eluted", "horned",
        "caplet", "reheat", "hodder", "dirigo", "celebs", "zombie", "methyl", "detail",
        "feeler", "quarry", "dasein", "struck", "bruges", "unread", "hubris", "hopper",
        "whiten", "scours", "notify", "resign", "wiggle", "engulf", "yogurt", "discus",
        "ferrer", "galore", "abused", "steels", "ornate", "verite", "bagger", "exotic",
        "justed", "scraps", "bedlam", "hanger", "calvin", "phased", "easter", "houser",
        "alumna", "pylons", "lizzie", "soaked", "routes", "wigeon", "hadith", "scrips",
        "fleets", "proofs", "heddle", "waited", "meeker", "veered", "elects", "barbed",
        "blight", "tanner", "demure", "tanked", "napper", "avatar", "ursine", "donnie",
        "locate", "kittie", "midden", "lowest", "wyvern", "hangul", "jaunts", "creaky",
        "renege", "emcees", "lipper", "capote", "orlean", "favors", "kuvasz", "baited",
        "ahimsa", "premix", "impair", "rubens", "lakota", "abided", "gallet", "shrubs",
        "varies", "photos", "monica", "faulds", "hoopoe", "refuel", "fathom", "roques",
        "depose", "coddle", "dredge", "impugn", "zinger", "motels", "verdun", "morice",
        "yakima", "hoodie", "hubbub", "pastis", "farris", "decree", "whence", "cheats",
        "goback", "barked", "unlink", "swords", "durant", "isobar", "abated", "wintry",
        "smarty", "basted", "common", "hakeem", "elliot", "holler", "emmett", "grotty",
        "tangle", "humbug", "jenson", "merely", "spices", "vannet", "sidled", "reiter",
        "sinewy", "humbly", "blooms", "ampere", "amante", "gander", "shelah", "coking",
        "granma", "palate", "update", "embark", "bougie", "policy", "addict", "worded",
        "pattie", "graeme", "wolves", "modena", "strewn", "siphon", "shilla", "revive",
        "balter", "instal", "givers", "resent", "teases", "broads", "mogged", "hotels",
        "minors", "dogmas", "vandal", "secede", "charms", "scharf", "pinyin", "chocks",
        "warder", "flavin", "freeze", "sappho", "ravine", "turbid", "mounds", "sanely",
        "romaji", "righty", "cartel", "tuneup", "pastel", "protid", "veniam", "dodges",
        "toledo", "hester", "beamer", "graham", "garage", "salvia", "vistas", "sahara",
        "joiner", "vestry", "shofar", "urging", "greets", "bobbed", "ardour", "artful",
        "gadfly", "hippie", "expire", "winnie", "crated", "buffed", "rodder", "shunts",
        "triton", "ruskin", "parlor", "grants", "fender", "scouts", "tickle", "shrews",
        "journo", "goethe", "knowns", "rented", "propel", "frowns", "mortis", "garnet",
        "escrow", "asylum", "ossian", "thalia", "brecht", "minced", "warsaw", "selene",
        "reckon", "liason", "swamps", "secret", "exeunt", "frosty", "micron", "protei",
        "corset", "jeanne", "attend", "primar", "indole", "stanze", "heller", "gobies",
        "paging", "briggs", "glared", "alpaca", "acadie", "befall", "venere", "pulley",
        "oblast", "square", "spores", "firkin", "tarrie", "seeker", "pinene", "pisgah",
        "devoto", "chaine", "abrupt", "eschew", "revues", "bowman", "rushed", "grovel",
        "tulipa", "baring", "abuser", "robles", "loathe", "koalas", "pilger", "beings",
        "occult", "astron", "chasse", "melena", "kagura", "banned", "boxers", "chacun",
        "colomb", "revise", "nailed", "diable", "golder", "tufted", "renown", "cloned",
        "online", "target", "surged", "breeze", "propos", "esteem", "duetto", "bethel",
        "tenner", "cashel", "warmly", "smugly", "result", "adored", "mutase", "spence",
        "florin", "anomie", "sneath", "marina", "rouser", "choirs", "fiends", "stroud",
        "simmer", "furrow", "cooper", "satiny", "burian", "combes", "unseat", "cupids",
        "slalom", "snuffy", "malted", "hermit", "plages", "badaxe", "deltas", "bogeys",
        "petrel", "ardent", "possum", "gables", "ducats", "biggie", "redraw", "orders",
        "pickel", "wampum", "tonsil", "ranker", "essays", "darker", "phasis", "pupils",
        "loaned", "strove", "enters", "surfer", "tablet", "jabiru", "gammon", "tricky",
        "tammuz", "huddle", "zoster", "foliar", "shasta", "havers", "whinge", "aspect",
        "kvetch", "cholla", "ghibli", "calmer", "kitbag", "picker", "marcot", "tongan",
        "lieder", "revved", "canela", "unroll", "dozers", "bailer", "darted", "licked",
        "joanne", "doting", "alerta", "parley", "shapes", "rising", "exiles", "versed",
        "lyases", "luring", "fogger", "stevel", "grammy", "eddies", "pastor", "pistil",
        "straka", "pinger", "lasses", "manent", "wilson", "hunker", "kaftan", "luther",
        "busboy", "manure", "mouton", "pumper", "oberon", "bowled", "feline", "veiled",
        "scopes", "raffia", "rashly", "futile", "prised", "altern", "borneo", "heated",
        "wrongs", "deists", "gouges", "discos", "azores", "tomcat", "blocky", "sitter",
        "shines", "naaman", "sparse", "demain", "seimas", "calles", "bitnet", "sicker",
        "clasts", "harbor", "rigger", "islets", "merino", "valued", "mishap", "imager",
        "wapato", "gulpin", "slough", "rebind", "broths", "eights", "skills", "pundit",
        "rectal", "ramona", "smiley", "pilate", "visite", "desire", "corrie", "sitcom",
        "lessee", "bisect", "hurdle", "colloq", "cogito", "mended", "choker", "mallee",
        "voters", "mimics", "infest", "thrush", "dannie", "output", "batted", "tawdry",
        "suplex", "shiner", "caucus", "tokens", "lazily", "soiled", "cloths", "collab",
        "forint", "ferrum", "navels", "norths", "excels", "blames", "moneys", "tilter",
        "emilia", "anansi", "adipic", "maniac", "remove", "batten", "ravers", "karren",
        "herero", "typify", "stitch", "auriga", "roused", "shades", "grouts", "parens",
        "harass", "bidden", "parish", "teresa", "ashman", "darwin", "fiddle", "hillel",
        "frater", "gunmen", "homers", "gorgon", "depths", "father", "chiles", "kimchi",
        "hangup", "raffle", "region", "haggis", "bennet", "dunner", "contes", "loader",
        "galium", "sundry", "renate", "strode", "scrawl", "sevens", "backer", "heuvel",
        "snooks", "mounts", "evades", "crypto", "corked", "harper", "elissa", "aeneid",
        "missal", "barong", "openly", "eighty", "baying", "sainte", "exhale", "shoppe",
        "sprite", "hinted", "outrun", "racked", "behold", "lingua", "abbots", "odious",
        "hoyles", "shamim", "expiry", "lowing", "exodus", "confit", "hainan", "sprung",
        "mystic", "caveat", "vernal", "outers", "netted", "euclid", "schule", "sherry",
        "fizzle", "zinnia", "albany", "eyelid", "shamed", "phobos", "globes", "monads",
        "juntas", "swirls", "battle", "venice", "pillow", "emptor", "whaler", "sunder",
        "format", "spying", "cadmus", "loaded", "baraka", "hitter", "fatima", "aspire",
        "safari", "mallow", "proven", "crowns", "coffee", "urbana", "mangle", "massif",
        "alcove", "amoeba", "cordis", "phones", "mayors", "eskimo", "remand", "looney",
        "forsee", "troupe", "orwell", "sallee", "curved", "cheney", "agates", "custom",
        "feeble", "cretin", "coning", "biomes", "keeper", "effort", "swales", "helped",
        "roader", "bailey", "toffee", "vivian", "graphs", "ultimo", "spaced", "coldly",
        "kitted", "newark", "tucson", "visita", "candor", "moaned", "relied", "straws",
        "becher", "daimyo", "lenard", "memory", "bypass", "ludden", "medley", "spring",
        "winnow", "anodes", "capita", "singly", "wigwam", "parkas", "semple", "francs",
        "pillar", "blenny", "cozens", "marked", "plaids", "malloy", "gassed", "scroll",
        "cental", "bosons", "budget", "throbs", "tennis", "tiffin", "portal", "nassau",
        "playas", "lusted", "quinta", "gratia", "crawly", "masons", "fixity", "oddest",
        "sorbed", "assure", "babbie", "casson", "fixate", "johann", "menace", "bamboo",
        "liable", "banged", "charta", "sativa", "forced", "flaked", "clonic", "corner",
        "blamed", "louver", "tattle", "labile", "hearts", "murrah", "parsed", "harold",
        "violin", "fibres", "parent", "archon", "cyclic", "ostomy", "halted", "reined",
        "toners", "lucian", "bright", "beulah", "shafts", "malaya", "audits", "hubert",
        "faeroe", "cutest", "elinor", "elwood", "vireos", "scored", "yantra", "refute",
        "faulty", "loving", "serena", "deepen", "napped", "forego", "theban", "demote",
        "jemima", "tonics", "oregon", "tuxedo", "boldin", "sheila", "bowker", "carman",
        "aymara", "dreamy", "cabins", "burkes", "gummed", "rigged", "riding", "vowing",
        "parses", "colmar", "floaty", "icarus", "sweaty", "radian", "postal", "buenos",
        "riches", "arcane", "turnip", "munger", "double", "otters", "driers", "othman",
        "espace", "hosing", "flinch", "lydian", "sefton", "sheiks", "pathol", "inform",
        "corded", "single", "rioted", "coffer", "marais", "eocene", "sontag", "hoover",
        "debits", "tabber", "lurked", "varuna", "outdid", "medals", "teensy", "sundog",
        "flaxen", "barlow", "titers", "rammed", "lamina", "mythos", "tented", "gretel",
        "parodi", "miller", "inputs", "polled", "ulcers", "pluton", "morton", "cesare",
        "sarong", "rosing", "layman", "cerium", "climes", "versus", "moloch", "entail",
        "utopia", "verily", "perron", "unsung", "texaco", "career", "tamper", "arcana",
        "mattes", "belive", "limped", "curtis", "oswald", "rabies", "reefer", "swifty",
        "preyed", "resaca", "puppet", "limpid", "inking", "tenino", "beaver", "buffet",
        "sector", "wormed", "refers", "poplar", "kindly", "adorns", "pathan", "scylla",
        "enlace", "canons", "birder", "yippee", "dimple", "ductus", "method", "nannie",
        "greyed", "killed", "sylvia", "hiatus", "funder", "oxygen", "atrial", "souped",
        "esters", "bionic", "halide", "sketch", "dermis", "tendre", "kittle", "otello",
        "adduct", "pagans", "indigo", "souled", "former", "cypher", "flings", "crouse",
        "craves", "hominy", "mortar", "tiptop", "trader", "carmen", "urbane", "stupid",
        "angles", "arriba", "magnon", "linker", "giving", "kobold", "bennis", "kicked",
        "ramjet", "specie", "tundra", "states", "athens", "opcode", "corral", "precis",
        "honest", "wenzel", "villas", "turbos", "paints", "roughs", "ricker", "gambol",
        "lament", "pander", "belton", "sachem", "chichi", "begone", "frosts", "martes",
        "agency", "restes", "lodges", "tracks", "kismet", "entree", "zodiac", "depict",
        "syndic", "draper", "cateye", "tricia", "reaper", "choses", "jabbed", "buyout",
        "stoked", "parody", "milles", "dyadic", "tacker", "oblate", "plexus", "woolen",
        "copula", "comber", "sombre", "plumer", "osmond", "stater", "nagged", "donned"
    };

    /// <summary>
    /// About 2,100 words
    /// </summary>
    private static string[] SevenLetterWords => new[]
    {
        "abating", "abiding", "abolish", "absinth", "abyssal", "acacias", "academy",
        "accents", "account", "acquire", "acreage", "actions", "acutely", "address",
        "adjunct", "adjusts", "adriana", "adviser", "affixed", "affront", "african",
        "against", "aground", "aileron", "aircrew", "airflow", "airmail", "airwise",
        "aladdin", "alcazar", "alister", "allelic", "alloted", "allured", "almanac",
        "almonds", "alpacas", "already", "alveoli", "amateur", "ambling", "amended",
        "america", "amharic", "ammonia", "amorous", "amyloid", "anarchy", "anasazi",
        "anatole", "aneurin", "anguish", "aniline", "annabel", "annexes", "annonce",
        "annulus", "another", "answers", "antacid", "antares", "antenna", "anthems",
        "antiqua", "antonia", "antonym", "anymore", "apaches", "apostle", "apparel",
        "applies", "aquaria", "aqueous", "aquinas", "arabian", "archean", "archery",
        "archive", "armband", "armorer", "armpits", "arouses", "arrival", "arsenic",
        "article", "artiest", "artless", "arugula", "asiatic", "asylums", "attract",
        "auberge", "auguste", "aunties", "auroral", "auroras", "austere", "austral",
        "austria", "authors", "autocar", "avenger", "avenues", "average", "awakens",
        "awaking", "axially", "babbled", "babysit", "baccara", "backing", "backlog",
        "badgers", "baggage", "baileys", "ballads", "ballard", "ballots", "bandura",
        "baneful", "banging", "bangles", "banking", "banquet", "baptize", "barbara",
        "barking", "barneys", "baronet", "baroque", "barrera", "barring", "bartram",
        "bashful", "basques", "bathtub", "batiste", "battles", "baubles", "beading",
        "bearish", "beatnik", "beatrix", "beavers", "because", "bedbugs", "bedrock",
        "beepers", "beeping", "befalls", "belcher", "belfast", "beliefs", "believe",
        "belleek", "belongs", "belting", "bending", "benefit", "benzine", "benzoic",
        "berated", "bergman", "betrays", "betting", "bettors", "between", "bicolor",
        "bicycle", "bifocal", "biggies", "billard", "binning", "biology", "biotics",
        "birding", "bishops", "bitched", "bitumen", "blacked", "blacken", "bladder",
        "blaming", "blankly", "bleeder", "blessed", "blinded", "blister", "blitzed",
        "blocker", "bloomed", "blooper", "blouses", "blowers", "blunted", "blurred",
        "blurted", "blusher", "boarded", "boatmen", "boffins", "boggles", "bolding",
        "bolivar", "bollard", "bombard", "bonuses", "bookend", "bookies", "booklet",
        "boolean", "boomers", "booster", "boppers", "boredom", "borings", "borough",
        "bosnian", "bossier", "bothers", "bottles", "boucher", "bouchon", "boulter",
        "bouncer", "bounces", "bowhead", "boycott", "bracing", "bradley", "brahmin",
        "braless", "brandon", "brasher", "braving", "breaded", "breathy", "breezes",
        "bribery", "bridals", "bridger", "briefed", "brights", "brining", "briskly",
        "bristle", "bristly", "bristol", "broadly", "bromide", "bronchi", "bronzes",
        "brother", "brownie", "browsed", "bruises", "brutish", "buddies", "budgets",
        "budgies", "buffers", "buffets", "bugbear", "bulbous", "bulging", "bulimia",
        "bulking", "bulldog", "bullies", "bullish", "bullpen", "bumpers", "bunched",
        "bungled", "bunions", "bunning", "bunting", "buoyant", "bureaus", "burgers",
        "burmese", "burrito", "butters", "buttery", "buyouts", "bylined", "bylines",
        "cabanas", "cabinet", "caching", "cadenza", "cadmium", "calcium", "calibre",
        "calkins", "cambium", "camilla", "camogie", "cantons", "canzone", "canzoni",
        "capping", "caprice", "captain", "captors", "carafes", "caramel", "caravan",
        "carcass", "cargoes", "carling", "carmela", "carmine", "carolan", "carolin",
        "carpets", "carrion", "carrots", "cartman", "cartoon", "castors", "catalan",
        "catarrh", "catcher", "catches", "catered", "causing", "caveman", "ceasing",
        "cements", "censors", "censure", "centage", "centers", "central", "centres",
        "centrum", "cerning", "ceviche", "chafers", "chafing", "chalice", "challah",
        "changes", "channel", "chanted", "chaotic", "chapman", "chapped", "charger",
        "charted", "chasers", "chasing", "checker", "chequer", "cherubs", "chevron",
        "chicory", "childre", "chilean", "chiming", "chloral", "choline", "chopped",
        "chorale", "chorizo", "choroid", "chucked", "chukchi", "cienega", "cinerea",
        "civitas", "clapped", "clarice", "clashed", "classic", "cleanse", "clicked",
        "climbed", "clinger", "clipped", "clipper", "clogged", "closely", "closeup",
        "closter", "clotted", "cloture", "clovers", "clubbed", "clumber", "coaches",
        "coasted", "cobourg", "coerced", "cognate", "collars", "college", "columba",
        "combust", "comedic", "comment", "commies", "commune", "company", "compare",
        "compels", "compere", "comport", "compose", "conceal", "concert", "concise",
        "condemn", "condors", "conjure", "conrail", "console", "consume", "contact",
        "contemp", "contend", "content", "contest", "control", "convert", "cookout",
        "coolest", "coopers", "cooties", "coppers", "copycat", "cordoba", "corinth",
        "cornell", "cornets", "coronet", "corsage", "corydon", "cougars", "couleur",
        "coulomb", "council", "country", "courage", "courant", "coursed", "courses",
        "covered", "cowbird", "cowlitz", "cowtown", "coyotes", "cracked", "crackle",
        "crappie", "crashed", "cravers", "creaked", "credits", "crimper", "crimson",
        "cronies", "cropper", "crowing", "crucify", "crudely", "cruelly", "crusher",
        "crushes", "cubbies", "cubicle", "culture", "culvert", "cumulus", "cunning",
        "curacao", "currant", "current", "curried", "cushion", "cuticle", "cutlery",
        "cutover", "cyclops", "cydonia", "cynthia", "cypress", "dabbled", "dakotas",
        "damiana", "damping", "damsels", "dancing", "daphnia", "darshan", "dawning",
        "daytime", "debater", "deborah", "debtors", "decease", "decided", "decimal",
        "deckers", "decoder", "decodes", "decreed", "deduced", "deeming", "defiant",
        "defraud", "defunct", "defused", "deicing", "delenda", "deliver", "deltoid",
        "demands", "demeter", "demonic", "demoted", "deniers", "densest", "depicts",
        "deposed", "depress", "dequeue", "derails", "derived", "derives", "derrick",
        "designs", "desired", "desktop", "details", "detects", "detente", "detroit",
        "devises", "devours", "dextran", "diabolo", "dialing", "diamant", "diapers",
        "diavolo", "dictate", "diesels", "digests", "diggers", "digging", "digital",
        "dilated", "dilator", "dimitry", "dinette", "diorama", "diptera", "dirtier",
        "disable", "disease", "dislike", "display", "dispute", "diurnal", "divider",
        "divides", "divisor", "doctors", "dodgers", "doilies", "dollars", "dominos",
        "donning", "doppler", "dorking", "dorothy", "dossier", "drafted", "dragoon",
        "dreaded", "drifter", "drinker", "drizzly", "droning", "dubious", "dumbing",
        "dunnage", "dupioni", "durable", "durango", "dusting", "dyeable", "eagerly",
        "earlier", "earlobe", "earners", "earshot", "echelon", "echoing", "edibles",
        "edifice", "edition", "eduardo", "effendi", "ejected", "elected", "elector",
        "embassy", "embolic", "eminent", "emirate", "empower", "emptied", "emptive",
        "emulate", "encased", "encores", "endgame", "endings", "endowed", "endures",
        "engarde", "english", "enjoins", "enjoyed", "enlaces", "enraged", "enteric",
        "envious", "enzymes", "epitaxy", "epoxies", "equiped", "erasmus", "erosive",
        "erratum", "escuela", "estates", "eulalia", "eurasia", "eustace", "evacuee",
        "evaders", "evasion", "eveline", "evoking", "evolved", "examine", "example",
        "excises", "exciter", "excites", "excreta", "exempts", "existed", "explain",
        "exploit", "exports", "expunge", "faculty", "faggots", "faintly", "fairies",
        "falcons", "familia", "fangled", "farmers", "farming", "farther", "fascias",
        "fastest", "fatally", "fateful", "fathers", "favored", "fawning", "federal",
        "feeders", "feelers", "felidae", "females", "femoral", "fetched", "fibroid",
        "fielder", "fifteen", "figures", "filling", "fimbles", "finance", "fishery",
        "fission", "fixedly", "flaming", "flatbed", "flecked", "flexion", "flicked",
        "floated", "floored", "flopped", "florian", "flowery", "fluidic", "flutter",
        "foaming", "focuser", "folders", "fondled", "fooling", "footers", "footway",
        "forages", "forceps", "foresaw", "forests", "forgave", "forging", "forgive",
        "forints", "formate", "formule", "forsake", "fourche", "foxwood", "framers",
        "fraying", "freddie", "freeing", "freezer", "fregata", "frescos", "freshly",
        "friends", "frigate", "fringes", "frisbee", "frisian", "fuchsia", "fuelled",
        "fulcrum", "fumbles", "funnies", "further", "futures", "galilee", "galilei",
        "gallery", "galling", "gallium", "gallops", "gambian", "gambler", "gangway",
        "gapping", "garners", "garters", "gasping", "gazelle", "gazette", "gelatin",
        "general", "genetic", "genital", "gentian", "gentium", "gestalt", "ghanian",
        "gigabit", "gigolos", "giraffe", "glacial", "gladius", "glamour", "glanced",
        "glassed", "glauber", "glazing", "gleeful", "glimmer", "glossed", "glutton",
        "glycols", "goalies", "gobbler", "godhead", "godward", "golding", "goliath",
        "gonadal", "gooding", "goofing", "gosling", "gouache", "goulash", "gracias",
        "gracing", "grained", "grammes", "granary", "grander", "grannie", "granola",
        "granted", "grantee", "granule", "grassed", "gravest", "graying", "greases",
        "greater", "greener", "gremlin", "grenade", "griddle", "griffin", "gripper",
        "grizzle", "grolier", "grooved", "groping", "grossen", "grosses", "grouchy",
        "grouped", "growths", "grudges", "grumble", "guessed", "gumtree", "gunther",
        "guzzles", "gymnast", "habitat", "hackman", "hacksaw", "hafnium", "haganah",
        "halbert", "halcyon", "hallman", "hallows", "hallway", "halting", "hambone",
        "hamburg", "hammers", "hamster", "hanbury", "handing", "handoff", "hankies",
        "hanuman", "hapless", "haploid", "harding", "harlots", "harmony", "harping",
        "harpoon", "harriet", "hastens", "hatches", "haulage", "hawkish", "hayseed",
        "hazards", "healers", "heaping", "heaters", "heavier", "heights", "helicon",
        "hellcat", "helmets", "hepatic", "herbert", "hernias", "heroine", "hessian",
        "hickory", "hillary", "hillman", "hipster", "history", "hitched", "hobnail",
        "hoedown", "hoisted", "hollers", "hollows", "homolog", "hoodlum", "hooting",
        "hopeful", "hoppers", "hopping", "hormone", "horrors", "hospice", "hostels",
        "hostile", "hosting", "hotdogs", "hotline", "however", "hulking", "humanly",
        "humbled", "humerus", "humidor", "humming", "humping", "hunters", "huskers",
        "huskies", "iberian", "idyllic", "ignited", "ilokano", "impaler", "imperil",
        "impiety", "impious", "implies", "imposed", "impound", "imprest", "imprint",
        "improve", "imputed", "include", "inconel", "indents", "indexed", "indexer",
        "indexes", "indicia", "indicts", "individ", "indulge", "infanta", "infects",
        "inflict", "infuser", "ingenio", "ingress", "injects", "injured", "innings",
        "inquest", "inroads", "insecta", "insider", "insides", "insight", "insists",
        "inspect", "instate", "insured", "insures", "invents", "inverse", "ionized",
        "ionizer", "islands", "isomers", "isotope", "issuers", "jackals", "jackson",
        "jadeite", "janitor", "january", "jarhead", "jarring", "jaspers", "jazzman",
        "jeepers", "jejunum", "jellies", "jerking", "jezebel", "jigsaws", "joannes",
        "jobbers", "jobsite", "johnnie", "johnson", "joiners", "joining", "jolting",
        "jornada", "journal", "judaica", "judging", "juergen", "jungian", "jungles",
        "juvenal", "kachina", "kannada", "karaoke", "kathryn", "kernels", "ketchup",
        "ketones", "ketosis", "keylock", "keypads", "kicking", "kimonos", "kindred",
        "kinetic", "kintyre", "kirghiz", "kirsten", "kirundi", "kissing", "kitting",
        "kneeled", "knowhow", "labored", "labours", "labrets", "lacking", "lacunae",
        "lagging", "lagoons", "laminae", "laminar", "lancets", "lansing", "laotian",
        "lapland", "lapwing", "largely", "latches", "lateral", "lateran", "latrine",
        "latrobe", "launder", "lawless", "layaway", "leaders", "leaflet", "leaking",
        "lecture", "leeches", "leeward", "legumes", "leisure", "lemming", "lempira",
        "leopard", "leptons", "lesbian", "lessees", "leucine", "levying", "liberia",
        "liberty", "libitum", "library", "licking", "lifting", "ligands", "likable",
        "lilting", "liminal", "limited", "limiter", "lindsey", "lingala", "lintels",
        "liqueur", "listens", "listing", "lithium", "liturgy", "livable", "lobbies",
        "located", "locater", "locates", "lockets", "locknut", "lockyer", "lodging",
        "logbook", "looking", "lookout", "lookups", "looming", "lorelei", "lorenzo",
        "lotions", "louvers", "loyalty", "luciana", "luminal", "lurkers", "macadam",
        "macbeth", "macedon", "machine", "madrona", "magasin", "magical", "magneto",
        "magnify", "mahjong", "mailbox", "maiming", "majeure", "majored", "malthus",
        "malting", "manatee", "mangers", "mangoes", "mangold", "mankind", "mannose",
        "mansion", "maoists", "marconi", "marilyn", "mariner", "markets", "marlena",
        "marries", "marsala", "marshal", "martial", "martins", "martyrs", "massing",
        "mastiff", "mastoid", "matawan", "matinee", "matsuri", "maudlin", "mauling",
        "maxilla", "meaning", "measles", "medevac", "medical", "mediums", "medulla",
        "meeting", "melanin", "melling", "melodia", "members", "mermaid", "message",
        "mestizo", "meteors", "micelle", "microbe", "middles", "midgard", "midline",
        "midterm", "militar", "million", "millman", "mimosas", "mincing", "minding",
        "mingled", "mingles", "minimum", "minnows", "minuses", "minutes", "mirabel",
        "mirador", "miranda", "miserly", "mislead", "missile", "mistake", "mithras",
        "modeled", "modular", "moliere", "moments", "monarch", "mongrel", "montage",
        "montero", "monthly", "moorhen", "moorish", "mosaics", "mothers", "mountie",
        "mujeres", "mullein", "mulling", "mullion", "mumbles", "mummies", "munster",
        "musings", "muskets", "muskrat", "mutable", "mutants", "mutates", "mutatis",
        "mutters", "myeloma", "mystics", "nagging", "napkins", "nappies", "natalia",
        "natchez", "natural", "naturel", "nauseam", "nebulae", "nesting", "netball",
        "netting", "network", "newgate", "niagara", "nibbles", "nickles", "nigeria",
        "nightly", "nipping", "noisily", "nonbank", "noncash", "nonstop", "northen",
        "nostril", "notable", "notably", "notched", "notches", "nothing", "noticed",
        "novator", "novelty", "nozzles", "nucleon", "numbers", "numeric", "nurture",
        "nutcase", "oakmoss", "obadiah", "obviate", "ocarina", "octaves", "october",
        "oeuvres", "offends", "offeror", "officio", "offside", "ohioans", "olympus",
        "ominous", "omnibus", "onboard", "oneself", "onshore", "oocytes", "openers",
        "ophelia", "opossum", "optical", "opticon", "options", "oracles", "oranges",
        "orbital", "ordeals", "orgasms", "orpheum", "orville", "othello", "otology",
        "outcome", "outfits", "outlets", "outlier", "outrage", "overdue", "overrun",
        "oversaw", "oxfords", "oysters", "pageant", "paisley", "palabra", "palaver",
        "palmyra", "pampers", "panacea", "pancake", "paneled", "panfish", "panning",
        "pansies", "panzers", "papayas", "papists", "papoose", "parable", "paraded",
        "parapet", "parfait", "parlors", "paroled", "parolee", "parsley", "parsons",
        "parties", "partita", "passage", "passant", "passing", "passive", "pastors",
        "patella", "patrons", "pattern", "pausing", "payable", "payment", "payroll",
        "peabody", "pecking", "penalty", "pending", "peonies", "percept", "perches",
        "perfume", "perfumy", "petiole", "phyllis", "pickets", "picture", "piercer",
        "pigtail", "pilcher", "pilsner", "pimlico", "pincher", "pinkish", "pinless",
        "pirated", "piscina", "piscine", "pitched", "pitfall", "pivoted", "placket",
        "plainer", "plainly", "planets", "planted", "planter", "plasmin", "plasmon",
        "plateau", "platoon", "playman", "pleases", "pliable", "plosive", "plotter",
        "plunges", "plywood", "poetess", "pogroms", "pointed", "pointer", "pollack",
        "pontiff", "pontoon", "pooling", "poppies", "popular", "porcine", "portlet",
        "possums", "postbag", "postfix", "posting", "postman", "postwar", "potions",
        "powdery", "powered", "preachy", "preamps", "precise", "pregame", "preheat",
        "premise", "prepare", "present", "preteen", "preying", "primacy", "primeur",
        "priming", "prisons", "privacy", "private", "problem", "process", "proctor",
        "prodded", "produce", "product", "profess", "profile", "profits", "program",
        "project", "prolong", "prosody", "provide", "proving", "proxied", "proxies",
        "prudent", "psyched", "ptolemy", "publish", "puccini", "puffing", "pulsars",
        "punched", "punches", "pungent", "purbeck", "puritan", "pursuer", "pushing",
        "pushrod", "putters", "puzzled", "pygmies", "pyloric", "pyramid", "quality",
        "quantal", "quarrel", "quartus", "quetzal", "quickly", "quintet", "quintus",
        "quizzes", "quonset", "radiate", "raiders", "raiding", "railway", "raining",
        "rallied", "ramadan", "rambled", "rancher", "ranchos", "randers", "rangers",
        "rankine", "rapidly", "rapists", "rappers", "rascals", "rattled", "ravioli",
        "rawhide", "reached", "reacher", "readout", "rebecca", "rebuked", "recalls",
        "recipes", "reckons", "recline", "records", "rectors", "rectory", "recurse",
        "recusal", "recused", "redbird", "reddish", "redfish", "redoute", "redskin",
        "reeling", "reentry", "referee", "reflect", "refocus", "reforms", "refuted",
        "regaled", "regency", "regimen", "regress", "regrets", "regroup", "reissue",
        "rejoice", "relapse", "related", "relaxed", "release", "reliant", "remakes",
        "remarks", "remarry", "remixed", "remnant", "remodel", "removes", "renames",
        "rentals", "renters", "reorder", "repeals", "repents", "replays", "replete",
        "reports", "reproof", "repulse", "request", "rescale", "resists", "resound",
        "restate", "results", "retails", "reticle", "retract", "retread", "returns",
        "reunion", "reveals", "reveled", "revered", "reverse", "reviews", "reviser",
        "revisor", "revived", "rewinds", "rewrote", "rhenium", "rhodium", "ribbons",
        "ricardo", "ricotta", "ridding", "riddled", "rioting", "ripcord", "ripoffs",
        "riposte", "rippers", "ripstop", "riveted", "riveter", "riviera", "roaches",
        "roaring", "roasted", "roebuck", "rollout", "romaine", "rompers", "rondeau",
        "roofing", "rooster", "rosalyn", "rosella", "rotator", "rounded", "rounder",
        "rousing", "roxanne", "royalty", "ruffles", "rumbled", "rumored", "rumours",
        "rundown", "runnels", "runtime", "russian", "rustled", "saguaro", "saintly",
        "salinas", "salmons", "salomon", "salting", "saltman", "salukis", "salvage",
        "samhain", "sampled", "sandman", "sangria", "sapphic", "satiety", "saucers",
        "savages", "savings", "saviour", "savored", "sawyers", "sayings", "scabies",
        "scalper", "scamper", "scandia", "scanner", "scarcer", "scarier", "scarred",
        "scatter", "schloss", "schools", "science", "scissor", "scooped", "scooter",
        "scopate", "scorpio", "scourge", "scouted", "scraped", "scrapes", "scratch",
        "scribes", "scrubby", "sculpin", "seafoam", "sealine", "seasons", "seaters",
        "seconds", "secrete", "section", "seeders", "seethed", "seismic", "selfish",
        "sellout", "semites", "servant", "service", "settles", "several", "shabbat",
        "shadowy", "shakeup", "shannon", "shapely", "shareef", "sharing", "sheared",
        "sheaths", "sheeted", "shellac", "shewing", "shifted", "shifter", "shingle",
        "shipman", "shipper", "shocker", "shoeing", "shovels", "showers", "shrieks",
        "shrinks", "shudder", "shuffle", "sickens", "sidecar", "sigmoid", "sigmund",
        "signers", "silents", "silesia", "similar", "similes", "sinatra", "sirloin",
        "sizzler", "skelter", "skewing", "skidded", "skillet", "skimmed", "skipper",
        "skylark", "slander", "slapped", "sleeper", "slicker", "slivers", "sloping",
        "sloughs", "smacked", "smarter", "smiling", "smitten", "smudges", "smuggle",
        "sneaked", "sneezed", "snicker", "snorkel", "snuffed", "soaking", "soberly",
        "society", "softest", "softies", "solutio", "sonnets", "soothed", "sorcery",
        "sorrows", "sorters", "sorting", "sources", "souther", "spartan", "spatial",
        "spawner", "spaying", "special", "species", "specify", "specter", "speight",
        "spencer", "spiders", "spiller", "spindle", "spinner", "spitzer", "splints",
        "spooner", "spotter", "sputnik", "squalls", "squeals", "squishy", "stabler",
        "stainer", "stamped", "stapled", "starker", "statute", "steeves", "stephan",
        "stereos", "sterner", "stetson", "stilton", "stipend", "stopgap", "stopped",
        "stopper", "stories", "storing", "strayer", "streets", "strider", "strolls",
        "stubble", "student", "studies", "stuffed", "stunned", "subdued", "subject",
        "sublime", "subsume", "subtype", "succour", "suggest", "sumatra", "summery",
        "summons", "sunrise", "sunsets", "sunspot", "suppers", "support", "surface",
        "surfeit", "surfers", "surgeon", "surgery", "surreal", "swapper", "swarmed",
        "swarthy", "sweeper", "swifter", "swisher", "systems", "tablets", "tabstop",
        "tacitly", "tacking", "tactics", "tagalog", "tailers", "tailors", "talking",
        "tamping", "tampons", "tangent", "tangles", "tanners", "tannery", "tannins",
        "tantric", "tantrum", "tapioca", "tartare", "tassels", "tatting", "tattoos",
        "taxiing", "teaches", "teacups", "teasers", "tedious", "telecon", "telesis",
        "tempera", "tempted", "tempter", "tenable", "tenders", "tending", "tenuate",
        "tenures", "termine", "termini", "terreno", "terrine", "testbed", "testers",
        "tesuque", "tetanus", "textual", "theater", "thereon", "thiamin", "thicken",
        "thimble", "thirdly", "thirsty", "thither", "threads", "through", "thumbed",
        "thumper", "tibetan", "tiburon", "tickets", "tickled", "tidings", "tidying",
        "tilings", "timothy", "tinting", "tippers", "tissues", "tithing", "toaster",
        "toggled", "toggles", "toiling", "tokelau", "tonight", "tonsils", "tooting",
        "topiary", "topside", "toronto", "torrens", "torrent", "tossing", "touched",
        "touches", "touting", "towards", "toyland", "trabant", "traffic", "tragedy",
        "tranter", "trapped", "treason", "treated", "trefoil", "tresses", "tricked",
        "trigram", "trimmed", "triplet", "tripods", "tritone", "trivium", "trolled",
        "troller", "trophic", "trotted", "trueman", "trumper", "trustee", "tubular",
        "tumbles", "tunisia", "tunning", "tussock", "tutored", "twaddle", "tweedle",
        "tweeter", "twiddle", "twining", "twinkle", "twisted", "ugliest", "ultimas",
        "ulysses", "unarmed", "unasked", "unblock", "uncivil", "unfired", "unicorn",
        "unifies", "unknown", "unlearn", "unspent", "untruth", "untying", "untyped",
        "unwired", "updated", "upholds", "uploads", "upwards", "urethra", "uterine",
        "utilize", "utopian", "vacancy", "vaccine", "vagrant", "valeria", "valiant",
        "vassals", "vectors", "velvety", "vendors", "verbose", "verdant", "version",
        "vertigo", "vesicle", "vestige", "vetting", "vibrant", "violent", "virtues",
        "viruses", "viscera", "viscous", "visions", "visuals", "vitesse", "vitriol",
        "voicing", "wafting", "wallaby", "walling", "waratah", "wardens", "warming",
        "warning", "warpath", "warring", "warrior", "warthog", "washita", "wayland",
        "wettest", "whaling", "wheelie", "whereby", "wherein", "whether", "whiskey",
        "whitish", "whizzes", "widower", "wilhelm", "winches", "windows", "windrow",
        "windsor", "winfred", "wingate", "winging", "winking", "winless", "winsome",
        "wiretap", "wisdoms", "wiseman", "wishful", "wishing", "without", "wizards",
        "wobbler", "wooster", "working", "workmen", "wrapper", "wreaths", "wriggle",
        "written", "xenopus", "xylitol", "yankees", "yankton", "yiddish", "yoghurt",
        "zeroing"
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
}
