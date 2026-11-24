# ComputedBehavior

## Definition

Lazily computes and caches a value from explicitly declared Behavior dependencies.

- `Select` transforms a single dependency; `Group.Combine` merges several.
- The compute function is pure; the cache recomputes when Value is read after a dependency updates.
- Dependencies are concrete instances; owners manage their activation and lifetime. Store intermediate Computed values in reactive slots as well.
- `OnStart → OnUpdate(1f) → OnComplete` reports cache invalidation, not the completion of a computation.
- `Target` is stored separately and does not drive the computation; a direct Value write holds until the next invalidation.

```text
Behavior<TResult> → ComputedBehavior<TResult>
  Value: TResult                         lazily computed value
  Target: TResult                        independent stored value
  Dependencies: IReadOnlyList<Behavior>   dependencies, read-only
  State: Inactive | Active
  IsUpdating: bool                        invalidation notification cycle

source.Select<TIn, TOut>(Func<TIn, TOut> selector)
  → ComputedBehavior<TOut>

Group.Combine(first, second, combiner)
Group.Combine(first, second, third, combiner)
Group.Combine(first, second, third, fourth, combiner)
Group.Combine<TResult>(Func<TResult> computer, params Behavior[] dependencies)
  → ComputedBehavior<TResult>

new ComputedBehavior<TResult>(Func<TResult> computer, params Behavior[] dependencies)

Events: OnActivate · OnStart · OnUpdate(float) · OnComplete · OnReset
Methods: Clone() · CloneUntyped() · SemEquals(Behavior) · UnionWith(Behavior)
Owner lifecycle: Activate() · Update(float) · Reset()
Clone: separate cache, shared dependency instances
```

## Examples

Widget slices; unshown properties are existing State/Props. Stored graphs are assembled in OnMount; projections for children in BuildRenderTree.

### Projection

```csharp
new Label { Content = Count.Select(count => $"Counter: {count}") }
```

### DerivedProgress

```csharp
[State] private Behavior<int> Completed { get; set; } = 0;
[State] private Behavior<int> Total { get; set; } = 8;
[State] private Behavior<float> Ratio { get; set; }
[State] private Behavior<string> Caption { get; set; }

public override void OnMount()
{
    Ratio = Group.Combine(Completed, Total,
        (completed, total) => total > 0 ? (float)completed / total : 0f);
    Caption = Ratio.Select(ratio => $"Completed: {ratio:P0}");
}

private void HandleStep() => Completed.Target = Math.Min(Completed.Value + 1, Total.Value);

public override IEnumerable<Widget> BuildRenderTree() =>
[
    new Label { Content = Caption },
    new Button { Label = "Step", OnClick = new Action(HandleStep) },
];
```

### ReactiveFiltering

```csharp
Filtered = Group.Combine(Names, Query,
    (names, query) => names.Where(name => name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToArray());
MatchCount = Filtered.Select(names => names.Length);
```

### AnimatedSelection

```csharp
Cursor = Group.Sequence<float>(Animate.FromTo(0f, 3f, 0.6f), Spring.To(1f));
Preview = Group.Combine(Cursor, Names, (cursor, names) => names.Length == 0
    ? ""
    : names[Mathf.Clamp(Mathf.RoundToInt(cursor), 0, names.Length - 1)]);
Reporting = Effect.Create(() => OnPreview.Value?.Invoke(Preview.Value), Preview);
```

### FourInputGeometry

```csharp
public override void OnMount() =>
    Anchor = Group.Combine(Size, Padding, Point, Offset, ComputeAnchor);

private Vector2 ComputeAnchor(Vector2 size, float padding, Vector2 point, Vector2 offset) =>
    new Vector2(
        padding + Mathf.Max(0f, size.x - padding * 2f) * point.x,
        padding + Mathf.Max(0f, size.y - padding * 2f) * point.y
    ) + offset;
```

### ConditionalDependencies

```csharp
// Both branches are observed explicitly, regardless of the current mode.
EffectiveValue = new ComputedBehavior<float>(
    () => Expanded.Value ? Detailed.Value : Summary.Value,
    Expanded, Detailed, Summary
);
```
