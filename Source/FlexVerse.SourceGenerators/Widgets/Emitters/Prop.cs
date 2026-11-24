// parents: @.frostline/graph/m-widget-emitter.xml

using System.Linq;
using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the Prop.</summary>
    public sealed class Prop : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            var props = model.GetProperties(PropertyFilter.Prop);
            if (props.Any())
            {
                cb.Line("#region Prop");
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

                    if (LayoutProperties.AppliesTo(prop))
                        EmitStyleSignals(cb, prop);
                    else
                        EmitPropSignals(cb, prop);

                    cb.Line("this.ActivateBehavior(@new);");
                });
                cb.Line();
                cb.Line($"this.ReleaseBehavior(@old);");
                cb.Line("break;");
            });
        }

        private static void EmitStyleSignals(CodeBuilder cb, PropertyModel prop)
        {
            cb.Line("System.Action action = () =>");
            cb.Block(() =>
                StyleLogic.GenerateStyleUpdate(
                    cb,
                    prop.Name,
                    "@new.Value",
                    "this.Engine.GetNode(this)!"
                )
            );
            cb.Line(";");
            cb.Line("this.Engine.Post(new SignalInfo(this, action));");
            cb.Line("@new.OnUpdate += (_) => this.Engine.Post(new SignalInfo(this, action));");
        }

        private void EmitPropSignals(CodeBuilder cb, PropertyModel prop)
        {
            if (prop.Reactivity.TriggersSet)
            {
                cb.Line("System.Action onSetAction = () =>");
                cb.Block(() => EmitOnSetActionBody(cb, prop));
                cb.Line(";");
                cb.Line("this.Engine.Post(new SignalInfo(this, onSetAction));");
            }

            if (prop.Reactivity.TriggersUpdate)
            {
                cb.Line("System.Action onUpdateAction = () =>");
                cb.Block(() => EmitOnUpdateActionBody(cb, prop));
                cb.Line(";");
                cb.Line(
                    "@new.OnUpdate += (_) => this.Engine.Post(new SignalInfo(this, onUpdateAction));"
                );
            }
        }

        private void EmitOnSetActionBody(CodeBuilder cb, PropertyModel prop)
        {
            if (prop.Reactivity.ReconcileOnSet)
            {
                cb.Line("this.Engine.ReconcileChildren(this);");
            }
            if (prop.Reactivity.MarkDirtyOnSet)
            {
                cb.Line("this.Engine.MarkNodeAsDirty(this);");
            }
        }

        private void EmitOnUpdateActionBody(CodeBuilder cb, PropertyModel prop)
        {
            if (prop.Reactivity.ReconcileOnUpdate)
            {
                cb.Line("this.Engine.ReconcileChildren(this);");
            }
            if (prop.Reactivity.MarkDirtyOnUpdate)
            {
                cb.Line("this.Engine.MarkNodeAsDirty(this);");
            }
        }

        private static string FormatBool(bool value) => value ? "true" : "false";
    }
}
