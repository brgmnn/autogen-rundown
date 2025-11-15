using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;
using AutogenRundown.DataBlocks.Enemies;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.PeerMods;

public partial class SupportedMod
{
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

        // Category
        // Detection
        // EnemyAbility
        // Global
        // Model
        // Projectile
        // Property
        // ScoutWave
        // Tentacle
    }

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

    private void Import_ExtraEnemyCustomizationDetection(string path)
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

    private void Import_ExtraEnemyCustomizationEnemyAbility(string path)
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

    private void Import_ExtraEnemyCustomizationGlobal(string path)
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

    private void Import_ExtraEnemyCustomizationTentacle(string path)
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
}
