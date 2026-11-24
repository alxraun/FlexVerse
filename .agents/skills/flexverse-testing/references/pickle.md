# Harness: pickle

In-game scenarios with asserts, run inside the game container; evidence is
grep-markers in `Player.log`.

## Run

```
uv run Scripts/test pickle [FEATURE...]        # --profile local|ci, --with MOD
uv run Scripts/test pickle tween.feature
```

Selection is by file **name** (`DEFAULT_FILTER` in `Scripts/_lib.py`),
never by path — moving files never changes what runs. Exit: `0`
pass/compatible, `1` fail/incompatible, `2` harness did not run
(S-VERIFICATION.REQ-001).

## Layout

```
Pickle/
  Features/      tween.feature, flexevent.feature, focus.feature,
                 flexverse-smoke.feature
  Behaviors/     TweenSteps.cs
  Diagnostics/   SmokeSteps.cs, SmokeWidget.cs (+ your own scenarios)
  (flat)         FlexEventsSteps.cs + FlexEventsProbeFixtures.cs,
                 FocusSteps.cs + FocusProbeFixtures.cs
```

- `.feature` files always go to `Features/` — the csproj deploys them via the
  `Features/**/*.feature` glob.
- Steps and fixtures live flat; a subdirectory only where the topic clearly
  coheres. Reuse an existing directory before creating a new one.
- One shared namespace `FlexVerse.Test.Pickle` — steps bind by the compiled
  dll, not by namespace.

## Diagnostics/

The agent's sandbox: to check something, reproduce a bug, or pin down
behavior, create a scenario there — as many as needed, no need to ask.
Exploratory working tests, not a curated suite: prune when they outlive
their purpose.

## Scenario markers

Every step class brackets its scenarios through `ScenarioMarkers`
(Pickle/ScenarioMarkers.cs) — never emit markers by hand:

```csharp
[BeforeScenario] public static void Begin() => ScenarioMarkers.Begin("M-<MODULE>");
[AfterScenario]  public static void End()   => ScenarioMarkers.End("M-<MODULE>");
```

The runner writes the named outcomes (`passed|failed in Nms: name`); region +
outcome lines together attribute every log slice to a scenario.

## Graph

Every code and feature file declares its node in the first line:
`# parents: @.frostline/graph/m-pickle.xml` (`//` in C#).
