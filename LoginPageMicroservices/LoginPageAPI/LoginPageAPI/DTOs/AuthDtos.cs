

using System.ComponentModel.DataAnnotations;

namespace LoginPageAPI.DTOs;

public class LoginRequest 
{
    [Required, StringLength(30)]
    public string Username { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
}

public class RegisterRequest 
{
    [Required, StringLength(30)]
    public string Username { get; set; }

    [Required, MinLength(6), DataType(DataType.Password)]

    public string Password { get; set; }
}

public class AuthResponse 
{
    public string Token { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}