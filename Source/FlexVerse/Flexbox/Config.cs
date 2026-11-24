// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

namespace FlexVerse.Flexbox
{
    /// <summary>Defines global layout configuration options shared by nodes.</summary>
    public class Config
    {
        /// <summary>Enables web-compatible flex behaviors when true.</summary>
        public bool UseWebDefaults = false;

        /// <summary>Provides user-defined context data passed to the logger.</summary>
        public object Context = null;

        /// <summary>Supplies the logging callback used by the layout engine.</summary>
        public LoggerFunc Logger = DefaultLog;

        internal readonly bool[] experimentalFeatures = new bool[
            Constant.ExperimentalFeatureCount + 1
        ];
        internal bool UseLegacyStretchBehaviour = false;
        internal float PointScaleFactor = 1;

        /// <summary>Writes the formatted log message to the console.</summary>
        public static int DefaultLog(
            Config config,
            Node node,
            LogLevel level,
            string format,
            params object[] args
        )
        {
            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Fatal:
                    System.Console.WriteLine(format, args);
                    return 0;
                case LogLevel.Warn:
                case LogLevel.Info:
                case LogLevel.Debug:
                case LogLevel.Verbose:
                default:
                    System.Console.WriteLine(format, args);
                    break;
            }

            return 0;
        }

        /// <summary>Copies all configuration values from one config to another.</summary>
        public static void Copy(Config dest, Config src)
        {
            dest.UseWebDefaults = src.UseWebDefaults;
            dest.UseLegacyStretchBehaviour = src.UseLegacyStretchBehaviour;
            dest.PointScaleFactor = src.PointScaleFactor;
            dest.Logger = src.Logger;
            dest.Context = src.Context;

            for (int i = 0; i < src.experimentalFeatures.Length; i++)
            {
                dest.experimentalFeatures[i] = src.experimentalFeatures[i];
            }
        }

        /// <summary>Enables or disables the given experimental feature.</summary>
        public void SetExperimentalFeatureEnabled(ExperimentalFeature feature, bool enabled)
        {
            experimentalFeatures[(int)feature] = enabled;
        }

        /// <summary>Returns whether the given experimental feature is enabled.</summary>
        public bool IsExperimentalFeatureEnabled(ExperimentalFeature feature)
        {
            return experimentalFeatures[(int)feature];
        }

        /// <summary>Sets the scale factor converting points to pixels.</summary>
        public void SetPointScaleFactor(float pixelsInPoint)
        {
            assertWithConfig(this, pixelsInPoint >= 0, "Scale factor should not be less than zero");

            if (pixelsInPoint == 0)
            {
                PointScaleFactor = 0;
            }
            else
            {
                PointScaleFactor = pixelsInPoint;
            }
        }

        internal static void assertWithConfig(Config config, bool condition, string message)
        {
            if (!condition)
            {
                throw new System.Exception(message);
            }
        }
    }
}
