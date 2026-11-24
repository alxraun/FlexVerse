// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

namespace FlexVerse.Flexbox
{
    /// <summary>Implements the flexbox layout algorithm core. (M-FLEXBOX)</summary>
    public partial class Flex
    {
        internal class CachedMeasurement
        {
            internal float availableWidth;
            internal float availableHeight;
            internal MeasureMode widthMeasureMode = MeasureMode.Undefined;
            internal MeasureMode heightMeasureMode = MeasureMode.Undefined;
            internal float computedWidth = -1;
            internal float computedHeight = -1;

            internal void ResetToDefault()
            {
                availableHeight = 0;
                availableWidth = 0;
                widthMeasureMode = MeasureMode.Undefined;
                heightMeasureMode = MeasureMode.Undefined;
                computedWidth = -1;
                computedHeight = -1;
            }
        }

        internal class Layout
        {
            internal readonly float[] Position = new float[4];
            internal readonly float[] Dimensions = [float.NaN, float.NaN];
            internal readonly float[] Margin = new float[6];
            internal readonly float[] Border = new float[6];
            internal readonly float[] Padding = new float[6];
            internal Directions Direction;
            internal int computedFlexBasisGeneration;
            internal float computedFlexBasis = float.NaN;
            internal bool HadOverflow = false;

            internal int generationCount;
            internal Directions lastParentDirection = Directions.NeverUsed_1;
            internal int nextCachedMeasurementsIndex = 0;
            internal readonly CachedMeasurement[] cachedMeasurements =
            [
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
            ];
            internal readonly float[] measuredDimensions = [float.NaN, float.NaN];
            internal readonly CachedMeasurement cachedLayout = new();

            internal void ResetToDefault()
            {
                for (int i = 0; i < Position.Length; i++)
                {
                    Position[i] = 0;
                }
                for (int i = 0; i < Dimensions.Length; i++)
                {
                    Dimensions[i] = float.NaN;
                }
                for (int i = 0; i < 6; i++)
                {
                    Margin[i] = 0;
                    Border[i] = 0;
                    Padding[i] = 0;
                }
                Direction = Directions.Inherit;
                computedFlexBasisGeneration = 0;
                computedFlexBasis = float.NaN;
                HadOverflow = false;
                generationCount = 0;
                lastParentDirection = Directions.NeverUsed_1;
                nextCachedMeasurementsIndex = 0;

                foreach (var cm in cachedMeasurements)
                {
                    cm.ResetToDefault();
                }

                for (int i = 0; i < measuredDimensions.Length; i++)
                {
                    measuredDimensions[i] = float.NaN;
                }

                cachedLayout.ResetToDefault();
            }
        }
    }
}
