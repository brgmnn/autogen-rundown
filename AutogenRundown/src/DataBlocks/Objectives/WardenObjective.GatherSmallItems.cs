using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/// <summary>
/// Objective: GatherSmallItems
///
/// Gather small items from around the level. This is a fairly simple objective
/// that can be completed in a variety of ways.
/// </summary>
public partial record WardenObjective
{
    public void PreBuild_GatherSmallItems(BuildDirector director, Level level)
    {
        GatherRequiredCount = (level.Tier, director.Bulkhead) switch
        {
            ("A", Bulkhead.Main) => Generator.Between(4, 8),
            ("A", _) =>             Generator.Between(2, 3),

            ("B", Bulkhead.Main) => Generator.Between(6, 10),
            ("B", _) =>             Generator.Between(2,  4),

            ("C", Bulkhead.Main)     => Generator.Between(7, 12),
            ("C", Bulkhead.Extreme)  => Generator.Between(5, 7),
            ("C", Bulkhead.Overload) => Generator.Between(2, 4),

            ("D", Bulkhead.Main)     => Generator.Between(8, 13),
            ("D", Bulkhead.Extreme)  => Generator.Between(5, 8),
            ("D", Bulkhead.Overload) => Generator.Between(3, 5),

            ("E", Bulkhead.Main)     => Generator.Between(9, 16),
            ("E", Bulkhead.Extreme)  => Generator.Between(6, 8),
            ("E", Bulkhead.Overload) => Generator.Between(3, 6),

            _ => 1,
        };

        GatherSpawnCount = GatherRequiredCount switch
        {
            < 6  => GatherRequiredCount, // 2, 3, 4, 5
            < 9  => Generator.Between(GatherRequiredCount, GatherRequiredCount + 1), // 6, 7, 8
            < 12 => Generator.Between(GatherRequiredCount, GatherRequiredCount + 2), // 9, 10, 11
            _    => Generator.Between(GatherRequiredCount, GatherRequiredCount + 3)  // 12, 13, 14, 15, 16
        };
    }

