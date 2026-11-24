// parents: @.frostline/graph/p-flextools.xml

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FlexTools
{
    /// <summary>Stores FlexTools tool settings and enable state.</summary>
    public class FlexToolsSettings : ModSettings
    {
        private List<IExposable> _toolSettingsData = [];
        private Dictionary<string, bool> _enabledTools = [];

        /// <summary>Returns the settings section for the given tool type, creating it if needed.</summary>
        public T GetToolSettings<T>()
            where T : class, IExposable, new()
        {
            var section = _toolSettingsData.OfType<T>().FirstOrDefault();

            if (section == null)
            {
                section = new T();
                _toolSettingsData.Add(section);
            }

            return section;
        }

        /// <summary>Returns whether the tool is enabled.</summary>
        public bool IsToolEnabled(string toolId)
        {
            return _enabledTools.TryGetValue(toolId, out var isEnabled) && isEnabled;
        }

        /// <summary>Sets the enabled state for the tool.</summary>
        public void SetToolEnabled(string toolId, bool isEnabled)
        {
            _enabledTools[toolId] = isEnabled;
        }

        /// <summary>Returns whether a stored enabled state exists for the tool.</summary>
        public bool HasSettingForTool(string toolId)
        {
            return _enabledTools.ContainsKey(toolId);
        }

        /// <summary>Saves and loads tool settings through the save system.</summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref _toolSettingsData, "sections", LookMode.Deep);
            Scribe_Collections.Look(
                ref _enabledTools,
                "enabledTools",
                LookMode.Value,
                LookMode.Value
            );

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                _toolSettingsData ??= [];
                _enabledTools ??= [];
            }
        }
    }
}
