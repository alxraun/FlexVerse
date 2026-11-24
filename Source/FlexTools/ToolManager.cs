// parents: @.frostline/graph/p-flextools.xml

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace FlexTools
{
    /// <summary>Discovers, activates, and manages tool instances.</summary>
    public static class ToolManager
    {
        private static readonly Dictionary<Type, Tool> _activeTools = [];
        private static readonly List<ToolMetadata> _discoveredTools = [];
        private static readonly Dictionary<string, ToolMetadata> _metadataById = [];

        private readonly record struct ToolMetadata(string Id, Type Type, bool IsEnabledByDefault);

        /// <summary>Discovers all registered tools and activates the enabled ones.</summary>
        public static void Initialize()
        {
            _discoveredTools.Clear();
            _metadataById.Clear();
            DiscoverTools();
            ReconcileSettings();
            ActivateEnabledTools();
        }

        /// <summary>Shuts down and clears all active tools.</summary>
        public static void Shutdown()
        {
            foreach (var tool in _activeTools.Values.ToList())
            {
                tool.Shutdown();
            }
            _activeTools.Clear();
        }

        /// <summary>Returns the active tool instance of the given type, or null.</summary>
        public static T GetTool<T>()
            where T : Tool
        {
            return _activeTools.TryGetValue(typeof(T), out var tool) ? (T)tool : null;
        }

        /// <summary>Activates or deactivates the tool with the given identifier.</summary>
        public static void RefreshTool(string toolId, bool isEnabled)
        {
            if (!_metadataById.TryGetValue(toolId, out var metadata))
            {
                return;
            }

            if (isEnabled)
            {
                ActivateTool(metadata);
                return;
            }

            DeactivateTool(metadata.Type);
        }

        private static void DiscoverTools()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var toolTypes = assembly
                .GetTypes()
                .Where(type =>
                    type.IsClass
                    && !type.IsAbstract
                    && type.GetCustomAttribute<ToolAttribute>() != null
                );

            foreach (var type in toolTypes)
            {
                var attribute = type.GetCustomAttribute<ToolAttribute>();
                if (attribute != null)
                {
                    var metadata = new ToolMetadata(
                        attribute.Id,
                        type,
                        attribute.IsEnabledByDefault
                    );
                    _discoveredTools.Add(metadata);
                    _metadataById[metadata.Id] = metadata;
                }
            }
        }

        private static void ReconcileSettings()
        {
            var settings = FlexToolsMod.Settings;
            foreach (var tool in _discoveredTools)
            {
                if (!settings.HasSettingForTool(tool.Id))
                {
                    settings.SetToolEnabled(tool.Id, tool.IsEnabledByDefault);
                }
            }
        }

        private static void ActivateEnabledTools()
        {
            foreach (var toolMetadata in _discoveredTools)
            {
                if (FlexToolsMod.Settings.IsToolEnabled(toolMetadata.Id))
                {
                    ActivateTool(toolMetadata);
                }
            }
        }

        private static void ActivateTool(ToolMetadata toolMetadata)
        {
            if (_activeTools.ContainsKey(toolMetadata.Type))
            {
                return;
            }

            try
            {
                var toolInstance = (Tool)Activator.CreateInstance(toolMetadata.Type);
                toolInstance.Initialize();
                _activeTools.Add(toolMetadata.Type, toolInstance);
            }
            catch (Exception ex)
            {
                FlexVerse.Log.Error(
                    "M-TOOL-MANAGER",
                    "tool.activate.failed",
                    ("tool", toolMetadata.Id),
                    ("error", ex.Message)
                );
            }
        }

        private static void DeactivateTool(Type toolType)
        {
            if (_activeTools.TryGetValue(toolType, out var tool))
            {
                tool.Shutdown();
                _activeTools.Remove(toolType);
            }
        }
    }
}
