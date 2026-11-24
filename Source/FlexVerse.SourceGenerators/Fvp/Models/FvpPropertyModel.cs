// parents: @.frostline/graph/m-flexverse-property.xml

namespace FlexVerse.SourceGenerators.Fvp.Models
{
    /// <summary>Performs the Fvp Property Model operation.</summary>
    public sealed record FvpPropertyModel(
        string Name,
        TypeIdentity Type,
        Initializer Initializer,
        bool IsOverride,
        bool IsNew,
        bool IsInherited,
        string DeclaringTypeName
    )
    {
        /// <summary>Gets the type name.</summary>
        public string TypeName => Type.ConstructedName;

        /// <summary>Gets the assignment.</summary>
        public string Assignment => Initializer.Assignment;

        /// <summary>Gets the field name.</summary>
        public string FieldName => $"_{StringUtils.ToCamel(Name)}";

        /// <summary>Gets the logic name.</summary>
        public string LogicName => $"_{Name}";

        /// <summary>Gets the ref type name.</summary>
        public string RefTypeName => IsInherited ? $"{DeclaringTypeName}.{Name}Ref" : $"{Name}Ref";
    }
}
