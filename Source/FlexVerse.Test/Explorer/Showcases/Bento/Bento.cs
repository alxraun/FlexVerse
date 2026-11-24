// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    [Screen("Showcases/Bento")]
    public partial class Bento : View
    {
        [State]
        public Behavior<BentoSkin> Skin { get; set; }

        [State]
        public Behavior<BentoPresentation> Presentation { get; set; }

        public Bento()
        {
            Width = 100f.Percent();
            Height = 100f.Percent();
            FlexGrow = 1f;
            BackgroundColor = BentoSkin.Background;
            PaddingAll = 18f.Point();
        }

        public override void OnMount()
        {
            Skin = BentoSkin.Create();
            var presentation = new BentoPresentation();
            presentation.Initialize();
            Presentation = presentation;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new BentoDashboard
                {
                    Scene = Presentation,
                    Skin = Skin,
                    FlexGrow = 1f,
                    MinHeight = 0f.Point(),
                },
            ];

        public override void OnUnmount()
        {
            Presentation.Value?.Release();
            Skin.Value?.Dispose();
        }
    }
}
