// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;

namespace FlexVerse.SourceGenerators.Fvp.Emitters
{
    /// <summary>FVP is the owner-independent reactive property foundation.</summary>
    public sealed class Fvp : IFvpMemberEmitter
    {
        /// <summary>Performs the Emit operation.</summary>
        public void Emit(CodeBuilder cb, FvpModel model)
        {
            if (model.Properties.IsEmpty)
                return;

            cb.Line("#region FVP");
            foreach (var prop in model.Properties)
            {
                if (prop.IsInherited)
                    continue;
                EmitProperty(cb, prop);
                cb.Line();
            }

            cb.Line("#endregion");
            cb.Line();
        }

        private static void EmitProperty(CodeBuilder cb, FvpPropertyModel prop)
        {
            if (prop.IsOverride || prop.IsInherited)
                return;

            var modifier = prop.IsNew ? "new " : "";
            cb.Line($"protected {modifier}Action {prop.FieldName}Changing;");
            cb.Line($"protected {modifier}Action {prop.FieldName}Changed;");
            cb.Line($"protected {modifier}{prop.TypeName} {prop.FieldName}{prop.Assignment};");
            cb.Line($"protected {modifier}{prop.TypeName} {prop.LogicName}");
            cb.Block(() =>
            {
                cb.Line("get");
                cb.Block(() => EmitGetter(cb, prop));
                cb.Line();
                cb.Line("set");
                cb.Block(() => EmitSetter(cb, prop));
                cb.Line();
            });
            cb.Line();
        }

        private static void EmitGetter(CodeBuilder cb, FvpPropertyModel prop)
        {
            cb.Line("EnsureFvpInitialized();");
            cb.Line($"return {prop.FieldName};");
        }

        private static void EmitSetter(CodeBuilder cb, FvpPropertyModel prop)
        {
            cb.Line("EnsureFvpInitialized();");
            cb.Line("var @new = value;");
            cb.Line($"var @old = {prop.FieldName};");
            cb.Line("if (@old is not null && @old.SemEquals(@new) == true) return;");
            cb.Line(
                $"FlexVerse.Log.Debug(\"M-FLEXVERSE-PROPERTY\", \"prop.changed\", (\"property\", \"{prop.Name}\"), (\"new\", @new), (\"old\", @old));"
            );
            cb.Line($"{prop.FieldName}Changing?.Invoke();");
            cb.Line($"{prop.FieldName} = @new;");
            cb.Line($"{prop.FieldName}Changed?.Invoke();");

            cb.Line("if (@new is not null)");
            cb.Block(() =>
            {
                cb.Line("if (@old is not null) @new.UnionWith(@old);");
                cb.Line("this.ActivateBehavior(@new);");
            });
            cb.Line("this.ReleaseBehavior(@old);");
        }
    }
}
