// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Represents the Refs.</summary>
    public sealed class Refs : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            if (model.Properties.IsEmpty && !model.HasFvpBase)
                return;

            var ownerType = model.DeclarationName;
            var modifier = model.HasFvpBase ? "new " : "";
            cb.Line("#region Refs");
            cb.Line(
                $"public {modifier}{model.ClassName}Refs Refs => new {model.ClassName}Refs(this);"
            );
            cb.Line($"public readonly ref struct {model.ClassName}Refs({ownerType} o)");
            cb.Block(() =>
            {
                foreach (var prop in model.Properties)
                    cb.Line($"public {prop.RefTypeName} {prop.Name} => new(o);");

                cb.Line("public IPropertyRef this[string key]");
                cb.Block(() =>
                {
                    cb.Line("get");
                    cb.Block(() =>
                    {
                        cb.Line("switch (key)");
                        cb.Block(() =>
                        {
                            foreach (var prop in model.Properties)
                                cb.Line($"case \"{prop.Name}\": return new {prop.RefTypeName}(o);");
                            cb.Line("default: return null;");
                        });
                    });
                });
            });
            cb.Line();

            foreach (var prop in model.Properties)
            {
                if (prop.IsInherited)
                    continue;

                EmitPropertyRefStruct(cb, model, prop);
            }
            cb.Line("#endregion");
            cb.Line();
        }

        private static void EmitPropertyRefStruct(
            CodeBuilder cb,
            FvpModel model,
            FvpPropertyModel prop
        )
        {
            var name = prop.Name;
            var typeName = prop.TypeName;
            var typed = prop.Type.IsGeneric;
            var typeArgument = prop.Type.ValueTypeName;
            var ownerType = model.DeclarationName;
            var fieldName = prop.FieldName;
            var valueExpression = prop.IsOverride ? $"o.{prop.Name}" : $"o.{prop.LogicName}";
            var isNew = prop.IsNew || prop.IsOverride;
            var modifier = isNew ? "new " : "";
            var iface = typed ? $"IPropertyRef<{typeArgument}>" : "IPropertyRef";
            cb.Line($"public {modifier}readonly struct {name}Ref({ownerType} o) : {iface}");
            cb.Block(() =>
            {
                cb.Line($"public string Name => \"{name}\";");
                cb.Line("public object Owner => o;");
                cb.Line(
                    $"public event System.Action Changing {{ add => o.{fieldName}Changing += value; remove => o.{fieldName}Changing -= value; }}"
                );
                cb.Line(
                    $"public event System.Action Changed {{ add => o.{fieldName}Changed += value; remove => o.{fieldName}Changed -= value; }}"
                );
                cb.Line($"public {typeName} Get() => {valueExpression};");
                cb.Line($"public void Set({typeName} value) => o.{name} = value;");
                cb.Line("Behavior IPropertyRef.Get() => Get();");
                cb.Line($"void IPropertyRef.Set(Behavior value) => Set(({typeName})value);");
                if (typed)
                {
                    cb.Line(
                        $"Behavior<{typeArgument}> IPropertyRef<{typeArgument}>.Get() => Get();"
                    );
                    cb.Line(
                        $"void IPropertyRef<{typeArgument}>.Set(Behavior<{typeArgument}> value) => Set(({typeName})value);"
                    );
                }
            });
            cb.Line();
        }
    }
}
