# Image

## Definition

An image from a texture or asset with scaling, tinting, and rounding.

Image: `TexturePath`, `TextureInstance`, `ResizeMode`, `TintColor`, `Tooltip`, `BorderRadius`, `BorderTopLeftRadius`, `BorderTopRightRadius`, `BorderBottomRightRadius`, `BorderBottomLeftRadius`

ImageResizeMode: `ScaleToFit`, `ScaleAndCrop`, `StretchToFill`

`Image : Widget`; defaults: `ResizeMode = ScaleToFit`, `TintColor = white`, `FlexShrink = 0`.

## Examples

Tree slices; unshown values are the owner's State/Props. TexturePath points to an existing asset; TextureInstance takes priority.

### RoundedPortrait

```csharp
new Image
{
    TextureInstance = Portrait, Width = 64.Point(), Height = 64.Point(),
    ResizeMode = ImageResizeMode.ScaleAndCrop, BorderRadius = 8f,
}
```

### ReactiveIcon

```csharp
new Image
{
    TexturePath = IconPath, Width = 24.Point(), Height = 24.Point(), Tooltip = Title,
    TintColor = Group.Combine(Enabled, Accent,
        (enabled, accent) => enabled ? accent : Color.gray),
}
```

### ImageOnSurface

```csharp
new View
{
    PaddingAll = 4.Point(), BorderAll = 1.Point(), BorderColor = Accent,
    BorderRadius = 12f, BackgroundColor = Color.black,
    Children = new WidgetList
    {
        new Image
        {
            TextureInstance = Portrait, Width = 96.Point(), Height = 96.Point(),
            ResizeMode = ImageResizeMode.ScaleAndCrop, BorderRadius = 8f,
        },
    },
}
```

### AnimatedReveal

```csharp
new Image
{
    TextureInstance = Preview, Width = 160.Point(), Height = 90.Point(),
    TintColor = Reveal.Select(alpha => new Color(1f, 1f, 1f, Mathf.Clamp01(alpha))),
    Translate = Offset,
}
```
