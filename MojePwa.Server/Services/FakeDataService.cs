using MojePwa.Domain.Dto;
using MojePwa.Server.Services.Exceptions;
using System.Collections.Concurrent;

namespace MojePwa.Server.Services;

/// <summary>
/// Only for demonstration purposes, not used in production. It is a placeholder for a database context or similar data access layer.
/// </summary>
public sealed class FakeDataService
{
    readonly ConcurrentDictionary<string, string> data = [];

    public Dictionary<string, string> GetAll() => data.ToDictionary();

    internal void Set(SetFakeDbKeyRequestDto r)
    {
        // Na úrovni servis už by něměly být požity argumentové user-friendly výjimky, protože toto už nejsou http parametry,
        // ale parametry normálních C# metod
        ArgumentNullException.ThrowIfNull(r.Key);
        data[r.Key] = r.Value;
    }

    internal bool TryAdd(SetFakeDbKeyRequestDto r)
    {
        // Na úrovni servis už by něměly být požity argumentové user-friendly výjimky, protože toto už nejsou http parametry,
        // ale parametry normálních C# metod
        ArgumentNullException.ThrowIfNull(r.Key);
        return data.TryAdd(r.Key, r.Value);
    }

    // Exception se týká přímo operace konzumenta api, proto je user friendly. Tyto informace by konzument vidět měl
    internal string Get(string key)
        => data.TryGetValue(key, out string? value)
            ? value
            : throw new UserFriendlyNotFoundException($"Requested cache key {key} not found");
}
