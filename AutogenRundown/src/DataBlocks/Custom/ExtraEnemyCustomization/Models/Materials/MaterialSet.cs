using AutogenRundown.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Materials;

public class MaterialSet
{
    /// <summary>
    /// Material Name that enemy originally have. This gets mapped to a string with the
    /// property below
    /// </summary>
    [JsonIgnore]
    public MaterialType From { get; set; } = MaterialType.None;

    [JsonProperty("From")]
    public string FromValue => From.ToEnumString();

    /// <summary>
    /// Material Name that will be changed to
    /// </summary>
    [JsonIgnore]
    public MaterialType To { get; set; } = MaterialType.None;

    [JsonProperty("To")]
    public string ToValue => To.ToEnumString();

    /// <summary>
    /// Skin Noise (Bullrush-like Materials) Mode:
    ///     - KeepOriginal
    ///     - ForceOn
    ///     - ForceOff
    /// </summary>
    [JsonIgnore]
    public SkinNoise SkinNoise { get; set; } = SkinNoise.KeepOriginal;

    [JsonProperty("SkinNoise")]
    public string SkinNoiseValue => SkinNoise.ToEnumString();

    /// <summary>
    /// Which Texture you want to use it for Skin Noise?
    /// </summary>
    public string SkinNoiseTexture { get; set; } = "";

    /// <summary>
    /// ADVANCED::Directly Edit Shader Property values
    /// NOT recommened to edit this if you not sure what you're doing!
    /// </summary>
    public JArray ColorProperties { get; set; } = new();

    /// <summary>
    /// ADVANCED::Directly Edit Shader Property values
    /// NOT recommened to edit this if you not sure what you're doing!
    /// </summary>
    public JArray FloatProperties { get; set; } = new();
}
