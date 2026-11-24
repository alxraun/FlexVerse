# SpringBehavior

## Definition

Moves a value toward a target using a spring model with mass and damping, completing when the rest threshold is reached.

- A `Target` change keeps the current velocity; duration is determined by physics and oscillation is possible.
- `To` receives the predecessor's Value; `From` receives its current Value as the target; `FromTo` sets the initial velocity explicitly.
- `Tension`, `Friction`, `Mass`, and `RestThreshold` are set before activation; to change parameters, install a new Behavior. The predecessor's velocity is not carried over on replacement.
- `OnUpdate` receives `0f`, not progress. Rest is reached when velocity and distance are below `RestThreshold`; Value then becomes Target.
- Supported types are `int`, `float`, `Vector2`, `Color`. Prefer `float` for smooth motion; derive Length through `Select`.

```text
Behavior<T> → SpringBehavior<T>
  Value: T                      current value
  Target: T                     rest point
  Tension: float = 300           stiffness
  Friction: float = 26           damping
  Mass: float = 1                mass
  RestThreshold: float = 0.001   rest threshold
  InitialVelocity: T = default   velocity at activation
  State: Inactive | Active
  IsUpdating: bool

Spring.To<T>(T targetValue, …)                                  → Behavior<T>
Spring.From<T>(T startValue, …)                                → Behavior<T>
Spring.FromTo<T>(T startValue, T targetValue, T initialVelocity, …) → Behavior<T>
Spring.State<T>(T value, …)                                    → Behavior<T>

Common parameters:
  tension: float = 300
  friction: float = 26
  mass: float = 1
  restThreshold: float = 0.001
  onActivate, onStart, onComplete, onReset: Action = null
  onUpdate: Action<float> = null

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
Methods: Clone() · CloneUntyped() · SemEquals(Behavior) · UnionWith(Behavior)
Owner lifecycle: Activate() · Update(float) · Reset()
```

## Examples

Widget slices; unshown properties are existing State/Props.

### DragAndRelease

```csharp
[State] private Behavior<Vector2> Position { get; set; } =
    Spring.State(Vector2.zero, tension: 60f, friction: 14f);

private void HandleDrag(Vector2 point) => Position.Target = point;
private void HandleRelease() => Position.Target = Home.Value;
```

### Impulse

```csharp
Position = Spring.FromTo(
    Position.Value, Vector2.zero, new Vector2(180f, -100f),
    tension: 80f, friction: 12f
);
```

### FollowerNetwork

```csharp
public override void OnMount() =>
    Following = Effect.Create(UpdateFollowers, Leader, Middle, Bias);

private void UpdateFollowers()
{
    Middle.Target = Leader.Value;
    Tail.Target = Middle.Value * 0.8f + Bias.Value;
}

private void HandleExpand() => Playback = Timeline.Create(Timeline.Sequence(
    Timeline.Call(() => Leader.Target = 1f),
    Timeline.WaitUntil(() => !Leader.IsUpdating && !Middle.IsUpdating && !Tail.IsUpdating),
    Timeline.Call(() => OnSettled.Value?.Invoke())
));
```

### AuthoredForce

```csharp
// OnMount; Position is a spring.
Force = Animate.Keyframes<float>(0.8f,
    new() { Time = 0f, Value = 0f },
    new() { Time = 0.2f, Value = 1f },
    new() { Time = 0.8f, Value = 0f }
);
Following = Effect.Create(
    () => Position.Target = new Vector2(Force.Value * 24f, 0f),
    EffectTrigger.OnActivate | EffectTrigger.OnUpdate | EffectTrigger.OnComplete, Force
);
```

### ReducedMotion

```csharp
// Handler; Motion projections are built in BuildRenderTree.
Motion = ReducedMotion.Value
    ? Static.Value(Destination.Value)
    : Spring.To(Destination.Value, tension: 50f, friction: 14f);
```
