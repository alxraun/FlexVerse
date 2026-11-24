// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ScreenAttribute(string path) : Attribute
    {
        public string Path { get; } = path;
        public int Order { get; set; } = 0;
    }

    public class ExplorerNode(string label, string fullPath, ExplorerNode parent)
    {
        public string Label { get; } = label;
        public string FullPath { get; } = fullPath;
        public int Order { get; set; }
        public List<ExplorerNode> Children { get; } = [];
        public Type ScreenWidgetType { get; set; }
        public ExplorerNode Parent { get; } = parent;
    }

    public static class ExplorerRegistry
    {
        private static ExplorerNode _rootNode;

        public static ExplorerNode RootNode => _rootNode ??= BuildAndSortTree();

        private static ExplorerNode BuildAndSortTree()
        {
            var root = BuildTree();
            SortTree(root);
            return root;
        }

        private static void SortTree(ExplorerNode node)
        {
            node.Children.Sort(
                (a, b) =>
                {
                    int orderComparison = a.Order.CompareTo(b.Order);
                    if (orderComparison != 0)
                        return orderComparison;
                    return string.Compare(a.Label, b.Label, StringComparison.Ordinal);
                }
            );

            foreach (var child in node.Children)
            {
                SortTree(child);
            }
        }

        private static ExplorerNode BuildTree()
        {
            var root = new ExplorerNode("/", "/", null);

            foreach (var (type, attribute) in DiscoverScreenTypes())
            {
                var parts = ParsePath(attribute.Path);
                if (parts.Length == 0)
                {
                    FlexVerse.Log.Warning(
                        "M-EXPLORER",
                        "explorer.path.invalid",
                        ("type", type.FullName)
                    );
                    continue;
                }

                var node = EnsureNodeForPath(root, parts);
                AttachScreen(node, type, attribute);
            }

            return root;
        }

        private static IEnumerable<(Type type, ScreenAttribute attribute)> DiscoverScreenTypes()
        {
            var allAssemblies = LoadedModManager
                .RunningMods.SelectMany(mod => mod.assemblies.loadedAssemblies)
                .Append(Assembly.GetExecutingAssembly())
                .Distinct();

            return allAssemblies
                .SelectMany(assembly =>
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        types = e.Types.Where(t => t != null).ToArray();
                    }
                    return types;
                })
                .Where(type => IsScreenWidgetType(type))
                .Select(type => (type, attribute: type.GetCustomAttribute<ScreenAttribute>()))
                .Where(t => t.attribute != null)!;
        }

        private static bool IsScreenWidgetType(Type type)
        {
            if (!type.IsClass)
                return false;
            if (type.IsAbstract)
                return false;
            return typeof(Widget).IsAssignableFrom(type);
        }

        private static string[] ParsePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return [];
            return path.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        }

        private static ExplorerNode EnsureNodeForPath(
            ExplorerNode root,
            IReadOnlyList<string> parts
        )
        {
            var current = root;
            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                var next = current.Children.FirstOrDefault(c => c.Label == part);
                if (next == null)
                {
                    var newPath =
                        current.FullPath == "/" ? $"/{part}" : $"{current.FullPath}/{part}";
                    next = new ExplorerNode(part, newPath, current);
                    current.Children.Add(next);
                }
                current = next;
            }
            return current;
        }

        private static void AttachScreen(ExplorerNode node, Type type, ScreenAttribute attribute)
        {
            node.ScreenWidgetType = type;
            node.Order = attribute.Order;
        }
    }

    public partial class Explorer : Window
    {
        private const string UiContextKey = "explorer";

        private static Explorer _instance;
        private readonly ExplorerNode _rootNode;
        private readonly DebugData _persistentData;
        private Widget _activeScreenInstance;
        private ExplorerNode _mountedScreenNode;
        private Vector2 _scrollPosition = Vector2.zero;
        private readonly Stack<ExplorerNode> _navigationStack;
        private readonly List<ExplorerNode> _flatScreenList;
        private bool _isFullscreen;

        private const float HeaderHeight = 35f;
        private const float HeaderButtonWidth = 80f;
        private const float HeaderPadding = 5f;
        private static readonly Color HeaderBgColor = new(0.15f, 0.15f, 0.15f);
        private static readonly Color ContentAreaBorderColor = new(0.15f, 0.15f, 0.15f);

        public override Vector2 InitialSize => new(1200, 800);
        protected override float Margin => _isFullscreen ? 0f : base.Margin;

        private ExplorerNode CurrentNode => _navigationStack.Peek();
        private ExplorerNode CurrentScreenNode =>
            CurrentNode.ScreenWidgetType != null ? CurrentNode : null;
        private bool IsViewingScreen => CurrentScreenNode != null;
        private bool CanGoBack => _navigationStack.Count > 1;
        private string CurrentBreadcrumbsPath => BuildBreadcrumbsPath();

        public Explorer()
        {
            doCloseX = true;

            closeOnAccept = false;
            closeOnCancel = false;
            draggable = true;
            resizeable = true;
            absorbInputAroundWindow = false;
            optionalTitle = "FlexVerse Explorer";

            _rootNode = ExplorerRegistry.RootNode;
            _navigationStack = new Stack<ExplorerNode>();
            _navigationStack.Push(_rootNode);
            _flatScreenList = [];
            BuildFlatScreenList(_rootNode);

            _persistentData = ExplorerMod.Settings.GetDataSection<DebugData>();
            var lastPath = _persistentData.lastExplorerScreenPath;
            var targetNode = FindNodeByPath(lastPath);
            if (targetNode != null)
            {
                GoToNode(targetNode);
            }
            else
            {
                ActivateScreen(CurrentScreenNode);
            }
        }

        public static void ToggleWindow()
        {
            if (_instance != null)
            {
                _instance.Close();
            }
            else
            {
                _instance = new Explorer();
                Find.WindowStack.Add(_instance);
            }
        }

        internal static void HandleFullscreenInput()
        {
            if (_instance != null && _instance.IsOpen)
                _instance.HandleFullscreenKey();
        }

        private void HandleFullscreenKey()
        {
            if (
                Event.current.type == EventType.KeyDown
                && Event.current.keyCode == KeyCode.F11
                && Find.WindowStack.GetsInput(this)
            )
            {
                _isFullscreen = !_isFullscreen;
                doCloseX = !_isFullscreen;
                draggable = !_isFullscreen;
                resizeable = !_isFullscreen;
                optionalTitle = _isFullscreen ? null : "FlexVerse Explorer";

                Event.current.Use();
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (_isFullscreen)
                Widgets.DrawBoxSolid(inRect, ContentAreaBorderColor);

            var contentRect = _isFullscreen ? inRect : DrawHeaderAndFrame(inRect);
            if (IsViewingScreen)
            {
                DoScreenView(contentRect);
            }
            else
            {
                DoExplorerView(contentRect);
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            FVUI.Release(UiContextKey);
            _instance = null;
        }

        private void BuildFlatScreenList(ExplorerNode node)
        {
            if (node.ScreenWidgetType != null)
            {
                _flatScreenList.Add(node);
            }

            foreach (var child in node.Children.OrderBy(c => c.Order).ThenBy(c => c.Label))
            {
                BuildFlatScreenList(child);
            }
        }

        private void RebuildNavigationStack(ExplorerNode targetNode)
        {
            _navigationStack.Clear();
            var chain = new List<ExplorerNode>();
            var current = targetNode;
            while (current != null)
            {
                chain.Add(current);
                current = current.Parent;
            }
            chain.Reverse();
            foreach (var node in chain)
            {
                _navigationStack.Push(node);
            }
        }

        private Rect DrawHeaderAndFrame(Rect inRect)
        {
            var headerRect = new Rect(inRect.x, inRect.y, inRect.width, HeaderHeight);
            DrawHeaderBar(headerRect);

            var contentOuterRect = new Rect(
                inRect.x,
                headerRect.yMax + HeaderPadding,
                inRect.width,
                inRect.height - headerRect.height - HeaderPadding
            );

            Widgets.DrawBoxSolid(contentOuterRect, ContentAreaBorderColor);
            var contentInnerRect = contentOuterRect.ContractedBy(1f);
            return contentInnerRect;
        }

        private void DrawHeaderBar(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, HeaderBgColor);

            var savedAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            float currentX = rect.x + HeaderPadding;
            float buttonHeight = rect.height - (HeaderPadding * 2);
            float buttonY = rect.y + HeaderPadding;

            var backButtonRect = new Rect(currentX, buttonY, HeaderButtonWidth, buttonHeight);
            if (Widgets.ButtonText(backButtonRect, "<- Back", active: CanGoBack))
            {
                GoBack();
            }
            currentX += HeaderButtonWidth + HeaderPadding;

            if (IsViewingScreen)
            {
                var prevButtonRect = new Rect(currentX, buttonY, HeaderButtonWidth, buttonHeight);
                if (Widgets.ButtonText(prevButtonRect, "<< Prev"))
                {
                    SwitchScreen(-1);
                }
                currentX += HeaderButtonWidth + HeaderPadding / 2;

                var nextButtonRect = new Rect(currentX, buttonY, HeaderButtonWidth, buttonHeight);
                if (Widgets.ButtonText(nextButtonRect, "Next >>"))
                {
                    SwitchScreen(1);
                }
                currentX += HeaderButtonWidth + HeaderPadding * 2;
            }

            var breadcrumbsRect = new Rect(
                currentX,
                rect.y,
                rect.width - currentX - HeaderPadding,
                rect.height
            );

            Widgets.Label(breadcrumbsRect, CurrentBreadcrumbsPath);

            Text.Anchor = savedAnchor;
        }

        private string BuildBreadcrumbsPath()
        {
            var node = CurrentScreenNode ?? CurrentNode;
            var fullPath = node.FullPath;
            if (fullPath == "/")
                return "/";
            var trimmedPath = fullPath.TrimStart('/');
            return "/ " + trimmedPath.Replace("/", " / ");
        }

        private void DoScreenView(Rect contentRect)
        {
            if (_activeScreenInstance != null)
            {
                FVUI.Render(UiContextKey, contentRect, _activeScreenInstance);
            }
        }

        private void DoExplorerView(Rect contentRect)
        {
            var listing = new Listing_Standard();
            var children = CurrentNode.Children;

            float viewHeight = children.Count * 32f;
            var scrollContentRect = new Rect(0, 0, contentRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(contentRect, ref _scrollPosition, scrollContentRect);
            listing.Begin(scrollContentRect);

            foreach (var child in children)
            {
                string label = child.ScreenWidgetType == null ? "> " + child.Label : child.Label;
                if (listing.ButtonText(label))
                {
                    GoToNode(child);
                }
            }

            listing.End();
            Widgets.EndScrollView();
        }

        private void GoToNode(ExplorerNode node)
        {
            if (node.Parent == CurrentNode)
            {
                _navigationStack.Push(node);
            }
            else
            {
                RebuildNavigationStack(node);
            }
            ActivateScreen(CurrentScreenNode);
        }

        private void GoBack()
        {
            if (!CanGoBack)
                return;
            _navigationStack.Pop();
            ActivateScreen(CurrentScreenNode);
        }

        private void SwitchScreen(int direction)
        {
            if (CurrentScreenNode == null || !_flatScreenList.Any())
                return;

            int currentIndex = _flatScreenList.IndexOf(CurrentScreenNode);
            if (currentIndex < 0)
                return;

            int nextIndex =
                (currentIndex + direction + _flatScreenList.Count) % _flatScreenList.Count;
            var nextNode = _flatScreenList[nextIndex];

            GoToNode(nextNode);
        }

        private void ActivateScreen(ExplorerNode node)
        {
            _activeScreenInstance = null;
            _mountedScreenNode = null;
            FVUI.Render(UiContextKey, Rect.zero, null);

            if (node == null || node.ScreenWidgetType == null)
            {
                _persistentData.lastExplorerScreenPath = null;
                ExplorerMod.Settings.Write();
                return;
            }

            _activeScreenInstance = CreateScreenInstance(node);
            _mountedScreenNode = node;
            _persistentData.lastExplorerScreenPath = node.FullPath;
            ExplorerMod.Settings.Write();
        }

        private Widget CreateScreenInstance(ExplorerNode node)
        {
            try
            {
                return (Widget)Activator.CreateInstance(node.ScreenWidgetType!);
            }
            catch (Exception e)
            {
                FlexVerse.Log.Error(
                    "M-EXPLORER",
                    "explorer.screen.create_failed",
                    ("path", node.FullPath),
                    ("error", e)
                );
                return CreateErrorScreenInstance(e, node);
            }
        }

        private Widget CreateErrorScreenInstance(Exception e, ExplorerNode node)
        {
            return new View
            {
                BackgroundColor = Color.red,
                Children = new WidgetList
                {
                    new Label
                    {
                        Content = "Error creating entry '" + node.FullPath + "':\n" + e.Message,
                    },
                },
            };
        }

        private ExplorerNode FindNodeByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return _flatScreenList.FirstOrDefault(node => node.FullPath == path);
        }
    }
}
