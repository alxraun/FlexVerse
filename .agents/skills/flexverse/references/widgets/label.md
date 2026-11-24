# Label

## Definition

Reactive text with content measurement, wrapping, ellipsis, and tooltips.

Label: `Content`, `Font`, `TextAlign`, `TextTransform`, `TextWrap`, `TextOverflow`, `Color`, `Tooltip`, `AutoTooltip`

TextTransformMode: `None`, `Uppercase`, `Lowercase`, `Capitalize`

TextOverflowMode: `Clip`, `Ellipsis`

`Label : Widget`; defaults: `Font = Small`, `TextAlign = MiddleCenter`, `TextWrap = true`, `FlexShrink = 0`.

## Examples

Tree slices; unshown values are the owner's State/Props.

### ComputedCaption

```csharp
new Label
{
    Content = Group.Combine(Completed, Total, (completed, total) => $"{completed} / {total}"),
    TextAlign = TextAnchor.MiddleLeft,
}
```

### EllipsisAndTooltip

```csharp
new Label
{
    Content = Title, FlexGrow = 1f, MinWidth = 0.Point(),
    TextWrap = false, TextOverflow = TextOverflowMode.Ellipsis, AutoTooltip = true,
}
```

### AnimatedTypography

```csharp
new Label
{
    Content = Caption, Font = GameFont.Medium, TextTransform = TextTransformMode.Uppercase,
    Color = Group.Combine(Accent, Reveal,
        (accent, reveal) => accent * new Color(1f, 1f, 1f, Mathf.Clamp01(reveal))),
    Scale = Pulse.Select(pulse => Vector2.one * (1f + pulse * 0.05f)),
}
```

### WrappedDescription

```csharp
new Label
{
    Content = Description, Width = 320.Point(),
    TextWrap = true, TextAlign = TextAnchor.UpperLeft, Font = GameFont.Small,
}
```

Ellipsis works without wrapping; AutoTooltip shows the full truncated text when Tooltip is not set.
