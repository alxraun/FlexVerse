---
name: flexverse-coding
description: FlexVerse coding rules — logging discipline and module conventions. Load before writing or modifying code under Source/.
---

# FlexVerse Coding

## Logging

Only through `FlexVerse.Log`; never `Verse.Log` / `UnityEngine.Debug`.

Grep-stable format:

```
[frame][FV][M-module][Class:Method][LEVEL] event VARIABLES: {k=v}
```

- dot-notated event, no prose
- `VARIABLES: {}` when empty

Levels:

- `Debug` — hot paths, internal loop state
- `Info` — milestones, lifecycle changes, contract dispatches
- `Warning` — recoverable anomalies, degraded operation
- `Error` — invariant violations, contract breaches, unhandled exceptions
