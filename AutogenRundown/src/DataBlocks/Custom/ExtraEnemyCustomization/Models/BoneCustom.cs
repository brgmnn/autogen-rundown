using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Bones;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models;

public record BoneCustom : CustomRecord
{
    public ICollection<Bone> Bones { get; set; } = new List<Bone>();

    /// <summary>
    /// TODO: use?
    /// </summary>
    public JArray Prefabs { get; set; } = new();
}
