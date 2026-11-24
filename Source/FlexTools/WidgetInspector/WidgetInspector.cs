// parents: @.frostline/graph/m-dbg-core.xml

using System;
using System.Linq;
using FlexTools.WidgetInspector.Inspection;
using FlexTools.WidgetInspector.Isolation;
using FlexTools.WidgetInspector.Overlays;
using FlexTools.WidgetInspector.Tracking;
using FlexTools.WidgetInspector.Widgets;
using FlexVerse;
using HarmonyLib;
using UnityEngine;
using Verse;
using Inspector = FlexTools.WidgetInspector.Inspection.WidgetInspector;

namespace FlexTools.WidgetInspector
{
    /// <summary>Tool entry point that applies observation patches and hosts the inspector window.</summary>
    [Tool("WidgetInspector", IsEnabledByDefault = true)]
    public class WidgetInspector : Tool
    {
        internal const string HarmonyCategory = "FlexTools.WidgetInspector";
        private Harmony _harmony;
        private Inspector _inspector;
        private InspectorSettings _settings;
        private InspectorWindow _window;
        private readonly ChangeTracker _changes = new();
        private readonly BoxModelOverlay _boxOverlay = new();
        private readonly FlashOverlay _flashOverlay = new();
        private string _engineKey;
        private string[] _engineKeys = Array.Empty<string>();
        private DebugNode _snapshot;
        private float _snapshotTime;
        private DebugNode _selected;

        /// <summary>Applies Harmony patches and creates the inspection session.</summary>
        public override void Initialize()
        {
            _settings = FlexToolsMod.Settings.GetToolSettings<InspectorSettings>();
            _inspector = new Inspector(_changes);
            _harmony = new Harmony(HarmonyCategory);
            _harmony.PatchCategory(typeof(WidgetInspector).Assembly, HarmonyCategory);
            FlexVerse.Log.Info("M-DBG-CORE", "debugger.initialized");
        }

        /// <summary>Removes patches, closes the window, and disposes inspection state.</summary>
        public override void Shutdown()
        {
            _harmony?.UnpatchCategory(typeof(WidgetInspector).Assembly, HarmonyCategory);
            _harmony = null;
            _window?.Close();
            _window = null;
            FVUI.Release(InspectorWindow.UiContextKey);
            _inspector?.Dispose();
            _inspector = null;
            _snapshot = null;
            _selected = null;
            _changes.Clear();
            FlexVerse.Log.Info("M-DBG-CORE", "debugger.shutdown");
        }

        /// <summary>Opens or closes the inspector window.</summary>
        public void ToggleWindow()
        {
            if (_window != null)
            {
                _window.Close();
                return;
            }
            _window = new InspectorWindow(this);
            _window.OnClosed = () =>
            {
                _window = null;
                _snapshot = null;
                _selected = null;
            };
            Find.WindowStack.Add(_window);
        }

        internal InspectorApp CreateApp()
        {
            var engines = EngineRegistry
                .List()
                .Where(engine => engine.GetRoot() != null && !InspectorIsolation.IsIsolated(engine))
                .OrderBy(engine => engine.Key)
                .ToArray();
            var engine =
                engines.FirstOrDefault(candidate => candidate.Key == _engineKey)
                ?? engines.FirstOrDefault();
            if (_engineKey != engine?.Key)
                SelectEngine(engine?.Key);

            if (engine == null)
                _snapshot = null;
            else if (_snapshot == null || Time.realtimeSinceStartup - _snapshotTime >= 0.1f)
            {
                _snapshot = _inspector.Inspect(engine);
                _snapshotTime = Time.realtimeSinceStartup;
            }
            var selectedWidget = _selected?.GetTarget();
            if (
                _selected != null
                && (
                    engine == null
                    || selectedWidget == null
                    || engine.GetNode(selectedWidget) == null
                )
            )
                _selected = null;

            var engineKeys = engines.Select(candidate => candidate.Key).ToArray();
            if (!_engineKeys.SequenceEqual(engineKeys))
                _engineKeys = engineKeys;

            return new InspectorApp
            {
                RootNode = _snapshot,
                EngineKeys = _engineKeys,
                EngineKey = _engineKey ?? "",
                OnSelectEngine = (Action<string>)SelectEngine,
                SelectedNodeId = _selected?.InstanceID ?? 0,
                OnSelectNode = (Action<DebugNode>)(node => _selected = node),
                Settings = _settings,
                ShowBoxModel = _settings.ShowBoxModel,
                ShowHeatmap = _settings.ShowHeatmap,
                HeatDecayDuration = _settings.HeatDecayDuration,
            };
        }

        internal void Observe(WidgetEngine engine)
        {
            if (InspectorIsolation.IsIsolated(engine) || Event.current?.type != EventType.Repaint)
                return;
            if (_settings.ShowHeatmap || _window != null)
                _inspector.Observe(engine);
            if (_settings.ShowHeatmap)
                _flashOverlay.Draw(engine, _changes, _settings.HeatDecayDuration);
            var selected = _selected?.GetTarget();
            if (
                _window != null
                && _settings.ShowBoxModel
                && engine.Key == _engineKey
                && selected != null
                && engine.GetNode(selected) != null
            )
                _boxOverlay.Draw(engine, selected);
        }

        private void SelectEngine(string key)
        {
            _engineKey = key;
            _snapshot = null;
            _selected = null;
        }
    }
}