    public void Build_GatherSmallItems(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        // Pulls a new item and description from the small pickup pack
        var itemId = Generator.DrawSelect(level.Settings.SmallPickupPack);

        var name = itemId switch
        {
            WardenObjectiveItem.MemoryStick      => "Memory stick",
            WardenObjectiveItem.PersonnelId      => "Personnel ID",
            WardenObjectiveItem.PartialDecoder   => "Partial Decoder",
            WardenObjectiveItem.Harddrive        => "Hard drive",
            WardenObjectiveItem.Glp_1            => "GLP-1 canister",
            WardenObjectiveItem.Glp_2            => "GLP-2 canister",
            WardenObjectiveItem.Osip             => "OSIP vial",
            WardenObjectiveItem.PlantSample      => "Plant sample",
            WardenObjectiveItem.DataCube         => "Data cube",
            WardenObjectiveItem.DataCubeBackup   => "Backup data cube",
            WardenObjectiveItem.DataCubeTampered => "Tampered data cube",
            _ => "item"
        };
        MainObjective = itemId switch
        {
            WardenObjectiveItem.MemoryStick =>
                "Gather [COUNT_REQUIRED] Memory sticks and return the memory sticks for analysis.",
            WardenObjectiveItem.PersonnelId =>
                "Gather [COUNT_REQUIRED] Personnel IDs and return the data to be processed.",
            WardenObjectiveItem.PartialDecoder =>
                "Gather [COUNT_REQUIRED] Partial Decoders and return the data to be processed.",
            WardenObjectiveItem.Harddrive =>
                "Gather [COUNT_REQUIRED] Hard Drives and return the drives for data archival.",
            WardenObjectiveItem.Glp_1 =>
                "Gather [COUNT_REQUIRED] GLP-1 canisters and return the canisters for genome sequencing.",
            WardenObjectiveItem.Glp_2 =>
                "Gather [COUNT_REQUIRED] GLP-2 canisters and return the canisters for genome sequencing.",
            WardenObjectiveItem.Osip =>
                "Gather [COUNT_REQUIRED] OSIP vials and return the vials for chemical analysis.",
            WardenObjectiveItem.PlantSample =>
                "Gather [COUNT_REQUIRED] Plant samples and return the samples for analysis.",
            WardenObjectiveItem.DataCube =>
                "Gather [COUNT_REQUIRED] Data cubes and return the cubes for data extraction.",
            WardenObjectiveItem.DataCubeBackup =>
                "Gather [COUNT_REQUIRED] Backup Data cubes and return the cubes for data archival.",
            WardenObjectiveItem.DataCubeTampered =>
                "Gather [COUNT_REQUIRED] Data cubes and return the cubes for inspection.",
            _ => "Gather items"
        };
        FindLocationInfo = $"Look for {name}s in the complex";
        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";

        GatherItemId = (uint)itemId;

        /*
         * We want to distribute the items evenly across all the zones marked as `find_items`.
         * The LevelLayout code will generate an interesting layout for these
         */
        var placements = Gather_PlacementNodes
            .Select(node => new ZonePlacementData()
            {
                LocalIndex = node.ZoneNumber,
                Weights = ZonePlacementWeights.EvenlyDistributed
            }).ToList();
        dataLayer.ObjectiveData.ZonePlacementDatas.Add(placements);

        // We only need to divide up the spawn count. Nothing fancy
        GatherMaxPerZone = GatherSpawnCount / placements.Count;

        // TODO: AddCompletedObjectiveWaves(level, director);

        var itemName = itemId switch
        {
            WardenObjectiveItem.PersonnelId => "Personnel ID",
            WardenObjectiveItem.PartialDecoder => "Decoder",
            WardenObjectiveItem.Harddrive => "Hard Drive",
            WardenObjectiveItem.Glp_1 => "GLP Canister",
            WardenObjectiveItem.Glp_2 => "GLP Canister",
            WardenObjectiveItem.Osip => "Vials",
            WardenObjectiveItem.PlantSample => "Plant Sample",
            WardenObjectiveItem.DataCube => "Data Cube",
            WardenObjectiveItem.DataCubeBackup => "Data Cube",
            WardenObjectiveItem.DataCubeTampered => "Data cube",
            _ => "item"
        };

        #region Warden Intel Messages
        /*
         * Gather small items elevator drop messages
         */
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
        {
            $">... [flashlight click]\r\n>... Check every locker and box.\r\n>... <size=200%><color=red>We need all small items!</color></size>",
            $">... Quiet now.\r\n>... If you spot any {itemName}'s, grab them.\r\n>... <size=200%><color=red>Don't hesitate.</color></size>",
            $">... Terminals can help us.\r\n>... QUERY, then PING the target.\r\n>... <size=200%><color=red>Follow the beep.</color></size>",
            $">... Mark rooms cleared.\r\n>... Call out any {itemName}'s.\r\n>... <size=200%><color=red>Track our count.</color></size>",
            $">... Shelves, floor, under pipes.\r\n>... Look everywhere.\r\n>... <size=200%><color=red>No blind spots.</color></size>",
            $">... [drawer creak]\r\n>... Easy on the noise.\r\n>... <size=200%><color=red>Sleepers are nearby.</color></size>",
            $">... Use the map.\r\n>... Grid the sweep.\r\n>... <size=200%><color=red>Flag any {itemName}'s.</color></size>",
            $">... Locker cluster ahead.\r\n>... Good odds.\r\n>... <size=200%><color=red>Open everything.</color></size>",
            $">... PING again.\r\n>... If it's a {itemName}, bag it.\r\n>... <size=200%><color=red>Move on.</color></size>",
            $">... [hiss of fog]\r\n>... Hard to see.\r\n>... <size=200%><color=red>Feel along the racks.</color></size>",
            $">... Split: open, cover, bag.\r\n>... Rotate fast.\r\n>... <size=200%><color=red>We're on the clock.</color></size>",
            $">... Box under the catwalk.\r\n>... Tag's half torn.\r\n>... <size=200%><color=red>Could be a {itemName}.</color></size>",
            $">... Behind the server rack.\r\n>... Dusty case.\r\n>... <size=200%><color=red>Bag it and move.</color></size>",
            $">... Inventory later.\r\n>... Right now, {itemName}'s.\r\n>... <size=200%><color=red>Hands moving.</color></size>",
            $">... Power's unstable.\r\n>... Use your lights low.\r\n>... <size=200%><color=red>Don't miss the small cases.</color></size>",
            $">... Keep count.\r\n>... We're short on {itemName}'s.\r\n>... <size=200%><color=red>Find more, quickly.</color></size>",
            $">... Shelves look picked clean.\r\n>... Floor crates next.\r\n>... <size=200%><color=red>Leave nothing behind.</color></size>",
            $">... Check bathrooms too.\r\n>... People stash {itemName}'s anywhere.\r\n>... <size=200%><color=red>Even there.</color></size>",
            $">... [soft ping]\r\n>... New location.\r\n>... <size=200%><color=red>Next zone over.</color></size>",
            $">... Quiet hands.\r\n>... Loud drawers wake them.\r\n>... <size=200%><color=red>Grab the {itemName} and go.</color></size>",
            $">... Use QUERY on the terminal.\r\n>... Cross-check the zone.\r\n>... <size=200%><color=red>Plan the route.</color></size>",
            $">... Found one.\r\n>... Need more {itemName}'s.\r\n>... <size=200%><color=red>Keep digging.</color></size>",
            $">... Check under the stairs.\r\n>... Someone stashed gear.\r\n>... <size=200%><color=red>Search it all.</color></size>",
            $">... PING drifted.\r\n>... Might be adjacent.\r\n>... <size=200%><color=red>Eyes open for {itemName}'s.</color></size>",
            $">... [shelf clatter]\r\n>... Watch the noise.\r\n>... <size=200%><color=red>Stealth saves ammo.</color></size>",
            $">... Bag capacity low.\r\n>... Hand-carry {itemName}'s.\r\n>... <size=200%><color=red>No excuses.</color></size>",
            $">... This one's sealed tight.\r\n>... Hack it fast.\r\n>... <size=200%><color=red>Don't trip the room.</color></size>",
            $">... Eyes low.\r\n>... Small cases blend in.\r\n>... <size=200%><color=red>Spot the {itemName}'s.</color></size>",
            $">... [faint beep through wall]\r\n>... Must be next area.\r\n>... <size=200%><color=red>Find the door key.</color></size>",
            $">... Staging area marked.\r\n>... Runner shuttles {itemName}'s.\r\n>... <size=200%><color=red>Less backtracking.</color></size>",
            $">... Lights flicker.\r\n>... Sweep the corners.\r\n>... <size=200%><color=red>Don't overlook the floor.</color></size>",
            $">... Map says three left.\r\n>... Any {itemName}'s count.\r\n>... <size=200%><color=red>Hurry.</color></size>",
            $">... Label reads 'consumables'.\r\n>... Might hide components.\r\n>... <size=200%><color=red>Open everything.</color></size>",
            $">... Under the tarp.\r\n>... Cases taped together.\r\n>... <size=200%><color=red>Cut and pull {itemName}'s.</color></size>",
            $">... Racks on wheels.\r\n>... Check behind them.\r\n>... <size=200%><color=red>People hide things.</color></size>",
            $">... Terminal lists last-seen.\r\n>... Could be moved.\r\n>... <size=200%><color=red>Search for {itemName}'s nearby.</color></size>",
            $">... Hear that click?\r\n>... Case latch under desk.\r\n>... <size=200%><color=red>Reach in slow.</color></size>",
            $">... Dead end.\r\n>... Backtrack.\r\n>... <size=200%><color=red>We still need {itemName}'s.</color></size>",
            $">... Vent noise above.\r\n>... Keep it calm.\r\n>... <size=200%><color=red>Work in whispers.</color></size>",
            $">... This bin's empty.\r\n>... Next room.\r\n>... <size=200%><color=red>Find those {itemName}'s.</color></size>",
            $">... Dust is thick.\r\n>... Looks untouched.\r\n>... <size=200%><color=red>Promising find.</color></size>",
            $">... Count out loud.\r\n>... Two {itemName}'s so far.\r\n>... <size=200%><color=red>How many remain?</color></size>",
            $">... Check the workbench.\r\n>... Tools, tags, vials.\r\n>... <size=200%><color=red>Bag everything.</color></size>",
            $">... Low on supplies?\r\n>... These {itemName}'s keep us going.\r\n>... <size=200%><color=red>Don't miss any.</color></size>",
            $">... Floor grate loose.\r\n>... Something slid under.\r\n>... <size=200%><color=red>Lift and look.</color></size>",
            $">... Found spares.\r\n>... Not the {itemName} we need.\r\n>... <size=200%><color=red>Keep moving.</color></size>",
            $">... Watch your step.\r\n>... Cases scatter on impact.\r\n>... <size=200%><color=red>Place, don't toss.</color></size>",
            $">... That shelf back corner.\r\n>... See the tag?\r\n>... <size=200%><color=red>Could be {itemName}'s.</color></size>",
            $">... Someone guard the door.\r\n>... Openers focus.\r\n>... <size=200%><color=red>Clean sweep.</color></size>",
            $">... Terminal ping steady.\r\n>... Close.\r\n>... <size=200%><color=red>Eyes open for {itemName}'s.</color></size>",
            $">... Quiet scrape.\r\n>... Drawer half-stuck.\r\n>... <size=200%><color=red>Ease it out.</color></size>",
            $">... Bag's heavy.\r\n>... Still room for {itemName}'s.\r\n>... <size=200%><color=red>Keep loading.</color></size>",
            $">... Under the pallet.\r\n>... Tiny case wedged.\r\n>... <size=200%><color=red>Pull it free.</color></size>",
            $">... If you spot any {itemName}'s, grab them.\r\n>... Call it in.\r\n>... <size=200%><color=red>Runner inbound.</color></size>",
            $">... Stash behind the conduit.\r\n>... Tape marks the spot.\r\n>... <size=200%><color=red>Cut it open.</color></size>",
            $">... Check the start area again.\r\n>... We missed {itemName}'s before.\r\n>... <size=200%><color=red>No repeats.</color></size>",
            $">... Cabinet with a false back.\r\n>... Press and slide.\r\n>... <size=200%><color=red>Hidden cache.</color></size>",
            $">... QUERY complete.\r\n>... Results show {itemName}'s across zones.\r\n>... <size=200%><color=red>Plan the loop.</color></size>",
            $">... Stepladder here.\r\n>... Top shelf has cases.\r\n>... <size=200%><color=red>Climb, check, clear.</color></size>",
            $">... That crate label.\r\n>... Matches {itemName} manifest.\r\n>... <size=200%><color=red>Open it.</color></size>",
            $">... Watch for trip clutter.\r\n>... Kicks ring out.\r\n>... <size=200%><color=red>Slow feet.</color></size>",
            $">... We have three {itemName}'s.\r\n>... Need at least five.\r\n>... <size=200%><color=red>Keep at it.</color></size>",
            $">... Corner office.\r\n>... Desk drawers locked.\r\n>... <size=200%><color=red>Pick them quietly.</color></size>",
            $">... Long aisle ahead.\r\n>... Tiny tags on the right.\r\n>... <size=200%><color=red>Scan for {itemName}'s.</color></size>",
            $">... Staging point marked.\r\n>... Pile the haul.\r\n>... <size=200%><color=red>Count twice.</color></size>",
            $">... If it's not a {itemName}, skip it.\r\n>... Don't waste time.\r\n>... <size=200%><color=red>Priorities.</color></size>",
            $">... Tool chest rattles.\r\n>... Something small inside.\r\n>... <size=200%><color=red>Open carefully.</color></size>",
            $">... Keep your light low.\r\n>... {itemName}'s reflect.\r\n>... <size=200%><color=red>Spot the glint.</color></size>",
            $">... Server bay side panel.\r\n>... Loose screws.\r\n>... <size=200%><color=red>Pop it off.</color></size>",
            $">... Ping faint.\r\n>... The {itemName} must be buried.\r\n>... <size=200%><color=red>Dig deeper.</color></size>",
            $">... Check the ceiling racks.\r\n>... A crate stuck above.\r\n>... <size=200%><color=red>Hook it down.</color></size>",
            $">... We doubled back.\r\n>... Found two more {itemName}'s.\r\n>... <size=200%><color=red>Good catch.</color></size>",
            $">... Filing cabinets.\r\n>... Bottom drawers often missed.\r\n>... <size=200%><color=red>Check them now.</color></size>",
            $">... If you spot any {itemName}'s, grab them.\r\n>... Don't call until secure.\r\n>... <size=200%><color=red>Then move.</color></size>",
            $">... Storage room smells stale.\r\n>... Untouched for ages.\r\n>... <size=200%><color=red>Likely stash.</color></size>",
            $">... Locker nameplates match {itemName} bins.\r\n>... Open every one.\r\n>... <size=200%><color=red>No skips.</color></size>",
            $">... Under desk cable tray.\r\n>... Slim case wedged in.\r\n>... <size=200%><color=red>Slide it out.</color></size>",
            $">... Status check.\r\n>... {itemName}'s collected: update.\r\n>... <size=200%><color=red>Call the number.</color></size>",
            $">... Cage door ajar.\r\n>... Somebody rushed.\r\n>... <size=200%><color=red>We finish their job.</color></size>",
            $">... If it's small and tagged {itemName}, take it.\r\n>... No duplicates problem.\r\n>... <size=200%><color=red>All count.</color></size>",
            $">... Worklight on the floor.\r\n>... Shadows hide cases.\r\n>... <size=200%><color=red>Shift the beam.</color></size>",
            $">... Drawer false bottom.\r\n>... Classic stash for {itemName}'s.\r\n>... <size=200%><color=red>Pop it.</color></size>",
            $">... Pallet jack in the way.\r\n>... Roll it aside.\r\n>... <size=200%><color=red>Access the bins.</color></size>",
            $">... Two teams.\r\n>... One pings {itemName}'s, one collects.\r\n>... <size=200%><color=red>Keep pace.</color></size>",
            $">... Smashed display case.\r\n>... Debris hides labels.\r\n>... <size=200%><color=red>Brush, then grab.</color></size>",
            $">... Terminal says cluster ahead.\r\n>... Multiple {itemName}'s together.\r\n>... <size=200%><color=red>Prime target.</color></size>",
            $">... Rattle in the duct.\r\n>... Ignore it.\r\n>... <size=200%><color=red>Focus on the search.</color></size>",
            $">... If you spot any {itemName}'s, grab them.\r\n>... Don't split too far.\r\n>... <size=200%><color=red>Stay in earshot.</color></size>",
            $">... Old med closet.\r\n>... Tiny trays lined up.\r\n>... <size=200%><color=red>Open each.</color></size>",
            $">... Bag's full.\r\n>... Swap carrier, keep {itemName}'s flowing.\r\n>... <size=200%><color=red>No downtime.</color></size>",
            $">... Rolling cart shelves.\r\n>... Check underside lip.\r\n>... <size=200%><color=red>Hidden tape.</color></size>",
            $">... Found decoys.\r\n>... Not the {itemName} tag.\r\n>... <size=200%><color=red>Skip and move.</color></size>",
            $">... Office safe open.\r\n>... Empty...\r\n>... <size=200%><color=red>Try the file room.</color></size>",
            $">... If it's marked {itemName}, it's ours.\r\n>... No exceptions.\r\n>... <size=200%><color=red>Bag it.</color></size>",
            $">... Mop closet.\r\n>... Box behind buckets.\r\n>... <size=200%><color=red>Grab and go.</color></size>",
            $">... Status: still missing {itemName}'s.\r\n>... Back to Zone 23.\r\n>... <size=200%><color=red>We missed something.</color></size>",
            $">... Cable spool rack.\r\n>... Narrow gap hides case.\r\n>... <size=200%><color=red>Reach carefully.</color></size>",
            $">... Ping settled.\r\n>... {itemName} is close.\r\n>... <size=200%><color=red>Eyes down.</color></size>",
            $">... Rusty locker.\r\n>... Hinge screams.\r\n>... <size=200%><color=red>Foam it, then open.</color></size>",
            $">... Count again.\r\n>... We need three more {itemName}'s.\r\n>... <size=200%><color=red>Make it happen.</color></size>",
            $">... Dark corner.\r\n>... Reflective label glints.\r\n>... <size=200%><color=red>There it is.</color></size>",
            $">... Keep a runner free.\r\n>... Shuttle {itemName}'s to staging.\r\n>... <size=200%><color=red>Maintain flow.</color></size>",
            $">... Collapse shelf pile.\r\n>... Dig from the top.\r\n>... <size=200%><color=red>Watch for noise.</color></size>",
            $">... Terminal cross-check.\r\n>... {itemName}'s seen in 45C.\r\n>... <size=200%><color=red>Head there now.</color></size>",
            $">... Drawer label 'misc'.\r\n>... That's usually gold.\r\n>... <size=200%><color=red>Open it.</color></size>",
            $">... If you spot any {itemName}'s, grab them.\r\n>... Call 'SECURE' after bagging.\r\n>... <size=200%><color=red>Then move.</color></size>",
            $">... Vent grate popped.\r\n>... Small box inside.\r\n>... <size=200%><color=red>Pull, don't drop.</color></size>",
            $">... Nearly there.\r\n>... One last {itemName} short.\r\n>... <size=200%><color=red>Search start to end.</color></size>",
            $">... Locker name scratched.\r\n>... Number still readable.\r\n>... <size=200%><color=red>Matches the manifest.</color></size>",
            $">... Final sweep.\r\n>... Any {itemName}'s left, now's the time.\r\n>... <size=200%><color=red>Finish it.</color></size>",
            $">... Stash behind signage.\r\n>... Tape residue fresh.\r\n>... <size=200%><color=red>Peel and check.</color></size>",
            $">... Good haul.\r\n>... {itemName}'s secured.\r\n>... <size=200%><color=red>On to extraction.</color></size>",

            // Mid-line highlighted intel
            $">... Sweep for any {itemName}'s along this wall.\r\n>... <size=200%><color=red>Flag and bag quickly.</color></size>\r\n>... Move to the next aisle.",
            $">... Check under the catwalk supports.\r\n>... <size=200%><color=red>Small cases love to hide low.</color></size>\r\n>... Don't miss the corners.",
            $">... Quiet steps—sleepers nearby.\r\n>... <size=200%><color=red>If you see {itemName}'s, take them.</color></size>\r\n>... Call it in after.",
            $">... Rack endcaps look untouched.\r\n>... <size=200%><color=red>Pop every bin you see.</color></size>\r\n>... Fast hands, soft noise.",
            $">... Terminal ping is faint.\r\n>... <size=200%><color=red>Could be {itemName}'s behind the bulkhead.</color></size>\r\n>... Find the key and push.",
            $">... Watch the floor grates.\r\n>... <size=200%><color=red>Thin cases slip underneath.</color></size>\r\n>... Pry them up carefully.",
            $">... Staging point marked on map.\r\n>... <size=200%><color=red>Runner ferries {itemName}'s back.</color></size>\r\n>... Keep the line moving.",
            $">... Check the desk sides, not just drawers.\r\n>... <size=200%><color=red>People tape loot underneath.</color></size>\r\n>... Rip the tape, slow.",
            $">... Lights to half power.\r\n>... <size=200%><color=red>Glare hides {itemName} labels.</color></size>\r\n>... Read twice before skipping.",
            $">... Watch that shelf shadow.\r\n>... <size=200%><color=red>Flat cases blend perfectly.</color></size>\r\n>... Brush away dust to see.",
            $">... Locker bank ahead.\r\n>... <size=200%><color=red>Call out if you spot {itemName}'s.</color></size>\r\n>... I'll bag; you cover.",
            $">... Vent intake is loose.\r\n>... <size=200%><color=red>There's room for a small crate.</color></size>\r\n>... Reach in slow.",
            $">... Under those cable trays.\r\n>... <size=200%><color=red>Check for spilled {itemName}'s.</color></size>\r\n>... Shine your light and sweep.",
            $">... Tarp pile by the door.\r\n>... <size=200%><color=red>Something's boxed under there.</color></size>\r\n>... Lift, don't yank.",
            $">... Map says two remaining.\r\n>... <size=200%><color=red>Any {itemName}'s still count.</color></size>\r\n>... Hit the far row next.",
            $">... Keep comms clear.\r\n>... <size=200%><color=red>Only call when you've secured it.</color></size>\r\n>... Then move on.",
            $">... Workbench drawers first.\r\n>... <size=200%><color=red>High chance of {itemName}'s.</color></size>\r\n>... I'll cover your six.",
            $">... Check behind signage.\r\n>... <size=200%><color=red>Foam peels off clean.</color></size>\r\n>... Pry and pocket.",
            $">... Floor's sticky—watch your step.\r\n>... <size=200%><color=red>Don't kick a {itemName} out of reach.</color></size>\r\n>... Slow feet, quick eyes.",
            $">... Pallet stack looks recent.\r\n>... <size=200%><color=red>Slide the middle crate out.</color></size>\r\n>... Minimal noise.",
            $">... Filing cabinets—bottom row.\r\n>... <size=200%><color=red>Small tags match {itemName} manifest.</color></size>\r\n>... Open all the way.",
            $">... Server bay panel wiggles.\r\n>... <size=200%><color=red>Pop it; check the cavity.</color></size>\r\n>... Mind the cables.",
            $">... If you spot any {itemName}'s, grab them.\r\n>... <size=200%><color=red>No lectures—just take them.</color></size>\r\n>... We sort later.",
            $">... Dust rings mark missing bins.\r\n>... <size=200%><color=red>Follow the drag lines.</color></size>\r\n>... They end somewhere.",
            $">... Bring repellers.\r\n>... <size=200%><color=red>Fog hides {itemName} stickers.</color></size>\r\n>... Clear the air, then search.",
            $">... Quiet on tools.\r\n>... <size=200%><color=red>Cracked plastic snaps loud.</color></size>\r\n>... Ease latches up.",
            $">... Check the start room again.\r\n>... <size=200%><color=red>We've missed {itemName}'s before.</color></size>\r\n>... Fresh eyes, same route.",
            $">... Aisle mirror helps.\r\n>... <size=200%><color=red>Read labels from the far side.</color></size>\r\n>... Saves footsteps.",
            $">... That crate corner's taped.\r\n>... <size=200%><color=red>Good sign for {itemName} storage.</color></size>\r\n>... Cut and open.",
            $">... Check the ceiling rack ends.\r\n>... <size=200%><color=red>Small bundles wedge there.</color></size>\r\n>... Hook them down gently.",
            $">... Don't overfill one bag.\r\n>... <size=200%><color=red>Distribute the {itemName}'s evenly.</color></size>\r\n>... Keep speed up.",
            $">... Power dip incoming.\r\n>... <size=200%><color=red>Use glowsticks near the floor.</color></size>\r\n>... Read the tags close.",
            $">... Look for fresh scuffs.\r\n>... <size=200%><color=red>Someone dragged {itemName}'s through here.</color></size>\r\n>... Follow the trail.",
            $">... Mop closet isn't empty.\r\n>... <size=200%><color=red>Box behind buckets—classic stash.</color></size>\r\n>... Grab and go.",
            $">... We're light by three.\r\n>... <size=200%><color=red>Any {itemName}'s fix that.</color></size>\r\n>... Stick to the grid.",
            $">... Drawer has a false bottom.\r\n>... <size=200%><color=red>Feel for the lip and lift.</color></size>\r\n>... Nice and slow.",
            $">... Under the pallet slats.\r\n>... <size=200%><color=red>Shine for {itemName} reflections.</color></size>\r\n>... Pull with pliers.",
            $">... Shelf label says ‘misc'.\r\n>... <size=200%><color=red>That usually means everything.</color></size>\r\n>... Open all bins.",
            $">... Keep a runner free.\r\n>... <size=200%><color=red>Hot-swap as {itemName}'s pile up.</color></size>\r\n>... No bottlenecks.",
            $">... Check behind the breaker panel.\r\n>... <size=200%><color=red>Shallow cavity fits a case.</color></size>\r\n>... Don't touch the mains.",
            $">... Terminal shows cluster ahead.\r\n>... <size=200%><color=red>Expect multiple {itemName}'s together.</color></size>\r\n>... Move quiet and fast.",
            $">... Catwalk underside has magnets.\r\n>... <size=200%><color=red>Cases stick there sometimes.</color></size>\r\n>... Feel along the beam.",
            $">... Check the bin lids.\r\n>... <size=200%><color=red>Loose {itemName}'s ride the top.</color></size>\r\n>... Don't dump loud.",
            $">... Unlabeled crate—pry corner first.\r\n>... <size=200%><color=red>Watch for brittle plastic.</color></size>\r\n>... Easy pressure.",
            $">... We've got two {itemName}'s so far.\r\n>... <size=200%><color=red>Need at least five.</color></size>\r\n>... Keep moving.",
            $">... Partition wall gap.\r\n>... <size=200%><color=red>See that glint? Small label.</color></size>\r\n>... Reach with hook.",
            $">... Don't hoard.\r\n>... <size=200%><color=red>Share {itemName}'s as we find them.</color></size>\r\n>... Team stays mobile.",
            $">... Check the crate handles.\r\n>... <size=200%><color=red>Sometimes they hide behind grips.</color></size>\r\n>... Slide fingers in.",
            $">... We're one short.\r\n>... <size=200%><color=red>Backtrack for stray {itemName}'s.</color></size>\r\n>... Start to finish.",
            $">... Long shelf run ahead.\r\n>... <size=200%><color=red>Scan barcode rows quickly.</color></size>\r\n>... Read aloud.",
            $">... Final sweep.\r\n>... <size=200%><color=red>Any {itemName}'s left—now or never.</color></size>\r\n>... Close it out."
        }))!);
        #endregion
    }
}
