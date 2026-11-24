---
name: flexverse-testing
description: FlexVerse testing — testing rules, choosing verification methods, writing and running tests, Explorer and in-game checks, reading evidence, and diagnosing failures. Load when designing tests, changing test coverage, verifying behavior or compatibility, or investigating test and runtime failures.
---

# FlexVerse Testing

One gate command, laptop and CI alike:

```
uv run Scripts/test all                       # gate: xunit + in-game, local game
uv run Scripts/test all --profile ci          # game from steamcmd (creds via env)
uv run Scripts/test pickle --with <mod>       # differential run vs runs/base
uv run Scripts/test pickle tween.feature      # narrow the matrix to one feature
uv run Scripts/test xunit [FILTER]            # one harness: offline xunit only
uv run Scripts/play                          # manual play, same container, no verdict
```

## Harnesses

| Harness   | Kind                       | Reference                       |
|-----------|----------------------------|---------------------------------|
| `xunit`   | offline scenarios + asserts | [references/xunit.md](references/xunit.md) |
| `pickle`  | in-game scenarios + asserts | [references/pickle.md](references/pickle.md) |
| `explorer`| in-game manual/visual scenarios | [references/explorer.md](references/explorer.md) |

Read the reference for a harness before writing tests for it.

Run harnesses only when the change is significant enough to verify (code or
behavior under test changed) — not after every edit.

## Evidence

- `.cache/runs/<run>/` (`runs/latest`): `Player.log` is the only primary source;
  `report.json`/`diff.json` derived from grep-markers.
- Markers: `[frame][FV][M-module][Class:Method][LEVEL] event VARIABLES: {k=v}`,
  `scenario:BEGIN/END`, `passed in Nms: <name>`.
- `.cache/` is fully regenerable.

When a run fails or behaves unexpectedly, read the log as evidence per
[references/evidence.md](references/evidence.md).
