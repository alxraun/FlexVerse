// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the ReleaseBehaviors.</summary>
    public sealed class ReleaseBehaviors : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            cb.Line("#region ReleaseBehaviors");
            cb.Line($"public {model.LifecycleMethodModifier} void ReleaseBehaviors()");
            cb.Block(() =>
            {
                if (!model.IsBase)
                {
                    cb.Line("base.ReleaseBehaviors();");
                }

                foreach (
                    var prop in model.GetProperties(
                        PropertyFilter.Fvp | PropertyFilter.Prop | PropertyFilter.State
                    )
                )
                {
                    if (prop.IsOverride)
                        continue;

                    cb.Line($"var old{prop.Name} = this.{prop.FieldName};");
                    cb.Line($"if (old{prop.Name} is not null)");
                    cb.Block(() =>
                    {
                        cb.Line($"this.{prop.FieldName}Changing?.Invoke();");
                        cb.Line($"this.{prop.FieldName} = null;");
                        cb.Line($"this.{prop.FieldName}Changed?.Invoke();");
                        cb.Line($"this.ReleaseBehavior(old{prop.Name});");
                    });
                }
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
