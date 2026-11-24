// parents: @.frostline/graph/m-dbg-core.xml

using FlexVerse;
using Verse;

namespace FlexTools.WidgetInspector
{
    /// <summary>Stores widget inspector display settings.</summary>
    public class InspectorSettings : IExposable
    {
        /// <summary>Shows the box model overlay when true.</summary>
        public bool ShowBoxModel = true;

        /// <summary>Shows the update heatmap when true.</summary>
        public bool ShowHeatmap = true;

        /// <summary>Heat decay duration in seconds.</summary>
        public float HeatDecayDuration = 0.5f;

        /// <summary>Saves and loads the settings values.</summary>
        public void ExposeData()
        {
            Scribe_Values.Look(ref ShowBoxModel, "showBoxModel", true);
            Scribe_Values.Look(ref ShowHeatmap, "showHeatmap", true);
            Scribe_Values.Look(ref HeatDecayDuration, "heatDecayDuration", 0.5f);
        }
    }
}
