// parents: @.frostline/graph/m-widget-emitter.xml

using System.Linq;
using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the Fvp.</summary>
    public sealed class Fvp : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            var props = model.GetProperties(PropertyFilter.Fvp);
            if (props.Any())
            {
                cb.Line("#region FVP");
                foreach (var prop in props)
                {
                    EmitProperty(cb, prop);
                    cb.Line();
                }
                cb.Line("#endregion");
                cb.Line();
            }
        }

        private void EmitProperty(CodeBuilder cb, PropertyModel prop)
        {
            if (prop.IsOverride)
            {
                return;
            }

            var modifier = prop.IsNew ? "new " : "";

            cb.Line($"protected {modifier}Action {prop.FieldName}Changing;");
            cb.Line($"protected {modifier}Action {prop.FieldName}Changed;");
            cb.Line($"protected {modifier}{prop.TypeName} {prop.FieldName}{prop.Assignment};");
            cb.Line($"protected {modifier}{prop.TypeName} {prop.LogicName}");
            cb.Block(() =>
            {
                cb.Line("get");
                cb.Block(() => EmitGetter(cb, prop));
                cb.Line();
                cb.Line("set");
                cb.Block(() => EmitSetter(cb, prop));
                cb.Line();
            });
            cb.Line();
        }

        private void EmitGetter(CodeBuilder cb, PropertyModel prop)
        {
            cb.Line($"return {prop.FieldName};");
        }

        private void EmitSetter(CodeBuilder cb, PropertyModel prop)
        {
            cb.Line("var @new = value;");
            cb.Line("switch (this.LifecycleState)");
            cb.Block(() =>
            {
                EmitDefinitionCase(cb, prop);
                EmitInstanceCase(cb, prop);
            });
        }

        private void EmitDefinitionCase(CodeBuilder cb, PropertyModel prop)
        {
            cb.Line("case Widget.Lifecycle.Definition:");
            cb.Indent(() =>
            {
                cb.Line($"{prop.FieldName} = @new;");
                cb.Line("break;");
            });
        }

        private void EmitInstanceCase(CodeBuilder cb, PropertyModel prop)
        {
            cb.Line("case Widget.Lifecycle.Instance:");
            cb.Indent(() =>
            {
                cb.Line($"var @old = {prop.FieldName};");
                cb.Line(
                    "if (!this._isInitializing && @old is not null && @old.SemEquals(@new) == true) return;"
                );

                cb.Line(
                    $"FlexVerse.Log.Debug(\"M-WIDGET\", \"prop.changed\", (\"property\", \"{prop.Name}\"), (\"new\", @new), (\"old\", @old));"
                );

                cb.Line($"{prop.FieldName}Changing?.Invoke();");
                cb.Line($"{prop.FieldName} = @new;");
                cb.Line($"{prop.FieldName}Changed?.Invoke();");
                cb.Line("if (@new is not null)");
                cb.Block(() =>
                {
                    cb.Line("if (@old is not null) @new.UnionWith(@old);");

                    cb.Line("this.ActivateBehavior(@new);");
                });
                cb.Line();
                cb.Line($"this.ReleaseBehavior(@old);");
                cb.Line("break;");
            });
        }
    }
}
