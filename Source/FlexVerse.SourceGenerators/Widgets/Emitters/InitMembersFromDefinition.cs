// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the InitMembersFromDefinition.</summary>
    public sealed class InitMembersFromDefinition : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            cb.Line("#region InitMembersFromDefinition");
            cb.Line(
                $"public {model.LifecycleMethodModifier} void InitMembersFromDefinition(Widget definition)"
            );
            cb.Block(() =>
            {
                cb.Line("bool isEntryPoint = !this._isInitializing;");
                cb.Line("if (isEntryPoint) this._isInitializing = true;");
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-WIDGET\", \"members.initialized\", (\"definition\", definition), (\"widget\", this));"
                );
                cb.Line("try");
                cb.Block(() =>
                {
                    if (!model.IsBase)
                    {
                        cb.Line("base.InitMembersFromDefinition(definition);");
                    }

                    cb.Line($"if (definition is {model.DeclarationName} def)");
                    cb.Block(() =>
                    {
                        foreach (
                            var prop in model.GetProperties(
                                PropertyFilter.Fvp
                                    | PropertyFilter.Prop
                                    | PropertyFilter.State
                                    | PropertyFilter.IcProvider
                            )
                        )
                        {
                            if (prop.IsOverride)
                                continue;
                            string defaultInitializer = GetDefaultInitializer(prop);
                            cb.Line($"this.{prop.Name} = def.{prop.Name} ?? {defaultInitializer};");
                        }
                    });
                    cb.Line();
                });
                cb.Line();
                cb.Line("finally");
                cb.Block(() =>
                {
                    cb.Line("if (isEntryPoint) this._isInitializing = false;");
                });
                cb.Line();
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }

        private static string GetDefaultInitializer(PropertyModel prop)
        {
            if (prop.Type.IsGeneric)
            {
                if (prop.Type.IsAbstract)
                    return $"new FlexVerse.StaticBehavior<{prop.Type.ArgName}>()";
                else
                    return $"new {prop.Type.ConstructedName}()";
            }
            else
            {
                if (prop.Type.IsAbstract)
                    return "FlexVerse.NullBehavior.Instance";
                else
                    return $"new {prop.Type.ConstructedName}()";
            }
        }
    }
}
