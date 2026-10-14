using System.ComponentModel.DataAnnotations;

namespace MojePwa.Domain.Dto;

/// <summary>
/// Ve fake databázi nastavuje hodnotu pro daný klíč.
/// Validační zprávy se vrací do GUI a vidí je uživatel v notifikaci!
/// </summary>
public readonly struct SetFakeDbKeyRequestDto(string key, string value)
{
    [Required, StringLength(MaxInputLength, ErrorMessage = "Maximální délka klíče je {1} znaků")]
    public string Key { get; } = key;


    [StringLength(MaxInputLength, ErrorMessage = "Maximální délka hodnoty je {1} znaků")]
    public string Value { get; } = value;

    public const int MaxInputLength = 16;
}