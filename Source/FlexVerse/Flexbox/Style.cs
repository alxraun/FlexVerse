// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

using System;
using System.Collections.Generic;

namespace FlexVerse.Flexbox
{
    /// <summary>Stores a node's flexbox layout properties as parsed attribute values.</summary>
    public class Style
    {
        internal Directions Direction = Directions.Inherit;
        internal FlexDirections FlexDirection = FlexDirections.Row;
        internal Justify JustifyContent = Justify.FlexStart;
        internal Align AlignContent = Align.Stretch;
        internal Align AlignItems = Align.Stretch;
        internal Align AlignSelf = Align.Auto;
        internal PositionTypes PositionType = PositionTypes.Relative;
        internal Wrap FlexWrap = Wrap.NoWrap;
        internal Overflows Overflow = Overflows.Visible;
        internal Displays Display = Displays.Flex;
        internal float FlexGrow = 0f;
        internal float FlexShrink = 1f;
        internal Length FlexBasis = CreateAutoValue();
        internal Length[] Margin = CreateDefaultEdgeValuesUnit();
        internal Length[] Position = CreateDefaultEdgeValuesUnit();
        internal Length[] Padding = CreateDefaultEdgeValuesUnit();
        internal Length[] Border = CreateDefaultEdgeValuesUnit();
        internal Length[] Dimensions = [CreateAutoValue(), CreateAutoValue()];
        internal Length[] MinDimensions = [Length.Undefined, Length.Undefined];
        internal Length[] MaxDimensions = [Length.Undefined, Length.Undefined];

        internal float AspectRatio = float.NaN;

        protected static readonly Dictionary<string, string> layoutAttributeDefault = new()
        {
            { "display", "flex" },
            { "overflow", "visible" },
            { "position", "relative" },
            { "align-content", "stretch" },
            { "align-items", "stretch" },
            { "align-self", "auto" },
            { "flex-direction", "row" },
            { "flex-wrap", "no-wrap" },
            { "flex-basis", "auto" },
            { "flex-shrink", "1" },
            { "flex-grow", "0" },
            { "justify-content", "flex-start" },
            { "direction", "inherit" },
            { "left", "auto" },
            { "top", "auto" },
            { "right", "auto" },
            { "bottom", "auto" },
            { "width", "auto" },
            { "height", "auto" },
            { "min-width", "auto" },
            { "min-height", "auto" },
            { "max-width", "auto" },
            { "max-height", "auto" },
            { "margin", "skip" },
            { "margin-left", "0" },
            { "margin-right", "0" },
            { "margin-top", "0" },
            { "margin-bottom", "0" },
            { "padding", "skip" },
            { "padding-left", "0" },
            { "padding-right", "0" },
            { "padding-top", "0" },
            { "padding-bottom", "0" },
            { "border-width", "skip" },
            { "border-left-width", "0" },
            { "border-right-width", "0" },
            { "border-top-width", "0" },
            { "border-bottom-width", "0" },
        };

        protected static readonly Dictionary<string, string> layoutAttributeInherit = new()
        {
            { "direction", "ltr" },
        };

        private bool forceLayoutDirty = false;

        /// <summary>Indicates whether the style changed since the last layout pass.</summary>
        public bool layoutDirty
        {
            get { return forceLayoutDirty || layoutAttributeChanged.Count > 0; }
            set
            {
                forceLayoutDirty = value;
                if (!value)
                    layoutAttributeChanged.Clear();
            }
        }

        protected bool setMode = false;

        protected readonly Dictionary<string, string> layoutAttribute = [];

        protected readonly Dictionary<string, string> layoutAttributeChanged = [];

        protected readonly Dictionary<string, string> layoutAttributeWas = [];

        protected readonly Dictionary<string, string> layoutAttributeAnimated = [];

        private static Dictionary<string, int> edgeNameToId = new()
        {
            { "left", (int)Edge.Left },
            { "top", (int)Edge.Top },
            { "right", (int)Edge.Right },
            { "bottom", (int)Edge.Bottom },
        };

