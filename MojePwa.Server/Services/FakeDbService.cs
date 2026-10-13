using MojePwa.Server.Services.Exceptions;
using System.Collections.Concurrent;

namespace MojePwa.Server.Services;

/// <summary>
/// Only for demonstration purposes, not used in production. It is a placeholder for a database context or similar data access layer.
/// </summary>
public sealed class FakeDbService
{
    readonly ConcurrentDictionary<string, string> data = [];

    public Dictionary<string, string> GetAll() => data.ToDictionary();

    internal void Set(string key, string value)
    {
        // Na úrovni servis už by něměly být požity argumentové user-friendly výjimky, protože toto už nejsou http parametry,
        // ale parametry normálních C# metod
        ArgumentNullException.ThrowIfNull(key);
        data[key] = value;
    }

    // Exception se týká přímo operace konzumenta api, proto je user friendly. Tyto informace by konzument vidět měl
    internal string Get(string key)
        => data.TryGetValue(key, out string? value)
            ? value
            : throw new UserFriendlyNotFoundException($"Requested cache key {key} not found");
}
