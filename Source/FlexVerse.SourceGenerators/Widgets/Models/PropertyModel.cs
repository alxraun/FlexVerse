// parents: @.frostline/graph/m-widget-models.xml

using System;

namespace FlexVerse.SourceGenerators.Widgets.Models
{
    /// <summary>Defines the PropertyFilter values.</summary>
    [Flags]
    public enum PropertyFilter
    {
        None = 0,
        Fvp = 1 << 0,
        Prop = 1 << 1,
        State = 1 << 2,
        InheritedContext = 1 << 4,
        IcProvider = 1 << 5,
        IcConsumer = 1 << 6,
    }

    /// <summary>Performs the Property Model operation.</summary>
    public sealed record PropertyModel(
        string Name,
        TypeIdentity Type,
        PropertyKind Kind,
        Initializer Initializer,
        Reactivity Reactivity,
        bool IsInherited,
        bool IsOverride,
        bool IsNew,
        bool IsAbstract,
        string SlotOwner = ""
    )
    {
        /// <summary>Gets the field name.</summary>
        public string FieldName => $"_{StringUtils.ToCamel(Name)}";

        /// <summary>Gets the logic name.</summary>
        public string LogicName => $"_{Name}";

        /// <summary>Gets the ancestor hook name.</summary>
        public string AncestorHookName => $"_Update{Name}FromAncestor";

        /// <summary>Gets the type name.</summary>
        public string TypeName => Type.ConstructedName;

        /// <summary>Gets the assignment.</summary>
        public string Assignment => Initializer.Assignment;

        /// <summary>Gets the kind as flag.</summary>
        public PropertyFilter KindAsFlag =>
            Kind switch
            {
                PropertyKind.Fvp => PropertyFilter.Fvp,
                PropertyKind.Prop => PropertyFilter.Prop,
                PropertyKind.State => PropertyFilter.State,
                PropertyKind.InheritedContext => PropertyFilter.InheritedContext,
                _ => PropertyFilter.None,
            };

        /// <summary>Gets a value indicating whether the property consumes values.</summary>
        public bool IsConsumer => Kind == PropertyKind.InheritedContext && !Initializer.IsDefined;

        /// <summary>Gets a value indicating whether the property provides values.</summary>
        public bool IsProvider => Kind == PropertyKind.InheritedContext && Initializer.IsDefined;
    }
}
