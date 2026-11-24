// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Represents the ReleaseFvp.</summary>
    public sealed class ReleaseFvp : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            if (model.HasFvpBase)
                return;
            var access = model.IsSealed ? "private" : "protected";
            cb.Line("#region ReleaseFvp");
            cb.Line($"{access} bool _fvpReleased;");
            cb.Line(
                model.IsSealed ? "public void ReleaseFvp()" : "public virtual void ReleaseFvp()"
            );
            cb.Block(() =>
            {
                cb.Line("if (_fvpReleased) return;");
                cb.Line("_fvpReleased = true;");
                cb.Line("FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscriptions(this);");
                cb.Line("this.ReleaseBehaviors();");
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
