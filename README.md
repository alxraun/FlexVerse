# FlexVerse

> ⚠️ The project is at a working-prototype stage and is not production-ready. The API may change without backward compatibility.

A declarative reactive framework for building UIs of any complexity from scratch in RimWorld, on top of IMGUI. You describe the interface as a widget tree with flexbox layout, and state as tracked dependencies: a value changes — the widgets re-render themselves. No imperative per-frame code.

https://github.com/user-attachments/assets/875337dc-7f9b-4d13-97ab-5cd83ef504ef

Every widget paints itself with ordinary IMGUI code in `OnGUI`. On top of that, FlexVerse adds a widget tree, flexbox layout and reactive state. You write your own controls the same way: override the widget's `OnGUI()` and draw with familiar IMGUI.

## Meet Behavior

The key difference is *what* you assign. In a typical UI you say "here is the value X" — and then someone has to animate X from the side, if it is supposed to move. In FlexVerse you say "here is the behavior of X" — the object you assign knows how to behave: stand still, smoothly travel to a target, or derive itself from other values.

```csharp
Opacity = 1f;                               // static (a plain value is lifted into a behavior)
Opacity = Animate.To(0f);                   // replaced with a tween animation
Opacity = Visible.Select(v => v ? 1f : 0f); // replaced with a computed dependency
```

The same prop slot accepts any behavior: start with a static value, replace it with an animation or a computed at any moment — the engine transfers the state and re-subscribes consumers for you.

## Example

A counter: the value lives in `Behavior<int>`, the label and the button react on their own.

```csharp
public class CounterPanel : View
{
    [State] public Behavior<int> Count { get; private set; } = 0;

    public override IEnumerable<Widget> BuildRenderTree() =>
    [
        new Label
        {
            Content = Count.Select(value => $"Clicks: {value}"),
            Height = 30f.Point(),
        },
        new Button
        {
            Label = "+1",
            OnClick = new Action(() => Count.Target++),
        },
    ];
}

// Host (for example, a RimWorld window):
void Draw(Rect rect) => FVUI.Render("counter", rect, new CounterPanel { });
void Close() => FVUI.Release("counter");
```

In IMGUI this would require an `int _count` field, a `Widgets.ButtonText` call every frame and manual string formatting — here a change to `Count.Target` automatically recomputes the derived string and re-renders only the label.

## This Is Still IMGUI

Under the hood is native Unity IMGUI: the same `Rect`, the same `GUI`/`Widgets`, the same rendering pass. On top of it — a widget tree and flexbox layout in place of manual `Rect` math. You write your own leaf widget exactly like the built-in ones: override `OnGUI()` and draw with anything you like.

| | IMGUI (`Widgets.*`, Verse) | FlexVerse |
|---|---|---|
| Code model | One imperative `DoWindowContents`, everything re-renders every frame | A declarative tree; only widgets whose dependencies changed re-render |
| State | Manual fields + manual UI synchronization | `Behavior<T>` — the value updates all subscribed widgets by itself |
| Layout | Manual `Rect` math and margin constants | Flexbox: `FlexDirection`, `FlexGrow`, `AlignItems`, percentages and points |
| Animation | Written by hand (timers, lerp) | Built in: `Spring`, `Animate`, `Timeline` |
| Reuse | Copy-pasted drawing blocks | Widgets are ordinary C# classes, composed as components |
| Custom controls | — | A new leaf widget = override `OnGUI()` and draw plain IMGUI |

## Key Concepts

- **`Behavior<T>`** — a behavioral value: read `.Value`, write `.Target`. Derivations: `Select` (map), `Group.Combine` (multiple sources), `Static.Value` (a constant).
- **Animation** — also a behavior: `Spring.State/To`, `Animate.To/FromTo`, `Timeline.Create(...)` for sequences.
- **Widgets** — `View` (a container) and ready-made primitives: `Label`, `Button`, `Image`, `TextField`, `Checkbox`, `ScrollView`, `Slider` and more.
- **Input** — overrides of `OnMouseDown/OnKeyDown/...` with a `FlexEvent` and `StopPropagation`.
- **Hosting** — `FVUI.Render(key, rect, root)` mounts the tree into any `Rect`; `FVUI.Release(key)` releases it.

## Examples

A rich showcase — [RimRewind](Source/FlexVerse.Test/Explorer/Showcases/RimRewind/): a settings screen with flexbox layout, popup menus, spring hover animations and timeline scenarios. More examples live in the `flexverse` skill (`.agents/skills/flexverse/`), which also contains the API reference for widgets and behaviors.

## Build

Requires the .NET SDK 10 (pinned in `global.json`).

```
dotnet build
```

## License

FlexVerse is released under the [MIT license](LICENSE). Third-party components and their licenses are listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
