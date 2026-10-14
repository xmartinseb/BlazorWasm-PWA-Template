using System.ComponentModel.DataAnnotations;

namespace MojePwa.Domain.Dto;

/// <summary>
/// Ve fake databázi nastavuje hodnotu pro daný klíč.
/// Validační zprávy se vrací do GUI a vidí je uživatel v notifikaci!
/// </summary>
public readonly record struct SetFakeDbKeyRequestDto
(
    [property: Required, StringLength(SetFakeDbKeyRequestDto.MaxInputLength, ErrorMessage = "Maximální délka klíče je {1} znaků")]
    string Key,

    [property: StringLength(SetFakeDbKeyRequestDto.MaxInputLength, ErrorMessage = "Maximální délka hodnoty je {1} znaků")]
    string Value
)
{
    public const int MaxInputLength = 16;
}