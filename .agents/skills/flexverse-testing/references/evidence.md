# Evidence

## Ground rules

- `Player.log` is the evidence — the only primary source of proof about what
  actually ran. `report.json` (schema `flexverse.report/1`) and `diff.json`
  (compat diff vs the base run, knownNoise subtracted) are derived verdicts —
  summaries, not proof.
- How you read the log is up to you; analyze it freely. Nothing below
  constrains the method.
- Marker format: `[frame][FV][M-module][Class:Method][LEVEL] event
  VARIABLES: {k=v}`.

## Evidence map

| Artifact | Role |
|---|---|
| `.cache/runs/<run>/Player.log` | primary evidence, ground truth |
| `runs/latest/report.json` | derived verdict (grep-markers only) |
| `runs/latest/diff.json` | compat runs: new facts vs base run |

## Heuristics (advisory, from the FLI logging contract)

- Start with a holistic sweep: filter out `[DEBUG]` noise, inspect only
  `[INFO]`/`[WARNING]`/`[ERROR]`, and map the macro-timeline of execution.
  Then dive pointwise wherever the map looks wrong.
- XUnit green + Pickle red points at runtime integration, not module logic.
- Silence is never green: an absent marker is a failed expectation, not a
  passed one. A claim is only as strong as the log lines that prove it.
