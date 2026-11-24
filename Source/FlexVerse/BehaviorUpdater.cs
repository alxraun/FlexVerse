// parents: @.frostline/graph/m-behavior-updater.xml

using System;
using System.Collections.Generic;

namespace FlexVerse
{
    /// <summary>Drives subscribed behaviors each frame through weak references. (M-BEHAVIOR-UPDATER)</summary>
    public class BehaviorUpdater
    {
        private readonly List<WeakReference<Behavior>> _activeBehaviors = [];

        /// <summary>Subscribes a behavior to the update loop once. (REQ-001)</summary>
        public void Subscribe(Behavior behavior)
        {
            for (int i = _activeBehaviors.Count - 1; i >= 0; i--)
            {
                if (!_activeBehaviors[i].TryGetTarget(out var existingBehavior))
                {
                    _activeBehaviors.RemoveAt(i);
                }
                else if (existingBehavior == behavior)
                {
                    return;
                }
            }

            _activeBehaviors.Add(new WeakReference<Behavior>(behavior));
        }

        /// <summary>Removes a behavior from the update loop.</summary>
        public void Unsubscribe(Behavior behavior)
        {
            for (int i = _activeBehaviors.Count - 1; i >= 0; i--)
            {
                if (
                    _activeBehaviors[i].TryGetTarget(out var existingBehavior)
                    && existingBehavior == behavior
                )
                {
                    _activeBehaviors.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>Ticks every live subscribed behavior in stable order. (REQ-002)</summary>
        public void Update(float deltaTime)
        {
            int liveIndex = 0;
            for (int i = 0; i < _activeBehaviors.Count; i++)
            {
                if (_activeBehaviors[i].TryGetTarget(out var behavior))
                {
                    behavior.Update(deltaTime);
                    if (liveIndex != i)
                    {
                        _activeBehaviors[liveIndex] = _activeBehaviors[i];
                    }
                    liveIndex++;
                }
            }

            if (liveIndex < _activeBehaviors.Count)
            {
                _activeBehaviors.RemoveRange(liveIndex, _activeBehaviors.Count - liveIndex);
            }
        }

        /// <summary>Clears the subscription list.</summary>
        public void Clear()
        {
            _activeBehaviors.Clear();
        }
    }
}
