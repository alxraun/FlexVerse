// parents: @.frostline/graph/m-widget-tree.xml

using System;
using System.Collections.Generic;
using System.Linq;
using FlexVerse.Flexbox;

namespace FlexVerse
{
    /// <summary>Tracks the keyed widget instance hierarchy and its Flexbox nodes. (M-WIDGET-TREE)</summary>
    public sealed class WidgetTree(WidgetEngine engine)
    {
        private sealed class InstanceData(Widget instance, Node node, Widget parent)
        {
            public Widget Instance { get; } = instance;
            public Node Node { get; } = node;
            public Widget Parent { get; set; } = parent;
            public WidgetList Children { get; } = [];
        }

        private readonly Dictionary<Widget, InstanceData> _instances = [];
        private readonly Dictionary<object, Widget> _instancesByKey = [];
        private Widget _rootInstance;
        private readonly WidgetEngine _engine = engine;
        private readonly BehaviorRegistry _registry = FlexVerseRoot.Instance.BehaviorRegistry;

        /// <summary>Creates an instance from a definition and mounts it. (EFF-001)</summary>
        public Widget CreateInstance(Widget definition, Widget parentInstance)
        {
            if (parentInstance != null && !_instances.ContainsKey(parentInstance))
            {
                throw new InvalidOperationException("Parent instance is not part of this tree.");
            }

            if (parentInstance == null && _rootInstance != null)
            {
                throw new InvalidOperationException("Tree already has a root instance.");
            }

            var newInstance = (Widget)Activator.CreateInstance(definition.GetType());
            newInstance.Engine = _engine;
            newInstance.Key = definition.Key;
            var node = CreateNode(newInstance);

            RegisterInstance(newInstance, node, parentInstance);
            AttachInstance(newInstance, parentInstance);

            if (parentInstance == null)
            {
                _rootInstance = newInstance;
            }

            Log.Info(
                "M-WIDGET-TREE",
                "instance.created",
                ("definition", definition),
                ("instance", newInstance),
                ("parent", parentInstance)
            );

            newInstance.LifecycleState = Widget.Lifecycle.Instance;

            newInstance.InitMembersFromCreation();
            newInstance.InitMembersFromDefinition(definition);
            newInstance.InitInheritedContext();
            newInstance.MountInvoke();
            newInstance.OnMount();

            return newInstance;
        }

        /// <summary>Removes an instance and its subtree, releasing ownership. (REQ-002)</summary>
        public void RemoveInstance(Widget instance)
        {
            if (!_instances.ContainsKey(instance))
            {
                return;
            }

            Log.Debug("M-WIDGET-TREE", "instance.removed", ("instance", instance));

            var childrenSnapshot = GetChildInstances(instance).ToArray();
            foreach (var child in childrenSnapshot)
            {
                if (child != null)
                {
                    RemoveInstance(child);
                }
            }

            instance.OnUnmount();
            instance.UnmountInvoke();
            _registry.UnregisterSubscriptions(instance);
            instance.DeinitInheritedContext();
            instance.ReleaseBehaviors();
            _engine.FocusManager.Unregister(instance);
            DetachInstance(instance);
            UnregisterInstance(instance);

            if (ReferenceEquals(_rootInstance, instance))
            {
                _rootInstance = null;
            }

            instance.LifecycleState = Widget.Lifecycle.Definition;
        }

        /// <summary>Moves an instance under a new parent at the given index.</summary>
        public void MoveInstance(Widget instance, Widget newParent, int newIndex)
        {
            if (!_instances.ContainsKey(instance))
            {
                throw new InvalidOperationException("Instance is not part of this tree.");
            }
            if (!_instances.ContainsKey(newParent))
            {
                throw new InvalidOperationException(
                    "New parent instance is not part of this tree."
                );
            }

            Log.Info(
                "M-WIDGET-TREE",
                "instance.moved",
                ("instance", instance),
                ("newParent", newParent),
                ("newIndex", newIndex)
            );

            DetachInstance(instance);
            AttachInstance(instance, newParent, newIndex);
        }

        /// <summary>Creates a Flexbox node wired to a widget's measure hooks.</summary>
        public Node CreateNode(Widget widget)
        {
            var node = Flex.CreateDefaultNode();
            node.Context = widget;
            var measure = widget.GetMeasureFunc();
            if (measure is not null)
            {
                node.SetMeasureFunc(measure);
            }
            var baseline = widget.GetBaselineFunc();
            if (baseline is not null)
            {
                node.SetBaselineFunc(baseline);
            }
            return node;
        }

        /// <summary>Returns the current root instance.</summary>
        public Widget GetRootInstance()
        {
            return _rootInstance;
        }

        /// <summary>Returns the Flexbox node of the root instance.</summary>
        public Node GetRootNode()
        {
            if (_rootInstance == null)
            {
                return null;
            }

            return _instances.TryGetValue(_rootInstance, out var data) ? data.Node : null;
        }

        /// <summary>Returns the Flexbox node of an instance.</summary>
        public Node GetNode(Widget instance)
        {
            return _instances.TryGetValue(instance, out var data) ? data.Node : null;
        }

