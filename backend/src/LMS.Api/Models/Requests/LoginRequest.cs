using System.ComponentModel.DataAnnotations;

namespace LMS.Api.Models.Requests;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Password { get; init; } = string.Empty;
}
