# Harness: xunit

Offline scenarios with asserts — everything provable without the game
(models, source generators, the IL weaver, FlexTools). Runs via `dotnet
test`; the verdict is the dotnet exit code itself.

## Run

```
uv run Scripts/test xunit [FILTER]
uv run Scripts/test xunit "FlexVerse.Test.SourceGenerators.*"
```

`FILTER` is a native dotnet test filter; the namespace-per-project convention
makes it a project selector. Exit: `0` pass, `1` fail, `2` infrastructure.

## Layout

One top-level directory per tested **project**, flat inside:

```
Xunit/
  FlexVerse/          runtime: Widget, WidgetEngine, EngineRegistry,
                      FocusManager, UIFacade, StaticBehavior, TweenBehavior
  SourceGenerators/   FlexVerseProperty, ReactivityAnalyzer, WidgetEmitter
  PropertyWeaver/     PropertyWeaver, FlexVerseProperty.weaver,
                      WidgetEmitter.weaver
  FlexTools/          DebugInspect (widget-debugger models, Unity-free
                      sources compiled into the test project)
  Shims/              ShimCompleteness (Unity shim surface)
  Helpers/            TestPaths
```

- Namespace follows the directory (`FlexVerse.Test`, `FlexVerse.Test.<Dir>`).
- A `.weaver` / `.semantics` / `.retarget` suffix splits a topic by aspect,
  never a new directory.
- Only code paths free of `RimWorld`/`Verse`/`UnityEngine` belong here;
  anything needing the game goes to [pickle](pickle.md) or
  [explorer](explorer.md).

## Graph

Every test file declares its node in the first line:
`// parents: @.frostline/graph/m-xunit.xml`.
