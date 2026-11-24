# SequenceBehavior

## Definition

Runs Behaviors of one value in sequence, transferring state between stages and adapting intermediate targets to the shared target.

- The next stage starts on the current stage's `OnComplete`, or immediately when activation did not start an update.
- Each stage keeps its own semantics: tween, spring, hold, or nested sequence.
- A `Target` change keeps the current Value and distributes targets according to `Strategy`; the last typed target matches the shared target.
- `Restart` selects replay from the first stage or continuation; a completed chain also accepts a target change.
- Sequence owns its child Behaviors; Timeline owns actions and individual slots.

```text
Behavior<T> → SequenceBehavior<T>
  Value: T                         current value of the chain
  Target: T                        shared target
  Strategy: SequenceStrategy       defaults to Proportional
  Children: IReadOnlyList<Behavior> read-only
  Behaviors: IEnumerable<Behavior>  write-only
  State: Inactive | Active
  IsUpdating: bool

Group.Sequence<T>(params Behavior<T>[] behaviors)
Group.Sequence<T>(IReadOnlyList<Behavior<T>> behaviors)
Group.Sequence<T>(IReadOnlyList<Behavior<T>> behaviors, SequenceStrategy strategy,
  onActivate: Action = null, onUpdate: Action<float> = null,
  onStart: Action = null, onComplete: Action = null, onReset: Action = null)
  → SequenceBehavior<T>

new SequenceBehavior<T>(params Behavior[] behaviors)

SequenceStrategy
  Elasticity: float                distribution of intermediate targets
  Anchor: float                    anchor of the distribution
  Restart: bool                    replay from the first stage
  Proportional = { Elasticity=1, Anchor=0, Restart=true }
  FixedOffset  = { Elasticity=0, Anchor=1, Restart=true }
  LastOnly     = { Elasticity=0, Anchor=0, Restart=true }

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
Methods: Clone() · CloneUntyped() · SemEquals(Behavior) · UnionWith(Behavior)
Owner lifecycle: Activate() · Update(float) · Reset()
```

## Examples

Widget slices; unshown properties are existing State/Props. Route is a `SequenceBehavior<Vector2>` in State.

### ApproachAndDock

```csharp
[State] private SequenceBehavior<Vector2> Route { get; set; }

public override void OnMount() => Route = Group.Sequence<Vector2>(
    Animate.To(Approach.Value, 0.4f, Easing.CubicEaseOut),
    Spring.To(Destination.Value, tension: 70f, friction: 14f)
);
```

### InteractiveStrategy

```csharp
private void HandleTarget(Vector2 destination)
{
    Route.Strategy = new SequenceStrategy
    {
        Elasticity = Elasticity.Value, Anchor = Anchor.Value, Restart = false,
    };
    Route.Target = destination;
}
```

### PolicyFromMode

```csharp
// AuthoredStages holds non-executing stage descriptions.
Route = Group.Sequence<Vector2>(
    AuthoredStages.Value.Select(stage => stage.Clone()).ToArray(),
    ShiftedRoute.Value ? SequenceStrategy.FixedOffset : SequenceStrategy.Proportional
);
Route.Target = Destination.Value;
```

### LiveTerminalWithoutReplay

```csharp
var policy = SequenceStrategy.LastOnly;
policy.Restart = false;
Route.Strategy = policy;
Following = Effect.Create(() => Route.Target = Dock.Value, Dock);
```

After the earlier stages finish, a changing Dock keeps steering the last one; earlier stages do not replay.

### NestedProfileAndLayout

```csharp
Motion = Group.Sequence<float>(
    Animate.Keyframes<float>(0.4f,
        new() { Time = 0.1f, Value = -0.1f }, new() { Time = 0.4f, Value = 1f }),
    Group.Sequence<float>(Animate.To(0.7f, 0.2f), Spring.To(1f))
);
Motion.Target = Amplitude.Value;
Width = Group.Combine(Motion, AvailableWidth,
    (motion, width) => (Mathf.Max(0f, motion) * width).Point());
```

### IndependentFollowers

```csharp
var plan = Group.Sequence<Vector2>(Animate.To(Corner.Value, 0.2f), Spring.To(Destination.Value));
Left = plan.Clone();
Right = plan.Clone();
Left.Target = LeftDock.Value;
Right.Target = RightDock.Value;
```
