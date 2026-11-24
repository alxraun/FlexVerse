// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the DeinitInheritedContext.</summary>
    public sealed class DeinitInheritedContext : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            cb.Line("#region DeinitInheritedContext");
            cb.Line($"public {model.LifecycleMethodModifier} void DeinitInheritedContext()");
            cb.Block(() =>
            {
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-WIDGET\", \"context.deinitialized\", (\"widget\", this));"
                );

                if (!model.IsBase)
                {
                    cb.Line("base.DeinitInheritedContext();");
                }

                foreach (var ic in model.GetProperties(PropertyFilter.InheritedContext))
                {
                    if (ic.IsOverride)
                        continue;
                    cb.Line(
                        $"this.Engine.InheritedContextManager.ReleaseInheritedContext(this, \"{ic.Name}\");"
                    );
                }
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
