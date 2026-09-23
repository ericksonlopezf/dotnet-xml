// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace EricksonLopez.Xml.Validation.Tests.Common;

/// <summary>
/// A thread-safe, lightweight spy logger capturing log output for assertion in tests.
/// </summary>
public sealed class TestLogger<T> : ILogger<T>
{
    private readonly object _lock = new();
    private readonly List<string> _loggedMessages = [];

    public IReadOnlyList<string> LoggedMessages
    {
        get
        {
            lock (_lock)
            {
                return [.. _loggedMessages];
            }
        }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        lock (_lock)
        {
            _loggedMessages.Add(message);
        }
    }
}
