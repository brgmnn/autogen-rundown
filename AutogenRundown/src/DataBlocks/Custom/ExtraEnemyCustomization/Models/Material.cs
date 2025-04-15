using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Materials;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models;

public record Material : CustomRecord
{
    public ICollection<MaterialSet> MaterialSets { get; set; } = new List<MaterialSet>();
}
