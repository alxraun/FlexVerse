// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public enum AnimationDisplayMode
    {
        Position,
        Velocity,
    }

    public partial class AnimationDisplay : Widget
    {
        [Prop]
        public Behavior<Behavior> Behavior { get; set; }

        [Prop]
        public Behavior<AnimationDisplayMode> Mode { get; set; } = AnimationDisplayMode.Position;

        [Prop]
        public Behavior<Color> BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.15f);

        [Prop]
        public Behavior<Color> GridColor { get; set; } = new Color(1f, 1f, 1f, 0.1f);

        [Prop]
        public Behavior<Color> LineColor { get; set; } = Color.cyan;

        [Prop]
        public Behavior<bool> ShowGrid { get; set; } = true;

        [Prop]
        public Behavior<bool> ShowLabels { get; set; } = true;

        [Prop]
        public Behavior<bool> ShowLiveReadout { get; set; } = true;

        private const int HistoryLimit = 1000;
        private const int MaxCsvPoints = 150;
        private const float MinWidthForLabels = 100f;
        private const float DeltaTimeEpsilon = 0.0001f;

        private const float LabelHeight = 15f;
        private const float LabelWidth = 50f;
        private const float LabelPadding = 2f;
        private const float ReadoutPadding = 5f;

        private readonly List<(float time, float value)> _positionHistory = [];
        private readonly List<(float time, float value)> _velocityHistory = [];
        private float _startTime;
        private bool _isRecording;

        private Func<object, float> _valueExtractor;
        private Behavior _lastMonitoredBehavior;

        public override void OnMount()
        {
            Refs.Behavior.Changed += HandleBehaviorChange;
            HandleBehaviorChange();
        }

        public override void OnUnmount()
        {
            Refs.Behavior.Changed -= HandleBehaviorChange;
            UnsubscribeFromBehavior();
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (e.Button == 2)
            {
                CopyToClipboard();
                e.StopPropagation();
            }
        }

        private void HandleBehaviorChange()
        {
            UnsubscribeFromBehavior();
            var currentBehavior = Behavior?.Value;
            if (currentBehavior != null)
            {
                _valueExtractor = CreateExtractorFor(currentBehavior);
                currentBehavior.OnStart += OnBehaviorStart;
                currentBehavior.OnUpdate += OnBehaviorUpdate;
                _lastMonitoredBehavior = currentBehavior;

                if (currentBehavior.IsUpdating)
                {
                    StartNewSession();
                }
            }
        }

        private void UnsubscribeFromBehavior()
        {
            if (_lastMonitoredBehavior != null)
            {
                _lastMonitoredBehavior.OnStart -= OnBehaviorStart;
                _lastMonitoredBehavior.OnUpdate -= OnBehaviorUpdate;
                _lastMonitoredBehavior = null;
            }
        }

        private void OnBehaviorStart()
        {
            StartNewSession();
        }

        private void StartNewSession()
        {
            _positionHistory.Clear();
            _velocityHistory.Clear();
            _startTime = Time.time;
            _isRecording = true;
            OnBehaviorUpdate(0f);
        }

        private void OnBehaviorUpdate(float _)
        {
            if (!_isRecording)
            {
                StartNewSession();
                return;
            }

            if (_valueExtractor == null || Behavior.Value == null)
                return;

            var behaviorValueProp = Behavior.Value.GetType().GetProperty("Value");
            if (behaviorValueProp == null)
                return;

            var rawValue = behaviorValueProp.GetValue(Behavior.Value);
            var floatValue = _valueExtractor(rawValue);
            var currentTime = Time.time - _startTime;

            _positionHistory.Add((currentTime, floatValue));

            if (_positionHistory.Count > 1)
            {
                var lastPoint = _positionHistory[^2];
                var deltaTime = currentTime - lastPoint.time;
                if (deltaTime > DeltaTimeEpsilon)
                {
                    var velocity = (floatValue - lastPoint.value) / deltaTime;
                    _velocityHistory.Add((currentTime, velocity));
                }
            }

            if (_positionHistory.Count > HistoryLimit)
            {
                _positionHistory.RemoveAt(0);
            }

            if (_velocityHistory.Count > HistoryLimit)
            {
                _velocityHistory.RemoveAt(0);
            }
        }

        public override void OnGUI(Rect calculatedRect)
        {
            Widgets.DrawBoxSolid(calculatedRect, BackgroundColor.Value);

            var data =
                Mode.Value == AnimationDisplayMode.Position ? _positionHistory : _velocityHistory;

            if (ShowLiveReadout.Value && data.Any())
            {
                DrawLiveReadout(calculatedRect, data.Last());
            }

            float minTime;
            float maxTime;
            float minValue;
            float maxValue;

            if (data.Count >= 1)
            {
                minTime = data[0].time;
                maxTime = data[^1].time;

                minValue = float.MaxValue;
                maxValue = float.MinValue;
                foreach (var point in data)
                {
                    if (point.value < minValue)
                        minValue = point.value;
                    if (point.value > maxValue)
                        maxValue = point.value;
                }

                if (Mathf.Approximately(minTime, maxTime))
                    maxTime = minTime + 1f;

                if (Mathf.Approximately(minValue, maxValue))
                {
                    var center = minValue;
                    minValue = center - 1f;
                    maxValue = center + 1f;
                }
            }
            else
            {
                minTime = 0f;
                maxTime = 1f;
                minValue = 0f;
                maxValue = 1f;
            }

            if (ShowGrid.Value)
            {
                DrawGrid(calculatedRect, minTime, maxTime, minValue, maxValue);
            }

            if (data.Count < 2)
            {
                return;
            }

            var points = new List<Vector2>(data.Count);
            foreach (var (time, value) in data)
            {
                var x = calculatedRect.x + GetCoord(time, minTime, maxTime, calculatedRect.width);
                var y =
                    calculatedRect.yMax
                    - GetCoord(value, minValue, maxValue, calculatedRect.height);
                points.Add(new Vector2(x, y));
            }

            for (var i = 0; i < points.Count - 1; i++)
            {
                Widgets.DrawLine(points[i], points[i + 1], LineColor.Value, 2f);
            }
        }

        private float GetCoord(float val, float min, float max, float scale)
        {
            if (Mathf.Approximately(max, min))
                return 0;
            return (val - min) / (max - min) * scale;
        }

        private Func<object, float> CreateExtractorFor(Behavior behavior)
        {
            var behaviorType = behavior.GetType();
            while (
                behaviorType != null
                && (
                    !behaviorType.IsGenericType
                    || behaviorType.GetGenericTypeDefinition() != typeof(Behavior<>)
                )
            )
            {
                behaviorType = behaviorType.BaseType;
            }

            if (behaviorType == null)
            {
                return _ => 0f;
            }

            var genericArgType = behaviorType.GetGenericArguments()[0];

            if (genericArgType == typeof(Length))
                return obj => ((Length)obj).value;
            if (genericArgType == typeof(float))
                return obj => (float)obj;
            if (genericArgType == typeof(int))
                return obj => (int)obj;
            if (genericArgType == typeof(Color))
                return obj => ((Color)obj).a;

            return _ => 0f;
        }

        private void DrawLiveReadout(Rect rect, (float time, float value) lastPoint)
        {
            var prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.UpperRight;

            var readoutText = $"T: {lastPoint.time:F2}\nV: {lastPoint.value:F2}";
            var readoutRect = new Rect(rect.x, rect.y, rect.width - ReadoutPadding, rect.height);

            Widgets.Label(readoutRect, readoutText);

            Text.Anchor = prevAnchor;
        }

        private void DrawGrid(
            Rect rect,
            float minTime,
            float maxTime,
            float minValue,
            float maxValue
        )
        {
            var prevColor = GUI.color;
            GUI.color = GridColor.Value;

            const int gridLineCount = 4;
            DrawGridLines(rect, gridLineCount);

            if (ShowLabels.Value && rect.width >= MinWidthForLabels)
            {
                DrawGridLabels(rect, gridLineCount, minTime, maxTime, minValue, maxValue);
            }

            GUI.color = prevColor;
        }

        private static void DrawGridLines(Rect rect, int lineCount)
        {
            for (var i = 0; i <= lineCount; i++)
            {
                var t = (float)i / lineCount;
                var x = rect.x + t * rect.width;
                Widgets.DrawLineVertical(x, rect.y, rect.height);

                var y = rect.yMax - t * rect.height;
                Widgets.DrawLineHorizontal(rect.x, y, rect.width);
            }
        }

        private static void DrawGridLabels(
            Rect rect,
            int lineCount,
            float minTime,
            float maxTime,
            float minValue,
            float maxValue
        )
        {
            var prevAnchor = Text.Anchor;

            var valueFormat = GetLabelFormat(maxValue - minValue);
            var timeFormat = GetLabelFormat(maxTime - minTime);

            var isOriginZero =
                Mathf.Approximately(minTime, 0f) && Mathf.Approximately(minValue, 0f);
            if (isOriginZero)
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                var labelRect = new Rect(
                    rect.x + LabelPadding,
                    rect.yMax - LabelHeight,
                    LabelWidth,
                    LabelHeight
                );
                Widgets.Label(labelRect, "0");
            }

            DrawYAxisLabels(rect, lineCount, minValue, maxValue, valueFormat, isOriginZero);
            DrawXAxisLabels(rect, lineCount, minTime, maxTime, timeFormat, isOriginZero);

            Text.Anchor = prevAnchor;
        }

        private static string GetLabelFormat(float range) => range > 10 ? "F0" : "F1";

        private static void DrawYAxisLabels(
            Rect rect,
            int lineCount,
            float minValue,
            float maxValue,
            string format,
            bool skipFirst
        )
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            for (var i = 0; i <= lineCount; i++)
            {
                if (i == 0 && skipFirst)
                    continue;

                var t = (float)i / lineCount;
                var y = rect.yMax - t * rect.height;
                var value = Mathf.Lerp(minValue, maxValue, t);

                var labelRect = new Rect(
                    rect.x + LabelPadding,
                    y - LabelHeight,
                    LabelWidth,
                    LabelHeight
                );
                if (i == lineCount)
                    labelRect.y = rect.y;

                Widgets.Label(labelRect, value.ToString(format));
            }
        }

        private static void DrawXAxisLabels(
            Rect rect,
            int lineCount,
            float minTime,
            float maxTime,
            string format,
            bool skipFirst
        )
        {
            for (var i = 0; i <= lineCount; i++)
            {
                if (i == 0 && skipFirst)
                    continue;

                var t = (float)i / lineCount;
                var x = rect.x + t * rect.width;
                var time = Mathf.Lerp(minTime, maxTime, t);

                Text.Anchor = TextAnchor.MiddleLeft;
                var labelRect = new Rect(
                    x + LabelPadding,
                    rect.yMax - LabelHeight,
                    LabelWidth,
                    LabelHeight
                );

                if (i == lineCount)
                {
                    Text.Anchor = TextAnchor.MiddleRight;
                    labelRect.x -= LabelWidth + LabelPadding;
                }

                Widgets.Label(labelRect, time.ToString(format));
            }
        }

        private void CopyToClipboard()
        {
            var data =
                Mode.Value == AnimationDisplayMode.Position ? _positionHistory : _velocityHistory;
            var sampledData = SampleData(data, MaxCsvPoints);
            var csv = GenerateCsv(sampledData);
            GUIUtility.systemCopyBuffer = csv;
        }

        private List<(float time, float value)> SampleData(
            List<(float time, float value)> source,
            int maxPoints
        )
        {
            if (source.Count <= maxPoints)
                return [.. source];

            var sampled = new List<(float, float)>();
            for (int i = 0; i < maxPoints - 1; i++)
            {
                var index = (int)((long)i * (source.Count - 1) / (maxPoints - 1));
                sampled.Add(source[index]);
            }
            sampled.Add(source[^1]);
            return sampled;
        }

        private string GenerateCsv(List<(float time, float value)> data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Time,Value");
            foreach (var (time, value) in data)
            {
                sb.AppendLine($"{time:F4},{value:F4}");
            }
            return sb.ToString();
        }
    }
}
