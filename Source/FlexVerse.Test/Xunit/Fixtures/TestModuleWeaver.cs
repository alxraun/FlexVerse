// parents: @.frostline/graph/m-xunit.xml

using System;
using FlexVerse.PropertyWeaver.Fody;

namespace FlexVerse.Test
{
    internal sealed class TestModuleWeaver : ModuleWeaver
    {
        public Action<string> InfoHandler { get; set; } = _ => { };
        public Action<string> WarningHandler { get; set; } = _ => { };
        public Action<string> ErrorHandler { get; set; } = _ => { };

        public override void WriteInfo(string message) => InfoHandler(message);

        public override void WriteWarning(string message) => WarningHandler(message);

        public override void WriteError(string message) => ErrorHandler(message);
    }
}