        /// <summary>Reads or writes a layout attribute by its CSS-style name.</summary>
        public virtual string this[string attr]
        {
            get
            {
                if (layoutAttributeAnimated.ContainsKey(attr))
                    return layoutAttributeAnimated[attr];
                if (!layoutAttributeDefault.ContainsKey(attr))
                    throw new Exception("Try to get unknown layout style attribute [" + attr + "]");
                string value = layoutAttribute.ContainsKey(attr)
                    ? layoutAttribute[attr]
                    : layoutAttributeDefault[attr];
                if (layoutAttributeInherit.ContainsKey(attr) && value == "inherit")
                {
                    value = layoutAttributeInherit[attr];
                }
                return value;
            }
            set
            {
                value = value.Trim();

                if (attr == "margin" || attr == "padding" || attr == "border-width")
                {
                    var tail = attr == "border-width" ? "-width" : "";
                    var name = attr == "border-width" ? "border" : attr;
                    if (Flex.ParseFourValueFromString(value, out var vals))
                        foreach (var kv in edgeNameToId)
                            this[name + "-" + kv.Key + tail] =
                                vals[kv.Value]
                                    .value.ToString(
                                        "F",
                                        System.Globalization.CultureInfo.InvariantCulture
                                    ) + (vals[kv.Value].unit == Unit.Percent ? "%" : "");
                    else
                        throw new Exception(
                            "Failed to parse attribute [" + attr + ":" + value + "]"
                        );

                    return;
                }

                if (!layoutAttributeDefault.ContainsKey(attr))
                    throw new Exception("Try to set unknown layout style attribute [" + attr + "]");
                if (setMode)
                {
                    var old_value = layoutAttributeWas.ContainsKey(attr)
                        ? layoutAttributeWas[attr]
                        : layoutAttributeDefault[attr];
                    if (value != old_value)
                        layoutAttributeChanged[attr] = old_value;
                    else if (layoutAttributeChanged.ContainsKey(attr))
                        layoutAttributeChanged.Remove(attr);
                }
                else
                {
                    layoutAttributeChanged[attr] = this[attr];
                }
                layoutAttribute[attr] = value;
                if (!Flex.ParseStyleAttr(this, attr, value))
                    throw new Exception("Failed to parse attribute [" + attr + ":" + value + "]");
            }
        }

        /// <summary>Applies animated attribute values on top of the current style.</summary>
        public virtual void ApplyAnimation(Dictionary<string, string> animated_attrs = null)
        {
            var prev_animation = new Dictionary<string, string>(layoutAttributeAnimated);
            layoutAttributeAnimated.Clear();
            if (animated_attrs != null)
            {
                foreach (var kv in animated_attrs)
                {
                    var attr = kv.Key;
                    var prev_value = prev_animation.ContainsKey(attr)
                        ? prev_animation[attr]
                        : this[attr];
                    if (prev_value != kv.Value)
                        layoutAttributeChanged[attr] = prev_value;
                    layoutAttributeAnimated[attr] = kv.Value;
                }
            }
            Sync();
        }

        protected virtual void Sync()
        {
            SetDefault(false);
            foreach (var kv in layoutAttribute)
                if (!Flex.ParseStyleAttr(this, kv.Key, kv.Value))
                    throw new Exception(
                        "Failed to parse attribute [" + kv.Key + ":" + kv.Value + "]"
                    );

            foreach (var kv in layoutAttributeAnimated)
                if (!Flex.ParseStyleAttr(this, kv.Key, kv.Value))
                    throw new Exception(
                        "Failed to parse attribute [" + kv.Key + ":" + kv.Value + "]"
                    );
        }

        protected virtual void CleanupLayoutAttributeChanged()
        {
            var lac = new Dictionary<string, string>(layoutAttributeChanged);
            foreach (var kv in lac)
            {
                var new_val = (
                    layoutAttribute.ContainsKey(kv.Key)
                        ? layoutAttribute[kv.Key]
                        : layoutAttributeDefault[kv.Key]
                );
                if (kv.Value == new_val)
                    layoutAttributeChanged.Remove(kv.Key);
            }
        }

        /// <summary>Returns the attributes that changed since the last sync.</summary>
        public virtual Dictionary<string, string> GetChangedAttributes()
        {
            return new Dictionary<string, string>(layoutAttributeChanged);
        }

        /// <summary>Replaces the style with the given CSS-style declarations.</summary>
        public virtual void Set(string style)
        {
            layoutAttributeWas.Clear();
            foreach (var kv in layoutAttributeChanged)
                layoutAttributeWas[kv.Key] = kv.Value;
            foreach (var kv in layoutAttribute)
                layoutAttributeWas[kv.Key] = kv.Value;

            setMode = true;
            SetDefault();
            Apply(style);
            foreach (var kv in layoutAttributeWas)
                if (!layoutAttribute.ContainsKey(kv.Key))
                    layoutAttributeChanged[kv.Key] = layoutAttributeWas.ContainsKey(kv.Key)
                        ? layoutAttributeWas[kv.Key]
                        : layoutAttributeDefault[kv.Key];

            setMode = false;
            CleanupLayoutAttributeChanged();
        }

