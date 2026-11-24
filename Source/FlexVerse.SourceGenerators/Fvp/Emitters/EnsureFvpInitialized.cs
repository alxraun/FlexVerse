// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Represents the EnsureFvpInitialized.</summary>
    public sealed class EnsureFvpInitialized : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            cb.Line("#region EnsureFvpInitialized");
            cb.Line("private bool _fvpInitialized;");
            cb.Line(
                $"{model.LifecycleAccess} {model.LifecycleMethodModifier} void EnsureFvpInitialized()"
            );
            cb.Block(() =>
            {
                cb.Line(
                    "if (_fvpReleased) throw new System.ObjectDisposedException(GetType().FullName);"
                );
                cb.Line("if (_fvpInitialized) return;");
                cb.Line("_fvpInitialized = true;");
                if (model.HasFvpBase)
                {
                    cb.Line("base.EnsureFvpInitialized();");
                }
                foreach (var prop in model.OwnProperties)
                {
                    cb.Line($"this.ActivateBehavior({prop.FieldName});");
                }
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
