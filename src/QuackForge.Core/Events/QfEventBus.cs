using System;
using System.Collections.Generic;

namespace QuackForge.Core.Events
{
    public sealed class QfEventBus
    {
        private readonly object _lock = new object();
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : notnull
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_lock)
            {
                if (_handlers.TryGetValue(typeof(TEvent), out var existing))
                    _handlers[typeof(TEvent)] = Delegate.Combine(existing, handler);
                else
                    _handlers[typeof(TEvent)] = handler;
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : notnull
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TEvent), out var existing)) return;
                var remaining = Delegate.Remove(existing, handler);
                if (remaining == null) _handlers.Remove(typeof(TEvent));
                else _handlers[typeof(TEvent)] = remaining;
            }
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : notnull
        {
            Delegate? snapshot;
            lock (_lock)
            {
                _handlers.TryGetValue(typeof(TEvent), out snapshot);
            }
            if (snapshot is Action<TEvent> typed) typed(evt);
        }
    }
}
