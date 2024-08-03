using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives.Reactor;

namespace AutogenRundown.DataBlocks;

public partial record class WardenObjective : DataBlock
{
    /// <summary>
    /// Reactor shutdown will result in the lights being off for the remainder of the
    /// level.Factor that as a difficulty modifier.
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void Build_ReactorShutdown(BuildDirector director, Level level)
    {
        MainObjective = "Find the main reactor and shut it down";
        FindLocationInfo = "Gather information about the location of the Reactor";
        GoToZone = "Navigate to [ITEM_ZONE] and initiate the shutdown process";
        SolveItem = "Make sure the Reactor is fully shut down before leaving";
        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_ToMainLayer = "Go back to the main objective and complete the expedition.";

        LightsOnFromBeginning = true;
        LightsOnDuringIntro = true;
        LightsOnWhenStartupComplete = false;

        StartPuzzle = ChainedPuzzle.FindOrPersist(ChainedPuzzle.TeamScan);

        // TODO: Rework this
        var midScan = Generator.Pick(ChainedPuzzle.BuildReactorShutdownPack(director.Tier)) ?? ChainedPuzzle.AlarmClass5;

        MidPuzzle = ChainedPuzzle.FindOrPersist(midScan);

        // Seems we set these as empty?
        // TODO: can we remove these?
        ReactorWaves = new List<ReactorWave>
        {
            new ReactorWave
            {
                Warmup = 90.0,
                WarmupFail = 20.0,
                Wave = 60.0,
                Verify = 0.0,
                VerifyFail = 45.0
            },
            new ReactorWave
            {
                Warmup = 90.0,
                WarmupFail = 20.0,
                Wave = 60.0,
                Verify = 0.0,
                VerifyFail = 45.0
            },
            new ReactorWave
            {
                Warmup = 90.0,
                WarmupFail = 20.0,
                Wave = 60.0,
                Verify = 0.0,
                VerifyFail = 45.0
            }
        };
    }
}
