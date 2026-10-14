using System.ComponentModel.DataAnnotations;

namespace MojePwa.Domain.Dto;

/// <summary>
/// Ve fake databázi nastavuje hodnotu pro daný klíč
/// </summary>
public readonly record struct SetFakeDbKeyRequestDto(
    [property: Required, MaxLength(16)] string Key, 
    [property: MaxLength(16)] string Value);
