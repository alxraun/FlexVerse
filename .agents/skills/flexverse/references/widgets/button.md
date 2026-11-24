# Button

## Definition

A native button with reactive label, availability, and action.

Button: `Label`, `OnClick`, `Tooltip`, `Disabled`, `DrawButtonBackground`, `Font`, `TextAlign`, `Color`

`Button : Widget`; defaults: `DrawButtonBackground = true`, `Font = Small`, `TextAlign = MiddleCenter`, `FlexShrink = 0`.

## Examples

Tree slices; unshown values are the owner's State/Props.

### ReactiveAction

```csharp
new Button
{
    Label = Running.Select(running => running ? "Stop" : "Run"),
    Disabled = Connected.Select(connected => !connected),
    OnClick = new Action(() => OnToggle.Value?.Invoke()),
}
```

### ComputedAvailability

```csharp
new Button
{
    Label = "Confirm",
    Disabled = Group.Combine(Selected, Busy, (selected, busy) => !selected || busy),
    Color = Accent, OnClick = new Action(() => OnConfirm.Value?.Invoke()),
}
```

### DecoratedNativeButton

```csharp
new View
{
    BorderRadius = 8f, BackgroundColor = Accent, PaddingAll = 4.Point(),
    Children = new WidgetList
    {
        new Button
        {
            Label = "Apply", Height = 32.Point(), DrawButtonBackground = false,
            OnClick = new Action(() => OnApply.Value?.Invoke()),
        },
    },
}
```

### DeferredAction

```csharp
private void HandleContinue() => Playback = Timeline.Create(Timeline.Sequence(
    Timeline.WaitUntil(() => Ready.Value),
    Timeline.Call(() => OnContinue.Value?.Invoke())
));
```

```csharp
new Button { Label = "Continue", OnClick = new Action(HandleContinue) }
```
