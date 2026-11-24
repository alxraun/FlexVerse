---
name: flexverse
description: Write declarative reactive code with FlexVerse for RimWorld — reactive models, scenarios, and UI through FVP, Behaviors, Timeline, and widgets.
---

# FlexVerse

## Architecture

A declarative reactive framework for RimWorld that includes a UI framework.

* `Behavior<T>` -> `Value` + `Target` + `OnUpdate` + `OnComplete`
* `Static` + `Animate` + `Spring` + `Group.Sequence` -> `Behavior<T>`
* `Select` + `Group.Combine` -> `ComputedBehavior`; `Effect.Create` -> `EffectBehavior`
* `FVPAttribute` -> `Refs` + `Bind` + `Watch`
* `old.SemEquals(next) == true` => `old`; `old.SemEquals(next) == false` -> `next.UnionWith(old)` => `next`
* `Timeline.Create` -> `TimelineBehavior` -> `ITimelineAction` -> `IPropertyRef.Set` | `Action` | `Func<bool>`
* `FlexVerseRoot` -> `Behavior.Update`
* `PropAttribute` + `StateAttribute` + `InheritedContextAttribute` : `FVPAttribute`
* `FVUI.Render` -> `Widget.BuildRenderTree` -> `Flexbox` -> `Widget.OnGUI`
* `FlexEvent` -> `Widget.OnMouseDown` | `Widget.OnKeyDown`
* `FVUI.Release` -> `Widget.OnUnmount`

## Widget API

Length: `12.Point()`, `50.Percent()`, `Length.Auto`, `Length.Undefined`

Widget: `Key`, `Children`, `Width`, `Height`, `MinWidth`, `MinHeight`, `MaxWidth`, `MaxHeight`, `AspectRatio`, `FlexBasis`, `FlexGrow`, `FlexShrink`, `Direction`, `FlexDirection`, `AlignItems`, `AlignSelf`, `AlignContent`, `JustifyContent`, `FlexWrap`, `Overflow`, `PositionType`, `Display`, `MarginAll`, `MarginHorizontal`, `MarginVertical`, `MarginLeft`, `MarginTop`, `MarginRight`, `MarginBottom`, `MarginStart`, `MarginEnd`, `PaddingAll`, `PaddingHorizontal`, `PaddingVertical`, `PaddingLeft`, `PaddingTop`, `PaddingRight`, `PaddingBottom`, `PaddingStart`, `PaddingEnd`, `BorderAll`, `BorderHorizontal`, `BorderVertical`, `BorderLeft`, `BorderTop`, `BorderRight`, `BorderBottom`, `BorderStart`, `BorderEnd`, `PositionHorizontal`, `PositionVertical`, `PositionLeft`, `PositionTop`, `PositionRight`, `PositionBottom`, `PositionStart`, `PositionEnd`, `ZIndex`, `Translate`, `Scale`, `Rotate`, `TransformOrigin`

Widget.Hooks: `BuildRenderTree`, `OnMount`, `OnUnmount`, `OnGUI`

Widget.Refs: `Refs`, `Bind`, `Watch`, `GetProperty`

Reconciliation: matching `Key` and type among children of the same parent → the previous instance and its State; a new declaration → Props. Without an explicit Key, a positional key is used.

Widget.Input: `OnMouseDown`, `OnMouseUp`, `OnMouseDrag`, `OnMouseMove`, `OnMouseEnter`, `OnMouseLeave`, `OnScrollWheel`, `OnKeyDown`, `OnKeyUp`

FlexEvent: `NativeEvent`, `Target`, `CurrentTarget`, `Type`, `MousePosition`, `Button`, `ScrollDelta`, `KeyCode`, `Character`, `Modifiers`, `IsPropagationStopped`, `StopPropagation`

[View](references/widgets/view.md): `Tooltip`, `OnClick`, `Disabled`, `BackgroundColor`, `BorderColor`, `BackgroundImage`, `BackgroundSize`, `BackgroundPosition`, `BackgroundImageTintColor`, `BorderRadius`, `BorderTopLeftRadius`, `BorderTopRightRadius`, `BorderBottomRightRadius`, `BorderBottomLeftRadius`, `BoxShadow`

[Label](references/widgets/label.md): `Content`, `Font`, `TextAlign`, `TextTransform`, `TextWrap`, `TextOverflow`, `Color`, `Tooltip`, `AutoTooltip`

[Image](references/widgets/image.md): `TexturePath`, `TextureInstance`, `ResizeMode`, `TintColor`, `Tooltip`, `BorderRadius`, `BorderTopLeftRadius`, `BorderTopRightRadius`, `BorderBottomRightRadius`, `BorderBottomLeftRadius`

[Button](references/widgets/button.md): `Label`, `OnClick`, `Tooltip`, `Disabled`, `DrawButtonBackground`, `Font`, `TextAlign`, `Color`

Secondary widgets: [ScrollView](references/widgets/scroll-view.md) · [CanvasView](references/widgets/canvas-view.md) · [TextField](references/widgets/text-field.md) · [Checkbox](references/widgets/checkbox.md) · [HorizontalSlider](references/widgets/horizontal-slider.md) · [AreaSlider](references/widgets/area-slider.md)

## Behaviors

