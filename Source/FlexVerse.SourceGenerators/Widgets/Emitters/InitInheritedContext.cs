// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Represents the InitInheritedContext.</summary>
    public sealed class InitInheritedContext : IWidgetMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, WidgetModel model)
        {
            cb.Line("#region InitInheritedContext");
            cb.Line($"public {model.LifecycleMethodModifier} void InitInheritedContext()");
            cb.Block(() =>
            {
                cb.Line(
                    "FlexVerse.Log.Debug(\"M-WIDGET\", \"context.initialized\", (\"widget\", this));"
                );

                if (!model.IsBase)
                {
                    cb.Line("base.InitInheritedContext();");
                }

                foreach (var ic in model.GetProperties(PropertyFilter.IcConsumer))
                {
                    if (ic.IsOverride)
                        continue;
                    cb.Line(
                        $"this.Engine.InheritedContextManager.SubscribeConsumer(this, \"{ic.Name}\", \"{ic.Type.ArgFullName}\");"
                    );
                }
            });
            cb.Line();
            cb.Line("#endregion");
            cb.Line();
        }
    }
}
