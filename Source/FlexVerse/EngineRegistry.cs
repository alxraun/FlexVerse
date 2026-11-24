// parents: @.frostline/graph/m-engine-registry.xml

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FlexVerse
{
    /// <summary>Manages active widget engines keyed by unique strings. (M-ENGINE-REGISTRY)</summary>
    public static class EngineRegistry
    {
        private static readonly ConcurrentDictionary<string, WidgetEngine> _engines =
            new ConcurrentDictionary<string, WidgetEngine>();

        private static string ValidateKey(string key)
        {
            if (key == null)
                throw new ArgumentException("Engine key must be a non-null string.", nameof(key));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(
                    "Engine key must not be empty or whitespace.",
                    nameof(key)
                );
            return key;
        }

        /// <summary>Returns the engine for a key, creating it when absent. (REQ-002)</summary>
        public static WidgetEngine GetOrCreate(string key)
        {
            ValidateKey(key);
            return _engines.GetOrAdd(key, k => new WidgetEngine(k));
        }

        /// <summary>Returns the engine for a key or null when unregistered. (REQ-003)</summary>
        public static WidgetEngine Get(string key)
        {
            ValidateKey(key);
            return _engines.TryGetValue(key, out var engine) ? engine : null;
        }

        /// <summary>Attempts to fetch the engine registered under a key. (REQ-004)</summary>
        public static bool TryGet(string key, out WidgetEngine engine)
        {
            ValidateKey(key);
            return _engines.TryGetValue(key, out engine);
        }

        /// <summary>Shuts down and removes the engine for a key. (REQ-005)</summary>
        public static bool Release(string key)
        {
            ValidateKey(key);
            if (!_engines.TryRemove(key, out var engine))
                return false;
            engine.Shutdown();
            return true;
        }

        /// <summary>Reports whether an engine is registered under a key. (REQ-006)</summary>
        public static bool Contains(string key)
        {
            ValidateKey(key);
            return _engines.ContainsKey(key);
        }

        /// <summary>Returns an immutable snapshot of the live engines. (REQ-007)</summary>
        public static IReadOnlyCollection<WidgetEngine> List()
        {
            return _engines.Values.ToArray();
        }

        /// <summary>Shuts down and removes every registered engine. (REQ-008)</summary>
        public static void Clear()
        {
            foreach (var key in _engines.Keys.ToArray())
            {
                if (_engines.TryRemove(key, out var engine))
                    engine.Shutdown();
            }
        }
    }
}
