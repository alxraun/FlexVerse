// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Represents the WatchRefs.</summary>
    public sealed class WatchRefs : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            if (model.Properties.IsEmpty && !model.HasFvpBase)
                return;

            var modifier = model.HasFvpBase ? "new " : "";
            cb.Line("#region WatchRefs");
            cb.Line($"public {modifier}WatchRefs Watch => new WatchRefs(this);");
            cb.Line($"public {modifier}readonly ref struct WatchRefs({model.DeclarationName} o)");
            cb.Block(() =>
            {
                foreach (var prop in model.Properties)
                    cb.Line($"public WatchRef {prop.Name} => new(new {prop.RefTypeName}(o));");
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
