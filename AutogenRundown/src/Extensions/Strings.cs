using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Materials;

namespace AutogenRundown.Extensions;

public static class Strings
{
    public static string ToEnumString(this MaterialType material)
        => material switch
        {
            MaterialType.MtrHeadHenk => "mtr_head_henk",
            MaterialType.Birther => "birther",
            MaterialType.MtrCocoon => "mtr_cocoon",
            MaterialType.MtrDestARMShtr => "mtr_dest_arm_shtr",
            MaterialType.MtrDestChestSides => "mtr_dest_chest_sides",
            MaterialType.MtrDestHalfShtr => "mtr_dest_half_shtr",
            MaterialType.MtrDestHead1 => "mtr_dest_head_1",
            MaterialType.MtrHipSide => "mtr_hip_side",
            MaterialType.MtrHipsHole => "mtr_hips_hole",
            MaterialType.MtrHoleShtr => "mtr_hole_shtr",
            MaterialType.MtrLegShtr => "mtr_leg_shtr",
            MaterialType.Scout => "scout",
            MaterialType.Shooter => "shooter",
            MaterialType.ShooterBig => "shooterBig",
            MaterialType.ShooterRapidFire => "shooter_RapidFire",
            MaterialType.ShooterHibernate => "shooter_hibernate",
            MaterialType.ShooterWave => "shooter_wave",
            MaterialType.MtrTank => "mtr_tank",
            MaterialType.MtrGibMeatTrailer => "mtr_gib_meat_trailer",
            MaterialType.MtrStriker => "mtr_striker",
            MaterialType.MtrStrikerChild => "mtr_StrikerChild",
            MaterialType.MtrChestHole => "mtr_chest_hole",
            MaterialType.MtrChestSideFix => "mtr_chest_side_fix",
            MaterialType.MtrDestARM => "mtr_dest_arm",
            MaterialType.MtrDestLeg => "mtr_dest_leg",
            MaterialType.MtrHalf => "mtr_half",
            MaterialType.MtrHeadshot => "mtr_headshot",
            MaterialType.MtrHip => "mtr_hip",
            MaterialType.MtrStomacheFix => "mtr_stomache_fix",
            MaterialType.MtrStrikerBullrush => "mtr_striker_bullrush",
            MaterialType.MtrStrikerBullrushHead => "mtr_striker_bullrush_head",
            MaterialType.MtrStrikerHibernate => "mtr_striker_hibernate",
            MaterialType.MtrStrikerWave => "mtr_striker_wave",
            MaterialType.Striker => "striker",
            MaterialType.StrikerBig => "strikerBig",
            _ => ""
        };

    public static string ToEnumString(this SkinNoise skinNoise)
        => skinNoise switch
        {
            SkinNoise.KeepOriginal => "KeepOriginal",

            SkinNoise.UnityDitherMask3D => "UnityDitherMask3D",
            SkinNoise.UnityDefault3D => "UnityDefault3D",
            SkinNoise.PixelNoise => "PixelNoise",
            SkinNoise.CloudDetailNoise => "CloudDetailNoise",
            SkinNoise.NoiseTexture => "NoiseTexture",
            SkinNoise.NoiseTexture1 => "NoiseTexture 1",
            SkinNoise.NoiseTexture3 => "NoiseTexture 3",
            SkinNoise.NoiseTexture4 => "NoiseTexture 4",
            SkinNoise.NoiseTexture5 => "NoiseTexture 5",
            SkinNoise.ThreeChannelNoise => "ThreeChannelNoise",
            _ => ""
        };
}
