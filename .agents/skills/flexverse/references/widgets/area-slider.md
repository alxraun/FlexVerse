# AreaSlider

## Definition

A controlled two-axis slider that converts dragging into a Vector2 within the given ranges.

AreaSlider: `Value`, `XRange`, `YRange`, `OnValueChange`, `Tooltip`, `Disabled`, `Label`, `LabelColor`

`AreaSlider : Widget`; defaults: `Value = zero`, `XRange = YRange = FloatRange(0, 1)`, `Width = Height = 100.Point()`.

## Examples

Tree slices; unshown values are the owner's State/Props. FloatRange is `Verse.FloatRange`.

### ControlledPoint

```csharp
new AreaSlider
{
    Value = Point, XRange = new FloatRange(-1f, 1f), YRange = new FloatRange(-1f, 1f),
    Label = "Direction", LabelColor = Accent,
    OnValueChange = new Action<Vector2>(value => Point.Target = value),
}
```

### PointerToSpring

```csharp
new AreaSlider
{
    Value = Requested, Disabled = Busy,
    OnValueChange = new Action<Vector2>(value =>
    {
        Requested.Target = value;
        Position.Target = value;
    }),
    Label = Position.Select(point => $"{point.x:F1}, {point.y:F1}"),
}
```

Requested stores the input and Position is a spring; `Translate = Position` links the spring to the result.

### ReactiveBounds

```csharp
new AreaSlider
{
    Value = Point,
    XRange = Bounds.Select(size => new FloatRange(-size.x * 0.5f, size.x * 0.5f)),
    YRange = Bounds.Select(size => new FloatRange(-size.y * 0.5f, size.y * 0.5f)),
    OnValueChange = new Action<Vector2>(value => Point.Target = value),
}
```
