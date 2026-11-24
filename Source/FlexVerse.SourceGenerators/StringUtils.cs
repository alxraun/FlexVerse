// parents: @.frostline/graph/m-source-generators-common.xml

using System.Text;

namespace FlexVerse.SourceGenerators
{
    /// <summary>Represents the StringUtils.</summary>
    public static class StringUtils
    {
        /// <summary>Performs the To Camel operation.</summary>
        public static string ToCamel(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return val;
            }

            if (!char.IsUpper(val[0]))
            {
                return val;
            }

            var chars = val.ToCharArray();
            chars[0] = char.ToLowerInvariant(chars[0]);
            return new string(chars);
        }

        /// <summary>Performs the Sanitize operation.</summary>
        public static string Sanitize(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return "identifier";
            }

            var sb = new StringBuilder(val.Length);

            var start = 0;
            while (start < val.Length && val[start] == '_')
            {
                start++;
            }

            if (start >= val.Length) { }

            var working = val.TrimStart('_');
            if (string.IsNullOrEmpty(working))
            {
                return "identifier";
            }

            if (!char.IsLetter(working[0]) && working[0] != '_')
            {
                sb.Append('_');
            }

            foreach (var c in working)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }
    }
}
