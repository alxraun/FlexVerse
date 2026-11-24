# CanvasView

## Definition

A canvas that pans and zooms content without changing the Flexbox layout.

CanvasView: `Children`, `Pan`, `Zoom`, `MinZoom`, `MaxZoom`

`CanvasView : Widget`; defaults: `Pan = zero`, `Zoom = 1`, `MinZoom = 0.1`, `MaxZoom = 8`, `Overflow = Hidden`.

## Examples

Tree slices; unshown values are the owner's State/Props.

### InteractiveCanvas

```csharp
new CanvasView
{
    Width = 100.Percent(), Height = 320.Point(),
    MinZoom = 0.25f, MaxZoom = 4f, Children = Scene,
}
```

The middle mouse button pans the canvas; the left button does so when pressed on empty canvas. The wheel zooms relative to the pointer. Gestures change Pan/Zoom; keep the declaration stable during interaction.

### AnimatedViewport

```csharp
new CanvasView
{
    Height = 320.Point(), Children = Scene,
    Pan = Spring.FromTo(new Vector2(-120f, -80f), Vector2.zero, Vector2.zero),
    Zoom = Animate.FromTo(0.5f, 1f, 0.8f, Easing.SineEaseInOut),
}
```

### ScrollablePanelOnCanvas

```csharp
new CanvasView
{
    Height = 320.Point(),
    Children = new WidgetList
    {
        new ScrollView { Width = 240.Point(), Height = 180.Point(), Children = Rows },
    },
}
```
