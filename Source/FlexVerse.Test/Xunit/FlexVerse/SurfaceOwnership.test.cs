// parents: @.frostline/graph/m-xunit.xml
using System;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    [Collection("Renderer GUI")]
    public class SurfaceOwnershipTests
    {
        [Fact]
        public void ViewOwnsDecorationWhileContentWidgetsExposeContentAndNativeControls()
        {
            foreach (
                var name in new[]
                {
                    "BackgroundColor",
                    "BackgroundImage",
                    "BackgroundSize",
                    "BackgroundPosition",
                    "BackgroundImageTintColor",
                    "BorderColor",
                    "BoxShadow",
                }
            )
            {
                Assert.NotNull(typeof(View).GetProperty(name));
                foreach (
                    var type in new[]
                    {
                        typeof(Label),
                        typeof(Image),
                        typeof(Button),
                        typeof(TextField),
                    }
                )
                    Assert.Null(type.GetProperty(name));
            }
            foreach (var type in new[] { typeof(Label), typeof(Button), typeof(TextField) })
                Assert.Null(type.GetProperty("BorderRadius"));
            Assert.NotNull(typeof(Image).GetProperty("BorderRadius"));
            Assert.NotNull(typeof(Button).GetProperty("DrawButtonBackground"));
            Assert.NotNull(typeof(TextField).GetProperty("DrawTextFieldBackground"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TextFieldNativeBackgroundCanBeSuppressedWithoutMutatingSharedStyle(
            bool drawBackground
        )
        {
            var stock = Verse.Text.CurTextFieldStyle;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            stock.normal.background = texture;
            var recorded = typeof(GUI).GetField("LastTextFieldStyle")!;
            recorded.SetValue(null, null);
            var engine = new WidgetEngine("composed_text");
            try
            {
                engine.ProcessUI(
                    new View
                    {
                        Width = 140.Point(),
                        Height = 40.Point(),
                        BackgroundColor = Color.blue,
                        Children = new WidgetList
                        {
                            new TextField
                            {
                                Width = 100f.Percent(),
                                Height = 100f.Percent(),
                                Value = "edit",
                                DrawTextFieldBackground = drawBackground,
                            },
                        },
                    },
                    new Rect(0, 0, 140, 40)
                );
                var drawn = Assert.IsType<GUIStyle>(recorded.GetValue(null));
                Assert.Same(texture, stock.normal.background);
                if (drawBackground)
                    Assert.Same(stock, drawn);
                else
                {
                    Assert.NotSame(stock, drawn);
                    Assert.Null(drawn.normal.background);
                }
                var input = engine.GetChildren(engine.GetRoot())[0];
                Assert.Equal(new Rect(0, 0, 140, 40), engine.GetLayoutResults()[input]);
                Assert.Same(input, engine.HitTester.HitTest(new Vector2(20, 20)));
            }
            finally
            {
                engine.Shutdown();
                stock.normal.background = null;
                UnityEngine.Object.Destroy(texture);
            }
        }
    }
}
