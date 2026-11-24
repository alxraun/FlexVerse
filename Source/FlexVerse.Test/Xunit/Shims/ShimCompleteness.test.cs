using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Xunit;

namespace FlexVerse.Test.Shims
{
    /// <summary>
    /// Shim completeness gate: FlexVerse.dll was compiled against the Krafs
    /// reference assemblies, so every external memberref it carries binds to
    /// a *real* Unity/Verse owner (e.g. Color::op_Implicit, not
    /// Vector4::op_Implicit). This test enumerates those memberrefs via
    /// System.Reflection.Metadata and requires each one to resolve against
    /// the runtime shim assemblies, so shim drift surfaces as one explicit
    /// failure here instead of a random MissingMethodException inside an
    /// unrelated runtime test.
    ///
    /// Scope rule is type-level: a declaring type that is not shimmed at all
    /// (Verse.Widgets, Verse.Text, ...) is skipped — those paths only run
    /// in-game. A type that IS shimmed must fully cover its referenced
    /// members by name + parameter count.
    /// </summary>
    public class ShimCompletenessTests
    {
        private static readonly string[] ShimmedAssemblies =
        {
            "UnityEngine.CoreModule",
            "UnityEngine.IMGUIModule",
            "Assembly-CSharp",
        };

        [Fact]
        public void FlexVerseMemberRefs_AllResolveAgainstRuntimeShims()
        {
            var productPath = TestPaths.FlexVerseDll;
            Assert.True(File.Exists(productPath), $"FlexVerse.dll not found at {productPath}");

            var (shimTypes, shimMembers) = LoadShimSurface();
            var missing = new SortedSet<string>();
            int checkedMemberRefs = 0;
            var skippedTypes = new SortedSet<string>();

            using var productPe = new PEReader(File.OpenRead(productPath));
            var productReader = productPe.GetMetadataReader();
            var provider = new MemberNameTypeProvider();

            foreach (var handle in productReader.MemberReferences)
            {
                var memberRef = productReader.GetMemberReference(handle);
                if (memberRef.Parent.Kind != HandleKind.TypeReference)
                {
                    continue;
                }

                var typeRef = productReader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                var assemblyName = ResolveOwningAssemblyName(productReader, typeRef);
                if (assemblyName == null || !ShimmedAssemblies.Contains(assemblyName))
                {
                    continue;
                }

                var typeName = TypeFullName(productReader, typeRef);
                if (!shimTypes.Contains(typeName))
                {
                    skippedTypes.Add(typeName);
                    continue;
                }

                var memberName = productReader.GetString(memberRef.Name);
                string key;
                var signatureReader = productReader.GetBlobReader(memberRef.Signature);
                if (
                    memberName == ".ctor"
                    || memberName == ".cctor"
                    || signatureReader.ReadByte() != 0x06
                )
                {
                    var signature = memberRef.DecodeMethodSignature(provider, null);
                    key = $"{typeName}:{memberName}/{signature.ParameterTypes.Length}";
                }
                else
                {
                    key = $"{typeName}:{memberName}";
                }

                checkedMemberRefs++;
                if (!shimMembers.Contains(key))
                {
                    missing.Add($"{assemblyName}!{typeName}.{memberName} (expected key {key})");
                }
            }

            Assert.True(
                missing.Count == 0,
                "Shim drift detected: FlexVerse.dll references members the runtime "
                    + "shims do not provide. Add the missing members to the shims "
                    + "(owners must match real UnityEngine/Verse exactly).\nMissing:\n"
                    + string.Join("\n", missing)
                    + $"\n(checked {checkedMemberRefs} memberrefs; "
                    + $"{skippedTypes.Count} unshimmed types skipped: "
                    + string.Join(", ", skippedTypes)
                    + ")"
            );
        }

