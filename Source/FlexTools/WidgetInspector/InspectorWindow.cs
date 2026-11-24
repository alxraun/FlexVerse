// parents: @.frostline/graph/m-dbg-core.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector
{
    /// <summary>Verse window that hosts the inspector UI.</summary>
    public class InspectorWindow : Window
    {
        internal const string UiContextKey = "FlexTools.WidgetInspector.Inspector";
        private readonly WidgetInspector _debugger;

        /// <summary>Callback invoked after the window closes.</summary>
        public System.Action OnClosed;

        /// <summary>Creates the inspector window for the given tool.</summary>
        public InspectorWindow(WidgetInspector debugger)
        {
            _debugger = debugger;
            doCloseX = true;
            draggable = true;
            resizeable = true;
            absorbInputAroundWindow = false;
            layer = WindowLayer.Super;
        }

        /// <summary>Gets the initial window size docked to the screen edge.</summary>
        public override Vector2 InitialSize => new Vector2(540f, Verse.UI.screenHeight);

        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect(
                Verse.UI.screenWidth - InitialSize.x,
                0,
                InitialSize.x,
                InitialSize.y
            );
        }

        /// <summary>Renders the inspector app inside the window.</summary>
        public override void DoWindowContents(Rect inRect) =>
            FVUI.Render(UiContextKey, inRect, _debugger.CreateApp());

        /// <summary>Releases the UI context and notifies listeners on close.</summary>
        public override void PostClose()
        {
            base.PostClose();
            FVUI.Release(UiContextKey);
            OnClosed?.Invoke();
        }
    }
}
