// parents: @.frostline/graph/m-source-generators-common.xml

using FlexVerse.SourceGenerators.Fvp;
using FlexVerse.SourceGenerators.Widgets;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators
{
    /// <summary>Represents the FlexVerseGenerator.</summary>
    [Generator]
    public class FlexVerseGenerator : IIncrementalGenerator
    {
        /// <summary>Performs the Initialize operation.</summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var widgets = WidgetProvider.CreatePipeline(context);

            context.RegisterSourceOutput(
                widgets,
                (spc, result) =>
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        spc.ReportDiagnostic(diagnostic);
                    }

                    if (result.Widget is not null)
                    {
                        var generatedCode = WidgetEmitter.Emit(result.Widget);
                        spc.AddSource($"{result.Widget.ClassName}.g.cs", generatedCode);
                    }
                }
            );

            var fvpProperties = FvpProvider.CreatePipeline(context);

            context.RegisterSourceOutput(
                fvpProperties,
                (spc, result) =>
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        spc.ReportDiagnostic(diagnostic);
                    }

                    if (result.Model is not null)
                    {
                        var generatedCode = FvpEmitter.Emit(result.Model);
                        spc.AddSource($"{result.Model.ClassName}.g.cs", generatedCode);
                    }
                }
            );
        }
    }
}
