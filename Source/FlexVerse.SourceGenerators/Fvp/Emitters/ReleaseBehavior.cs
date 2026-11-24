// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Represents the ReleaseBehavior.</summary>
    public sealed class ReleaseBehavior : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            if (model.HasFvpBase)
                return;
            var access = model.IsSealed ? "private" : "protected";
            cb.Line("#region ReleaseBehavior");
            cb.Line($"{access} void ReleaseBehavior(Behavior behavior)");
            cb.Block(() =>
            {
                cb.Line("if (behavior is null) return;");
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-FLEXVERSE-PROPERTY\", \"behavior.released\", (\"behavior\", behavior));"
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
