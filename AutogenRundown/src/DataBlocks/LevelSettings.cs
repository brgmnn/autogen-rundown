using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    public class WeightedModifier : Generator.ISelectable
    {
        public LevelModifiers Modifier { get; set; }

        public double Weight { get; set; }
    }

    public class ModifiersSet : HashSet<LevelModifiers>
    {
        /// <summary>
        /// Handle removing conflicting modifiers
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public new bool Add(LevelModifiers modifier)
        {
            switch (modifier)
            {
                case LevelModifiers.NoChargers:
                case LevelModifiers.Chargers:
                case LevelModifiers.ManyChargers:
                case LevelModifiers.OnlyChargers:
                    Remove(LevelModifiers.NoChargers);
                    Remove(LevelModifiers.Chargers);
                    Remove(LevelModifiers.ManyChargers);
                    Remove(LevelModifiers.OnlyChargers);
                    break;

                case LevelModifiers.NoShadows:
                case LevelModifiers.Shadows:
                case LevelModifiers.ManyShadows:
                case LevelModifiers.OnlyShadows:
                    Remove(LevelModifiers.NoShadows);
                    Remove(LevelModifiers.Shadows);
                    Remove(LevelModifiers.ManyShadows);
                    Remove(LevelModifiers.OnlyShadows);
                    break;
            }

            return base.Add(modifier);
        }

        public override string ToString() => $"[{string.Join(", ", this)}]";
    }

    public class LevelSettings
    {
        public string Tier { get; set; } = "A";

        public Bulkhead Bulkheads { get; set; } = Bulkhead.Main;

        public ModifiersSet Modifiers { get; set; } = new ModifiersSet();

        public LevelSettings(string? tier = null)
        {
            if (tier != null)
            {
                Tier = tier;
                Generate();
            }
        }

        public void Generate()
        {
            switch (Tier)
            {
                case "A":
                    Modifiers.Add(LevelModifiers.NoChargers);
                    Modifiers.Add(LevelModifiers.NoShadows);
                    break;

                case "B":
                    Modifiers.Add(LevelModifiers.NoChargers);
                    Modifiers.Add(LevelModifiers.NoShadows);
                    break;

                case "C":
                    Modifiers.Add(LevelModifiers.NoShadows);
                    Modifiers.Add(
                        Generator.Select(new List<WeightedModifier>
                        {
                            new WeightedModifier { Modifier = LevelModifiers.NoChargers,   Weight = 0.4 },
                            new WeightedModifier { Modifier = LevelModifiers.Chargers,     Weight = 0.5 },
                            new WeightedModifier { Modifier = LevelModifiers.ManyChargers, Weight = 0.1 },
                        }).Modifier);
                    break;
            }
        }
    }
}