[Behavior](references/behaviors/behavior.md): `Value`, `Target`, `State`, `IsUpdating`, `SemEquals`, `UnionWith`, `Clone`, `CloneUntyped`, `Reset`

Events: `OnActivate`, `OnStart`, `OnUpdate`, `OnComplete`, `OnReset`

[Static](references/behaviors/static-behavior.md) · [Tween](references/behaviors/tween-behavior.md) · [Spring](references/behaviors/spring-behavior.md) · [Keyframe](references/behaviors/keyframe-behavior.md) · [Sequence](references/behaviors/sequence-behavior.md) · [Computed](references/behaviors/computed-behavior.md) · [Effect](references/behaviors/effect-behavior.md) · [Timeline](references/behaviors/timeline-behavior.md)

## FVP

A self-contained process; the game system, test, or tool supplies the steps.

```csharp
public partial class BatchFlow
{
    [FVP] public Behavior<int> Completed { get; private set; } = 0;
    [FVP] public Behavior<int> Total { get; private set; } = 0;
    [FVP] public Behavior<bool> Running { get; private set; } = false;
    [FVP] public Behavior<float> Progress { get; private set; }
    [FVP] private TimelineBehavior Playback { get; set; }

    public void Initialize() => Progress = Group.Combine(Completed, Total,
        (completed, total) => total > 0 ? (float)completed / total : 0f);

    public void Run(IReadOnlyList<Action> steps)
    {
        Completed.Target = 0;
        Total.Target = steps.Count;
        Running.Target = true;
        Playback = Timeline.Create(Timeline.Sequence(
            Timeline.While(() => Completed.Value < steps.Count, Timeline.Sequence(
                Timeline.Call(() => steps[Completed.Value]()),
                Timeline.Call(() => Completed.Target++)
            )),
            Timeline.Call(() => Running.Target = false)
        ));
    }

    public void Stop() { Playback?.Stop(); Running.Target = false; }
}
```

## Widget

Two slices of the same `BatchPanel`: state/tree and handlers. The UI owns local animation; the model owns step execution.

### State and tree

```csharp
public partial class BatchPanel : View
{
    [Prop] public Behavior<Action[]> Steps { get; set; } = Array.Empty<Action>();
    [Prop] public Behavior<Action<float>> OnFeedback { get; set; }
    [Prop] public Behavior<Action<Vector2>> OnPick { get; set; }
    [InheritedContext] public Behavior<Color> Accent { get; set; }
    [State] private Behavior<BatchFlow> Flow { get; set; }
    [State] private Behavior<string> Title { get; set; } = "Package";
    [State] private Behavior<float> Hover { get; set; } = Spring.State(0f);
    [State] private Behavior<float> Feedback { get; set; } = 0f;

    public override void OnMount()
    {
        Flow = new BatchFlow();
        Flow.Value.Initialize();
        Scale = Hover.Select(value => Vector2.one * (1f + value * 0.02f));
        Watch.Feedback.OnUpdate += _ => OnFeedback.Value?.Invoke(Feedback.Value);
    }

    public override IEnumerable<Widget> BuildRenderTree() =>
    [
        new TextField
        {
            Value = Title,
            OnValueChange = new Action<string>(value => Title.Target = value),
            OnSubmit = new Action(Start), OnCancel = new Action(Flow.Value.Stop),
        },
        new Label
        {
            Content = Group.Combine(Title, Flow.Value.Progress, (title, progress) => $"{title}: {progress:P0}"),
            Color = Group.Combine(Accent, Feedback, (accent, flash) => Color.Lerp(accent, Color.white, flash)),
        },
        new Button { Label = "Run", OnClick = new Action(Start) },
    ];

    public override void OnUnmount() => Flow.Value.ReleaseFvp();
}
```

### Subscriptions and input

`Refs` addresses a slot; `Bind` lives until the Behavior is replaced, `Watch` follows replacements. TextField handles Enter/Escape; F5 bubbles from it up to BatchPanel.

```csharp
// Members of the same BatchPanel.
private void Start()
{
    Flow.Value.Run(Steps.Value);
    Refs.Feedback.Set(Animate.FromTo(0f, 1f, 0.2f));
    Bind.Feedback.OnComplete += () => Feedback = Spring.To(0f);
}

public override void OnMouseEnter(FlexEvent e) => Hover.Target = 1f;
public override void OnMouseLeave(FlexEvent e) => Hover.Target = 0f;

public override void OnMouseDown(FlexEvent e)
{
    if (e.Button != 0 || !ReferenceEquals(e.Target, this)) return;
    OnPick.Value?.Invoke(e.MousePosition);
    e.StopPropagation();
}

public override void OnKeyDown(FlexEvent e)
{
    if (e.KeyCode != KeyCode.F5) return;
    Start();
    e.StopPropagation();
}
```

## FVUI

```csharp
public partial class ThemeScope : View
{
    [InheritedContext] public Behavior<Color> Accent { get; set; } = Color.cyan;
}
```

```csharp
// Members of the native host: Draw is called from IMGUI, Close on closing.
private readonly string _uiKey = $"batch:{Guid.NewGuid():N}";
void Draw(Rect rect, Action[] steps) => FVUI.Render(_uiKey, rect, new ThemeScope
{
    Children = new WidgetList { new BatchPanel { Steps = steps } },
});
void Close() => FVUI.Release(_uiKey);
```
