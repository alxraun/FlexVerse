// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ScenarioSwitcher : Widget
    {
        private readonly Dictionary<Type, string> _descriptions = [];

        [Prop]
        public Behavior<List<Type>> Scenarios { get; set; }

        [Prop]
        public Behavior<Action<Type>> OnSelect { get; set; }

        [Prop]
        public Behavior<string> Prefix { get; set; }

        [Prop]
        public Behavior<string> Suffix { get; set; }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            if (Scenarios.Value is { Count: > 0 })
                return
                [
                    new View
                    {
                        FlexDirection = FlexDirections.Row,
                        FlexWrap = Wrap.Wrap,
                        AlignItems = Align.Stretch,
                        Width = 100f.Percent(),
                        PaddingAll = 5f.Point(),
                        Children = BuildButtons(),
                    },
                ];
            else
                return [];
        }

        private WidgetList BuildButtons()
        {
            var (prefix, suffix) = GetAffixes();

            var list = new WidgetList();
            foreach (var scenarioType in Scenarios.Value)
            {
                list.Add(
                    new Button
                    {
                        FlexGrow = 1,
                        Label = FormatScenarioName(scenarioType.Name, prefix, suffix),
                        OnClick = (Action)(() => OnSelect.Value.Invoke(scenarioType)),
                        Tooltip = GetScenarioDescription(scenarioType),
                    }
                );
            }
            return list;
        }

        private (string prefix, string suffix) GetAffixes()
        {
            var prefix =
                Prefix?.Value != null
                    ? Prefix.Value
                    : CalculateCommonAffixes(Scenarios.Value).prefix;

            var suffix =
                Suffix?.Value != null
                    ? Suffix.Value
                    : CalculateCommonAffixes(Scenarios.Value).suffix;

            return (prefix, suffix);
        }

        private (string prefix, string suffix) CalculateCommonAffixes(IReadOnlyList<Type> scenarios)
        {
            var scenarioNames = scenarios.Select(t => t.Name).ToList();
            var commonPrefix = FindCommonPrefix(scenarioNames);
            var commonSuffix = FindCommonSuffix(scenarioNames);
            return (commonPrefix, commonSuffix);
        }

        private string FormatScenarioName(string name, string prefix, string suffix)
        {
            if (!string.IsNullOrEmpty(prefix) && name.StartsWith(prefix))
                name = name.Substring(prefix.Length);

            if (!string.IsNullOrEmpty(suffix) && name.EndsWith(suffix))
                name = name.Substring(0, name.Length - suffix.Length);

            return name;
        }

        private string FindCommonPrefix(IReadOnlyList<string> strings)
        {
            if (strings == null || strings.Count == 0)
                return "";
            if (strings.Count == 1)
                return "";

            var first = strings[0];
            if (first.Length == 0 || !char.IsUpper(first[0]))
                return "";

            int maxPrefixLength = 0;

            for (int i = 1; i <= first.Length; i++)
            {
                if (i == first.Length || char.IsUpper(first[i]))
                {
                    var candidatePrefix = first.Substring(0, i);
                    if (AllStringsStartWith(strings, candidatePrefix))
                        maxPrefixLength = i;
                    else
                        break;
                }
            }

            return maxPrefixLength > 0 ? first.Substring(0, maxPrefixLength) : "";
        }

        private bool AllStringsStartWith(IReadOnlyList<string> strings, string prefix)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i].Length < prefix.Length || !strings[i].StartsWith(prefix))
                    return false;
            }
            return true;
        }

        private string FindCommonSuffix(IReadOnlyList<string> strings)
        {
            if (strings == null || strings.Count == 0)
                return "";
            if (strings.Count == 1)
                return "";

            var first = strings[0];
            if (first.Length == 0)
                return "";

            int maxSuffixLength = 0;

            for (int i = first.Length; i >= 1; i--)
            {
                if (char.IsUpper(first[i - 1]))
                {
                    var candidateSuffix = first.Substring(i - 1);
                    if (AllStringsEndWith(strings, candidateSuffix))
                        maxSuffixLength = Math.Max(maxSuffixLength, candidateSuffix.Length);
                }
            }

            return maxSuffixLength > 0 ? first.Substring(first.Length - maxSuffixLength) : "";
        }

        private bool AllStringsEndWith(IReadOnlyList<string> strings, string suffix)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i].Length < suffix.Length || !strings[i].EndsWith(suffix))
                    return false;
            }
            return true;
        }

        private string GetScenarioDescription(Type scenarioType)
        {
            if (_descriptions.TryGetValue(scenarioType, out var cached))
                return cached;

            var propInfo = scenarioType.GetProperty(
                "Description",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
            );
            var description = propInfo?.GetValue(null) is string rawDescription
                ? TrimEmptyLines(rawDescription)
                : "Description not found.";

            _descriptions[scenarioType] = description;
            return description;
        }

        private string TrimEmptyLines(string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var startIndex = 0;
            var endIndex = lines.Length - 1;

            while (startIndex < lines.Length && string.IsNullOrWhiteSpace(lines[startIndex]))
                startIndex++;

            while (endIndex >= startIndex && string.IsNullOrWhiteSpace(lines[endIndex]))
                endIndex--;

            if (startIndex > endIndex)
                return string.Empty;

            var originalLineBreak = text.Contains("\r\n") ? "\r\n" : "\n";
            var trimmedLines = lines.Skip(startIndex).Take(endIndex - startIndex + 1);
            return string.Join(originalLineBreak, trimmedLines);
        }
    }
}
