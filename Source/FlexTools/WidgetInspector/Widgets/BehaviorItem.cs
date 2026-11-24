// parents: @.frostline/graph/m-dbg-ui.xml

using System.Collections.Generic;
using FlexTools.WidgetInspector.Models;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Shows a behavior snapshot's type and parameters.</summary>
    public partial class BehaviorItem : View
    {
        /// <summary>Gets or sets the behavior snapshot to display.</summary>
        [Prop]
        public Behavior<DebugBehaviorNode> Node { get; set; }

        /// <summary>Initializes the item styling.</summary>
        public BehaviorItem() => BackgroundColor = new Color(0f, 0f, 0f, 0.25f);

        /// <summary>Builds the type label and parameter labels.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            var node = Node.Value;
            if (node == null)
                yield break;
            yield return new Label
            {
                Key = "Type",
                Content = $"{node.TypeName}  #{node.LiveBehaviorId:X}",
                Font = GameFont.Tiny,
                Color = new Color(0.45f, 0.7f, 1f),
                TextAlign = TextAnchor.MiddleLeft,
            };
            foreach (var parameter in node.Parameters)
                yield return new Label
                {
                    Key = parameter.Key,
                    Content = $"{parameter.Key}: {parameter.Value}",
                    Font = GameFont.Tiny,
                    TextAlign = TextAnchor.MiddleLeft,
                };
        }
    }
}
