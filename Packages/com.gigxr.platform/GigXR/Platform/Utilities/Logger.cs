namespace GIGXR.Platform.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal struct StackInfo
    {
        public string methodName;
        public string fileName;
        public int lineNumber;

    }

    public class TagLogMessage : EventArgs
    {
        public string tag;
        public string message;

        public TagLogMessage(string tag, string message)
        {
            this.tag = tag;
            this.message = message;
        }
    }
    
    /// <summary>
    /// GigXR`s custom logger. This is a wrapper of Unity`s Debug.Log methods with a environment check to strip these from
    /// release builds as logging takes a lot of hardware resources.
    /// </summary>
    public static class Logger
    {
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        private const bool isDevelopment = true;
#else
        private const bool isDevelopment = false;
#endif

        // Static event, subscribers must make sure to clean up their references to this event
        public static event EventHandler<TagLogMessage> NewTaggedMessage;

        private static Dictionary<string, string> TaggedLoggers = new Dictionary<string, string>();

        public static void AddTaggedLogger(string tag, string displayName)
        {
            if(!TaggedLoggers.ContainsKey(tag))
            {
                TaggedLoggers.Add(tag, displayName);
            }
        }

        public static Dictionary<string, string> GetTaggedLoggers()
        {
            return TaggedLoggers;
        }

        /// <summary>
        /// Logs a debug message. These are always stripped from production builds
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="tag">A optional tag for the message</param>
        public static void Debug(string message, string tag = "")
        {
            string formattedMessage = FormatMessage(message, FormatTag(tag), "DEBUG", GetStackInfo());

            CheckTaggedLogger(tag, formattedMessage);

            if (!isDevelopment)
            {
                return;
            }

            UnityEngine.Debug.Log(formattedMessage);
        }
        
        /// <summary>
        /// Logs a info message. These are stripped from production builds by default but can be opted-in.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="tag">A optional tag for the message</param>
        /// <param name="production">Opt-in for production builds</param>
        public static void Info(string message, string tag = "", bool production = false, UnityEngine.Object context = null)
        {
            string formattedMessage = FormatMessage(message, FormatTag(tag), "INFO", GetStackInfo());

            CheckTaggedLogger(tag, formattedMessage);

            if (!isDevelopment && !production)
            {
                return;
            }

            UnityEngine.Debug.Log(formattedMessage, context);
        }
        
        /// <summary>
        /// Logs a warning message. These are stripped from production builds by default but can be opted-in.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="tag">A optional tag for the message</param>
        /// <param name="production">Opt-in for production builds</param>
        public static void Warning(string message, string tag = "", bool production = false)
        {
            string formattedMessage = FormatMessage(message, FormatTag(tag), "WARNING", GetStackInfo());

            CheckTaggedLogger(tag, formattedMessage);

            if (!isDevelopment && !production)
            {
                return;
            }

            UnityEngine.Debug.LogWarning(formattedMessage);
        }

        /// <summary>
        /// Logs a error message. These are included in production builds by default but can be opted-out.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="tag">A optional tag for the message</param>
        /// <param name="exception">A optional exception</param>
        /// <param name="production">Opt-in for production builds</param>
        public static void Error(string message, string tag = "", Exception exception = null, bool production = true)
        {
            string formattedMessage = FormatMessage(message, FormatTag(tag), "ERROR", GetStackInfo(), exception);

            CheckTaggedLogger(tag, formattedMessage);

            if (!isDevelopment && !production)
            {
                return;
            }

            UnityEngine.Debug.LogError(formattedMessage);

            if (exception != null)
            {
                UnityEngine.Debug.LogException(exception);
            }
        }

        private static void CheckTaggedLogger(string tag, string message)
        {
            if (TaggedLoggers.ContainsKey(tag))
            {
                NewTaggedMessage?.Invoke(tag, new TagLogMessage(tag, message));
            }
        }

        private static string FormatTag(string tag)
        {
            if (!String.IsNullOrEmpty(tag))
            {
                return $"[{tag}]:";
            }

            return null;
        }
        
        private static string FormatMessage(string message, string tag, string level, StackInfo stackInfo, Exception ex = null)
        {
            var msg = $"{stackInfo.fileName}::{stackInfo.methodName}({stackInfo.lineNumber})::{level} -> {tag} {message}";
            if (ex != null)
            {
                msg += $"\nSee exception in next log.\n{ex.Message}\n{ex.StackTrace}";
            }

            return msg;
        }

        private static StackInfo GetStackInfo()
        {
            try
            {
                StackFrame fileFrame = new StackFrame(2, true);
                StackFrame methodFrame = new StackFrame(4);
                var filename = fileFrame.GetFileName()?.Split('\\').Last();
                var methodName = methodFrame.GetMethod().Name;
                int fileLineNumber = fileFrame.GetFileLineNumber();
                return new StackInfo() { fileName = filename, methodName = methodName, lineNumber = fileLineNumber };
            }
            catch (Exception ex)
            {
                return new StackInfo() { fileName = "", methodName = "", lineNumber = 0 };
            }
        }
    }
}