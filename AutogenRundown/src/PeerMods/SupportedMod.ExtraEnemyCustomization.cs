using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.PeerMods;

public partial class SupportedMod
{
    // TODO: support merging the unused files too
    protected void CopyCustomExtraEnemyCustomization()
    {
        var customPath = Path.Combine("Custom", "ExtraEnemyCustomization");
        var pluginPath = Path.Combine(PluginFolder, customPath);
        var gameDataPath = Path.Combine(GameDataFolder, customPath);

        if (!Directory.Exists(pluginPath))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(gameDataPath)!);

        Plugin.Logger.LogDebug($"{ModName}: Copying \"Custom/ExtraEnemyCustomization\"");
        // CopyDirectory(pluginPath, gameDataPath);

        CopyDirectory(Path.Combine(pluginPath, "icons"), Path.Combine(gameDataPath, "icons"));

        Import_ExtraEnemyCustomizationAbility(Path.Combine(pluginPath, "Ability.json"));
        Import_ExtraEnemyCustomizationEnemyAbility(Path.Combine(pluginPath, "EnemyAbility.json"));

        // Each of the unused files for now.
        foreach (var filename in new List<string> { "Category.json", "Detection.json", "ScoutWave.json", "Tentacle.json" })
        {
            File.Copy(
                Path.Combine(pluginPath, filename),
                Path.Combine(gameDataPath, filename),
                overwrite: true);
        }

        // EnemyAbility
        // Global
        // Model
        // Projectile
        // Property
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    private void Import_ExtraEnemyCustomizationAbility(string path)
    {
        var data = JObject.Parse(File.ReadAllText(path)).ToObject<Ability>();

        if (data == null)
            return;

        // Copy over each of the list of definitions as is
        EnemyCustomization.Ability.FogSphere.AddRange(data.FogSphere);
        EnemyCustomization.Ability.Birthings.AddRange(data.Birthings);
        EnemyCustomization.Ability.HealthRegen.AddRange(data.HealthRegen);
        EnemyCustomization.Ability.InfectionAttacks.AddRange(data.InfectionAttacks);
        EnemyCustomization.Ability.ExplosiveAttacks.AddRange(data.ExplosiveAttacks);
        EnemyCustomization.Ability.KnockbackAttacks.AddRange(data.KnockbackAttacks);
        EnemyCustomization.Ability.BleedAttacks.AddRange(data.BleedAttacks);
        EnemyCustomization.Ability.DrainStaminaAttacks.AddRange(data.DrainStaminaAttacks);
        EnemyCustomization.Ability.DoorBreaker.AddRange(data.DoorBreaker);
        EnemyCustomization.Ability.ScoutScreaming.AddRange(data.ScoutScreaming);
        EnemyCustomization.Ability.Pouncer.AddRange(data.Pouncer);
    }

    private void Import_ExtraEnemyCustomizationCategory(string path)
    {
    }

    private void Import_ExtraEnemyCustomizationDetection(string path)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    private void Import_ExtraEnemyCustomizationEnemyAbility(string path)
    {
        var data = JObject.Parse(File.ReadAllText(path)).ToObject<DataBlocks.Custom.ExtraEnemyCustomization.EnemyAbility>();

        if (data == null)
            return;

        EnemyCustomization.EnemyAbility.BehaviourAbilities.AddRange(data.BehaviourAbilities);
        EnemyCustomization.EnemyAbility.LimbDestroyedAbilities.AddRange(data.LimbDestroyedAbilities);
        EnemyCustomization.EnemyAbility.DeathAbilities.AddRange(data.DeathAbilities);
        EnemyCustomization.EnemyAbility.Abilities.Chain.AddRange(data.Abilities.Chain);
        EnemyCustomization.EnemyAbility.Abilities.FogSphere.AddRange(data.Abilities.FogSphere);
        EnemyCustomization.EnemyAbility.Abilities.Explosion.AddRange(data.Abilities.Explosion);
        EnemyCustomization.EnemyAbility.Abilities.SpawnEnemy.AddRange(data.Abilities.SpawnEnemy);
        EnemyCustomization.EnemyAbility.Abilities.SpawnWave.AddRange(data.Abilities.SpawnWave);
        EnemyCustomization.EnemyAbility.Abilities.SpawnProjectile.AddRange(data.Abilities.SpawnProjectile);
        EnemyCustomization.EnemyAbility.Abilities.DoAnim.AddRange(data.Abilities.DoAnim);
        EnemyCustomization.EnemyAbility.Abilities.Cloak.AddRange(data.Abilities.Cloak);
        EnemyCustomization.EnemyAbility.Abilities.EMP.AddRange(data.Abilities.EMP);
    }

