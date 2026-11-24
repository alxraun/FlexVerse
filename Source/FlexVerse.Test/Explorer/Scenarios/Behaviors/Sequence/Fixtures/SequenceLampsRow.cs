// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>One chip per child. DONE requires an observed start; instant/no-op stay SILENT.</summary>
    public partial class SequenceLampsRow : Widget
    {
        [Prop]
        public Behavior Sequence { get; set; }

        [Prop]
        public Behavior<string[]> Names { get; set; }

        private IReadOnlyList<Behavior> _chainChildren;
        private string[] _labels;
        private bool[] _started;
        private Action[] _handlers;

        public SequenceLampsRow()
        {
            Width = 100f.Percent();
            Height = 42f.Point();
            MarginBottom = 8f.Point();
        }

        public override void OnMount()
        {
            base.OnMount();
            if (Sequence is not SequenceBehavior<Vector2> sequence)
                return;
            _chainChildren = sequence.Children;
            _labels = new string[_chainChildren.Count];
            _started = new bool[_chainChildren.Count];
            _handlers = new Action[_chainChildren.Count];
            var names = Names?.Value;
            for (int i = 0; i < _chainChildren.Count; i++)
            {
                var index = i;
                _labels[i] = names is not null && i < names.Length ? $"T{i} · {names[i]}" : $"T{i}";
                _started[i] = _chainChildren[i].IsUpdating;
                _handlers[i] = () => _started[index] = true;
                _chainChildren[i].OnStart += _handlers[i];
            }
        }

        public override void OnUnmount()
        {
            if (_chainChildren is not null)
                for (int i = 0; i < _chainChildren.Count; i++)
                    _chainChildren[i].OnStart -= _handlers[i];
            base.OnUnmount();
        }

        public override void OnGUI(Rect rect)
        {
            if (_chainChildren is null || _chainChildren.Count == 0)
                return;
            var color = GUI.color;
            var font = Text.Font;
            var anchor = Text.Anchor;
            try
            {
                Text.Font = GameFont.Tiny;
                var width = (rect.width - (_chainChildren.Count - 1) * 6f) / _chainChildren.Count;
                for (int i = 0; i < _chainChildren.Count; i++)
                {
                    var running = _chainChildren[i].IsUpdating;
                    var tint =
                        running ? SequenceStyle.Accent
                        : _started[i] ? SequenceStyle.Success
                        : SequenceStyle.Muted;
                    var cell = new Rect(rect.x + i * (width + 6f), rect.y, width, rect.height);
                    Widgets.DrawBoxSolid(
                        cell,
                        running ? SequenceStyle.ActiveSurface : SequenceStyle.Surface
                    );
                    Widgets.DrawBoxSolid(
                        new Rect(cell.x, cell.y, running ? cell.width : 3f, 2f),
                        tint
                    );
                    GUI.color = SequenceStyle.Text;
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(
                        new Rect(cell.x + 10f, cell.y, cell.width - 20f, 21f),
                        _labels[i]
                    );
                    GUI.color = tint;
                    Widgets.Label(
                        new Rect(cell.x + 10f, cell.y + 20f, cell.width - 20f, 20f),
                        running ? "RUNNING"
                            : _started[i] ? "DONE"
                            : "SILENT"
                    );
                }
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
