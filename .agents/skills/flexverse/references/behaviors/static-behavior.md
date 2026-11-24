# StaticBehavior

## Definition

Sets the value instantly; a positive `Duration` holds it before completion.

- A plain `T` implicitly converts to `StaticBehavior<T>`.
- Changing an active `Target` changes `Value` immediately; repeating the same target is ignored.
- Without a duration, a target change emits `OnUpdate(1f)`; activation does not emit `OnComplete`.
- While holding, a target change keeps the timer; assigning a positive `Duration` restarts the countdown.
- `Static.Wait<T>` receives the predecessor's target through `UnionWith` when its own target is unset.

```text
Behavior<T> → StaticBehavior<T>
  Value: T                 current value
  Target: T                value being set
  Duration: float = 0      hold period, seconds
  State: Inactive | Active
  IsUpdating: bool

Static.Value<T>(T value)                  → Behavior<T>
Static.Value<T>(T value, float duration)  → Behavior<T>
Static.Wait<T>(float duration)            → Behavior<T>

Optional parameters of all factories:
  onActivate: Action
  onUpdate: Action<float>
  onStart: Action
  onComplete: Action
  onReset: Action

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
Methods: Clone() · CloneUntyped() · SemEquals(Behavior) · UnionWith(Behavior)
Owner lifecycle: Activate() · Update(float) · Reset()
```

## Examples

Widget slices; unshown properties are existing State/Props.

### ReactiveCounter

```csharp
[State] private Behavior<int> Count { get; set; } = 0;
[State] private Behavior<string> Caption { get; set; }

public override void OnMount() => Caption = Count.Select(count => $"Processed: {count}");
private void HandleStep() => Count.Target++;

public override IEnumerable<Widget> BuildRenderTree() =>
[
    new Label { Content = Caption },
    new Button { Label = "+1", OnClick = new Action(HandleStep) },
];
```

### LiveConstraint

```csharp
[State] private Behavior<float> Limit { get; set; } = 80f;

public override void OnMount()
{
    Bounded = Group.Combine(Input, Limit, (input, limit) => Mathf.Min(input, limit));
    Following = Effect.Create(() => Motion.Target = Bounded.Value, Bounded);
}

private void HandleLimitChange(float limit) => Limit.Target = limit;
```

### TimedProtocol

```csharp
Status = Group.Sequence<string>(
    Static.Value("Preparing", 0.4f),
    Static.Value("Recording", 1.2f),
    Static.Value("Done", 0.2f)
);
```

### RenewableCountdown

```csharp
[State] private StaticBehavior<int> Window { get; set; } = new() { Target = 0 };

public override void OnMount()
{
    Watch.Window.OnUpdate += progress => Remaining.Target =
        Mathf.Max(0, Mathf.CeilToInt(Window.Duration * (1f - progress)));
    Watch.Window.OnComplete += () => OnTimeout.Value?.Invoke();
}

private void HandleActivity() => Window.Duration = 5f;
```

### CaptureSettledTarget

```csharp
private void HandleHold()
{
    Motion = Spring.To(1f, tension: 55f, friction: 14f);
    Bind.Motion.OnComplete += () => Motion = Static.Wait<float>(0.4f);
}
```

Static.Wait receives the reached target through the slot replacement and holds it.

### CheckpointAndDelta

```csharp
[State] private Behavior<float> Checkpoint { get; set; } = 0f;

// OnMount; Signal holds the instance.
Delta = Group.Combine(Signal, Checkpoint, (signal, checkpoint) => signal - checkpoint);
// Scenario actions: fix the base after confirmation.
Timeline.Sequence(
    Timeline.WaitUntil(() => Confirmed.Value),
    Timeline.Call(() => Checkpoint.Target = Signal.Value)
)
```
