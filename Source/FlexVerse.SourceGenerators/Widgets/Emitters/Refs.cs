// parents: @.frostline/graph/m-widget-emitter.xml

using System.Linq;
using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the Refs.</summary>
    public sealed class Refs : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            var allProps = model.GetProperties(
                PropertyFilter.Fvp
                    | PropertyFilter.Prop
                    | PropertyFilter.State
                    | PropertyFilter.InheritedContext,
                inherited: true
            );
            var ownProps = model.GetProperties(
                PropertyFilter.Fvp
                    | PropertyFilter.Prop
                    | PropertyFilter.State
                    | PropertyFilter.InheritedContext
            );

            if (!allProps.Any())
                return;

            var modifier = model.IsBase ? "" : "new ";
            cb.Line("#region Refs");
            cb.Line(
                $"public {modifier}{model.ClassName}Refs Refs => new {model.ClassName}Refs(this);"
            );
            cb.Line();

            cb.Line($"public readonly ref struct {model.ClassName}Refs({model.DeclarationName} o)");
            cb.Block(() =>
            {
                foreach (var prop in allProps)
                    cb.Line($"public {prop.Name}Ref {prop.Name} => new(o);");
                EmitStringIndexer(cb, allProps);
            });
            cb.Line();
            cb.Line();

            foreach (var prop in ownProps)
                EmitPropertyRefStruct(cb, model, prop);

            cb.Line("#endregion");
            cb.Line();
        }

        private static void EmitStringIndexer(
            CodeBuilder cb,
            System.Collections.Generic.IEnumerable<PropertyModel> allProps
        )
        {
            var props = allProps.ToList();
            if (props.Count == 0)
                return;

            cb.Line("public IPropertyRef this[string key]");
            cb.Block(() =>
            {
                cb.Line("get");
                cb.Block(() =>
                {
                    cb.Line("switch (key)");
                    cb.Block(() =>
                    {
                        foreach (var prop in props)
                            cb.Line($"case \"{prop.Name}\": return new {prop.Name}Ref(o);");
                        cb.Line("default: return null;");
                    });
                });
            });
            cb.Line();
        }

        private void EmitPropertyRefStruct(CodeBuilder cb, WidgetModel model, PropertyModel prop)
        {
            var name = prop.Name;
            var typeName = prop.TypeName;
            var typeArgument = prop.Type.ArgName;
            var typed = prop.Type.IsGeneric;
            var ownerType = model.DeclarationName;
            var fieldName = prop.FieldName;
            var valueExpression =
                prop.IsOverride || prop.IsInherited ? $"o.{prop.Name}" : $"o.{prop.FieldName}";
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
