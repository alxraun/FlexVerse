// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the ApplyPropsFromDefinition.</summary>
    public sealed class ApplyPropsFromDefinition : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            cb.Line("#region ApplyPropsFromDefinition");
            cb.Line(
                $"public {model.LifecycleMethodModifier} void ApplyPropsFromDefinition(Widget definition)"
            );
            cb.Block(() =>
            {
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-WIDGET\", \"props.applied\", (\"definition\", definition));"
                );
                if (!model.IsBase)
                {
                    cb.Line("base.ApplyPropsFromDefinition(definition);");
                }

                cb.Line($"if (definition is {model.DeclarationName} def)");
                cb.Block(() =>
                {
                    foreach (
                        var prop in model.GetProperties(PropertyFilter.Fvp | PropertyFilter.Prop)
                    )
                    {
                        string camelCase = StringUtils.ToCamel(prop.Name);
                        cb.Line(
                            $"if (def.{prop.Name} is {{ }} {camelCase}Behavior) this.{prop.Name} = {camelCase}Behavior;"
                        );
                    }
                });
                cb.Line();
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
