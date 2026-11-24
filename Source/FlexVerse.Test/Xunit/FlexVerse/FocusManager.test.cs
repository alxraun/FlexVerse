// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// FocusManager adapter semantics over the stateful IMGUI focus shim:
    /// stable engine-scoped control names, foreign-focus isolation,
    /// native focus round-trip, and per-frame liveness pruning
    ///.
    /// </summary>
    [Collection("IMGUI focus")]
    public class FocusManagerTests
    {
        private static FocusManager NewManager(string key) => new WidgetEngine(key).FocusManager;

        private sealed class ProbeWidget : Widget { }

        public FocusManagerTests()
        {
            GUI.FocusControl(null);
        }

        [Fact]
        public void Register_IsStableAcrossRepeatedCalls()
        {
            var manager = NewManager("focus_stable");
            var widget = new ProbeWidget();

            var first = manager.Register(widget);
            var second = manager.Register(widget);

            Assert.Equal(first, second);
        }

        [Fact]
        public void Register_IsPrefixedWithEngineKey()
        {
            var manager = NewManager("focus_prefix");

            var name = manager.Register(new ProbeWidget());

            Assert.StartsWith("FV:focus_prefix:", name, StringComparison.Ordinal);
        }

        [Fact]
        public void Register_TwoWidgets_DistinctNames()
        {
            var manager = NewManager("focus_distinct");

            var a = manager.Register(new ProbeWidget());
            var b = manager.Register(new ProbeWidget());

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void GetFocusedWidget_WithoutNativeFocus_ReturnsNull()
        {
            var manager = NewManager("focus_no_focus");

            manager.Register(new ProbeWidget());

            Assert.Null(manager.GetFocusedWidget());
        }

        [Fact]
        public void GetFocusedWidget_ForeignControlName_ReturnsNull()
        {
            var manager = NewManager("focus_foreign");

            manager.Register(new ProbeWidget());
            GUI.FocusControl("SomeOtherEngineControl");

            Assert.Null(manager.GetFocusedWidget());
        }

        [Fact]
        public void RequestFocus_RoundTripsThroughNativeFocus()
        {
            var manager = NewManager("focus_roundtrip");
            var widget = new ProbeWidget();

            manager.Register(widget);
            manager.RequestFocus(widget);

            Assert.Same(widget, manager.GetFocusedWidget());
            Assert.True(manager.HasFocus(widget));
        }

        [Fact]
        public void Unregister_FocusedWidget_ReleasesNativeFocus()
        {
            var manager = NewManager("focus_release");
            var widget = new ProbeWidget();

            manager.Register(widget);
            manager.RequestFocus(widget);
            manager.Unregister(widget);

            Assert.Equal(string.Empty, GUI.GetNameOfFocusedControl());
            Assert.Null(manager.GetFocusedWidget());
        }

        [Fact]
        public void EndFrame_PrunesWidgetNotRegisteredThisFrame()
        {
            var manager = NewManager("focus_prune");
            var stale = new ProbeWidget();

            manager.BeginFrame();
            var originalName = manager.Register(stale);
            manager.EndFrame();

            manager.RequestFocus(stale);
            manager.BeginFrame();
            manager.EndFrame();

            GUI.FocusControl(originalName);
            Assert.Null(manager.GetFocusedWidget());

            manager.BeginFrame();
            var reminted = manager.Register(stale);
            manager.EndFrame();
            Assert.NotEqual(originalName, reminted);
        }

        [Fact]
        public void EndFrame_KeepsWidgetRegisteredThisFrame()
        {
            var manager = NewManager("focus_keep");
            var widget = new ProbeWidget();

            manager.Register(widget);
            manager.RequestFocus(widget);

            manager.BeginFrame();
            manager.Register(widget);
            manager.EndFrame();

            Assert.Same(widget, manager.GetFocusedWidget());
        }
    }

    /// <summary>
    /// Keyboard dispatch through FlexEventDispatcher: KeyDown reaches the
    /// focused widget and bubbles; without a resolvable focus target the
    /// event is left undelivered (M-FLEX-EVENTS.REQ-003).
    /// </summary>
    [Collection("IMGUI focus")]
    public class FocusKeyboardDispatchTests
    {
        private sealed class KeyProbeWidget : Widget
        {
            public readonly List<string> Facts = [];

            public override void OnKeyDown(FlexEvent e) => Facts.Add($"keydown:{e.KeyCode}");
        }

        private sealed class DispatchHarness
        {
            public readonly FlexEventDispatcher Dispatcher;
            public readonly FocusManager Focus;
            public readonly KeyProbeWidget Parent;
            public readonly KeyProbeWidget Child;

            public DispatchHarness(string key)
            {
                var engine = new WidgetEngine(key);
                var tree = new WidgetTree(engine);
                Parent = (KeyProbeWidget)tree.CreateInstance(new KeyProbeWidget(), null);
                Child = (KeyProbeWidget)tree.CreateInstance(new KeyProbeWidget(), Parent);
                var snapshot = new WidgetGeometrySnapshot();
                var transformer = new TransformService(snapshot);
                Focus = engine.FocusManager;
                Dispatcher = new FlexEventDispatcher(
                    tree,
                    new WidgetHitTester(
                        snapshot,
                        transformer,
                        new ClipService(snapshot, transformer)
                    ),
                    Focus
                );
            }
        }

        private static Event KeyDown(KeyCode keyCode) =>
            new() { type = EventType.KeyDown, keyCode = keyCode };

        public FocusKeyboardDispatchTests()
        {
            GUI.FocusControl(null);
        }

        [Fact]
        public void KeyDown_FocusedWidget_DeliversAndBubbles()
        {
            var harness = new DispatchHarness("key_dispatch_focus");

            harness.Focus.Register(harness.Child);
            harness.Focus.RequestFocus(harness.Child);

            harness.Dispatcher.ProcessEvents(KeyDown(KeyCode.A));

            Assert.Equal(["keydown:A"], harness.Child.Facts);
            Assert.Equal(["keydown:A"], harness.Parent.Facts);
        }

        [Fact]
        public void KeyDown_WithoutFocus_DeliversNothing()
        {
            var harness = new DispatchHarness("key_dispatch_nofocus");

            harness.Focus.Register(harness.Child);

            harness.Dispatcher.ProcessEvents(KeyDown(KeyCode.A));

            Assert.Empty(harness.Child.Facts);
            Assert.Empty(harness.Parent.Facts);
        }

        [Fact]
        public void KeyDown_ForeignFocus_DeliversNothing()
        {
            var harness = new DispatchHarness("key_dispatch_foreign");

            harness.Focus.Register(harness.Child);
            GUI.FocusControl("SomeOtherEngineControl");

            harness.Dispatcher.ProcessEvents(KeyDown(KeyCode.A));

            Assert.Empty(harness.Child.Facts);
            Assert.Empty(harness.Parent.Facts);
        }
    }
}
