// parents: @.frostline/graph/m-pickle.xml

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RimWorks.Pickle;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Pickle
{
    public partial class DebuggerProbeBase : Widget
    {
        [Prop]
        public virtual Behavior<float> InheritedSpeed { get; set; } = 3f;
    }

    public partial class DebuggerProbeRoot : DebuggerProbeBase
    {
        public override Behavior<float> InheritedSpeed
        {
            get => base.InheritedSpeed;
            set => base.InheritedSpeed = value;
        }

        [Prop]
        public Behavior<float> Speed { get; set; } = 2.5f;

        [State]
        public Behavior<float> Counter { get; set; } = 1f;

        [State]
        public Behavior<bool> ShowChild { get; set; } = true;

        [FVP]
        public Behavior<float> Baseline { get; set; } = 4f;
        public Behavior<float> Unannotated { get; set; } = 99f;

        public override IEnumerable<Widget> BuildRenderTree() =>
            ShowChild.Value
                ?
                [
                    new View
                    {
                        Key = "live-child",
                        Width = 80f.Point(),
                        Height = 24f.Point(),
                    },
                ]
                : [];
    }

    [PickleSteps]
    public static class WidgetInspectorSteps
    {
        private static Type ToolsType(string name) =>
            Type.GetType("FlexTools.WidgetInspector." + name + ", FlexTools", throwOnError: true);

        private static object Read(object target, string name) =>
            target.GetType().GetProperty(name).GetValue(target, null);

        private static object Call(object target, string name, params object[] args) =>
            target.GetType().GetMethod(name).Invoke(target, args);

        private static object[] Items(object target, string name) =>
            ((IEnumerable)Read(target, name)).Cast<object>().ToArray();

        private static object Property(object node, string name) =>
            Items(node, "Properties").Single(property => (string)Read(property, "Name") == name);

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private sealed class ProbeWindow : Window
        {
            public bool Done;
            public Exception Failure;

            public override void DoWindowContents(Rect rect)
            {
                if (Done || Event.current.type != EventType.Repaint)
                    return;
                try
                {
                    Check(rect);
                }
                catch (Exception exception)
                {
                    Failure = exception;
                }
                finally
                {
                    Done = true;
                }
            }
        }

        private static void Check(Rect rect)
        {
            if (!LoadedModManager.RunningMods.Any(mod => mod.PackageId == "flexverse.tools"))
            {
                Require(
                    Type.GetType("FlexTools.WidgetInspector.WidgetInspector, FlexTools", false)
                        == null,
                    "disabled optional tooling assembly was unexpectedly loaded"
                );
                var baseline = new WidgetEngine("debugger-pickle-baseline");
                try
                {
                    baseline.ProcessUI(new DebuggerProbeRoot(), rect);
                    var root = (DebuggerProbeRoot)baseline.GetRoot();
                    Require(
                        root.Speed.Value == 2.5f && baseline.GetChildren(root).Count == 1,
                        "the target tree must work without optional tooling"
                    );
                    Log.Info("M-PICKLE-DEBUGGER", "debugger.absent.verified");
                }
                finally
                {
                    baseline.Shutdown();
                }
                return;
            }
            var engine = new WidgetEngine("debugger-pickle");
            var tracker = Activator.CreateInstance(ToolsType("Tracking.ChangeTracker"));
            var inspector = Activator.CreateInstance(
                ToolsType("Inspection.WidgetInspector"),
                tracker
            );
            var definition = new DebuggerProbeRoot { Width = 220f.Point(), Height = 100f.Point() };
            try
            {
                engine.ProcessUI(definition, rect);
                var root = (DebuggerProbeRoot)engine.GetRoot();
                Require(root.Children.Value.Count == 0, "probe must have no declarative Children");
                var first = Call(inspector, "Inspect", engine);
                engine.ProcessUI(definition, rect);
                Require(
                    !engine.DidLayoutCalculationHappenThisCycle,
                    "inspection forced an extra layout pass"
                );
                var child = Items(first, "Children").Single();
                Require(
                    ReferenceEquals(Call(child, "GetTarget"), engine.GetChildren(root).Single()),
                    "debugger did not inspect the live generated child instance"
                );
                Require(
                    (string)Read(Property(first, "Speed"), "ValueDisplay") == "2.5",
                    "derived generated Prop did not expose its current value"
                );
                Require(
                    (string)Read(Property(first, "Counter"), "Kind") == "State",
                    "State misclassified"
                );
                Require((string)Read(Property(first, "Baseline"), "Kind") == "FVP", "FVP missing");
                Require(
                    (string)Read(Property(first, "Width"), "Kind") == "Prop",
                    "base Prop missing"
                );
                Require(
                    !Items(first, "Properties")
                        .Any(property => (string)Read(property, "Name") == "Unannotated"),
                    "unannotated member entered inspection"
                );
                Require(
                    (string)Read(Property(first, "InheritedSpeed"), "ValueDisplay") == "3",
                    "unannotated override lost its inherited reactive category"
                );
                var behavior = Read(Property(first, "Speed"), "BehaviorNode");
                var parameters = (IReadOnlyDictionary<string, string>)Read(behavior, "Parameters");
                Require(
                    parameters["State"] == "Active" && parameters["IsUpdating"] == "False",
                    "idle stability was not presented using the public behavior lifecycle"
                );
                Require(Read(behavior, "RefCount") == null, "unknown reference count was invented");

                Call(tracker, "Clear");
                root.Counter = 7f;
                Require(
                    (float)Call(tracker, "GetHeat", root, 0.5f) > 0.9f,
                    "slot replacement was not tracked"
                );
                Call(tracker, "Clear");
                root.Counter.Target = 8f;
                Require(
                    (float)Call(tracker, "GetHeat", root, 0.5f) > 0.9f,
                    "WatchRef missed an update after rebinding"
                );
                Require(
                    (float)Call(tracker, "GetHeat", root, 0f) == 0f,
                    "zero heat duration was not handled"
                );
                var second = Call(inspector, "Inspect", engine);
                Require(
                    (int)Read(first, "InstanceID") == (int)Read(second, "InstanceID"),
                    "stable widget identity was lost"
                );
                Require(
                    (string)Read(Property(first, "Counter"), "ValueDisplay") == "1",
                    "previous snapshot was mutated"
                );
                Require(
                    (string)Read(Property(second, "Counter"), "ValueDisplay") == "8",
                    "new snapshot is stale"
                );

                var originalMatrix = GUI.matrix;
                var box = Activator.CreateInstance(ToolsType("Overlays.BoxModelOverlay"));
                Call(box, "Draw", engine, root);
                Require(GUI.matrix == originalMatrix, "overlay leaked GUI.matrix");
                var isolated = new WidgetEngine("FlexTools.WidgetInspector.Inspector");
                try
                {
                    isolated.ProcessUI(new View(), rect);
                    Require(
                        Call(inspector, "Inspect", isolated) == null,
                        "debugger engine did not exclude plain Views"
                    );
                }
                finally
                {
                    isolated.Shutdown();
                }

                var staleSignalRan = false;
                root.ShowChild = false;
                engine.Post(
                    new SignalInfo(engine.GetChildren(root).Single(), () => staleSignalRan = true)
                );
                engine.ProcessUI(definition, rect);
                Require(
                    !staleSignalRan && engine.GetChildren(root).Count == 0,
                    "a queued signal ran after its source was removed earlier in the same pass"
                );

                CheckInspectorUi(first, second, rect);
                CheckHotkey(rect);
                Log.Info("M-PICKLE-DEBUGGER", "debugger.enabled.verified");

                ((IDisposable)inspector).Dispose();
                Call(tracker, "Clear");
                root.Counter.Target = 9f;
                Require(
                    (float)Call(tracker, "GetHeat", root, 0.5f) == 0f,
                    "shutdown leaked observation subscriptions"
                );
                engine.Shutdown();
                Require(
                    Call(inspector, "Inspect", engine) == null,
                    "destroyed tree remains inspectable"
                );
            }
            finally
            {
                ((IDisposable)inspector).Dispose();
                engine.Shutdown();
            }
        }

        private static void SetInput(object widget, string name, object value)
        {
            var property = widget.GetType().GetProperty(name);
            var behaviorType = typeof(StaticBehavior<>).MakeGenericType(
                property.PropertyType.GetGenericArguments()[0]
            );
            var behavior = Activator.CreateInstance(behaviorType);
            behaviorType.GetProperty("Target").SetValue(behavior, value, null);
            property.SetValue(widget, behavior, null);
        }

        private static IEnumerable<Widget> Descendants(WidgetEngine engine, Widget widget)
        {
            yield return widget;
            foreach (var child in engine.GetChildren(widget))
            foreach (var descendant in Descendants(engine, child))
                yield return descendant;
        }

        private static void CheckInspectorUi(object snapshot, object updatedSnapshot, Rect rect)
        {
            const string uiKey = "FlexTools.WidgetInspector.Inspector";
            var ui = EngineRegistry.GetOrCreate(uiKey);
            var definition = (Widget)Activator.CreateInstance(ToolsType("Widgets.InspectorApp"));
            SetInput(definition, "RootNode", snapshot);
            SetInput(definition, "EngineKeys", new string[] { "debugger-pickle" });
            SetInput(definition, "EngineKey", "debugger-pickle");
            SetInput(
                definition,
                "Settings",
                Activator.CreateInstance(ToolsType("InspectorSettings"))
            );
            SetInput(definition, "ShowBoxModel", true);
            SetInput(definition, "ShowHeatmap", true);
            SetInput(definition, "HeatDecayDuration", 0.5f);
            SetInput(definition, "ActiveFilters", new HashSet<string> { "State" });
            try
            {
                for (var i = 0; i < 4; i++)
                    ui.ProcessUI(definition, rect);
                var labels = Descendants(ui, ui.GetRoot())
                    .OfType<Label>()
                    .Select(label => label.Content.Value)
                    .ToArray();
                Require(
                    labels.Contains("STRUCTURE") && labels.Contains("PREFERENCES"),
                    "tab labels were not copied into UI instances"
                );
                var hierarchy = Descendants(ui, ui.GetRoot())
                    .Single(widget => widget.GetType().Name == "HierarchyTree");
                var nodeId = (int)Read(snapshot, "InstanceID");
                var rowButton = Descendants(ui, hierarchy)
                    .OfType<Button>()
                    .Single(button =>
                        button.Label.Value?.Contains((string)Read(snapshot, "TypeName")) == true
                    );
                rowButton.OnMouseUp(
                    new FlexEvent(new Event { type = EventType.MouseUp, button = 0 }, rowButton)
                );
                for (var i = 0; i < 4; i++)
                    ui.ProcessUI(definition, rect);
                var propertyRow = Descendants(ui, ui.GetRoot())
                    .Single(widget =>
                        widget.GetType().Name == "PropertyRow"
                        && (string)Read(Read(Read(widget, "Property"), "Value"), "Name")
                            == "Counter"
                    );
                var propertyButton = Descendants(ui, propertyRow).OfType<Button>().Single();
                propertyButton.OnMouseUp(
                    new FlexEvent(
                        new Event { type = EventType.MouseUp, button = 0 },
                        propertyButton
                    )
                );
                for (var i = 0; i < 4; i++)
                    ui.ProcessUI(definition, rect);
                Require(
                    Descendants(ui, propertyRow)
                        .Any(widget => widget.GetType().Name == "BehaviorItem"),
                    "behavior details did not open"
                );
                definition = definition.Clone();
                SetInput(definition, "RootNode", updatedSnapshot);
                for (var i = 0; i < 5; i++)
                    ui.ProcessUI(definition, rect);
                Require(
                    (string)Read(Read(Read(propertyRow, "Property"), "Value"), "ValueDisplay")
                        == "8",
                    "open property drawer did not follow a refreshed snapshot"
                );
                Require(
                    (bool)Read(Read(propertyRow, "IsOpen"), "Value"),
                    "snapshot refresh reset drawer state"
                );
                SetInput(hierarchy, "ShowMembersNodeIds", new HashSet<int>());
                SetInput(hierarchy, "CollapsedNodeIds", new HashSet<int> { nodeId });
                for (var i = 0; i < 4; i++)
                    ui.ProcessUI(definition, rect);
                Require(
                    !Descendants(ui, ui.GetRoot())
                        .Any(widget => widget.GetType().Name == "PropertyRow"),
                    "closed member rows survived reconciliation"
                );
                SetInput(ui.GetRoot(), "ActiveTab", 1);
                for (var i = 0; i < 4; i++)
                    ui.ProcessUI(definition, rect);
                labels = Descendants(ui, ui.GetRoot())
                    .OfType<Label>()
                    .Select(label => label.Content.Value)
                    .ToArray();
                Require(
                    labels.Contains("DEBUG OVERLAYS"),
                    "active tab did not reconcile preferences"
                );
                Require(
                    Descendants(ui, ui.GetRoot()).OfType<Checkbox>().Count() == 6,
                    "preferences did not compose the native controls"
                );
                definition = definition.Clone();
                SetInput(definition, "ShowHeatmap", false);
                SetInput(definition, "HeatDecayDuration", 1.25f);
                for (var i = 0; i < 5; i++)
                    ui.ProcessUI(definition, rect);
                var slider = Descendants(ui, ui.GetRoot()).OfType<HorizontalSlider>().Single();
                Require(
                    slider.Disabled.Value && Math.Abs(slider.Value.Value - 1.25f) < 0.001f,
                    "preferences failed to update the native slider's controlled Props"
                );
            }
            finally
            {
                FVUI.Release(uiKey);
            }
        }

        private static void CheckHotkey(Rect rect)
        {
            var hook = ToolsType("Tracking.Patches.Patch_Root_OnGUI").GetMethod("Prefix");
            var windowType = ToolsType("InspectorWindow");
            var original = Event.current;
            try
            {
                Event.current = new Event { type = EventType.KeyDown, keyCode = KeyCode.F12 };
                hook.Invoke(null, null);
                Require(
                    Event.current.type == EventType.Used,
                    "F12 was not consumed by the global hotkey"
                );
                Require(
                    Find.WindowStack.Windows.Any(window => window.GetType() == windowType),
                    "global F12 did not open the debugger window"
                );
                Event.current = original;
                Find.WindowStack.Windows.Single(window => window.GetType() == windowType)
                    .DoWindowContents(rect);
                Require(
                    EngineRegistry.Contains("FlexTools.WidgetInspector.Inspector"),
                    "the real debugger window did not create its FVUI engine"
                );
                Event.current = new Event { type = EventType.KeyDown, keyCode = KeyCode.F12 };
                hook.Invoke(null, null);
                Require(
                    !Find.WindowStack.Windows.Any(window => window.GetType() == windowType),
                    "global F12 did not close the debugger window"
                );
                Require(
                    !EngineRegistry.Contains("FlexTools.WidgetInspector.Inspector"),
                    "closing through F12 leaked the debugger engine"
                );
            }
            finally
            {
                Event.current = original;
            }
        }

        [BeforeScenario]
        public static void Begin() => ScenarioMarkers.Begin("M-PICKLE-DEBUGGER");

        [AfterScenario]
        public static void End() => ScenarioMarkers.End("M-PICKLE-DEBUGGER");

        [Then("optional widget debugging preserves the live FlexVerse API")]
        public static async Task CheckDebugger(PickleContext context)
        {
            var window = new ProbeWindow();
            Find.WindowStack.Add(window);
            try
            {
                for (var frame = 0; frame < 90 && !window.Done; frame++)
                    await context.WaitFrames(1);
                context.Require(window.Done, "debugger probe never rendered");
                context.Require(
                    window.Failure == null,
                    window.Failure?.ToString() ?? "debugger probe failed"
                );
            }
            finally
            {
                Find.WindowStack.TryRemove(window, doCloseSound: false);
            }
        }
    }
}
