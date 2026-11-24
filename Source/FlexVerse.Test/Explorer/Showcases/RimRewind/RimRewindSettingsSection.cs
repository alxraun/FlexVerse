// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSettingsSection : View
    {
        [Prop]
        public Behavior<string> Title { get; set; } = "";

        [Prop]
        public Behavior<RimRewindOption[]> Options { get; set; } = Array.Empty<RimRewindOption>();

        [Prop]
        public Behavior<int> FirstIndex { get; set; } = 0;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = Title,
                    Font = GameFont.Tiny,
                    Color = RimRewindSkin.Muted,
                    TextAlign = TextAnchor.MiddleLeft,
                    Height = 24f.Point(),
                    MarginTop = 8f.Point(),
                    MarginBottom = 8f.Point(),
                    TextWrap = false,
                },
                .. Options.Value.Select(
                    (option, index) =>
                        new RimRewindSettingRow
                        {
                            Key = option.Id,
                            Option = option,
                            EntranceIndex = FirstIndex.Value + index,
                        }
                ),
            ];
    }
}
