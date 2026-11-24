// parents: @.frostline/graph/m-flexverse-property.xml

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FlexVerse.SourceGenerators.Fvp.Models
{
    /// <summary>Performs the Fvp Model operation.</summary>
    public sealed record FvpModel(
        string ClassName,
        string DeclarationName,
        string Namespace,
        ImmutableArray<string> Usings,
        ImmutableArray<StructureModel> Hierarchy,
        ImmutableArray<FvpPropertyModel> Properties,
        bool IsSealed,
        bool HasFvpBase
    )
    {
        /// <summary>Gets the structure.</summary>
        public StructureModel Structure => Hierarchy.Last();

        /// <summary>Gets the own properties.</summary>
        public IEnumerable<FvpPropertyModel> OwnProperties =>
            Properties.Where(p => !p.IsInherited && !p.IsOverride);

        /// <summary>Gets the lifecycle method modifier.</summary>
        public string LifecycleMethodModifier =>
            HasFvpBase ? "override"
            : IsSealed ? ""
            : "virtual";

        /// <summary>Gets the lifecycle access.</summary>
        public string LifecycleAccess => !HasFvpBase && IsSealed ? "private" : "protected";

        /// <summary>Gets the qualified name.</summary>
        public string QualifiedName => string.Join(".", Hierarchy.Select(t => t.Name));
    }
}
