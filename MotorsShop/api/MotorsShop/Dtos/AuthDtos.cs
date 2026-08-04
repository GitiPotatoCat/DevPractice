using System.ComponentModel.DataAnnotations;

namespace MotorsShop.Dtos;

public record RegisterDto(
    [Required, EmailAddress] string Email,
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, StringLength(100, MinimumLength = 8)] string Password);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponseDto(string Token, string Email, string FullName, IList<string> Roles);

public record ForgotPasswordDto(
    [Required, EmailAddress] string Email);

public record ResetPasswordDto(
    [Required, EmailAddress] string Email,
    [Required] string Token,
    [Required, StringLength(100, MinimumLength = 8)] string NewPassword);

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required, StringLength(100, MinimumLength = 8)] string NewPassword);