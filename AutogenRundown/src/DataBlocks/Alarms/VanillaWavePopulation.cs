namespace AutogenRundown.DataBlocks.Alarms
{
    /// <summary>
    /// Persistent IDs for Survival Wave Populations
    /// </summary>
    enum VanillaWavePopulation : uint
    {
        None = 0,

        // Regular enemy pops
        Baseline = 1,
        Shooters = 3,
        BigStrikers = 4,
        Chargers = 5, // Called Bullrush
        Shadows = 7,
        Strikers = 18,

        // Shadows
        Shadows_BigsOnly = 38,

        BaselineHybrid = 8,
        BaselineHybrid_M_NoShooter = 11,
        Baseline_Sp_Shadows = 29,

        ModifiedSpHybrid = 9,
        ModifiedSpStrikerBig = 21,

        // Chargers [x] Confirmed
        Bullrush_mix = 17,
        Bullrush_mix2 = 19,
        Bullrush_mix3 = 24,
        Bullrush_mix4 = 13,
        Bullrush_mix5 = 14,
        Bullrush_mix6 = 36,
        Bullrush_mix7 = 40,
        BullrushBigs = 15,
        Baseline_M_Bullrush = 23, // M = maybe? medium?
        Bullrush_Only = 47,

        // Bigs
        BigsAndBosses = 22,
        BigsAndBosses_M_Hybrid = 25,
        BigsAndHybrid = 28,
        BigsAndBosses_v2 = 33,

        // Mother?
        BaselineBirther_MB = 12,
        Birther = 31,

        // Tank
        Tank = 16,

        // Grabber, Pouncer
        Pouncer = 56,
        WavePouncer = 39,
        WavePouncerCombo = 45,

        BigsAndBosses_S_Hybrid = 20, // S = small?

        // Reactor
        ReactorBaseline = 6,
        ReactorBaselineHybrid = 10,
        ReactorBaseline_S_Child = 44,

        // Flyers
        Flyers = 35,
        FlyersBig = 37,
        Boss_Flyer = 26,
        Modified_S_Flyer = 27
    }
}
