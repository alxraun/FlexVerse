// parents: @.frostline/graph/m-widget-engine.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Queues a deferred action bound to a mounted source widget. (REQ-007)</summary>
    public record SignalInfo(Widget Source, Action Action)
    {
        /// <summary>Returns a short debug description of the signal.</summary>
        public override string ToString()
        {
            var method = Action.Method;
            var target = Action.Target;
            var targetName = target?.GetType().Name ?? "Static";
            return $"SignalInfo {{ Source = {Source}, Action = {targetName}.{method.Name} }}";
        }
    }

    /// <summary>Orchestrates reconciliation, layout, geometry and rendering for one UI context. (M-WIDGET-ENGINE)</summary>
    public class WidgetEngine
    {
        private readonly WidgetTree _widgetTree;
        private readonly WidgetGeometryBuilder _geometryBuilder;
        private readonly WidgetReconciler _reconciler;
        private readonly WidgetLayoutCalculator _layoutCalculator;
        private readonly WidgetRenderer _renderer;
        private readonly WidgetSignalQueue _signalQueue;
        private Widget _lastRootDefinition;
        private bool _isLayoutDirty;
        private Rect _lastAvailableRect;
        private readonly FlexEventDispatcher _eventDispatcher;
        private readonly FocusManager _focusManager;
        private bool _isShutdown;

        /// <summary>Reports whether layout ran during the current ProcessUI call. (REQ-002)</summary>
        public bool DidLayoutCalculationHappenThisCycle { get; private set; }

        /// <summary>Immutable unique identifier of this engine context (M-WIDGET-ENGINE.REQ-003).</summary>
        public string Key { get; }

        /// <summary>Exposes the inherited context manager of this engine.</summary>
        public InheritedContextManager InheritedContextManager { get; }

        /// <summary>Exposes the published geometry snapshot of this engine.</summary>
        public WidgetGeometrySnapshot Geometry { get; }

        /// <summary>Exposes the coordinate conversion service of this engine.</summary>
        public TransformService TransformService { get; }

        /// <summary>Exposes the clipping query service of this engine.</summary>
        public ClipService ClipService { get; }

        /// <summary>Exposes the pointer hit tester of this engine.</summary>
        public WidgetHitTester HitTester { get; }
        internal FocusManager FocusManager => _focusManager;

        /// <summary>Creates an engine with an auto-generated GUID key. (REQ-003)</summary>
        public WidgetEngine()
            : this("auto:" + Guid.NewGuid().ToString("N")) { }

        /// <summary>Creates an engine identified by the given key. (REQ-003)</summary>
        public WidgetEngine(string key)
        {
            if (key == null)
                throw new ArgumentException("Engine key must be a non-null string.", nameof(key));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(
                    "Engine key must not be empty or whitespace.",
                    nameof(key)
                );
            Key = key;

            _widgetTree = new WidgetTree(this);
            _reconciler = new WidgetReconciler(_widgetTree, ReconcileChildInstances);
            _layoutCalculator = new WidgetLayoutCalculator(_widgetTree);
            Geometry = new WidgetGeometrySnapshot();
            _geometryBuilder = new WidgetGeometryBuilder(
                _widgetTree,
                _layoutCalculator.Results,
                _layoutCalculator.PaddingBoxes,
                Geometry
            );
            TransformService = new TransformService(Geometry);
            ClipService = new ClipService(Geometry, TransformService);
            HitTester = new WidgetHitTester(Geometry, TransformService, ClipService);
            _focusManager = new FocusManager(this);
            _renderer = new WidgetRenderer(Geometry, RefreshGeometry, _focusManager);
            _signalQueue = new WidgetSignalQueue();
            InheritedContextManager = new InheritedContextManager(_widgetTree);
            _isLayoutDirty = true;
            _eventDispatcher = new FlexEventDispatcher(_widgetTree, HitTester, _focusManager);
        }

        /// <summary>Runs one full UI cycle from input dispatch through rendering. (REQ-001)</summary>
        public void ProcessUI(Widget rootDefinition, Rect availableRect)
        {
            _focusManager.BeginFrame();
            DidLayoutCalculationHappenThisCycle = false;

            try
            {
                _eventDispatcher.ProcessEvents(Event.current);

                ReconcileRoot(rootDefinition);
                UpdateAvailableRect(availableRect);

                var currentRoot = _widgetTree.GetRootInstance();
                if (currentRoot == null)
                {
                    _layoutCalculator.Clear();
                    RefreshGeometry();
                    return;
                }

                ConsumeSignals();
                CalculateLayoutIfDirty(availableRect);
                RefreshGeometry();

                currentRoot = _widgetTree.GetRootInstance();
                if (currentRoot != null)
                    _renderer.RenderInstanceTree();
            }
            finally
            {
                _focusManager.EndFrame();
            }
        }

        /// <summary>Reconciles the children of a widget immediately.</summary>
        public void ReconcileChildren(Widget widget)
        {
            ReconcileChildInstances(widget);
        }

        /// <summary>Queues a deferred signal for the next cycle. (REQ-007)</summary>
        public void Post(SignalInfo signal)
        {
            _signalQueue.Post(signal);
        }

        /// <summary>Marks the Flexbox node of a widget as dirty.</summary>
        public void MarkNodeAsDirty(Widget widget)
        {
            _widgetTree.GetNode(widget)?.MarkAsDirty();
        }

        /// <summary>Returns the parent instance of a widget.</summary>
        public Widget GetParent(Widget instance)
        {
            return _widgetTree.GetParentInstance(instance);
        }

        /// <summary>Returns the child instances of a widget.</summary>
        public WidgetList GetChildren(Widget instance)
        {
            return _widgetTree.GetChildInstances(instance);
        }

        /// <summary>Returns the instance registered under a reconciliation key.</summary>
        public Widget GetInstanceByKey(object key)
        {
            return _widgetTree.GetInstanceByKey(key);
        }

        /// <summary>Returns the current root instance.</summary>
        public Widget GetRoot()
        {
            return _widgetTree.GetRootInstance();
        }

        /// <summary>Returns the actual Flexbox node of a widget. (REQ-008)</summary>
        public Node GetNode(Widget instance)
        {
            return _widgetTree.GetNode(instance);
        }

        /// <summary>Returns the calculated layout rectangles.</summary>
        public IReadOnlyDictionary<Widget, Rect> GetLayoutResults()
        {
            return _layoutCalculator.Results;
        }

        /// <summary>Tears down owned widget resources idempotently. (REQ-004)</summary>
        public void Shutdown()
        {
            if (_isShutdown)
                return;

            var root = _widgetTree.GetRootInstance();
            if (root != null)
            {
                _widgetTree.RemoveInstance(root);
            }
            _signalQueue.Clear();
            _layoutCalculator.Clear();
            RefreshGeometry();
            _isShutdown = true;
        }

        private void ReconcileRoot(Widget rootDefinition)
        {
            if (_lastRootDefinition == rootDefinition)
                return;

            if (_reconciler.ReconcileRootInstance(rootDefinition?.Clone()))
                _isLayoutDirty = true;

            _lastRootDefinition = rootDefinition;
        }

        private void UpdateAvailableRect(Rect availableRect)
        {
            if (availableRect == _lastAvailableRect)
                return;

            _isLayoutDirty = true;
            _lastAvailableRect = availableRect;
        }

        private void ConsumeSignals()
        {
            foreach (var signal in _signalQueue.ConsumeAllSignals())
            {
                if (signal.Source != null && !_widgetTree.Contains(signal.Source))
                    continue;
                Log.Debug("M-WIDGET-ENGINE", "signal.executed", ("signal", signal));
                signal.Action();
                _isLayoutDirty = true;
            }
        }

        private void CalculateLayoutIfDirty(Rect availableRect)
        {
            if (!_isLayoutDirty)
                return;

            DidLayoutCalculationHappenThisCycle = true;
            Log.Info("M-WIDGET-ENGINE", "layout.calculated", ("rect", availableRect));
            _layoutCalculator.CalculateLayout(availableRect);
            _isLayoutDirty = false;
        }

        internal void RefreshGeometry()
        {
            _geometryBuilder.Refresh();
        }

        private void ReconcileChildInstances(Widget parentInstance)
        {
            _reconciler.ReconcileChildInstances(parentInstance);
        }
    }
}
