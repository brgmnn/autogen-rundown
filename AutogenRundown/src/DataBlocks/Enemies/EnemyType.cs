namespace AutogenRundown.DataBlocks.Enemies;

public enum EnemyType : uint
{
    Weakling = 0,
    Standard = 1,
    Special = 2,
    MiniBoss = 3,

    // Do not use this as it doesn't spawn anything but uses up points
    Boss = 4
}
