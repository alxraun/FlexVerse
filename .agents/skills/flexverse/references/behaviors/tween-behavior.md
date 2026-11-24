# TweenBehavior

## Definition

Interpolates a value toward a target over a given time using an easing function.

- `To` receives the predecessor's start value; `From` receives its target; `FromTo` sets both ends, `State` sets the rest state.
- Changing active `Target`, `Value`, or `Duration` starts movement from the current value with a fresh progress count.
- `MinDuration`/`MaxDuration` bound the duration based on distance and calibrated speed; both default to `Duration`.
- `OnUpdate` receives eased progress; Back/Elastic may go outside the 0…1 range. `OnComplete` ends the update cycle, including interruption.
- Supported types are `int`, `float`, `Vector2`, `Color`, and numeric `Length`; keep Length units consistent.

```text
Behavior<T> → TweenBehavior<T>
  Value: T                      current value
  Target: T                     target
  Duration: float = 1            base duration, seconds
  MinDuration: float            lower bound of duration
  MaxDuration: float            upper bound of duration
  Easing: EasingFunction         defaults to Linear
  State: Inactive | Active
  IsUpdating: bool

Animate.To<T>(T target, …)               → TweenBehavior<T>
Animate.From<T>(T value, …)              → TweenBehavior<T>
Animate.FromTo<T>(T value, T target, …)   → TweenBehavior<T>
Animate.State<T>(T value, …)             → TweenBehavior<T>

Common parameters:
  duration: float = 1
  minDuration: float? = null             null → duration
  maxDuration: float? = null             null → duration
  easing: EasingFunction = null          null → Linear
  onActivate, onStart, onComplete, onReset: Action = null
  onUpdate: Action<float> = null
Short form: Animate.To(target, duration, easing); the other factories are analogous.

EasingFunction: float → float
Easing.Linear
Easing.{Quad,Cubic,Sine,Back,Circ,Expo,Elastic,Quart,Quint}Ease{In,Out,InOut}

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
Methods: Clone() · CloneUntyped() · SemEquals(Behavior) · UnionWith(Behavior)
Owner lifecycle: Activate() · Update(float) · Reset()
```

## Examples

Widget slices; unshown properties are existing State/Props.

### SpeedAwareRetarget

```csharp
[State] private Behavior<Vector2> Position { get; set; } = Animate.State(
    Vector2.zero, duration: 0.6f, minDuration: 0.05f, maxDuration: 2f
);

private void HandlePointer(Vector2 point) => Position.Target = point;
```

### ScrubbableClock

```csharp
[State] private Behavior<float> Clock { get; set; } = Animate.FromTo(0f, 1f, 2f);

public override void OnMount()
{
    Rotate = Clock.Select(phase => phase * 360f);
    Scale = Clock.Select(phase => Vector2.one * (1f + Mathf.Sin(phase * Mathf.PI) * 0.1f));
}

private void HandleSeek(float phase) => Clock.Value = phase;

public override IEnumerable<Widget> BuildRenderTree() =>
[
    new HorizontalSlider { Value = Clock, OnValueChange = new Action<float>(HandleSeek) },
];
```

### ComplementaryCrossfade

```csharp
[State] private Behavior<float> Blend { get; set; } =
    Animate.State(0f, 0.4f, Easing.SineEaseInOut);

public override void OnMount()
{
    FrontTint = Blend.Select(t => new Color(1f, 1f, 1f, t));
    BackTint = Blend.Select(t => new Color(1f, 1f, 1f, 1f - t));
}

private void HandleFrontSelected() => Blend.Target = 1f;
```

### StateDrivenColor

```csharp
[State] private Behavior<Color> Tint { get; set; } = Animate.State(Color.gray, 0.25f);

// OnMount; Selected and Accent are stable inputs.
Following = Effect.Create(
    () => Tint.Target = Selected.Value ? Accent.Value : Color.gray,
    Selected, Accent
);
```

### TwoSpeedRoute

```csharp
Travel = Group.Sequence<Vector2>(
    Animate.To(Corner.Value, 0.2f, Easing.CubicEaseOut),
    Animate.To(Destination.Value, 0.6f, Easing.SineEaseInOut)
);
Travel.Target = NewDestination.Value; // The shared target adapts both segments.
```
