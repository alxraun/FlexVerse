// parents: @.frostline/graph/m-widget-emitter.xml

namespace FlexVerse.SourceGenerators.Widgets.Emitters
{
    internal static class StyleLogic
    {
        /// <summary>Performs the Generate Style Update operation.</summary>
        public static void GenerateStyleUpdate(
            CodeBuilder cb,
            string propName,
            string valueAccess,
            string nodeAccess
        )
        {
            switch (propName)
            {
                case "Direction":
                    cb.Line($"{nodeAccess}.StyleSetDirection({valueAccess});");
                    break;
                case "FlexDirection":
                    cb.Line($"{nodeAccess}.StyleSetFlexDirection({valueAccess});");
                    break;
                case "AlignItems":
                    cb.Line($"{nodeAccess}.StyleSetAlignItems({valueAccess});");
                    break;
                case "AlignSelf":
                    cb.Line($"{nodeAccess}.StyleSetAlignSelf({valueAccess});");
                    break;
                case "AlignContent":
                    cb.Line($"{nodeAccess}.StyleSetAlignContent({valueAccess});");
                    break;
                case "JustifyContent":
                    cb.Line($"{nodeAccess}.StyleSetJustifyContent({valueAccess});");
                    break;
                case "FlexWrap":
                    cb.Line($"{nodeAccess}.StyleSetFlexWrap({valueAccess});");
                    break;
                case "Overflow":
                    cb.Line($"{nodeAccess}.StyleSetOverflow({valueAccess});");
                    break;
                case "PositionType":
                    cb.Line($"{nodeAccess}.StyleSetPositionType({valueAccess});");
                    break;
                case "Display":
                    cb.Line($"{nodeAccess}.StyleSetDisplay({valueAccess});");
                    break;
                case "FlexGrow":
                    cb.Line($"{nodeAccess}.StyleSetFlexGrow({valueAccess});");
                    break;
                case "FlexShrink":
                    cb.Line($"{nodeAccess}.StyleSetFlexShrink({valueAccess});");
                    break;
                case "AspectRatio":
                    cb.Line($"{nodeAccess}.StyleSetAspectRatio({valueAccess});");
                    break;
                case "Width":
                case "Height":
                    GenerateDimensionAutoLogic(cb, propName, valueAccess, nodeAccess);
                    break;
                case "FlexBasis":
                    GenerateFlexBasisLogic(cb, valueAccess, nodeAccess);
                    break;
                case "MinWidth":
                case "MinHeight":
                case "MaxWidth":
                case "MaxHeight":
                    GenerateDimensionLogic(cb, propName, valueAccess, nodeAccess);
                    break;
                case var p when p.StartsWith("Margin"):
                    GenerateEdgeLogic(
                        cb,
                        "Margin",
                        p.Substring("Margin".Length),
                        valueAccess,
                        nodeAccess
                    );
                    break;
                case var p when p.StartsWith("Padding"):
                    GenerateEdgeLogic(
                        cb,
                        "Padding",
                        p.Substring("Padding".Length),
                        valueAccess,
                        nodeAccess
                    );
                    break;
                case var p when p.StartsWith("Border"):
                    GenerateEdgeLogic(
                        cb,
                        "Border",
                        p.Substring("Border".Length),
                        valueAccess,
                        nodeAccess
                    );
                    break;
                case var p when p.StartsWith("Position") && p != "PositionType":
                    GenerateEdgeLogic(
                        cb,
                        "Position",
                        p.Substring("Position".Length),
                        valueAccess,
                        nodeAccess
                    );
                    break;
                default:
                    cb.Line(
                        $"/* WARNING: Unhandled style property '{propName}' for direct application. */"
                    );
                    break;
            }
        }

