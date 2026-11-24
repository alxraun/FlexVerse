// parents: @.frostline/graph/p-flextools.xml

using System;

namespace FlexTools
{
    /// <summary>Marks a Tool subclass with a registration identifier.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ToolAttribute(string id) : Attribute
    {
        /// <summary>Gets the unique tool identifier.</summary>
        public string Id { get; } = id;

        /// <summary>Gets or sets a value indicating whether the tool is enabled by default.</summary>
        public bool IsEnabledByDefault { get; set; } = false;
    }

    /// <summary>Base class for a FlexTools developer tool.</summary>
    public abstract class Tool
    {
        /// <summary>Initializes the tool.</summary>
        public virtual void Initialize() { }

        /// <summary>Shuts down the tool and releases its resources.</summary>
        public virtual void Shutdown() { }
    }
}
