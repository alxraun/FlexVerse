// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexTools.WidgetInspector.Inspection;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Root view of the widget inspector window.</summary>
    public partial class InspectorApp : View
    {
        /// <summary>Initializes the app layout.</summary>
        public InspectorApp()
        {
            Width = 100f.Percent();
            Height = 100f.Percent();
            BackgroundColor = new Color(0.11f, 0.11f, 0.11f);
        }

        /// <summary>Gets or sets the root snapshot node.</summary>
        [Prop]
        public Behavior<DebugNode> RootNode { get; set; }

        /// <summary>Gets or sets the inspectable engine keys.</summary>
        [Prop]
        public Behavior<IReadOnlyList<string>> EngineKeys { get; set; }

        /// <summary>Gets or sets the selected engine key.</summary>
        [Prop]
        public Behavior<string> EngineKey { get; set; }

        /// <summary>Gets or sets the engine selection callback.</summary>
        [Prop]
        public Behavior<Action<string>> OnSelectEngine { get; set; }

        /// <summary>Gets or sets the selected node identifier.</summary>
        [Prop]
        public Behavior<int> SelectedNodeId { get; set; }

        /// <summary>Gets or sets the node selection callback.</summary>
        [Prop]
        public Behavior<Action<DebugNode>> OnSelectNode { get; set; }

        /// <summary>Gets or sets the inspector settings.</summary>
        [Prop]
        public Behavior<InspectorSettings> Settings { get; set; }

        /// <summary>Gets or sets whether the box model overlay is shown.</summary>
        [Prop]
        public Behavior<bool> ShowBoxModel { get; set; }

        /// <summary>Gets or sets whether the update heatmap is shown.</summary>
        [Prop]
        public Behavior<bool> ShowHeatmap { get; set; }

        /// <summary>Gets or sets the heatmap decay duration in seconds.</summary>
        [Prop]
        public Behavior<float> HeatDecayDuration { get; set; }

        /// <summary>Gets or sets the active property category filters.</summary>
        [State]
        public Behavior<HashSet<string>> ActiveFilters { get; set; } =
            new HashSet<string> { "Prop", "State", "FVP", "InheritedContext" };

        /// <summary>Gets or sets the active tab index.</summary>
        [State]
        public Behavior<int> ActiveTab { get; set; } = 0;

        /// <summary>Builds the header, context bar, content, and footer.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new InspectorHeader
                {
                    FlexShrink = 0,
                    ActiveTab = ActiveTab,
                    OnTabChanged = (Action<int>)(tab => ActiveTab = tab),
                },
                new Button
                {
                    Label = EngineKey.Select(key =>
                        string.IsNullOrEmpty(key) ? "No active FlexVerse UI" : $"Context: {key}  ▸"
                    ),
                    Height = 30f.Point(),
                    FlexShrink = 0,
                    Disabled = EngineKeys.Select(keys => keys == null || keys.Count < 2),
                    OnClick = (Action)NextEngine,
                },
                ActiveTab.Value == 0
                    ? new HierarchyTree
                    {
                        Key = EngineKey.Value,
                        RootNode = RootNode,
                        SelectedNodeId = SelectedNodeId,
                        OnSelectNode = OnSelectNode,
                        ActiveFilters = ActiveFilters,
                    }
                    : new SettingsView
                    {
                        Settings = Settings,
                        ShowBoxModel = ShowBoxModel,
                        ShowHeatmap = ShowHeatmap,
                        HeatDecayDuration = HeatDecayDuration,
                        ActiveFilters = ActiveFilters,
                        OnFiltersChanged =
                            (Action<HashSet<string>>)(filters => ActiveFilters = filters),
                    },
                new InspectorFooter { Height = 24f.Point(), FlexShrink = 0 },
            ];
        }

        private void NextEngine()
        {
            var keys = EngineKeys.Value;
            if (keys == null || keys.Count < 2)
                return;
            var index = 0;
            for (var i = 0; i < keys.Count; i++)
                if (keys[i] == EngineKey.Value)
                    index = i;
            OnSelectEngine.Value?.Invoke(keys[(index + 1) % keys.Count]);
        }
    }
}
