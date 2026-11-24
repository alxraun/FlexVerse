# KeyframeBehavior

## Definition

Interpolates a value along a timed keyframe profile with easing between points.

- `Time` is set in seconds; frames are sorted by time.
- Before the first frame, movement starts from the received Value; the frame's easing defines the transition from the previous frame to it.
- `Duration` determines the end of execution; at the end, Value becomes the value of the last frame.
- A `Target` change sets only the last value, keeping times, intermediate points, and progress.
- Supported types are `int`, `float`, `Vector2`, `Color`, and numeric `Length`; set the profile and duration before activation.

```text
Behavior<T> → KeyframeBehavior<T>
  Value: T                              current value
  Target: T                             value of the last frame
  Duration: float = 1                    duration, seconds
  Keyframes: IReadOnlyList<Keyframe<T>>   profile
  State: Inactive | Active
  IsUpdating: bool

Keyframe<T>
  Time: float                           time, seconds
  Value: T                              value of the point
  Easing: EasingFunction                 transition to the point; null → Linear
                                        before the first frame — Linear

Animate.Keyframes<T>(float duration, params Keyframe<T>[] keyframes)
  → KeyframeBehavior<T>
new KeyframeBehavior<T> { Duration = …, Keyframes = … }

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
OnUpdate: normalized time of the whole profile, without easing
Methods: Clone() · CloneUntyped() · SemEquals(Behavior) · UnionWith(Behavior)
Owner lifecycle: Activate() · Update(float) · Reset()
```

## Examples

Widget slices; unshown properties are existing State/Props.

### AnticipationAndOvershoot

```csharp
private void HandlePlay() => Envelope = Animate.Keyframes<float>(0.9f,
    new() { Time = 0f, Value = 0f },
    new() { Time = 0.15f, Value = -0.1f },
    new() { Time = 0.5f, Value = 1.15f, Easing = Easing.CubicEaseOut },
    new() { Time = 0.9f, Value = 1f, Easing = Easing.SineEaseInOut }
);
```

### LiveProfileBlend

```csharp
// Envelope and Alternate are stored Keyframe profiles in State.
[State] private Behavior<float> Blend { get; set; } = Animate.State(0f, 0.3f);
[State] private Behavior<float> Mixed { get; set; }

public override void OnMount() => Mixed = Group.Combine(Envelope, Alternate, Blend,
    (first, second, blend) => Mathf.LerpUnclamped(first, second, blend));

private void HandleAlternate() => Blend.Target = 1f;

public override IEnumerable<Widget> BuildRenderTree() =>
[
    new Label { Content = Mixed.Select(value => $"{value:P0}") },
    new Button { Label = "Alternate profile", OnClick = new Action(HandleAlternate) },
];
```

### AdaptiveTerminalSize

```csharp
Width = Animate.Keyframes<Length>(0.8f,
    new() { Time = 0f, Value = 80.Point() },
    new() { Time = 0.2f, Value = 48.Point() },
    new() { Time = 0.8f, Value = 160.Point() }
);
Width.Target = RequestedWidth.Value.Point(); // Only the final point changes.
```

### AuthoredRouteAndSampling

```csharp
Route = Group.Sequence<Vector2>(
    Animate.Keyframes<Vector2>(0.7f,
        new() { Time = 0.2f, Value = new Vector2(-12f, -8f) },
        new() { Time = 0.7f, Value = new Vector2(80f, 0f), Easing = Easing.CubicEaseOut }
    ),
    Spring.To(Home.Value, tension: 60f, friction: 14f)
);
Sampling = Effect.Create(() => OnSample.Value?.Invoke(Route.Value), Route);
```

### RepeatingCharge

```csharp
Timeline.While(() => Held.Value,
    Timeline.Set(Refs.Energy, Animate.Keyframes<float>(1.2f,
        new() { Time = 0f, Value = 0f },
        new() { Time = 0.25f, Value = 1f, Easing = Easing.CubicEaseOut },
        new() { Time = 1.2f, Value = 0f }
    ))
)
```
