// parents: @.frostline/graph/m-source-generators-common.xml

using System;
using System.Text;

namespace FlexVerse.SourceGenerators
{
    /// <summary>Represents the CodeBuilder.</summary>
    public class CodeBuilder
    {
        private readonly StringBuilder _sb = new();
        private int _indent = 0;
        private bool _isAtStartOfLine = true;

        /// <summary>Performs the Append operation.</summary>
        public void Append(string text)
        {
            if (_isAtStartOfLine && text.Length > 0)
            {
                _sb.Append(new string(' ', _indent * 4));
            }
            _sb.Append(text);
            _isAtStartOfLine = string.IsNullOrEmpty(text) && _isAtStartOfLine;
        }

        /// <summary>Performs the Line operation.</summary>
        public void Line(string text = "")
        {
            Append(text);
            _sb.AppendLine();
            _isAtStartOfLine = true;
        }

        /// <summary>Performs the Block operation.</summary>
        public void Block(Action content)
        {
            Line("{");
            _indent++;
            content();
            _indent--;
            Append("}");
        }

        /// <summary>Performs the Indent operation.</summary>
        public void Indent(Action content)
        {
            _indent++;
            content();
            _indent--;
        }

        /// <summary>Performs the To String operation.</summary>
        public override string ToString() => _sb.ToString();
    }
}
