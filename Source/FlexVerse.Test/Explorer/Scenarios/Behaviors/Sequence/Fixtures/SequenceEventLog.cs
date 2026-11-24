// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Text;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Self-subscribing event journal for a sequence: logs SEQ and T0..Tn
    /// OnStart/OnComplete lines with timestamps since mount. Lines are joined
    /// into a cached string on append only; OnGUI draws that string inside a
    /// scroll view without allocations.
    /// </summary>
    public partial class SequenceEventLog : Widget
    {
        private const int LineCapacity = 60;
        private const float LineHeight = 18f;

        private static readonly Color BgColor = SequenceStyle.Canvas;
        private static readonly Color TitleColor = SequenceStyle.Muted;
        private static readonly Color TextLogColor = SequenceStyle.Text;

        [Prop]
        public Behavior Sequence { get; set; }

        [Prop]
        public Behavior<string> Title { get; set; } = "EVENT JOURNAL";

        private readonly List<string> _lines = [];
        private readonly List<Behavior> _watched = [];
        private readonly List<Action> _startHandlers = [];
        private readonly List<Action> _completeHandlers = [];

        private string _joined = "Retarget the field to record events.";
        private float _mountTime;
        private Vector2 _scroll;

        public SequenceEventLog()
        {
            Width = 100f.Percent();
            Height = 170f.Point();
            MinHeight = 100f.Point();
            FlexShrink = 1;
            MarginTop = 12f.Point();
        }

        public override void OnMount()
        {
            base.OnMount();

            _mountTime = Time.realtimeSinceStartup;

            if (Sequence is not SequenceBehavior<Vector2> sequence)
                return;

            WatchBehavior(sequence, "SEQ");
            for (int i = 0; i < sequence.Children.Count; i++)
                WatchBehavior(sequence.Children[i], $"T{i}");
        }

        public override void OnUnmount()
        {
            for (int i = 0; i < _watched.Count; i++)
            {
                _watched[i].OnStart -= _startHandlers[i];
                _watched[i].OnComplete -= _completeHandlers[i];
            }
            _watched.Clear();
            _startHandlers.Clear();
            _completeHandlers.Clear();
            _lines.Clear();
            _joined = "";

            base.OnUnmount();
        }

        private void WatchBehavior(Behavior behavior, string name)
        {
            Action start = () => Append($"{name} OnStart");
            Action complete = () => Append($"{name} OnComplete");
            behavior.OnStart += start;
            behavior.OnComplete += complete;
            _watched.Add(behavior);
            _startHandlers.Add(start);
            _completeHandlers.Add(complete);
        }

        private void Append(string entry)
        {
            _lines.Add($"{Time.realtimeSinceStartup - _mountTime:0.00}s  {entry}");
            if (_lines.Count > LineCapacity)
                _lines.RemoveRange(0, _lines.Count - LineCapacity);

            var builder = new StringBuilder();
            for (int i = 0; i < _lines.Count; i++)
            {
                if (i > 0)
                    builder.Append('\n');
                builder.Append(_lines[i]);
            }
            _joined = builder.ToString();
            _scroll.y = float.MaxValue;
        }

        public override void OnGUI(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, BgColor);

            var originalAnchor = Text.Anchor;
            var originalFont = Text.Font;
            var originalColor = GUI.color;
            try
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;

                GUI.color = TitleColor;
                var titleRect = new Rect(rect.x + 12f, rect.y + 8f, rect.width - 24f, 20f);
                Widgets.Label(titleRect, Title?.Value ?? "Events");

                var viewRect = new Rect(
                    0f,
                    0f,
                    rect.width - 48f,
                    Mathf.Max(LineHeight * _lines.Count, LineHeight)
                );
                var contentRect = new Rect(
                    rect.x + 12f,
                    titleRect.yMax + 4f,
                    rect.width - 24f,
                    rect.height - 40f
                );
                GUI.color = TextLogColor;
                _scroll = GUI.BeginScrollView(contentRect, _scroll, viewRect, false, false);
                try
                {
                    Widgets.Label(viewRect, _joined);
                }
                finally
                {
                    GUI.EndScrollView();
                }
            }
            finally
            {
                Text.Anchor = originalAnchor;
                Text.Font = originalFont;
                GUI.color = originalColor;
            }
        }
    }
}
