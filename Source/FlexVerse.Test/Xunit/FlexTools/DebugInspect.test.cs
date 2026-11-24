// parents: @.frostline/graph/m-xunit.xml

using System.Collections.Generic;
using FlexTools.WidgetInspector.Models;
using Xunit;

namespace FlexVerse.Test.FlexTools
{
    /// <summary>
    /// Tests the Unity-free widget-debugger models. The sources are compiled
    /// directly into the test project (see the csproj comment): FlexTools
    /// itself cannot be referenced from a net10 test project because its
    /// Release build requires the local FlexVerse NuGet package and its Debug
    /// build imports the mod deployment targets. Only members whose code paths
    /// never touch RimWorld/Verse/UnityEngine are exercised.
    /// </summary>
    public class DebugInspectTests
    {
        [Fact]
        public void DebugBehaviorNode_ExposesConstructorValuesVerbatim()
        {
            var parameters = new Dictionary<string, string>
            {
                ["duration"] = "1.5",
                ["target"] = "42",
            };

            var node = new DebugBehaviorNode(
                typeName: "StaticBehavior<float>",
                parameters: parameters,
                refCount: 3,
                liveBehaviorId: 77
            );

            Assert.Equal("StaticBehavior<float>", node.TypeName);
            Assert.Equal(3, node.RefCount);
            Assert.Equal(77, node.LiveBehaviorId);
            Assert.Equal("1.5", node.Parameters["duration"]);
            Assert.Equal("42", node.Parameters["target"]);
        }

        [Fact]
        public void DebugBehaviorNode_NullParametersBecomesEmptyReadOnlyDictionary()
        {
            var node = new DebugBehaviorNode("TweenBehavior<float>", null, 0, 1);

            Assert.NotNull(node.Parameters);
            Assert.Empty(node.Parameters);

            Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(node.Parameters);
        }

        [Fact]
        public void DebugPropertyNode_ExposesNameKindAndDisplayValue()
        {
            var node = new DebugPropertyNode("Speed", "Prop", "42.0");

            Assert.Equal("Speed", node.Name);
            Assert.Equal("Prop", node.Kind);
            Assert.Equal("42.0", node.ValueDisplay);
            Assert.Null(node.BehaviorNode);
        }

        [Fact]
        public void DebugPropertyNode_OptionalBehaviorNode_IsPreserved()
        {
            var behaviorNode = new DebugBehaviorNode("NullBehavior<float>", null, 1, 9);
            var node = new DebugPropertyNode("Alpha", "State", "disabled", behaviorNode);

            Assert.Same(behaviorNode, node.BehaviorNode);
        }

        [Fact]
        public void HierarchyItemState_DefaultsToCollapsedAndUnselected()
        {
            var state = new HierarchyItemState();

            Assert.False(state.IsExpanded);
            Assert.False(state.IsSelected);
        }

        [Fact]
        public void HierarchyItemState_IsExpandedAndIsSelected_AreIndependentlyMutable()
        {
            var state = new HierarchyItemState { IsExpanded = true };

            Assert.True(state.IsExpanded);
            Assert.False(state.IsSelected);

            state.IsSelected = true;
            Assert.True(state.IsSelected);
        }
    }
}
