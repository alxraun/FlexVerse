# Checkbox

## Definition

A controlled toggle: it proposes a new Checked through OnValueChange, and the owner accepts the value.

Checkbox: `Checked`, `OnValueChange`, `Tooltip`, `Disabled`

`Checkbox : Widget`; defaults: `Disabled = false`, `FlexShrink = 0`; native checkbox size.

## Examples

Tree slices; unshown values are the owner's State/Props.

### ControlledValue

```csharp
new Checkbox
{
    Checked = Enabled, Tooltip = "Enable",
    OnValueChange = new Action<bool>(value => Enabled.Target = value),
}
```

### ConditionalAcceptance

```csharp
new Checkbox
{
    Checked = Enabled,
    OnValueChange = new Action<bool>(value =>
    {
        if (!value || CanEnable.Value) Enabled.Target = value;
    }),
}
```

### SelectionAndFeedback

```csharp
new Checkbox
{
    Checked = Selected, Disabled = Busy,
    OnValueChange = new Action<bool>(value =>
    {
        Selected.Target = value;
        Emphasis.Target = value ? 1f : 0f;
    }),
}
```

Emphasis is a spring in State; its projections control the styling of neighboring widgets.
