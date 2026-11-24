// parents: @.frostline/graph/m-widget-emitter.xml

using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    /// <summary>Defines the IWidgetMemberEmitter contract.</summary>
    public interface IWidgetMemberEmitter
    {
        void Emit(CodeBuilder writer, WidgetModel model);
    }
}
