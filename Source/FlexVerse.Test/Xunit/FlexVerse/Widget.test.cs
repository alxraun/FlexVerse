// parents: @.frostline/graph/m-xunit.xml

using System;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// Core tests for the reflection-free Widget.GetProperty accessor
    ///. The base Widget contract exposes [Prop]/[State]
    /// members through the generated Refs string indexer.
    /// </summary>
    public class WidgetPropertyTests
    {
        private sealed class StubWidget : Widget { }

        [Fact]
        public void GetProperty_ExistingKey_ReturnsMatchingRef()
        {
            var widget = new StubWidget();

            var childrenRef = widget.GetProperty("Children");

            Assert.NotNull(childrenRef);
            Assert.Equal("Children", childrenRef.Name);
            Assert.Same(widget, childrenRef.Owner);

            var widthRef = widget.GetProperty("Width");
            Assert.NotNull(widthRef);
            Assert.Equal("Width", widthRef.Name);
        }

        [Fact]
        public void GetProperty_UnknownKey_ReturnsNull()
        {
            var widget = new StubWidget();

            Assert.Null(widget.GetProperty("NoSuchProperty"));
            Assert.Null(widget.GetProperty(string.Empty));
        }

        [Fact]
        public void ChildrenRenderingHooks_DefaultsAreNoOps()
        {
            var widget = new StubWidget();

            widget.BeginChildrenGUI(new Rect(0, 0, 10, 10), new Rect(0, 0, 10, 10));
            widget.EndChildrenGUI();
        }
    }
}
