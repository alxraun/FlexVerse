// parents: @.frostline/graph/m-pickle.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Pickle
{
    public static class EventProbe
    {
        private static readonly List<string> Facts = new();

        public static void Record(string fact)
        {
            Facts.Add(fact);

            Log.Info("M-FLEX-EVENTS", $"probe:{fact}");
        }

        public static bool Seen(string fact) => Facts.Contains(fact);

        public static void Clear() => Facts.Clear();
    }

    public partial class ProbeParent : Widget
    {
        public override void OnMouseDown(FlexEvent e) => EventProbe.Record("click:parent");
    }

    public partial class ProbeChild : Widget
    {
        public override void OnMouseDown(FlexEvent e) => EventProbe.Record("click:child");
    }

    public partial class ProbeGrandchild : Widget
    {
        public override void OnMouseDown(FlexEvent e) => EventProbe.Record("click:grandchild");
    }

    public partial class StopParent : Widget
    {
        public override void OnMouseDown(FlexEvent e)
        {
            EventProbe.Record("stop:parent");
            e.StopPropagation();
        }
    }

    public partial class StopChild : Widget
    {
        public override void OnMouseDown(FlexEvent e)
        {
            EventProbe.Record("stop:child");
            e.StopPropagation();
        }
    }

    public partial class StopGrandchild : Widget
    {
        public override void OnMouseDown(FlexEvent e)
        {
            EventProbe.Record("stop:grandchild");
            e.StopPropagation();
        }
    }

    public partial class HoverProbe : Widget
    {
        public override void OnMouseEnter(FlexEvent e) => EventProbe.Record("hover:enter");

        public override void OnMouseLeave(FlexEvent e) => EventProbe.Record("hover:leave");
    }
}
