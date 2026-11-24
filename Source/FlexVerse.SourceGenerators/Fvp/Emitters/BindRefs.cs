// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Represents the BindRefs.</summary>
    public sealed class BindRefs : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            if (model.Properties.IsEmpty && !model.HasFvpBase)
                return;

            var modifier = model.HasFvpBase ? "new " : "";
            cb.Line("#region BindRefs");
            cb.Line($"public {modifier}BindRefs Bind => new BindRefs(this);");
            cb.Line($"public {modifier}readonly ref struct BindRefs({model.DeclarationName} o)");
            cb.Block(() =>
            {
                foreach (var prop in model.Properties)
                    cb.Line($"public BindRef {prop.Name} => new(new {prop.RefTypeName}(o));");
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
