using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.EnemyAbilities;

public class FogSphereAbility
{
    public Sound SoundEventID { get; set; } = Sound.None;

    public string ColorMin { get; set; } = "#FFFFFFFF";

    public string ColorMax { get; set; } = "#00000000";

    public double IntensityMin { get; set; } = 1;

    public double IntensityMax { get; set; } = 5;

    public double RangeMin { get; set; } = 1;

    public double RangeMax { get; set; } = 3;

    public double DensityMin { get; set; } = 1;

    public double DensityMax { get; set; } = 5;

    public double DensityAmountMin { get; set; } = 0;

    public double DensityAmountMax { get; set; } = 5;

    public int Duration { get; set; } = 30;

    public JObject EffectVolume { get; set; } = new JObject
    {
        ["Enabled"] = false,
        ["Contents"] = "Infection",
        ["Modification"] = "Inflict",
        ["Scale"] = 1
    };

    public string Name { get; set; } = "";
}
