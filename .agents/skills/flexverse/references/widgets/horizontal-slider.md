# HorizontalSlider

## Definition

A controlled numeric slider with bounds that passes the proposed value to the owner.

HorizontalSlider: `Value`, `MinValue`, `MaxValue`, `OnValueChange`, `Tooltip`, `Disabled`

`HorizontalSlider : Widget`; defaults: `MinValue = 0`, `MaxValue = 1`, `Width = 100.Percent()`, `FlexShrink = 1`.

## Examples

Tree slices; unshown values are the owner's State/Props.

### ControlledValue

```csharp
new HorizontalSlider
{
    Value = Volume, MinValue = 0f, MaxValue = 1f,
    OnValueChange = new Action<float>(value => Volume.Target = value),
}
```

### QuantizedInput

```csharp
new HorizontalSlider
{
    Value = Volume, Tooltip = Volume.Select(value => $"{value:P0}"),
    OnValueChange = new Action<float>(value => Volume.Target = Mathf.Round(value * 10f) / 10f),
}
```

### InputToSpring

```csharp
new HorizontalSlider
{
    Value = Requested, Disabled = Busy,
    OnValueChange = new Action<float>(value =>
    {
        Requested.Target = value;
        Motion.Target = value;
    }),
}
```

Requested stores input without delay; Motion is a spring animating the result.
