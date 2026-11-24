// parents: @.frostline/graph/m-source-generators-common.xml

using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexVerse.SourceGenerators
{
    /// <summary>Represents the RoslynUtils.</summary>
    public static class RoslynUtils
    {
        private static MethodInfo? _locMethod;
        private static PropertyInfo? _locDataProp;
        private static bool _refFail = false;

        /// <summary>Performs the Get Attribute operation.</summary>
        public static AttributeData? GetAttribute(ISymbol sym, string attrName)
        {
            if (sym == null)
                return null;

            var attrs = sym.GetAttributes();
            foreach (var attr in attrs)
            {
                if (attr.AttributeClass?.ToDisplayString() == attrName)
                {
                    return attr;
                }
            }
            return null;
        }

        /// <summary>Performs the Get Argument operation.</summary>
        public static bool? GetArgument(AttributeData attr, string argName)
        {
            if (attr.NamedArguments.IsDefaultOrEmpty)
            {
                return null;
            }

            foreach (var kv in attr.NamedArguments)
            {
                if (kv.Key == argName)
                {
                    if (kv.Value.Value is bool b)
                    {
                        return b;
                    }
                    return null;
                }
            }
            return null;
        }

        /// <summary>Performs the Get Initializer operation.</summary>
        public static string? GetInitializer(PropertyDeclarationSyntax prop)
        {
            if (prop.Initializer != null)
            {
                return prop.Initializer.Value.ToString();
            }

            return null;
        }

        /// <summary>Performs the Get Initializer operation.</summary>
        public static string? GetInitializer(VariableDeclaratorSyntax varDecl)
        {
            if (varDecl.Initializer != null)
            {
                return varDecl.Initializer.Value.ToString();
            }

            return null;
        }

        /// <summary>Performs the Get Location operation.</summary>
        public static string? GetLocation(
            SemanticModel model,
            SyntaxNode node,
            CancellationToken ct
        )
        {
            if (_refFail)
                return null;

            try
            {
                if (_locMethod is null)
                {
                    var asm = typeof(CSharpCompilation).Assembly;
                    var type = asm.GetType(
                        "Microsoft.CodeAnalysis.CSharp.Interception.CSharpInterception"
                    );
                    if (type is null)
                    {
                        _refFail = true;
                        return null;
                    }

                    var method = type.GetMethod(
                        "GetInterceptableLocation",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                    );
                    if (method is null)
                    {
                        _refFail = true;
                        return null;
                    }

                    _locMethod = method;

                    var retType = method.ReturnType;
                    var dataType =
                        retType.IsGenericType
                        && retType.GetGenericTypeDefinition() == typeof(System.Nullable<>)
                            ? retType.GetGenericArguments()[0]
                            : retType;

                    var prop = dataType.GetProperty(
                        "Data",
                        BindingFlags.Instance | BindingFlags.Public
                    );
                    if (prop is null)
                    {
                        _refFail = true;
                        return null;
                    }

                    _locDataProp = prop;
                }

                object?[] args = [model, node, ct];
                var res = _locMethod.Invoke(null, args);

                if (res is null)
                    return null;

                return (string?)_locDataProp!.GetValue(res);
            }
            catch
            {
                _refFail = true;
                return null;
            }
        }
    }
}
