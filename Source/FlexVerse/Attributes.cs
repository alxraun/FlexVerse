// parents: @.frostline/graph/m-widget.xml

using System;

namespace FlexVerse
{
    /// <summary>Marks a Behavior-typed property as a reactive FlexVerse property shared by widgets and FVP owners. (M-WIDGET)</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class FVPAttribute : Attribute { }

    /// <summary>Marks a widget property as an externally supplied input prop.</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class PropAttribute : FVPAttribute
    {
        /// <summary>Requests reconciliation after property assignment.</summary>
        public bool ReconcileOnSet { get; set; }

        /// <summary>Requests reconciliation after each update tick.</summary>
        public bool ReconcileOnUpdate { get; set; }

        /// <summary>Requests a layout-dirty mark after property assignment.</summary>
        public bool MarkDirtyOnSet { get; set; }

        /// <summary>Requests a layout-dirty mark after each update tick.</summary>
        public bool MarkDirtyOnUpdate { get; set; }
    }

    /// <summary>Marks a widget property as instance state.</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class StateAttribute : FVPAttribute
    {
        /// <summary>Requests reconciliation after property assignment.</summary>
        public bool ReconcileOnSet { get; set; }

        /// <summary>Requests reconciliation after each update tick.</summary>
        public bool ReconcileOnUpdate { get; set; }

        /// <summary>Requests a layout-dirty mark after property assignment.</summary>
        public bool MarkDirtyOnSet { get; set; }

        /// <summary>Requests a layout-dirty mark after each update tick.</summary>
        public bool MarkDirtyOnUpdate { get; set; }
    }

    /// <summary>Marks a widget property as an inherited context provider and consumer.</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class InheritedContextAttribute : FVPAttribute
    {
        /// <summary>Requests reconciliation after property assignment.</summary>
        public bool ReconcileOnSet { get; set; }

        /// <summary>Requests reconciliation after each update tick.</summary>
        public bool ReconcileOnUpdate { get; set; }

        /// <summary>Requests a layout-dirty mark after property assignment.</summary>
        public bool MarkDirtyOnSet { get; set; }

        /// <summary>Requests a layout-dirty mark after each update tick.</summary>
        public bool MarkDirtyOnUpdate { get; set; }
    }
}
