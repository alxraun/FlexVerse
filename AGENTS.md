# FlexVerse

FlexVerse is a declarative, reactive modding framework for RimWorld: mod UI and
behavior are described as behavior trees with computed, dependency-tracked
properties (FlexProps) that update widgets automatically instead of imperative
per-frame code. The repo hosts three deliverables: the FlexVerse runtime
framework (Source/FlexVerse), the FlexTools in-game devtools mod, and the
MSBuild-based tooling and verification pipeline (Scripts/).

## Build

```
dotnet build
```

## Reference Sources

Don't guess APIs: read sources at these paths. If a path is missing or empty, decompile/clone it there yourself (ilspycmd, git clone). `.cache/references/` is regenerable — never edit, never commit.

| Path | Content |
|---|---|
| `.cache/references/rimworld/` | decompiled Assembly-CSharp(-firstpass), keyed by game version |
| `.cache/references/pickle/` | Pickle framework, pinned to `rimworks.pickle` tag in `Scripts/mods.json` |
| `.cache/references/fody/` | Fody @ 6.9.3 |
| `.cache/references/roslyn/` | dotnet/roslyn (shallow HEAD) |

Answer Verse/Unity API questions from decompiled game code, not from memory.