    private void Import_ExtraEnemyCustomizationGlobal(string path)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    private void Import_ExtraEnemyCustomizationModel(string path)
    {
        var data = JObject.Parse(File.ReadAllText(path)).ToObject<Ability>();

        if (data == null)
            return;

        // Copy over each of the list of definitions as is
        EnemyCustomization.Ability.FogSphere.AddRange(data.FogSphere);
        EnemyCustomization.Ability.Birthings.AddRange(data.Birthings);
        EnemyCustomization.Ability.HealthRegen.AddRange(data.HealthRegen);
        EnemyCustomization.Ability.InfectionAttacks.AddRange(data.InfectionAttacks);
        EnemyCustomization.Ability.ExplosiveAttacks.AddRange(data.ExplosiveAttacks);
        EnemyCustomization.Ability.KnockbackAttacks.AddRange(data.KnockbackAttacks);
        EnemyCustomization.Ability.BleedAttacks.AddRange(data.BleedAttacks);
        EnemyCustomization.Ability.DrainStaminaAttacks.AddRange(data.DrainStaminaAttacks);
        EnemyCustomization.Ability.DoorBreaker.AddRange(data.DoorBreaker);
        EnemyCustomization.Ability.ScoutScreaming.AddRange(data.ScoutScreaming);
        EnemyCustomization.Ability.Pouncer.AddRange(data.Pouncer);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    private void Import_ExtraEnemyCustomizationProjectile(string path)
    {
        var data = JObject.Parse(File.ReadAllText(path)).ToObject<Ability>();

        if (data == null)
            return;

        // Copy over each of the list of definitions as is
        EnemyCustomization.Ability.FogSphere.AddRange(data.FogSphere);
        EnemyCustomization.Ability.Birthings.AddRange(data.Birthings);
        EnemyCustomization.Ability.HealthRegen.AddRange(data.HealthRegen);
        EnemyCustomization.Ability.InfectionAttacks.AddRange(data.InfectionAttacks);
        EnemyCustomization.Ability.ExplosiveAttacks.AddRange(data.ExplosiveAttacks);
        EnemyCustomization.Ability.KnockbackAttacks.AddRange(data.KnockbackAttacks);
        EnemyCustomization.Ability.BleedAttacks.AddRange(data.BleedAttacks);
        EnemyCustomization.Ability.DrainStaminaAttacks.AddRange(data.DrainStaminaAttacks);
        EnemyCustomization.Ability.DoorBreaker.AddRange(data.DoorBreaker);
        EnemyCustomization.Ability.ScoutScreaming.AddRange(data.ScoutScreaming);
        EnemyCustomization.Ability.Pouncer.AddRange(data.Pouncer);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    private void Import_ExtraEnemyCustomizationProperty(string path)
    {
        var data = JObject.Parse(File.ReadAllText(path)).ToObject<Ability>();

        if (data == null)
            return;

        // Copy over each of the list of definitions as is
        EnemyCustomization.Ability.FogSphere.AddRange(data.FogSphere);
        EnemyCustomization.Ability.Birthings.AddRange(data.Birthings);
        EnemyCustomization.Ability.HealthRegen.AddRange(data.HealthRegen);
        EnemyCustomization.Ability.InfectionAttacks.AddRange(data.InfectionAttacks);
        EnemyCustomization.Ability.ExplosiveAttacks.AddRange(data.ExplosiveAttacks);
        EnemyCustomization.Ability.KnockbackAttacks.AddRange(data.KnockbackAttacks);
        EnemyCustomization.Ability.BleedAttacks.AddRange(data.BleedAttacks);
        EnemyCustomization.Ability.DrainStaminaAttacks.AddRange(data.DrainStaminaAttacks);
        EnemyCustomization.Ability.DoorBreaker.AddRange(data.DoorBreaker);
        EnemyCustomization.Ability.ScoutScreaming.AddRange(data.ScoutScreaming);
        EnemyCustomization.Ability.Pouncer.AddRange(data.Pouncer);
    }

    private void Import_ExtraEnemyCustomizationScoutWave(string path)
    {
    }

    private void Import_ExtraEnemyCustomizationTentacle(string path)
    {
    }
}
