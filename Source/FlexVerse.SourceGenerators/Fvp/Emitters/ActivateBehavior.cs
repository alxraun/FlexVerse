// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Represents the ActivateBehavior.</summary>
    public sealed class ActivateBehavior : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            if (model.HasFvpBase)
                return;
            var access = model.IsSealed ? "private" : "protected";
            cb.Line("#region ActivateBehavior");
            cb.Line($"{access} void ActivateBehavior(Behavior behavior)");
            cb.Block(() =>
            {
                cb.Line("if (behavior is null) return;");
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-FLEXVERSE-PROPERTY\", \"behavior.activated\", (\"behavior\", behavior));"
                );
                cb.Line("FlexVerseRoot.Instance.BehaviorRegistry.Increment(behavior);");
                cb.Line("if (behavior.State == Behavior.Lifecycle.Inactive)");
                cb.Block(() =>
                {
                    cb.Line(
                        "behavior.OnStart += () => FlexVerseRoot.Instance.BehaviorUpdater.Subscribe(behavior);"
                    );
                    cb.Line(
                        "behavior.OnComplete += () => FlexVerseRoot.Instance.BehaviorUpdater.Unsubscribe(behavior);"
                    );
                    cb.Line("behavior.Activate();");
                });
                cb.Line();
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
