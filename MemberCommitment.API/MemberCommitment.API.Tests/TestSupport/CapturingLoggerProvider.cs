using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MemberCommitment.API.Tests.TestSupport
{
    public sealed record CapturedLog(
        string Category,
        LogLevel Level,
        string Message,
        IReadOnlyDictionary<string, object?> Properties,
        IReadOnlyDictionary<string, object?> ScopeProperties,
        Exception? Exception)
    {
        public string? OriginalFormat =>
            Properties.TryGetValue("{OriginalFormat}", out var format) ? format as string : null;
    }

    /// <summary>
    /// Records every log entry together with its structured state and the scopes active at the time,
    /// so tests can assert on levels, message templates and correlation IDs.
    /// </summary>
    public sealed class CapturingLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentQueue<CapturedLog> _logs = new();
        private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

        public IReadOnlyList<CapturedLog> Logs => _logs.ToArray();

        public void Clear() => _logs.Clear();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, this);

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

        public void Dispose()
        {
        }

        private static IReadOnlyDictionary<string, object?> ToProperties(object? state)
        {
            var properties = new Dictionary<string, object?>();
            if (state is IEnumerable<KeyValuePair<string, object?>> pairs)
            {
                foreach (var pair in pairs)
                {
                    properties[pair.Key] = pair.Value;
                }
            }
            return properties;
        }

        private sealed class CapturingLogger : ILogger
        {
            private readonly string _category;
            private readonly CapturingLoggerProvider _provider;

            public CapturingLogger(string category, CapturingLoggerProvider provider)
            {
                _category = category;
                _provider = provider;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
                _provider._scopeProvider.Push(state);

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var scopeProperties = new Dictionary<string, object?>();
                _provider._scopeProvider.ForEachScope((scope, props) =>
                {
                    foreach (var pair in ToProperties(scope))
                    {
                        props[pair.Key] = pair.Value;
                    }
                }, scopeProperties);

                _provider._logs.Enqueue(new CapturedLog(
                    _category,
                    logLevel,
                    formatter(state, exception),
                    ToProperties(state),
                    scopeProperties,
                    exception));
            }
        }
    }
}
