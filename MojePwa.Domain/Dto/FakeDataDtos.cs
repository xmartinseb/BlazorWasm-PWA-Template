using System.ComponentModel.DataAnnotations;

namespace MojePwa.Domain.Dto;

/// <summary>
/// Ve fake databázi nastavuje hodnotu pro daný klíč
/// </summary>
public readonly record struct SetFakeDbKeyRequestDto
(
    [property: Required, MaxLength(SetFakeDbKeyRequestDto.MaxInputLength)] 
    string Key, 
    
    [property: MaxLength(SetFakeDbKeyRequestDto.MaxInputLength)] 
    string Value
)
{
    public const int MaxInputLength = 16;
}
