// parents: @.frostline/graph/m-widget-engine.xml

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexVerse
{
    internal sealed class WidgetReconciler
    {
        private readonly WidgetTree _widgetTree;
        private readonly Action<Widget> _reconcileChildren;

        public WidgetReconciler(WidgetTree widgetTree, Action<Widget> reconcileChildren)
        {
            _widgetTree = widgetTree;
            _reconcileChildren = reconcileChildren;
        }

        public bool ReconcileRootInstance(Widget rootDefinition)
        {
            var rootInstance = _widgetTree.GetRootInstance();

            if (rootDefinition == null)
            {
                if (rootInstance != null)
                {
                    _widgetTree.RemoveInstance(rootInstance);
                    Log.Debug("M-WIDGET-TREE", "root.instance.removed", ("root", rootInstance));
                    return true;
                }
                return false;
            }

            if (rootInstance != null && rootInstance.GetType() == rootDefinition.GetType())
            {
                rootInstance.ApplyPropsFromDefinition(rootDefinition);
                return false;
            }

            if (rootInstance != null)
            {
                _widgetTree.RemoveInstance(rootInstance);
            }

            rootInstance = _widgetTree.CreateInstance(rootDefinition, null);

            _reconcileChildren(rootInstance);

            return true;
        }

        public void ReconcileChildInstances(Widget parentInstance)
        {
            var childInstances = _widgetTree.GetChildInstances(parentInstance);
            var childDefinitions = GetChildDefinitions(parentInstance);
            var childInstancesByKey = GetChildInstancesByKey(childInstances);
            var newChildren = ReconcileDefinitionsWithInstances(
                parentInstance,
                childDefinitions,
                childInstancesByKey
            );
            RemoveObsoleteInstances(childInstances, newChildren);
            SyncChildOrder(parentInstance, newChildren);

            Log.Info(
                "M-WIDGET-TREE",
                "children.reconciled",
                ("parent", parentInstance),
                ("definitions", childDefinitions.Count),
                ("children", newChildren.Count)
            );
        }

        private List<Widget> GetChildDefinitions(Widget parentInstance)
        {
            return parentInstance.BuildRenderTree()?.Where(c => c != null).Select(c => c!).ToList()
                ?? [];
        }

        private Dictionary<object, Widget> GetChildInstancesByKey(IList<Widget> childInstances)
        {
            var map = new Dictionary<object, Widget>();
            for (int i = 0; i < childInstances.Count; i++)
            {
                var childInstance = childInstances[i];
                if (childInstance == null)
                    continue;

                var instanceKey = childInstance.Key;
                if (instanceKey != null)
                    map[instanceKey] = childInstance;
            }
            return map;
        }

        private List<Widget> ReconcileDefinitionsWithInstances(
            Widget parentInstance,
            IReadOnlyList<Widget> childDefinitions,
            IReadOnlyDictionary<object, Widget> childInstancesByKey
        )
        {
            var newChildInstances = new List<Widget>();
            var index = 0;

            foreach (var childDefinition in childDefinitions)
            {
                childDefinition.Key ??= new ImplicitKey(
                    parentInstance.GetType(),
                    childDefinition.GetType(),
                    index
                );
                var key = childDefinition.Key;
                index++;

                if (childInstancesByKey.TryGetValue(key, out var childInstance))
                {
                    if (childInstance.GetType() == childDefinition.GetType())
                    {
                        childInstance.ApplyPropsFromDefinition(childDefinition);
                        newChildInstances.Add(childInstance);
                    }
                    else
                    {
                        _widgetTree.RemoveInstance(childInstance);
                        var created = _widgetTree.CreateInstance(childDefinition, parentInstance);
                        newChildInstances.Add(created);
                    }
                }
                else
                {
                    var created = _widgetTree.CreateInstance(childDefinition, parentInstance);
                    newChildInstances.Add(created);
                }
            }

            return newChildInstances;
        }

        private void RemoveObsoleteInstances(
            IList<Widget> currentChildren,
            IReadOnlyList<Widget> newChildren
        )
        {
            var toRemove = new HashSet<Widget>(
                currentChildren.Where(c => c != null)!.Cast<Widget>()
            );
            foreach (var kept in newChildren)
            {
                toRemove.Remove(kept);
            }
            foreach (var removed in toRemove)
            {
                _widgetTree.RemoveInstance(removed);
            }
        }

        private void SyncChildOrder(Widget parentInstance, IReadOnlyList<Widget> newChildren)
        {
            for (int i = 0; i < newChildren.Count; i++)
            {
                var child = newChildren[i];
                var currentParent = _widgetTree.GetParentInstance(child);
                if (!ReferenceEquals(currentParent, parentInstance))
                {
                    _widgetTree.MoveInstance(child, parentInstance, i);
                }
                else
                {
                    var siblings = _widgetTree.GetChildInstances(parentInstance);
                    if (!(i < siblings.Count && ReferenceEquals(siblings[i], child)))
                    {
                        _widgetTree.MoveInstance(child, parentInstance, i);
                    }
                }
            }
        }
    }
}
