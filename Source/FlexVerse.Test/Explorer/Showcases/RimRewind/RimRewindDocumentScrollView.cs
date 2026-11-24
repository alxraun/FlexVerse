// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>Continuous settings document, section anchors and scroll-driven navigation selection.</summary>
    public partial class RimRewindDocumentScrollView : RimRewindScrollView
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [State]
        private Behavior<int> LayoutRevision { get; set; } = 0;
        private RimRewind _screen;
        private List<RimRewindCategorySection> _sections;
        private Vector2 _viewportSize;

        public override void OnMount()
        {
            base.OnMount();
            _screen = Session.Value;
            _sections = new List<RimRewindCategorySection>(RimRewindOptions.Categories.Length);
            _screen.AttachDocument(this);
            Watch.LayoutRevision.OnUpdate += _ => HandleLayout();
            Watch.ScrollPosition.OnUpdate += _ => HandleScroll();
            Watch.ScrollPosition.OnComplete += HandleSettled;
        }

        public override void OnUnmount()
        {
            _screen.DetachDocument(this);
            base.OnUnmount();
        }

        public override IEnumerable<Widget> BuildRenderTree() => [new RimRewindSettingsContent()];

        public override ChildrenGuiSpace BeginChildrenGUI(Rect rect, Rect viewport)
        {
            var context = base.BeginChildrenGUI(rect, viewport);
            if (Engine.DidLayoutCalculationHappenThisCycle || _viewportSize != viewport.size)
            {
                _viewportSize = viewport.size;
                LayoutRevision.Target = LayoutRevision.Value + 1;
            }
            return context;
        }

        private void HandleLayout()
        {
            _sections.Clear();
            CollectSections(this);
            if (_screen.NavigationTarget.Value >= 0)
            {
                if (!NavigateTo(_screen.NavigationTarget.Value))
                    _screen.CancelNavigation();
            }
            HandleScroll();
        }

        private void CollectSections(Widget widget)
        {
            var children = Engine.GetChildren(widget);
            for (var i = 0; i < children.Count; i++)
                if (children[i] is RimRewindCategorySection section)
                    _sections.Add(section);
                else
                    CollectSections(children[i]);
        }

        private float Offset(RimRewindCategorySection section)
        {
            var layout = Engine.GetLayoutResults();
            return
                layout.TryGetValue(section, out var bounds)
                && layout.TryGetValue(this, out var viewport)
                ? Mathf.Max(0f, bounds.y - viewport.y)
                : 0f;
        }

        public bool NavigateTo(int category)
        {
            for (var i = 0; i < _sections.Count; i++)
                if (_sections[i].Index.Value == category)
                {
                    ScrollTo(new Vector2(0f, Offset(_sections[i])));
                    if (!ScrollPosition.IsUpdating)
                    {
                        _screen.CancelNavigation();
                        HandleScroll();
                    }
                    return true;
                }
            return false;
        }

        private void HandleSettled()
        {
            _screen.CancelNavigation();
            HandleScroll();
        }

        private void HandleScroll()
        {
            if (_screen.NavigationTarget.Value >= 0 || _sections.Count == 0)
                return;
            var active = _sections[0].Index.Value;
            var position = ScrollPosition.Value.y + 24f;
            for (var i = 0; i < _sections.Count; i++)
                if (Offset(_sections[i]) <= position)
                    active = _sections[i].Index.Value;
            _screen.ReportSection(active);
        }
    }
}
