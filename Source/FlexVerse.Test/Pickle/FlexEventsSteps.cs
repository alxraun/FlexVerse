// parents: @.frostline/graph/m-pickle.xml

using System.Collections.Generic;
using FlexVerse;
using RimWorks.Pickle;
using UnityEngine;

namespace FlexVerse.Test.Pickle
{
    [PickleSteps]
    public static class FlexEventSteps
    {
        private static WidgetTree _tree;

        [BeforeScenario]
        public static void ResetProbe(PickleContext ctx)
        {
            EventProbe.Clear();
            _tree = null;
            ScenarioMarkers.Begin("M-FLEX-EVENTS");
        }

        [AfterScenario]
        public static void Teardown(PickleContext ctx)
        {
            ScenarioMarkers.End("M-FLEX-EVENTS");
        }

        private static WidgetTree BuildProbeTree(
            Widget rootDef,
            Widget childDef,
            Widget grandchildDef
        )
        {
            var tree = new WidgetTree(new WidgetEngine());
            var root = tree.CreateInstance(rootDef, null);
            var child = tree.CreateInstance(childDef, root);
            tree.CreateInstance(grandchildDef, child);
            return tree;
        }

        private static void DispatchMouseDown(WidgetTree tree, Widget target)
        {
            var flexEvent = new FlexEvent(new Event { type = EventType.MouseDown }, target);
            foreach (var widget in tree.TraverseAncestors(target))
            {
                flexEvent.CurrentTarget = widget;
                widget.OnMouseDown(flexEvent);
                if (flexEvent.IsPropagationStopped)
                {
                    break;
                }
            }
        }

        private static void EnterPath(WidgetTree tree, Widget target)
        {
            var flexEvent = new FlexEvent(new Event { type = EventType.MouseMove }, target);
            foreach (var widget in tree.GetAncestorPath(target))
            {
                flexEvent.CurrentTarget = widget;
                widget.OnMouseEnter(flexEvent);
            }
        }

        private static void LeavePath(WidgetTree tree, Widget target)
        {
            var flexEvent = new FlexEvent(new Event { type = EventType.MouseMove }, target);
            var path = tree.GetAncestorPath(target);
            for (var i = path.Count - 1; i >= 0; i--)
            {
                flexEvent.CurrentTarget = path[i];
                path[i].OnMouseLeave(flexEvent);
            }
        }

        [Given("a bubbling probe tree parent-child-grandchild")]
        public static void GivenBubblingTree(PickleContext ctx)
        {
            _tree = BuildProbeTree(new ProbeParent(), new ProbeChild(), new ProbeGrandchild());
        }

        [Given("a stop-propagation probe tree parent-child-grandchild")]
        public static void GivenStopTree(PickleContext ctx)
        {
            _tree = BuildProbeTree(new StopParent(), new StopChild(), new StopGrandchild());
        }

        [Given("a hover probe widget")]
        public static void GivenHoverProbe(PickleContext ctx)
        {
            _tree = new WidgetTree(new WidgetEngine());
            _tree.CreateInstance(new HoverProbe(), null);
        }

        [When("a mouse down is dispatched at the grandchild")]
        public static void WhenMouseDownAtGrandchild(PickleContext ctx)
        {
            var grandchild = _tree.GetChildInstances(_tree.GetRootInstance())[0];
            var target = _tree.GetChildInstances(grandchild)[0];
            DispatchMouseDown(_tree, target);
        }

        [When("the pointer enters the hover probe")]
        public static void WhenPointerEnters(PickleContext ctx)
        {
            EnterPath(_tree, _tree.GetRootInstance());
        }

        [When("the pointer leaves the hover probe")]
        public static void WhenPointerLeaves(PickleContext ctx)
        {
            LeavePath(_tree, _tree.GetRootInstance());
        }

        [Then("the probe recorded fact {string}")]
        public static void FactRecorded(PickleContext ctx, string fact)
        {
            ctx.Require(EventProbe.Seen(fact), $"expected probe fact '{fact}' to be recorded");
        }

        [Then("the probe did not record fact {string}")]
        public static void FactNotRecorded(PickleContext ctx, string fact)
        {
            ctx.Require(!EventProbe.Seen(fact), $"expected probe fact '{fact}' NOT to be recorded");
        }
    }
}
