// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>Member-emitter contract — mirrors IWidgetMemberEmitter.</summary>
    public interface IFvpMemberEmitter
    {
        void Emit(CodeBuilder cb, FvpModel model);
    }
}