        /// <summary>Returns the parent instance of a widget.</summary>
        public Widget GetParentInstance(Widget instance)
        {
            return _instances.TryGetValue(instance, out var data) ? data.Parent : null;
        }

        /// <summary>Returns the child instances of a widget.</summary>
        public WidgetList GetChildInstances(Widget instance)
        {
            return _instances.TryGetValue(instance, out var data) ? data.Children : [];
        }

        internal IReadOnlyList<Widget> GetChildrenInPaintOrder(Widget instance)
        {
            var children = GetChildInstances(instance);
            if (children.Count <= 1)
                return children;

            return children
                .Where(child => child != null)
                .OrderBy(child => child.ZIndex.Value)
                .ToArray();
        }

        /// <summary>Returns the instance registered under a reconciliation key. (REQ-001)</summary>
        public Widget GetInstanceByKey(object key)
        {
            return _instancesByKey.TryGetValue(key, out var instance) ? instance : null;
        }

        /// <summary>Reports whether an instance belongs to this tree.</summary>
        public bool Contains(Widget instance)
        {
            return _instances.ContainsKey(instance);
        }

        /// <summary>Returns the ancestor depth of a widget.</summary>
        public int GetDepth(Widget widget)
        {
            var depth = 0;
            var current = widget;
            while (true)
            {
                var parent = GetParentInstance(current);
                if (parent == null)
                {
                    return depth;
                }

                depth++;
                current = parent;
            }
        }

        /// <summary>Returns the deepest candidate by ancestor depth.</summary>
        public Widget FindDeepest(IEnumerable<Widget> candidates)
        {
            Widget deepestWidget = null;
            var maxDepth = -1;
            foreach (var candidate in candidates)
            {
                var depth = GetDepth(candidate);
                if (depth > maxDepth)
                {
                    maxDepth = depth;
                    deepestWidget = candidate;
                }
            }

            return deepestWidget;
        }

        /// <summary>Returns the root-to-widget ancestor path in outer-to-inner order.</summary>
        public List<Widget> GetAncestorPath(Widget widget)
        {
            var path = new List<Widget>();
            var current = widget;
            while (current != null)
            {
                path.Add(current);
                current = GetParentInstance(current);
            }
            path.Reverse();
            return path;
        }

        /// <summary>Yields the ancestors of an instance from itself to the root.</summary>
        public IEnumerable<Widget> TraverseAncestors(Widget startInstance)
        {
            var currentInstance = startInstance;
            while (currentInstance != null)
            {
                yield return currentInstance;
                currentInstance = GetParentInstance(currentInstance);
            }
        }

        private void RegisterInstance(Widget instance, Node node, Widget parent)
        {
            Log.Debug(
                "M-WIDGET-TREE",
                "instance.registered",
                ("instance", instance),
                ("parent", parent)
            );
            var data = new InstanceData(instance, node, parent);
            _instances[instance] = data;
            if (instance.Key != null)
            {
                _instancesByKey[instance.Key] = instance;
            }
        }

        private void UnregisterInstance(Widget instance)
        {
            Log.Debug("M-WIDGET-TREE", "instance.unregistered", ("instance", instance));
            if (instance.Key != null)
            {
                if (
                    _instancesByKey.TryGetValue(instance.Key, out var instanceByKey)
                    && ReferenceEquals(instanceByKey, instance)
                )
                {
                    _instancesByKey.Remove(instance.Key);
                }
            }

            _instances.Remove(instance);
        }

        private void AttachInstance(Widget instance, Widget parent, int? index = null)
        {
            if (!_instances.TryGetValue(instance, out var instanceData))
            {
                throw new InvalidOperationException(
                    "Instance must be registered before adding to parent."
                );
            }

            Log.Debug(
                "M-WIDGET-TREE",
                "instance.attached",
                ("instance", instance),
                ("parent", parent),
                ("index", index)
            );

            instanceData.Parent = parent;

            if (parent == null)
            {
                return;
            }

            if (!_instances.TryGetValue(parent, out var parentData))
            {
                throw new InvalidOperationException("Parent instance is not registered.");
            }

            var parentChildren = parentData.Children;
            int insertionIndex;
            if (index.HasValue)
            {
                var idx = index.Value;
                if (idx < 0)
                    idx = 0;
                if (idx > parentChildren.Count)
                    idx = parentChildren.Count;

                insertionIndex = idx;
                parentChildren.Insert(insertionIndex, instance);
            }
            else
            {
                insertionIndex = parentChildren.Count;
                parentChildren.Add(instance);
            }

            parentData.Node.InsertChild(instanceData.Node, insertionIndex);
        }

        private void DetachInstance(Widget instance)
        {
            if (!_instances.TryGetValue(instance, out var instanceData))
            {
                return;
            }

            var parentInstance = instanceData.Parent;
            Log.Debug(
                "M-WIDGET-TREE",
                "instance.detached",
                ("instance", instance),
                ("parent", parentInstance)
            );
            if (
                parentInstance != null
                && _instances.TryGetValue(parentInstance, out var parentData)
            )
            {
                parentData.Children.Remove(instance);
                parentData.Node.RemoveChild(instanceData.Node);
            }

            instanceData.Parent = null;
        }
    }
}
