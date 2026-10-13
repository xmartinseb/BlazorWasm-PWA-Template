using System.ComponentModel.DataAnnotations;

namespace MojePwa.Domain.Dto;

/// <summary>
/// Ve fake databázi nastavuje hodnotu pro daný klíč
/// </summary>
public readonly record struct SetFakeDbKeyRequestDto([Required] string Key, string Value);
