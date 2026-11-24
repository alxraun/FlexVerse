# Behavior

## Definition

The common contract of reactive value and execution; a concrete Behavior defines the semantics of Value, Target, and state transfer.

- `SemEquals` defines equivalence by implementation rules; an equal assignment keeps the previous instance.
- `next.UnionWith(old)` transfers the applicable state of the predecessor; the rules for carrying over and preserving explicitly set parameters belong to next.
- One Behavior can belong to multiple slots; releasing the last owner triggers Reset.
- `Reset` stops execution and subscriptions; create a new Behavior to run again. `ReleaseFvp` closes the owner permanently.
- `Clone` creates a separate Behavior; dependencies, delegates, and captured objects may remain shared.

```text
Behavior
  State: Inactive | Active
  IsUpdating: bool
  SemEquals(Behavior other) → bool
  UnionWith(Behavior source) → void
  CloneUntyped() → Behavior
  Activate() · Update(float deltaTime) · Reset()

Behavior<T> : Behavior
  Value: T
  Target: T
  Clone() → Behavior<T>
  T → StaticBehavior<T>

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
Active + IsUpdating=false → rest; a subsequent target change can resume execution
OnComplete → the end of an update cycle, including stop and interruption
```

Ordinary replacement of non-null Behaviors in a slot:

```text
old.SemEquals(next) == true  => slot == old
old.SemEquals(next) == false -> next.UnionWith(old) => slot == next
```

## Examples

Widget slices; unshown properties are existing State/Props.

### RetargetAndHandoff

```csharp
[State] private Behavior<float> Motion { get; set; } = Spring.State(0f);

private void HandleRetarget(float target) => Motion.Target = target;
private void HandleTween(float target) => Motion = Animate.To(target, 0.4f);
private void HandleReplay(float target) => Motion = Animate.FromTo(0f, target, 0.4f);
```

### EquivalentConfiguration

```csharp
Count = Static.Value(5);
Count = Static.Value(5); // SemEquals keeps the first instance.
```

### SharedOwnership

```csharp
[State] private Behavior<Color> Ink { get; set; } = Spring.State(Color.gray);

return
[
    new View { BackgroundColor = Ink },
    new Label { Content = "Shared state", Color = Ink },
];
```

Removing one child leaves Ink with the owner's State and the second child.

### IndependentPlans

```csharp
var plan = Group.Sequence<float>(Animate.FromTo(0f, 0.4f, 0.2f), Spring.To(1f));
Left = plan.Clone();
Right = plan.Clone();
Right.Target = 2f; // Its own chain, its own child Behaviors.
```

### ClonedComputation

```csharp
Ratio = Group.Combine(Current, Maximum,
    (current, maximum) => maximum > 0f ? (float)current / maximum : 0f);
RatioCopy = Ratio.Clone(); // A separate cache; shared Current and Maximum.
```

### RebuildBorrowedDependencies

```csharp
private void HandleSourceChange()
{
    Source = Spring.To(Destination.Value);
    Caption = Source.Select(value => $"{value:P0}");
    Notify = Effect.Create(() => OnSample.Value?.Invoke(Source.Value), Source);
}
```
