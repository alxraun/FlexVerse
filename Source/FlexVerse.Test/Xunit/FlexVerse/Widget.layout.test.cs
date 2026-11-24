// parents: @.frostline/graph/m-xunit.xml

using System;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    public class WidgetLayoutTests
    {
        public sealed class Probe : Widget
        {
            public override Widget Clone()
            {
                var clone = new Probe();
                clone.CloneMembersFromDefinition(this);
                return clone;
            }
        }

        private sealed class MutableLength : Behavior<Length>
        {
            public int Reads;
            public int Subscribers => _onUpdate?.GetInvocationList().Length ?? 0;
            public override Length Value
            {
                get
                {
                    Reads++;
                    return _value;
                }
                set => _value = value;
            }

            public MutableLength(float value)
            {
                _value = value.Point();
            }

            public void Set(float value)
            {
                _value = value.Point();
                _onUpdate?.Invoke(1);
            }

            public override void Activate() => State = Lifecycle.Active;

            public override void Update(float deltaTime) => _onUpdate?.Invoke(deltaTime);

            public override void UnionWith(Behavior source) { }

            public override bool SemEquals(Behavior other) => ReferenceEquals(this, other);

            public override Behavior<Length> Clone() => new MutableLength(_value.value);
        }

        private static readonly Rect Bounds = new(0, 0, 400, 300);

        [Fact]
        public void LayoutProp_SetAndUpdatesPreserveQueuedApplications()
        {
            var engine = new WidgetEngine();
            var definition = new Probe { Width = 100.Point(), Height = 20.Point() };
            try
            {
                engine.ProcessUI(definition, Bounds);
                var instance = engine.GetRoot();
                var behavior = new MutableLength(120);
                instance.Width = behavior;
                behavior.Set(130);
                behavior.Set(140);
                engine.ProcessUI(definition, Bounds);
                Assert.Equal(140f, engine.GetNode(instance).StyleGetWidth().value);
                Assert.Equal(3, behavior.Reads);
                Assert.True(engine.DidLayoutCalculationHappenThisCycle);
                behavior.Set(160);
                engine.ProcessUI(definition, Bounds);
                Assert.Equal(160f, engine.GetLayoutResults()[instance].width);
                Assert.Equal(4, behavior.Reads);
                Assert.Equal(1, behavior.Subscribers);
            }
            finally
            {
                engine.Shutdown();
            }
        }

        [Fact]
        public void EveryEdgeSlot_UsesItsOwnNodeMethodAndEdge()
        {
            var engine = new WidgetEngine();
            var definition = new Probe();
            try
            {
                engine.ProcessUI(definition, Bounds);
                var instance = engine.GetRoot();
                var node = engine.GetNode(instance);
                foreach (var method in new[] { "Margin", "Padding", "Border", "Position" })
                foreach (Edge edge in Enum.GetValues(typeof(Edge)))
                {
                    var property = typeof(Widget).GetProperty(method + edge);
                    if (property is null)
                        continue;
                    foreach (
                        var unit in new[] { Unit.Point, Unit.Percent, Unit.Auto, Unit.Undefined }
                    )
                    {
                        property.SetValue(instance, (Behavior<Length>)new Length(17, unit));
                        engine.ProcessUI(definition, Bounds);
                        var actual = typeof(Node)
                            .GetMethod("StyleGet" + method)!
                            .Invoke(node, new object[] { edge });
                        if (method == "Border")
                        {
                            if (unit == Unit.Point)
                                Assert.Equal(17f, (float)actual!);
                            else
                                Assert.True(
                                    float.IsNaN((float)actual!),
                                    "An unset border must preserve edge fallback."
                                );
                        }
                        else
                        {
                            var expectedUnit =
                                unit == Unit.Auto && method != "Margin" ? Unit.Undefined : unit;
                            Assert.Equal(expectedUnit, ((Length)actual!).unit);
                            if (unit is Unit.Point or Unit.Percent)
                                Assert.Equal(17f, ((Length)actual!).value);
                        }
                    }
                }
            }
            finally
            {
                engine.Shutdown();
            }
        }

        [Theory]
        [InlineData(Unit.Point, 37)]
        [InlineData(Unit.Percent, 50)]
        [InlineData(Unit.Auto, float.NaN)]
        [InlineData(Unit.Undefined, float.NaN)]
        public void DimensionsAndFlexBasis_PreserveUnits(Unit unit, float value)
        {
            var engine = new WidgetEngine();
            var length = new Length(value, unit);
            var definition = new Probe { Width = length, FlexBasis = length };
            try
            {
                engine.ProcessUI(definition, Bounds);
                var node = engine.GetNode(engine.GetRoot());
                Assert.Equal(unit == Unit.Undefined ? Unit.Auto : unit, node.StyleGetWidth().unit);
                Assert.Equal(
                    unit == Unit.Undefined ? Unit.Auto : unit,
                    node.NodeStyleGetFlexBasis().unit
                );
                if (unit is Unit.Point or Unit.Percent)
                {
                    Assert.Equal(value, node.StyleGetWidth().value);
                    Assert.Equal(value, node.NodeStyleGetFlexBasis().value);
                }
            }
            finally
            {
                engine.Shutdown();
            }
        }
    }
}
