// parents: @.frostline/graph/m-flexverse-root.xml

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FlexVerse
{
    /// <summary>Stores FlexVerse mod settings with exposable data sections.</summary>
    public class FlexVerseSettings : ModSettings
    {
        private List<IExposable> _dataSections = [];

        /// <summary>Returns or creates the settings section of the given type.</summary>
        public T GetDataSection<T>()
            where T : class, IExposable, new()
        {
            var section = _dataSections.OfType<T>().FirstOrDefault();

            if (section == null)
            {
                section = new T();
                _dataSections.Add(section);
            }

            return section;
        }

        /// <summary>Saves and loads the settings sections.</summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref _dataSections, "sections", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars && _dataSections == null)
            {
                _dataSections = [];
            }
        }
    }
}
