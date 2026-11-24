// parents: @.frostline/graph/m-reactivity-analyzer.xml

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexVerse.SourceGenerators.Widgets.Extraction
{
    /// <summary>Represents the PropertyExtractor.</summary>
    public static class PropertyExtractor
    {
        /// <summary>Performs the Extract operation.</summary>
        public static ImmutableArray<PropertyModel> Extract(
            INamedTypeSymbol classSymbol,
            Compilation compilation,
            CancellationToken ct,
            ImmutableArray<Diagnostic>.Builder? diagnostics = null
        )
        {
            var defaults = ReactivityAnalyzer.AnalyzeClassDefaults(classSymbol, compilation, ct);

            var properties = ImmutableArray.CreateBuilder<PropertyModel>();
            var processedPropertyNames = new System.Collections.Generic.HashSet<string>();

            INamedTypeSymbol? currentClassSymbol = classSymbol;
            while (
                currentClassSymbol != null
                && currentClassSymbol.SpecialType != SpecialType.System_Object
            )
            {
                foreach (
                    var member in currentClassSymbol
                        .GetMembers()
                        .Where(m => !m.IsImplicitlyDeclared)
                )
                {
                    if (
                        member is not IPropertySymbol propSymbol
                        || propSymbol.IsStatic
                        || processedPropertyNames.Contains(propSymbol.Name)
                    )
                    {
                        continue;
                    }

                    processedPropertyNames.Add(propSymbol.Name);

                    bool isInherited = !SymbolEqualityComparer.Default.Equals(
                        propSymbol.ContainingType,
                        classSymbol
                    );

                    if (
                        !AttributeReader.TryResolveKind(
                            propSymbol,
                            out var kind,
                            out var primaryAttribute
                        )
                    )
                    {
                        continue;
                    }

                    if (propSymbol.Type is not INamedTypeSymbol propertyTypeSymbol)
                    {
                        continue;
                    }

                    var typeIdentity = BehaviorReader.Read(propertyTypeSymbol);
                    if (typeIdentity == null)
                    {
                        continue;
                    }

                    var slot = propSymbol;
                    while (slot.OverriddenProperty is { } overridden)
                        slot = overridden;
                    var slotOwner = slot.ContainingType.ToDisplayString();
                    var model = AssemblePropertyModel(
                        propSymbol,
                        typeIdentity,
                        kind!.Value,
                        primaryAttribute,
                        defaults,
                        isInherited,
                        slotOwner,
                        ct
                    );

                    properties.Add(model);
                }
                currentClassSymbol = currentClassSymbol.BaseType;
            }

            return properties.ToImmutable();
        }

        private static PropertyModel AssemblePropertyModel(
            IPropertySymbol propertySymbol,
            TypeIdentity typeIdentity,
            PropertyKind kind,
            AttributeData? primaryAttribute,
            System.Collections.Generic.Dictionary<string, ReactivityAnalyzer.Defaults> defaults,
            bool isInherited,
            string slotOwner,
            CancellationToken ct
        )
        {
            var propSyntax =
                propertySymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(ct)
                as PropertyDeclarationSyntax;

            var initializerSyntaxString =
                propSyntax != null ? RoslynUtils.GetInitializer(propSyntax) : null;

            var initializer = new Initializer(initializerSyntaxString ?? string.Empty);

            var isOverride = propertySymbol.IsOverride;
            var isAbstract = propertySymbol.IsAbstract;
            var isNew = false;

            if (!isOverride && propSyntax != null)
            {
                foreach (var modifier in propSyntax.Modifiers)
                {
                    if (modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NewKeyword))
                    {
                        isNew = true;
                        break;
                    }
                }
            }

            var reactivity = ReactivityAnalyzer.ResolveReactivity(
                propertySymbol,
                kind,
                primaryAttribute,
                defaults
            );

            return new PropertyModel(
                Name: propertySymbol.Name,
                Type: typeIdentity,
                Kind: kind,
                Initializer: initializer,
                Reactivity: reactivity,
                IsInherited: isInherited,
                IsOverride: isOverride,
                IsNew: isNew,
                IsAbstract: isAbstract,
                SlotOwner: slotOwner
            );
        }
    }
}
