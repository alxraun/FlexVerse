// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Explorer-only observation and input boundary; observed geometry is never mutated.
    /// a previous plan is copied before dispatch, never reconstructed from the new geometry.
    /// </summary>
    public sealed class SequenceScene
    {
        public const int TrailCapacity = 256;
        public const float TrailLifetime = 5f;
        public const float HistoryLifetime = 8f;

        public SequenceBehavior<Vector2> Sequence { get; private set; }
        public event Action<Behavior> SequenceChanged;
        public Vector2 Value => _project is null ? Sequence.Value : _project(Sequence.Value);
        public Vector2 Target => _getTarget is null ? Sequence.Target : _getTarget();
        public IReadOnlyList<Vector2> Live { get; private set; }
        public string[] WaypointLabels { get; private set; }
        public bool HasRetargeted { get; private set; }
        public Behavior<Vector2>[] WaypointChildren { get; private set; }
        public Vector2 Entry { get; private set; }
        public Vector2[] Previous { get; private set; }
        public int PreviousCount { get; private set; }
        public float PreviousTime { get; private set; }
        public Vector2[] Trail { get; } = new Vector2[TrailCapacity];
        public float[] TrailTimes { get; } = new float[TrailCapacity];
        public int TrailCount { get; private set; }
        public int TrailHead { get; private set; }
        public int CurrentWaypoint { get; private set; } = -1;
        public bool Ready =>
            Live is not null && _originField is not null && _currentIndexField is not null;
        public int UpdatingChildren { get; private set; }
        public string Readout { get; private set; } = "Waiting for sequence geometry";

        private Func<SequenceBehavior<Vector2>> _getSequence;
        private Func<Vector2, Vector2> _project;
        private Func<Vector2> _getTarget;
        private Func<Vector2> _getEntry;
        private Action<Vector2> _retarget;
        private IReadOnlyList<Vector2> _localLive;
        private Vector2[] _worldLive;
        private object _geometry;
        private FieldInfo _originField;
        private FieldInfo _currentIndexField;
        private bool _originRead;
        private int _planFirst;
        private float _nextReadout;
        private int _sampleFrame = -1;

        public void Initialize(
            Func<SequenceBehavior<Vector2>> getSequence,
            Func<Vector2, Vector2> project,
            Func<Vector2> getTarget,
            Func<Vector2> getEntry,
            Action<Vector2> retarget
        )
        {
            _getSequence = getSequence;
            _project = project;
            _getTarget = getTarget;
            _getEntry = getEntry;
            _retarget = retarget;
            Initialize(getSequence());
        }

        public void Initialize(SequenceBehavior<Vector2> sequence)
        {
            Sequence = sequence;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            _currentIndexField = sequence.GetType().GetField("_currentIndex", flags);
            var geometryField = sequence.GetType().GetField("_geometry", flags);
            _geometry = geometryField?.GetValue(sequence);
            var geometryType = _geometry?.GetType();
            _originField = geometryType?.GetField("_origin", flags);
            Live =
                geometryType?.GetField("_results", flags)?.GetValue(_geometry)
                as IReadOnlyList<Vector2>;

            _localLive = Live;
            if (_project is not null && Live is not null)
            {
                if (_worldLive is null || _worldLive.Length != Live.Count)
                    _worldLive = new Vector2[Live.Count];
                Live = _worldLive;
            }

            if (!Ready || _originField is null)
            {
                FlexVerse.Log.Warning(
                    "M-EXPLORER",
                    "sequence.canvas.geometry.unavailable",
                    ("sequence", sequence)
                );
                return;
            }

            var children = new List<Behavior<Vector2>>();
            foreach (var child in sequence.Children)
                if (child is Behavior<Vector2> typed)
                    children.Add(typed);
            WaypointChildren = children.ToArray();
            WaypointLabels = new string[WaypointChildren.Length];
            for (int i = 0; i < WaypointLabels.Length; i++)
                WaypointLabels[i] = (i + 1).ToString(CultureInfo.InvariantCulture);
            if (Previous is null || Previous.Length != WaypointChildren.Length + 1)
                Previous = new Vector2[WaypointChildren.Length + 1];
            SequenceChanged?.Invoke(sequence);
            Capture();
        }

        public void Capture()
        {
            if (_getSequence is not null && !ReferenceEquals(Sequence, _getSequence()))
            {
                _originRead = false;
                Initialize(_getSequence());
            }
            if (!Ready || _sampleFrame == Time.frameCount)
                return;
            _sampleFrame = Time.frameCount;

            if (_project is not null)
                for (int i = 0; i < _localLive.Count; i++)
                    _worldLive[i] = _project(_localLive[i]);

            if (!_originRead && Live.Count > 0)
            {
                Entry = _getEntry is null ? (Vector2)_originField.GetValue(_geometry) : _getEntry();
                _originRead = true;
            }

            CurrentWaypoint = FindCurrentWaypoint();
            UpdatingChildren = 0;
            foreach (var child in Sequence.Children)
                if (child.IsUpdating)
                    UpdatingChildren++;
            var value = Value;
            var previousIndex = (TrailHead + TrailCapacity - 1) % TrailCapacity;
            if (TrailCount == 0 || (value - Trail[previousIndex]).sqrMagnitude > 0.0001f)
            {
                Trail[TrailHead] = value;
                TrailTimes[TrailHead] = Time.unscaledTime;
                TrailHead = (TrailHead + 1) % TrailCapacity;
                TrailCount = Math.Min(TrailCount + 1, TrailCapacity);
            }

            if (Time.unscaledTime < _nextReadout)
                return;
            _nextReadout = Time.unscaledTime + 0.1f;
            var target = Target;
            Readout = string.Format(
                CultureInfo.InvariantCulture,
                "VALUE  ({0:0.0}, {1:0.0})     TARGET  ({2:0.0}, {3:0.0})",
                value.x,
                value.y,
                target.x,
                target.y
            );
        }

        public void Retarget(Vector2 target) => Retarget(target, true);

        public void Retarget(Vector2 target, bool capturePrevious)
        {
            if (Target == target)
                return;
            if (Ready && capturePrevious)
            {
                PreviousCount = 0;
                Previous[PreviousCount++] = Entry;
                for (int i = _planFirst; i < Live.Count; i++)
                    Previous[PreviousCount++] = Live[i];
                PreviousTime = Time.unscaledTime;
            }

            HasRetargeted = true;
            Entry = Value;
            _planFirst = Sequence.Strategy.Restart ? 0 : FindCurrentWaypoint();
            if (_planFirst < 0)
                _planFirst = Live?.Count ?? 0;
            if (_retarget is null)
                Sequence.Target = target;
            else
                _retarget(target);
            _nextReadout = 0f;
            _sampleFrame = -1;
            Capture();
        }

        private int FindCurrentWaypoint()
        {
            if (_currentIndexField is null)
                return -1;
            var currentIndex = (int)_currentIndexField.GetValue(Sequence);
            var waypoint = 0;
            for (int i = 0; i < Sequence.Children.Count; i++)
            {
                if (Sequence.Children[i] is not Behavior<Vector2>)
                    continue;
                if (i == currentIndex)
                    return waypoint;
                waypoint++;
            }
            return -1;
        }
    }
}
