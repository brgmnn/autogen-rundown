using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;

namespace AutogenRundownTests.DataBlocks;

[TestClass]
public class GenericWave_Tests
{
    [TestMethod]
    public void Test_BossErrorAlarmPresets_AreRealAlarms()
    {
        Assert.IsTrue(GenericWave.ErrorAlarm_Boss_Hard_Tank.TriggerAlarm);
        Assert.IsTrue(GenericWave.ErrorAlarm_Boss_VeryHard_TankPotato.TriggerAlarm);
        Assert.IsTrue(GenericWave.ErrorAlarm_Boss_Hard_Mother.TriggerAlarm);
    }

    [TestMethod]
    public void Test_BossErrorAlarmPresets_PairSettingsAndPopulation()
    {
        Assert.AreEqual(WaveSettings.Error_Boss_VeryHard, GenericWave.ErrorAlarm_Boss_VeryHard_TankPotato.Settings);
        Assert.AreEqual(WavePopulation.SingleEnemy_TankPotato, GenericWave.ErrorAlarm_Boss_VeryHard_TankPotato.Population);

        Assert.AreEqual(WaveSettings.Error_Boss_Hard, GenericWave.ErrorAlarm_Boss_Hard_Mother.Settings);
        Assert.AreEqual(WavePopulation.SingleEnemy_Mother, GenericWave.ErrorAlarm_Boss_Hard_Mother.Population);
    }
}
