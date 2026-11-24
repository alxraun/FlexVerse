// parents: @.frostline/graph/m-widget-emitter.xml

using System.Linq;
using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the BindRefs.</summary>
    public sealed class BindRefs : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            var props = model.GetProperties(
                PropertyFilter.Fvp
                    | PropertyFilter.Prop
                    | PropertyFilter.State
                    | PropertyFilter.InheritedContext,
                inherited: true
            );
            if (!props.Any())
                return;

            var modifier = model.IsBase ? "" : "new ";
            cb.Line("#region BindRefs");
            cb.Line($"public {modifier}BindRefs Bind => new BindRefs(this);");
            cb.Line();
            cb.Line($"public {modifier}readonly ref struct BindRefs({model.DeclarationName} o)");
            cb.Block(() =>
            {
                foreach (var prop in props)
                    cb.Line($"public BindRef {prop.Name} => new(new {prop.Name}Ref(o));");
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
