# ScrollView

## Definition

A container with native scrolling, content measurement, and viewport clipping.

ScrollView: `Children`, `Tooltip`, `Disabled`, `ScrollHorizontal`, `ScrollVertical`, `OnScroll`, `BackgroundColor`, `Color`

`ScrollView : Widget`; defaults: `ScrollVertical = true`, `ScrollHorizontal = false`, `FlexDirection = Column`, `FlexGrow = 1`, `FlexShrink = 0`, `FlexBasis = 0.Point()`, `Overflow = Scroll`.

## Examples

Tree slices; unshown values are the owner's State/Props. The ScrollView instance holds the scroll position.

### VerticalList

```csharp
new ScrollView
{
    Width = 280.Point(), Height = 240.Point(),
    Children = new WidgetList(Enumerable.Range(0, 40).Select(index => new Label
    {
        Key = index, Content = $"Row {index + 1}", Height = 28.Point(),
    })),
}
```

### HorizontalStrip

```csharp
new ScrollView
{
    FlexDirection = FlexDirections.Row, Width = 320.Point(), Height = 80.Point(),
    ScrollHorizontal = true, ScrollVertical = false,
    Children = new WidgetList(Enumerable.Range(0, 12).Select(index => new View
    {
        Key = index, Width = 96.Point(), Height = 64.Point(), MarginRight = 8.Point(),
        Children = new WidgetList { new Label { Content = $"Card {index + 1}" } },
    })),
}
```

### ScrollToFeedback

```csharp
new ScrollView
{
    Height = 240.Point(), Children = Rows,
    OnScroll = new Action<Vector2>(position => HeaderEmphasis.Target = position.y > 0f ? 1f : 0f),
}
```

HeaderEmphasis is a spring; its projections change the header color or shadow.
