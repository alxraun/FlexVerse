// parents: @.frostline/graph/m-widget.xml
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Reports the coordinate space and native work of a widget-opened child context.</summary>
    public readonly struct ChildrenGuiSpace
    {
        /// <summary>Gets the child-context origin in the incoming GUI space.</summary>
        public Vector2 Origin { get; }

        /// <summary>Reports whether the native context applies the children transform.</summary>
        public bool AppliesChildrenTransform { get; }

        /// <summary>Reports whether the native context clips children.</summary>
        public bool ClipsChildren { get; }

        /// <summary>Creates the child-context report.</summary>
        public ChildrenGuiSpace(Vector2 origin, bool appliesChildrenTransform, bool clipsChildren)
        {
            Origin = origin;
            AppliesChildrenTransform = appliesChildrenTransform;
            ClipsChildren = clipsChildren;
        }
    }
}
