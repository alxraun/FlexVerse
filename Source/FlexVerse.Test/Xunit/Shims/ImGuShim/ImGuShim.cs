// parents: @.frostline/graph/m-xunit.xml

using System;

namespace UnityEngine
{
    [Flags]
    public enum EventModifiers
    {
        None = 0,
        Shift = 1,
        Control = 2,
        Alt = 4,
        Command = 8,
        Numeric = 16,
        CapsLock = 32,
        FunctionKey = 64,
    }

    public enum EventType
    {
        MouseDown = 0,
        MouseUp = 1,
        MouseDrag = 2,
        KeyDown = 3,
        KeyUp = 4,
        ScrollWheel = 5,
        Repaint = 6,
        Layout = 7,
        DragUpdated = 8,
        DragPerform = 9,
        DragExited = 10,
        Ignore = 11,
        Used = 12,
        ValidateCommand = 13,
        ExecuteCommand = 14,
        ContextClick = 15,
        MouseEnterWindow = 16,
        MouseLeaveWindow = 17,
        TouchDown = 18,
        TouchUp = 19,
        TouchMove = 20,
        TouchEnter = 21,
        TouchLeave = 22,
        TouchStationary = 23,
        TouchPhaseChanged = 24,
        MouseMove = 25,
    }

    public class Event
    {
        public static Event current => null;

        private EventType _type;
        private Vector2 _mousePosition = Vector2.zero;
        private Vector2 _delta = Vector2.zero;
        private int _button;
        private KeyCode _keyCode;
        private char _character;
        private EventModifiers _modifiers;
        private int _clickCount;

        public EventType type
        {
            get => _type;
            set => _type = value;
        }
        public Vector2 mousePosition
        {
            get => _mousePosition;
            set => _mousePosition = value;
        }
        public Vector2 delta
        {
            get => _delta;
            set => _delta = value;
        }
        public bool isMouse =>
            _type == EventType.MouseDown
            || _type == EventType.MouseUp
            || _type == EventType.MouseMove
            || _type == EventType.MouseDrag
            || _type == EventType.ContextClick
            || _type == EventType.ScrollWheel;

        public bool isKey => _type == EventType.KeyDown || _type == EventType.KeyUp;
        public int button
        {
            get => _button;
            set => _button = value;
        }
        public KeyCode keyCode
        {
            get => _keyCode;
            set => _keyCode = value;
        }
        public char character
        {
            get => _character;
            set => _character = value;
        }
        public EventModifiers modifiers
        {
            get => _modifiers;
            set => _modifiers = value;
        }
        public int clickCount
        {
            get => _clickCount;
            set => _clickCount = value;
        }

        public void Use()
        {
            type = EventType.Used;
        }
    }

    public class GUIStyleState
    {
        public Texture2D background { get; set; }
    }

    public class GUIStyle
    {
        public GUIStyleState normal { get; } = new();
        public GUIStyleState hover { get; } = new();
        public GUIStyleState focused { get; } = new();
        public GUIStyleState active { get; } = new();
        public GUIStyleState onNormal { get; } = new();
        public GUIStyleState onHover { get; } = new();
        public GUIStyleState onFocused { get; } = new();
        public GUIStyleState onActive { get; } = new();

        public GUIStyle() { }

        public GUIStyle(GUIStyle source)
        {
            alignment = source.alignment;
            fontStyle = source.fontStyle;
            fontSize = source.fontSize;
            normalBackground = source.normalBackground;
            normalTextColor = source.normalTextColor;
        }

        public TextAnchor alignment { get; set; }
        public FontStyle fontStyle;
        public float fontSize;
        public string normalBackground;
        public Color normalTextColor;

        public Vector2 CalcSize(GUIContent content) => Vector2.zero;
    }

    public class GUIContent
    {
        public string text;
        public string tooltip;
        public Texture image;

        public GUIContent() { }

        public GUIContent(string text)
        {
            this.text = text;
        }

        public GUIContent(string text, string tooltip)
        {
            this.text = text;
            this.tooltip = tooltip;
        }

        public GUIContent(Texture image)
        {
            this.image = image;
        }
    }

    public static class GUI
    {
        private static Color _color = Color.white;
        private static bool _enabled = true;
        private static Matrix4x4 _matrix = Matrix4x4.identity;

        public static Matrix4x4 matrix
        {
            get => _matrix;
            set => _matrix = value;
        }

        public static Color color
        {
            get => _color;
            set => _color = value;
        }
        public static bool enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public static void Label(Rect rect, GUIContent content) { }

        public static void Label(Rect rect, GUIContent content, GUIStyle style) { }

        public static void Label(Rect rect, string text) { }

        public static void Label(Rect rect, Texture image) { }

        public static void DrawTexture(Rect rect, Texture image) { }

        public static void DrawTexture(Rect rect, Texture image, ScaleMode scaleMode) { }

        public static void DrawTextureWithTexCoords(Rect rect, Texture image, Rect texCoords) { }

        public static void Box(Rect rect, GUIContent content) { }

        private static int _groupDepth;
        private static readonly System.Collections.Generic.Stack<Vector2> GroupOrigins = new();
        internal static Vector2 GroupOrigin { get; private set; }

        public static void BeginGroup(Rect rect)
        {
            GroupOrigins.Push(GroupOrigin);
            GroupOrigin += rect.position;
            _groupDepth++;
        }

        public static void EndGroup()
        {
            if (_groupDepth <= 0)
                throw new InvalidOperationException("Unbalanced GUI.EndGroup");
            _groupDepth--;
            GroupOrigin = GroupOrigins.Pop();
        }

        public static bool Button(Rect rect, GUIContent content) => false;

        public static string TextField(Rect rect, string text) => text;

        public static GUIStyle LastTextFieldStyle;

        public static string TextField(Rect rect, string text, int maxLength, GUIStyle style)
        {
            LastTextFieldStyle = style;
            return text;
        }

        public static void SetNextControlName(string name) { }

        public static string GetNameOfFocusedControl() => _focusedControl;

        public static void FocusControl(string name) => _focusedControl = name ?? string.Empty;

        private static string _focusedControl = string.Empty;
    }

    public enum ScaleMode
    {
        StretchToFill = 0,
        ScaleAndCrop = 1,
        ScaleToFit = 2,
    }

    public enum FocusType
    {
        Passive = 0,
        Keyboard = 1,
    }

    public static class GUIUtility
    {
        public static Vector2 GUIToScreenPoint(Vector2 point)
        {
            var transformed = GUI.matrix.MultiplyPoint3x4(new Vector3(point.x, point.y, 0f));
            return new Vector2(transformed.x, transformed.y) + GUI.GroupOrigin;
        }

        public static int GetControlID(int hint) => 0;

        public static int GetControlID(int hint, FocusType focus) => 0;

        public static int GetControlID(GUIContent content, FocusType focus, Rect rect) => 0;
    }
}
