// parents: @.frostline/graph/m-widget-emitter.xml

using System.Linq;
using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the WatchRefs.</summary>
    public sealed class WatchRefs : IWidgetMemberEmitter
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
            cb.Line("#region WatchRefs");
            cb.Line($"public {modifier}WatchRefs Watch => new WatchRefs(this);");
            cb.Line();
            cb.Line($"public {modifier}readonly ref struct WatchRefs({model.DeclarationName} o)");
            cb.Block(() =>
            {
                foreach (var prop in props)
                    cb.Line($"public WatchRef {prop.Name} => new(new {prop.Name}Ref(o));");
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
