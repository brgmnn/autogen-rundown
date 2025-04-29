using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Objectives.Reactor;

public class ReactorWave
{
    #region Internal Properties

    [JsonIgnore]
    internal bool IsFetchWave { get; set; } = false;

    [JsonIgnore]
    internal bool IsFogWave { get; set; } = false;

    #endregion

    #region Properties

    public double Warmup { get; set; } = 90.0;

    public double WarmupFail { get; set; } = 20.0;

    /// <summary>
    /// Duration of the wave in seconds
    /// </summary>
    public double Wave { get; set; } = 60.0;

    /// <summary>
    /// Duration given to input the verification code the first time.
    /// </summary>
    public double Verify { get; set; } = 0.0;

    /// <summary>
    /// Duration given to input the verification code if the last wave was failed.
    /// </summary>
    public double VerifyFail { get; set; } = 45.0;

    public bool VerifyInOtherZone { get; set; } = false;

    public int ZoneForVerification { get; set; } = 0;

    public List<ReactorEnemyWave> EnemyWaves { get; set; } = new();

    public List<WardenObjectiveEvent> Events { get; set; } = new();

    #endregion

    /// <summary>
    /// Recalculates the wave duration as well as rel spawn times for waves.
    ///
    /// Each wave is assigned a Duration, which is the estimated time required to clear that
    /// wave. Each wave also can be assigned a SpawnTime (in seconds), which is the start time
    /// after the reactor wave has started that the wave should begin spawning. The game
    /// supports overlaying waves on each other.
    ///
    /// GTFO uses SpawnTimeRel (not SpawnTime) to determine when to spawn the wave. This value
    /// is relative to the wave duration. As such this function is called to set the correct
    /// relative values for our desired exact second spawn times.
    /// </summary>
    public void RecalculateWaveSpawnTimes()
    {
        // Give 30s base time, plus additional time per wave, plus the max wave duration. This
        // should be a good amount of time to complete the wave.
        // Add a small random amount of time to throw off people trying to guess the enemies
        Wave = 30 +
               (EnemyWaves.Count * 10) +
               EnemyWaves.Max(wave => wave.Duration + wave.SpawnTime) +
               Generator.Random.Next(-3, 9);

        // Update each individual wave in the enemy waves to set the right SpawnTimeRel.
        foreach (var wave in EnemyWaves)
            wave.SpawnTimeRel = wave.SpawnTime / Wave;
    }

    public double GetPoints()
        => EnemyWaves.Select(wave => wave.Settings.PopulationPointsTotal).Sum();

    public static string ListToString(IEnumerable<ReactorWave> waves, string separator = ", ")
        => string.Join(separator, waves.Select(wave => wave.ToString()));

    public new string ToString()
        => $"ReactorWave {{ Points = {GetPoints()}, Duration = {Wave}, IsFetch = {IsFetchWave}, IsFog = {IsFogWave} }}";
}
