// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// R07: CanvasView's visual child transform and gestures use the existing
    /// TransformService, ClipService, Widget and FlexEvent contracts. These
    /// tests are authored for the separate verification agent, not run here.
    /// </summary>
    [Collection("Renderer GUI")]
    public class CanvasViewTests
    {
        private static Event Mouse(
            EventType type,
            Vector2 at,
            int button = 0,
            Vector2 wheel = default
        ) =>
            new Event
            {
                type = type,
                mousePosition = at,
                button = button,
                delta = wheel,
            };

        private static CanvasView Canvas(params Widget[] children) =>
            new CanvasView
            {
                Width = 200f.Point(),
                Height = 120f.Point(),
                Children = new WidgetList(children),
            };

        private static View Sheet(float width = 600f, float height = 450f) =>
            new View
            {
                Key = "sheet",
                Width = width.Point(),
                Height = height.Point(),
            };

        [Fact]
        public void PanAndZoom_TransformDescendantsWithoutChangingFlexboxGeometry()
        {
            var engine = new WidgetEngine("r07_pan_zoom_geometry");
            var definition = Canvas(Sheet(40f, 30f));
            definition.Pan = new Vector2(10f, 8f);
            definition.Zoom = 2f;

            engine.ProcessUI(definition, new Rect(0f, 0f, 200f, 120f));

            var root = (CanvasView)engine.GetRoot();
            var child = engine.GetChildren(root)[0];
            var beforeLayout = engine.GetLayoutResults()[child];
            var transformed = engine.TransformService.ToScreen(child, new Vector2(5f, 5f));

            Assert.InRange(Mathf.Abs(transformed.x - 20f), 0f, 0.002f);
            Assert.InRange(Mathf.Abs(transformed.y - 18f), 0f, 0.002f);
            Assert.Same(child, engine.HitTester.HitTest(transformed));

            root.Pan = new Vector2(-20f, 15f);
            engine.RefreshGeometry();

            Assert.Equal(beforeLayout, engine.GetLayoutResults()[child]);
            Assert.Equal(new Vector2(-20f, 15f), ((CanvasView)root).Pan.Value);
            Assert.Same(root, engine.HitTester.HitTest(new Vector2(10f, 5f)));
        }

        [Fact]
        public void WheelZoom_PreservesContentPointUnderCursor()
        {
            var engine = new WidgetEngine("r07_cursor_zoom");
            var definition = Canvas(Sheet());
            definition.Pan = new Vector2(25f, -15f);
            engine.ProcessUI(definition, new Rect(0f, 0f, 200f, 120f));

            var canvas = (CanvasView)engine.GetRoot();
            var sheet = engine.GetChildren(canvas)[0];
            var contentPoint = new Vector2(45f, 55f);
            var cursor = engine.TransformService.ToScreen(sheet, contentPoint);

            var evt = new FlexEvent(
                Mouse(EventType.ScrollWheel, cursor, wheel: new Vector2(0f, -1f)),
                sheet
            );
            canvas.OnScrollWheel(evt);
            engine.RefreshGeometry();

            var after = engine.TransformService.ToScreen(sheet, contentPoint);
            Assert.True(evt.IsPropagationStopped);
            Assert.True(canvas.Zoom.Value > 1f);
            Assert.InRange(Mathf.Abs(after.x - cursor.x), 0f, 0.002f);
            Assert.InRange(Mathf.Abs(after.y - cursor.y), 0f, 0.002f);
        }

        [Fact]
        public void WheelZoom_UsesEffectivePositiveBounds()
        {
            var engine = new WidgetEngine("r07_limits");
            var definition = Canvas(Sheet());
            definition.MinZoom = 0.5f;
            definition.MaxZoom = 1.2f;
            engine.ProcessUI(definition, new Rect(0f, 0f, 200f, 120f));

            var canvas = (CanvasView)engine.GetRoot();
            var cursor = new Vector2(40f, 30f);

            canvas.OnScrollWheel(
                new FlexEvent(
                    Mouse(EventType.ScrollWheel, cursor, wheel: new Vector2(0f, -100f)),
                    canvas
                )
            );
            Assert.InRange(Mathf.Abs(canvas.Zoom.Value - 1.2f), 0f, 0.002f);

            canvas.OnScrollWheel(
                new FlexEvent(
                    Mouse(EventType.ScrollWheel, cursor, wheel: new Vector2(0f, 100f)),
                    canvas
                )
            );
            Assert.InRange(Mathf.Abs(canvas.Zoom.Value - 0.5f), 0f, 0.002f);
        }

        [Fact]
        public void PanIsUnbounded_EvenWithFiniteContent()
        {
            var engine = new WidgetEngine("r07_unbounded");
            var definition = Canvas(Sheet(1600f, 900f));
            definition.Pan = new Vector2(3000f, -2000f);
            engine.ProcessUI(definition, new Rect(0f, 0f, 200f, 120f));

            var canvas = (CanvasView)engine.GetRoot();
            var child = engine.GetChildren(canvas)[0];
            var origin = engine.TransformService.ToScreen(child, Vector2.zero);

            Assert.Equal(new Vector2(3000f, -2000f), origin);
            Assert.Equal(new Vector2(3000f, -2000f), canvas.Pan.Value);
            Assert.Same(canvas, engine.HitTester.HitTest(new Vector2(10f, 10f)));
            Assert.False(engine.ClipService.IsVisible(child, origin));
        }

        [Fact]
        public void MiddleButtonPanWorksOverChild_WhileLeftPanNeedsEmptyCanvas()
        {
            var engine = new WidgetEngine("r07_mouse_pan");
            engine.ProcessUI(Canvas(Sheet(70f, 60f)), new Rect(0f, 0f, 200f, 120f));
            var canvas = (CanvasView)engine.GetRoot();
            var sheet = engine.GetChildren(canvas)[0];

            var blocked = new FlexEvent(Mouse(EventType.MouseDown, new Vector2(10f, 10f)), sheet);
            canvas.OnMouseDown(blocked);
            canvas.OnMouseDrag(
                new FlexEvent(Mouse(EventType.MouseDrag, new Vector2(30f, 20f)), sheet)
            );

            Assert.False(blocked.IsPropagationStopped);
            Assert.Equal(Vector2.zero, canvas.Pan.Value);

            var down = new FlexEvent(
                Mouse(EventType.MouseDown, new Vector2(10f, 10f), button: 2),
                sheet
            );
            canvas.OnMouseDown(down);
            var drag = new FlexEvent(
                Mouse(EventType.MouseDrag, new Vector2(30f, 20f), button: 2),
                sheet
            );
            canvas.OnMouseDrag(drag);
            canvas.OnMouseUp(
                new FlexEvent(Mouse(EventType.MouseUp, new Vector2(30f, 20f), button: 2), sheet)
            );

            Assert.True(down.IsPropagationStopped);
            Assert.True(drag.IsPropagationStopped);
            Assert.Equal(new Vector2(20f, 10f), canvas.Pan.Value);

            var empty = new FlexEvent(Mouse(EventType.MouseDown, new Vector2(180f, 90f)), canvas);
            canvas.OnMouseDown(empty);
            canvas.OnMouseDrag(
                new FlexEvent(Mouse(EventType.MouseDrag, new Vector2(150f, 80f)), canvas)
            );
            canvas.OnMouseUp(
                new FlexEvent(Mouse(EventType.MouseUp, new Vector2(150f, 80f)), canvas)
            );
            Assert.True(empty.IsPropagationStopped);
            Assert.Equal(new Vector2(-10f, 0f), canvas.Pan.Value);
        }

        [Fact]
        public void InnerScrollViewOwnsWheelWithoutChangingCanvasZoom()
        {
            var engine = new WidgetEngine("r07_nested_scroll");
            var scroll = new ScrollView
            {
                Width = 120.Point(),
                Height = 100.Point(),
                Children = new WidgetList { Sheet(120f, 400f) },
            };
            engine.ProcessUI(Canvas(scroll), new Rect(0f, 0f, 200f, 120f));

            var canvas = (CanvasView)engine.GetRoot();
            var scrollInstance = engine.GetChildren(canvas)[0];

            var wheel = new FlexEvent(
                Mouse(EventType.ScrollWheel, new Vector2(40f, 40f), wheel: new Vector2(0f, -1f)),
                scrollInstance
            );
            canvas.OnScrollWheel(wheel);

            Assert.Equal(1f, canvas.Zoom.Value);
            Assert.Equal(Vector2.zero, canvas.Pan.Value);
            Assert.False(wheel.IsPropagationStopped);

            var down = new FlexEvent(
                Mouse(EventType.MouseDown, new Vector2(40f, 40f), button: 2),
                scrollInstance
            );
            canvas.OnMouseDown(down);
            canvas.OnMouseDrag(
                new FlexEvent(
                    Mouse(EventType.MouseDrag, new Vector2(80f, 65f), button: 2),
                    scrollInstance
                )
            );
            Assert.False(down.IsPropagationStopped);
            Assert.Equal(Vector2.zero, canvas.Pan.Value);
        }

        [Fact]
        public void ZoomedNativeButton_UsesSameVisualAndHitGeometry()
        {
            var buttonCalls = typeof(Verse.Widgets).GetField("ButtonTextCalls");
            Assert.NotNull(buttonCalls);
            buttonCalls.SetValue(null, 0);
            var engine = new WidgetEngine("r07_zoom_button");
            var button = new Button
            {
                Key = "button",
                Width = 70f.Point(),
                Height = 35f.Point(),
                Label = "Click",
            };
            var definition = Canvas(button);
            definition.Zoom = 2f;
            definition.Pan = new Vector2(15f, 10f);

            engine.ProcessUI(definition, new Rect(0f, 0f, 200f, 120f));
            var canvas = (CanvasView)engine.GetRoot();
            var buttonInstance = engine.GetChildren(canvas)[0];
            var screen = engine.TransformService.ToScreen(buttonInstance, new Vector2(15f, 10f));

            Assert.Same(buttonInstance, engine.HitTester.HitTest(screen));
            Assert.Equal(1, (int)buttonCalls.GetValue(null)!);
            var drawnMatrix = (Matrix4x4)
                typeof(Verse.Widgets).GetField("LastButtonMatrix")!.GetValue(null)!;
            var drawnRect = (Rect)typeof(Verse.Widgets).GetField("LastButtonRect")!.GetValue(null)!;
            var drawnPoint = drawnMatrix.MultiplyPoint3x4(
                new Vector3(drawnRect.x + 15f, drawnRect.y + 10f, 0)
            );
            Assert.InRange(Mathf.Abs(drawnPoint.x - screen.x), 0, 0.002f);
            Assert.InRange(Mathf.Abs(drawnPoint.y - screen.y), 0, 0.002f);
            Assert.Equal(Matrix4x4.identity, GUI.matrix);
        }

        [Fact]
        public void ExternalNonpositiveZoom_IsSafelyClampedToMinZoom()
        {
            var engine = new WidgetEngine("r07_zero_zoom");
            var definition = Canvas(Sheet(40f, 30f));
            definition.Zoom = 0f;
            definition.MinZoom = 0.25f;
            engine.ProcessUI(definition, new Rect(0f, 0f, 200f, 120f));

            var canvas = (CanvasView)engine.GetRoot();
            var child = engine.GetChildren(canvas)[0];
            var point = engine.TransformService.ToScreen(child, new Vector2(20f, 10f));

            Assert.InRange(Mathf.Abs(point.x - 5f), 0f, 0.002f);
            Assert.InRange(Mathf.Abs(point.y - 2.5f), 0f, 0.002f);
            Assert.Same(child, engine.HitTester.HitTest(point));
        }

        [Fact]
        public void CursorZoomWorksThroughTransformedAncestor()
        {
            var engine = new WidgetEngine("r07_parent_transform");
            var childCanvas = Canvas(Sheet());
            childCanvas.Width = 180.Point();
            childCanvas.Height = 100.Point();

            var parent = new View
            {
                Width = 240.Point(),
                Height = 160.Point(),
                Scale = new Vector2(1.5f, 2f),
                Translate = new Vector2(10f, -3f),
                Children = new WidgetList { childCanvas },
            };

            engine.ProcessUI(parent, new Rect(0f, 0f, 240f, 160f));

            var canvas = (CanvasView)engine.GetChildren(engine.GetRoot())[0];
            var sheet = engine.GetChildren(canvas)[0];
            var contentPoint = new Vector2(55f, 25f);
            var cursor = engine.TransformService.ToScreen(sheet, contentPoint);

            canvas.OnScrollWheel(
                new FlexEvent(
                    Mouse(EventType.ScrollWheel, cursor, wheel: new Vector2(0f, -1f)),
                    sheet
                )
            );
            engine.RefreshGeometry();

            var after = engine.TransformService.ToScreen(sheet, contentPoint);
            Assert.InRange(Mathf.Abs(after.x - cursor.x), 0f, 0.002f);
            Assert.InRange(Mathf.Abs(after.y - cursor.y), 0f, 0.002f);
        }
    }
}
