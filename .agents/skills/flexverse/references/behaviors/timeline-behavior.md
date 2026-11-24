# TimelineBehavior

## Definition

Orders actions, waits, and parallel execution of Behaviors in separate reactive slots.

- `Create` builds a description; assigning it to a reactive slot activates the scenario.
- `Sequence` runs actions in order; `Parallel` waits for all branches to complete.
- `Set` installs a clone of a Behavior and waits for the `OnComplete` of the slot's actual value. Perform instant changes through `Call`; skip motionless tweens through `If`.
- Replacing the awaited slot interrupts the scenario. `Stop` ends control while keeping already-installed Behaviors and external effects.
- Mark a successful result with a final `Call`: `OnComplete` also reports stop and interruption.

```text
Behavior → TimelineBehavior
  State: Inactive | Active
  IsUpdating: bool
  Stop(): void

Timeline.Create(ITimelineAction rootAction) → TimelineBehavior
new TimelineBehavior(ITimelineAction rootAction)

Timeline.Sequence(params ITimelineAction[] actions)
Timeline.Parallel(params ITimelineAction[] actions)
Timeline.Set<T>(IPropertyRef<T> property, Behavior<T> behavior)
Timeline.Set(IPropertyRef property, Behavior behavior)
Timeline.Call(Action callback)
Timeline.Wait(float duration)
Timeline.WaitUntil(Func<bool> predicate)
Timeline.If(Func<bool> condition, ITimelineAction thenAction,
            ITimelineAction elseAction = null)
Timeline.While(Func<bool> condition, ITimelineAction action)
  → ITimelineAction

ITimelineAction
  Status: Ready | Running | Completed | Interrupted
  Start() · Update(float) · Stop() · Clone()

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
Methods: CloneUntyped() · SemEquals(Behavior) · UnionWith(Behavior)
Owner lifecycle: Activate() · Update(float) · Reset()
Time: Unity Time.deltaTime, seconds
```

## Examples

Widget slices; unshown properties are existing State/Props, Playback is a TimelineBehavior. Action expressions are passed to Timeline.Create.

### TimedStatus

```csharp
Timeline.Sequence(
    Timeline.Call(() => Status.Target = "Recording"),
    Timeline.Wait(2f),
    Timeline.Call(() => Status.Target = "Done")
)
```

### DataAndLayoutJoin

```csharp
Timeline.Sequence(
    Timeline.Parallel(
        Timeline.WaitUntil(() => DataReady.Value),
        Timeline.Set(Refs.Width, Animate.FromTo(80.Point(), 240.Point(), 0.4f))
    ),
    Timeline.Call(() => OnOpen.Value?.Invoke())
)
```

### RetargetAwaitedMotion

```csharp
private void HandleStart() => Playback = Timeline.Create(Timeline.Sequence(
    Timeline.Set(Refs.Position, Spring.To(Destination.Value)),
    Timeline.Call(() => OnArrive.Value?.Invoke())
));

private void HandlePointer(Vector2 point) => Position.Target = point;
```

### CancellationBranch

```csharp
public override void OnMount() =>
    Ready = Group.Combine(Loaded, CancelRequested, (loaded, cancel) => loaded || cancel);

private void HandleApply() => Playback = Timeline.Create(Timeline.Sequence(
    Timeline.WaitUntil(() => Ready.Value),
    Timeline.If(
        () => CancelRequested.Value,
        Timeline.Call(() => Status.Target = "Cancelled"),
        Timeline.Call(() => OnApply.Value?.Invoke())
    )
));
```

### ConfirmedTutorialSteps

```csharp
Timeline.While(() => Step.Value < StepCount.Value, Timeline.Sequence(
    Timeline.Call(() => OnShowStep.Value?.Invoke(Step.Value)),
    Timeline.WaitUntil(() => Confirmed.Value),
    Timeline.Call(() =>
    {
        Confirmed.Target = false;
        Step.Target++;
    })
))
```

### ObservationScope

```csharp
private void HandleBegin()
{
    Observation = Effect.Create(() => OnProgress.Value?.Invoke(Completed.Value), Completed);
    Playback = Timeline.Create(Timeline.Sequence(
        Timeline.Call(() => OnBegin.Value?.Invoke()),
        Timeline.WaitUntil(() => Finished.Value),
        Timeline.Call(() => Observation = NullBehavior.Instance)
    ));
}

private void HandleCancel()
{
    Playback.Stop();
    Observation = NullBehavior.Instance;
}
```
