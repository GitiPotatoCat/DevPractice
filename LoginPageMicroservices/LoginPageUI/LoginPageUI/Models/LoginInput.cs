
using System.ComponentModel.DataAnnotations;

namespace LoginPageUI.Models;

public class LoginInput 
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(30, ErrorMessage = "Username must be at most 30 characters")]
    public string Username { get; set; } = string.Empty;


    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}
