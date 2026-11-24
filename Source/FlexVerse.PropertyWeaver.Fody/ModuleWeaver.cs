// parents: @.frostline/graph/m-property-weaver.xml

using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace FlexVerse.PropertyWeaver.Fody
{
    /// <summary>Represents the ModuleWeaver.</summary>
    public class ModuleWeaver : BaseModuleWeaver
    {
        /// <summary>Performs the Execute operation.</summary>
        public override void Execute()
        {
            WriteInfo("FlexVerse.PropertyWeaver: Starting IL weaving process.");

            var context = new WeavingContext(this);
            if (!context.TryResolveReferences())
            {
                WriteError(
                    "Initialization failed due to missing critical types. Aborting weaving."
                );
                return;
            }

            var processor = new WeavingProcessor(this, context);
            processor.WeaveTargetProperties();

            WriteInfo("FlexVerse.PropertyWeaver: Finished IL weaving process.");
        }

        /// <summary>Performs the Get Assemblies For Scanning operation.</summary>
        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "mscorlib";
            yield return "System";
            yield return "System.Runtime";
            yield return "netstandard";
        }

        /// <summary>Gets a value indicating whether the reference should be cleaned.</summary>
        public override bool ShouldCleanReference => true;

        private sealed class WeavingContext(ModuleWeaver weaver)
        {
            private readonly ModuleWeaver _weaver = weaver;

            /// <summary>Gets or sets the widget.</summary>
            public TypeReference? Widget { get; private set; }

            /// <summary>Gets or sets the behavior generic.</summary>
            public TypeReference? BehaviorGeneric { get; private set; }

            /// <summary>Gets or sets the behavior.</summary>
            public TypeReference? Behavior { get; private set; }

            /// <summary>Gets or sets the prop attribute.</summary>
            public TypeReference? PropAttribute { get; private set; }

            /// <summary>Gets or sets the state attribute.</summary>
            public TypeReference? StateAttribute { get; private set; }

            /// <summary>Gets or sets the fvp attribute.</summary>
            public TypeReference? FvpAttribute { get; private set; }

            /// <summary>Gets or sets the inherited context attribute.</summary>
            public TypeReference? InheritedContextAttribute { get; private set; }

            /// <summary>Gets or sets the compiler generated attribute.</summary>
            public TypeReference? CompilerGeneratedAttribute { get; private set; }

            /// <summary>Performs the Try Resolve References operation.</summary>
            public bool TryResolveReferences()
            {
                if (!TryImport("FlexVerse.Widget", out var widget))
                    return false;
                Widget = widget;

                if (!TryImport("FlexVerse.Behavior`1", out var behaviorGeneric))
                    return false;
                BehaviorGeneric = behaviorGeneric;

                TryImport("FlexVerse.Behavior", out var behavior, isOptional: true);
                Behavior = behavior;

                if (!TryImport("FlexVerse.PropAttribute", out var propAttr))
                    return false;
                PropAttribute = propAttr;

                TryImport("FlexVerse.StateAttribute", out var stateAttr, isOptional: true);
                StateAttribute = stateAttr;

                TryImport(
                    "FlexVerse.InheritedContextAttribute",
                    out var inheritedContextAttr,
                    isOptional: true
                );
                InheritedContextAttribute = inheritedContextAttr;

                TryImport("FlexVerse.FVPAttribute", out var fvpAttr, isOptional: true);
                FvpAttribute = fvpAttr;

                CompilerGeneratedAttribute = _weaver.ModuleDefinition.ImportReference(
                    typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)
                );

                return true;
            }

            private bool TryImport(
                string typeName,
                out TypeReference? typeRef,
                bool isOptional = false
            )
            {
                typeRef = null;
                var typeDef = FindTypeDefinition(typeName);

                if (typeDef == null)
                {
                    if (!isOptional)
                    {
                        _weaver.WriteError($"Could not find required type '{typeName}'.");
                    }
                    return false;
                }

                typeRef = _weaver.ModuleDefinition.ImportReference(typeDef);
                return true;
            }

            private TypeDefinition? FindTypeDefinition(string fullName)
            {
                var type = _weaver.ModuleDefinition.GetType(fullName);
                if (type != null)
                    return type;

                var assemblyResolver = _weaver.ModuleDefinition.AssemblyResolver;
                var flexVerseReference = _weaver.ModuleDefinition.AssemblyReferences.FirstOrDefault(
                    a => a.Name == "FlexVerse"
                );

                if (flexVerseReference != null)
                {
                    try
                    {
                        var flexVerseAssembly = assemblyResolver.Resolve(flexVerseReference);
                        return flexVerseAssembly?.MainModule.GetType(fullName);
                    }
                    catch (AssemblyResolutionException ex)
                    {
                        _weaver.WriteError(
                            $"Failed to resolve FlexVerse assembly when searching for '{fullName}': {ex.Message}"
                        );
                    }
                }

                return null;
            }
        }

        private sealed class WeavingProcessor(
            ModuleWeaver weaver,
            ModuleWeaver.WeavingContext context
        )
        {
            private readonly ModuleWeaver _weaver = weaver;
            private readonly WeavingContext _context = context;

            /// <summary>Performs the Weave Target Properties operation.</summary>
            public void WeaveTargetProperties()
            {
                foreach (var type in _weaver.ModuleDefinition.Types.ToList())
                {
                    WeaveTypeRecursively(type);
                }
            }

            private void WeaveTypeRecursively(TypeDefinition type)
            {
                if (ShouldProcess(type))
                {
                    _weaver.WriteInfo($"Processing type: {type.FullName}");
                    foreach (var property in type.Properties.ToList())
                    {
                        WeaveProperty(property);
                    }
                }

                foreach (var nestedType in type.NestedTypes)
                {
                    WeaveTypeRecursively(nestedType);
                }
            }

            private bool ShouldProcess(TypeDefinition type)
            {
                if (type.IsInterface || type.IsValueType || !type.HasProperties)
                    return false;
                if (type.FullName == _context.Widget!.FullName)
                    return true;
                if (type.IsAbstract)
                    return false;
                if (IsDerivedFrom(type, _context.Widget))
                    return true;

                return type.Properties.Any(IsTarget);
            }

            private void WeaveProperty(PropertyDefinition property)
            {
                if (!IsTarget(property))
                    return;

                if (!IsAutoProperty(property))
                {
                    _weaver.WriteWarning(
                        $"Property '{property.FullName}' is a target for weaving but is not a standard auto-property. It will be skipped."
                    );
                    return;
                }

                var generatedProperty = FindGeneratedCounterpart(property);
                if (generatedProperty == null)
                {
                    _weaver.WriteError(
                        $"Could not find the source-generated property counterpart for '{property.FullName}'. Make sure the Source Generator ran correctly."
                    );
                    return;
                }

                _weaver.WriteInfo(
                    $"Found target property '{property.Name}' and its generated counterpart '{generatedProperty.Name}' in type '{property.DeclaringType.Name}'."
                );
                RedirectAccessors(property, generatedProperty);
            }

            private void RedirectAccessors(
                PropertyDefinition original,
                PropertyDefinition generated
            )
            {
                if (original.GetMethod != null && generated.GetMethod != null)
                {
                    RedirectMethod(original.GetMethod, generated.GetMethod);
                }

                if (original.SetMethod != null && generated.SetMethod != null)
                {
                    RedirectMethod(original.SetMethod, generated.SetMethod);
                }
            }

            private void RedirectMethod(MethodDefinition original, MethodDefinition target)
            {
                original.Body.Instructions.Clear();
                original.Body.Variables.Clear();
                original.Body.InitLocals = false;

                var il = original.Body.GetILProcessor();
                var targetRef = _weaver.ModuleDefinition.ImportReference(target);

                il.Emit(OpCodes.Ldarg_0);
                if (original.HasParameters)
                {
                    il.Emit(OpCodes.Ldarg_1);
                }
                il.Emit(OpCodes.Call, targetRef);
                il.Emit(OpCodes.Ret);
            }

            private PropertyDefinition? FindGeneratedCounterpart(PropertyDefinition original)
            {
                var generatedName = $"_{original.Name}";
                TypeDefinition? currentType = original.DeclaringType;

                while (currentType != null)
                {
                    var generated = currentType.Properties.FirstOrDefault(p =>
                        p.Name == generatedName
                    );

                    if (generated?.GetMethod != null && generated?.SetMethod != null)
                        return generated;

                    try
                    {
                        var baseType = currentType.BaseType;
                        if (baseType == null)
                            break;

                        currentType = baseType.Resolve();
                    }
                    catch (AssemblyResolutionException)
                    {
                        break;
                    }
                }

                return null;
            }

            private bool IsTarget(PropertyDefinition property)
            {
                if (!IsBehaviorType(property.PropertyType))
                    return false;
                if (!property.HasCustomAttributes)
                    return false;

                return property.CustomAttributes.Any(attr =>
                    IsAttribute(attr, _context.PropAttribute)
                    || IsAttribute(attr, _context.StateAttribute)
                    || IsAttribute(attr, _context.FvpAttribute)
                    || IsAttribute(attr, _context.InheritedContextAttribute)
                );
            }

            private bool IsBehaviorType(TypeReference type)
            {
                if (type is GenericInstanceType git)
                {
                    return git.ElementType.FullName == _context.BehaviorGeneric!.FullName;
                }

                if (_context.Behavior != null && type.FullName == _context.Behavior.FullName)
                {
                    return true;
                }

                try
                {
                    var def = type.Resolve();
                    while (def != null)
                    {
                        if (def.BaseType == null)
                            break;

                        var baseType = def.BaseType;
                        if (baseType.IsGenericInstance)
                        {
                            var baseGit = (GenericInstanceType)baseType;
                            if (baseGit.ElementType.FullName == _context.BehaviorGeneric!.FullName)
                                return true;
                        }
                        else if (
                            _context.Behavior != null
                            && baseType.FullName == _context.Behavior.FullName
                        )
                        {
                            return true;
                        }

                        def = baseType.Resolve();
                    }
                }
                catch { }

                return false;
            }

            private bool IsAttribute(CustomAttribute attr, TypeReference? targetAttr)
            {
                return targetAttr != null && attr.AttributeType.FullName == targetAttr.FullName;
            }

            private bool IsAutoProperty(PropertyDefinition property)
            {
                if (property.GetMethod == null || property.SetMethod == null)
                    return false;

                if (
                    !IsCompilerGenerated(property.GetMethod)
                    || !IsCompilerGenerated(property.SetMethod)
                )
                    return false;

                var backingFieldName = $"<{property.Name}>k__BackingField";
                var backingField = property.DeclaringType.Fields.FirstOrDefault(f =>
                    f.Name == backingFieldName
                );

                return backingField != null
                    && backingField.IsPrivate
                    && IsCompilerGenerated(backingField);
            }

            private bool IsCompilerGenerated(ICustomAttributeProvider provider)
            {
                return provider.CustomAttributes.Any(a =>
                    a.AttributeType.FullName == _context.CompilerGeneratedAttribute!.FullName
                );
            }

            private bool IsDerivedFrom(TypeDefinition type, TypeReference baseTypeRef)
            {
                var currentBase = type.BaseType;
                while (currentBase != null)
                {
                    if (currentBase.FullName == baseTypeRef.FullName)
                        return true;

                    try
                    {
                        var currentBaseDef = currentBase.Resolve();
                        currentBase = currentBaseDef?.BaseType;
                    }
                    catch (AssemblyResolutionException)
                    {
                        return false;
                    }
                }
                return false;
            }
        }
    }
}
