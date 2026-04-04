# EventBreak - Sequential Event Activation

## What It Does

`EventBreak` (type 999) splits a list of warden events into groups that fire one group at a time. Without it, all events in a list fire simultaneously on the first trigger. With `EventBreak`, events before the first break fire on the first trigger, events between the first and second break fire on the second trigger, and so on.

## When to Use

Use `EventBreak` when you have an event list tied to a repeating trigger (like `OnActivateOnSolveItem`) and you want different events to fire each time the trigger occurs.

The primary use case is Cryptomnesia: each time a player picks up a data cube, they should warp to a different dimension. Without `EventBreak`, all warps would fire on the first pickup.

## How It Works

Given an event list like:

```
[WarpTeam Dimension1] [EventBreak] [WarpTeam Dimension2] [EventBreak] [WarpTeam Reality]
```

- 1st trigger: fires `WarpTeam Dimension1`
- 2nd trigger: fires `WarpTeam Dimension2`
- 3rd trigger: fires `WarpTeam Reality`

The game engine processes events sequentially and stops at each `EventBreak`. On the next trigger, it resumes from after the break.

## Usage

### Extension Methods

```csharp
// Add a break to split event groups
events.AddEventBreak();

// Commonly paired with dimension warps
events
    .AddDimensionWarp(DimensionIndex.Dimension1)
    .AddEventBreak();
```

### Example: Cryptomnesia Warp Sequence

Each data cube pickup warps the team to the next dimension, cycling through all dimensions and ending back in Reality:

```csharp
// Warp to each dimension (one per item pickup), then back to Reality
foreach (var zoneNode in Gather_PlacementNodes.TakeLast(GatherSpawnCount - 1))
{
    EventsOnActivate
        .AddDimensionWarp(zoneNode.Dimension)
        .AddEventBreak();
}

EventsOnActivate.AddDimensionWarp(DimensionIndex.Reality);

// Enable per-item activation so events fire on each pickup
OnActivateOnSolveItem = true;
```

With 4 data cubes (1 in Reality, 3 in dimensions), this produces:

```
Pickup cube 1 (Reality)  -> Warp to Dimension1
Pickup cube 2 (Dim1)     -> Warp to Dimension2
Pickup cube 3 (Dim2)     -> Warp to Dimension3... but wait,
                            last group has no break so:
Pickup cube 3 (Dim2)     -> Warp to Reality
```

Note: the final event group (after the last `EventBreak`) has no break after it, so it fires on the last trigger and the sequence is complete.

## Key Properties

- **Event type**: `WardenObjectiveEventType.EventBreak` (999)
- **No parameters**: `EventBreak` takes no additional configuration
- **Position matters**: only the position within the event list determines grouping
- **Works with any event list**: `EventsOnActivate`, `EventsOnOpenDoor`, etc.
