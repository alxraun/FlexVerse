# View

## Definition

A container with reactive styling, child widgets, and click handling.

View: `Children`, `Tooltip`, `OnClick`, `Disabled`, `BackgroundColor`, `BorderColor`, `BackgroundImage`, `BackgroundSize`, `BackgroundPosition`, `BackgroundImageTintColor`, `BorderRadius`, `BorderTopLeftRadius`, `BorderTopRightRadius`, `BorderBottomRightRadius`, `BorderBottomLeftRadius`, `BoxShadow`

BackgroundSize: `Cover`, `Contain`, `Stretch`, `OriginalSize`

Shadow: `Offset`, `BlurRadius`, `SpreadRadius`, `Color`, `None`

`View : Widget`; defaults: `FlexDirection = Column`, `FlexShrink = 0`.

## Examples

Tree slices; unshown values are the owner's State/Props.

### DecoratedSurface

```csharp
new View
{
    PaddingAll = 12.Point(), BorderAll = 1.Point(), BorderRadius = 8f,
    BackgroundColor = new Color(0.1f, 0.12f, 0.16f), BorderColor = Color.gray,
    Children = new WidgetList { new Label { Content = Title } },
}
```

### ReactiveSplit

```csharp
new View
{
    FlexDirection = FlexDirections.Row,
    Children = new WidgetList
    {
        new View { FlexBasis = 0.Point(), MinWidth = 0.Point(), FlexGrow = Expansion },
        new View { FlexBasis = 0.Point(), MinWidth = 0.Point(), FlexGrow = 1f },
    },
}
```

### GradientAndShadow

```csharp
new View
{
    Width = 100.Percent(), Height = 64.Point(), BorderRadius = 12f,
    BackgroundImage = GradientTextures.Linear(Color.black, Color.gray),
    BackgroundSize = BackgroundSize.Stretch,
    BoxShadow = new Shadow(new Vector2(0f, 3f), 8f, 0f, new Color(0f, 0f, 0f, 0.3f)),
}
```

### SelectionAndFeedback

```csharp
new View
{
    Height = 48.Point(), Disabled = Busy,
    OnClick = new Action(() => Selected.Target = !Selected.Value),
    BackgroundColor = Group.Combine(Selected, Hover,
        (selected, hover) => Color.Lerp(selected ? Color.cyan : Color.gray, Color.white, hover * 0.2f)),
    Children = new WidgetList { new Label { Content = Title } },
}
```

`BorderRadius` rounds the surface; `Overflow.Hidden` clips descendants to a rectangle.
