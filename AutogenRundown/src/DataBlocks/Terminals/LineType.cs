﻿namespace AutogenRundown.DataBlocks.Terminals;

public enum LineType
{
    Normal = 0,
    Fail = 1,
    SpinningWaitDone = 2,
    SpinningWaitNoDone = 3,
    ProgressWait = 4,
    Warning = 5,
}
