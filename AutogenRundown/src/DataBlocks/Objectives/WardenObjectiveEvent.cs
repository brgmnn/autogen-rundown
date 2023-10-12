using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutogenRundown.DataBlocks.Enemies;

namespace AutogenRundown.DataBlocks.Objectives
{
    internal class WardenObjectiveEvent
    {
        public static WardenObjectiveEvent LightsOff = new WardenObjectiveEvent { Type = WardenObjectiveEventType.AllLightsOff };

        public WardenObjectiveEventType Type { get; set; } = WardenObjectiveEventType.None;

        public JObject Condition = new JObject
        {
            ["ConditionIndex"] = -1,
            ["IsTrue"] = false
        };

        public WardenObjectiveEventTrigger Trigger { get; set; } = WardenObjectiveEventTrigger.None;

        /// <summary>
        /// Persistent Id for chain puzzle
        /// </summary>
        public uint ChainPuzzle { get; set; } = 0;

        public bool UseStaticBioscanPoints { get; set; } = false;

        public int LocalIndex { get; set; } = 0;

        public double Delay { get; set; } = 0.0;

        public double Duration { get; set; } = 0.0;

        public bool ClearDimension { get; set; } = false;

        public string WardenIntel { get; set; } = "";

        public string CustomSubObjectiveHeader { get; set; } = "";

        public string CustomSubObjective { get; set; } = "";

        #region Fog settings
        public uint FogSetting { get; set; } = 0;

        public double FogTransitionDuration { get; set; } = 0.0;
        #endregion

        #region Enemy settings
        public GenericWave EnemyWaveData { get; set; } = new GenericWave();

        [JsonProperty("EnemyID")]
        public uint EnemyId { get; set; } = 0;

        public Vector3 Position { get; set; } = new Vector3();

        public int Count { get; set; } = 0;

        public bool Enabled { get; set; } = false;
        #endregion

        #region Terminal
        public int TerminalCommand = 0;
        public int TerminalCommandRule = 0;
        #endregion

        #region Not implemented yet
        public int Layer { get; set; } = 0;
        public int DimensionIndex = 0;
        public uint SoundID = 0;
        public uint SoundSubtitle = 0;
        public uint DialogueID = 0;
        #endregion
    }
}
