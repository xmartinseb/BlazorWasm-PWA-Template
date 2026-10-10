using MojePwa.Server.Services.Exceptions;
using System.Collections.Concurrent;

namespace MojePwa.Server.Data;

/// <summary>
/// Only for demonstration purposes, not used in production. It is a placeholder for a database context or similar data access layer.
/// </summary>
public sealed class FakeDbService
{
    readonly ConcurrentDictionary<string, string> data = [];

    public Dictionary<string, string> GetAll() => data.ToDictionary();

    internal void Set(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        data[key] = value;
    }

    internal string Get(string key)
        => data.TryGetValue(key, out string? value)
            ? value
            : throw new UserFriendlyNotFoundException($"Requested key {key} not found");
}
