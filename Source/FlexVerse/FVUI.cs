// parents: @.frostline/graph/m-fvui.xml

using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Routes host rects and widget trees to their keyed engine. (M-FVUI)</summary>
    public static class FVUI
    {
        /// <summary>Drives one UI cycle and returns the layout results. (REQ-001)</summary>
        public static IReadOnlyDictionary<Widget, Rect> Render(string key, Rect rect, Widget root)
        {
            var engine = EngineRegistry.GetOrCreate(key);
            engine.ProcessUI(root, rect);
            return engine.GetLayoutResults();
        }

        /// <summary>Releases the engine registered under a key. (REQ-002)</summary>
        public static bool Release(string key)
        {
            return EngineRegistry.Release(key);
        }

        /// <summary>Returns the engine registered under a key. (REQ-004)</summary>
        public static WidgetEngine GetEngine(string key)
        {
            return EngineRegistry.Get(key);
        }
    }
}
