// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Represents the ReleaseBehaviors.</summary>
    public sealed class ReleaseBehaviors : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            cb.Line("#region ReleaseBehaviors");
            cb.Line(
                $"{model.LifecycleAccess} {model.LifecycleMethodModifier} void ReleaseBehaviors()"
            );
            cb.Block(() =>
            {
                if (model.HasFvpBase)
                {
                    cb.Line("base.ReleaseBehaviors();");
                }
                cb.Line("if (!_fvpInitialized) return;");
                foreach (var prop in model.OwnProperties)
                {
                    cb.Line($"var old{prop.Name} = {prop.FieldName};");
                    cb.Line($"if (old{prop.Name} is not null)");
                    cb.Block(() =>
                    {
                        cb.Line($"{prop.FieldName}Changing?.Invoke();");
                        cb.Line($"{prop.FieldName} = null;");
                        cb.Line($"{prop.FieldName}Changed?.Invoke();");
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
