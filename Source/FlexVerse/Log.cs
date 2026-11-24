// parents: @.frostline/graph/m-log.xml

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace FlexVerse
{
    /// <summary>Provides the framework-wide structured logging seam. (M-LOG)</summary>
    public static class Log
    {
        /// <summary>Gates emission of all log messages. (REQ-004)</summary>
        public static bool Enabled = true;

        /// <summary>Logs a milestone event at the INFO level. (REQ-001)</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Info(
            string module,
            string @event,
            params (string key, object value)[] vars
        )
        {
            Emit("INFO", module, @event, vars);
        }

        /// <summary>Logs a hot-path event at the DEBUG level.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Debug(
            string module,
            string @event,
            params (string key, object value)[] vars
        )
        {
            Emit("DEBUG", module, @event, vars);
        }

        /// <summary>Logs a recoverable problem at the WARNING level.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Warning(
            string module,
            string @event,
            params (string key, object value)[] vars
        )
        {
            Emit("WARNING", module, @event, vars);
        }

        /// <summary>Logs a failure at the ERROR level.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(
            string module,
            string @event,
            params (string key, object value)[] vars
        )
        {
            Emit("ERROR", module, @event, vars);
        }

        private static void Emit(
            string level,
            string module,
            string @event,
            (string key, object value)[] vars
        )
        {
            if (!Enabled)
            {
                return;
            }

            DefaultSink($"[FV][{module}][{Origin()}][{level}] {@event} {FormatVars(vars)}");
        }

        private static string FormatVars((string key, object value)[] vars)
        {
            if (vars == null || vars.Length == 0)
            {
                return "VARIABLES: {}";
            }

            var sb = new StringBuilder("VARIABLES: {");
            for (var i = 0; i < vars.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(vars[i].key).Append('=').Append(Format(vars[i].value));
            }

            return sb.Append('}').ToString();
        }

        private static string Format(object value)
        {
            if (value == null)
            {
                return "";
            }

            return value is IFormattable f
                ? f.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString();
        }

        private static string Origin()
        {
            var method = new StackFrame(3, false).GetMethod();
            if (method == null)
            {
                return "?:?";
            }

            var type = method.DeclaringType != null ? method.DeclaringType.Name : "?";
            return $"{type}:{method.Name}";
        }

        private static void DefaultSink(string payload)
        {
            if (!HasUnityRuntime())
            {
                return;
            }

            UnityWrite(payload);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UnityWrite(string payload)
        {
            UnityEngine.Debug.Log($"[{UnityEngine.Time.frameCount}]{payload}");
        }

        private static bool HasUnityRuntime()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name == "UnityEngine.CoreModule")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