        private static (HashSet<string> Types, HashSet<string> Members) LoadShimSurface()
        {
            var types = new HashSet<string>();
            var members = new HashSet<string>();
            var provider = new MemberNameTypeProvider();

            foreach (var assemblyName in ShimmedAssemblies)
            {
                var path = Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll");
                Assert.True(File.Exists(path), $"runtime shim not found: {path}");

                using var pe = new PEReader(File.OpenRead(path));
                var reader = pe.GetMetadataReader();

                var fullNames = new Dictionary<TypeDefinitionHandle, string>();
                foreach (var handle in reader.TypeDefinitions)
                {
                    var td = reader.GetTypeDefinition(handle);
                    var name = reader.GetString(td.Name);
                    var ns = reader.GetString(td.Namespace);
                    var fullName = ns.Length > 0 ? ns + "." + name : name;
                    if (td.IsNested)
                    {
                        var outer = fullNames.TryGetValue(td.GetDeclaringType(), out var outerName)
                            ? outerName
                            : "?";
                        fullName = outer + "+" + name;
                    }
                    fullNames[handle] = fullName;
                    types.Add(fullName);
                }

                foreach (var handle in reader.TypeDefinitions)
                {
                    var td = reader.GetTypeDefinition(handle);
                    var fullName = fullNames[handle];

                    foreach (var methodHandle in td.GetMethods())
                    {
                        var md = reader.GetMethodDefinition(methodHandle);
                        var name = reader.GetString(md.Name);
                        var paramCount = md.DecodeSignature(provider, null).ParameterTypes.Length;
                        members.Add($"{fullName}:{name}/{paramCount}");
                    }

                    foreach (var fieldHandle in td.GetFields())
                    {
                        var fd = reader.GetFieldDefinition(fieldHandle);
                        members.Add($"{fullName}:{reader.GetString(fd.Name)}");
                    }
                }
            }

            return (types, members);
        }

        private static string? ResolveOwningAssemblyName(
            MetadataReader reader,
            TypeReference typeRef
        )
        {
            var scope = typeRef.ResolutionScope;
            while (scope.Kind == HandleKind.TypeReference)
            {
                scope = reader.GetTypeReference((TypeReferenceHandle)scope).ResolutionScope;
            }

            return scope.Kind == HandleKind.AssemblyReference
                ? reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)scope).Name)
                : null;
        }

        private static string TypeFullName(MetadataReader reader, TypeReference typeRef)
        {
            var name = reader.GetString(typeRef.Name);
            var scope = typeRef.ResolutionScope;
            if (scope.Kind == HandleKind.TypeReference)
            {
                return TypeFullName(reader, reader.GetTypeReference((TypeReferenceHandle)scope))
                    + "+"
                    + name;
            }

            var ns = reader.GetString(typeRef.Namespace);
            return ns.Length > 0 ? ns + "." + name : name;
        }

        /// <summary>Reduces signature types to stable name strings; only the
        /// parameter *count* participates in matching.</summary>
        private sealed class MemberNameTypeProvider : ISignatureTypeProvider<string, object?>
        {
            public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[]";

            public string GetByReferenceType(string elementType) => elementType + "&";

            public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";

            public string GetGenericInstantiation(
                string genericType,
                ImmutableArray<string> typeArguments
            ) => genericType;

            public string GetGenericMethodParameter(object? genericContext, int index) =>
                "!!" + index;

            public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

            public string GetModifiedType(
                string modifier,
                string unmodifiedType,
                bool isRequired
            ) => unmodifiedType;

            public string GetPinnedType(string elementType) => elementType;

            public string GetPointerType(string elementType) => elementType + "*";

            public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();

            public string GetSZArrayType(string elementType) => elementType + "[]";

            public string GetTypeFromDefinition(
                MetadataReader reader,
                TypeDefinitionHandle handle,
                byte rawTypeKind
            )
            {
                var td = reader.GetTypeDefinition(handle);
                var name = reader.GetString(td.Name);
                var ns = reader.GetString(td.Namespace);
                return ns.Length > 0 ? ns + "." + name : name;
            }

            public string GetTypeFromReference(
                MetadataReader reader,
                TypeReferenceHandle handle,
                byte rawTypeKind
            )
            {
                var tr = reader.GetTypeReference(handle);
                var name = reader.GetString(tr.Name);
                if (tr.ResolutionScope.Kind == HandleKind.TypeReference)
                {
                    return GetTypeFromReference(
                            reader,
                            (TypeReferenceHandle)tr.ResolutionScope,
                            rawTypeKind
                        )
                        + "+"
                        + name;
                }

                var ns = reader.GetString(tr.Namespace);
                return ns.Length > 0 ? ns + "." + name : name;
            }

            public string GetTypeFromSpecification(
                MetadataReader reader,
                object? genericContext,
                TypeSpecificationHandle handle,
                byte rawTypeKind
            ) => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        }
    }
}
