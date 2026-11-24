// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindInstabilityMeter : Widget
    {
        [Prop]
        public Behavior<float> Level { get; set; } = 0f;

        [Prop]
        public Behavior<float> First { get; set; } = .3f;

        [Prop]
        public Behavior<float> Second { get; set; } = .6f;

        [Prop]
        public Behavior<float> Third { get; set; } = .9f;

        public RimRewindInstabilityMeter()
        {
            Height = 8f.Point();
            MarginTop = 6f.Point();
            MarginBottom = 10f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Height = 8f.Point(),
                    Width = 100f.Percent(),
                    BackgroundColor = RimRewindSkin.Raised,
                    Children = new WidgetList
                    {
                        new View
                        {
                            Height = 8f.Point(),
                            Width = Level.Select(value =>
                                (UnityEngine.Mathf.Clamp01(value) * 100f).Percent()
                            ),
                            BackgroundColor = RimRewindSkin.Amber,
                        },
                        BuildMarker(First),
                        BuildMarker(Second),
                        BuildMarker(Third),
                    },
                },
            ];

        private static Widget BuildMarker(Behavior<float> threshold) =>
            new View
            {
                PositionType = PositionTypes.Absolute,
                PositionLeft = threshold.Select(value => (value * 100f).Percent()),
                PositionTop = (-2f).Point(),
                Width = 1f.Point(),
                Height = 12f.Point(),
                BackgroundColor = RimRewindSkin.Text,
            };
    }
}
