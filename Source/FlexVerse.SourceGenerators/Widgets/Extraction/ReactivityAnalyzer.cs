// parents: @.frostline/graph/m-reactivity-analyzer.xml

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexVerse.SourceGenerators.Widgets.Extraction
{
    /// <summary>Represents the ReactivityAnalyzer.</summary>
    public static class ReactivityAnalyzer
    {
        /// <summary>Represents the Defaults struct.</summary>
        public readonly record struct Defaults(
            bool ReconcileOnSet,
            bool ReconcileOnUpdate,
            bool MarkDirtyOnSet,
            bool MarkDirtyOnUpdate
        );

        /// <summary>Analyzes the class and infers per-property reactivity defaults.</summary>
        public static Dictionary<string, Defaults> AnalyzeClassDefaults(
            INamedTypeSymbol classSymbol,
            Compilation compilation,
            CancellationToken ct
        )
        {
            var usageMap = Scan(classSymbol, compilation, ct);
            var results = new Dictionary<string, Defaults>();

            foreach (var kvp in usageMap)
            {
                results[kvp.Key] = DecideDefaults(kvp.Value);
            }

            return results;
        }

        /// <summary>Performs the Resolve Reactivity operation.</summary>
        public static Reactivity ResolveReactivity(
            IPropertySymbol propertySymbol,
            PropertyKind kind,
            AttributeData? attribute,
            Dictionary<string, Defaults> defaults
        )
        {
            var propertyDefaults = defaults.TryGetValue(propertySymbol.Name, out var d)
                ? d
                : new Defaults(false, false, false, false);

            if (kind == PropertyKind.Fvp)
            {
                return new Reactivity(false, false, false, false);
            }

            return new Reactivity(
                ReconcileOnSet: GetOverride(attribute, "ReconcileOnSet")
                    ?? propertyDefaults.ReconcileOnSet,
                ReconcileOnUpdate: GetOverride(attribute, "ReconcileOnUpdate")
                    ?? propertyDefaults.ReconcileOnUpdate,
                MarkDirtyOnSet: GetOverride(attribute, "MarkDirtyOnSet")
                    ?? propertyDefaults.MarkDirtyOnSet,
                MarkDirtyOnUpdate: GetOverride(attribute, "MarkDirtyOnUpdate")
                    ?? propertyDefaults.MarkDirtyOnUpdate
            );
        }

        private static Defaults DecideDefaults(UsageFacts facts)
        {
            return new Defaults(
                ReconcileOnSet: facts.IsDependencyOfValue || facts.IsReadInBuildRenderTree,
                ReconcileOnUpdate: facts.IsDependencyOfValue,
                MarkDirtyOnSet: facts.IsReadInMeasureFunc,
                MarkDirtyOnUpdate: facts.IsReadInMeasureFunc
            );
        }

        private static bool? GetOverride(AttributeData? attribute, string name)
        {
            return attribute != null ? RoslynUtils.GetArgument(attribute, name) : null;
        }

        /// <summary>Represents the UsageFacts struct.</summary>
        public record struct UsageFacts(
            bool IsDependencyOfValue,
            bool IsReadInBuildRenderTree,
            bool IsReadInMeasureFunc
        );

        private static Dictionary<string, UsageFacts> Scan(
            INamedTypeSymbol classSymbol,
            Compilation compilation,
            CancellationToken ct
        )
        {
            var results = new Dictionary<string, UsageFacts>();

            var properties = classSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic)
                .ToDictionary(p => p.Name, p => p);

            var methods = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.Name == "BuildRenderTree" && m.DeclaringSyntaxReferences.Length > 0)
                .FirstOrDefault();

            var measure = classSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.Name == "MeasureFunc" && p.DeclaringSyntaxReferences.Length > 0)
                .FirstOrDefault();

            foreach (var propName in properties.Keys)
            {
                results[propName] = new UsageFacts();
            }

            if (methods != null)
            {
                ScanScope(
                    methods,
                    compilation,
                    ct,
                    (node, model) =>
                    {
                        if (ExtractPropertyUsage(node, model, ct, properties) is { } match)
                        {
                            var facts = results[match.Name];
                            facts.IsReadInBuildRenderTree = true;

                            if (
                                node.Parent is MemberAccessExpressionSyntax mae
                                && mae.Name.Identifier.Text == "Value"
                                && SymbolEqualityComparer.Default.Equals(
                                    model.GetSymbolInfo(mae.Expression, ct).Symbol,
                                    match.Symbol
                                )
                            )
                            {
                                facts.IsDependencyOfValue = true;
                            }

                            results[match.Name] = facts;
                        }
                    }
                );
            }

            if (measure != null)
            {
                ScanScope(
                    measure,
                    compilation,
                    ct,
                    (node, model) =>
                    {
                        if (ExtractPropertyUsage(node, model, ct, properties) is { } match)
                        {
                            var facts = results[match.Name];
                            facts.IsReadInMeasureFunc = true;
                            results[match.Name] = facts;
                        }
                    }
                );
            }

            return results;
        }

        private static void ScanScope(
            ISymbol symbol,
            Compilation compilation,
            CancellationToken ct,
            System.Action<SyntaxNode, SemanticModel> analyzeNode
        )
        {
            var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            Follow(symbol);

            void Follow(ISymbol target)
            {
                ct.ThrowIfCancellationRequested();
                if (!visited.Add(target))
                    return;
                foreach (var decl in target.DeclaringSyntaxReferences)
                {
                    var root = decl.GetSyntax(ct);
                    Visit(
                        root,
                        compilation.GetSemanticModel(root.SyntaxTree),
                        SymbolEqualityComparer.Default.Equals(target, symbol)
                            && symbol.Name == "MeasureFunc"
                    );
                }
            }

            void Visit(SyntaxNode node, SemanticModel model, bool measureEntry = false)
            {
                ct.ThrowIfCancellationRequested();
                if (node is AnonymousFunctionExpressionSyntax && !measureEntry)
                    return;
                if (
                    node is LocalFunctionStatementSyntax
                    && node != model.SyntaxTree.GetRoot(ct)
                    && !visited.Contains(model.GetDeclaredSymbol(node, ct)!)
                )
                    return;

                analyzeNode(node, model);
                ISymbol? target = null;
                ExpressionSyntax? receiver = null;
                if (node is InvocationExpressionSyntax call)
                {
                    target = model.GetSymbolInfo(call, ct).Symbol;
                    receiver = (call.Expression as MemberAccessExpressionSyntax)?.Expression;
                }
                else if (
                    node is IdentifierNameSyntax id
                    && model.GetSymbolInfo(id, ct).Symbol is IPropertySymbol property
                )
                {
                    target = property.GetMethod;
                    receiver = (id.Parent as MemberAccessExpressionSyntax)?.Expression;
                }
                if (
                    target is IMethodSymbol method
                    && !method.IsStatic
                    && !method.IsVirtual
                    && !method.IsOverride
                    && !method.IsAbstract
                    && (receiver == null || receiver is ThisExpressionSyntax)
                    && (
                        method.MethodKind == MethodKind.LocalFunction
                        || SymbolEqualityComparer.Default.Equals(
                            method.ContainingType,
                            symbol.ContainingType
                        )
                    )
                )
                    Follow(method);

                foreach (var child in node.ChildNodes())
                    Visit(
                        child,
                        model,
                        measureEntry && node is not AnonymousFunctionExpressionSyntax
                    );
            }
        }

        private static (string Name, IPropertySymbol Symbol)? ExtractPropertyUsage(
            SyntaxNode node,
            SemanticModel model,
            CancellationToken ct,
            Dictionary<string, IPropertySymbol> properties
        )
        {
            if (node is IdentifierNameSyntax id)
            {
                var sym = model.GetSymbolInfo(id, ct).Symbol;
                if (
                    sym is IPropertySymbol p
                    && properties.TryGetValue(p.Name, out var match)
                    && SymbolEqualityComparer.Default.Equals(p, match)
                    && (
                        id.Parent is not MemberAccessExpressionSyntax access
                        || access.Expression == id
                        || access.Expression is ThisExpressionSyntax
                    )
                )
                {
                    return (p.Name, match);
                }
            }
            return null;
        }
    }
}
