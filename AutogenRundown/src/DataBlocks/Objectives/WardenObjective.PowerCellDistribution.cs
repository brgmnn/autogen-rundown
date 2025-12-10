using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: PowerCellDistribution
 *
 *
 * Drop in with power cells and distribute them to generators in various zones.
 *
 * The power cells set with PowerCellsToDistribute are dropped in with you
 * automatically.
 *      Edit: THE POWER CELLS DO NOT APPEAR IN THE FIRST ZONE. YOU MUST PLACE THEM.
 *
 ***************************************************************************************************
 *      TODO List
 *
 *  - Interesting power cell placement requirements?
 */
public partial record WardenObjective
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void PreBuild_PowerCellDistribution(BuildDirector director, Level level)
    {
        // Fast version of this objective
        if (level.MainDirector.Objective is WardenObjectiveType.ReachKdsDeep)
        {
            PowerCellsToDistribute = director.Tier switch
            {
                "D" => Generator.Select(new List<(double, int)>
                {
                    (0.7, 1),
                    (0.3, 2)
                }),

                "E" => 2,

                _ => 1
            };

            return;
        }

        PowerCellsToDistribute = director.Tier switch
        {
            "A" => Generator.Between(1, 2),
            "B" => Generator.Between(1, 3),
            "C" => Generator.Between(2, 3),
            "D" => Generator.Between(3, 4),
            "E" => Generator.Between(3, 5),
            _ => 2
        };
    }

    public void Build_PowerCellDistribution(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Distribute Power Cells from the elevator cargo container to [ALL_ITEMS]");
        FindLocationInfo = "Locate the Generators and bring the Power Cells to them";
        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        if (!director.Bulkhead.HasFlag(Bulkhead.Main))
        {
            // Place the cells in the first zone of the bulkhead if we are not in Main
            var node = level.Planner.GetZones(director.Bulkhead).First();
            var zone = level.Planner.GetZone(node)!;

            switch (PowerCellsToDistribute)
            {
                case 1:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;
                    break;
                case 2:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_2.PersistentId;
                    break;
                case 3:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_3.PersistentId;
                    break;
                case 4:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_4.PersistentId;
                    break;
                case 5:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_5.PersistentId;
                    break;
                default:
                    Plugin.Logger.LogError($"Unhandled number of power cells ({PowerCellsToDistribute}) to distribute");
                    throw new Exception($"Unhandled number of power cells to distribute");
            }
        }

        #region Warden Intel Messages

        var cells = PowerCellsToDistribute.ToCardinal();

        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
        {
            // Theme 1: Cell Count & Overwhelming Task (70 messages: 20 with PowerCellsToDistribute, 50 generic)
            $">... {cells.ToTitleCase()} cells total!\r\n>... <size=200%><color=red>That's a lot!</color></size>\r\n>... Get moving!",
            ">... Multiple generators!\r\n>... How many cells?\r\n>... <size=200%><color=red>Check the terminal!</color></size>",
            $">... Need to distribute {cells} cells!\r\n>... <size=200%><color=red>Split up!</color></size>\r\n>... Cover more ground!",
            ">... How many cells?\r\n>... <size=200%><color=red>Count them!</color></size>\r\n>... List on the terminal!",
            $">... {cells.ToTitleCase()} cells to power!\r\n>... Multiple generators!\r\n>... <size=200%><color=red>This is huge!</color></size>",
            ">... So many generators!\r\n>... <size=200%><color=red>How many cells?!</color></size>\r\n>... Check LIST CELL!",
            $">... We need {cells} cells!\r\n>... <size=200%><color=red>That's the mission!</color></size>\r\n>... Find them all!",
            ">... Multiple cells to find!\r\n>... Multiple generators!\r\n>... <size=200%><color=red>This'll take time!</color></size>",
            $">... {cells.ToTitleCase()} cells!\r\n>... <size=200%><color=red>Count's high!</color></size>\r\n>... Get started!",
            ">... How many?\r\n>... <size=200%><color=red>Check the objective!</color></size>\r\n>... Too many cells!",
            $">... Task requires {cells} cells!\r\n>... That's insane!\r\n>... <size=200%><color=red>Move fast!</color></size>",
            ">... Multiple generators!\r\n>... Cells scattered!\r\n>... <size=200%><color=red>Split the work!</color></size>",
            $">... {cells.ToTitleCase()} cells to distribute!\r\n>... <size=200%><color=red>All generators!</color></size>\r\n>... Every single one!",
            ">... Too many cells!\r\n>... <size=200%><color=red>This is overwhelming!</color></size>\r\n>... Stay focused!",
            $">... Count's {cells}!\r\n>... That's the target!\r\n>... <size=200%><color=red>Get moving!</color></size>",
            ">... Multiple cells!\r\n>... Multiple generators!\r\n>... <size=200%><color=red>Big task!</color></size>",
            $">... {cells.ToTitleCase()} power cells!\r\n>... <size=200%><color=red>All of them!</color></size>\r\n>... Don't miss any!",
            ">... How many generators?\r\n>... <size=200%><color=red>Same as cells!</color></size>\r\n>... Check the count!",
            $">... Need {cells}!\r\n>... That's a lot!\r\n>... <size=200%><color=red>Coordinate!</color></size>",
            ">... Counting cells!\r\n>... <size=200%><color=red>Multiple locations!</color></size>\r\n>... Map them out!",
            ">... Several cells to move!\r\n>... <size=200%><color=red>This'll take multiple trips!</color></size>\r\n>... Plan the route!",
            ">... How many left?\r\n>... <size=200%><color=red>Count down!</color></size>\r\n>... Track progress!",
            ">... Still more cells!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... Not done yet!",
            ">... Another cell!\r\n>... How many total?\r\n>... <size=200%><color=red>Check the terminal!</color></size>",
            ">... Multiple generators waiting!\r\n>... <size=200%><color=red>All need power!</color></size>\r\n>... Get the cells!",
            ">... Too many to carry at once!\r\n>... <size=200%><color=red>Multiple trips!</color></size>\r\n>... One at a time!",
            ">... How many done?\r\n>... <size=200%><color=red>Count them!</color></size>\r\n>... Track completion!",
            ">... Still more generators!\r\n>... <size=200%><color=red>Not finished!</color></size>\r\n>... Keep working!",
            ">... Another generator!\r\n>... Another cell!\r\n>... <size=200%><color=red>This never ends!</color></size>",
            ">... Count the cells!\r\n>... <size=200%><color=red>Make sure we have all!</color></size>\r\n>... Don't lose track!",
            ">... How many remaining?\r\n>... Still several!\r\n>... <size=200%><color=red>Keep moving!</color></size>",
            ">... Multiple cells needed!\r\n>... <size=200%><color=red>Find them all!</color></size>\r\n>... Search everywhere!",
            ">... Big task!\r\n>... Multiple generators!\r\n>... <size=200%><color=red>All need cells!</color></size>",
            ">... Count's high!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... Work faster!",
            ">... Several more to go!\r\n>... <size=200%><color=red>Not done yet!</color></size>\r\n>... Keep pushing!",
            ">... Another one!\r\n>... How many left?\r\n>... <size=200%><color=red>Losing count!</color></size>",
            ">... Still more generators!\r\n>... Still more cells!\r\n>... <size=200%><color=red>This is huge!</color></size>",
            ">... Multiple trips needed!\r\n>... <size=200%><color=red>Can't do it all at once!</color></size>\r\n>... Plan it out!",
            ">... How many total?\r\n>... <size=200%><color=red>Check LIST CELL!</color></size>\r\n>... Get the count!",
            ">... Too many cells!\r\n>... Too many generators!\r\n>... <size=200%><color=red>Split the work!</color></size>",
            ">... Count them!\r\n>... <size=200%><color=red>All generators!</color></size>\r\n>... Every single one!",
            ">... Multiple cells remaining!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... Don't stop!",
            ">... Another generator waiting!\r\n>... Another cell needed!\r\n>... <size=200%><color=red>Get moving!</color></size>",
            ">... Still several left!\r\n>... <size=200%><color=red>Not finished!</color></size>\r\n>... Keep working!",
            ">... How many done?\r\n>... How many left?\r\n>... <size=200%><color=red>Track it!</color></size>",
            ">... Multiple generators!\r\n>... <size=200%><color=red>All waiting for power!</color></size>\r\n>... Get the cells!",
            ">... Count's high!\r\n>... Task is big!\r\n>... <size=200%><color=red>Coordinate!</color></size>",
            ">... Several more!\r\n>... <size=200%><color=red>Keep pushing!</color></size>\r\n>... Not done!",
            ">... Another cell!\r\n>... Another generator!\r\n>... <size=200%><color=red>This takes forever!</color></size>",
            ">... Too many to handle!\r\n>... <size=200%><color=red>Split the team!</color></size>\r\n>... Cover more ground!",
            ">... Multiple cells!\r\n>... Multiple generators!\r\n>... <size=200%><color=red>Big job!</color></size>",
            ">... How many left?\r\n>... <size=200%><color=red>Still several!</color></size>\r\n>... Keep moving!",
            ">... Count's overwhelming!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... Stay focused!",
            ">... Still more to do!\r\n>... <size=200%><color=red>Not finished!</color></size>\r\n>... Keep working!",
            ">... Another one!\r\n>... Another one!\r\n>... <size=200%><color=red>Never ends!</color></size>",
            ">... Multiple cells needed!\r\n>... <size=200%><color=red>Find them all!</color></size>\r\n>... Check the terminal!",
            ">... Task is huge!\r\n>... Multiple generators!\r\n>... <size=200%><color=red>All need power!</color></size>",
            ">... Count them up!\r\n>... <size=200%><color=red>Make sure!</color></size>\r\n>... Don't miss any!",
            ">... How many total?\r\n>... How many done?\r\n>... <size=200%><color=red>Track progress!</color></size>",
            ">... Still several left!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... Don't stop now!",
            ">... Multiple trips!\r\n>... Multiple cells!\r\n>... <size=200%><color=red>This'll take time!</color></size>",
            ">... Another generator waiting!\r\n>... <size=200%><color=red>Get the cell!</color></size>\r\n>... Move fast!",
            ">... Too many!\r\n>... <size=200%><color=red>Overwhelming!</color></size>\r\n>... Focus!",
            ">... Count's high!\r\n>... Task's big!\r\n>... <size=200%><color=red>Work together!</color></size>",
            ">... Still more!\r\n>... <size=200%><color=red>Not done!</color></size>\r\n>... Keep pushing!",
            ">... Multiple cells!\r\n>... Multiple generators!\r\n>... <size=200%><color=red>Big mission!</color></size>",
            ">... How many remaining?\r\n>... <size=200%><color=red>Several more!</color></size>\r\n>... Keep working!",
            ">... Another cell needed!\r\n>... Another generator!\r\n>... <size=200%><color=red>This is huge!</color></size>",
            ">... Count them!\r\n>... <size=200%><color=red>All of them!</color></size>\r\n>... Don't lose track!",
            ">... Too many cells!\r\n>... <size=200%><color=red>Split up!</color></size>\r\n>... Cover more ground!",

            // Theme 2: Carrying & Movement Restrictions (60 messages: 15 with PowerCellsToDistribute, 45 generic)
            ">... Cell's heavy!\r\n>... <size=200%><color=red>Can't sprint!</color></size>\r\n>... Moving slow!",
            ">... This is slowing me down!\r\n>... Need to drop it!\r\n>... <size=200%><color=red>Fight first!</color></size>",
            ">... Carrying a cell!\r\n>... Walking speed only!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            $">... {cells.ToTitleCase()} cells to carry!\r\n>... <size=200%><color=red>Multiple trips!</color></size>\r\n>... One at a time!",
            ">... Can't sprint with this!\r\n>... <size=200%><color=red>Too heavy!</color></size>\r\n>... Moving slow!",
            ">... Cell's weighing me down!\r\n>... <size=200%><color=red>Walk speed!</color></size>\r\n>... Stay close!",
            $">... Need to carry {cells}!\r\n>... Can't do it all at once!\r\n>... <size=200%><color=red>Multiple trips!</color></size>",
            ">... This is heavy!\r\n>... <size=200%><color=red>Moving slow!</color></size>\r\n>... [grunting]",
            ">... Can't run with this!\r\n>... Walking only!\r\n>... <size=200%><color=red>I'm slow!</color></size>",
            $">... Carrying for {cells} generators!\r\n>... <size=200%><color=red>Takes forever!</color></size>\r\n>... So slow!",
            ">... Cell's restricting me!\r\n>... <size=200%><color=red>Can't sprint!</color></size>\r\n>... Move carefully!",
            ">... Heavy load!\r\n>... Walking speed!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            $">... {cells.ToTitleCase()} trips needed!\r\n>... <size=200%><color=red>One at a time!</color></size>\r\n>... Can't carry multiple!",
            ">... This is weighing me down!\r\n>... <size=200%><color=red>So slow!</color></size>\r\n>... Stay with me!",
            ">... Can't move fast!\r\n>... Carrying a cell!\r\n>... <size=200%><color=red>Watch my back!</color></size>",
            $">... All {cells} cells are heavy!\r\n>... <size=200%><color=red>Multiple slow trips!</color></size>\r\n>... This'll take time!",
            ">... Slowing me down!\r\n>... <size=200%><color=red>Can't sprint!</color></size>\r\n>... Need cover!",
            ">... Cell's heavy!\r\n>... Walk speed only!\r\n>... <size=200%><color=red>I'm vulnerable!</color></size>",
            $">... Need {cells} trips!\r\n>... Can't carry them all!\r\n>... <size=200%><color=red>One by one!</color></size>",
            ">... This is restricting!\r\n>... <size=200%><color=red>Moving slow!</color></size>\r\n>... Stay close!",
            ">... Heavy cell!\r\n>... Can't run!\r\n>... <size=200%><color=red>Protect me!</color></size>",
            $">... {cells.ToTitleCase()} heavy cells!\r\n>... <size=200%><color=red>So many trips!</color></size>\r\n>... This takes forever!",
            ">... Weighing me down!\r\n>... <size=200%><color=red>Walk speed!</color></size>\r\n>... Cover me!",
            ">... Can't sprint!\r\n>... Carrying a cell!\r\n>... <size=200%><color=red>I'm slow!</color></size>",
            $">... Each of {cells} is heavy!\r\n>... <size=200%><color=red>No sprinting!</color></size>\r\n>... Walk them over!",
            ">... Cell's restricting me!\r\n>... Moving slow!\r\n>... <size=200%><color=red>Watch my back!</color></size>",
            ">... Heavy load!\r\n>... <size=200%><color=red>Can't run!</color></size>\r\n>... Need escort!",
            $">... Carrying {cells} total!\r\n>... One at a time!\r\n>... <size=200%><color=red>Multiple slow trips!</color></size>",
            ">... This is slowing me!\r\n>... <size=200%><color=red>Walk speed!</color></size>\r\n>... Stay with me!",
            ">... Can't move fast!\r\n>... Cell's heavy!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            ">... Dropping it!\r\n>... <size=200%><color=red>Need to fight!</color></size>\r\n>... Pick up after!",
            ">... Had to drop the cell!\r\n>... Fighting them!\r\n>... <size=200%><color=red>Get it after!</color></size>",
            ">... Cell down!\r\n>... <size=200%><color=red>Fight first!</color></size>\r\n>... Retrieve later!",
            ">... Setting it down!\r\n>... Need my speed!\r\n>... <size=200%><color=red>Clear the area!</color></size>",
            ">... Dropped the cell!\r\n>... <size=200%><color=red>Kill them!</color></size>\r\n>... Then pickup!",
            ">... Had to drop it!\r\n>... Too many enemies!\r\n>... <size=200%><color=red>Fight back!</color></size>",
            ">... Cell's down!\r\n>... <size=200%><color=red>Clear the zone!</color></size>\r\n>... Get it after!",
            ">... Dropping it!\r\n>... Need to fight!\r\n>... <size=200%><color=red>Retrieve after!</color></size>",
            ">... Set it down!\r\n>... <size=200%><color=red>Fight them!</color></size>\r\n>... Pick up later!",
            ">... Had to drop!\r\n>... Fighting!\r\n>... <size=200%><color=red>Get it after!</color></size>",
            ">... Cell dropped!\r\n>... <size=200%><color=red>Kill them all!</color></size>\r\n>... Then secure!",
            ">... Setting down!\r\n>... Too many!\r\n>... <size=200%><color=red>Fight first!</color></size>",
            ">... Dropped it!\r\n>... <size=200%><color=red>Clear the area!</color></size>\r\n>... Pick up after!",
            ">... Cell's down!\r\n>... Fighting!\r\n>... <size=200%><color=red>Retrieve later!</color></size>",
            ">... Had to set it down!\r\n>... <size=200%><color=red>Too many of them!</color></size>\r\n>... Fight back!",
            ">... Dropping the cell!\r\n>... Need to fight!\r\n>... <size=200%><color=red>Get it after!</color></size>",
            ">... Cell down!\r\n>... <size=200%><color=red>Kill them!</color></size>\r\n>... Secure after!",
            ">... Set it down!\r\n>... Fight them!\r\n>... <size=200%><color=red>Retrieve after!</color></size>",
            ">... Dropped!\r\n>... <size=200%><color=red>Clear the zone!</color></size>\r\n>... Get it later!",
            ">... Had to drop it!\r\n>... Fighting!\r\n>... <size=200%><color=red>Pick up after!</color></size>",
            ">... Cell dropped!\r\n>... <size=200%><color=red>Fight first!</color></size>\r\n>... Retrieve later!",
            ">... Setting down!\r\n>... Too many!\r\n>... <size=200%><color=red>Clear them!</color></size>",
            ">... Dropped the cell!\r\n>... <size=200%><color=red>Fight back!</color></size>\r\n>... Get it after!",
            ">... Cell's down!\r\n>... Killing them!\r\n>... <size=200%><color=red>Secure after!</color></size>",
            ">... Had to drop!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... Fight them!",

            // Theme 3: Terminal & Navigation (50 messages: 10 with PowerCellsToDistribute, 40 generic)
            ">... Check the terminal!\r\n>... <size=200%><color=red>LIST CELL!</color></size>\r\n>... Find them!",
            ">... Where are the generators?\r\n>... LIST GENERATOR!\r\n>... <size=200%><color=red>Map them out!</color></size>",
            $">... {cells.ToTitleCase()} cells to find!\r\n>... <size=200%><color=red>Use the terminal!</color></size>\r\n>... [typing]",
            ">... Terminal query!\r\n>... <size=200%><color=red>LIST CELL!</color></size>\r\n>... Get locations!",
            ">... Where are they?\r\n>... Check LIST!\r\n>... <size=200%><color=red>Find the cells!</color></size>",
            $">... Need locations for {cells}!\r\n>... <size=200%><color=red>Use terminal!</color></size>\r\n>... Query it!",
            ">... LIST GENERATOR!\r\n>... <size=200%><color=red>Map the route!</color></size>\r\n>... Plan the path!",
            ">... Check the terminal!\r\n>... <size=200%><color=red>Find the cells!</color></size>\r\n>... [typing]",
            $">... {cells.ToTitleCase()} cells!\r\n>... <size=200%><color=red>Query locations!</color></size>\r\n>... Use LIST CELL!",
            ">... Terminal commands!\r\n>... LIST CELL!\r\n>... <size=200%><color=red>Get the zones!</color></size>",
            ">... Where are generators?\r\n>... <size=200%><color=red>Check terminal!</color></size>\r\n>... Map them!",
            $">... Locating {cells}!\r\n>... <size=200%><color=red>Terminal query!</color></size>\r\n>... [typing]",
            ">... LIST CELL command!\r\n>... <size=200%><color=red>Find locations!</color></size>\r\n>... Check zones!",
            ">... Query the terminal!\r\n>... <size=200%><color=red>LIST GENERATOR!</color></size>\r\n>... Get the map!",
            $">... Need to find {cells}!\r\n>... Terminal locations!\r\n>... <size=200%><color=red>Query now!</color></size>",
            ">... Check LIST!\r\n>... <size=200%><color=red>Cell locations!</color></size>\r\n>... Map the route!",
            ">... Terminal access!\r\n>... LIST CELL!\r\n>... <size=200%><color=red>Find them!</color></size>",
            $">... Locating all {cells}!\r\n>... <size=200%><color=red>Use the terminal!</color></size>\r\n>... Get zones!",
            ">... LIST GENERATOR command!\r\n>... <size=200%><color=red>Map them out!</color></size>\r\n>... Plan route!",
            ">... Query terminal!\r\n>... <size=200%><color=red>Cell locations!</color></size>\r\n>... [typing]",
            ">... Where are they?\r\n>... Use LIST CELL!\r\n>... <size=200%><color=red>Get locations!</color></size>",
            ">... Terminal query!\r\n>... <size=200%><color=red>LIST GENERATOR!</color></size>\r\n>... Map the zones!",
            ">... Find the cells!\r\n>... <size=200%><color=red>Check terminal!</color></size>\r\n>... Query locations!",
            ">... LIST command!\r\n>... Cell locations!\r\n>... <size=200%><color=red>Get the zones!</color></size>",
            ">... Where are generators?\r\n>... <size=200%><color=red>Terminal query!</color></size>\r\n>... Map them!",
            ">... Use LIST CELL!\r\n>... <size=200%><color=red>Find locations!</color></size>\r\n>... Plan route!",
            ">... Terminal access!\r\n>... <size=200%><color=red>Query generators!</color></size>\r\n>... Map the path!",
            ">... Check LIST!\r\n>... <size=200%><color=red>Cell zones!</color></size>\r\n>... Get locations!",
            ">... LIST GENERATOR!\r\n>... <size=200%><color=red>Map the route!</color></size>\r\n>... Plan it out!",
            ">... Query the terminal!\r\n>... Cell locations!\r\n>... <size=200%><color=red>Find them!</color></size>",
            ">... Use terminal!\r\n>... <size=200%><color=red>LIST CELL!</color></size>\r\n>... Get zones!",
            ">... Terminal commands!\r\n>... <size=200%><color=red>Query locations!</color></size>\r\n>... Map them!",
            ">... Find generators!\r\n>... LIST command!\r\n>... <size=200%><color=red>Map zones!</color></size>",
            ">... Check terminal!\r\n>... <size=200%><color=red>Cell locations!</color></size>\r\n>... Query now!",
            ">... LIST CELL!\r\n>... <size=200%><color=red>Find zones!</color></size>\r\n>... Plan route!",
            ">... Terminal query!\r\n>... Generator locations!\r\n>... <size=200%><color=red>Map them!</color></size>",
            ">... Use LIST!\r\n>... <size=200%><color=red>Cell zones!</color></size>\r\n>... Get locations!",
            ">... Query terminal!\r\n>... <size=200%><color=red>Find generators!</color></size>\r\n>... Map route!",
            ">... LIST GENERATOR!\r\n>... <size=200%><color=red>Get zones!</color></size>\r\n>... Plan path!",
            ">... Terminal access!\r\n>... Cell locations!\r\n>... <size=200%><color=red>Query now!</color></size>",
            ">... Check LIST CELL!\r\n>... <size=200%><color=red>Find them!</color></size>\r\n>... Map zones!",
            ">... Query generators!\r\n>... <size=200%><color=red>Terminal access!</color></size>\r\n>... Get locations!",
            ">... Use terminal!\r\n>... <size=200%><color=red>LIST commands!</color></size>\r\n>... Find cells!",
            ">... LIST CELL command!\r\n>... <size=200%><color=red>Get zones!</color></size>\r\n>... Map route!",
            ">... Query terminal!\r\n>... <size=200%><color=red>Find locations!</color></size>\r\n>... Plan path!",
            ">... Terminal commands!\r\n>... LIST GENERATOR!\r\n>... <size=200%><color=red>Map zones!</color></size>",
            ">... Check LIST!\r\n>... <size=200%><color=red>Cell locations!</color></size>\r\n>... Query now!",
            ">... Use terminal!\r\n>... <size=200%><color=red>Find generators!</color></size>\r\n>... Map them!",
            ">... LIST query!\r\n>... <size=200%><color=red>Get zones!</color></size>\r\n>... Find cells!",
            ">... Terminal access!\r\n>... <size=200%><color=red>Query locations!</color></size>\r\n>... Map route!",

            // Theme 4: Generator Activation & Branches (50 messages: 10 with PowerCellsToDistribute, 40 generic)
            ">... Generator's down this branch!\r\n>... <size=200%><color=red>Long walk!</color></size>\r\n>... Stay alert!",
            ">... Which generator next?\r\n>... <size=200%><color=red>Check the map!</color></size>\r\n>... Plan the route!",
            ">... Activating generator!\r\n>... Cell inserted!\r\n>... <size=200%><color=red>It's online!</color></size>",
            $">... {cells.ToTitleCase()} generators total!\r\n>... <size=200%><color=red>All need power!</color></size>\r\n>... Get the cells!",
            ">... Branch path!\r\n>... Generator at the end!\r\n>... <size=200%><color=red>Long walk!</color></size>",
            ">... Inserting cell!\r\n>... <size=200%><color=red>Generator activating!</color></size>\r\n>... [powering up]",
            $">... Need to power {cells}!\r\n>... <size=200%><color=red>All branches!</color></size>\r\n>... Split up!",
            ">... Generator location!\r\n>... End of the branch!\r\n>... <size=200%><color=red>Navigate there!</color></size>",
            ">... Which branch?\r\n>... <size=200%><color=red>Check the map!</color></size>\r\n>... Plan route!",
            $">... {cells.ToTitleCase()} branches!\r\n>... One generator each!\r\n>... <size=200%><color=red>Split the work!</color></size>",
            ">... Cell inserted!\r\n>... <size=200%><color=red>Generator online!</color></size>\r\n>... One down!",
            ">... Branch navigation!\r\n>... <size=200%><color=red>Generator at end!</color></size>\r\n>... Long path!",
            $">... Powering {cells} generators!\r\n>... <size=200%><color=red>All branches!</color></size>\r\n>... Get moving!",
            ">... Generator's far!\r\n>... End of branch!\r\n>... <size=200%><color=red>Keep walking!</color></size>",
            ">... Activating!\r\n>... <size=200%><color=red>Cell inserted!</color></size>\r\n>... [humming]",
            $">... {cells.ToTitleCase()} to activate!\r\n>... <size=200%><color=red>All generators!</color></size>\r\n>... Power them!",
            ">... Which branch next?\r\n>... <size=200%><color=red>Plan the route!</color></size>\r\n>... Check map!",
            ">... Generator powered!\r\n>... <size=200%><color=red>One done!</color></size>\r\n>... Next one!",
            $">... All {cells} generators!\r\n>... <size=200%><color=red>Branch by branch!</color></size>\r\n>... Systematic!",
            ">... Branch path ahead!\r\n>... <size=200%><color=red>Generator waiting!</color></size>\r\n>... Navigate there!",
            ">... Cell in place!\r\n>... <size=200%><color=red>Activating!</color></size>\r\n>... [powering]",
            ">... Generator location!\r\n>... Branch terminus!\r\n>... <size=200%><color=red>Get there!</color></size>",
            ">... Activating generator!\r\n>... <size=200%><color=red>Powering up!</color></size>\r\n>... [mechanical sounds]",
            ">... Which branch?\r\n>... <size=200%><color=red>Next generator!</color></size>\r\n>... Plan it!",
            ">... Cell delivered!\r\n>... <size=200%><color=red>Generator online!</color></size>\r\n>... Next one!",
            ">... Branch navigation!\r\n>... <size=200%><color=red>Long walk!</color></size>\r\n>... Stay alert!",
            ">... Generator's far!\r\n>... <size=200%><color=red>End of branch!</color></size>\r\n>... Keep moving!",
            ">... Inserting cell!\r\n>... <size=200%><color=red>Activating!</color></size>\r\n>... [humming]",
            ">... Branch path!\r\n>... Generator waiting!\r\n>... <size=200%><color=red>Navigate there!</color></size>",
            ">... Cell in generator!\r\n>... <size=200%><color=red>Powering up!</color></size>\r\n>... One done!",
            ">... Which generator?\r\n>... <size=200%><color=red>Check the map!</color></size>\r\n>... Next branch!",
            ">... Generator powered!\r\n>... <size=200%><color=red>Next one!</color></size>\r\n>... Keep going!",
            ">... Branch terminus!\r\n>... <size=200%><color=red>Generator here!</color></size>\r\n>... Insert cell!",
            ">... Activating!\r\n>... <size=200%><color=red>Generator online!</color></size>\r\n>... [powering]",
            ">... Next branch!\r\n>... Next generator!\r\n>... <size=200%><color=red>Keep moving!</color></size>",
            ">... Cell inserted!\r\n>... <size=200%><color=red>Powering up!</color></size>\r\n>... One complete!",
            ">... Generator location!\r\n>... <size=200%><color=red>Branch end!</color></size>\r\n>... Navigate!",
            ">... Which one next?\r\n>... <size=200%><color=red>Plan the route!</color></size>\r\n>... Check map!",
            ">... Powering generator!\r\n>... <size=200%><color=red>Cell in place!</color></size>\r\n>... [humming]",
            ">... Branch path ahead!\r\n>... <size=200%><color=red>Generator waiting!</color></size>\r\n>... Get there!",
            ">... Generator active!\r\n>... <size=200%><color=red>Next one!</color></size>\r\n>... Move out!",
            ">... Branch terminus!\r\n>... <size=200%><color=red>Insert cell!</color></size>\r\n>... Activate!",
            ">... Cell delivered!\r\n>... <size=200%><color=red>Generator online!</color></size>\r\n>... Keep going!",
            ">... Next generator!\r\n>... Next branch!\r\n>... <size=200%><color=red>Plan route!</color></size>",
            ">... Activating!\r\n>... <size=200%><color=red>Powering up!</color></size>\r\n>... One done!",
            ">... Branch navigation!\r\n>... <size=200%><color=red>Generator ahead!</color></size>\r\n>... Stay alert!",
            ">... Cell in generator!\r\n>... <size=200%><color=red>Online!</color></size>\r\n>... Next one!",
            ">... Generator waiting!\r\n>... <size=200%><color=red>Branch end!</color></size>\r\n>... Navigate there!",
            ">... Powering up!\r\n>... <size=200%><color=red>Generator active!</color></size>\r\n>... [mechanical]",
            ">... Which branch next?\r\n>... <size=200%><color=red>Check map!</color></size>\r\n>... Plan it!",

            // Theme 5: Alarms & Combat (40 messages: 5 with PowerCellsToDistribute, 35 generic)
            ">... [alarm blaring]\r\n>... Generator triggered it!\r\n>... <size=200%><color=red>They're coming!</color></size>",
            ">... Activation alarm!\r\n>... <size=200%><color=red>Spawning!</color></size>\r\n>... [gunfire]",
            ">... Had to drop the cell!\r\n>... Fighting them!\r\n>... <size=200%><color=red>Clear the area!</color></size>",
            $">... {cells.ToTitleCase()} activations!\r\n>... <size=200%><color=red>Each triggers alarm!</color></size>\r\n>... Get ready!",
            ">... [klaxon wailing]\r\n>... Generator alarm!\r\n>... <size=200%><color=red>They're spawning!</color></size>",
            ">... Alarm triggered!\r\n>... <size=200%><color=red>Fight them!</color></size>\r\n>... Protect the cell!",
            $">... Every one of {cells}!\r\n>... <size=200%><color=red>Triggers alarm!</color></size>\r\n>... Be ready!",
            ">... Generator alarm!\r\n>... <size=200%><color=red>Enemies spawning!</color></size>\r\n>... [gunfire]",
            ">... [alarm]\r\n>... Activation triggered them!\r\n>... <size=200%><color=red>Fight back!</color></size>",
            $">... {cells.ToTitleCase()} alarms!\r\n>... <size=200%><color=red>Each activation!</color></size>\r\n>... Prepare!",
            ">... Alarm's going!\r\n>... Generator powered!\r\n>... <size=200%><color=red>They're coming!</color></size>",
            ">... [klaxon blaring]\r\n>... <size=200%><color=red>Spawning!</color></size>\r\n>... Fight them!",
            $">... All {cells} will alarm!\r\n>... <size=200%><color=red>Every single one!</color></size>\r\n>... Get ready!",
            ">... Generator triggered alarm!\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... [approaching]",
            ">... Alarm's blaring!\r\n>... <size=200%><color=red>Fight while carrying!</color></size>\r\n>... Keep moving!",
            ">... Activation alarm!\r\n>... <size=200%><color=red>Enemies incoming!</color></size>\r\n>... [gunfire]",
            ">... [alarm wailing]\r\n>... Triggered it!\r\n>... <size=200%><color=red>Fight back!</color></size>",
            ">... Generator alarm!\r\n>... <size=200%><color=red>Spawning!</color></size>\r\n>... Defend!",
            ">... Alarm triggered!\r\n>... Fighting!\r\n>... <size=200%><color=red>Drop the cell!</color></size>",
            ">... [klaxon]\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... Generator alarm!",
            ">... Activation triggered alarm!\r\n>... <size=200%><color=red>Fight them!</color></size>\r\n>... [gunfire]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Enemies spawning!</color></size>\r\n>... Defend!",
            ">... Generator alarm!\r\n>... <size=200%><color=red>Drop the cell!</color></size>\r\n>... Fight!",
            ">... Alarm's going!\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... [approaching]",
            ">... [klaxon wailing]\r\n>... Generator triggered it!\r\n>... <size=200%><color=red>Fight back!</color></size>",
            ">... Activation alarm!\r\n>... <size=200%><color=red>Spawning!</color></size>\r\n>... Defend the cell!",
            ">... [alarm]\r\n>... <size=200%><color=red>Enemies coming!</color></size>\r\n>... Fight them!",
            ">... Generator alarm!\r\n>... Fighting!\r\n>... <size=200%><color=red>Drop it!</color></size>",
            ">... Alarm triggered!\r\n>... <size=200%><color=red>They're spawning!</color></size>\r\n>... [gunfire]",
            ">... [klaxon blaring]\r\n>... <size=200%><color=red>Fight them!</color></size>\r\n>... Generator alarm!",
            ">... Activation triggered!\r\n>... <size=200%><color=red>Enemies incoming!</color></size>\r\n>... Defend!",
            ">... [alarm wailing]\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... Fight back!",
            ">... Generator alarm!\r\n>... <size=200%><color=red>Spawning!</color></size>\r\n>... Drop the cell!",
            ">... Alarm's going!\r\n>... <size=200%><color=red>Fight while carrying!</color></size>\r\n>... Stay moving!",
            ">... [klaxon]\r\n>... <size=200%><color=red>Enemies coming!</color></size>\r\n>... Activation alarm!",
            ">... Generator triggered alarm!\r\n>... <size=200%><color=red>Fight them!</color></size>\r\n>... [gunfire]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>They're spawning!</color></size>\r\n>... Defend!",
            ">... Activation alarm!\r\n>... <size=200%><color=red>Drop the cell!</color></size>\r\n>... Fight!",
            ">... [klaxon wailing]\r\n>... <size=200%><color=red>Enemies incoming!</color></size>\r\n>... Generator alarm!",
            ">... Alarm triggered!\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... Fight back!",

            // Theme 6: Team Coordination (30 messages: 0 with PowerCellsToDistribute, 30 generic)
            ">... Split up!\r\n>... <size=200%><color=red>Cover more generators!</color></size>\r\n>... Meet back at hub!",
            ">... You take that branch!\r\n>... I'll get this one!\r\n>... <size=200%><color=red>Stay in contact!</color></size>",
            ">... Two people per cell!\r\n>... <size=200%><color=red>Cover each other!</color></size>\r\n>... Move out!",
            ">... Divide the work!\r\n>... <size=200%><color=red>Split the team!</color></size>\r\n>... More efficient!",
            ">... You get that generator!\r\n>... <size=200%><color=red>I'll take this one!</color></size>\r\n>... Coordinate!",
            ">... Team split!\r\n>... Cover different branches!\r\n>... <size=200%><color=red>Meet at hub!</color></size>",
            ">... Divide the cells!\r\n>... <size=200%><color=red>Split the work!</color></size>\r\n>... Faster!",
            ">... You take left branch!\r\n>... <size=200%><color=red>I'll go right!</color></size>\r\n>... Stay in contact!",
            ">... Two on cells!\r\n>... Two on defense!\r\n>... <size=200%><color=red>Coordinate!</color></size>",
            ">... Split the team!\r\n>... <size=200%><color=red>Cover more ground!</color></size>\r\n>... Be efficient!",
            ">... You get that one!\r\n>... <size=200%><color=red>I'll take this!</color></size>\r\n>... Work together!",
            ">... Divide the branches!\r\n>... <size=200%><color=red>Split up!</color></size>\r\n>... Meet back!",
            ">... Two per generator!\r\n>... <size=200%><color=red>Cover each other!</color></size>\r\n>... Stay safe!",
            ">... You go there!\r\n>... <size=200%><color=red>I'll go here!</color></size>\r\n>... Coordinate!",
            ">... Team split!\r\n>... <size=200%><color=red>Different branches!</color></size>\r\n>... Meet at hub!",
            ">... Divide the work!\r\n>... <size=200%><color=red>Split the team!</color></size>\r\n>... Be efficient!",
            ">... You take that generator!\r\n>... <size=200%><color=red>I'll get this one!</color></size>\r\n>... Work together!",
            ">... Split up!\r\n>... <size=200%><color=red>Cover more cells!</color></size>\r\n>... Meet back!",
            ">... Two on cells!\r\n>... <size=200%><color=red>Two on escort!</color></size>\r\n>... Coordinate!",
            ">... You go left!\r\n>... <size=200%><color=red>I'll go right!</color></size>\r\n>... Stay in contact!",
            ">... Divide the branches!\r\n>... <size=200%><color=red>Split the work!</color></size>\r\n>... Faster!",
            ">... Team split!\r\n>... <size=200%><color=red>Cover different generators!</color></size>\r\n>... Meet up!",
            ">... You get that one!\r\n>... <size=200%><color=red>I'll take this!</color></size>\r\n>... Be efficient!",
            ">... Split the team!\r\n>... <size=200%><color=red>Cover more ground!</color></size>\r\n>... Work together!",
            ">... Two per branch!\r\n>... <size=200%><color=red>Cover each other!</color></size>\r\n>... Coordinate!",
            ">... You go there!\r\n>... <size=200%><color=red>I'll go here!</color></size>\r\n>... Meet back!",
            ">... Divide the work!\r\n>... <size=200%><color=red>Split up!</color></size>\r\n>... More efficient!",
            ">... You take that branch!\r\n>... <size=200%><color=red>I'll take this!</color></size>\r\n>... Stay safe!",
            ">... Team split!\r\n>... <size=200%><color=red>Different generators!</color></size>\r\n>... Meet at hub!",
            ">... Split the team!\r\n>... <size=200%><color=red>Cover more branches!</color></size>\r\n>... Coordinate!",
        }))!);
        #endregion
    }
}
