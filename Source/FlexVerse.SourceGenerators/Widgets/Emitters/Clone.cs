// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the Clone.</summary>
    public sealed class Clone : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            cb.Line("#region Clone");
            cb.Line($"public {model.LifecycleMethodModifier} Widget Clone()");
            cb.Block(() =>
            {
                if (model.IsBase)
                {
                    cb.Line(
                        "throw new System.InvalidOperationException(\"Cannot clone an abstract widget directly.\");"
                    );
                }
                else if (model.IsAbstract)
                {
                    cb.Line(
                        "throw new System.InvalidOperationException(\"Cannot clone an abstract widget directly.\");"
                    );
                }
                else
                {
                    cb.Line($"var clone = new {model.DeclarationName}();");
                    cb.Line("clone.CloneMembersFromDefinition(this);");
                    cb.Line("return clone;");
                }
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
