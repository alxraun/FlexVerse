// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexTools.WidgetInspector.Inspection;
using FlexVerse;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Scrollable tree of the inspected widget hierarchy.</summary>
    public partial class HierarchyTree : ScrollView
    {
        /// <summary>Initializes the tree layout.</summary>
        public HierarchyTree()
        {
            FlexGrow = 1f;
            FlexShrink = 1f;
            MinHeight = 0f.Point();
        }

        /// <summary>Gets or sets the root snapshot node.</summary>
        [Prop]
        public Behavior<DebugNode> RootNode { get; set; }

        /// <summary>Gets or sets the selected node identifier.</summary>
        [Prop]
        public Behavior<int> SelectedNodeId { get; set; }

        /// <summary>Gets or sets the node selection callback.</summary>
        [Prop]
        public Behavior<Action<DebugNode>> OnSelectNode { get; set; }

        /// <summary>Gets or sets the active category filters.</summary>
        [Prop]
        public Behavior<HashSet<string>> ActiveFilters { get; set; }

        /// <summary>Gets or sets the set of collapsed node identifiers.</summary>
        [State]
        public Behavior<HashSet<int>> CollapsedNodeIds { get; set; } = new HashSet<int>();

        /// <summary>Gets or sets the set of nodes showing member drawers.</summary>
        [State]
        public Behavior<HashSet<int>> ShowMembersNodeIds { get; set; } = new HashSet<int>();

        /// <summary>Builds the tree rows or an empty-state prompt.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            var root = RootNode.Value;
            if (root == null)
                return
                [
                    new Label
                    {
                        Content = "Open a FlexVerse screen to inspect its live widgets.",
                        Font = GameFont.Small,
                        PaddingAll = 12f.Point(),
                        TextAlign = UnityEngine.TextAnchor.UpperLeft,
                    },
                ];
            return BuildRows(root, 0);
        }

        private IEnumerable<Widget> BuildRows(DebugNode node, int depth)
        {
            var expanded = !CollapsedNodeIds.Value.Contains(node.InstanceID);
            var showMembers = ShowMembersNodeIds.Value.Contains(node.InstanceID);
            yield return new HierarchyItem
            {
                Key = $"Widget:{node.InstanceID}",
                Node = node,
                Depth = depth,
                IsExpanded = expanded,
                IsSelected = SelectedNodeId.Value == node.InstanceID,
                OnSelect = (Action)(
                    () =>
                    {
                        OnSelectNode.Value?.Invoke(node);
                        ShowMembersNodeIds = Toggle(ShowMembersNodeIds.Value, node.InstanceID);
                    }
                ),
                OnToggleExpand = (Action)(
                    () => CollapsedNodeIds = Toggle(CollapsedNodeIds.Value, node.InstanceID)
                ),
            };
            if (showMembers)
                yield return new HierarchyNodeProperties
                {
                    Key = $"Properties:{node.InstanceID}",
                    Node = node,
                    Depth = depth,
                    ActiveFilters = ActiveFilters,
                };
            if (expanded)
                foreach (var child in node.Children)
                foreach (var row in BuildRows(child, depth + 1))
                    yield return row;
        }

        private static HashSet<int> Toggle(HashSet<int> source, int id)
        {
            var next = new HashSet<int>(source);
            if (!next.Add(id))
                next.Remove(id);
            return next;
        }
    }
}
