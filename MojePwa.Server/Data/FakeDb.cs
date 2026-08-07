using System.Collections.Concurrent;

namespace MojePwa.Server.Data;

/// <summary>
/// Only for demonstration purposes, not used in production. It is a placeholder for a database context or similar data access layer.
/// </summary>
public sealed class FakeDb
{
    public ConcurrentDictionary<string, string> Data { get; } = [];
}
