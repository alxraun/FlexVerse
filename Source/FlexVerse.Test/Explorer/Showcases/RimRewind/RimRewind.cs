// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    [Screen("Showcases/RimRewind")]
    public partial class RimRewind : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [State]
        public Behavior<RimRewindWorkspace> Workspace { get; private set; }

        [State]
        public Behavior<RimRewindProfile> Draft { get; private set; }

        [State]
        public Behavior<RimRewindProfile> Applied { get; private set; }

        [State]
        public Behavior<RimRewindOption[]> VisibleOptions { get; private set; }

        [State]
        public Behavior<int> Category { get; private set; } = 0;

        [State]
        public Behavior<string> Query { get; private set; } = "";

        [State]
        public Behavior<bool> Advanced { get; private set; } = false;

        [State]
        public Behavior<bool> ChangedOnly { get; private set; } = false;

        [State]
        public Behavior<float> PreviewDepth { get; private set; } = 30f;

        [State]
        public Behavior<string> Status { get; private set; } = "";

        [State]
        public Behavior<float> BrandPulse { get; private set; } = Spring.State(0f, 80f, 16f);

        [State]
        public Behavior<RimRewindPopup> Popup { get; private set; } =
            Static.Value<RimRewindPopup>(null);

        [State]
        public Behavior<int> MenuFocus { get; private set; } = 0;

        [State]
        private Behavior Notice { get; set; }

        [State]
        public Behavior<Widget> FocusOwner { get; private set; } = Static.Value<Widget>(null);

        [State]
        public Behavior<int> NavigationTarget { get; private set; } = -1;
        private RimRewindDocumentScrollView _document;

        [State]
        private Behavior<Vector2> Viewport { get; set; } = new Vector2(1200f, 800f);

        [State]
        public Behavior<bool> Compact { get; private set; }
        private int _popupIdentity;

        public RimRewind()
        {
            Width = 100f.Percent();
            Height = 100f.Percent();
            FlexGrow = 1f;
            BackgroundColor = RimRewindSkin.Background;
        }

        public override void OnMount()
        {
            Session = this;
            Compact = Viewport.Select(size => size.y < 740f);
            Workspace = new RimRewindWorkspace();
            Draft = Workspace.Select(workspace => workspace.Draft);
            Applied = Workspace.Select(workspace => workspace.Applied);
            VisibleOptions = Group.Combine(
                SelectVisibleOptions,
                Workspace,
                Query,
                Advanced,
                ChangedOnly
            );
            RimRewindSkin.LoadMaterials();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Width = 100f.Percent(),
                    Height = 100f.Percent(),
                    BackgroundColor = RimRewindPalette.Surface,
                    Children = new WidgetList
                    {
                        new RimRewindWindow
                        {
                            PaddingAll = Compact.Select(compact => (compact ? 16f : 24f).Point()),
                        },
                    },
                },
                new RimRewindMenuLayer
                {
                    Display = Popup.Select(popup => popup == null ? Displays.None : Displays.Flex),
                },
            ];

        public override void OnGUI(Rect rect)
        {
            if (Viewport.Value != rect.size)
                Viewport.Target = rect.size;
            base.OnGUI(rect);
        }

        public void HandleFocus(Widget widget)
        {
            FocusOwner.Target = widget;
            for (
                var parent = Engine.GetParent(widget);
                parent != null;
                parent = Engine.GetParent(parent)
            )
                if (parent is RimRewindScrollView scroll)
                    scroll.Reveal(widget);
        }

        public void HandleBlur(Widget widget)
        {
            if (ReferenceEquals(FocusOwner.Value, widget))
                FocusOwner.Target = null;
        }

        public void AttachDocument(RimRewindDocumentScrollView document) => _document = document;

        public void DetachDocument(RimRewindDocumentScrollView document)
        {
            if (ReferenceEquals(_document, document))
                _document = null;
        }

        public void CancelNavigation() => NavigationTarget.Target = -1;

        public void ReportSection(int category)
        {
            if (NavigationTarget.Value < 0 && Category.Value != category)
                Category.Target = category;
        }

        public void HandleEdit(RimRewindSetting id, float value)
        {
            HandleCloseMenu();
            Workspace.Target = Workspace.Value.Edit(id, value);
            Status.Target = "";
        }

        public void HandleOptionEdit(RimRewindOption option, float value)
        {
            if (option.Unavailable?.Invoke(Draft.Value) == null)
                HandleEdit(option.Id, option.Normalize(value));
        }

        public void HandleCategory(int category)
        {
            HandleCloseMenu();
            var filtered = !string.IsNullOrWhiteSpace(Query.Value) || ChangedOnly.Value;
            Category.Target = category;
            Query.Target = "";
            ChangedOnly.Target = false;
            NavigationTarget.Target = category;
            if (!filtered)
                _document?.NavigateTo(category);
        }

        public void HandleScope(int scope)
        {
            HandleCloseMenu();
            Workspace.Target = Workspace.Value.SwitchScope(scope);
            Status.Target = "";
        }

        public void HandleSearch(string query)
        {
            HandleCloseMenu();
            CancelNavigation();
            Query.Target = query;
        }

        public void HandleClearFilters()
        {
            Query.Target = "";
            ChangedOnly.Target = false;
        }

        public void HandleResetCategory()
        {
            HandleCloseMenu();
            Workspace.Target = Workspace.Value.ResetCategory(Category.Value);
            Status.Target = "";
        }

        public void HandleRevert()
        {
            HandleCloseMenu();
            Workspace.Target = Workspace.Value.Revert();
            Status.Target = "";
        }

        public void HandleApply()
        {
            HandleCloseMenu();
            Workspace.Target = Workspace.Value.Apply();
            Status.Target =
                Workspace.Value.Scope == 0 ? "Applied for this colony" : "Applied for new colonies";
            Notice = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Call(() => BrandPulse.Target = 1f),
                    Timeline.Wait(.16f),
                    Timeline.Call(() => BrandPulse.Target = 0f),
                    Timeline.Wait(3f),
                    Timeline.Call(() => Status.Target = "")
                )
            );
        }

        public void HandleOpenMenu(
            RimRewindChoice anchor,
            string[] options,
            int selected,
            Action<int> choose
        )
        {
            if (Popup.Value?.Anchor == anchor)
            {
                HandleCloseMenu();
                return;
            }
            for (
                var parent = Engine.GetParent(anchor);
                parent != null;
                parent = Engine.GetParent(parent)
            )
                if (parent is RimRewindScrollView scroll)
                    scroll.StopScrolling();
            GUI.FocusControl(null);
            GUIUtility.keyboardControl = 0;
            MenuFocus.Target = Mathf.Clamp(selected, 0, options.Length - 1);
            Popup.Target = new RimRewindPopup(
                ++_popupIdentity,
                anchor,
                options,
                selected,
                choose,
                GetAnchorBounds(anchor),
                GetHostBounds()
            );
        }

        public void HandleCloseMenu()
        {
            var popup = Popup.Value;
            Popup.Target = null;
            if (popup != null && IsMounted(popup.Anchor))
                popup.Anchor.RestoreNativeFocus();
        }

        public void HandleMenuLayout()
        {
            var popup = Popup.Value;
            if (popup == null)
                return;
            if (!IsMounted(popup.Anchor))
            {
                HandleCloseMenu();
                return;
            }
            Popup.Target = new RimRewindPopup(
                popup.Identity,
                popup.Anchor,
                popup.Options,
                popup.Selected,
                popup.Choose,
                GetAnchorBounds(popup.Anchor),
                GetHostBounds()
            );
        }

        private RimRewindOption[] SelectVisibleOptions()
        {
            var search = !string.IsNullOrWhiteSpace(Query.Value);
            var workspace = Workspace.Value;
            return RimRewindOptions
                .All.Where(option =>
                    (search || Advanced.Value || !option.Advanced)
                    && (option.Visible?.Invoke(workspace.Draft) ?? true)
                    && (!ChangedOnly.Value || workspace.Draft.Differs(workspace.Applied, option.Id))
                    && RimRewindOptions.Matches(option, Query.Value)
                )
                .ToArray();
        }

        public void HandleMenuFocus(int index) => MenuFocus.Target = index;

        public void HandleMenuChoose(int index)
        {
            var popup = Popup.Value;
            if (popup == null || index < 0 || index >= popup.Options.Length)
                return;
            HandleCloseMenu();
            popup.Choose(index);
        }

        public Rect GetAnchorBounds(Widget anchor) => Engine.TransformService.ToScreen(anchor);

        public Rect GetHostBounds() => Engine.TransformService.ToScreen(this);

        public bool IsMounted(Widget widget) => Engine.GetNode(widget) != null;
    }
}
