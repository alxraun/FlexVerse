// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    [Screen("Showcases/Traffic")]
    public partial class Traffic : View
    {
        [State]
        public Behavior<TrafficSkin> Skin { get; set; }

        [State]
        public Behavior Playback { get; set; }

        [State]
        public Behavior Dispatch { get; set; }

        [State]
        public Behavior<bool> DispatchFinished { get; set; } = false;
        private TrafficMotion[] _tiles;
        private TrafficFlow _traffic;

        private static readonly (string Caption, string Value)[] Metrics =
        [
            ("Revenue", "$48.2k"),
            ("Users", "9.1k"),
            ("Uptime", "99.98%"),
            ("Response", "182 ms"),
            ("Orders", "1,284"),
            ("Conversion", "4.8%"),
            ("Growth", "+24%"),
            ("Regions", "12"),
            ("Sessions", "24.6k"),
            ("Retention", "92%"),
            ("Requests", "1.2M"),
            ("Latency", "42 ms"),
            ("Deploys", "128"),
            ("Errors", "0.02%"),
            ("Storage", "84 GB"),
            ("Throughput", "2.4k/s"),
        ];

        public Traffic()
        {
            BackgroundColor = TrafficSkin.Background;
            Width = 100f.Percent();
            Height = 100f.Percent();
            FlexGrow = 1f;
            AlignItems = Align.Center;
            JustifyContent = Justify.Center;
            PaddingAll = 28f.Point();
        }

        public override void OnMount()
        {
            Skin = TrafficSkin.Create();
            _tiles = Enumerable
                .Range(0, TrafficLayouts.Count)
                .Select(index =>
                {
                    var tile = new TrafficMotion();
                    tile.Initialize(index % 4 + 1, index / 4);
                    return tile;
                })
                .ToArray();
            _traffic = new TrafficFlow();
            _traffic.Initialize(_tiles);
            Playback = Timeline.Create(
                Timeline.While(
                    () => true,
                    Timeline.Sequence(
                        Timeline.Wait(0.18f),
                        Timeline.Call(StartDispatch),
                        Timeline.WaitUntil(() => DispatchFinished.Value)
                    )
                )
            );
        }

        private void StartDispatch()
        {
            DispatchFinished.Target = false;
            Dispatch = Timeline.Create(
                Timeline.Sequence(
                    _traffic.Build(),
                    Timeline.Call(() => DispatchFinished.Target = true)
                )
            );
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            if (_tiles == null)
                return [];
            return
            [
                new View
                {
                    Width = 100f.Percent(),
                    MaxWidth = 1040f.Point(),
                    Height = 100f.Percent(),
                    MaxHeight = 620f.Point(),
                    Children = BuildTiles(),
                },
            ];
        }

        private WidgetList BuildTiles() =>
            new(
                Enumerable
                    .Range(1, TrafficLayouts.Count - 1)
                    .Concat(new[] { 0 })
                    .Select(index => new TrafficTile
                    {
                        Key = index,
                        Skin = Skin,
                        Caption = Metrics[index].Caption,
                        Value = Metrics[index].Value,
                        Hero = index == 0,
                        Activity = _tiles[index].Activity,
                        PositionType = PositionTypes.Absolute,
                        PositionLeft = _tiles[index].Left,
                        PositionTop = _tiles[index].Top,
                        Width = _tiles[index].Width,
                        Height = _tiles[index].Height,
                    })
            );

        public override void OnUnmount()
        {
            if (_tiles != null)
                foreach (var tile in _tiles)
                    tile.ReleaseFvp();
            Skin.Value?.Dispose();
        }
    }
}
