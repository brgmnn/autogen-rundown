namespace AutogenRundown.DataBlocks.Enemies;

public record AiAbility
{
    public bool Enabled { get; set; } = true;

    public string AbilityPrefab { get; set; } = "Assets/AssetPrefabs/Characters/Enemies/Abilities/EAB_StrikerBigTentacle.prefab";

    public int AbilityType { get; set; } = 2;

    public double Cooldown { get; set; } = 1.0;
}
