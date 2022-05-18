using System;
using System.IO;
using System.Runtime.CompilerServices;
using IPALogger = IPA.Logging.Logger;

namespace EnhancedStreamChat
{
    internal static class Logger
    {
        internal static IPALogger Log { get; set; }
        public static void Debug(string message, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Debug($"{Path.GetFileName(path)}[{member}({num})] : {message}");
        }

        public static void Debug(Exception e, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Debug($"{Path.GetFileName(path)}[{member}({num})] : {e}");
        }

        public static void Error(Exception e, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Error($"{Path.GetFileName(path)}[{member}({num})] : {e}");
        }

        public static void Error(string message, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Error($"{Path.GetFileName(path)}[{member}({num})] : {message}");
        }

        public static void Info(string message, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Info($"{Path.GetFileName(path)}[{member}({num})] : {message}");
        }

        public static void Info(Exception e, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Info($"{Path.GetFileName(path)}[{member}({num})] : {e}");
        }

        public static void Notice(Exception e, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Notice($"{Path.GetFileName(path)}[{member}({num})] : {e}");
        }

        public static void Notice(string message, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Notice($"{Path.GetFileName(path)}[{member}({num})] : {message}");
        }

        public static void Warn(Exception e, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Warn($"{Path.GetFileName(path)}[{member}({num})] : {e}");
        }

        public static void Warn(string message, [CallerFilePath] string path = null, [CallerMemberName] string member = null, [CallerLineNumber] int? num = null)
        {
            Log.Warn($"{Path.GetFileName(path)}[{member}({num})] : {message}");
        }
    }
}
