// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>Raw progress is validated before clamping the drawing width.</summary>
    public partial class SequenceProgressBar : Widget
    {
        [Prop]
        public Behavior Sequence { get; set; }

        [Prop]
        public Behavior<SequenceScene> Scene { get; set; }
        private SequenceScene _sceneSource;
        private Behavior _observedSequence;
        private float _fraction;
        private int _percent = -1;
        private bool _invalid;
        private string _label = "0%";

        public SequenceProgressBar()
        {
            Width = 100f.Percent();
            Height = 46f.Point();
        }

        public override void OnMount()
        {
            base.OnMount();
            _sceneSource = Scene?.Value;
            if (_sceneSource is not null)
                _sceneSource.SequenceChanged += ObserveSequence;
            ObserveSequence(_sceneSource?.Sequence ?? Sequence);
        }

        public override void OnUnmount()
        {
            if (_sceneSource is not null)
                _sceneSource.SequenceChanged -= ObserveSequence;
            if (_observedSequence is not null)
                _observedSequence.OnUpdate -= OnProgress;
            base.OnUnmount();
        }

        private void ObserveSequence(Behavior sequence)
        {
            if (_observedSequence is not null)
                _observedSequence.OnUpdate -= OnProgress;
            _observedSequence = sequence;
            if (_observedSequence is not null)
                _observedSequence.OnUpdate += OnProgress;
            OnProgress(0f);
        }

        private void OnProgress(float progress)
        {
            var invalid =
                float.IsNaN(progress)
                || float.IsInfinity(progress)
                || progress < 0f
                || progress > 1f;
            _fraction = invalid ? 0f : progress;
            var percent = invalid ? -1 : Mathf.RoundToInt(progress * 100f);
            if (percent == _percent && invalid == _invalid)
                return;
            _percent = percent;
            _invalid = invalid;
            _label = invalid ? "OUT OF RANGE" : percent + "%";
        }

        public override void OnGUI(Rect rect)
        {
            var color = GUI.color;
            var font = Text.Font;
            var anchor = Text.Anchor;
            try
            {
                Widgets.DrawBoxSolid(rect, SequenceStyle.Surface);
                var tint = _invalid ? SequenceStyle.Warning : SequenceStyle.Accent;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = SequenceStyle.Muted;
                var label = new Rect(rect.x + 12f, rect.y + 4f, rect.width - 24f, 22f);
                Widgets.Label(label, "CHAIN PROGRESS");
                Text.Anchor = TextAnchor.MiddleRight;
                GUI.color = tint;
                Widgets.Label(label, _label);
                var bar = new Rect(rect.x + 12f, rect.yMax - 12f, rect.width - 24f, 3f);
                Widgets.DrawBoxSolid(bar, SequenceStyle.Border);
                Widgets.DrawBoxSolid(
                    new Rect(bar.x, bar.y, bar.width * _fraction, bar.height),
                    tint
                );
            }
            finally
            {
                GUI.color = color;
                Text.Font = font;
                Text.Anchor = anchor;
            }
        }
    }
}
