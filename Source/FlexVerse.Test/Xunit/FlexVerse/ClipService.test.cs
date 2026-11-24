// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// R04 geometry and native scope regression fixtures.
    /// The implementation agent does not run tests; the verification agent
    /// runs them in the Unity shim and the real Unity/Waylith hosts.
    /// </summary>
    [Collection("Renderer GUI")]
    public class ClipServiceTests
    {
        private static StaticBehavior<WidgetList> MakeChildren(WidgetList list)
        {
            var behavior = new StaticBehavior<WidgetList>();
            behavior.Value = list;
            behavior.Target = list;
            return behavior;
        }

        private sealed class ClipProbe : Widget
        {
            public ClipProbe() { }

            public bool ThrowOnDraw { get; set; }

            public override IEnumerable<Widget> BuildRenderTree() =>
                Children.Value ?? Enumerable.Empty<Widget>();

            public override void OnGUI(Rect rect)
            {
                if (ThrowOnDraw)
                    throw new InvalidOperationException("deliberate test failure");
            }

            public override Widget Clone() =>
                new ClipProbe
                {
                    Key = Key,
                    ThrowOnDraw = ThrowOnDraw,
                    Width = Width,
                    Height = Height,
                    FlexShrink = FlexShrink,
                    Overflow = Overflow,
                    ZIndex = ZIndex,
                    BorderAll = BorderAll,
                    BorderLeft = BorderLeft,
                    BorderTop = BorderTop,
                    BorderRight = BorderRight,
                    BorderBottom = BorderBottom,
                    PaddingAll = PaddingAll,
                    PositionType = PositionType,
                    PositionLeft = PositionLeft,
                    PositionTop = PositionTop,
                    Translate = Translate,
                    Scale = Scale,
                    Rotate = Rotate,
                    TransformOrigin = TransformOrigin,
                    Children = MakeChildren(
                        Children.Value != default
                            ? new WidgetList(Children.Value)
                            : new WidgetList()
                    ),
                };
        }

        [Fact]
        public void HiddenClip_UsesPaddingBoxNotContentBox()
        {
            var engine = new WidgetEngine("r04_padding_box");
            var root = new ClipProbe
            {
                Width = 100.Point(),
                Height = 80.Point(),
                BorderLeft = 10.Point(),
                BorderTop = 5.Point(),
                BorderRight = 8.Point(),
                BorderBottom = 7.Point(),
                PaddingAll = 15.Point(),
                Overflow = Overflows.Hidden,
            };

            engine.ProcessUI(root, new Rect(0f, 0f, 100f, 80f));
            var instance = engine.GetRoot();
            var rect = engine.ClipService.GetLocalChildrenClip(instance);

            Assert.Equal(new Rect(10f, 5f, 82f, 68f), rect);
            Assert.True(engine.ClipService.ContainsChildrenPoint(instance, new Vector2(11f, 6f)));
            Assert.False(engine.ClipService.ContainsChildrenPoint(instance, new Vector2(9f, 6f)));
        }

        [Fact]
        public void UndefinedLogicalBorderFallsBackButExplicitZeroOverridesPhysicalEdge()
        {
            var engine = new WidgetEngine("border_fallback");
            var root = new ClipProbe
            {
                Width = 100.Point(),
                Height = 80.Point(),
                BorderAll = 6.Point(),
                BorderLeft = 10.Point(),
                Overflow = Overflows.Hidden,
            };
            var available = new Rect(0, 0, 100, 80);
            engine.ProcessUI(root, available);
            var instance = engine.GetRoot();
            Assert.Equal(
                new Rect(10, 6, 84, 68),
                engine.ClipService.GetLocalChildrenClip(instance)
            );

            instance.BorderStart = 0.Point();
            engine.ProcessUI(root, available);
            Assert.Equal(new Rect(0, 6, 94, 68), engine.ClipService.GetLocalChildrenClip(instance));

            instance.BorderStart = Length.Undefined;
            engine.ProcessUI(root, available);
            Assert.Equal(
                new Rect(10, 6, 84, 68),
                engine.ClipService.GetLocalChildrenClip(instance)
            );
        }

        [Fact]
        public void HiddenAncestor_RejectsOutsideDescendantHitAndVisibility()
        {
            var engine = new WidgetEngine("r04_hidden_hit");
            var child = new ClipProbe
            {
                Width = 120.Point(),
                Height = 30.Point(),
                FlexShrink = 0f,
            };
            var root = new ClipProbe
            {
                Width = 100.Point(),
                Height = 60.Point(),
                Overflow = Overflows.Hidden,
                Children = MakeChildren(new WidgetList { child }),
            };

            engine.ProcessUI(root, new Rect(0f, 0f, 100f, 60f));
            var instance = engine.GetRoot();
            var childInstance = engine.GetChildren(instance).Single();

            Assert.Same(childInstance, engine.HitTester.HitTest(new Vector2(30f, 10f)));
            Assert.Null(engine.HitTester.HitTest(new Vector2(110f, 10f)));
            Assert.True(engine.ClipService.IsVisible(childInstance, new Vector2(30f, 10f)));
            Assert.False(engine.ClipService.IsVisible(childInstance, new Vector2(110f, 10f)));

            var clips = engine.ClipService.GetClips(childInstance);
            Assert.Single(clips);
            Assert.Same(instance, clips[0].Owner);
        }

        [Fact]
        public void NestedHiddenClips_UseIntersectionOfBothOwners()
        {
            var engine = new WidgetEngine("r04_nested_clips");
            var leaf = new ClipProbe
            {
                Width = 140.Point(),
                Height = 20.Point(),
                FlexShrink = 0f,
            };
            var middle = new ClipProbe
            {
                Width = 50.Point(),
                Height = 30.Point(),
                Overflow = Overflows.Hidden,
                Children = MakeChildren(new WidgetList { leaf }),
            };
            var root = new ClipProbe
            {
                Width = 100.Point(),
                Height = 60.Point(),
                Overflow = Overflows.Hidden,
                Children = MakeChildren(new WidgetList { middle }),
            };

            engine.ProcessUI(root, new Rect(0f, 0f, 100f, 60f));
            var inner = engine.GetChildren(engine.GetRoot()).Single();
            var leafInstance = engine.GetChildren(inner).Single();

            Assert.Equal(2, engine.ClipService.GetClips(leafInstance).Count);
            Assert.True(engine.ClipService.IsVisible(leafInstance, new Vector2(25f, 10f)));
            Assert.False(engine.ClipService.IsVisible(leafInstance, new Vector2(75f, 10f)));
            Assert.Same(leafInstance, engine.HitTester.HitTest(new Vector2(49f, 10f)));
            Assert.Same(engine.GetRoot(), engine.HitTester.HitTest(new Vector2(75f, 10f)));
        }

        [Fact]
        public void VisibleOverflow_DoesNotAddChildClip()
        {
            var engine = new WidgetEngine("r04_visible_clips");
            var child = new ClipProbe
            {
                Width = 140.Point(),
                Height = 20.Point(),
                FlexShrink = 0f,
            };
            var root = new ClipProbe
            {
                Width = 100.Point(),
                Height = 60.Point(),
                Overflow = Overflows.Visible,
                Children = MakeChildren(new WidgetList { child }),
            };

            engine.ProcessUI(root, new Rect(0f, 0f, 100f, 60f));
            var childInstance = engine.GetChildren(engine.GetRoot()).Single();

            Assert.Empty(engine.ClipService.GetClips(childInstance));
            Assert.True(engine.ClipService.IsVisible(childInstance, new Vector2(110f, 10f)));
            Assert.Same(childInstance, engine.HitTester.HitTest(new Vector2(110f, 10f)));
        }

        [Fact]
        public void TranslatedClip_UsesInverseOwnerWorldMatrix()
        {
            var engine = new WidgetEngine("r04_translate_clip");
            var root = new ClipProbe
            {
                Width = 80.Point(),
                Height = 50.Point(),
                Translate = new Vector2(20f, 0f),
                Overflow = Overflows.Hidden,
            };

            engine.ProcessUI(root, new Rect(0f, 0f, 80f, 50f));
            var instance = engine.GetRoot();
            var worldInside = engine.TransformService.ToScreen(instance, new Vector2(5f, 10f));
            var worldOutside = engine.TransformService.ToScreen(instance, new Vector2(-5f, 10f));

            Assert.True(engine.ClipService.ContainsChildrenPoint(instance, worldInside));
            Assert.False(engine.ClipService.ContainsChildrenPoint(instance, worldOutside));
        }

        [Fact]
        public void NativeGroupAndGuiMatrix_AreRestoredAfterChildDrawFailure()
        {
            GUI.matrix = Matrix4x4.identity;
            var engine = new WidgetEngine("r04_scope_cleanup");
            var child = new ClipProbe
            {
                Width = 25.Point(),
                Height = 25.Point(),
                ThrowOnDraw = true,
            };
            var root = new ClipProbe
            {
                Width = 60.Point(),
                Height = 60.Point(),
                Overflow = Overflows.Hidden,
                Children = MakeChildren(new WidgetList { child }),
            };

            engine.ProcessUI(root, new Rect(0f, 0f, 60f, 60f));

            Assert.Equal(Matrix4x4.identity, GUI.matrix);
            var nativeDepth = typeof(GUI).GetField(
                "_groupDepth",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            Assert.NotNull(nativeDepth);
            Assert.Equal(0, (int)nativeDepth.GetValue(null)!);
        }
    }
}
