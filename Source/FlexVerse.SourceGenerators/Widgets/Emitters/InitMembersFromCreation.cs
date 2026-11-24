// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the InitMembersFromCreation.</summary>
    public sealed class InitMembersFromCreation : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            cb.Line("#region InitMembersFromCreation");
            cb.Line($"public {model.LifecycleMethodModifier} void InitMembersFromCreation()");
            cb.Block(() =>
            {
                if (!model.IsBase)
                {
                    cb.Line("base.InitMembersFromCreation();");
                }

                cb.Line("var registry = FlexVerseRoot.Instance.BehaviorRegistry;");
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-WIDGET\", \"members.initialized\", (\"widget\", this));"
                );

                foreach (
                    var prop in model.GetProperties(
                        PropertyFilter.Fvp
                            | PropertyFilter.Prop
                            | PropertyFilter.State
                            | PropertyFilter.IcProvider
                    )
                )
                {
                    cb.Line(
                        $"if (this.{prop.FieldName} is not null) registry.Increment(this.{prop.FieldName});"
                    );
                }
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
