using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin.DataBlocks.Alarms
{
    internal enum VanillaWaveSettings : UInt32
    {
        None = 0,

        // Alarms?
        Apex = 1,
        ApexReducedShadows = 4,
        ApexReduced = 12,
        ApexS = 13,

        // Surge alarms
        Surge = 21,

        // Scout Waves
        Scout_sspm = 3,

        // Exit Trickles
        ExitTrickle_38S_Original = 5,
        ExitTrickle_18S = 11,
        ExitTrickle_410_S = 14,
        ExitTrickle_445_sspb = 15,
        ExitTrickle_46_b = 16,
        ExitTrickle_212_S = 20,

        // Reactor
        Reactor_24_sspmb = 7,
        Reactor_6_mb = 9,
    }

    internal class WaveSettings : DataBlock
    { }
}
