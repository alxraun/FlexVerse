// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the CloneMembersFromDefinition.</summary>
    public sealed class CloneMembersFromDefinition : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            cb.Line("#region CloneMembersFromDefinition");
            cb.Line(
                $"public {model.LifecycleMethodModifier} void CloneMembersFromDefinition(Widget definition)"
            );
            cb.Block(() =>
            {
                if (!model.IsBase)
                {
                    cb.Line("base.CloneMembersFromDefinition(definition);");
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
                        EmitCloneAssignment(cb, prop);
                    }
                });
                cb.Line();
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }

        private void EmitCloneAssignment(CodeBuilder cb, PropertyModel prop)
        {
            string camelCase = StringUtils.ToCamel(prop.Name);
            cb.Line(
                $"if (def.{prop.Name} is {{ }} {camelCase}Behavior) "
                    + $"this.{prop.Name} = {camelCase}Behavior.CloneUntyped() as {prop.TypeName};"
            );
        }
    }
}
