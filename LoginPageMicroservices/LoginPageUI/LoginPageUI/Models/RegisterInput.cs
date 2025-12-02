
using System.ComponentModel.DataAnnotations;

namespace LoginPageUI.Models;


//[Required, StringLength(30)]
//public string Username { get; set; }

//[Required, MinLength(6), DataType(DataType.Password)]
//public string Password { get; set; }
public class RegisterInput 
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(30, ErrorMessage = "Username must be at most 30 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password required, Minimum password length 6"), MinLength(6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}