        /// <summary>Applies the given CSS-style declarations onto the current style.</summary>
        public virtual void Apply(string style)
        {
            if (style.Trim() != "")
                Parse(style);
            CleanupLayoutAttributeChanged();
        }

        /// <summary>Parses semicolon-separated CSS-style declarations into attributes.</summary>
        public virtual void Parse(string style)
        {
            var items = style.Split(';');
            foreach (var item in items)
            {
                if (item.Trim() == "")
                    continue;
                var part = item.Trim().Split(':');
                if (part.Length == 2)
                    this[part[0].Trim()] = part[1].Trim();
                else
                    throw new System.Exception(
                        "Failed to parse style [" + item + "] in  [" + style + "]"
                    );
            }
        }

        /// <summary>Resets all layout properties to their default values.</summary>
        public virtual void SetDefault(bool clear_text_values = true)
        {
            if (clear_text_values)
            {
                layoutAttributeChanged.Clear();
                layoutAttribute.Clear();
            }
            Direction = Directions.Inherit;
            FlexDirection = FlexDirections.Row;
            JustifyContent = Justify.FlexStart;
            AlignContent = Align.Stretch;
            AlignItems = Align.Stretch;
            AlignSelf = Align.Auto;
            PositionType = PositionTypes.Relative;
            FlexWrap = Wrap.NoWrap;
            Overflow = Overflows.Visible;
            Display = Displays.Flex;
            FlexGrow = 0f;
            FlexShrink = 1f;
            FlexBasis = CreateAutoValue();
            Margin = CreateDefaultEdgeValuesUnit();
            Position = CreateDefaultEdgeValuesUnit();
            Padding = CreateDefaultEdgeValuesUnit();
            Border = CreateDefaultEdgeValuesUnit();
            Dimensions = [CreateAutoValue(), CreateAutoValue()];
            MinDimensions = [Length.Undefined, Length.Undefined];
            MaxDimensions = [Length.Undefined, Length.Undefined];

            AspectRatio = float.NaN;
        }

        /// <summary>Copies all style values from one style to another.</summary>
        public static void Copy(Style dest, Style src)
        {
            dest.Direction = src.Direction;
            dest.FlexDirection = src.FlexDirection;
            dest.JustifyContent = src.JustifyContent;
            dest.AlignContent = src.AlignContent;
            dest.AlignItems = src.AlignItems;
            dest.AlignSelf = src.AlignSelf;
            dest.PositionType = src.PositionType;
            dest.FlexWrap = src.FlexWrap;
            dest.Overflow = src.Overflow;
            dest.Display = src.Display;
            dest.FlexGrow = src.FlexGrow;
            dest.FlexShrink = src.FlexShrink;
            dest.FlexBasis = src.FlexBasis;

            Array.Copy(src.Margin, dest.Margin, src.Margin.Length);
            Array.Copy(src.Position, dest.Position, src.Position.Length);
            Array.Copy(src.Padding, dest.Padding, src.Padding.Length);
            Array.Copy(src.Border, dest.Border, src.Border.Length);

            Array.Copy(src.Dimensions, dest.Dimensions, src.Dimensions.Length);
            Array.Copy(src.MinDimensions, dest.MinDimensions, src.MinDimensions.Length);
            Array.Copy(src.MaxDimensions, dest.MaxDimensions, src.MaxDimensions.Length);

            dest.AspectRatio = src.AspectRatio;
        }

        internal static Length CreateAutoValue()
        {
            return new Length(float.NaN, Unit.Auto);
        }

        internal static Length[] CreateDefaultEdgeValuesUnit()
        {
            return
            [
                Length.Undefined,
                Length.Undefined,
                Length.Undefined,
                Length.Undefined,
                Length.Undefined,
                Length.Undefined,
                Length.Undefined,
                Length.Undefined,
                Length.Undefined,
            ];
        }

        /// <summary>Returns a copy of this style.</summary>
        public virtual Style Clone()
        {
            var clone = new Style();
            Copy(this, clone);
            return clone;
        }
    }
}
