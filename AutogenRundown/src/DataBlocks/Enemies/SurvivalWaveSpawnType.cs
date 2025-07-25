﻿namespace AutogenRundown.DataBlocks.Enemies;

public enum SurvivalWaveSpawnType
{
    InRelationToClosestAlivePlayer = 0,
    InSuppliedCourseNodeZone = 1,
    InSuppliedCourseNode = 2,
    InSuppliedCourseNode_OnPosition = 3,
    ClosestToSuppliedNodeButNoBetweenPlayers = 4,
    OnSpawnPoints = 5,
    FromElevatorDirection = 6
}
