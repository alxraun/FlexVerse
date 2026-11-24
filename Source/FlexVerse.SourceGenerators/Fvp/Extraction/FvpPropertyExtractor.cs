// parents: @.frostline/graph/m-flexverse-property.xml

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FlexVerse.SourceGenerators.Fvp.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexVerse.SourceGenerators.Fvp.Extraction
{
    /// <summary>Extracts [FVP] property models from a partial class and its</summary>
    public static class FvpPropertyExtractor
    {
        /// <summary>Performs the Extract operation.</summary>
        public static ImmutableArray<FvpPropertyModel> Extract(
            INamedTypeSymbol classSymbol,
            CancellationToken ct
        )
        {
            var properties = ImmutableArray.CreateBuilder<FvpPropertyModel>();
            var processedPropertyNames = new HashSet<string>();

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

                    if (!FvpAttributeReader.HasFvpAttribute(propSymbol))
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

                    properties.Add(
                        AssemblePropertyModel(
                            propSymbol,
                            typeIdentity,
                            ct,
                            !SymbolEqualityComparer.Default.Equals(
                                propSymbol.ContainingType,
                                classSymbol
                            )
                        )
                    );
                }
                currentClassSymbol = currentClassSymbol.BaseType;
            }

            return properties.ToImmutable();
        }

        private static FvpPropertyModel AssemblePropertyModel(
            IPropertySymbol propertySymbol,
            TypeIdentity typeIdentity,
            CancellationToken ct,
            bool isInherited
        )
        {
            var propSyntax =
                propertySymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(ct)
                as PropertyDeclarationSyntax;

            var initializerSyntaxString =
                propSyntax != null ? RoslynUtils.GetInitializer(propSyntax) : null;

            var isNew = propSyntax?.Modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword)) == true;

            return new FvpPropertyModel(
                Name: propertySymbol.Name,
                Type: typeIdentity,
                Initializer: new Initializer(initializerSyntaxString ?? string.Empty),
                IsOverride: propertySymbol.IsOverride,
                IsNew: isNew,
                IsInherited: isInherited,
                DeclaringTypeName: propertySymbol.ContainingType.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                )
            );
        }
    }
}
