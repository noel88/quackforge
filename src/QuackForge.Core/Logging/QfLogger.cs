using System;
using BepInEx.Logging;

namespace QuackForge.Core.Logging
{
    public static class QfLogger
    {
        private static ManualLogSource? _root;

        public static void Init(ManualLogSource rootLog)
        {
            _root = rootLog ?? throw new ArgumentNullException(nameof(rootLog));
        }

        public static IQfLog For(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("tag required", nameof(tag));
            if (_root == null) throw new InvalidOperationException("QfLogger.Init must be called from Plugin.Awake before any module logs.");
            return new TaggedLog(_root, tag);
        }

        private sealed class TaggedLog : IQfLog
        {
            private readonly ManualLogSource _inner;
            private readonly string _prefix;

            internal TaggedLog(ManualLogSource inner, string tag)
            {
                _inner = inner;
                _prefix = "[" + tag + "] ";
            }

            public void Debug(string message) => _inner.LogDebug(_prefix + message);
            public void Info(string message)  => _inner.LogInfo(_prefix + message);
            public void Warn(string message)  => _inner.LogWarning(_prefix + message);
            public void Error(string message) => _inner.LogError(_prefix + message);
            public void Error(string message, Exception ex) => _inner.LogError(_prefix + message + " :: " + ex);
        }
    }

    public interface IQfLog
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(string message, Exception ex);
    }
}
