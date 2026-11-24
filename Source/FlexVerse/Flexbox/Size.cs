// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

namespace FlexVerse
{
    /// <summary>Holds a width and height pair.</summary>
    public class Size(float w, float h)
    {
        /// <summary>Holds the width component.</summary>
        public float Width = w;

        /// <summary>Holds the height component.</summary>
        public float Height = h;
    }
}
