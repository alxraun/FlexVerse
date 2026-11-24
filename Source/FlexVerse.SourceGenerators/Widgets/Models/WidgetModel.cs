// parents: @.frostline/graph/m-widget-models.xml

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets.Models
{
    /// <summary>Performs the Widget Model operation.</summary>
    public sealed record WidgetModel(
        string ClassName,
        string DeclarationName,
        string Namespace,
        bool IsBase,
        bool IsAbstract,
        ImmutableArray<string> Usings,
        ImmutableArray<StructureModel> Hierarchy,
        ImmutableArray<PropertyModel> Properties
    )
    {
        /// <summary>Gets the widget structure.</summary>
        public StructureModel WidgetStructure => Hierarchy.Last();

        /// <summary>Gets the qualified name.</summary>
        public string QualifiedName =>
            Hierarchy.IsEmpty ? ClassName : string.Join(".", Hierarchy.Select(t => t.Name));

        /// <summary>Gets the own properties.</summary>
        public ImmutableArray<PropertyModel> OwnProperties =>
            Properties.Where(p => !p.IsInherited).ToImmutableArray();

        /// <summary>Performs the Get Properties operation.</summary>
        public IEnumerable<PropertyModel> GetProperties(
            PropertyFilter filter,
            bool inherited = false
        )
        {
            var source = inherited ? Properties : OwnProperties;

            foreach (var p in source)
            {
                var effectiveFilter = filter;
                if (
                    (effectiveFilter & (PropertyFilter.IcProvider | PropertyFilter.IcConsumer)) != 0
                )
                {
                    effectiveFilter |= PropertyFilter.InheritedContext;
                }
                if (!effectiveFilter.HasFlag(p.KindAsFlag))
                {
                    continue;
                }

                if (p.Kind == PropertyKind.InheritedContext)
                {
                    var roleFilter =
                        filter & (PropertyFilter.IcProvider | PropertyFilter.IcConsumer);
                    if (roleFilter != PropertyFilter.None)
                    {
                        if (
                            !(
                                (p.IsProvider && roleFilter.HasFlag(PropertyFilter.IcProvider))
                                || (p.IsConsumer && roleFilter.HasFlag(PropertyFilter.IcConsumer))
                            )
                        )
                        {
                            continue;
                        }
                    }
                }

                yield return p;
            }
        }

        /// <summary>Gets the lifecycle method modifier.</summary>
        public string LifecycleMethodModifier => IsBase ? "virtual" : "override";
    }
}