        private static void GenerateDimensionAutoLogic(
            CodeBuilder cb,
            string propName,
            string valueAccess,
            string nodeAccess
        )
        {
            cb.Line($"var v_dim_auto = {valueAccess};");
            cb.Line($"if (v_dim_auto.unit == FlexVerse.Unit.Undefined)");
            cb.Block(() =>
            {
                cb.Line($"{nodeAccess}.StyleSet{propName}Auto();");
                cb.Line("return;");
            });
            cb.Line("switch (v_dim_auto.unit)");
            cb.Block(() =>
            {
                cb.Line("case FlexVerse.Unit.Point:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSet{propName}(v_dim_auto.value);");
                    cb.Line("break;");
                });
                cb.Line("case FlexVerse.Unit.Percent:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSet{propName}Percent(v_dim_auto.value);");
                    cb.Line("break;");
                });
                cb.Line("case FlexVerse.Unit.Auto:");
                cb.Line("case FlexVerse.Unit.Undefined:");
                cb.Line("default:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSet{propName}Auto();");
                    cb.Line("break;");
                });
            });
        }

        private static void GenerateFlexBasisLogic(
            CodeBuilder cb,
            string valueAccess,
            string nodeAccess
        )
        {
            cb.Line($"var v_fb = {valueAccess};");
            cb.Line("if (v_fb.unit == FlexVerse.Unit.Undefined)");
            cb.Block(() =>
            {
                cb.Line($"{nodeAccess}.NodeStyleSetFlexBasisAuto();");
                cb.Line("return;");
            });
            cb.Line("switch (v_fb.unit)");
            cb.Block(() =>
            {
                cb.Line("case FlexVerse.Unit.Point:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSetFlexBasis(v_fb.value);");
                    cb.Line("break;");
                });
                cb.Line("case FlexVerse.Unit.Percent:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSetFlexBasisPercent(v_fb.value);");
                    cb.Line("break;");
                });
                cb.Line("case FlexVerse.Unit.Auto:");
                cb.Line("case FlexVerse.Unit.Undefined:");
                cb.Line("default:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.NodeStyleSetFlexBasisAuto();");
                    cb.Line("break;");
                });
            });
        }

        private static void GenerateDimensionLogic(
            CodeBuilder cb,
            string propName,
            string valueAccess,
            string nodeAccess
        )
        {
            cb.Line($"var v_dim = {valueAccess};");
            cb.Line("if (v_dim.unit == FlexVerse.Unit.Undefined)");
            cb.Block(() =>
            {
                cb.Line($"{nodeAccess}.StyleSet{propName}(float.NaN);");
                cb.Line("return;");
            });
            cb.Line("switch (v_dim.unit)");
            cb.Block(() =>
            {
                cb.Line("case FlexVerse.Unit.Point:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSet{propName}(v_dim.value);");
                    cb.Line("break;");
                });
                cb.Line("case FlexVerse.Unit.Percent:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSet{propName}Percent(v_dim.value);");
                    cb.Line("break;");
                });
                cb.Line("case FlexVerse.Unit.Undefined:");
                cb.Line("default:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSet{propName}(float.NaN);");
                    cb.Line("break;");
                });
            });
        }

        private static void GenerateEdgeLogic(
            CodeBuilder cb,
            string typePrefix,
            string edgeName,
            string valueAccess,
            string nodeAccess
        )
        {
            var flexboxEdge = MapEdgeToFlexboxEdge(edgeName);
            cb.Line($"var v_edge = {valueAccess};");

            var defaultValueCall = $"{nodeAccess}.StyleSet{typePrefix}({flexboxEdge}, float.NaN);";

            cb.Line("if (v_edge.unit == FlexVerse.Unit.Undefined)");
            cb.Block(() =>
            {
                cb.Line($"{defaultValueCall}");
                cb.Line("return;");
            });
            cb.Line();
            cb.Line("switch (v_edge.unit)");
            cb.Block(() =>
            {
                cb.Line("case FlexVerse.Unit.Point:");
                cb.Indent(() =>
                {
                    cb.Line($"{nodeAccess}.StyleSet{typePrefix}({flexboxEdge}, v_edge.value);");
                    cb.Line("break;");
                });

                if (typePrefix != "Border")
                {
                    cb.Line("case FlexVerse.Unit.Percent:");
                    cb.Indent(() =>
                    {
                        cb.Line(
                            $"{nodeAccess}.StyleSet{typePrefix}Percent({flexboxEdge}, v_edge.value);"
                        );
                        cb.Line("break;");
                    });

                    if (typePrefix == "Margin")
                    {
                        cb.Line("case FlexVerse.Unit.Auto:");
                        cb.Indent(() =>
                        {
                            cb.Line($"{nodeAccess}.StyleSetMarginAuto({flexboxEdge});");
                            cb.Line("break;");
                        });
                    }
                }

                if (typePrefix != "Margin")
                {
                    cb.Line("case FlexVerse.Unit.Auto:");
                }
                cb.Line("case FlexVerse.Unit.Undefined:");
                cb.Line("default:");
                cb.Indent(() =>
                {
                    cb.Line($"{defaultValueCall}");
                    cb.Line("break;");
                });
            });
            cb.Line();
        }

        private static string MapEdgeToFlexboxEdge(string edgeName)
        {
            var edge = edgeName switch
            {
                "All" => "All",
                "" => "All",
                "Left" => "Left",
                "Top" => "Top",
                "Right" => "Right",
                "Bottom" => "Bottom",
                "Start" => "Start",
                "End" => "End",
                "Horizontal" => "Horizontal",
                "Vertical" => "Vertical",
                _ => "All",
            };
            return $"FlexVerse.Edge.{edge}";
        }
    }
}
