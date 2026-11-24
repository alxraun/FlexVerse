// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the ReleaseBehavior.</summary>
    public sealed class ReleaseBehavior : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            if (!model.IsBase)
            {
                return;
            }

            cb.Line("#region ReleaseBehavior");
            cb.Line("public void ReleaseBehavior(Behavior behavior)");
            cb.Block(() =>
            {
                cb.Line("if (behavior is null) return;");
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-WIDGET\", \"behavior.released\", (\"behavior\", behavior));"
                );
                cb.Line(
                    "if (FlexVerseRoot.Instance.BehaviorRegistry.Decrement(behavior)) behavior.Reset();"
                );
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
