// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the ActivateBehavior.</summary>
    public sealed class ActivateBehavior : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            if (!model.IsBase)
            {
                return;
            }

            cb.Line("#region ActivateBehavior");
            cb.Line("public void ActivateBehavior(Behavior behavior)");
            cb.Block(() =>
            {
                cb.Line("if (this.LifecycleState != Widget.Lifecycle.Instance) return;");
                cb.Line("if (behavior is null) return;");
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-WIDGET\", \"behavior.activated\", (\"behavior\", behavior));"
                );
                cb.Line("FlexVerseRoot.Instance.BehaviorRegistry.Increment(behavior);");
                cb.Line("switch (behavior.State)");
                cb.Block(() =>
                {
                    cb.Line("case FlexVerse.Behavior.Lifecycle.Inactive:");
                    cb.Indent(() =>
                    {
                        cb.Line(
                            "behavior.OnStart += () => FlexVerseRoot.Instance.BehaviorUpdater.Subscribe(behavior);"
                        );
                        cb.Line(
                            "behavior.OnComplete += () => FlexVerseRoot.Instance.BehaviorUpdater.Unsubscribe(behavior);"
                        );
                        cb.Line("behavior.Activate();");
                        cb.Line("break;");
                    });
                    cb.Line("case FlexVerse.Behavior.Lifecycle.Active:");
                    cb.Indent(() =>
                    {
                        cb.Line("break;");
                    });
                });
                cb.Line();
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
