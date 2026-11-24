// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Reflection;

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; }

        public static void DontDestroyOnLoad(Object target) { }

        public static void Destroy(Object target) { }

        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);

        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);

        public override bool Equals(object obj) => ReferenceEquals(this, obj);

        public override int GetHashCode() =>
            System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

        public static implicit operator bool(Object o) => o != null;
    }

    public class Component : Object
    {
        public GameObject gameObject => new GameObject(name);
        public Transform transform => null;
    }

    public class Behaviour : Component { }

    public class Texture : Object
    {
        public FilterMode filterMode { get; set; }
        public TextureWrapMode wrapMode { get; set; }
        private int _width;
        private int _height;

        public int width
        {
            get => _width;
            set => _width = value;
        }
        public int height
        {
            get => _height;
            set => _height = value;
        }
    }

    public enum TextureFormat
    {
        RGBA32 = 4,
    }

    public enum FilterMode
    {
        Point = 0,
        Bilinear = 1,
        Trilinear = 2,
    }

    public enum TextureWrapMode
    {
        Repeat = 0,
        Clamp = 1,
    }

    public class Texture2D : Texture
    {
        private Color[] _pixels;
        public TextureFormat format { get; }

        public Texture2D(int width, int height, TextureFormat format, bool mipChain)
        {
            this.width = width;
            this.height = height;
            this.format = format;
            _pixels = new Color[width * height];
        }

        public void SetPixels(Color[] pixels)
        {
            if (pixels == null || pixels.Length != width * height)
                throw new ArgumentException("Texture dimensions do not match SetPixels.");
            _pixels = (Color[])pixels.Clone();
        }

        public Color[] GetPixels() => (Color[])_pixels.Clone();

        public Color GetPixel(int x, int y) => _pixels[y * width + x];

        public void Apply(bool updateMipmaps = true, bool makeNoLongerReadable = false) { }
    }

    public class MonoBehaviour : Behaviour { }

    public class Transform : Component { }

    public class GameObject : Object
    {
        public GameObject() { }

        public GameObject(string name)
        {
            this.name = name;
        }

        public Transform transform => null;

        public T AddComponent<T>()
            where T : Component
        {
            var component = (T)Activator.CreateInstance(typeof(T));

            var awake =
                typeof(T).GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                )
                ?? typeof(T).BaseType?.GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            awake?.Invoke(component, null);
            return component;
        }
    }

    public enum KeyCode : int
    {
        None = 0,
        Backspace = 8,
        Tab = 9,
        Return = 13,
        Escape = 27,
        Space = 32,
        A = 97,
        B = 98,
        C = 99,
        D = 100,
        E = 101,
        F = 102,
        LeftArrow = 276,
        RightArrow = 275,
        UpArrow = 273,
        DownArrow = 274,
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator -(Vector3 value) =>
            new Vector3(-value.x, -value.y, -value.z);
    }

    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        private Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion Euler(float x, float y, float z)
        {
            var half = z * Math.PI / 360.0;
            return new Quaternion(0f, 0f, (float)Math.Sin(half), (float)Math.Cos(half));
        }
    }

    public struct Matrix4x4
    {
        public float m00,
            m01,
            m03;
        public float m10,
            m11,
            m13;
        public float m22,
            m33;

        public static Matrix4x4 identity =>
            new Matrix4x4
            {
                m00 = 1f,
                m11 = 1f,
                m22 = 1f,
                m33 = 1f,
            };

        public static Matrix4x4 Translate(Vector3 value)
        {
            var result = identity;
            result.m03 = value.x;
            result.m13 = value.y;
            return result;
        }

        public static Matrix4x4 Scale(Vector3 value)
        {
            var result = identity;
            result.m00 = value.x;
            result.m11 = value.y;
            result.m22 = value.z;
            return result;
        }

        public static Matrix4x4 Rotate(Quaternion q)
        {
            var result = identity;
            var cos = 1f - 2f * q.z * q.z;
            var sin = 2f * q.z * q.w;
            result.m00 = cos;
            result.m01 = -sin;
            result.m10 = sin;
            result.m11 = cos;
            return result;
        }

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b) =>
            new Matrix4x4
            {
                m00 = a.m00 * b.m00 + a.m01 * b.m10,
                m01 = a.m00 * b.m01 + a.m01 * b.m11,
                m03 = a.m00 * b.m03 + a.m01 * b.m13 + a.m03,
                m10 = a.m10 * b.m00 + a.m11 * b.m10,
                m11 = a.m10 * b.m01 + a.m11 * b.m11,
                m13 = a.m10 * b.m03 + a.m11 * b.m13 + a.m13,
                m22 = a.m22 * b.m22,
                m33 = 1f,
            };

        public Vector3 MultiplyPoint3x4(Vector3 point) =>
            new Vector3(
                m00 * point.x + m01 * point.y + m03,
                m10 * point.x + m11 * point.y + m13,
                m22 * point.z
            );

        public float determinant => (m00 * m11 - m01 * m10) * m22;

        public Matrix4x4 inverse
        {
            get
            {
                var det = m00 * m11 - m01 * m10;
                if (det == 0f)
                    return identity;

                var result = identity;
                result.m00 = m11 / det;
                result.m01 = -m01 / det;
                result.m10 = -m10 / det;
                result.m11 = m00 / det;
                result.m03 = -(result.m00 * m03 + result.m01 * m13);
                result.m13 = -(result.m10 * m03 + result.m11 * m13);
                result.m22 = m22 == 0f ? 0f : 1f / m22;
                return result;
            }
        }

        public bool Equals(Matrix4x4 other) => Equals((object)other);

        public override bool Equals(object obj)
        {
            if (!(obj is Matrix4x4 value))
                return false;
            return m00 == value.m00
                && m01 == value.m01
                && m03 == value.m03
                && m10 == value.m10
                && m11 == value.m11
                && m13 == value.m13
                && m22 == value.m22
                && m33 == value.m33;
        }

        public override int GetHashCode() =>
            m00.GetHashCode()
            ^ m01.GetHashCode()
            ^ m03.GetHashCode()
            ^ m10.GetHashCode()
            ^ m11.GetHashCode()
            ^ m13.GetHashCode();
    }

    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 zero => new Vector2(0f, 0f);
        public static Vector2 one => new Vector2(1f, 1f);

        public float magnitude => (float)Math.Sqrt(x * x + y * y);

        public static float Distance(Vector2 a, Vector2 b) => (a - b).magnitude;

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            t = t < 0f ? 0f : (t > 1f ? 1f : t);
            return a + (b - a) * t;
        }

        public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t) => a + (b - a) * t;

        public Vector2 normalized
        {
            get
            {
                var m = magnitude;
                return m > 0f ? new Vector2(x / m, y / m) : zero;
            }
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);

        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);

        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y);

        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.x * d, a.y * d);

        public static Vector2 operator *(float d, Vector2 a) => new Vector2(a.x * d, a.y * d);

        public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.x / d, a.y / d);

        public static bool operator ==(Vector2 a, Vector2 b) => a.x == b.x && a.y == b.y;

        public static bool operator !=(Vector2 a, Vector2 b) => a.x != b.x || a.y != b.y;

        public override bool Equals(object other) => other is Vector2 v && this == v;

        public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2);

        public override string ToString() => $"({x:F1}, {y:F1})";
    }

    public struct Rect
    {
        private float _x;
        private float _y;
        private float _width;
        private float _height;

        public float x
        {
            get => _x;
            set => _x = value;
        }
        public float y
        {
            get => _y;
            set => _y = value;
        }
        public float width
        {
            get => _width;
            set => _width = value;
        }
        public float height
        {
            get => _height;
            set => _height = value;
        }

        public Rect(float x, float y, float width, float height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public Rect(Vector2 position, Vector2 size)
        {
            _x = position.x;
            _y = position.y;
            _width = size.x;
            _height = size.y;
        }

        public static Rect zero => new Rect(0f, 0f, 0f, 0f);
        public static Rect one => new Rect(0f, 0f, 1f, 1f);

        public static Rect MinMaxRect(float xmin, float ymin, float xmax, float ymax) =>
            new Rect(xmin, ymin, xmax - xmin, ymax - ymin);

        public Vector2 center => new Vector2(x + width / 2f, y + height / 2f);
        public Vector2 min => new Vector2(xMin, yMin);
        public Vector2 max => new Vector2(xMax, yMax);
        public Vector2 position => new Vector2(x, y);
        public Vector2 size => new Vector2(width, height);

        public float xMin => x;
        public float yMin => y;
        public float xMax => x + width;
        public float yMax => y + height;

        public bool Contains(Vector2 point) =>
            point.x >= x && point.x < x + width && point.y >= y && point.y < y + height;

        public bool Contains(Rect other) =>
            other.xMin >= xMin && other.xMax <= xMax && other.yMin >= yMin && other.yMax <= yMax;

        public bool Overlaps(Rect other) =>
            other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;

        public static bool operator ==(Rect a, Rect b) =>
            a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;

        public static bool operator !=(Rect a, Rect b) => !(a == b);

        public override bool Equals(object other) => other is Rect r && this == r;

        public override int GetHashCode() =>
            x.GetHashCode()
            ^ (y.GetHashCode() << 2)
            ^ (width.GetHashCode() >> 2)
            ^ (height.GetHashCode() >> 1);

        public override string ToString() =>
            $"(x:{x:F2}, y:{y:F2}, width:{width:F2}, height:{height:F2})";
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(float r, float g, float b)
            : this(r, g, b, 1f) { }

        public static Color Lerp(Color a, Color b, float t)
        {
            t = t < 0f ? 0f : (t > 1f ? 1f : t);
            return new Color(
                a.r + (b.r - a.r) * t,
                a.g + (b.g - a.g) * t,
                a.b + (b.b - a.b) * t,
                a.a + (b.a - a.a) * t
            );
        }

        public static Color LerpUnclamped(Color a, Color b, float t) =>
            new Color(
                a.r + (b.r - a.r) * t,
                a.g + (b.g - a.g) * t,
                a.b + (b.b - a.b) * t,
                a.a + (b.a - a.a) * t
            );

        public static Color white => new Color(1f, 1f, 1f, 1f);
        public static Color black => new Color(0f, 0f, 0f, 1f);
        public static Color clear => new Color(0f, 0f, 0f, 0f);
        public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);
        public static Color red => new Color(1f, 0f, 0f, 1f);
        public static Color green => new Color(0f, 1f, 0f, 1f);
        public static Color blue => new Color(0f, 0f, 1f, 1f);

        public static Color operator *(Color c, float d) =>
            new Color(c.r * d, c.g * d, c.b * d, c.a * d);

        public static Color operator *(Color a, Color b) =>
            new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);

        public static Color operator /(Color c, float d) =>
            new Color(c.r / d, c.g / d, c.b / d, c.a / d);

        public static Color operator -(Color a, Color b) =>
            new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);

        public static Color operator +(Color a, Color b) =>
            new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);

        public static bool operator ==(Color a, Color b) =>
            a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;

        public static bool operator !=(Color a, Color b) => !(a == b);

        public override bool Equals(object other) => other is Color c && this == c;

        public override int GetHashCode() =>
            r.GetHashCode() ^ (g.GetHashCode() << 2) ^ (b.GetHashCode() >> 2) ^ a.GetHashCode();

        public override string ToString() => $"RGBA({r:F3}, {g:F3}, {b:F3}, {a:F3})";

        public static implicit operator Vector4(Color c) => new Vector4(c.r, c.g, c.b, c.a);
    }

    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Vector4 zero => new Vector4(0f, 0f, 0f, 0f);
        public static Vector4 one => new Vector4(1f, 1f, 1f, 1f);

        public float sqrMagnitude => x * x + y * y + z * z + w * w;
        public float magnitude => (float)System.Math.Sqrt(sqrMagnitude);

        public static Vector4 operator +(Vector4 a, Vector4 b) =>
            new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

        public static Vector4 operator -(Vector4 a, Vector4 b) =>
            new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);

        public static Vector4 operator *(Vector4 a, float d) =>
            new Vector4(a.x * d, a.y * d, a.z * d, a.w * d);

        public static bool operator ==(Vector4 a, Vector4 b) =>
            a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;

        public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);

        public override bool Equals(object other) => other is Vector4 v && this == v;

        public override int GetHashCode() =>
            x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2) ^ w.GetHashCode();

        public override string ToString() => $"({x:F1}, {y:F1}, {z:F1}, {w:F1})";

        public static implicit operator Color(Vector4 v) => new Color(v.x, v.y, v.z, v.w);
    }

    public static class Debug
    {
        public static void Log(object message) { }

        public static void LogWarning(object message) { }

        public static void LogError(object message) { }
    }

    public static class Time
    {
        public static int frameCount => 0;
        public static float deltaTime => 0.016f;
        public static float unscaledDeltaTime => 0.016f;
        public static float realtimeSinceStartup => 0f;
    }

    public static class Mathf
    {
        public static float Clamp(float v, float min, float max) =>
            v < min ? min : (v > max ? max : v);

        public static int Min(int a, int b) => Math.Min(a, b);

        public static int Max(int a, int b) => Math.Max(a, b);

        public static float Max(float a, float b) => a > b ? a : b;

        public static float Min(float a, float b) => a < b ? a : b;

        public static float Abs(float v) => v < 0f ? -v : v;

        public static int Abs(int v) => v < 0 ? -v : v;

        public static int RoundToInt(float v) => (int)Math.Round(v);

        public static int FloorToInt(float v) => (int)Math.Floor(v);

        public static int CeilToInt(float v) => (int)Math.Ceiling(v);

        public static float Floor(float v) => (float)Math.Floor(v);

        public static float Ceil(float v) => (float)Math.Ceiling(v);

        public static float Sign(float v) => v < 0f ? -1f : (v > 0f ? 1f : 0f);

        public static float MoveTowards(float current, float target, float maxDelta) =>
            Math.Abs(target - current) <= maxDelta
                ? target
                : current + Math.Sign(target - current) * maxDelta;

        public static float Pow(float f, float p) => (float)Math.Pow(f, p);

        public static float Sin(float f) => (float)Math.Sin(f);

        public static float Cos(float f) => (float)Math.Cos(f);

        public static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        public static float InverseLerp(float a, float b, float value) =>
            a == b ? 0f : Clamp01((value - a) / (b - a));

        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

        public static float LerpUnclamped(float a, float b, float t) => a + (b - a) * t;

        public static float Sqrt(float v) => (float)System.Math.Sqrt(v);

        public static bool Approximately(float a, float b) => System.Math.Abs(a - b) < 1e-6f;
    }
}
