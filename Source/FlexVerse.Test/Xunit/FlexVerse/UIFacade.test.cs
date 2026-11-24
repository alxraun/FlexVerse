// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// Core tests for the UI static facade. The host rect is
    /// routed to the keyed engine; the inert UnityEngine shim keeps the frame
    /// pipeline executable outside the game.
    /// </summary>
    [Collection("EngineRegistry")]
    public class UIFacadeTests
    {
        private static string FreshKey()
        {
            return "facade:" + Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Minimal concrete widget: base Widget lifecycle only, no IMGUI output.
        /// Clone is overridden because the generator (which produces clones for
        /// library widgets) does not run over the test assembly.
        /// </summary>
        private sealed class StubWidget : Widget
        {
            public override Widget Clone()
            {
                var clone = new StubWidget();
                clone.CloneMembersFromDefinition(this);
                return clone;
            }
        }

        [Fact]
        public void Render_FirstCall_CreatesEngineAndDrivesProcessUI()
        {
            EngineRegistry.Clear();
            var key = FreshKey();

            var results = FVUI.Render(key, new Rect(0f, 0f, 200f, 100f), new StubWidget());

            Assert.NotNull(results);
            Assert.IsAssignableFrom<IReadOnlyDictionary<Widget, Rect>>(results);
            Assert.True(EngineRegistry.Contains(key));
        }

        [Fact]
        public void Render_SubsequentCalls_PreservesEngineIdentity()
        {
            EngineRegistry.Clear();
            var key = FreshKey();
            var rect = new Rect(0f, 0f, 200f, 100f);
            FVUI.Render(key, rect, new StubWidget());

            FVUI.Render(key, rect, new StubWidget());

            var engine = EngineRegistry.Get(key);
            Assert.NotNull(engine);
            Assert.Same(engine, EngineRegistry.GetOrCreate(key));
        }

        [Fact]
        public void Release_RemovesEngineFromRegistry()
        {
            EngineRegistry.Clear();
            var key = FreshKey();
            FVUI.Render(key, new Rect(0f, 0f, 200f, 100f), new StubWidget());

            var released = FVUI.Release(key);

            Assert.True(released);
            Assert.False(EngineRegistry.Contains(key));
        }

        [Fact]
        public void GetEngine_ReturnsRegisteredEngineOrNull()
        {
            EngineRegistry.Clear();
            var key = FreshKey();

            Assert.Null(FVUI.GetEngine(key));

            var engine = EngineRegistry.GetOrCreate(key);

            Assert.Same(engine, FVUI.GetEngine(key));
        }
    }
}
