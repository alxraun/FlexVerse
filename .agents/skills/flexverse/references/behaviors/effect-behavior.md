# EffectBehavior

## Definition

Runs a side effect synchronously on selected events of its dependencies.

- By default it runs on its own activation and on dependencies' `OnUpdate`.
- Dependencies are concrete instances; the Effect observes them without activating or updating them itself.
- It runs at most once per Unity frame: repeated triggers are skipped without a deferred call.
- `Dependencies` and `Trigger` can change at runtime: subscriptions are rebuilt.
- A reactive slot owns the effect and releases subscriptions; express computed values through Computed.

```text
Behavior → EffectBehavior
  EffectCallback: Action
  Dependencies: Behavior[]
  Trigger: EffectTrigger = OnActivate | OnUpdate
  State: Inactive | Active
  IsUpdating: bool                   callback execution

Effect.Create(Action effectCallback, params Behavior[] dependencies)
Effect.Create(Action effectCallback, EffectTrigger trigger,
              params Behavior[] dependencies)
Effect.Create()
  → EffectBehavior

EffectTrigger [Flags]
  None = 0
  OnActivate = 1                    the Effect's own activation
  OnUpdate = 2                      a dependency update
  OnComplete = 4                    the end of a dependency cycle

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
Callback invocation: OnStart → action → OnUpdate(1f) → OnComplete
Methods: CloneUntyped() · SemEquals(Behavior) · UnionWith(Behavior)
Owner lifecycle: Activate() · Update(float) · Reset()
Clone: a new Effect with the same callback and dependencies
```

## Examples

Widget slices; effects are created in OnMount and stored in State. Dependencies keep their instances unless explicitly switched.

### ExternalNotification

```csharp
[State] private Behavior Notify { get; set; }

public override void OnMount() => Notify = Effect.Create(
    () => OnMode.Value?.Invoke(Mode.Value),
    EffectTrigger.OnUpdate, Mode
);
```

### ArmedThreshold

```csharp
[State] private Behavior Alarm { get; set; }

public override void OnMount() => Alarm = Effect.Create(HandleThreshold, Armed, Level, Limit);

private void HandleThreshold()
{
    if (!Armed.Value || Level.Value < Limit.Value) return;
    Armed.Target = false;
    OnAlarm.Value?.Invoke();
}
```

### ComputedRouting

```csharp
public override void OnMount()
{
    Desired = Group.Combine(Requested, Bounds,
        (requested, bounds) => Mathf.Clamp(requested, bounds.x, bounds.y));
    Routing = Effect.Create(ApplyDesired, Desired);
}

private void ApplyDesired()
{
    Position.Target = new Vector2(Desired.Value, 0f);
    Tint.Target = Desired.Value > 0f ? Color.cyan : Color.gray;
}
```

Position is a Spring, Tint is a Tween.

### SwitchObservedSource

```csharp
// Follow is an EffectBehavior in State; Knob is a Static, Program is a Tween.
Behavior<float> source = Manual.Value ? Knob : Program;
Follow.EffectCallback = () => Motion.Target = source.Value;
Follow.Dependencies = new Behavior[] { source };
Follow.Trigger = EffectTrigger.OnUpdate | EffectTrigger.OnComplete;
```

### RevisionStartsBatch

```csharp
public override void OnMount() =>
    Reaction = Effect.Create(HandleRevision, EffectTrigger.OnUpdate, Revision);

private void HandleRevision() => Playback = Timeline.Create(Timeline.While(
    () => Pending.Value > 0,
    Timeline.Sequence(
        Timeline.Call(() => OnStep.Value?.Invoke()),
        Timeline.Call(() => Pending.Target--),
        Timeline.Wait(0.1f)
    )
));
```

### SampleProfileOnCompletion

```csharp
Profile = Animate.Keyframes<float>(0.8f,
    new() { Time = 0.2f, Value = 1f }, new() { Time = 0.8f, Value = 0.4f });
Sampling = Effect.Create(
    () => OnSample.Value?.Invoke(Profile.Value),
    EffectTrigger.OnComplete, Profile
);
```
