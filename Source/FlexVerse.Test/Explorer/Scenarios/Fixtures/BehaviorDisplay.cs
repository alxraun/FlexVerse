// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class BehaviorDisplay : Widget
    {
        private static readonly Color BgColor = new(0.2f, 0.2f, 0.25f);
        private static readonly Color BgColorShade = new(
            Color.black.r,
            Color.black.g,
            Color.black.b,
            0.05f
        );
        private static readonly Color LabelColor = new(0.6f, 0.6f, 0.6f);
        private static readonly Color ValueColor = new(0.9f, 0.9f, 0.9f);
        private static readonly Color BorderColor = new(0.1f, 0.1f, 0.1f);
        private static readonly Color SeparatorColor = new(0.3f, 0.3f, 0.3f);
        private static readonly Color HookSubscriberColor = new(0.7f, 0.7f, 0.9f);

        private static readonly Color FlashChangeColor = new(
            Color.green.r,
            Color.green.g,
            Color.green.b,
            0.5f
        );
        private static readonly Color FlashInvokeColor = new(
            Color.cyan.r,
            Color.cyan.g,
            Color.cyan.b,
            0.5f
        );
        private Behavior _lastActiveBehavior;
        private readonly Dictionary<string, List<string>> _hookSubscribersCache = new();
        private readonly Dictionary<string, List<string>> _lastHookSubscribers = new();
        private Behavior.Lifecycle _lastState;
        private object _lastValue;
        private object _lastTarget;
        private Type _lastBehaviorType;
        private static readonly string[] HookNames =
        [
            "OnStart",
            "OnUpdate",
            "OnComplete",
            "OnReset",
        ];

        [Prop]
        public Behavior ActiveBehavior { get; set; }

        [Prop]
        public Behavior<string> Label { get; set; }

        [Prop]
        public Behavior<Action> OnClick { get; set; }

        [State]
        private Behavior<Color> PropFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> TpFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> StFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> ValueFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> TargetFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> OnStartInvokeFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> OnUpdateInvokeFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> OnCompleteInvokeFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> OnResetInvokeFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> OnStartChangeFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> OnUpdateChangeFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> OnCompleteChangeFlashColor { get; set; } = BgColor;

        [State]
        private Behavior<Color> OnResetChangeFlashColor { get; set; } = BgColor;

        public BehaviorDisplay()
        {
            MinWidth = 180f.Point();
            MinHeight = 85f.Point();
            Width = 180f.Point();
            Height = 85f.Point();
        }

        public override void OnMouseDown(FlexEvent e)
        {
            OnClick.Value?.Invoke();
        }

        public override void OnMount()
        {
            Refs.ActiveBehavior.Changed += OnBehaviorSet;
            if (ActiveBehavior is not null)
            {
                OnBehaviorSet();
            }
        }

        public override void OnUnmount()
        {
            Refs.ActiveBehavior.Changed -= OnBehaviorSet;
            Unsubscribe(_lastActiveBehavior);
        }

        private void OnBehaviorSet()
        {
            Flash(c => PropFlashColor = c, FlashChangeColor);

            var newBehaviorType = ActiveBehavior?.GetType();
            if (newBehaviorType != _lastBehaviorType)
            {
                Flash(c => TpFlashColor = c, FlashChangeColor);
            }

            Unsubscribe(_lastActiveBehavior);
            Subscribe(ActiveBehavior);
            _lastActiveBehavior = ActiveBehavior;
            _lastBehaviorType = newBehaviorType;
            _hookSubscribersCache.Clear();
            _lastHookSubscribers.Clear();
            if (ActiveBehavior != null)
            {
                _lastState = ActiveBehavior.State;
                _lastValue = GetValueString(ActiveBehavior, "Value");
                _lastTarget = GetValueString(ActiveBehavior, "Target");
                foreach (var hookName in HookNames)
                {
                    _lastHookSubscribers[hookName] = new List<string>(GetHookSubscribers(hookName));
                }
            }
        }

        private void Subscribe(Behavior d)
        {
            if (d is null)
                return;
            d.OnStart += OnBehaviorStart;
            d.OnUpdate += OnBehaviorUpdate;
            d.OnComplete += OnBehaviorComplete;
            d.OnReset += OnBehaviorReset;
        }

        private void Unsubscribe(Behavior d)
        {
            if (d is null)
                return;
            d.OnStart -= OnBehaviorStart;
            d.OnUpdate -= OnBehaviorUpdate;
            d.OnComplete -= OnBehaviorComplete;
            d.OnReset -= OnBehaviorReset;
        }

        private void OnBehaviorStart() => Flash(c => OnStartInvokeFlashColor = c, FlashInvokeColor);

        private void OnBehaviorUpdate(float _) =>
            Flash(c => OnUpdateInvokeFlashColor = c, FlashInvokeColor);

        private void OnBehaviorComplete() =>
            Flash(c => OnCompleteInvokeFlashColor = c, FlashInvokeColor);

        private void OnBehaviorReset() => Flash(c => OnResetInvokeFlashColor = c, FlashInvokeColor);

        private void Flash(Action<Behavior<Color>> setter, Color flashColor, float duration = 1f)
        {
            setter(Animate.FromTo(flashColor, BgColor, duration, Easing.CubicEaseOut));
        }

        public override void OnGUI(Rect calculatedRect)
        {
            Widgets.DrawBoxSolid(calculatedRect, BgColor);
            var prevGuiColor = GUI.color;
            GUI.color = BorderColor;
            Widgets.DrawBox(calculatedRect, 1);
            GUI.color = prevGuiColor;

            var contentRect = new Rect(
                calculatedRect.x + 4f,
                calculatedRect.y + 4f,
                calculatedRect.width - 8f,
                calculatedRect.height - 8f
            );

            if (ActiveBehavior is null || ActiveBehavior is NullBehavior)
            {
                var prevColor = GUI.color;
                GUI.color = LabelColor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(
                    contentRect,
                    ActiveBehavior is NullBehavior ? "NullBehavior" : "ActiveBehavior is null"
                );
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = prevColor;
                return;
            }

            _hookSubscribersCache.Clear();
            CheckForStateChanges();
            RenderCompactView(contentRect);
        }

        private void CheckForStateChanges()
        {
            if (ActiveBehavior is null || ActiveBehavior is NullBehavior)
                return;

            if (ActiveBehavior.State != _lastState)
            {
                Flash(c => StFlashColor = c, FlashChangeColor);
                _lastState = ActiveBehavior.State;
            }

            var currentValue = GetValueString(ActiveBehavior, "Value");
            if (!Equals(currentValue, _lastValue))
            {
                Flash(c => ValueFlashColor = c, FlashChangeColor);
                _lastValue = currentValue;
            }

            var currentTarget = GetValueString(ActiveBehavior, "Target");
            if (!Equals(currentTarget, _lastTarget))
            {
                Flash(c => TargetFlashColor = c, FlashChangeColor);
                _lastTarget = currentTarget;
            }

            Action<Behavior<Color>>[] setters =
            [
                c => OnStartChangeFlashColor = c,
                c => OnUpdateChangeFlashColor = c,
                c => OnCompleteChangeFlashColor = c,
                c => OnResetChangeFlashColor = c,
            ];

            for (var i = 0; i < HookNames.Length; i++)
            {
                var hookName = HookNames[i];
                var currentSubscribers = GetHookSubscribers(hookName);
                _lastHookSubscribers.TryGetValue(hookName, out var lastSubscribers);

                if (lastSubscribers == null || !currentSubscribers.SequenceEqual(lastSubscribers))
                {
                    Flash(setters[i], FlashChangeColor);
                    _lastHookSubscribers[hookName] = new List<string>(currentSubscribers);
                }
            }
        }

        private void RenderCompactView(Rect rect)
        {
            var r = new Rect(rect);
            Text.Font = GameFont.Small;

            var headerRect = new Rect(r.x, r.y, r.width, 20f);
            r.yMin += 20f;

            var idStr = $"{ActiveBehavior.GetHashCode() & 0xFF:X2}";
            var typeName = ActiveBehavior.GetType().Name.Replace("Behavior", "");
            var typeAbbr = (typeName.Length >= 2 ? typeName.Substring(0, 2) : typeName).ToUpper();
            var stateStr = ActiveBehavior.State == Behavior.Lifecycle.Active ? "ACT" : "INA";

            DrawInfoLine(
                ref headerRect,
                ["ID:", "TP:", "ST:"],
                [idStr, typeAbbr, stateStr],
                [PropFlashColor.Value, TpFlashColor.Value, StFlashColor.Value],
                true
            );

            Widgets.DrawLineHorizontal(r.x, r.y, r.width, SeparatorColor);
            r.yMin += 2f;

            var leftColWidth = new[] { Text.CalcSize("V:").x, Text.CalcSize("T:").x }.Max() + 5f;

            var kvRect1 = new Rect(r.x, r.y, r.width, 18f);
            r.yMin += 18f;
            DrawKeyValue(
                kvRect1,
                "T:",
                GetValueString(ActiveBehavior, "Target"),
                leftColWidth,
                TargetFlashColor.Value
            );

            var kvRect2 = new Rect(r.x, r.y, r.width, 18f);
            r.yMin += 18f;
            DrawKeyValue(
                kvRect2,
                "V:",
                GetValueString(ActiveBehavior, "Value"),
                leftColWidth,
                ValueFlashColor.Value
            );

            var flashColors = new[]
            {
                OnStartInvokeFlashColor.Value,
                OnUpdateInvokeFlashColor.Value,
                OnCompleteInvokeFlashColor.Value,
                OnResetInvokeFlashColor.Value,
            };
            var hookAbbr = new[] { "S:", "U:", "C:", "R:" };

            var hooksRect = new Rect(r.x, r.y, r.width, 18f);
            var hookPartWidth = r.width / HookNames.Length;

            for (var i = 0; i < HookNames.Length; i++)
            {
                var hookRect = new Rect(
                    hooksRect.x + i * hookPartWidth,
                    hooksRect.y,
                    hookPartWidth,
                    hooksRect.height
                );

                var bgColor = BgColorShade;
                Widgets.DrawBoxSolid(hookRect, bgColor);

                var labelWidth = Text.CalcSize(hookAbbr[i]).x + 2f;
                var labelRect = new Rect(hookRect.x, hookRect.y, labelWidth, hookRect.height);

                var padding = 2f;
                var valueRect = new Rect(
                    hookRect.x + labelWidth + padding,
                    hookRect.y,
                    hookRect.width - labelWidth - padding,
                    hookRect.height
                );

                var changeFlashColors = new[]
                {
                    OnStartChangeFlashColor.Value,
                    OnUpdateChangeFlashColor.Value,
                    OnCompleteChangeFlashColor.Value,
                    OnResetChangeFlashColor.Value,
                };

                DrawLabel(labelRect, hookAbbr[i], changeFlashColors[i]);

                if (flashColors[i] != BgColor)
                {
                    Widgets.DrawBoxSolid(valueRect, flashColors[i]);
                }

                var subscribers = GetHookSubscribers(HookNames[i]);
                if (subscribers.Any())
                {
                    DrawSubscriberDots(valueRect, subscribers.Count);
                }
                else
                {
                    DrawValue(valueRect, "-", LabelColor);
                }

                var tooltipText = subscribers.Any() ? string.Join("\n", subscribers) : "(none)";
                TooltipHandler.TipRegion(hookRect, tooltipText);
            }

            Text.Font = GameFont.Medium;
        }

        private void DrawSubscriberDots(Rect rect, int count)
        {
            const int dotsPerRow = 3;
            const int rowsPerSquare = 3;
            const int dotsPerSquare = dotsPerRow * rowsPerSquare;

            var prevFont = Text.Font;
            Text.Font = GameFont.Tiny;
            var dotSize = Text.CalcSize("•");
            Text.Font = prevFont;

            float dotSpacing = 0f;
            float squareSpacing = 2f;

            var prevColor = GUI.color;
            var prevAnchor = Text.Anchor;

            GUI.color = HookSubscriberColor;
            Text.Anchor = TextAnchor.LowerLeft;

            for (var i = 0; i < count; i++)
            {
                var squareIndex = i / dotsPerSquare;
                var dotInSquareIndex = i % dotsPerSquare;

                var rowIndex = dotInSquareIndex / dotsPerRow;
                var colIndex = dotInSquareIndex % dotsPerRow;

                var squareWidth = dotsPerRow * (dotSize.x + dotSpacing) - dotSpacing;

                var squareBaseX = rect.x + squareIndex * (squareWidth + squareSpacing);
                var dotX = squareBaseX + colIndex * (dotSize.x + dotSpacing);
                var dotY = rect.yMax - (rowIndex * (dotSize.y + dotSpacing));

                var dotRect = new Rect(dotX, dotY - dotSize.y, dotSize.x, dotSize.y);

                if (dotRect.xMax > rect.xMax)
                {
                    break;
                }

                Widgets.Label(dotRect, "•");
            }

            GUI.color = prevColor;
            Text.Anchor = prevAnchor;
        }

        private void DrawInfoLine(
            ref Rect rect,
            string[] labels,
            string[] values,
            Color[] bgColors,
            bool spaceBetween = false
        )
        {
            var prevAnchor = Text.Anchor;

            if (spaceBetween)
            {
                var part1Width = Text.CalcSize(labels[0]).x + Text.CalcSize(values[0]).x + 10f;
                var part3Width = Text.CalcSize(labels[2]).x + Text.CalcSize(values[2]).x + 10f;

                var part1Rect = new Rect(rect.x, rect.y, part1Width, rect.height);
                var part3Rect = new Rect(rect.xMax - part3Width, rect.y, part3Width, rect.height);
                var part2Rect = new Rect(
                    part1Rect.xMax,
                    rect.y,
                    rect.xMax - part1Rect.xMax - part3Width,
                    rect.height
                );

                var label1Size = Text.CalcSize(labels[0]);
                DrawLabel(
                    new Rect(part1Rect.x, part1Rect.y, label1Size.x, part1Rect.height),
                    labels[0],
                    bgColors[0]
                );
                DrawValue(
                    new Rect(
                        part1Rect.x + label1Size.x + 2f,
                        part1Rect.y,
                        part1Rect.width - label1Size.x - 2f,
                        part1Rect.height
                    ),
                    values[0]
                );

                Text.Anchor = TextAnchor.MiddleCenter;
                var label2Size = Text.CalcSize(labels[1]);
                DrawLabel(
                    new Rect(
                        part2Rect.center.x - (label2Size.x + Text.CalcSize(values[1]).x) / 2f,
                        part2Rect.y,
                        label2Size.x,
                        part2Rect.height
                    ),
                    labels[1],
                    bgColors[1]
                );
                DrawValue(
                    new Rect(
                        part2Rect.center.x
                            - (label2Size.x + Text.CalcSize(values[1]).x) / 2f
                            + label2Size.x
                            + 2f,
                        part2Rect.y,
                        Text.CalcSize(values[1]).x,
                        part2Rect.height
                    ),
                    values[1]
                );
                Text.Anchor = prevAnchor;

                var label3Size = Text.CalcSize(labels[2]);
                DrawLabel(
                    new Rect(part3Rect.x, part3Rect.y, label3Size.x, part3Rect.height),
                    labels[2],
                    bgColors[2]
                );
                DrawValue(
                    new Rect(
                        part3Rect.x + label3Size.x + 2f,
                        part3Rect.y,
                        part3Rect.width - label3Size.x - 2f,
                        part3Rect.height
                    ),
                    values[2]
                );
            }
            else
            {
                var currentX = rect.x;
                for (int i = 0; i < labels.Length; i++)
                {
                    var label = labels[i] + ": ";
                    var labelSize = Text.CalcSize(label);
                    var valueSize = Text.CalcSize(values[i]);
                    var totalWidth = labelSize.x + valueSize.x + 10f;

                    var itemRect = new Rect(currentX, rect.y, totalWidth, rect.height);
                    currentX += totalWidth;

                    DrawLabel(
                        new Rect(itemRect.x, itemRect.y, labelSize.x, itemRect.height),
                        label,
                        bgColors[i]
                    );
                    DrawValue(
                        new Rect(
                            itemRect.x + labelSize.x + 2f,
                            itemRect.y,
                            itemRect.width - labelSize.x - 2f,
                            itemRect.height
                        ),
                        values[i]
                    );
                }
            }
            Text.Anchor = prevAnchor;
        }

        private void DrawKeyValue(
            Rect rect,
            string label,
            string value,
            float labelWidth,
            Color? bgColor = null
        )
        {
            var labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            var padding = 2f;
            var valueRect = new Rect(
                rect.x + labelWidth + padding,
                rect.y,
                rect.width - labelWidth - padding,
                rect.height
            );
            DrawLabel(labelRect, label, bgColor);
            DrawValue(valueRect, value, ValueColor);
        }

        private void DrawLabel(Rect rect, string text, Color? bgColor = null)
        {
            if (bgColor.HasValue && bgColor.Value != BgColor)
            {
                Widgets.DrawBoxSolid(rect, bgColor.Value);
            }
            var prevAnchor = Text.Anchor;
            var prevColor = GUI.color;
            GUI.color = LabelColor;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect, text);
            Text.Anchor = prevAnchor;
            GUI.color = prevColor;
        }

        private void DrawValue(Rect rect, string text, Color? textColor = null)
        {
            var prevAnchor = Text.Anchor;
            var prevColor = GUI.color;
            GUI.color = textColor ?? ValueColor;
            Text.Anchor = TextAnchor.MiddleLeft;
            var clipped =
                Text.CalcSize(text).x > rect.width
                    ? text.Substring(0, Mathf.Min(text.Length, 256)) + "..."
                    : text;
            Widgets.Label(rect, clipped);
            Text.Anchor = prevAnchor;
            GUI.color = prevColor;
        }

        private List<string> GetHookSubscribers(string hookName)
        {
            if (_hookSubscribersCache.TryGetValue(hookName, out var cached))
            {
                return cached;
            }

            if (ActiveBehavior is null || ActiveBehavior is NullBehavior)
                return new List<string>();

            var hookProp = ActiveBehavior.GetType().GetProperty(hookName);
            if (hookProp is null)
                return new List<string>();

            var hookDelegate = hookProp.GetValue(ActiveBehavior) as MulticastDelegate;
            if (hookDelegate is null)
                return new List<string>();

            var invocationList = hookDelegate.GetInvocationList();
            var subscribers = new List<string>();

            foreach (var callback in invocationList)
            {
                var target = callback.Target;
                var method = callback.Method;
                string targetInfo;

                if (target is null)
                {
                    targetInfo = method.DeclaringType?.Name ?? "Static";
                }
                else
                {
                    var targetType = target.GetType().Name;
                    var targetId = $"{target.GetHashCode() & 0xFF:X2}";
                    targetInfo = $"{targetType}[{targetId}]";
                }

                var fullMethodName = $"{targetInfo}.{method.Name}";
                var safeMethodName = fullMethodName.Replace('<', '[').Replace('>', ']');
                subscribers.Add(safeMethodName);
            }

            _hookSubscribersCache[hookName] = subscribers;
            return subscribers;
        }

        private string GetValueString(Behavior behavior, string propName)
        {
            if (behavior is null || behavior is NullBehavior)
                return "N/A";

            var prop = behavior.GetType().GetProperty(propName);
            if (prop == null)
                return "N/A";
            var value = prop.GetValue(behavior);
            return value?.ToString() ?? "null";
        }
    }
}
