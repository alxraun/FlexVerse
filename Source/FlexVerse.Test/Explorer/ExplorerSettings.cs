// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FlexVerse.Test.Explorer
{
    public partial class ExplorerSettings : ModSettings
    {
        private List<IExposable> _dataSections = [];

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
