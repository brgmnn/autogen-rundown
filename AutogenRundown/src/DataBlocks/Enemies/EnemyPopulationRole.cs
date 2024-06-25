namespace AutogenRundown.DataBlocks.Enemies
{
    /// <summary>
    /// Maps Role/Difficulty matches to enemies using persistentIDs from EnemyDataBlock.
    ///
    /// Largely the game randomly picks from all matches in this list that have the same
    /// Role/Difficulty.
    /// </summary>
    public record class EnemyPopulationRole
    {
        public uint Role { get; set; }

        public uint Difficulty { get; set; }

        public Enemy Enemy { get; set; }

        public double Cost { get; set; }

        public double Weight { get; set; } = 1.0;

        /// <summary>
        /// File: GameData_EnemyPopulationDataBlock_bin.json
        /// </summary>
        public const string VanillaData = @"[
        {
          ""Role"": 0,
          ""Difficulty"": 0,
          ""Enemy"": 24,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 0,
          ""Difficulty"": 1,
          ""Enemy"": 24,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 0,
          ""Difficulty"": 1,
          ""Enemy"": 28,
          ""Cost"": 4.0,
          ""Weight"": 0.3
        },
        {
          ""Role"": 0,
          ""Difficulty"": 2,
          ""Enemy"": 28,
          ""Cost"": 4.0,
          ""Weight"": 0.3
        },
        {
          ""Role"": 0,
          ""Difficulty"": 2,
          ""Enemy"": 24,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 0,
          ""Difficulty"": 6,
          ""Enemy"": 21,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 0,
          ""Difficulty"": 7,
          ""Enemy"": 35,
          ""Cost"": 4.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 1,
          ""Difficulty"": 0,
          ""Enemy"": 26,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 1,
          ""Difficulty"": 1,
          ""Enemy"": 26,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 1,
          ""Difficulty"": 1,
          ""Enemy"": 18,
          ""Cost"": 4.0,
          ""Weight"": 0.2
        },
        {
          ""Role"": 1,
          ""Difficulty"": 2,
          ""Enemy"": 26,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 1,
          ""Difficulty"": 2,
          ""Enemy"": 18,
          ""Cost"": 4.0,
          ""Weight"": 0.2
        },
        {
          ""Role"": 5,
          ""Difficulty"": 0,
          ""Enemy"": 20,
          ""Cost"": 5.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 5,
          ""Difficulty"": 1,
          ""Enemy"": 20,
          ""Cost"": 5.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 5,
          ""Difficulty"": 2,
          ""Enemy"": 20,
          ""Cost"": 5.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 5,
          ""Difficulty"": 3,
          ""Enemy"": 41,
          ""Cost"": 5.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 5,
          ""Difficulty"": 4,
          ""Enemy"": 40,
          ""Cost"": 5.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 5,
          ""Difficulty"": 5,
          ""Enemy"": 54,
          ""Cost"": 5.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 5,
          ""Difficulty"": 6,
          ""Enemy"": 56,
          ""Cost"": 5.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 4,
          ""Difficulty"": 0,
          ""Enemy"": 28,
          ""Cost"": 3.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 4,
          ""Difficulty"": 1,
          ""Enemy"": 28,
          ""Cost"": 3.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 4,
          ""Difficulty"": 1,
          ""Enemy"": 18,
          ""Cost"": 3.0,
          ""Weight"": 0.2
        },
        {
          ""Role"": 4,
          ""Difficulty"": 2,
          ""Enemy"": 36,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 4,
          ""Difficulty"": 6,
          ""Enemy"": 33,
          ""Cost"": 3.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 4,
          ""Difficulty"": 4,
          ""Enemy"": 44,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 4,
          ""Difficulty"": 5,
          ""Enemy"": 29,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 4,
          ""Difficulty"": 3,
          ""Enemy"": 37,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 8,
          ""Difficulty"": 7,
          ""Enemy"": 38,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 8,
          ""Difficulty"": 0,
          ""Enemy"": 38,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 8,
          ""Difficulty"": 6,
          ""Enemy"": 42,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 7,
          ""Difficulty"": 0,
          ""Enemy"": 33,
          ""Cost"": 4.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 7,
          ""Difficulty"": 1,
          ""Enemy"": 33,
          ""Cost"": 4.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 7,
          ""Difficulty"": 2,
          ""Enemy"": 36,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 7,
          ""Difficulty"": 3,
          ""Enemy"": 37,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 7,
          ""Difficulty"": 5,
          ""Enemy"": 29,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 7,
          ""Difficulty"": 7,
          ""Enemy"": 16,
          ""Cost"": 4.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 7,
          ""Difficulty"": 6,
          ""Enemy"": 46,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 3,
          ""Difficulty"": 0,
          ""Enemy"": 30,
          ""Cost"": 2.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 3,
          ""Difficulty"": 1,
          ""Enemy"": 30,
          ""Cost"": 2.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 3,
          ""Difficulty"": 2,
          ""Enemy"": 39,
          ""Cost"": 4.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 10,
          ""Difficulty"": 4,
          ""Enemy"": 37,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 9,
          ""Difficulty"": 3,
          ""Enemy"": 36,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 9,
          ""Difficulty"": 5,
          ""Enemy"": 29,
          ""Cost"": 10.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 6,
          ""Difficulty"": 0,
          ""Enemy"": 30,
          ""Cost"": 1.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 3,
          ""Difficulty"": 6,
          ""Enemy"": 53,
          ""Cost"": 2.0,
          ""Weight"": 1.0
        },
        {
          ""Role"": 3,
          ""Difficulty"": 7,
          ""Enemy"": 52,
          ""Cost"": 2.0,
          ""Weight"": 1.0
        }
      ]";
    }
}
